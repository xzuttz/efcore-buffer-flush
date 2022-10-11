using Microsoft.EntityFrameworkCore;
using SaveChangesMaybe.Extensions.Common;
using Z.BulkOperations;

namespace SaveChangesMaybe.Extensions
{
    public static class BulkMergeMaybeExtensions
    {

        // DbContext

        public static void BulkMergeMaybe<T>(this DbContext dbContext, List<T> entities, int batchSize, Action<BulkOperation<T>>? options = null) where T : class
        {
            dbContext.SaveChangesMaybe(entities, batchSize, SaveChangesMaybeOperationType.BulkMergeAsync, options);
        }

        public static async Task BulkMergeMaybeAsync<T>(this DbContext dbContext, List<T> entities, int batchSize, CancellationToken cancellationToken, Action<BulkOperation<T>>? options = null) where T : class
        {
            await Task.Run(() =>
            {
                dbContext.SaveChangesMaybe(entities, batchSize, SaveChangesMaybeOperationType.BulkMergeAsync, options);
            }, cancellationToken).ConfigureAwait(false);
        }

        // DbSet

        public static async Task BulkMergeMaybeAsync<T>(this DbSet<T> dbSet, List<T> entities, int batchSize, CancellationToken cancellationToken, Action<BulkOperation<T>>? options = null) where T : class
        {
            await Task.Run(() =>
            {
                dbSet.SaveChangesMaybe(entities, batchSize, SaveChangesMaybeOperationType.BulkMergeAsync, options);
            }, cancellationToken).ConfigureAwait(false);
        }

        public static void BulkMergeMaybe<T>(this DbSet<T> dbSet, List<T> entities, Action<BulkOperation<T>> options, int batchSize) where T : class
        {
            dbSet.SaveChangesMaybe(entities, batchSize, SaveChangesMaybeOperationType.BulkMerge, options);
        }
    }
}
