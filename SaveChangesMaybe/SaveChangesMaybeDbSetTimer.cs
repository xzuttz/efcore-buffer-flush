using System.Timers;
using Microsoft.EntityFrameworkCore;
using Z.BulkOperations;

namespace SaveChangesMaybe
{
    public interface ISaveChangesMaybeDbSetTimer
    {
        void Start();
    }

    public class SaveChangesMaybeDbSetTimer<T> : ISaveChangesMaybeDbSetTimer where T : class
    {
        public DbSet<T> DbSet { get; set; }

        public SaveChangesMaybeBulkOperationType BulkOperationType { get; set; }

        public Action<BulkOperation<T>> BulkOperationOptions { get; set; }

        private Action BulkOperationCallback { get; set; }

        private System.Timers.Timer timer;

        public SaveChangesMaybeDbSetTimer(int timerInterval)
        {
            timer = new System.Timers.Timer(timerInterval);

            timer.Enabled = true;

            timer.Elapsed += TimerOnElapsed;

            switch (BulkOperationType)
            {
                case SaveChangesMaybeBulkOperationType.BulkMerge:
                {
                    if (BulkOperationOptions is null)
                    {
                        BulkOperationCallback = () => DbSet.SaveChangesMaybeFlushBuffer(BulkOperationType);
                        break;
                    }
                    else
                    {
                        //BulkOperationCallback = () => DbSet.SaveChangesMaybeFlushCache(BulkOperationOptions);
                        break;
                    }
                }

                case SaveChangesMaybeBulkOperationType.BulkMergeAsync:
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
            timer.Start();
        }
    }
}
