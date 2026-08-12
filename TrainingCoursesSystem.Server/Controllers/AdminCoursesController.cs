using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingCoursesSystem.Server.Data;
using TrainingCoursesSystem.Server.DTOs;
using TrainingCoursesSystem.Server.Models;

namespace TrainingCoursesSystem.Server.Controllers {
    [ApiController]
    [Route("api/admin/courses")]
    public class AdminCoursesController : ControllerBase {
        private readonly TrainingDbContext _context;

        public AdminCoursesController(TrainingDbContext context) {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetCourses() {
            var courses = await _context.Courses
                .AsNoTracking()
                .Where(c => c.IsDeleted == false)
                .Select(c => new AdminCourseDto
                {
                    CourseId = c.CourseId,
                    Title = c.Title,
                    Description = c.Description,
                    Category = c.Category,
                    LevelName = c.LevelName,
                    DurationHours = c.DurationHours,
                    InstructorId = c.InstructorId,
                    InstructorName = c.Instructor.FullName,
                    IsPublished = c.IsPublished,
                    LessonsCount = c.Lessons.Count,
                    EnrollmentsCount = c.Enrollments.Count()
                })
                .ToListAsync();

            return Ok(courses);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCourseById(int id) {
            var course = await _context.Courses
                .AsNoTracking()
                .Where(c => c.CourseId == id && c.IsDeleted == false)
                .Select(c => new AdminCourseDto
                {
                    CourseId = c.CourseId,
                    Title = c.Title,
                    Description = c.Description,
                    Category = c.Category,
                    LevelName = c.LevelName,
                    DurationHours = c.DurationHours,
                    InstructorId = c.InstructorId,
                    InstructorName = c.Instructor.FullName,
                    IsPublished = c.IsPublished,
                    LessonsCount = c.Lessons.Count,
                    EnrollmentsCount = c.Enrollments.Count()
                })
                .FirstOrDefaultAsync();

            if (course == null) {
                return NotFound("Course not found.");
            }

            return Ok(course);
        }

        [HttpGet("instructors")]
        public async Task<IActionResult> GetInstructors() {
            var instructors = await _context.Users
                .AsNoTracking()
                .Where(u => u.UserRole == "Instructor" && u.IsActive == true)
                .Select(u => new InstructorDto
                {
                    InstructorId = u.UserId,
                    FullName = u.FullName,
                    Email = u.Email
                })
                .ToListAsync();

            return Ok(instructors);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCourse(CreateCourseDto request) {
            if (string.IsNullOrWhiteSpace(request.Title)) {
                return BadRequest("Course title is required.");
            }

            if (request.DurationHours <= 0) {
                return BadRequest("Duration hours must be greater than zero.");
            }

            var instructorExists = await _context.Users
                .AnyAsync(u => u.UserId == request.InstructorId
                            && u.UserRole == "Instructor"
                            && u.IsActive == true);

            if (!instructorExists) {
                return BadRequest("Instructor not found.");
            }

            var course = new Course
            {
                Title = request.Title.Trim(),
                Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
                Category = string.IsNullOrWhiteSpace(request.Category) ? null : request.Category.Trim(),
                LevelName = string.IsNullOrWhiteSpace(request.LevelName) ? null : request.LevelName.Trim(),
                DurationHours = request.DurationHours,
                InstructorId = request.InstructorId,
                IsPublished = request.IsPublished,
                IsDeleted = false,
                CreatedAt = DateTime.Now
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                course.CourseId,
                course.Title,
                course.Description,
                course.Category,
                course.LevelName,
                course.DurationHours,
                course.InstructorId,
                course.IsPublished
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCourse(int id, UpdateCourseDto request) {
            if (string.IsNullOrWhiteSpace(request.Title)) {
                return BadRequest("Course title is required.");
            }

            if (request.DurationHours <= 0) {
                return BadRequest("Duration hours must be greater than zero.");
            }

            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.CourseId == id && c.IsDeleted == false);

            if (course == null) {
                return NotFound("Course not found.");
            }

            var instructorExists = await _context.Users
                .AnyAsync(u => u.UserId == request.InstructorId
                            && u.UserRole == "Instructor"
                            && u.IsActive == true);

            if (!instructorExists) {
                return BadRequest("Instructor not found.");
            }

            course.Title = request.Title.Trim();
            course.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
            course.Category = string.IsNullOrWhiteSpace(request.Category) ? null : request.Category.Trim();
            course.LevelName = string.IsNullOrWhiteSpace(request.LevelName) ? null : request.LevelName.Trim();
            course.DurationHours = request.DurationHours;
            course.InstructorId = request.InstructorId;
            course.IsPublished = request.IsPublished;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                course.CourseId,
                course.Title,
                course.Description,
                course.Category,
                course.LevelName,
                course.DurationHours,
                course.InstructorId,
                course.IsPublished
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCourse(int id) {
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.CourseId == id && c.IsDeleted == false);

            if (course == null) {
                return NotFound("Course not found.");
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try {
                var courseLessons = await _context.Lessons
                    .Where(l => l.CourseId == id)
                    .ToListAsync();

                var lessonIds = courseLessons.Select(l => l.LessonId).ToList();

                var lessonsProgress = await _context.StudentLessonProgresses
                    .Where(p => lessonIds.Contains(p.LessonId))
                    .ToListAsync();

                _context.StudentLessonProgresses.RemoveRange(lessonsProgress);

                _context.Lessons.RemoveRange(courseLessons);

                course.IsDeleted = true;
                course.IsPublished = false;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok("Course and all its associated lessons permanently deleted successfully.");
            }
            catch {
                await transaction.RollbackAsync();
                return StatusCode(500, "An error occurred while deleting the course and its lessons.");
            }
        }
    }
}
