namespace TrainingCoursesSystem.Server.DTOs
{
    public class LessonDto
    {
        public int LessonId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Content { get; set; }

        public string? VideoUrl { get; set; }

        public int LessonOrder { get; set; }
        public DateTime AvailableFrom { get; set; }

        public bool IsLocked { get; set; }
    }
}