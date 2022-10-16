using System.ComponentModel.DataAnnotations.Schema;

namespace SaveChangesMaybe.DemoConsole.Models;

public class Course
{
    public Guid CourseID { get; set; }
    public string Title { get; set; }
    public int Credits { get; set; }

    public ICollection<Enrollment> Enrollments { get; set; }
}