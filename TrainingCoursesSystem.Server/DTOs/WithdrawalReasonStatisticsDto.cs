namespace TrainingCoursesSystem.Server.DTOs
{
    public class WithdrawalReasonStatisticsDto
    {
        public int WithdrawalReasonId { get; set; }

        public string ReasonText { get; set; } = string.Empty;

        public int WithdrawalsCount { get; set; }
    }
}