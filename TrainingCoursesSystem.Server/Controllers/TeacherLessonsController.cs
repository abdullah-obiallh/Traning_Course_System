using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingCoursesSystem.Server.Data;
using TrainingCoursesSystem.Server.DTOs;
using TrainingCoursesSystem.Server.Models;

namespace TrainingCoursesSystem.Server.Controllers {
    [ApiController]
    [Route("api/teacher")]
    public class TeacherLessonsController : ControllerBase {
        private readonly TrainingDbContext _context;

        public TeacherLessonsController(TrainingDbContext context) {
            _context = context;
        }
        [HttpGet("courses/{courseId}")]
        public async Task<IActionResult> GetCourseHeader(int courseId, [FromQuery] int instructorId) {
            var course = await _context.Courses.AsNoTracking().Where(c => c.CourseId == courseId&& c.InstructorId == instructorId)
                .Select(c => new CourseHeaderDto
                {
                    CourseId = c.CourseId,
                    Title = c.Title
                })
                .FirstOrDefaultAsync();

            if (course == null) {
                return NotFound("Course not found.");
            }

            return Ok(course);
        }
        [HttpGet("courses/{courseId}/lessons")]
        public async Task<IActionResult> GetCourseLessons(int courseId, [FromQuery] int instructorId) {
            if (instructorId <= 0) {
                return BadRequest("InstructorId is required.");
            }

            var courseExists = await _context.Courses
                .AnyAsync(c => c.CourseId == courseId
                            && c.InstructorId == instructorId
                            && c.IsDeleted == false);

            if (!courseExists) {
                return BadRequest("Course not found or not assigned to this instructor.");
            }

            var lessons = await _context.Lessons
                .AsNoTracking()
                .Where(l => l.CourseId == courseId)
                .OrderBy(l => l.LessonOrder)
                .Select(l => new LessonDto
                {
                    LessonId = l.LessonId,
                    Title = l.Title,
                    Content = l.Content,
                    VideoUrl = l.VideoUrl,
                    LessonOrder = l.LessonOrder,
                    AvailableFrom = l.AvailableFrom,
                    IsLocked = l.AvailableFrom > DateTime.Now
                })
                .ToListAsync();

            return Ok(lessons);
        }
        [HttpDelete("lessons/{lessonId}")]
        public async Task<IActionResult> DeleteLesson(int lessonId, [FromQuery] int instructorId) {
            if (instructorId <= 0) {
                return BadRequest("InstructorId is required.");
            }

            var lesson = await _context.Lessons.FirstOrDefaultAsync(l => l.LessonId == lessonId);

            if (lesson == null) {
                return NotFound("Lesson not found.");
            }

            var courseExists = await _context.Courses.AnyAsync(c => c.CourseId == lesson.CourseId && c.InstructorId == instructorId && c.IsDeleted == false);

            if (!courseExists) {
                return BadRequest("This lesson does not belong to this instructor.");
            }

            var courseId = lesson.CourseId;
            var deletedLessonOrder = lesson.LessonOrder;

            try {
                var strategy = _context.Database.CreateExecutionStrategy();

                await strategy.ExecuteAsync(async () =>
                {
                    await using var transaction = await _context.Database.BeginTransactionAsync();

                    try {
                        await _context.Database.ExecuteSqlRawAsync(
                            "DELETE FROM StudentLessonProgress WHERE LessonId = {0}", lessonId);

                        await _context.Database.ExecuteSqlRawAsync(
                            "DELETE FROM Lessons WHERE LessonId = {0}", lessonId);


                        await _context.Database.ExecuteSqlRawAsync(
                            "UPDATE Lessons SET LessonOrder = LessonOrder + 10000 WHERE CourseId = {0} AND LessonOrder > {1}",
                            courseId, deletedLessonOrder);

                        await _context.Database.ExecuteSqlRawAsync(
                            "UPDATE Lessons SET LessonOrder = LessonOrder - 10001 WHERE CourseId = {0} AND LessonOrder > 10000",
                            courseId);

                        _context.ChangeTracker.Clear();

                        await RecalculateCourseEnrollmentsStatus(courseId);

                        await transaction.CommitAsync();
                    }
                    catch {
                        await transaction.RollbackAsync();
                        throw;
                    }
                });

                return Ok("Lesson permanently deleted successfully.");
            }
            catch (Exception ex) {
                Console.WriteLine($"Delete Hard Error: {ex.Message}");
                return StatusCode(500, $"An error occurred while deleting the lesson: {ex.Message}");
            }
        }


        [HttpPost("courses/{courseId}/lessons")]
        public async Task<IActionResult> CreateLesson(int courseId, CreateLessonDto request) {
            if (request.InstructorId <= 0) {
                return BadRequest("InstructorId is required.");
            }

            if (string.IsNullOrWhiteSpace(request.Title)) {
                return BadRequest("Lesson title is required.");
            }

            if (request.LessonOrder <= 0) {
                return BadRequest("Lesson order must be greater than zero.");
            }
            if (request.AvailableFrom < DateTime.Now) {
                return BadRequest("Available date cannot be in the past.");
            }
            var courseExists = await _context.Courses
                .AnyAsync(c => c.CourseId == courseId
                            && c.InstructorId == request.InstructorId
                            && c.IsDeleted == false);

            if (!courseExists) {
                return BadRequest("Course not found or not assigned to this instructor.");
            }

            var orderExists = await _context.Lessons
                .AnyAsync(l => l.CourseId == courseId
                                && l.LessonOrder == request.LessonOrder);

            if (orderExists) {
                return BadRequest("Lesson order is already used in this course.");
            }

            var lesson = new Lesson
            {
                CourseId = courseId,
                Title = request.Title.Trim(),
                Content = string.IsNullOrWhiteSpace(request.Content) ? null : request.Content.Trim(),
                VideoUrl = string.IsNullOrWhiteSpace(request.VideoUrl) ? null : request.VideoUrl.Trim(),
                LessonOrder = request.LessonOrder,
                AvailableFrom = request.AvailableFrom,
                IsDeleted = false,
                CreatedAt = DateTime.Now
            };

            _context.Lessons.Add(lesson);
            await _context.SaveChangesAsync();

            await RecalculateCourseEnrollmentsStatus(courseId);

            return Ok(new LessonDto
            {
                LessonId = lesson.LessonId,
                Title = lesson.Title,
                Content = lesson.Content,
                VideoUrl = lesson.VideoUrl,
                LessonOrder = lesson.LessonOrder,
                AvailableFrom = lesson.AvailableFrom,
                IsLocked = lesson.AvailableFrom > DateTime.Now
            });
        }

        [HttpPut("lessons/{lessonId}")]
        public async Task<IActionResult> UpdateLesson(int lessonId, UpdateLessonDto request) {
            if (request.InstructorId <= 0) {
                return BadRequest("InstructorId is required.");
            }

            if (string.IsNullOrWhiteSpace(request.Title)) {
                return BadRequest("Lesson title is required.");
            }

            if (request.LessonOrder <= 0) {
                return BadRequest("Lesson order must be greater than zero.");
            }
            if (request.AvailableFrom < DateTime.Now) {
                return BadRequest("Available date cannot be in the past.");
            }
            var lesson = await _context.Lessons
                .FirstOrDefaultAsync(l => l.LessonId == lessonId);

            if (lesson == null) {
                return NotFound("Lesson not found.");
            }

            var courseExists = await _context.Courses
                .AnyAsync(c => c.CourseId == lesson.CourseId
                            && c.InstructorId == request.InstructorId
                            && c.IsDeleted == false);

            if (!courseExists) {
                return BadRequest("This lesson does not belong to this instructor.");
            }

            var orderExists = await _context.Lessons
                .AnyAsync(l => l.CourseId == lesson.CourseId
                            && l.LessonOrder == request.LessonOrder
                            && l.LessonId != lessonId);

            if (orderExists) {
                return BadRequest("Lesson order is already used in this course.");
            }

            lesson.Title = request.Title.Trim();
            lesson.Content = string.IsNullOrWhiteSpace(request.Content) ? null : request.Content.Trim();
            lesson.VideoUrl = string.IsNullOrWhiteSpace(request.VideoUrl) ? null : request.VideoUrl.Trim();
            lesson.LessonOrder = request.LessonOrder;
            lesson.AvailableFrom = request.AvailableFrom;

            await _context.SaveChangesAsync();

            return Ok(new LessonDto
            {
                LessonId = lesson.LessonId,
                Title = lesson.Title,
                Content = lesson.Content,
                VideoUrl = lesson.VideoUrl,
                LessonOrder = lesson.LessonOrder,
                AvailableFrom = lesson.AvailableFrom,
                IsLocked = lesson.AvailableFrom > DateTime.Now
            });
        }

        private async Task RecalculateCourseEnrollmentsStatus(int courseId) {
            var totalLessons = await _context.Lessons.CountAsync(l => l.CourseId == courseId);

            var enrollments = await _context.Enrollments
                .Where(e => e.CourseId == courseId && e.Status != "Withdrawn")
                .ToListAsync();

            var completedLessonsMap = await _context.StudentLessonProgresses
                .Where(p => p.IsCompleted == true && p.Enrollment.CourseId == courseId)
                .GroupBy(p => p.EnrollmentId)
                .Select(g => new { EnrollmentId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.EnrollmentId, x => x.Count);

            foreach (var enrollment in enrollments) {
                completedLessonsMap.TryGetValue(enrollment.EnrollmentId, out int completedLessons);

                if (totalLessons > 0 && completedLessons == totalLessons) {
                    enrollment.Status = "Completed";
                    if (enrollment.CompletedAt == null) {
                        enrollment.CompletedAt = DateTime.Now;
                    }
                }
                else {
                    enrollment.Status = "Enrolled";
                    enrollment.CompletedAt = null;
                }
            }

            await _context.SaveChangesAsync();
        }
    }
}
