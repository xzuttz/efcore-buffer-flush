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

                var entityTypeName = wrapper.DbSetType;

                var changedEntities = GetChangedEntities(entityTypeName);

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

                    FlushDbSetBufferAndClearMemory(entityTypeName, all);
                }
            }
        }

        /// <summary>
        /// Used by timers
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entityTypeName"></param>
        internal static void FlushDbSetBuffer<T>(string entityTypeName) where T : class
        {
            lock (PadLock)
            {
                var changedEntities = GetChangedEntities(entityTypeName).Cast<SaveChangesBuffer<T>>().ToList();

                if (changedEntities.Any())
                {
                    FlushDbSetBufferAndClearMemory(entityTypeName, changedEntities.ToList());
                }
            }
        }

        /// <summary>
        /// Used to clear the cache after every unit test
        /// </summary>
        public static void FlushCache()
        {
            lock (PadLock)
            {
                ChangedEntities.Clear();
            }
        }

        private static List<object> GetChangedEntities(string entityTypeName)
        {
            return ChangedEntities.ContainsKey(entityTypeName) ? ChangedEntities[entityTypeName] : new List<object>();
        }

        private static void ClearDbSetBufferMemory(string entityTypeName)
        {
            if (ChangedEntities.ContainsKey(entityTypeName))
            {
                ChangedEntities[entityTypeName].Clear();
            }
        }

        private static void FlushDbSetBufferAndClearMemory<T>(string entityTypeName, List<SaveChangesBuffer<T>> all) where T : class
        {
            SaveChanges(all);

            // Execute all future Actions. DbContext instance is the same on all SaveChangesBuffer.

            var firstBuffer = all.First();

            if (firstBuffer.DbContext != null)
            {
                firstBuffer.DbContext.ExecuteFutureAction();
            }
            else
            {
                throw new NullReferenceException("Could not find a DbContext to execute future actions on");
            }

            ClearDbSetBufferMemory(entityTypeName);
        }

        private static void SaveChanges<T>(List<SaveChangesBuffer<T>> buffer) where T : class
        {
            foreach (var saveChangesBuffer in buffer)
            {
                Debug.WriteLine($"Operation: {saveChangesBuffer.OperationType}");

                if (saveChangesBuffer.Entities.Any())
                {
                    var list = saveChangesBuffer.Entities.Cast<T>().ToList();
                    saveChangesBuffer.SaveChangesCallback.Invoke(list);
                }
                else
                {
                    Log.Logger.Debug("No entities to save");
                }
            }
        }
    }
}
