﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using SaveChangesMaybe.Core;
using SaveChangesMaybe.Models;
using Z.BulkOperations;

namespace SaveChangesMaybe.Extensions
{
    public static class BulkInsertMaybeExtensions
    {
        // DbContext

        public static async Task BulkInsertMaybeAsync<T>(this DbContext dbContext, List<T> entities, int batchSize, CancellationToken cancellationToken,
            Action<BulkOperation<T>>? options = null) where T : class
        {
            /// TODO: async all the way down
            await Task.Run(() =>
            {
                BulkInsertMaybe(dbContext, entities, batchSize, options);
            }, cancellationToken).ConfigureAwait(false);
        }

        public static void BulkInsertMaybe<T>(this DbContext dbContext, List<T> entities, int batchSize, Action<BulkOperation<T>>? options = null) where T : class
        {
            var callback = new Action<List<T>>(list =>
            {
                if (options is null)
                {
                    dbContext.FutureAction(_ => dbContext.BulkInsert(list));
                }
                else
                {
                    dbContext.FutureAction(_ => dbContext.BulkInsert(list, options));
                }
            });

            var wrapper = new SaveChangesMaybeWrapper<T>
            {
                SaveChangesCallback = callback,
                BatchSize = batchSize,
                Entities = entities,
                OperationType = SaveChangesMaybeOperationType.BulkInsert,
                Options = options,
                DbContext = dbContext
            };

            SaveChangesMaybeBufferHelper.SaveChangesMaybe(wrapper);
        }

        // DbSet

        public static async Task BulkInsertMaybeAsync<T>(this DbSet<T> dbSet, List<T> entities, int batchSize, CancellationToken cancellationToken, Action<BulkOperation<T>>? options = null) where T : class
        {
            /// TODO: async all the way down

            await Task.Run(() =>
            {
                BulkInsertMaybe<T>(dbSet, entities, batchSize, options);
            }, cancellationToken).ConfigureAwait(false);
        }

        public static void BulkInsertMaybe<T>(this DbSet<T> dbSet, List<T> entities, int batchSize, Action<BulkOperation<T>>? options = null) where T : class
        {
            var dbContext = dbSet.GetService<ICurrentDbContext>().Context;

            var callback = new Action<List<T>>(list =>
            {
                if (options is null)
                {
                    dbContext.FutureAction(_ => dbContext.BulkInsert(list));
                }
                else
                {
                    dbContext.FutureAction(_ => dbContext.BulkInsert(list, options));
                }
            });

            var wrapper = new SaveChangesMaybeWrapper<T>
            {
                SaveChangesCallback = callback,
                BatchSize = batchSize,
                Entities = entities,
                OperationType = SaveChangesMaybeOperationType.BulkInsert,
                Options = options,
                DbContext = dbContext
            };

            SaveChangesMaybeBufferHelper.SaveChangesMaybe(wrapper);
        }
    }
}
