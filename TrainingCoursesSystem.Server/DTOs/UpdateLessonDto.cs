namespace TrainingCoursesSystem.Server.DTOs
{
    public class UpdateLessonDto
    {
        public int InstructorId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Content { get; set; }

        public string? VideoUrl { get; set; }

        public int LessonOrder { get; set; }

        public DateTime AvailableFrom { get; set; }
    }
}