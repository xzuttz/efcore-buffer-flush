using Microsoft.EntityFrameworkCore;
using Serilog;
using Z.BulkOperations;

namespace SaveChangesMaybe.Extensions.Common
{
    internal static class SaveChangesMaybeBufferHelperDbContext
    {
        internal static void SaveChangesMaybe<T>(this DbContext dbContext, List<T> entities, int batchSize, SaveChangesMaybeOperationType operationType, Action<BulkOperation<T>>? options = null) where T : class
        {
            lock (SaveChangesMaybeBufferHelperDbSet.PadLock)
            {
                // Find all entries for the DBSet

                var key = typeof(T).ToString();

                var changedEntities = GetChangedEntities(key);

                if (!changedEntities.Any())
                {
                    SaveChangesMaybeBufferHelperDbSet.ChangedEntities[key] = changedEntities;
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

                if (changeCount > batchSize)
                {
                    Log.Logger.Debug("Batch size exceeded");

                    FlushDbSetBufferAndClearMemory(dbContext, all);
                }
            }
        }

        private static List<object> GetChangedEntities(string key)
        {
            return SaveChangesMaybeBufferHelperDbSet.ChangedEntities.ContainsKey(key) ? SaveChangesMaybeBufferHelperDbSet.ChangedEntities[key] : new List<object>();
        }

        private static void ClearDbSetBufferMemory(string entityTypeName)
        {
            if (SaveChangesMaybeBufferHelperDbSet.ChangedEntities.ContainsKey(entityTypeName))
            {
                SaveChangesMaybeBufferHelperDbSet.ChangedEntities[entityTypeName].Clear();
            }
        }

        private static void FlushDbSetBufferAndClearMemory<T>(DbContext dbContext, List<SaveChangesBuffer<T>> all) where T : class
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

                    SaveChanges(dbContext, allChangesByOptions, operationTypeGroup, optionGroup);
                }
            }

            var type = dbContext.GetType();

            ClearDbSetBufferMemory(typeof(T).ToString());
        }

        private static void SaveChanges<T>(DbContext dbContext, List<T> entities, SaveChangesMaybeOperationType operationType, Action<BulkOperation<T>>? options = null) where T : class
        {
            if (entities.Any())
            {
                Log.Logger.Debug($"Saving {entities.Count} {dbContext.GetType()}");

                switch (operationType)
                {
                    case SaveChangesMaybeOperationType.BulkMerge:
                    case SaveChangesMaybeOperationType.BulkMergeAsync:
                        {
                            if (options is null)
                            {
                                dbContext.BulkMerge(entities);
                            }
                            else
                            {
                                dbContext.BulkMerge(entities, options);
                            }
                            break;
                        }
                    case SaveChangesMaybeOperationType.BulkUpdate:
                    case SaveChangesMaybeOperationType.BulkUpdateAsync:
                        {
                            if (options is null)
                            {
                                dbContext.BulkUpdate(entities);
                            }
                            else
                            {
                                dbContext.BulkUpdate(entities, options);
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
