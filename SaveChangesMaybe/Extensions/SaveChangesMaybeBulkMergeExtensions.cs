using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Z.BulkOperations;

namespace SaveChangesMaybe.Extensions
{
    public static class SaveChangesMaybeBulkMergeExtensions
    {
        private static readonly object PadLock = new();

        private static ConcurrentDictionary<string, List<object>> ChangedEntities { get; } = new();
        private static ConcurrentDictionary<string, List<object>> ChangedEntitiesWithOptions { get; } = new();

        public static void BulkMergeMaybe<T>(this DbSet<T> dbSet, List<T> entities, int batchSize) where T : class
        {
            BulkMergeMaybeCommon(dbSet, entities, batchSize, SaveChangesMaybeBulkOperationType.BulkMerge);
        }

        public static Task BulkMergeMaybeAsync<T>(this DbSet<T> dbSet, List<T> entities, int batchSize) where T : class
        {
            BulkMergeMaybeCommon(dbSet, entities, batchSize, SaveChangesMaybeBulkOperationType.BulkMergeAsync);
            return Task.FromResult(Task.CompletedTask);
        }

        public static void BulkMergeMaybe<T>(this DbSet<T> dbSet, List<T> entities, Action<BulkOperation<T>> options, int batchSize) where T : class
        {
            lock (PadLock)
            {
                var operationBufferKey = GetDbSetOperationBufferKey(dbSet, SaveChangesMaybeBulkOperationType.BulkMerge, true);

                var localEntities = GetChangedEntitiesWithOptions(operationBufferKey);

                if (!localEntities.Any())
                {
                    ChangedEntitiesWithOptions[operationBufferKey] = localEntities;
                }

                var bufferWithOptions = new BufferWithOptions<T>
                {
                    Options = options
                };

                foreach (var entity in entities)
                {
                    bufferWithOptions.Entities.Add(entity);
                }

                ChangedEntitiesWithOptions[operationBufferKey].Add(bufferWithOptions);

                var all = ChangedEntitiesWithOptions[operationBufferKey].Cast<BufferWithOptions<T>>().ToList();

                var changeCount = all.Sum(withOptions => withOptions.Entities.Count);

                Log.Logger.Debug($"Number of changes: {changeCount}");

                if (changeCount >= batchSize)
                {
                    Log.Logger.Debug("Batch size exceeded");

                    // group changes by option, so all changes with same options will go in one batch

                    var groupByOptions = all.GroupBy(x => x.Options);

                    using var groupEnumerator = groupByOptions.GetEnumerator();

                    int groupEnumeratorIteration = 0;

                    while (groupEnumerator.MoveNext())
                    {
                        var current = groupEnumerator.Current;

                        var allChanges = current.ToList().SelectMany(x => x.Entities).Cast<T>().ToList();

                        SaveEntities(dbSet, allChanges, operationBufferKey, SaveChangesMaybeBulkOperationType.BulkMerge, options);

                        groupEnumeratorIteration++;

                        Log.Logger.Information($"Group iterator iteration: {groupEnumeratorIteration}");
                    }
                }
            }
        }

        public static void SaveChangesMaybeFlushBuffer<T>(this DbSet<T> dbSet, SaveChangesMaybeBulkOperationType operationType) where T : class
        {
            //TODO: How to get options ? 

            lock (PadLock)
            {
                var operationBufferKey = GetDbSetOperationBufferKey(dbSet, SaveChangesMaybeBulkOperationType.BulkMerge, false);

                var entitiesToSave = GetChangedEntities(operationBufferKey);

                var entitiesCasted = entitiesToSave.Cast<T>().ToList();

                SaveEntities(dbSet, entitiesCasted, operationBufferKey, operationType);
            }
        }

        private static void BulkMergeMaybeCommon<T>(this DbSet<T> dbSet, List<T> entities, int batchSize, SaveChangesMaybeBulkOperationType operationType, Action<BulkOperation<T>>? options = null) where T : class
        {
            lock (PadLock)
            {
                var operationBufferKey = GetDbSetOperationBufferKey(dbSet, SaveChangesMaybeBulkOperationType.BulkMerge, false);

                var entitiesInBuffer = GetChangedEntities(operationBufferKey);

                if (!entitiesInBuffer.Any())
                {
                    ChangedEntities[operationBufferKey] = entitiesInBuffer;
                }

                foreach (var entity in entities)
                {
                    ChangedEntities[operationBufferKey].Add(entity);
                }

                var changes = ChangedEntities[operationBufferKey].Count;

                Log.Logger.Debug($"Number of changes: {changes}");

                if (changes >= batchSize)
                {
                    Log.Logger.Debug("Batch size exceeded");

                    var ents = ChangedEntities[operationBufferKey].Cast<T>().ToList();

                    SaveEntities(dbSet, ents, operationBufferKey, operationType, options);
                }
            }
        }

        private static void SaveEntities<T>(DbSet<T> dbSet, List<T> entities, string type, SaveChangesMaybeBulkOperationType operationType, Action<BulkOperation<T>>? options = null) where T : class
        {
            lock (PadLock)
            {
                if (entities.Any())
                {

                    Log.Logger.Debug($"Saving {entities.Count}");

                    switch (operationType)
                    {
                        case SaveChangesMaybeBulkOperationType.BulkMerge:
                        case SaveChangesMaybeBulkOperationType.BulkMergeAsync:
                            {

                                if (options is null)
                                {
                                    dbSet.BulkMerge(entities);
                                }
                                else
                                {
                                    dbSet.BulkMerge(entities, options);
                                }
                                break;
                            }
                        default:
                            throw new ArgumentOutOfRangeException(nameof(operationType), operationType, null);
                    }

                    if (ChangedEntities.ContainsKey(type))
                    {
                        ChangedEntities[type].Clear();
                    }

                    if (ChangedEntitiesWithOptions.ContainsKey(type))
                    {
                        ChangedEntitiesWithOptions[type].Clear();
                    }
                }
                else
                {
                    Log.Logger.Debug("No entities to save");
                }
            }
        }

        private static List<object> GetChangedEntities(string key)
        {
            lock (PadLock)
            {
                return ChangedEntities.ContainsKey(key) ? ChangedEntities[key] : new List<object>();
            }
        }

        private static List<object> GetChangedEntitiesWithOptions(string key)
        {
            lock (PadLock)
            {
                return ChangedEntitiesWithOptions.ContainsKey(key) ? ChangedEntitiesWithOptions[key] : new List<object>();
            }
        }

        private static string GetDbSetOperationBufferKey<T>(DbSet<T> dbSet, SaveChangesMaybeBulkOperationType operationType, bool hasOptions) where T : class
        {
            var entityTypeName = dbSet.EntityType.Name;
            return hasOptions ? $"{entityTypeName + "-" + operationType}-options" : $"{entityTypeName + "-" + operationType}";
        }
    }
}
