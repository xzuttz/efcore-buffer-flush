using Microsoft.EntityFrameworkCore;
using Z.BulkOperations;

namespace SaveChangesMaybe.Extensions
{
    public static class BulkUpdateMaybeExtensions
    {
        public delegate void Print();

        public static Task BulkUpdateMaybeAsync<T>(this DbSet<T> dbSet, List<T> entities, int batchSize, Action<BulkOperation<T>>? options = null) where T : class
        {
            dbSet.BulkMerge(null, null);
            dbSet.SaveChangesMaybe(entities, batchSize, SaveChangesMaybeOperationType.BulkUpdateAsync, options);
            return Task.FromResult(Task.CompletedTask);
        }

        public static void BulkUpdateMaybe<T>(this DbSet<T> dbSet, List<T> entities, Action<BulkOperation<T>> options, int batchSize) where T : class
        {
            dbSet.SaveChangesMaybe(entities, batchSize, SaveChangesMaybeOperationType.BulkUpdate, options);
        }
    }
}
