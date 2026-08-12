namespace TrainingCoursesSystem.Server.DTOs
{
    public class InstructorDto
    {
        public int InstructorId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;
    }
}