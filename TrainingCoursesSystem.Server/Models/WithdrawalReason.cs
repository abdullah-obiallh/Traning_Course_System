using System;
using System.Collections.Generic;

namespace TrainingCoursesSystem.Server.Models;

public partial class WithdrawalReason
{
    public int WithdrawalReasonId { get; set; }

    public string ReasonText { get; set; } = null!;

    public bool IsActive { get; set; }

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
