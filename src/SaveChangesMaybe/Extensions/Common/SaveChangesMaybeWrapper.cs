using Z.BulkOperations;

namespace SaveChangesMaybe.Extensions.Common
{
    internal class SaveChangesMaybeWrapper<T> where T : class
    {
        public Action<List<T>> SaveChangesCallback { get; set; }
        public Action<BulkOperation<T>>? Options { get; set; }
        public int BatchSize { get; set; }
        public string DbSetType { get; set; }
        public SaveChangesMaybeOperationType OperationType { get; set; }
        public List<T> Entities { get; set; }
    }
}
