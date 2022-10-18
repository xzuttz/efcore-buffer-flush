namespace SaveChangesMaybe.DemoConsole.Models;

public class Student
{
#pragma warning disable CS8618
    public Student()
#pragma warning restore CS8618
    {
        
    }

    public Student(Guid id, string lastName, string firstMidName, DateTime enrollmentDate, ICollection<Enrollment> enrollments)
    {
        ID = id;
        LastName = lastName;
        FirstMidName = firstMidName;
        EnrollmentDate = enrollmentDate;
        Enrollments = enrollments;
    }

    public Guid ID { get; set; }
    public string LastName { get; set; }
    public string FirstMidName { get; set; }
    public DateTime EnrollmentDate { get; set; }

    public ICollection<Enrollment> Enrollments { get; set; }
}