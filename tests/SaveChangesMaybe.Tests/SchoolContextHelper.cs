using System;
using System.Linq;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SaveChangesMaybe.DemoConsole.Models;

namespace SaveChangesMaybe.Tests
{
    public static class TestWithSqlite
    {
        public static SchoolContext CreateSchoolContext()
        {
            string InMemoryConnectionString = "DataSource=:memory:";
            SqliteConnection _connection;
            SchoolContext DbContext;
            _connection = new SqliteConnection(InMemoryConnectionString);
            _connection.Open();

            var options = new DbContextOptionsBuilder<SchoolContext>()
                .UseSqlite(_connection)
                .Options;

            DbContext = new SchoolContext(options);

            DbContext.ChangeTracker
                .Entries()
                .ToList()
                .ForEach(e => e.State = EntityState.Detached);

            DbContext.Database.EnsureDeleted();
            DbContext.Database.EnsureCreated();
            
            return DbContext;
        }
    }
}
