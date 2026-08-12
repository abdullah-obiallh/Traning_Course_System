namespace TrainingCoursesSystem.Server.DTOs
{
    public class AdminCourseDto
    {
        public int CourseId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? Category { get; set; }

        public string? LevelName { get; set; }

        public int DurationHours { get; set; }

        public int InstructorId { get; set; }

        public string InstructorName { get; set; } = string.Empty;

        public bool IsPublished { get; set; }

        public int LessonsCount { get; set; }

        public int EnrollmentsCount { get; set; }
    }
}