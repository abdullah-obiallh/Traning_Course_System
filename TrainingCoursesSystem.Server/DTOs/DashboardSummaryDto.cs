namespace TrainingCoursesSystem.Server.DTOs
{
    public class DashboardSummaryDto
    {
        public int CoursesCount { get; set; }

        public int PublishedCoursesCount { get; set; }

        public int StudentsCount { get; set; }

        public int InstructorsCount { get; set; }

        public int TotalEnrollmentsCount { get; set; }

        public int ActiveEnrollmentsCount { get; set; }

        public int CompletedEnrollmentsCount { get; set; }

        public int WithdrawnEnrollmentsCount { get; set; }
    }
}