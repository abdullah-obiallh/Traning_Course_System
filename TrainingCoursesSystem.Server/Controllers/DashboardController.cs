using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingCoursesSystem.Server.Data;
using TrainingCoursesSystem.Server.DTOs;
using TrainingCoursesSystem.Server.Models;

namespace TrainingCoursesSystem.Server.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase {
        private readonly TrainingDbContext _context;

        public DashboardController(TrainingDbContext context) {
            _context = context;
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary() {
            var summary = new DashboardSummaryDto
            {
                CoursesCount = await _context.Courses.CountAsync(c => !c.IsDeleted),
                PublishedCoursesCount = await _context.Courses.CountAsync(c => !c.IsDeleted && c.IsPublished),
                StudentsCount = await _context.Users.CountAsync(u => u.UserRole == "Student" && u.IsActive),
                InstructorsCount = await _context.Users.CountAsync(u => u.UserRole == "Instructor" && u.IsActive),
                TotalEnrollmentsCount = await _context.Enrollments.CountAsync(),
                ActiveEnrollmentsCount = await _context.Enrollments.CountAsync(e => e.Status == "Enrolled"),
                CompletedEnrollmentsCount = await _context.Enrollments.CountAsync(e => e.Status == "Completed"),
                WithdrawnEnrollmentsCount = await _context.Enrollments.CountAsync(e => e.Status == "Withdrawn")
            };

            return Ok(summary);
        }

        [HttpGet("courses-statistics")]
        public async Task<IActionResult> GetCoursesStatistics() {
            var result = await _context.Courses
                .AsNoTracking()
                .Where(c => !c.IsDeleted)
                .Select(c => new CourseStatisticsDto
                {
                    CourseId = c.CourseId,
                    CourseTitle = c.Title,
                    InstructorName = c.Instructor.FullName,
                    TotalEnrollments = c.Enrollments.Count(),
                    ActiveStudents = c.Enrollments.Count(e => e.Status == "Enrolled"),
                    CompletedStudents = c.Enrollments.Count(e => e.Status == "Completed"),
                    WithdrawnStudents = c.Enrollments.Count(e => e.Status == "Withdrawn"),

                    CompletionPercentage = c.Enrollments.Any()
                        ? Math.Round(c.Enrollments.Count(e => e.Status == "Completed") * 100m / c.Enrollments.Count(), 2)
                        : 0,
                    WithdrawalPercentage = c.Enrollments.Any()
                        ? Math.Round(c.Enrollments.Count(e => e.Status == "Withdrawn") * 100m / c.Enrollments.Count(), 2)
                        : 0
                })
                .ToListAsync();

            return Ok(result);
        }

        [HttpGet("withdrawal-reasons")]
        public async Task<IActionResult> GetWithdrawalReasonsStatistics() {
            var result = await _context.WithdrawalReasons
                .AsNoTracking()
                .Where(r => r.IsActive)
                .Select(r => new WithdrawalReasonStatisticsDto
                {
                    WithdrawalReasonId = r.WithdrawalReasonId,
                    ReasonText = r.ReasonText,
                    WithdrawalsCount = r.Enrollments.Count(e => e.Status == "Withdrawn")
                })
                .ToListAsync();

            return Ok(result);
        }

        [HttpGet("courses/{courseId}/withdrawals")]
        public async Task<IActionResult> GetCourseWithdrawals(int courseId) {
            var courseInfo = await _context.Courses
                .AsNoTracking()
                .Where(c => c.CourseId == courseId && !c.IsDeleted)
                .Select(c => new
                {
                    c.CourseId,
                    c.Title,
                    TotalEnrollments = c.Enrollments.Count(),
                    WithdrawnCount = c.Enrollments.Count(e => e.Status == "Withdrawn")
                })
                .FirstOrDefaultAsync();

            if (courseInfo == null) {
                return NotFound("Course not found.");
            }

            var withdrawals = await _context.Enrollments
                .AsNoTracking()
                .Where(e => e.CourseId == courseId && e.Status == "Withdrawn")
                .Select(e => new CourseWithdrawalDto
                {
                    EnrollmentId = e.EnrollmentId,
                    StudentId = e.StudentId,
                    StudentName = e.Student.FullName,
                    StudentEmail = e.Student.Email,
                    EnrolledAt = e.EnrolledAt,
                    WithdrawnAt = e.WithdrawnAt,
                    ReasonText = e.WithdrawalReason != null ? e.WithdrawalReason.ReasonText : "No reason",
                    WithdrawalNote = e.WithdrawalNote
                })
                .ToListAsync();

            decimal withdrawalPercentage = courseInfo.TotalEnrollments > 0
                ? Math.Round(courseInfo.WithdrawnCount * 100m / courseInfo.TotalEnrollments, 2)
                : 0;

            var result = new CourseWithdrawalsReportDto
            {
                CourseId = courseInfo.CourseId,
                CourseTitle = courseInfo.Title,
                TotalEnrollments = courseInfo.TotalEnrollments,
                WithdrawnCount = courseInfo.WithdrawnCount,
                WithdrawalPercentage = withdrawalPercentage,
                Withdrawals = withdrawals
            };

            return Ok(result);
        }
    }
}
