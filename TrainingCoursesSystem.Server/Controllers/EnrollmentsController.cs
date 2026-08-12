using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingCoursesSystem.Server.Data;
using TrainingCoursesSystem.Server.DTOs;
using TrainingCoursesSystem.Server.Models;

namespace TrainingCoursesSystem.Server.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class EnrollmentsController : ControllerBase {
        private readonly TrainingDbContext _context;

        public EnrollmentsController(TrainingDbContext context) {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> EnrollInCourse(CreateEnrollmentDto request) {
            if (request.StudentId <= 0 || request.CourseId <= 0) {
                return BadRequest("StudentId and CourseId are required.");
            }

            var studentExists = await _context.Users
                .AnyAsync(u => u.UserId == request.StudentId
                            && u.UserRole == "Student"
                            && u.IsActive);

            if (!studentExists) {
                return BadRequest("Student not found.");
            }

            var courseExists = await _context.Courses
                .AnyAsync(c => c.CourseId == request.CourseId
                            && !c.IsDeleted
                            && c.IsPublished);

            if (!courseExists) {
                return BadRequest("Course not found.");
            }

            var existingEnrollment = await _context.Enrollments
                 .FirstOrDefaultAsync(e => e.StudentId == request.StudentId
                                && e.CourseId == request.CourseId);

            if (existingEnrollment != null) {
                if (existingEnrollment.Status == "Enrolled") {
                    return BadRequest("Student is already enrolled in this course.");
                }

                if (existingEnrollment.Status == "Completed") {
                    return BadRequest("Student already completed this course.");
                }

                if (existingEnrollment.Status == "Withdrawn") {
                    var oldProgress = await _context.StudentLessonProgresses
                        .Where(p => p.EnrollmentId == existingEnrollment.EnrollmentId)
                        .ToListAsync();

                    _context.StudentLessonProgresses.RemoveRange(oldProgress);

                    existingEnrollment.Status = "Enrolled";
                    existingEnrollment.EnrolledAt = DateTime.Now;
                    existingEnrollment.CompletedAt = null;
                    existingEnrollment.WithdrawnAt = null;
                    existingEnrollment.WithdrawalReasonId = null;
                    existingEnrollment.WithdrawalNote = null;

                    await _context.SaveChangesAsync();

                    var reactivatedResult = new EnrollmentDto
                    {
                        EnrollmentId = existingEnrollment.EnrollmentId,
                        StudentId = existingEnrollment.StudentId,
                        CourseId = existingEnrollment.CourseId,
                        Status = existingEnrollment.Status,
                        EnrolledAt = existingEnrollment.EnrolledAt
                    };

                    return Ok(reactivatedResult);
                }
            }

            var enrollment = new Enrollment
            {
                StudentId = request.StudentId,
                CourseId = request.CourseId,
                Status = "Enrolled",
                EnrolledAt = DateTime.Now
            };

            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            var result = new EnrollmentDto
            {
                EnrollmentId = enrollment.EnrollmentId,
                StudentId = enrollment.StudentId,
                CourseId = enrollment.CourseId,
                Status = enrollment.Status,
                EnrolledAt = enrollment.EnrolledAt
            };

            return Ok(result);
        }

        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetStudentCourses(int studentId) {
            var studentExists = await _context.Users
                .AnyAsync(u => u.UserId == studentId
                            && u.UserRole == "Student"
                            && u.IsActive);

            if (!studentExists) {
                return NotFound("Student not found.");
            }

            var result = await _context.Enrollments
                .AsNoTracking()
                .Where(e => e.StudentId == studentId)
                .Select(e => new StudentCourseDto
                {
                    EnrollmentId = e.EnrollmentId,
                    CourseId = e.CourseId,
                    CourseTitle = e.Course.Title,
                    Status = e.Status,
                    EnrolledAt = e.EnrolledAt,
                    TotalLessons = e.Course.Lessons.Count,
                    CompletedLessons = e.StudentLessonProgresses.Count(p => p.IsCompleted),
                    ProgressPercentage = e.Course.Lessons.Any()
                        ? Math.Round(e.StudentLessonProgresses.Count(p => p.IsCompleted) * 100m / e.Course.Lessons.Count, 2)
                        : 0
                })
                .ToListAsync();

            return Ok(result);
        }

        [HttpPost("withdraw")]
        public async Task<IActionResult> WithdrawFromCourse(WithdrawEnrollmentDto request) {
            if (request.EnrollmentId <= 0 || request.WithdrawalReasonId <= 0) {
                return BadRequest("EnrollmentId and WithdrawalReasonId are required.");
            }

            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.EnrollmentId == request.EnrollmentId);

            if (enrollment == null) {
                return NotFound("Enrollment not found.");
            }

            if (enrollment.Status != "Enrolled") {
                return BadRequest("Only active enrollments can be withdrawn.");
            }

            var reasonExists = await _context.WithdrawalReasons
                .AnyAsync(r => r.WithdrawalReasonId == request.WithdrawalReasonId && r.IsActive);

            if (!reasonExists) {
                return BadRequest("Withdrawal reason not found.");
            }

            enrollment.Status = "Withdrawn";
            enrollment.WithdrawnAt = DateTime.Now;
            enrollment.WithdrawalReasonId = request.WithdrawalReasonId;
            enrollment.WithdrawalNote = string.IsNullOrWhiteSpace(request.WithdrawalNote) ? null : request.WithdrawalNote.Trim();

            await _context.SaveChangesAsync();

            return Ok(new
            {
                enrollment.EnrollmentId,
                enrollment.StudentId,
                enrollment.CourseId,
                enrollment.Status,
                enrollment.WithdrawnAt,
                enrollment.WithdrawalReasonId,
                enrollment.WithdrawalNote
            });
        }
    }
}
