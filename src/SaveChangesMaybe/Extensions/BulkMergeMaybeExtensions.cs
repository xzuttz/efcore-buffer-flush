using Microsoft.EntityFrameworkCore;
using SaveChangesMaybe.Extensions.Common;
using Z.BulkOperations;

namespace SaveChangesMaybe.Extensions
{
    public static class BulkMergeMaybeExtensions
    {

        // DbContext

        public static async Task BulkMergeMaybeAsync<T>(this DbContext dbContext, List<T> entities, int batchSize, CancellationToken cancellationToken,
            Action<BulkOperation<T>>? options = null) where T : class
        {
            /// TODO: async all the way down
            await Task.Run(() =>
            {
                BulkMergeMaybe<T>(dbContext, entities, batchSize, options);
            }, cancellationToken).ConfigureAwait(false);
        }

        public static void BulkMergeMaybe<T>(this DbContext dbContext, List<T> entities, int batchSize, Action<BulkOperation<T>>? options = null) where T : class
        {
            var callback = new Action<List<T>>(list =>
            {
                if (options is null)
                {
                    dbContext.BulkMerge(list);
                }
                else
                {
                    dbContext.BulkMerge(list, options);
                }
            });

            var wrapper = new SaveChangesMaybeWrapper<T>
            {
                SaveChangesCallback = callback,
                BatchSize = batchSize,
                DbSetType = typeof(T).ToString(),
                Entities = entities,
                OperationType = SaveChangesMaybeOperationType.BulkMerge,
                Options = options
            };

            SaveChangesMaybeBufferHelperDbContext.SaveChangesMaybe(wrapper);
        }

        // DbSet

        public static async Task BulkMergeMaybeAsync<T>(this DbSet<T> dbSet, List<T> entities, int batchSize, CancellationToken cancellationToken, Action<BulkOperation<T>>? options = null) where T : class
        {
            /// TODO: async all the way down

            await Task.Run(() =>
            {
                dbSet.SaveChangesMaybe(entities, batchSize, SaveChangesMaybeOperationType.BulkMerge, options);
            }, cancellationToken).ConfigureAwait(false);
        }

        public static void BulkMergeMaybe<T>(this DbSet<T> dbSet, List<T> entities, int batchSize, Action<BulkOperation<T>>? options = null) where T : class
        {
            dbSet.SaveChangesMaybe(entities, batchSize, SaveChangesMaybeOperationType.BulkMerge, options);
        }
    }
}
