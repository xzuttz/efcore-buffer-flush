using Microsoft.EntityFrameworkCore;
using SaveChangesMaybe.Extensions.Common;
using Z.BulkOperations;

namespace SaveChangesMaybe.Extensions
{
    public static class BulkUpdateMaybeExtensions
    {
        public static Task BulkUpdateMaybeAsync<T>(this DbSet<T> dbSet, List<T> entities, int batchSize, Action<BulkOperation<T>>? options = null) where T : class
        {
            dbSet.SaveChangesMaybe(entities, batchSize, SaveChangesMaybeOperationType.BulkUpdate, options);
            return Task.FromResult(Task.CompletedTask);
        }

        public static void BulkUpdateMaybe<T>(this DbSet<T> dbSet, List<T> entities, Action<BulkOperation<T>> options, int batchSize) where T : class
        {
            dbSet.SaveChangesMaybe(entities, batchSize, SaveChangesMaybeOperationType.BulkUpdate, options);
        }
    }
}
