﻿using Microsoft.EntityFrameworkCore;
using SaveChangesMaybe.Core;
using System;
using Z.BulkOperations;

namespace SaveChangesMaybe.Models
{
    internal class SaveChangesBuffer<T> : ISaveChangesBuffer where T : class
    {
        public SaveChangesBuffer(Action<List<object>> saveChangesCallback, DbContext dbContext, Action<BulkOperation<T>>? options, SaveChangesMaybeOperationType operationType)
        {
            SaveChangesCallback = saveChangesCallback;
            DbContext = dbContext;
            Options = options;
            OperationType = operationType;
        }

        public Action<List<object>> SaveChangesCallback { get; set; }
        public List<object> Entities { get; set; } = new();
        public DbContext DbContext { get; set; }
        public Action<BulkOperation<T>>? Options { get; set; }
        public SaveChangesMaybeOperationType OperationType { get; set; }

        public void SaveChanges()
        {
            SaveChangesMaybeHelper.SaveChanges(this);
        }
    }

    public interface ISaveChangesBuffer
    {
        void SaveChanges();
        public DbContext DbContext { get; }
    }
}
