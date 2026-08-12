using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingCoursesSystem.Server.Data;
using TrainingCoursesSystem.Server.DTOs;
using TrainingCoursesSystem.Server.Models;

namespace TrainingCoursesSystem.Server.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class ProgressController : ControllerBase {
        private readonly TrainingDbContext _context;

        public ProgressController(TrainingDbContext context) {
            _context = context;
        }

        [HttpPost("complete-lesson")]
        public async Task<IActionResult> CompleteLesson(CompleteLessonDto request) {
            if (request.EnrollmentId <= 0 || request.LessonId <= 0) {
                return BadRequest("EnrollmentId and LessonId are required.");
            }

            var enrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.EnrollmentId == request.EnrollmentId);

            if (enrollment == null) {
                return NotFound("Enrollment not found.");
            }

            if (enrollment.Status != "Enrolled") {
                return BadRequest("This enrollment is not active.");
            }

            var lesson = await _context.Lessons
                .FirstOrDefaultAsync(l => l.LessonId == request.LessonId && l.IsDeleted == false);

            if (lesson == null) {
                return NotFound("Lesson not found.");
            }
            if (lesson.AvailableFrom > DateTime.Now) {
                return BadRequest("This lesson is not available yet.");
            }

            if (lesson.CourseId != enrollment.CourseId) {
                return BadRequest("This lesson does not belong to the enrolled course.");
            }

            var alreadyCompleted = await _context.StudentLessonProgresses
                .AnyAsync(p => p.EnrollmentId == request.EnrollmentId
                            && p.LessonId == request.LessonId
                            && p.IsCompleted == true);

            if (alreadyCompleted) {
                return BadRequest("This lesson is already completed.");
            }

            var progress = new StudentLessonProgress
            {
                EnrollmentId = request.EnrollmentId,
                LessonId = request.LessonId,
                IsCompleted = true,
                CompletedAt = DateTime.Now
            };

            _context.StudentLessonProgresses.Add(progress);
            await _context.SaveChangesAsync();

            var result = await CalculateProgress(enrollment.EnrollmentId, enrollment.CourseId);

            if (result.TotalLessons > 0 && result.CompletedLessons == result.TotalLessons) {
                enrollment.Status = "Completed";
                enrollment.CompletedAt = DateTime.Now;

                await _context.SaveChangesAsync();

                result.EnrollmentStatus = "Completed";
            }

            return Ok(result);
        }

        [HttpGet("enrollment/{enrollmentId}")]
        public async Task<IActionResult> GetEnrollmentProgress(int enrollmentId) {
            var enrollment = await _context.Enrollments
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.EnrollmentId == enrollmentId);

            if (enrollment == null) {
                return NotFound("Enrollment not found.");
            }

            var result = await CalculateProgress(enrollment.EnrollmentId, enrollment.CourseId);
            result.EnrollmentStatus = enrollment.Status;

            return Ok(result);
        }
        [HttpGet("enrollment/{enrollmentId}/course")]
        public async Task<IActionResult> GetCourseHeader(int enrollmentId) {
            var course = await _context.Enrollments
                .AsNoTracking()
                .Where(e => e.EnrollmentId == enrollmentId)
                .Select(e => new CourseHeaderDto
                {
                    CourseId = e.Course.CourseId,
                    Title = e.Course.Title
                })
                .FirstOrDefaultAsync();

            if (course == null) {
                return NotFound("Course not found.");
            }

            return Ok(course);
        }
        [HttpGet("enrollment/{enrollmentId}/lessons")]
        public async Task<IActionResult> GetEnrollmentLessons(int enrollmentId) {
            var enrollment = await _context.Enrollments
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.EnrollmentId == enrollmentId);

            if (enrollment == null) {
                return NotFound("Enrollment not found.");
            }

            var userProgress = await _context.StudentLessonProgresses
                .AsNoTracking()
                .Where(p => p.EnrollmentId == enrollmentId && p.IsCompleted == true)
                .ToDictionaryAsync(p => p.LessonId);

            var dbLessons = await _context.Lessons
                .AsNoTracking()
                .Where(l => l.CourseId == enrollment.CourseId && l.IsDeleted == false)
                .OrderBy(l => l.LessonOrder)
                .ToListAsync();

            var lessonsDto = dbLessons.Select(l => {
                var hasProgress = userProgress.TryGetValue(l.LessonId, out var progress);
                return new StudentLessonDto
                {
                    LessonId = l.LessonId,
                    Title = l.Title,
                    Content = l.AvailableFrom <= DateTime.Now ? l.Content : null,
                    VideoUrl = l.AvailableFrom <= DateTime.Now ? l.VideoUrl : null,
                    LessonOrder = l.LessonOrder,
                    AvailableFrom = l.AvailableFrom,
                    IsLocked = l.AvailableFrom > DateTime.Now,
                    IsCompleted = hasProgress,
                    CompletedAt = hasProgress ? progress?.CompletedAt : null
                };
            }).ToList();

            return Ok(lessonsDto);
        }

        private async Task<ProgressResultDto> CalculateProgress(int enrollmentId, int courseId) {
            var totalLessons = await _context.Lessons
                .CountAsync(l => l.CourseId == courseId && l.IsDeleted == false);

            var completedLessons = await _context.StudentLessonProgresses
                .CountAsync(p => p.EnrollmentId == enrollmentId
                              && p.IsCompleted == true
                              && _context.Lessons.Any(l => l.LessonId == p.LessonId
                                                        && l.CourseId == courseId
                                                        && l.IsDeleted == false));

            decimal progressPercentage = 0;

            if (totalLessons > 0) {
                progressPercentage = completedLessons * 100m / totalLessons;
            }

            return new ProgressResultDto
            {
                EnrollmentId = enrollmentId,
                CourseId = courseId,
                TotalLessons = totalLessons,
                CompletedLessons = completedLessons,
                ProgressPercentage = Math.Round(progressPercentage, 2),
                EnrollmentStatus = "Enrolled"
            };
        }
    }
}
