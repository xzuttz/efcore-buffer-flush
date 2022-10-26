using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using SaveChangesMaybe.Core;
using SaveChangesMaybe.Models;
using Z.BulkOperations;

namespace SaveChangesMaybe.Extensions
{
    public static class BulkUpdateMaybeExtensions
    {

        // DbContext

        public static async Task BulkUpdateMaybeAsync<T>(this DbContext dbContext, List<T> entities, int batchSize, CancellationToken cancellationToken) where T : class
        {
            /// TODO: async all the way down
            await Task.Run(() =>
            {
                BulkUpdateMaybe(dbContext, entities, batchSize, null);
            }, cancellationToken).ConfigureAwait(false);
        }

        public static async Task BulkUpdateMaybeAsync<T>(this DbContext dbContext, List<T> entities, int batchSize, Action<BulkOperation<T>> options, CancellationToken cancellationToken) where T : class
        {
            /// TODO: async all the way down
            await Task.Run(() =>
            {
                BulkUpdateMaybe(dbContext, entities, batchSize, options);
            }, cancellationToken).ConfigureAwait(false);
        }

        public static void BulkUpdateMaybe<T>(this DbContext dbContext, List<T> entities, int batchSize, Action<BulkOperation<T>>? options = null) where T : class
        {
            var callback = new Action<List<object>>(list =>
            {
                dbContext.FutureAction(x => x.BulkUpdate(list, options));
            });

            var wrapper = new SaveChangesMaybeWrapper<T>
            (
                callback,
                options,
                batchSize,
                dbContext,
                SaveChangesMaybeOperationType.BulkUpdate,
                entities
            );

            SaveChangesMaybeHelper.SaveChangesMaybe(wrapper);
        }

        // DbSet

        public static async Task BulkUpdateMaybeAsync<T>(this DbSet<T> dbSet, List<T> entities, int batchSize, CancellationToken cancellationToken) where T : class
        {
            /// TODO: async all the way down

            await Task.Run(() =>
            {
                BulkUpdateMaybe<T>(dbSet, entities, batchSize, null);
            }, cancellationToken).ConfigureAwait(false);
        }

        public static async Task BulkUpdateMaybeAsync<T>(this DbSet<T> dbSet, List<T> entities, int batchSize, Action<BulkOperation<T>> options, CancellationToken cancellationToken) where T : class
        {
            /// TODO: async all the way down

            await Task.Run(() =>
            {
                BulkUpdateMaybe<T>(dbSet, entities, batchSize, options);
            }, cancellationToken).ConfigureAwait(false);
        }

        public static void BulkUpdateMaybe<T>(this DbSet<T> dbSet, List<T> entities, int batchSize, Action<BulkOperation<T>>? options = null) where T : class
        {
            var dbContext = dbSet.GetService<ICurrentDbContext>().Context;

            var callback = new Action<List<object>>(list =>
            {
                dbContext.FutureAction(x => x.BulkUpdate(list, options));
            });

            var wrapper = new SaveChangesMaybeWrapper<T>
            (
                callback,
                options,
                batchSize,
                dbContext,
                SaveChangesMaybeOperationType.BulkUpdate,
                entities
            );

            SaveChangesMaybeHelper.SaveChangesMaybe(wrapper);
        }
    }
}
