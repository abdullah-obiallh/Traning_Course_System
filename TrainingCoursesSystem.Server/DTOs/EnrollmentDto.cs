namespace TrainingCoursesSystem.Server.DTOs
{
    public class EnrollmentDto
    {
        public int EnrollmentId { get; set; }

        public int StudentId { get; set; }

        public int CourseId { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTime EnrolledAt { get; set; }
    }
}
