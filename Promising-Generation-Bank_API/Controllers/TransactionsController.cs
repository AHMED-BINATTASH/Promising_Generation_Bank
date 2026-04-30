using Microsoft.AspNetCore.Mvc;
using Promising_Generation_Bank_API.Data.Repositories;
using Promising_Generation_Bank_API.DTOs;
using Promising_Generation_Bank_API.Models;

namespace Promising_Generation_Bank_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionsController : ControllerBase
    {
        // بافتراض أنك تستخدم واجهة (Interface) للـ Repository
        private readonly TransactionRepository _transactionRepo;

        public TransactionsController(TransactionRepository transactionRepo)
        {
            _transactionRepo = transactionRepo;
        }

        [HttpGet("Achievements")]
        public async Task<IActionResult> GetAchievements()
        {
            var achievements = await _transactionRepo.GetCompletedAchievementsAsync();

            if (achievements == null || !achievements.Any())
            {
                return NotFound(ApiResponse<List<AchievementDto>>.FailureResponse("No achievements found", ResultCode.NotFound));
            }

            return Ok(ApiResponse<List<AchievementDto>>.SuccessResponse(achievements, "Achievements retrieved successfully", ResultCode.Found));
        }
    }
}

