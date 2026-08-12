namespace TrainingCoursesSystem.Server.DTOs
{
    public class TeacherCourseStudentDto
    {
        public int EnrollmentId { get; set; }

        public int StudentId { get; set; }

        public string StudentName { get; set; } = string.Empty;

        public string StudentEmail { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public DateTime EnrolledAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        public DateTime? WithdrawnAt { get; set; }

        public string? WithdrawalReasonText { get; set; }

        public int TotalLessons { get; set; }

        public int CompletedLessons { get; set; }

        public decimal ProgressPercentage { get; set; }
    }
}