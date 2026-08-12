namespace TrainingCoursesSystem.Server.DTOs
{
    public class StudentCourseDto
    {
        public int EnrollmentId { get; set; }

        public int CourseId { get; set; }

        public string CourseTitle { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public DateTime EnrolledAt { get; set; }

        public int TotalLessons { get; set; }

        public int CompletedLessons { get; set; }

        public decimal ProgressPercentage { get; set; }
    }
}