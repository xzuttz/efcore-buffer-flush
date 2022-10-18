using Microsoft.EntityFrameworkCore;

namespace SaveChangesMaybe.DemoConsole.Models
{
    public class SchoolContext : DbContext
    {
#pragma warning disable CS8618
        public SchoolContext(DbContextOptions<SchoolContext> options) : base(options)
#pragma warning restore CS8618
        {
        }

#pragma warning disable CS8618
        public SchoolContext()
#pragma warning restore CS8618
        {
            
        }

        public DbSet<Course> Courses { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Student> Students { get; set; }
    }
}
