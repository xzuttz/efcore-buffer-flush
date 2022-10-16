using Z.BulkOperations;

namespace SaveChangesMaybe.Models
{
    public class SaveChangesBuffer<T> where T : class
    {
        public Action<List<T>>? SaveChangesCallback { get; set; }
        public List<object> Entities { get; set; } = new();
        public Action<BulkOperation<T>>? Options { get; set; }
        public SaveChangesMaybeOperationType OperationType { get; set; }
    }
}
