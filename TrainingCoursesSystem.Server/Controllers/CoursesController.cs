using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingCoursesSystem.Server.Data;
using TrainingCoursesSystem.Server.DTOs;
using TrainingCoursesSystem.Server.Models;

namespace TrainingCoursesSystem.Server.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class CoursesController : ControllerBase {
        private readonly TrainingDbContext _context;

        public CoursesController(TrainingDbContext context) {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetCourses() {
            var courses = await _context.Courses
                .AsNoTracking()
                .Where(c => c.IsDeleted == false && c.IsPublished == true)
                .Select(c => new CourseListDto
                {
                    CourseId = c.CourseId,
                    Title = c.Title,
                    Description = c.Description,
                    Category = c.Category,
                    LevelName = c.LevelName,
                    DurationHours = c.DurationHours,
                    InstructorName = c.Instructor.FullName,
                    LessonsCount = c.Lessons.Count
                })
                .ToListAsync();

            return Ok(courses);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCourseById(int id) {
            var course = await _context.Courses
                .AsNoTracking()
                .Where(c => c.CourseId == id && c.IsDeleted == false && c.IsPublished == true)
                .Select(c => new CourseDetailsDto
                {
                    CourseId = c.CourseId,
                    Title = c.Title,
                    Description = c.Description,
                    Category = c.Category,
                    LevelName = c.LevelName,
                    DurationHours = c.DurationHours,
                    InstructorName = c.Instructor.FullName,

                    Lessons = c.Lessons
                        .OrderBy(l => l.LessonOrder)
                        .Select(l => new LessonDto
                        {
                            LessonId = l.LessonId,
                            Title = l.Title,
                            LessonOrder = l.LessonOrder,
                            Content = null,   
                            VideoUrl = null  
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (course == null) {
                return NotFound("Course not found");
            }

            return Ok(course);
        }
    }
}
