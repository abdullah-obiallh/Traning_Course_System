using System;
using System.Collections.Generic;

namespace TrainingCoursesSystem.Server.Models;

public partial class Course
{
    public int CourseId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string? Category { get; set; }

    public string? LevelName { get; set; }

    public int DurationHours { get; set; }

    public int InstructorId { get; set; }

    public bool IsPublished { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

    public virtual User Instructor { get; set; } = null!;

    public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
}
