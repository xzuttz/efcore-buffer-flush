using System.Timers;
using Microsoft.EntityFrameworkCore;
using SaveChangesMaybe.Extensions;
using Z.BulkOperations;

namespace SaveChangesMaybe
{
    public class SaveChangesMaybeDbSetTimer<T> : ISaveChangesMaybeDbSetTimer where T : class
    {
        public DbSet<T> DbSetToFlush { get; set; }

        public SaveChangesMaybeOperationType OperationType { get; set; }

        public Action<BulkOperation<T>> BulkOperationOptions { get; set; }

        private Action BulkOperationCallback { get; set; }

        private readonly System.Timers.Timer _timer;

        public SaveChangesMaybeDbSetTimer(int timerInterval)
        {
            _timer = new System.Timers.Timer(timerInterval);

            _timer.Enabled = true;

            _timer.Elapsed += TimerOnElapsed;

            switch (OperationType)
            {
                case SaveChangesMaybeOperationType.BulkMerge:
                {
                    if (BulkOperationOptions is null)
                    {
                        

                        BulkOperationCallback = () => DbSetToFlush.FlushDbSetBuffer();
                        break;
                    }
                    else
                    {
                        break;
                    }
                }

                case SaveChangesMaybeOperationType.BulkMergeAsync:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
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
