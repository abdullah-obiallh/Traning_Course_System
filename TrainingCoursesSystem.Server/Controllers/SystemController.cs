using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingCoursesSystem.Server.Data;
using TrainingCoursesSystem.Server.DTOs;
using TrainingCoursesSystem.Server.Models;

namespace TrainingCoursesSystem.Server.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class SystemController : ControllerBase {
        private readonly TrainingDbContext _context;

        public SystemController(TrainingDbContext context) {
            _context = context;
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetStatus() {
            try {
                var databaseConnected = await _context.Database.CanConnectAsync();

                var result = new SystemStatusDto
                {
                    Status = "System is working",
                    DatabaseConnected = databaseConnected,
                    UsersCount = await _context.Users.CountAsync(),
                    CoursesCount = await _context.Courses.CountAsync(c => c.IsDeleted == false),

                    LessonsCount = await _context.Lessons.CountAsync(),

                    EnrollmentsCount = await _context.Enrollments.CountAsync(),
                    WithdrawalReasonsCount = await _context.WithdrawalReasons.CountAsync(r => r.IsActive == true),
                    ServerTime = DateTime.Now
                };

                return Ok(result);
            }
            catch (Exception ex) {
                return StatusCode(500, new
                {
                    Status = "System error",
                    Message = ex.Message
                });
            }
        }
    }
}
