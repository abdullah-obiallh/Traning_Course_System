namespace TrainingCoursesSystem.Server.DTOs
{
    public class StudentLessonDto
    {
        public int LessonId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Content { get; set; }

        public string? VideoUrl { get; set; }

        public int LessonOrder { get; set; }

        public DateTime AvailableFrom { get; set; }

        public bool IsLocked { get; set; }

        public bool IsCompleted { get; set; }

        public DateTime? CompletedAt { get; set; }
    }
}