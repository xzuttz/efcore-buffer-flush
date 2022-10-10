//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading;
//using AutoFixture;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Internal;
//using Microsoft.VisualStudio.TestPlatform.ObjectModel;
//using SaveChangesMaybe.Models;
//using Xunit;

//namespace SaveChangesMaybe.Tests
//{
//    public class UnitTest1
//    {
//        [Fact]
//        public void Test1()
//        {
//            var builder = new DbContextOptionsBuilder<SchoolContext>();
//            builder.UseInMemoryDatabase(databaseName: "SchoolDbInMemory");

//            var dbContextOptions = builder.Options;
//            SchoolContext ctx = new SchoolContext(dbContextOptions);

//            var fixture = new Fixture();

//            fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
//                .ForEach(b => fixture.Behaviors.Remove(b));
//            fixture.Behaviors.Add(new OmitOnRecursionBehavior());

//            var composer = fixture.Build<Course>();
//            var courses = composer.CreateMany(20).ToList();

//            ctx.Courses.BulkMergeMaybe(courses, 20);

//            var bulkSaveMaybeService = new SaveChangesMaybeSyncService(10000);

//            bulkSaveMaybeService.SyncrhonizeDbSetsPeriodically(ctx.Courses);

//            while (true)
//            {
//                Thread.Sleep(1000);
//                Log.Logger.Debug("Sleeping");
//            }

//            var coursesInMem = ctx.Courses.ToList();
//        }
//    }
//}