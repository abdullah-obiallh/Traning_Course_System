using System;
using System.Collections.Generic;

namespace TrainingCoursesSystem.Server.Models;

public partial class StudentLessonProgress
{
    public int ProgressId { get; set; }

    public int EnrollmentId { get; set; }

    public int LessonId { get; set; }

    public bool IsCompleted { get; set; }

    public DateTime? CompletedAt { get; set; }

    public virtual Enrollment Enrollment { get; set; } = null!;

    public virtual Lesson Lesson { get; set; } = null!;
}
