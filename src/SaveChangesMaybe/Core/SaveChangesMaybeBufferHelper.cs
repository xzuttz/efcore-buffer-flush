using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using SaveChangesMaybe.Models;
using Serilog;

namespace SaveChangesMaybe.Core
{
    public static class SaveChangesMaybeBufferHelper
    {
        internal static Dictionary<string, List<object>> ChangedEntities { get; } = new();

        internal static readonly object PadLock = new();

        internal static void SaveChangesMaybe<T>(SaveChangesMaybeWrapper<T> wrapper) where T : class
        {
            lock (PadLock)
            {
                // Find all entries for the DBSet

                var changedEntities = GetChangedEntities(wrapper.DbSetType);

                if (!changedEntities.Any())
                {
                    ChangedEntities[wrapper.DbSetType] = changedEntities;
                }

                var bufferWithOptions = new SaveChangesBuffer<T>()
                {
                    Options = wrapper.Options,
                    OperationType = wrapper.OperationType,
                    SaveChangesCallback = wrapper.SaveChangesCallback,
                    DbContext = wrapper.DbContext
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

        internal static void FlushDbSetBuffer<T>(SaveChangesMaybeWrapper<T> wrapper) where T : class
        {
            lock (PadLock)
            {
                var changedEntities = GetChangedEntities(wrapper.DbSetType).Cast<SaveChangesBuffer<T>>().ToList();

                if (changedEntities.Any())
                {
                    FlushDbSetBufferAndClearMemory(wrapper, changedEntities.ToList());
                }
            }
        }

        public static void FlushCache()
        {
            lock (PadLock)
            {
                ChangedEntities.Clear();
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
                    var allChangesByOptions = optionsEnumerator.Current.ToList().SelectMany(x => x.Entities).Cast<T>().ToList();

                    // Save changes

                    // If callback on wrapper is null, the call is from the fixed SaveChangesMaybeDbSetTimer. In this case, pick the first CallBack from any of the entities in the list, as they are in the same operation and options group, the same callback applies.

                    var callBack = optionsEnumerator.Current.First().SaveChangesCallback;

                    Debug.WriteLine(optionsEnumerator.Current.First().OperationType);

                    SaveChanges(callBack, allChangesByOptions);
                }
            }

            // Execute all future Actions

            var firstBuffer = all.First();

            if (firstBuffer.DbContext != null)
            {
                firstBuffer.DbContext.ExecuteFutureAction();
            }
            else
            {
                throw new NullReferenceException("Could not find a DbContext to execute future actions on");
            }

            ClearDbSetBufferMemory(wrapper.DbSetType);
        }

        private static void SaveChanges<T>(Action<List<T>> callBack, List<T> entities) where T : class
        {
            if (entities.Any())
            {
                callBack.Invoke(entities);
            }
            else
            {
                Log.Logger.Debug("No entities to save");
            }
        }
    }
}
