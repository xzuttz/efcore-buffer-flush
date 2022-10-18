using System;
using System.Collections.Generic;
using System.Linq;
using SaveChangesMaybe.DemoConsole.Models;
using SaveChangesMaybe.Extensions;
using Xunit;
using Z.BulkOperations;

namespace SaveChangesMaybe.Tests
{
    public class OrderTests
    {
        [Fact]
        public void BulkMergeMaybe_BulkUpdateMaybe()
        {
            using (var schoolContext = TestWithSqlite.CreateSchoolContext())
            {
                var course1 = new Course
                {
                    CourseID = Guid.NewGuid(),
                    Credits = 200,
                    Title = "My course 1"
                };

                var courses = new List<Course> { course1 };

                schoolContext.BulkMergeMaybe(courses, 2, Options);

                var course2 = new Course()
                {
                    CourseID = Guid.NewGuid(),
                    Credits = 100,
                    Title = "My course 1"
                };

                courses = new List<Course>() { course2 };

                schoolContext.BulkUpdateMaybe(courses, batchSize: 2, Options);

                var savedCourses = schoolContext.Courses.ToList();

                Assert.Single(savedCourses);
                Assert.Equal(100, savedCourses.First().Credits);
            }
        }

        [Fact]
        public void BulkUpdateMaybe_BulkMergeMaybe()
        {
            using (var schoolContext = TestWithSqlite.CreateSchoolContext())
            {
                var course1 = new Course
                {
                    CourseID = Guid.NewGuid(),
                    Credits = 200,
                    Title = "My course 1"
                };

                var courses = new List<Course> { course1 };

                schoolContext.BulkUpdateMaybe(courses, batchSize: 2, Options);

                var course2 = new Course()
                {
                    CourseID = Guid.NewGuid(),
                    Credits = 100,
                    Title = "My course 1"
                };

                courses = new List<Course>() { course2 };

                schoolContext.BulkMergeMaybe(courses, 2, Options);

                var savedCourses = schoolContext.Courses.ToList();

                Assert.Single(savedCourses);
                Assert.Equal(100, savedCourses.First().Credits);
            }
        }

        [Fact]
        public void BulkInsertMaybe_BulkMergeMaybe()
        {
            using (var schoolContext = TestWithSqlite.CreateSchoolContext())
            {
                var course1 = new Course
                {
                    CourseID = Guid.NewGuid(),
                    Credits = 200,
                    Title = "My course 1"
                };

                var courses = new List<Course> { course1 };

                schoolContext.BulkInsertMaybe(courses, batchSize: 2, Options);

                var course2 = new Course()
                {
                    CourseID = Guid.NewGuid(),
                    Credits = 100,
                    Title = "My course 1"
                };

                courses = new List<Course>() { course2 };

                schoolContext.BulkMergeMaybe(courses, 2, Options);

                var savedCourses = schoolContext.Courses.ToList();

                Assert.Single(savedCourses);
                Assert.Equal(100, savedCourses.First().Credits);
            }
        }

        [Fact]
        public void BulkUpdateMaybe_BulkInsertMaybe()
        {
            using (var schoolContext = TestWithSqlite.CreateSchoolContext())
            {
                var course1 = new Course
                {
                    CourseID = Guid.NewGuid(),
                    Credits = 200,
                    Title = "My course 1"
                };

                var courses = new List<Course> { course1 };

                schoolContext.BulkUpdateMaybe(courses, batchSize: 2, Options);

                var course2 = new Course()
                {
                    CourseID = Guid.NewGuid(),
                    Credits = 100,
                    Title = "My course 1"
                };

                courses = new List<Course>() { course2 };

                schoolContext.BulkInsertMaybe(courses, 2, Options);

                var savedCourses = schoolContext.Courses.ToList();

                Assert.Single(savedCourses);
                Assert.Equal(100, savedCourses.First().Credits);
            }
        }

        [Fact]
        public void BulkInsertMaybe_BulkUpdateMaybe()
        {
            using (var schoolContext = TestWithSqlite.CreateSchoolContext())
            {
                var course1 = new Course
                {
                    CourseID = Guid.NewGuid(),
                    Credits = 200,
                    Title = "My course 1"
                };

                var courses = new List<Course> { course1 };

                schoolContext.BulkInsertMaybe(courses, batchSize: 2, Options);

                var course2 = new Course()
                {
                    CourseID = Guid.NewGuid(),
                    Credits = 100,
                    Title = "My course 1"
                };

                courses = new List<Course>() { course2 };

                schoolContext.BulkUpdateMaybe(courses, 2, Options);

                var savedCourses = schoolContext.Courses.ToList();

                Assert.Single(savedCourses);
                Assert.Equal(100, savedCourses.First().Credits);
            }
        }


        [Fact]
        public void BulkUpdateMaybe_BulkInsertMaybe_BulkUpdateMaybe()
        {
            using (var schoolContext = TestWithSqlite.CreateSchoolContext())
            {
                var course1 = new Course
                {
                    CourseID = Guid.NewGuid(),
                    Credits = 200,
                    Title = "My course 1"
                };

                var courses = new List<Course> { course1 };

                schoolContext.BulkUpdateMaybe(courses, 2, Options);

                var course2 = new Course()
                {
                    CourseID = Guid.NewGuid(),
                    Credits = 100,
                    Title = "My course 1"
                };

                courses = new List<Course>() { course2 };

                schoolContext.BulkInsertMaybe(courses, batchSize: 2, Options);

                courses = new List<Course>() { course1 };

                schoolContext.BulkUpdateMaybe(courses, 1, Options);

                var savedCourses = schoolContext.Courses.ToList();

                Assert.Single(savedCourses);
                Assert.Equal(200, savedCourses.First().Credits);
            }
        }


        private void Options(BulkOperation<Course> obj)
        {
            obj.ColumnPrimaryKeyExpression = customer => customer.Title;
            obj.AllowDuplicateKeys = true;
        }
    }
}
