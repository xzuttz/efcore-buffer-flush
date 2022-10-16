using Microsoft.EntityFrameworkCore;
using SaveChangesMaybe.Extensions.Common;
using SaveChangesMaybe.Models;
using Z.BulkOperations;

namespace SaveChangesMaybe.Extensions
{
    public static class BulkUpdateMaybeExtensions
    {
        public static async Task BulkUpdateMaybeAsync<T>(this DbSet<T> dbSet, List<T> entities, int batchSize, CancellationToken cancellationToken, Action<BulkOperation<T>>? options = null) where T : class
        {
            /// TODO: async all the way down

            await Task.Run(() =>
            {
                BulkUpdateMaybe<T>(dbSet, entities, batchSize, options);
            }, cancellationToken).ConfigureAwait(false);
        }

        public static void BulkUpdateMaybe<T>(this DbSet<T> dbSet, List<T> entities, int batchSize, Action<BulkOperation<T>>? options = null) where T : class
        {

            var callback = new Action<List<T>>(list =>
            {
                if (options is null)
                {
                    dbSet.BulkUpdate(list);
                }
                else
                {
                    dbSet.BulkUpdate(list, options);
                }
            });

            var wrapper = new SaveChangesMaybeWrapper<T>
            {
                SaveChangesCallback = callback,
                BatchSize = batchSize,
                Entities = entities,
                OperationType = SaveChangesMaybeOperationType.BulkMerge,
                Options = options
            };

            SaveChangesMaybeBufferHelperDbSet.SaveChangesMaybe(wrapper);
        }
    }
}
