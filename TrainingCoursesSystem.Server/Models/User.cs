using System;
using System.Collections.Generic;

namespace TrainingCoursesSystem.Server.Models;

public partial class User
{
    public int UserId { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string UserRole { get; set; } = null!;

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? PasswordResetCodeHash { get; set; }

    public DateTime? PasswordResetCodeExpiresAt { get; set; }

    public bool PasswordResetCodeUsed { get; set; }

    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();

    public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
