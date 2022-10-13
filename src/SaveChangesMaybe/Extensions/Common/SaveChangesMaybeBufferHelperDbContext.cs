﻿using Serilog;
using Z.BulkOperations;

namespace SaveChangesMaybe.Extensions.Common
{
    internal static class SaveChangesMaybeBufferHelperDbContext
    {
        internal static void SaveChangesMaybe<T>(SaveChangesMaybeWrapper<T> wrapper) where T : class
        {
            lock (SaveChangesMaybeBufferHelperDbSet.PadLock)
            {
                // Find all entries for the DBSet

                var changedEntities = GetChangedEntities(wrapper.DbSetType);

                if (!changedEntities.Any())
                {
                    SaveChangesMaybeBufferHelperDbSet.ChangedEntities[wrapper.DbSetType] = changedEntities;
                }

                var bufferWithOptions = new SaveChangesBuffer<T>()
                {
                    Options = wrapper.Options,
                    OperationType = wrapper.OperationType
                };

                foreach (var entity in wrapper.Entities)
                {
                    bufferWithOptions.Entities.Add(entity);
                }

                changedEntities.Add(bufferWithOptions);

                // Count the number of changes
                var all = changedEntities.Cast<SaveChangesBuffer<T>>().ToList();

                var changeCount = all.Sum(withOptions => withOptions.Entities.Count);

                if (changeCount >= wrapper.BatchSize)
                {
                    Log.Logger.Debug("Batch size exceeded");

                    FlushDbSetBufferAndClearMemory(wrapper, all);
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

        private static void FlushDbSetBufferAndClearMemory<T>(SaveChangesMaybeWrapper<T> wrapper, List<SaveChangesBuffer<T>> all) where T : class
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

                    SaveChanges(wrapper, allChangesByOptions, operationTypeGroup, optionGroup);
                }
            }

            ClearDbSetBufferMemory(typeof(T).ToString());
        }

        private static void SaveChanges<T>(SaveChangesMaybeWrapper<T> wrapper, List<T> entities, SaveChangesMaybeOperationType operationType, Action<BulkOperation<T>>? options = null) where T : class
        {
            if (entities.Any())
            {
                Log.Logger.Debug($"Saving {entities.Count} {wrapper.DbSetType}");

                wrapper.SaveChangesCallback.Invoke(entities);
            }
            else
            {
                Log.Logger.Debug("No entities to save");
            }
        }
    }
}