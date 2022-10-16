namespace SaveChangesMaybe.DemoConsole.Models;

public class Student
{
    public Guid ID { get; set; }
    public string LastName { get; set; }
    public string FirstMidName { get; set; }
    public DateTime EnrollmentDate { get; set; }

    public ICollection<Enrollment> Enrollments { get; set; }
}