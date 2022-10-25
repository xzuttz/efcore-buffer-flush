using SaveChangesMaybe.DemoConsole.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using SaveChangesMaybe.Core;
using SaveChangesMaybe.Extensions;
using Xunit;

namespace SaveChangesMaybe.Tests
{
    public class FlushTests : TestBase
    {
        [Fact]
        public void FlushDbSet_ChangesSaved()
        {
            using (var schoolContext = TestWithSqlite.CreateSchoolContext())
            {
                var course1 = new Course(
                    Guid.NewGuid(),
                    "My course 1",
                    200,
                    new List<Enrollment>());

                var courses = new List<Course> {course1};

                schoolContext.BulkMergeMaybe(courses, 100);

                Assert.Empty(schoolContext.Courses.ToList());

                SaveChangesMaybeHelper.FlushDbSet<Course>();

                Assert.Single(schoolContext.Courses.ToList());
            }
        }

        [Fact]
        public void FlushDbSet_WrongDbSet_NoChangesSaved()
        {
            using (var schoolContext = TestWithSqlite.CreateSchoolContext())
            {
                var course1 = new Course(
                    Guid.NewGuid(),
                    "My course 1",
                    200,
                    new List<Enrollment>());

                var courses = new List<Course> { course1 };

                schoolContext.BulkMergeMaybe(courses, 100);

                Assert.Empty(schoolContext.Courses.ToList());

                SaveChangesMaybeHelper.FlushDbSet<Student>();

                Assert.Empty(schoolContext.Courses.ToList());
                Assert.Empty(schoolContext.Students.ToList());
            }
        }

        [Fact]
        public void FlushCache_ChangesSaved()
        {
            using (var schoolContext = TestWithSqlite.CreateSchoolContext())
            {
                var course1 = new Course(
                    Guid.NewGuid(),
                    "My course 1",
                    200,
                    new List<Enrollment>());

                var courses = new List<Course> { course1 };

                schoolContext.BulkMergeMaybe(courses, 100);

                var students = new List<Student>()
                {
                    new Student()
                    {
                        EnrollmentDate = DateTime.Now,
                        FirstMidName = "Test",
                        LastName = "Test",
                        ID = Guid.NewGuid()
                    }
                };

                schoolContext.BulkMergeMaybe(students, 100);

                Assert.Empty(schoolContext.Courses.ToList());
                Assert.Empty(schoolContext.Students.ToList());

                SaveChangesMaybeHelper.FlushCache();

                Assert.Single(schoolContext.Courses.ToList());
                Assert.Single(schoolContext.Students.ToList());
            }
        }
    }
}
