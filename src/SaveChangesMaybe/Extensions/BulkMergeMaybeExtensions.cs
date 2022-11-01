using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using SaveChangesMaybe.Core;
using SaveChangesMaybe.Models;
using Z.BulkOperations;

namespace SaveChangesMaybe.Extensions
{
    public static class BulkMergeMaybeExtensions
    {

        // DbContext

        public static async Task BulkMergeMaybeAsync<T>(this DbContext dbContext, List<T> entities, int? batchSize = null, Action<BulkOperation<T>>? options = null, CancellationToken cancellationToken = default) where T : class
        {
            /// TODO: async all the way down
            await Task.Run(() =>
            {
                BulkMergeMaybe(dbContext, entities, batchSize, options);
            }, cancellationToken).ConfigureAwait(false);
        }

        public static void BulkMergeMaybe<T>(this DbContext dbContext, List<T> entities, int? batchSize = null, Action<BulkOperation<T>>? options = null) where T : class
        {
            var callback = new Action<List<object>>(list =>
            {
                dbContext.FutureAction(x => x.BulkMerge(list, options));
            });

            var wrapper = new SaveChangesMaybeWrapper<T>
            (
                callback,
                options,
                batchSize,
                dbContext,
                SaveChangesMaybeOperationType.BulkMerge,
                entities
            );
            
            SaveChangesMaybeHelper.SaveChangesMaybe(wrapper);
        }

        // DbSet

        public static async Task BulkMergeMaybeAsync<T>(this DbSet<T> dbSet, List<T> entities, int? batchSize, Action<BulkOperation<T>>? options, CancellationToken cancellationToken = default) where T : class
        {
            /// TODO: async all the way down

            await Task.Run(() =>
            {
                BulkMergeMaybe(dbSet, entities, batchSize, options);
            }, cancellationToken).ConfigureAwait(false);
        }

        public static void BulkMergeMaybe<T>(this DbSet<T> dbSet, List<T> entities, int? batchSize = null, Action<BulkOperation<T>>? options = null) where T : class
        {
            var dbContext = dbSet.GetService<ICurrentDbContext>().Context;

            var callback = new Action<List<object>>(list =>
            {
                dbContext.FutureAction(x => x.BulkMerge(list, options));
            });

            var wrapper = new SaveChangesMaybeWrapper<T>
            (
                callback,
                options,
                batchSize,
                dbContext,
                SaveChangesMaybeOperationType.BulkMerge,
                entities
            );

            SaveChangesMaybeHelper.SaveChangesMaybe(wrapper);
        }
    }
}
