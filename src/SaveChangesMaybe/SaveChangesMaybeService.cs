namespace SaveChangesMaybe
{
    public class SaveChangesMaybeService : ISaveChangesMaybeService
    {
        private List<ISaveChangesMaybeDbSetTimer> DbSetList { get; } = new();

        public void AddTimer(ISaveChangesMaybeDbSetTimer timer)
        {
            DbSetList.Add(timer);
        }

        public void AddTimers(List<ISaveChangesMaybeDbSetTimer> timers)
        {
            DbSetList.AddRange(timers);
        }

        public void Start()
        {
            foreach (var saveChangesMaybeDbSetTimer in DbSetList)
            {
                saveChangesMaybeDbSetTimer.Start();
            }
        }
    }
}
