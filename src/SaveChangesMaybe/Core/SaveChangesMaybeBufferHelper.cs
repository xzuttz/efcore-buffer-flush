using SaveChangesMaybe.Models;
using Serilog;

namespace SaveChangesMaybe.Core
{
    public static class SaveChangesMaybeBufferHelper
    {
        /// <summary>
        /// Save all changes and clear memory
        /// </summary>
        public static void FlushCache()
        {
            lock (PadLock)
            {
                var allChanges = ChangedEntities.Values;

                foreach (var value in allChanges)
                {
                    foreach (var saveChangesBuffer in value.ToArray()) // Because we are modifying the list, we need to create a copy with ToArray
                    {
                        saveChangesBuffer.FlushDbSetBuffer();
                    }
                }

                ChangedEntities.Clear();
            }
        }

        internal static Dictionary<string, List<ISaveChangesBuffer>> ChangedEntities { get; } = new();

        internal static readonly object PadLock = new();

        internal static void SaveChangesMaybe<T>(SaveChangesMaybeWrapper<T> wrapper) where T : class
        {
            lock (PadLock)
            {
                var entityTypeName = wrapper.DbSetType;

                var changedEntities = GetChangedEntities(entityTypeName);

                if (!changedEntities.Any())
                {
                    ChangedEntities[wrapper.DbSetType] = changedEntities;
                }

                var buffer = new SaveChangesBuffer<T>
                (
                    wrapper.SaveChangesCallback, 
                    wrapper.DbContext, 
                    wrapper.Options, 
                    wrapper.OperationType
                );

                foreach (var entity in wrapper.Entities)
                {
                    buffer.Entities.Add(entity);
                }

                changedEntities.Add(buffer);

                var all = changedEntities.Cast<SaveChangesBuffer<T>>().ToList();

                var changeCount = all.Sum(withOptions => withOptions.Entities.Count);

                if (changeCount >= wrapper.BatchSize)
                {
                    Log.Logger.Debug("Batch size exceeded");

                    FlushDbSetBuffer(all);
                    ClearDbSetBufferMemory(entityTypeName);
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
                var all = GetChangedEntities(entityTypeName).Cast<SaveChangesBuffer<T>>().ToList();

                if (!all.Any()) return;

                FlushDbSetBuffer(all);
                ClearDbSetBufferMemory(entityTypeName);
            }
        }

        private static List<ISaveChangesBuffer> GetChangedEntities(string entityTypeName)
        {
            return ChangedEntities.ContainsKey(entityTypeName) ? ChangedEntities[entityTypeName] : new List<ISaveChangesBuffer>();
        }

        private static void ClearDbSetBufferMemory(string entityTypeName)
        {
            if (ChangedEntities.ContainsKey(entityTypeName))
            {
                ChangedEntities[entityTypeName].Clear();
            }
        }

        private static void FlushDbSetBuffer<T>(List<SaveChangesBuffer<T>> all) where T : class
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
        }

        private static void SaveChanges<T>(List<SaveChangesBuffer<T>> buffer) where T : class
        {
            foreach (var saveChangesBuffer in buffer)
            {
                Log.Logger.Debug($"Operation: {saveChangesBuffer.OperationType}");

                if (saveChangesBuffer.Entities.Any())
                {
                    var list = saveChangesBuffer.Entities;
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
