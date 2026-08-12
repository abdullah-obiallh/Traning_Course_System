using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TrainingCoursesSystem.Server.Models;

namespace TrainingCoursesSystem.Server.Data;

public partial class TrainingDbContext : DbContext
{
    public TrainingDbContext()
    {
    }

    public TrainingDbContext(DbContextOptions<TrainingDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<Enrollment> Enrollments { get; set; }

    public virtual DbSet<Lesson> Lessons { get; set; }

    public virtual DbSet<StudentLessonProgress> StudentLessonProgresses { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<WithdrawalReason> WithdrawalReasons { get; set; }

    

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("Arabic_CI_AS");

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.CourseId).HasName("PK__Courses__C92D71A76C007517");

            entity.HasIndex(e => e.InstructorId, "IX_Courses_InstructorId");

            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.IsPublished).HasDefaultValue(true);
            entity.Property(e => e.LevelName).HasMaxLength(50);
            entity.Property(e => e.Title).HasMaxLength(150);

            entity.HasOne(d => d.Instructor).WithMany(p => p.Courses)
                .HasForeignKey(d => d.InstructorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Courses_Users_Instructor");
        });

        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.HasKey(e => e.EnrollmentId).HasName("PK__Enrollme__7F68771BEE3898D3");

            entity.HasIndex(e => e.CourseId, "IX_Enrollments_CourseId");

            entity.HasIndex(e => new { e.StudentId, e.CourseId }, "UQ_Enrollments_Student_Course").IsUnique();

            entity.Property(e => e.EnrolledAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Enrolled");
            entity.Property(e => e.WithdrawalNote).HasMaxLength(300);

            entity.HasOne(d => d.Course).WithMany(p => p.Enrollments)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Enrollments_Courses");

            entity.HasOne(d => d.Student).WithMany(p => p.Enrollments)
                .HasForeignKey(d => d.StudentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Enrollments_Users_Student");

            entity.HasOne(d => d.WithdrawalReason).WithMany(p => p.Enrollments)
                .HasForeignKey(d => d.WithdrawalReasonId)
                .HasConstraintName("FK_Enrollments_WithdrawalReasons");
        });

        modelBuilder.Entity<Lesson>(entity =>
        {
            entity.HasKey(e => e.LessonId).HasName("PK__Lessons__B084ACD021E4EA23");

            entity.HasIndex(e => new { e.CourseId, e.LessonId }, "UQ_Lessons_Course_LessonId").IsUnique();

            entity.HasIndex(e => new { e.CourseId, e.LessonOrder }, "UQ_Lessons_Course_LessonOrder").IsUnique();

            entity.Property(e => e.AvailableFrom).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Title).HasMaxLength(150);
            entity.Property(e => e.VideoUrl).HasMaxLength(500);

            entity.HasOne(d => d.Course).WithMany(p => p.Lessons)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Lessons_Courses");
        });

        modelBuilder.Entity<StudentLessonProgress>(entity =>
        {
            entity.HasKey(e => e.ProgressId).HasName("PK__StudentL__BAE29CA5BB04EA54");

            entity.ToTable("StudentLessonProgress");

            entity.HasIndex(e => e.LessonId, "IX_Progress_LessonId");

            entity.HasIndex(e => new { e.EnrollmentId, e.LessonId }, "UQ_Progress_Enrollment_Lesson").IsUnique();

            entity.HasOne(d => d.Enrollment).WithMany(p => p.StudentLessonProgresses)
                .HasForeignKey(d => d.EnrollmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Progress_Enrollments");

            entity.HasOne(d => d.Lesson).WithMany(p => p.StudentLessonProgresses)
                .HasForeignKey(d => d.LessonId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Progress_Lessons");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4C0B382414");

            entity.HasIndex(e => e.Email, "UQ__Users__A9D1053476B0611E").IsUnique();

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.PasswordResetCodeExpiresAt).HasColumnType("datetime");
            entity.Property(e => e.PasswordResetCodeHash).HasMaxLength(255);
            entity.Property(e => e.UserRole).HasMaxLength(20);
        });

        modelBuilder.Entity<WithdrawalReason>(entity =>
        {
            entity.HasKey(e => e.WithdrawalReasonId).HasName("PK__Withdraw__BE8C15C51212E3B1");

            entity.HasIndex(e => e.ReasonText, "UQ__Withdraw__04EF0F4026140F5A").IsUnique();

            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ReasonText).HasMaxLength(100);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
