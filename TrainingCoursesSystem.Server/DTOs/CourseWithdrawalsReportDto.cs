namespace TrainingCoursesSystem.Server.DTOs
{
    public class CourseWithdrawalsReportDto
    {
        public int CourseId { get; set; }

        public string CourseTitle { get; set; } = string.Empty;

        public int TotalEnrollments { get; set; }

        public int WithdrawnCount { get; set; }

        public decimal WithdrawalPercentage { get; set; }

        public List<CourseWithdrawalDto> Withdrawals { get; set; } = new List<CourseWithdrawalDto>();
    }
}