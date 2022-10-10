using System.Timers;
using Microsoft.EntityFrameworkCore;
using SaveChangesMaybe.Extensions;
using Z.BulkOperations;

namespace SaveChangesMaybe
{
    public class SaveChangesMaybeDbSetTimer<T> : ISaveChangesMaybeDbSetTimer where T : class
    {
        public DbSet<T> DbSetToFlush { get; set; }

        public Action<BulkOperation<T>> BulkOperationOptions { get; set; }

        private Action BulkOperationCallback { get; set; }

        private readonly System.Timers.Timer _timer;

        public SaveChangesMaybeDbSetTimer(int timerInterval)
        {
            _timer = new System.Timers.Timer(timerInterval);

            _timer.Elapsed += TimerOnElapsed;

            BulkOperationCallback = () => DbSetToFlush.FlushDbSetBuffer();
        }

        private void TimerOnElapsed(object? sender, ElapsedEventArgs e)
        {
            BulkOperationCallback.Invoke();
        }

        public void Start()
        {
            _timer.Start();
        }
    }
}
