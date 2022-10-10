namespace SaveChangesMaybe
{
    public class SaveChangesMaybeServiceFactory : ISaveChangesMaybeServiceFactory
    {
        public ISaveChangesMaybeService CreateSaveChangesMaybeService()
        {
            return new SaveChangesMaybeService();
        }
    }
}
