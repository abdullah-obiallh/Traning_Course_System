using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingCoursesSystem.Server.Data;

namespace TrainingCoursesSystem.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly TrainingDbContext _context;

        public TestController(TrainingDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                message = "Backend is working",
                project = "Training Courses System"
            });
        }

        [HttpGet("db")]
        public async Task<IActionResult> TestDatabase()
        {
            var canConnect = await _context.Database.CanConnectAsync();

            var usersCount = await _context.Users.CountAsync();
            var coursesCount = await _context.Courses.CountAsync();
            var lessonsCount = await _context.Lessons.CountAsync();
            var enrollmentsCount = await _context.Enrollments.CountAsync();

            return Ok(new
            {
                databaseConnected = canConnect,
                usersCount,
                coursesCount,
                lessonsCount,
                enrollmentsCount
            });
        }
    }
}