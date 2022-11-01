using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using SaveChangesMaybe.Core;
using SaveChangesMaybe.Models;
using Z.BulkOperations;

namespace SaveChangesMaybe.Extensions
{
    public static class BulkInsertMaybeExtensions
    {
        // DbContext

        public static async Task BulkInsertMaybeAsync<T>(this DbContext dbContext, List<T> entities, int? batchSize = null, Action<BulkOperation<T>>? options = null, CancellationToken cancellationToken = default) where T : class
        {
            /// TODO: async all the way down
            await Task.Run(() =>
            {
                BulkInsertMaybe(dbContext, entities, batchSize, options);
            }, cancellationToken).ConfigureAwait(false);
        }

        public static void BulkInsertMaybe<T>(this DbContext dbContext, List<T> entities, int? batchSize = null, Action<BulkOperation<T>>? options = null) where T : class
        {
            var callback = new Action<List<object>>(list =>
            {
                dbContext.FutureAction(x => x.BulkInsert(list, options));
            });

            var wrapper = new SaveChangesMaybeWrapper<T>
            (
                callback,
                options,
                batchSize,
                dbContext,
                SaveChangesMaybeOperationType.BulkInsert,
                entities
            );

            SaveChangesMaybeHelper.SaveChangesMaybe(wrapper);
        }

        // DbSet

        public static async Task BulkInsertMaybeAsync<T>(this DbSet<T> dbSet, List<T> entities, int batchSize, CancellationToken cancellationToken) where T : class
        {
            /// TODO: async all the way down

            await Task.Run(() =>
            {
                BulkInsertMaybe<T>(dbSet, entities, batchSize, null);
            }, cancellationToken).ConfigureAwait(false);
        }


        public static async Task BulkInsertMaybeAsync<T>(this DbSet<T> dbSet, List<T> entities, int? batchSize = null, Action<BulkOperation<T>>? options = null, CancellationToken cancellationToken = default) where T : class
        {
            /// TODO: async all the way down

            await Task.Run(() =>
            {
                BulkInsertMaybe<T>(dbSet, entities, batchSize, options);
            }, cancellationToken).ConfigureAwait(false);
        }

        public static void BulkInsertMaybe<T>(this DbSet<T> dbSet, List<T> entities, int? batchSize = null, Action<BulkOperation<T>>? options = null) where T : class
        {
            var dbContext = dbSet.GetService<ICurrentDbContext>().Context;

            var callback = new Action<List<object>>(list =>
            {
                dbContext.FutureAction(x => x.BulkInsert(list, options));
            });

            var wrapper = new SaveChangesMaybeWrapper<T>
            (
                callback,
                options,
                batchSize,
                dbContext,
                SaveChangesMaybeOperationType.BulkInsert,
                entities
            );

            SaveChangesMaybeHelper.SaveChangesMaybe(wrapper);
        }
    }
}
