namespace TrainingCoursesSystem.Server.DTOs
{
    public class ProgressResultDto
    {
        public int EnrollmentId { get; set; }

        public int CourseId { get; set; }

        public int TotalLessons { get; set; }

        public int CompletedLessons { get; set; }

        public decimal ProgressPercentage { get; set; }

        public string EnrollmentStatus { get; set; } = string.Empty;
    }
}