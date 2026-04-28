using Microsoft.AspNetCore.Mvc;
using Promising_Generation_Bank_API.Data.Repositories;
using Promising_Generation_Bank_API.Data.Repositories.PromisingGenerationBank.Repositories;
using Promising_Generation_Bank_API.FinGuardAI.API.Utilities;
using Promising_Generation_Bank_API.Models;

namespace Promising_Generation_Bank_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SavingsGoalsController : ControllerBase
    {
        private readonly SavingsGoalRepository _goalRepo;

        public SavingsGoalsController(SavingsGoalRepository goalRepo)
        {
            _goalRepo = goalRepo;
        }

        // [Child View] - الحصول على أهداف الادخار للطفل
        [HttpGet("Get/{childId}")]
        public async Task<IActionResult> GetChildGoals(int childId)
        {
            var goals = await _goalRepo.GetGoalsByChildIdAsync(childId);
            return Ok(ApiResponse<IEnumerable<SavingsGoal>>.SuccessResponse(goals, "Savings goals retrieved successfully", ResultCode.Success));
        }

        // [CRUD] - إضافة هدف ادخار جديد (مثلاً: شراء سيكل)
        [HttpPost("Add")]
        public async Task<IActionResult> CreateGoal([FromBody] SavingsGoal goal)
        {
            var newGoal = await _goalRepo.AddAsync(goal);
            return Ok(ApiResponse<SavingsGoal>.SuccessResponse(newGoal, "Savings goal added to your piggy bank", ResultCode.Created));
        }

        // [Child Action] - إضافة مبلغ للهدف (لتحديث الأنيميشن في الواجهة)
        [HttpPatch("{id}/add-money")]
        public async Task<IActionResult> ContributeToGoal(int id, [FromBody] decimal amount)
        {
            var goal = await _goalRepo.UpdateProgressAsync(id, amount);
            if (goal == null) return NotFound(ApiResponse<SavingsGoal>.FailureResponse("Goal not found", ResultCode.NotFound));

            return Ok(ApiResponse<SavingsGoal>.SuccessResponse(goal, "Money added to your goal! Watch your piggy bank grow!", ResultCode.Success));
        }
    }
}