namespace SaveChangesMaybe.DemoConsole.Models;

public class Enrollment
{
#pragma warning disable CS8618
    public Enrollment()
#pragma warning restore CS8618
    {
        
    }

    public Enrollment(int enrollmentId, int courseId, int studentId, Course course, Student student)
    {
        EnrollmentID = enrollmentId;
        CourseID = courseId;
        StudentID = studentId;
        Course = course;
        Student = student;
    }

    public int EnrollmentID { get; set; }
    public int CourseID { get; set; }
    public int StudentID { get; set; }
    public Course Course { get; set; }
    public Student Student { get; set; }
}