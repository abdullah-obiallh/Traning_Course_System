namespace TrainingCoursesSystem.Server.DTOs
{
    public class WithdrawEnrollmentDto
    {
        public int EnrollmentId { get; set; }

        public int WithdrawalReasonId { get; set; }

        public string? WithdrawalNote { get; set; }
    }
}