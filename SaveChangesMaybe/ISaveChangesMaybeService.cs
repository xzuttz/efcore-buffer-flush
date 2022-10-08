namespace SaveChangesMaybe;

public interface ISaveChangesMaybeService
{
    void AddTimer(ISaveChangesMaybeDbSetTimer timer);
    void AddTimers(List<ISaveChangesMaybeDbSetTimer> timers);
    void Start();
}