namespace TrainingCoursesSystem.Server.DTOs
{
    public class CourseDetailsDto
    {
        public int CourseId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? Category { get; set; }

        public string? LevelName { get; set; }

        public int DurationHours { get; set; }

        public string InstructorName { get; set; } = string.Empty;

        public List<LessonDto> Lessons { get; set; } = new List<LessonDto>();
    }
}