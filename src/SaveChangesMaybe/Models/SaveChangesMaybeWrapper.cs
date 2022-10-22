using Microsoft.EntityFrameworkCore;
using Z.BulkOperations;

namespace SaveChangesMaybe.Models
{
    internal class SaveChangesMaybeWrapper<T> where T : class
    {
        public SaveChangesMaybeWrapper(Action<List<object>> saveChangesCallback, Action<BulkOperation<T>>? options, int batchSize, DbContext dbContext, SaveChangesMaybeOperationType operationType, List<T> entities)
        {
            SaveChangesCallback = saveChangesCallback;
            Options = options;
            BatchSize = batchSize;
            DbContext = dbContext;
            OperationType = operationType;
            Entities = entities;
        }

        public Action<List<object>> SaveChangesCallback { get; set; }
        public Action<BulkOperation<T>>? Options { get; set; }
        public int BatchSize { get; set; }
        public string DbSetType => typeof(T).ToString();
        public DbContext DbContext { get; set; }
        public SaveChangesMaybeOperationType OperationType { get; set; }
        public List<T> Entities { get; set; }
    }
}
