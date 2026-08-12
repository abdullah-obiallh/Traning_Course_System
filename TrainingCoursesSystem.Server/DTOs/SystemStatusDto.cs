namespace TrainingCoursesSystem.Server.DTOs
{
    public class SystemStatusDto
    {
        public string Status { get; set; } = string.Empty;

        public bool DatabaseConnected { get; set; }

        public int UsersCount { get; set; }

        public int CoursesCount { get; set; }

        public int LessonsCount { get; set; }

        public int EnrollmentsCount { get; set; }

        public int WithdrawalReasonsCount { get; set; }

        public DateTime ServerTime { get; set; }
    }
}