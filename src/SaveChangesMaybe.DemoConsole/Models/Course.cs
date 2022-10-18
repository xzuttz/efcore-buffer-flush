namespace SaveChangesMaybe.DemoConsole.Models;

public class Course
{
#pragma warning disable CS8618
    public Course()
#pragma warning restore CS8618
    {
        
    }

    public Course(Guid courseId, string title, int credits, ICollection<Enrollment> enrollments)
    {
        CourseID = courseId;
        Title = title;
        Credits = credits;
        Enrollments = enrollments;
    }

    public Guid CourseID { get; set; }
    public string Title { get; set; }
    public int Credits { get; set; }

    public ICollection<Enrollment> Enrollments { get; set; }
}