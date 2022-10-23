using System.Linq;
using AutoFixture;
using SaveChangesMaybe.Core;
using SaveChangesMaybe.DemoConsole.Models;
using SaveChangesMaybe.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace SaveChangesMaybe.Tests
{
    public class DbContextTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public DbContextTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void DbContext_BulkMerge_CalledOnce_NotExceedingBatchSize_ZeroChanges()
        {
            using (var schoolContext = TestWithSqlite.CreateSchoolContext())
            {
                var fixture = new Fixture();

                fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                    .ForEach(b => fixture.Behaviors.Remove(b));
                fixture.Behaviors.Add(new OmitOnRecursionBehavior());

                var composer = fixture.Build<Course>();

                int addedTimes = 0;

                int addedCourses = 0;

                while (addedTimes < 1)
                {
                    var numberOfCoursesToAdd = 50;

                    var courses = composer.CreateMany(numberOfCoursesToAdd).ToList();

                    schoolContext.BulkMergeMaybe(courses, batchSize: 60, operation =>
                    {
                        operation.AllowDuplicateKeys = true;
                    });

                    addedCourses += numberOfCoursesToAdd;
                    addedTimes++;
                    _testOutputHelper.WriteLine(addedTimes.ToString());
                }

                Assert.Equal(0, schoolContext.Courses.Count());

                SaveChangesMaybeHelper.FlushCache();
            }
        }

        [Fact]
        public void DbContext_BulkMerge_CalledTwice_ExceededBatchSizeTwice_100Changes()
        {
            using (var schoolContext = TestWithSqlite.CreateSchoolContext())
            {
                var fixture = new Fixture();

                fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                    .ForEach(b => fixture.Behaviors.Remove(b));
                fixture.Behaviors.Add(new OmitOnRecursionBehavior());

                var composer = fixture.Build<Course>();

                int addedTimes = 0;

                int addedCourses = 0;

                while (addedTimes < 2)
                {
                    var numberOfCoursesToAdd = 50;

                    var courses = composer.CreateMany(numberOfCoursesToAdd).ToList();

                    schoolContext.BulkMergeMaybe(courses, batchSize: 50, operation =>
                    {
                        operation.AllowDuplicateKeys = true;
                    });

                    addedCourses += numberOfCoursesToAdd;
                    addedTimes++;
                    _testOutputHelper.WriteLine(addedTimes.ToString());
                }

                Assert.Equal(addedCourses, schoolContext.Courses.Count());

                SaveChangesMaybeHelper.FlushCache();
            }
        }

        [Fact]
        public void DbContext_BulkMerge_CalledTwice_ExceededBatchSizeOnce_100Changes()
        {
            using (var schoolContext = TestWithSqlite.CreateSchoolContext())
            {
                var fixture = new Fixture();

                fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                    .ForEach(b => fixture.Behaviors.Remove(b));
                fixture.Behaviors.Add(new OmitOnRecursionBehavior());

                var composer = fixture.Build<Course>();

                int addedTimes = 0;

                int addedCourses = 0;

                while (addedTimes < 2)
                {
                    var numberOfCoursesToAdd = 50;

                    var courses = composer.CreateMany(numberOfCoursesToAdd).ToList();

                    schoolContext.BulkMergeMaybe(courses, batchSize: 60, operation =>
                    {
                        operation.AllowDuplicateKeys = true;
                    });

                    addedCourses += numberOfCoursesToAdd;
                    addedTimes++;
                    _testOutputHelper.WriteLine(addedTimes.ToString());
                }

                Assert.Equal(100, schoolContext.Courses.Count());

                SaveChangesMaybeHelper.FlushCache();
            }
        }
    }
}