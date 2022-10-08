using Z.BulkOperations;

namespace SaveChangesMaybe
{
    public class BufferWithOptions<T> where T : class
    {
        public List<object> Entities { get; set; } = new();
        public Action<BulkOperation<T>> Options { get; set; }
    }
}
