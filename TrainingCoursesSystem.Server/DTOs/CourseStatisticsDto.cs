namespace TrainingCoursesSystem.Server.DTOs
{
    public class CourseStatisticsDto
    {
        public int CourseId { get; set; }

        public string CourseTitle { get; set; } = string.Empty;

        public string InstructorName { get; set; } = string.Empty;

        public int TotalEnrollments { get; set; }

        public int ActiveStudents { get; set; }

        public int CompletedStudents { get; set; }

        public int WithdrawnStudents { get; set; }

        public decimal CompletionPercentage { get; set; }

        public decimal WithdrawalPercentage { get; set; }
    }
}