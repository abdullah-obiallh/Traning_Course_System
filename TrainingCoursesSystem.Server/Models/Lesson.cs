using System;
using System.Collections.Generic;

namespace TrainingCoursesSystem.Server.Models;

public partial class Lesson
{
    public int LessonId { get; set; }

    public int CourseId { get; set; }

    public string Title { get; set; } = null!;

    public string? Content { get; set; }

    public string? VideoUrl { get; set; }

    public int LessonOrder { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime AvailableFrom { get; set; }

    public virtual Course Course { get; set; } = null!;

    public virtual ICollection<StudentLessonProgress> StudentLessonProgresses { get; set; } = new List<StudentLessonProgress>();
}
