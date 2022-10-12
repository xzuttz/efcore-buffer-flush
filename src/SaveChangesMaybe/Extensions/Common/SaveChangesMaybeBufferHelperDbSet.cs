using Microsoft.EntityFrameworkCore;
using Serilog;
using Z.BulkOperations;

namespace SaveChangesMaybe.Extensions.Common
{
    internal static class SaveChangesMaybeBufferHelperDbSet
    {
        internal static Dictionary<string, List<object>> ChangedEntities { get; } = new();

        internal static readonly object PadLock = new();

        internal static void SaveChangesMaybe<T>(this DbSet<T> dbSet, List<T> entities, int batchSize, SaveChangesMaybeOperationType operationType, Action<BulkOperation<T>>? options = null) where T : class
        {
            lock (PadLock)
            {
                // Find all entries for the DBSet

                var changedEntities = GetChangedEntities(dbSet.EntityType.Name);

                if (!changedEntities.Any())
                {
                    ChangedEntities[dbSet.EntityType.Name] = changedEntities;
                }

                var bufferWithOptions = new SaveChangesBuffer<T>()
                {
                    Options = options,
                    OperationType = operationType
                };

                foreach (var entity in entities)
                {
                    bufferWithOptions.Entities.Add(entity);
                }

                changedEntities.Add(bufferWithOptions);

                // Count the number of changes
                var all = changedEntities.Cast<SaveChangesBuffer<T>>().ToList();

                var changeCount = all.Sum(withOptions => withOptions.Entities.Count);

                if (changeCount >= batchSize)
                {
                    Log.Logger.Debug("Batch size exceeded");

                    FlushDbSetBufferAndClearMemory(dbSet, all);
                }
            }
        }

        internal static void FlushDbSetBuffer<T>(this DbSet<T> dbSet) where T : class
        {
            lock (PadLock)
            {
                var changedEntities = GetChangedEntities(dbSet.EntityType.Name).Cast<SaveChangesBuffer<T>>().ToList();

                if (changedEntities.Any())
                {
                    FlushDbSetBufferAndClearMemory(dbSet, changedEntities.ToList());
                }
            }
        }

        private static List<object> GetChangedEntities(string key)
        {
            //var q = SaveChangesMaybeBufferBase<BufferDummy>.ChangedEntities["moo"];
            return ChangedEntities.ContainsKey(key) ? ChangedEntities[key] : new List<object>();
        }

        private static void ClearDbSetBufferMemory(string entityTypeName)
        {
            if (ChangedEntities.ContainsKey(entityTypeName))
            {
                ChangedEntities[entityTypeName].Clear();
            }
        }

        private static void FlushDbSetBufferAndClearMemory<T>(DbSet<T> dbSet, List<SaveChangesBuffer<T>> all) where T : class
        {
            // Group changes first by operation type, then by options, and persist

            var entitiesGroupedByOperationType = all.GroupBy(x => x.OperationType);

            using var operationEnumerator = entitiesGroupedByOperationType.GetEnumerator();

            while (operationEnumerator.MoveNext())
            {
                var allChangesByOperation = operationEnumerator.Current.ToList();

                var entitiesGroupedByOptions = allChangesByOperation.GroupBy(x => x.Options);

                using var optionsEnumerator = entitiesGroupedByOptions.GetEnumerator();

                while (optionsEnumerator.MoveNext())
                {
                    var optionGroup = optionsEnumerator.Current.Key;
                    var allChangesByOptions = optionsEnumerator.Current.ToList().SelectMany(x => x.Entities).Cast<T>().ToList();

                    // Save changes

                    var operationTypeGroup = operationEnumerator.Current.Key;

                    SaveChanges(dbSet, allChangesByOptions, operationTypeGroup, optionGroup);
                }
            }

            ClearDbSetBufferMemory(dbSet.EntityType.Name);
        }

        private static void SaveChanges<T>(DbSet<T> dbSet, List<T> entities, SaveChangesMaybeOperationType operationType, Action<BulkOperation<T>>? options = null) where T : class
        {
            if (entities.Any())
            {
                Log.Logger.Debug($"Saving {entities.Count} {dbSet.GetType()}");

                switch (operationType)
                {
                    case SaveChangesMaybeOperationType.BulkMerge:
                    case SaveChangesMaybeOperationType.BulkMergeAsync:
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
                    case SaveChangesMaybeOperationType.BulkUpdate:
                    case SaveChangesMaybeOperationType.BulkUpdateAsync:
                    {
                        if (options is null)
                        {
                            dbSet.BulkUpdate(entities);
                        }
                        else
                        {
                            dbSet.BulkUpdate(entities, options);
                        }
                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException(nameof(operationType), operationType, null);
                }
            }
            else
            {
                Log.Logger.Debug("No entities to save");
            }
        }
    }
}
