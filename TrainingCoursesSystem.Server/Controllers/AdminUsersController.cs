using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingCoursesSystem.Server.Data;
using TrainingCoursesSystem.Server.DTOs;
using TrainingCoursesSystem.Server.Models;

namespace TrainingCoursesSystem.Server.Controllers {
    [ApiController]
    [Route("api/admin/users")]
    public class AdminUsersController : ControllerBase {
        private readonly TrainingDbContext _context;

        public AdminUsersController(TrainingDbContext context) {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers() {
            var users = await _context.Users
                .AsNoTracking()
                .Where(u => u.UserRole != "Admin")
                .OrderByDescending(u => u.CreatedAt)
                .Select(u => new UserManagementDto
                {
                    UserId = u.UserId,
                    FullName = u.FullName,
                    Email = u.Email,
                    UserRole = u.UserRole,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();

            return Ok(users);
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateUserStatus(int id, UpdateUserStatusDto request) {

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == id);
            if (user.UserRole == "Admin") {
                return BadRequest("Admin account cannot be modified.");
            }
            if (user == null) {
                return NotFound("User not found.");
            }

            user.IsActive = request.IsActive;

            if (user.UserRole == "Instructor" && !user.IsActive) {
                var instructorCourses = await _context.Courses
                    .Where(c => c.InstructorId == id && c.IsDeleted == false)
                    .ToListAsync();

                foreach (var course in instructorCourses) {
                    course.IsPublished = false;
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "User status updated successfully." });
        }

        [HttpPut("{id}/role")]
        public async Task<IActionResult> UpdateUserRole(int id, UpdateUserRoleDto request) {
            var allowedRoles = new[] { "Student", "Instructor" };

            if (!allowedRoles.Contains(request.UserRole)) {
                return BadRequest("Role must be Student or Instructor.");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == id);
            if (user.UserRole == "Admin") {
                return BadRequest("Admin account cannot be modified.");
            }
            if (user == null) {
                return NotFound("User not found.");
            }

            if (user.UserRole == "Instructor" && request.UserRole == "Student") {
                var hasActiveCourses = await _context.Courses
                    .AnyAsync(c => c.InstructorId == id && c.IsDeleted == false);

                if (hasActiveCourses) {
                    return BadRequest("Cannot change role. This instructor has active courses assigned. Reassign or delete the courses first.");
                }
            }

            user.UserRole = request.UserRole;

            await _context.SaveChangesAsync();

            return Ok(new { message = "User role updated successfully." });
        }
    }
}
