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

            var cb1 = new Action<List<T>>(list =>
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

            //var callback = new Action(() =>
            //{
            //    if (options is null)
            //    {
            //        dbContext.BulkMerge(entities);
            //    }
            //    else
            //    {
            //        dbContext.BulkMerge(entities, options);
            //    }
            //});

            var wrapper = new SaveChangesMaybeWrapper<T>
            {
                SaveChangesCallback = cb1,
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
