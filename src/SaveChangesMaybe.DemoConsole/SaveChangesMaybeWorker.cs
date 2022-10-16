using AutoFixture;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SaveChangesMaybe.DemoConsole.Models;
using SaveChangesMaybe.Extensions;
using Z.BulkOperations;

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

            var fixture2 = new Fixture();

            fixture2.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => fixture.Behaviors.Remove(b));
            fixture2.Behaviors.Add(new OmitOnRecursionBehavior());

            var composer2 = fixture2.Build<Student>();

            int addedTimes = 0;

            while (true)
            {
                if (addedTimes < 10)
                {
                    var rand = new Random();
                    var random = rand.Next(50);

                    var courses = composer.CreateMany(random).ToList();

                    //_logger.LogInformation($"Adding {courses.Count} courses.");

                    SaveCourses(courses); // Testing if the same call multiple times will result in 1 "group" iteration of the BulkOperation

                    var students = composer2.CreateMany(random).ToList();

                    _logger.LogInformation($"Adding {students.Count} students.");

                    Action<BulkOperation<Student>> optionsCallback = static (opts) =>
                    {
                        opts.IgnoreColumnOutputNames = new List<string>();
                        opts.AllowDuplicateKeys = true;
                    };

                    var students2 = composer2.CreateMany(random).ToList();

                    // Testing calling BulkMerge directly on context and DbSet

                    _schoolCtx.Students.BulkMergeMaybe(students2, batchSize: 50, optionsCallback);

                    _schoolCtx.BulkMergeMaybe(students, batchSize: 50, optionsCallback);

                    addedTimes++;
                }
                else
                {
                    _logger.LogError($"Number of courses saved: {_schoolCtx.Courses.Count()}");
                    _logger.LogError($"Number of students saved: {_schoolCtx.Students.Count()}");

                    Thread.Sleep(1000);
                }
            }
        }

        private void SaveCourses(List<Course> courses)
        {
            _schoolCtx.Courses.BulkMergeMaybe(courses, batchSize: 5000,
                operation =>
                {
                    operation.AllowDuplicateKeys = true;
                });
        }

        private void StartSaveChangesMaybeService()
        {
            var saveChangesMaybeService = _maybeServiceFactory.CreateSaveChangesMaybeService();

            var schoolTimer = new SaveChangesMaybeDbSetTimer<Course>(1000)
            {
                DbSetToFlush = _schoolCtx.Courses,
            };

            var studentsTimer = new SaveChangesMaybeDbSetTimer<Student>(1000)
            {
                DbSetToFlush = _schoolCtx.Students,
            };

            saveChangesMaybeService.AddTimer(schoolTimer);
            saveChangesMaybeService.AddTimer(studentsTimer);

            saveChangesMaybeService.Start();
        }
    }
}
