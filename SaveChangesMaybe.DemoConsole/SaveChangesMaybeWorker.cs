using AutoFixture;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SaveChangesMaybe.DemoConsole.Models;
using Serilog;

namespace SaveChangesMaybe.DemoConsole
{
    public class SaveChangesMaybeWorker : BackgroundService
    {
        private readonly ILogger<SaveChangesMaybeWorker> _logger;
        private readonly SchoolContext _schoolCtx;
        private readonly ISaveChangesMaybeServiceFactory _maybeServiceFactory;

        public SaveChangesMaybeWorker(ILogger<SaveChangesMaybeWorker> logger, SchoolContext schoolCtx, ISaveChangesMaybeServiceFactory maybeServiceFactory)
        {
            _logger = logger;
            _schoolCtx = schoolCtx;
            _maybeServiceFactory = maybeServiceFactory;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            StartSaveChangesMaybeService();
            SaveDummyData();
            return Task.CompletedTask;
        }

        private void SaveDummyData()
        {
            var fixture = new Fixture();

            fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => fixture.Behaviors.Remove(b));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            var composer = fixture.Build<Course>();

            int addedTimes = 0;

            while (true)
            {
                if (addedTimes < 10)
                {
                    var rand = new Random();
                    var random = rand.Next(50);

                    var courses = composer.CreateMany(random).ToList();

                    Log.Logger.Debug($"Adding {courses.Count} courses.");

                    _schoolCtx.Courses.BulkMergeMaybe(courses, 50);

                    _schoolCtx.Courses.BulkMergeMaybe(courses, operation =>
                    {
                        operation.IgnoreColumnOutputNames = new List<string>();
                    }, 
                    batchSize: 50);

                    Thread.Sleep(1000);

                    addedTimes++;
                }
                else
                {
                    _logger.LogDebug($"Number of courses saved: {_schoolCtx.Courses.Count()}");
                    Thread.Sleep(1000);
                }
            }
        }

        private void StartSaveChangesMaybeService()
        {
            var saveChangesMaybeService = _maybeServiceFactory.CreateSaveChangesMaybeService();

            var schoolTimer = new SaveChangesMaybeDbSetTimer<Course>(1000)
            {
                BulkOperationType = SaveChangesMaybeBulkOperationType.BulkMerge,
                DbSet = _schoolCtx.Courses,
            };

            var schoolTimer2 = new SaveChangesMaybeDbSetTimer<Course>(1000)
            {
                BulkOperationType = SaveChangesMaybeBulkOperationType.BulkMergeAsync,
                DbSet = _schoolCtx.Courses,
            };

            saveChangesMaybeService.AddTimer(schoolTimer);
            saveChangesMaybeService.AddTimer(schoolTimer2);

            saveChangesMaybeService.Start();
        }
    }
}
