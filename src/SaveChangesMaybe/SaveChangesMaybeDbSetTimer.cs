using System.Timers;
using Microsoft.EntityFrameworkCore;
using SaveChangesMaybe.Extensions.Common;
using SaveChangesMaybe.Models;

namespace SaveChangesMaybe
{
    public class SaveChangesMaybeDbSetTimer<T> : ISaveChangesMaybeDbSetTimer where T : class
    {
        private Action BulkOperationCallback { get; set; }

        private readonly System.Timers.Timer _timer;

        public SaveChangesMaybeDbSetTimer(int timerInterval)
        {
            _timer = new System.Timers.Timer(timerInterval);

            _timer.Elapsed += TimerOnElapsed;

            var wrapper = new SaveChangesMaybeWrapper<T>();

            BulkOperationCallback = () => SaveChangesMaybeBufferHelperDbSet.FlushDbSetBuffer(wrapper);
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
