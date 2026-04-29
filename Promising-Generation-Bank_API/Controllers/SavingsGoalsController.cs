using Microsoft.AspNetCore.Mvc;
using Promising_Generation_Bank_API.Data;
using Promising_Generation_Bank_API.Data.Repositories.PromisingGenerationBank.Repositories;
using Promising_Generation_Bank_API.Models;

namespace Promising_Generation_Bank_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SavingsGoalsController : ControllerBase
    {
        private readonly SavingsGoalRepository _goalRepo;
        private readonly AppDbContext _context;

        public SavingsGoalsController(SavingsGoalRepository goalRepo, AppDbContext context)
        {
            _goalRepo = goalRepo;
            _context = context;

        }

        [HttpGet("GetChildGoals")]
        public async Task<IActionResult> GetChildGoals(int childId)
        {
            var goals = await _goalRepo.GetGoalsByChildIdAsync(childId);
            return Ok(ApiResponse<IEnumerable<SavingsGoal>>.SuccessResponse(goals, "Savings goals retrieved successfully", ResultCode.Success));
        }

        [HttpPost("Add")]
        public async Task<IActionResult> CreateGoal([FromBody] SavingsGoal goal)
        {
            var newGoal = await _goalRepo.AddAsync(goal);
            return Ok(ApiResponse<SavingsGoal>.SuccessResponse(newGoal, "Savings goal added to your piggy bank", ResultCode.Created));
        }

        //[HttpPatch("ContributeToGoal")]
        //public async Task<IActionResult> ContributeToGoal(int id, [FromBody] decimal amount)
        //{
        //    var goal = await _goalRepo.UpdateProgressAsync(id, amount);
        //    if (goal == null) return NotFound(ApiResponse<SavingsGoal>.FailureResponse("Goal not found", ResultCode.NotFound));

        //    return Ok(ApiResponse<SavingsGoal>.SuccessResponse(goal, "Money added to your goal! Watch your piggy bank grow!", ResultCode.Success));
        //}



        [HttpGet("GetCompletedGoals")]
        public async Task<IActionResult> GetCompletedGoals(int id)
        {
            var goal = await _goalRepo.GetCompletedGoalsAsync(id);
            if (goal == null) return NotFound(ApiResponse<SavingsGoal>.FailureResponse("completed Goal not found", ResultCode.NotFound));

            return Ok(ApiResponse<IEnumerable<object>>.SuccessResponse(goal, "Get all Completed Goals", ResultCode.Success));
        }


        [HttpGet("AddSavingsDeposit")]
        public async Task<IActionResult> AddSavingsDepositAsync(int goalId, decimal amount)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. جلب الهدف من قاعدة البيانات
                var goal =  _context.SavingsGoals.FirstOrDefault(g => g.Id == goalId );


                if (goal == null)  return NotFound(ApiResponse<bool>.FailureResponse("the Goal is not Found", ResultCode.NotFound));

                // 2. تحديث المبلغ الحالي في الهدف
                goal.CurrentAmount += amount;

                // التحقق مما إذا كان الهدف قد اكتمل
                if (goal.CurrentAmount >= goal.TargetAmount)
                {
                    goal.IsCompleted = true;
                }

                // 3. إنشاء سجل العملية الجديد
                var savingsLog = new SavingsTransaction(goalId, amount);

                // 4. الحفظ في قاعدة البيانات
                _context.SavingsTransactions.Add(savingsLog);
                await _context.SaveChangesAsync();

                // تأكيد العملية (Commit)
                await transaction.CommitAsync();
                return Ok(ApiResponse<bool>.SuccessResponse( true , $"The {amount} is added ", ResultCode.Success)); ;
            }
            catch (Exception e )
            {
                // في حال حدوث أي خطأ، يتم التراجع عن كل التغييرات
                await transaction.RollbackAsync();
                return NotFound(ApiResponse<bool>.FailureResponse( e.Message, ResultCode.NotFound)); ;
            }
        }

    }
}