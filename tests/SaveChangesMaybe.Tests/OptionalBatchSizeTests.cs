using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SaveChangesMaybe.DemoConsole.Models;
using SaveChangesMaybe.Extensions;
using SaveChangesMaybe.Extensions.SaveChangesMaybe.Extensions;
using Xunit;

namespace SaveChangesMaybe.Tests
{
    public class OptionalBatchSizeTests : TestBase
    {
        [Fact]
        public void BulkMergeMaybeDbSet_NoBatchSize_NothingPersisted()
        {
            using (var schoolContext = TestWithSqlite.CreateSchoolContext())
            {
                var list = new List<Course>()
                {
                    new Course(Guid.NewGuid(), "Title", 100, new List<Enrollment>())
                };

                schoolContext.Courses.BulkMergeMaybe(list);

                Assert.Empty(schoolContext.Courses.ToList());
            }
        }

        [Fact]
        public void BulkMergeMaybeDbContext_NoBatchSize_NothingPersisted()
        {
            using (var schoolContext = TestWithSqlite.CreateSchoolContext())
            {
                var list = new List<Course>()
                {
                    new Course(Guid.NewGuid(), "Title", 100, new List<Enrollment>())
                };

                schoolContext.BulkMergeMaybe(list);

                Assert.Empty(schoolContext.Courses.ToList());
            }
        }

        [Fact]
        public void BulkDeleteMaybeDbSet_NoBatchSize_NothingPersisted()
        {
            using (var schoolContext = TestWithSqlite.CreateSchoolContext())
            {
                var list = new List<Course>()
                {
                    new Course(Guid.NewGuid(), "Title", 100, new List<Enrollment>())
                };

                schoolContext.Courses.BulkDeleteMaybe(list);

                Assert.Empty(schoolContext.Courses.ToList());
            }
        }

        [Fact]
        public void BulkDeleteMaybeDbContext_NoBatchSize_NothingPersisted()
        {
            using (var schoolContext = TestWithSqlite.CreateSchoolContext())
            {
                var list = new List<Course>()
                {
                    new Course(Guid.NewGuid(), "Title", 100, new List<Enrollment>())
                };

                schoolContext.BulkDeleteMaybe(list);

                Assert.Empty(schoolContext.Courses.ToList());
            }
        }

        [Fact]
        public void BulkInsertMaybeDbSet_NoBatchSize_NothingPersisted()
        {
            using (var schoolContext = TestWithSqlite.CreateSchoolContext())
            {
                var list = new List<Course>()
                {
                    new Course(Guid.NewGuid(), "Title", 100, new List<Enrollment>())
                };

                schoolContext.Courses.BulkInsertMaybe(list);

                Assert.Empty(schoolContext.Courses.ToList());
            }
        }

        [Fact]
        public void BulkInsertMaybeDbContext_NoBatchSize_NothingPersisted()
        {
            using (var schoolContext = TestWithSqlite.CreateSchoolContext())
            {
                var list = new List<Course>()
                {
                    new Course(Guid.NewGuid(), "Title", 100, new List<Enrollment>())
                };

                schoolContext.BulkInsertMaybe(list);

                Assert.Empty(schoolContext.Courses.ToList());
            }
        }

        [Fact]
        public void BulkSynchronizeMaybeDbSet_NoBatchSize_NothingPersisted()
        {
            using (var schoolContext = TestWithSqlite.CreateSchoolContext())
            {
                var list = new List<Course>()
                {
                    new Course(Guid.NewGuid(), "Title", 100, new List<Enrollment>())
                };

                schoolContext.Courses.BulkSynchronizeMaybe(list);

                Assert.Empty(schoolContext.Courses.ToList());
            }
        }

        [Fact]
        public void BulkSynchronizeMaybeDbContext_NoBatchSize_NothingPersisted()
        {
            using (var schoolContext = TestWithSqlite.CreateSchoolContext())
            {
                var list = new List<Course>()
                {
                    new Course(Guid.NewGuid(), "Title", 100, new List<Enrollment>())
                };

                schoolContext.BulkSynchronizeMaybe(list);

                Assert.Empty(schoolContext.Courses.ToList());
            }
        }

        [Fact]
        public void BulkUpdateMaybeDbSet_NoBatchSize_NothingPersisted()
        {
            using (var schoolContext = TestWithSqlite.CreateSchoolContext())
            {
                var list = new List<Course>()
                {
                    new Course(Guid.NewGuid(), "Title", 100, new List<Enrollment>())
                };

                schoolContext.Courses.BulkUpdateMaybe(list);

                Assert.Empty(schoolContext.Courses.ToList());
            }
        }


        [Fact]
        public void BulkUpdateMaybeDbContext_NoBatchSize_NothingPersisted()
        {
            using (var schoolContext = TestWithSqlite.CreateSchoolContext())
            {
                var list = new List<Course>()
                {
                    new Course(Guid.NewGuid(), "Title", 100, new List<Enrollment>())
                };

                schoolContext.BulkUpdateMaybe(list);

                Assert.Empty(schoolContext.Courses.ToList());
            }
        }
    }
}
