using System.Linq;
using AutoFixture;
using SaveChangesMaybe.Core;
using SaveChangesMaybe.DemoConsole.Models;
using SaveChangesMaybe.Extensions;
using Xunit;
using Xunit.Abstractions;

namespace SaveChangesMaybe.Tests
{
    public partial class DbSetTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public DbSetTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void DbSet_BulkMerge_CalledOnce_NotExceedingBatchSize_ZeroChanges()
        {
            var fixture = new Fixture();

            fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => fixture.Behaviors.Remove(b));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            var composer = fixture.Build<Course>();

            int addedTimes = 0;

            int addedCourses = 0;

            using (var DbContext = TestWithSqlite.CreateSchoolContext())
            {
                while (addedTimes < 1)
                {
                    var numberOfCoursesToAdd = 50;

                    var courses = composer.CreateMany(numberOfCoursesToAdd).ToList();


                    DbContext.Courses.BulkMergeMaybe(courses, batchSize: 60,
                        operation =>
                        {
                            //operation.AllowDuplicateKeys = true;
                        });


                    addedCourses += numberOfCoursesToAdd;
                    addedTimes++;
                    _testOutputHelper.WriteLine(addedTimes.ToString());
                }

                Assert.Equal(0, DbContext.Courses.Count());

                SaveChangesMaybeBufferHelper.FlushCache();
            }
        }

        [Fact]
        public void DbSet_BulkMerge_CalledTwice_ExceededBatchSizeTwice_100Changes()
        {
            var fixture = new Fixture();

            fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => fixture.Behaviors.Remove(b));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            var composer = fixture.Build<Course>();

            int addedTimes = 0;

            int addedCourses = 0;

            using (var DbContext = TestWithSqlite.CreateSchoolContext())
            {
                while (addedTimes < 2)
                {
                    var numberOfCoursesToAdd = 50;

                    var courses = composer.CreateMany(numberOfCoursesToAdd).ToList();

                    DbContext.Courses.BulkMergeMaybe(courses, batchSize: 50,
                        operation =>
                        {
                            operation.AllowDuplicateKeys = false;
                        });

                    addedCourses += numberOfCoursesToAdd;
                    addedTimes++;
                    _testOutputHelper.WriteLine(addedTimes.ToString());
                }

                Assert.Equal(addedCourses, DbContext.Courses.Count());

                SaveChangesMaybeBufferHelper.FlushCache();
            }
        }

        [Fact]
        public void DbSet_BulkMerge_CalledTwice_ExceededBatchSizeOnce_100Changes()
        {
            var fixture = new Fixture();

            fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => fixture.Behaviors.Remove(b));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            var composer = fixture.Build<Course>();

            int addedTimes = 0;

            int addedCourses = 0;

            using (var DbContext = TestWithSqlite.CreateSchoolContext())
            {
                while (addedTimes < 2)
                {
                    var numberOfCoursesToAdd = 50;

                    var courses = composer.CreateMany(numberOfCoursesToAdd).ToList();

                    DbContext.Courses.BulkMergeMaybe(courses, batchSize: 60,
                        operation =>
                        {
                            operation.ColumnPrimaryKeyExpression = course => course.CourseID;
                            operation.AllowDuplicateKeys = true;
                        });

                    addedCourses += numberOfCoursesToAdd;
                    addedTimes++;
                    _testOutputHelper.WriteLine(addedTimes.ToString());
                }

                Assert.Equal(100, DbContext.Courses.Count());

                SaveChangesMaybeBufferHelper.FlushCache();
            }
        }
    }
}