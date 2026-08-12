using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainingCoursesSystem.Server.Data;
using TrainingCoursesSystem.Server.DTOs;

namespace TrainingCoursesSystem.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WithdrawalReasonsController : ControllerBase
    {
        private readonly TrainingDbContext _context;

        public WithdrawalReasonsController(TrainingDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetWithdrawalReasons()
        {
            var reasons = await _context.WithdrawalReasons
                .AsNoTracking()
                .Where(r => r.IsActive == true)
                .Select(r => new WithdrawalReasonDto
                {
                    WithdrawalReasonId = r.WithdrawalReasonId,
                    ReasonText = r.ReasonText
                })
                .ToListAsync();

            return Ok(reasons);
        }
    }
}