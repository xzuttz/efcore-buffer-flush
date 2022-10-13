using System.Linq;
using AutoFixture;
using SaveChangesMaybe.DemoConsole.Models;
using SaveChangesMaybe.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace SaveChangesMaybe.Tests
{
    public class DbSetTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public DbSetTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void BulkMerge_CalledOnce_NotExceedingBatchSize_ZeroChanges()
        {
            using (var schoolContext = new SchoolContext(SchoolContextHelper.CreateNewContextOptions()))
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

                    schoolContext.Courses.BulkMergeMaybe(courses, batchSize: 60,
                        operation =>
                        {
                            operation.AllowDuplicateKeys = true;
                        });

                    addedCourses += numberOfCoursesToAdd;
                    addedTimes++;
                    _testOutputHelper.WriteLine(addedTimes.ToString());
                }

                Assert.Equal(0, schoolContext.Courses.Count());
            }
        }

        [Fact]
        public void BulkMerge_CalledTwice_ExceededBatchSizeTwice_100Changes()
        {
            using (var schoolContext = new SchoolContext(SchoolContextHelper.CreateNewContextOptions()))
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

                    schoolContext.Courses.BulkMergeMaybe(courses, batchSize: 50,
                        operation =>
                        {
                            operation.AllowDuplicateKeys = true;
                        });

                    addedCourses += numberOfCoursesToAdd;
                    addedTimes++;
                    _testOutputHelper.WriteLine(addedTimes.ToString());
                }

                Assert.Equal(addedCourses, schoolContext.Courses.Count());
            }
        }

        [Fact]
        public void BulkMerge_CalledTwice_ExceededBatchSizeOnce_100Changes()
        {
            using (var schoolContext = new SchoolContext(SchoolContextHelper.CreateNewContextOptions()))
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

                    schoolContext.Courses.BulkMergeMaybe(courses, batchSize: 60, 
                        operation =>
                        {
                            operation.AllowDuplicateKeys = true;
                        });

                    addedCourses += numberOfCoursesToAdd;
                    addedTimes++;
                    _testOutputHelper.WriteLine(addedTimes.ToString());
                }

                Assert.Equal(100, schoolContext.Courses.Count());
            }
        }
    }
}