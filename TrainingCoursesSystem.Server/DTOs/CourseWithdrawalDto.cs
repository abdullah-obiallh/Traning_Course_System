namespace TrainingCoursesSystem.Server.DTOs
{
    public class CourseWithdrawalDto
    {
        public int EnrollmentId { get; set; }

        public int StudentId { get; set; }

        public string StudentName { get; set; } = string.Empty;

        public string StudentEmail { get; set; } = string.Empty;

        public DateTime EnrolledAt { get; set; }

        public DateTime? WithdrawnAt { get; set; }

        public string ReasonText { get; set; } = string.Empty;

        public string? WithdrawalNote { get; set; }
    }
}