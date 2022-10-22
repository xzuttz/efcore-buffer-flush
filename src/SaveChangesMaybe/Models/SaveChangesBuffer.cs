using Microsoft.EntityFrameworkCore;
using Z.BulkOperations;

namespace SaveChangesMaybe.Models
{
    public class SaveChangesBuffer<T> where T : class
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
    }
}
