using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingCoursesSystem.Server.Data;
using TrainingCoursesSystem.Server.DTOs;
using TrainingCoursesSystem.Server.Helpers;
using TrainingCoursesSystem.Server.Models;
using TrainingCoursesSystem.Server.Services;

namespace TrainingCoursesSystem.Server.Controllers {
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase {
        private readonly TrainingDbContext _context;
        private readonly IEmailSender _emailSender;

        public AuthController(TrainingDbContext context, IEmailSender emailSender) {
            _context = context;
            _emailSender = emailSender;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto request) {
            if (string.IsNullOrWhiteSpace(request.FullName)) {
                return BadRequest("Full name is required.");
            }
            if (string.IsNullOrWhiteSpace(request.Email)) {
                return BadRequest("Email is required.");
            }
            if (string.IsNullOrWhiteSpace(request.Password)) {
                return BadRequest("Password is required.");
            }
            if (request.Password.Length < 6) {
                return BadRequest("Password must be at least 6 characters.");
            }
            var email = request.Email.Trim().ToLower();

            var emailExists = await _context.Users.AnyAsync(u => u.Email == email);

            if (emailExists) {
                return BadRequest("Email is already used.");
            }

            var user = new User
            {
                FullName = request.FullName.Trim(),
                Email = email,
                PasswordHash = PasswordHelper.HashPassword(request.Password),
                UserRole = "Student",
                IsActive = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Account created successfully. Please wait for admin activation."
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto request) {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password)) {
                return BadRequest("Email and password are required.");
            }

            var email = request.Email.Trim().ToLower();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null) {
                return Unauthorized("Invalid email or password.");
            }

            if (!user.IsActive) {
                return Unauthorized("Your account is not active yet. Please wait for admin approval.");
            }

            var passwordIsValid = PasswordHelper.VerifyPassword(request.Password, user.PasswordHash);
            if (!passwordIsValid) {
                return Unauthorized("Invalid email or password.");
            }

            var result = new AuthUserDto
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                UserRole = user.UserRole
            };

            return Ok(result);
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto request) {
            if (string.IsNullOrWhiteSpace(request.Email)) {
                return BadRequest("Email is required.");
            }
            var email = request.Email.Trim().ToLower();

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null) {
                return Ok(new { message = "If this email exists, a reset code has been sent." });
            }

            var code = Random.Shared.Next(100000, 999999).ToString();
            user.PasswordResetCodeHash = PasswordHelper.HashPassword(code);
            user.PasswordResetCodeExpiresAt = DateTime.Now.AddMinutes(10);
            user.PasswordResetCodeUsed = false;

            await _context.SaveChangesAsync();

            var emailBody = $"Your password reset code is: {code}\n\nThis code will expire in 10 minutes.";

            await _emailSender.SendEmailAsync(user.Email, "Password Reset Code", emailBody);

            return Ok(new { message = "If this email exists, a reset code has been sent." });
        }

        [HttpPost("verify-reset-code")]
        public async Task<IActionResult> VerifyResetCode(VerifyResetCodeDto request) {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Code)) {
                return BadRequest("Email and code are required.");
            }

            var user = await FindUserForPasswordReset(request.Email);

            if (user == null) {
                return BadRequest("Invalid or expired reset code.");
            }

            var codeIsValid = PasswordHelper.VerifyPassword(request.Code, user.PasswordResetCodeHash!);
            if (!codeIsValid) {
                return BadRequest("Invalid or expired reset code.");
            }

            return Ok(new { message = "Code verified successfully." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto request) {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Code)) {
                return BadRequest("Email and code are required.");
            }

            if (string.IsNullOrWhiteSpace(request.NewPassword)) {
                return BadRequest("New password is required.");
            }

            if (request.NewPassword.Length < 6) {
                return BadRequest("Password must be at least 6 characters.");
            }

            var user = await FindUserForPasswordReset(request.Email);

            if (user == null) {
                return BadRequest("Invalid or expired reset code.");
            }

            var codeIsValid = PasswordHelper.VerifyPassword(request.Code, user.PasswordResetCodeHash!);
            if (!codeIsValid) {
                return BadRequest("Invalid or expired reset code.");
            }

            user.PasswordHash = PasswordHelper.HashPassword(request.NewPassword);
            user.PasswordResetCodeHash = null;
            user.PasswordResetCodeExpiresAt = null;
            user.PasswordResetCodeUsed = true;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Password has been reset successfully." });
        }

        private async Task<User?> FindUserForPasswordReset(string email) {
            if (string.IsNullOrWhiteSpace(email)) {
                return null;
            }
            var normalizedEmail = email.Trim().ToLower();

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == normalizedEmail);

            if (user == null) return null;
            if (string.IsNullOrWhiteSpace(user.PasswordResetCodeHash)) return null;
            if (user.PasswordResetCodeUsed) return null;
            if (user.PasswordResetCodeExpiresAt == null) return null;
            if (user.PasswordResetCodeExpiresAt < DateTime.Now) return null;

            return user;
        }
    }
}
