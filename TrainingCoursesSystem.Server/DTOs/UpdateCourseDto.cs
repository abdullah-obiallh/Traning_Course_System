namespace TrainingCoursesSystem.Server.DTOs
{
    public class UpdateCourseDto
    {
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? Category { get; set; }

        public string? LevelName { get; set; }

        public int DurationHours { get; set; }

        public int InstructorId { get; set; }

        public bool IsPublished { get; set; }
    }
}