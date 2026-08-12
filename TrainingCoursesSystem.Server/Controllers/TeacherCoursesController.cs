using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingCoursesSystem.Server.Data;
using TrainingCoursesSystem.Server.DTOs;
using TrainingCoursesSystem.Server.Models;

namespace TrainingCoursesSystem.Server.Controllers {
    [ApiController]
    [Route("api/teacher/courses")]
    public class TeacherCoursesController : ControllerBase {
        private readonly TrainingDbContext _context;

        public TeacherCoursesController(TrainingDbContext context) {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetTeacherCourses([FromQuery] int instructorId) {
            if (instructorId <= 0) {
                return BadRequest("InstructorId is required.");
            }

            var instructorExists = await _context.Users
                .AnyAsync(u => u.UserId == instructorId
                            && u.UserRole == "Instructor"
                            && u.IsActive);

            if (!instructorExists) {
                return BadRequest("Instructor not found.");
            }

            var courses = await _context.Courses
                .AsNoTracking()
                .Where(c => c.InstructorId == instructorId && !c.IsDeleted)
                .Select(c => new TeacherCourseDto
                {
                    CourseId = c.CourseId,
                    Title = c.Title,
                    Category = c.Category,
                    LevelName = c.LevelName,
                    DurationHours = c.DurationHours,
                    IsPublished = c.IsPublished,
                    LessonsCount = c.Lessons.Count,
                    StudentsCount = c.Enrollments.Count()
                })
                .ToListAsync();

            return Ok(courses);
        }

        [HttpGet("{courseId}/students")]
        public async Task<IActionResult> GetCourseStudents(int courseId, [FromQuery] int instructorId) {
            if (instructorId <= 0) {
                return BadRequest("InstructorId is required.");
            }

            var courseExists = await _context.Courses
                .AnyAsync(c => c.CourseId == courseId
                            && c.InstructorId == instructorId
                            && !c.IsDeleted);

            if (!courseExists) {
                return BadRequest("Course not found or not assigned to this instructor.");
            }

            var totalLessons = await _context.Lessons
                .CountAsync(l => l.CourseId == courseId);

            var result = await _context.Enrollments
                .AsNoTracking()
                .Where(e => e.CourseId == courseId)
                .Select(e => new TeacherCourseStudentDto
                {
                    EnrollmentId = e.EnrollmentId,
                    StudentId = e.StudentId,
                    StudentName = e.Student.FullName,
                    StudentEmail = e.Student.Email,
                    Status = e.Status,
                    EnrolledAt = e.EnrolledAt,
                    CompletedAt = e.CompletedAt,
                    WithdrawnAt = e.WithdrawnAt,
                    WithdrawalReasonText = e.WithdrawalReason != null ? e.WithdrawalReason.ReasonText : null,

                    TotalLessons = totalLessons,
                    CompletedLessons = e.StudentLessonProgresses.Count(p => p.IsCompleted),
                    ProgressPercentage = totalLessons > 0
                        ? Math.Round(e.StudentLessonProgresses.Count(p => p.IsCompleted) * 100m / totalLessons, 2)
                        : 0
                })
                .ToListAsync();

            return Ok(result);
        }
    }
}
