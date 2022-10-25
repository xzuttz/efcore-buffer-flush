using SaveChangesMaybe.Models;
using Serilog;

namespace SaveChangesMaybe.Core
{
    public static class SaveChangesMaybeHelper
    {
        /// <summary>
        /// Save all changes and clear memory
        /// </summary>
        public static void FlushCache()
        {
            lock (PadLock)
            {
                var allChanges = ChangedEntities.Values;

                foreach (var dbSetBuffer in allChanges)
                {
                    if (dbSetBuffer.Any())
                    {
                        foreach (var saveChangesBuffer in dbSetBuffer)
                        {
                            // This invokes a FutureAction for each buffer

                            saveChangesBuffer.SaveChanges();
                        }

                        // Execute all future actions

                        var firstBuffer = dbSetBuffer.FirstOrDefault();

                        if (firstBuffer != null)
                        {
                            firstBuffer.DbContext.ExecuteFutureAction();
                        }
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

                    FlushDbSet(all);
                    ClearDbSetBufferMemory(entityTypeName);
                }
            }
        }

        /// <summary>
        /// Used by timers
        /// </summary>
        /// <typeparam name="T"></typeparam>
        internal static void FlushDbSet<T>() where T : class
        {
            lock (PadLock)
            {
                var entityTypeName = typeof(T).ToString();

                var all = GetChangedEntities(entityTypeName).Cast<SaveChangesBuffer<T>>().ToList();

                if (!all.Any()) return;

                FlushDbSet(all);
                ClearDbSetBufferMemory(entityTypeName);
            }
        }

        internal static void SaveChanges<T>(SaveChangesBuffer<T> buffer) where T : class
        {
            Log.Logger.Debug($"Operation: {buffer.OperationType}");

            if (buffer.Entities.Any())
            {
                var list = buffer.Entities;
                buffer.SaveChangesCallback.Invoke(list);
            }
            else
            {
                Log.Logger.Debug("No entities to save");
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

        private static void FlushDbSet<T>(List<SaveChangesBuffer<T>> all) where T : class
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

        private static void SaveChanges<T>(List<SaveChangesBuffer<T>> bufferList) where T : class
        {
            foreach (var buffer in bufferList)
            {
                SaveChanges(buffer);
            }
        }

    }
}
