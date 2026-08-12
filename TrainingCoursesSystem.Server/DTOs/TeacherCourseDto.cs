namespace TrainingCoursesSystem.Server.DTOs
{
    public class TeacherCourseDto
    {
        public int CourseId { get; set; }

        public string Title { get; set; } = string.Empty;

        public string? Category { get; set; }

        public string? LevelName { get; set; }

        public int DurationHours { get; set; }

        public int LessonsCount { get; set; }

        public int StudentsCount { get; set; }

        public bool IsPublished { get; set; }
    }
}