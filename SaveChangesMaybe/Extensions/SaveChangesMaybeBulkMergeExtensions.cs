using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Z.BulkOperations;

namespace SaveChangesMaybe.Extensions
{
    public static class SaveChangesMaybeBulkMergeExtensions
    {
        private static readonly object PadLock = new();

        public static Task BulkMergeMaybeAsync<T>(this DbSet<T> dbSet, List<T> entities, int batchSize, Action<BulkOperation<T>>? options = null) where T : class
        {
            lock (PadLock)
            {
                dbSet.SaveChangesMaybe(entities, batchSize, SaveChangesMaybeOperationType.BulkMergeAsync, options);
                return Task.FromResult(Task.CompletedTask);
            }
        }

        public static void BulkMergeMaybe<T>(this DbSet<T> dbSet, List<T> entities, Action<BulkOperation<T>> options, int batchSize) where T : class
        {
            lock (PadLock)
            {
                dbSet.SaveChangesMaybe(entities, batchSize, SaveChangesMaybeOperationType.BulkMerge, options);
            }
        }
    }
}
