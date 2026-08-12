using System;
using System.Collections.Generic;

namespace TrainingCoursesSystem.Server.Models;

public partial class Enrollment
{
    public int EnrollmentId { get; set; }

    public int StudentId { get; set; }

    public int CourseId { get; set; }

    public string Status { get; set; } = null!;

    public DateTime EnrolledAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime? WithdrawnAt { get; set; }

    public int? WithdrawalReasonId { get; set; }

    public string? WithdrawalNote { get; set; }

    public virtual Course Course { get; set; } = null!;

    public virtual User Student { get; set; } = null!;

    public virtual ICollection<StudentLessonProgress> StudentLessonProgresses { get; set; } = new List<StudentLessonProgress>();

    public virtual WithdrawalReason? WithdrawalReason { get; set; }
}
