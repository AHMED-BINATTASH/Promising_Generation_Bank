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
        private readonly SavingsGoalRepository _savingsGoalRepo;
        private readonly ChildRepository _childRepo;
        private readonly TransactionRepository _transactionRepo;

        public SavingsGoalsController(
            SavingsGoalRepository savingsGoalRepo,
            ChildRepository childRepo,
            TransactionRepository transactionRepo)
        {
            _savingsGoalRepo = savingsGoalRepo;
            _childRepo = childRepo;
            _transactionRepo = transactionRepo;
        }

        // 1. إنشاء هدف ادخار جديد للطفل
        [HttpPost]
        public async Task<IActionResult> CreateGoal([FromBody] SavingsGoal goal)
        {
            // التحقق من أن الطفل موجود
            var child = await _childRepo.GetByIdAsync(goal.ChildId);
            if (child == null)
                return NotFound(ApiResponse<SavingsGoal>.FailureResponse("Child account not found", ResultCode.NotFound));

            // تهيئة القيم الافتراضية للهدف الجديد
            goal.CurrentAmount = 0;
            goal.IsCompleted = false;

            await _savingsGoalRepo.AddAsync(goal);
            await _savingsGoalRepo.SaveChangesAsync();

            return Ok(ApiResponse<SavingsGoal>.SuccessResponse(goal, "Savings goal created successfully", ResultCode.Created));
        }

        // 2. تمويل الهدف (تحويل مبلغ من الرصيد العام للطفل إلى هذا الهدف المخصص)
        [HttpPut("{id}/fund")]
        public async Task<IActionResult> FundGoal(int id, [FromBody] decimal amountToFund)
        {
            if (amountToFund <= 0)
                return BadRequest(ApiResponse<object>.FailureResponse("Amount must be greater than zero", ResultCode.BadRequest));

            var goal = await _savingsGoalRepo.GetByIdAsync(id);
            if (goal == null)
                return NotFound(ApiResponse<object>.FailureResponse("Savings goal not found", ResultCode.NotFound));

            if (goal.IsCompleted)
                return BadRequest(ApiResponse<object>.FailureResponse("This goal is already completed", ResultCode.BadRequest));

            var child = await _childRepo.GetByIdAsync(goal.ChildId);
            if (child == null)
                return NotFound(ApiResponse<object>.FailureResponse("Child account not found", ResultCode.NotFound));

            // التحقق من أن الطفل يمتلك رصيداً كافياً لتمويل الهدف
            if (child.SavingsBalance < amountToFund)
                return BadRequest(ApiResponse<object>.FailureResponse("Insufficient savings balance", ResultCode.BadRequest));

            // 1. خصم المبلغ من الرصيد العام للطفل
            child.SavingsBalance -= amountToFund;
            _childRepo.Update(child);

            // 2. إضافة المبلغ للهدف
            goal.CurrentAmount += amountToFund;

            // التحقق مما إذا كان الهدف قد اكتمل
            if (goal.CurrentAmount >= goal.TargetAmount)
            {
                goal.IsCompleted = true;
                goal.CurrentAmount = goal.TargetAmount; // لضمان عدم تجاوز الهدف
            }
            _savingsGoalRepo.Update(goal);

            // 3. تسجيل معاملة مالية (سحب من الرصيد لتمويل الهدف)
            var transaction = new Transaction
            {
                ChildId = child.Id,
                ActivityName = $"Funded Savings Goal: {goal.Name}",
                CashAmount = -amountToFund, // بالسالب لأنها خصم من الرصيد المتاح للطفل
                Date = DateTime.UtcNow
            };
            await _transactionRepo.AddAsync(transaction);

            // حفظ جميع التغييرات معاً (Atomic)
            await _savingsGoalRepo.SaveChangesAsync();

            var responseData = new
            {
                GoalCurrentAmount = goal.CurrentAmount,
                ChildRemainingBalance = child.SavingsBalance,
                IsCompleted = goal.IsCompleted
            };

            return Ok(ApiResponse<object>.SuccessResponse(responseData, "Goal funded successfully", ResultCode.Success));
        }

        // 3. حذف هدف ادخار (اختياري - في حال تراجع الطفل عن الهدف)
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGoal(int id)
        {
            var goal = await _savingsGoalRepo.GetByIdAsync(id);
            if (goal == null)
                return NotFound(ApiResponse<object>.FailureResponse("Savings goal not found", ResultCode.NotFound));

            // إذا كان هناك مبلغ مجمع في الهدف، نقوم بإعادته لرصيد الطفل قبل الحذف
            if (goal.CurrentAmount > 0)
            {
                var child = await _childRepo.GetByIdAsync(goal.ChildId);
                if (child != null)
                {
                    child.SavingsBalance += goal.CurrentAmount;
                    _childRepo.Update(child);

                    var transaction = new Transaction
                    {
                        ChildId = child.Id,
                        ActivityName = $"Refund from deleted goal: {goal.Name}",
                        CashAmount = goal.CurrentAmount, // بالموجب لأنها استرداد
                        Date = DateTime.UtcNow
                    };
                    await _transactionRepo.AddAsync(transaction);
                }
            }

            _savingsGoalRepo.Delete(goal);
            await _savingsGoalRepo.SaveChangesAsync();

            return Ok(ApiResponse<object>.SuccessResponse(null, "Savings goal deleted successfully", ResultCode.Deleted));
        }
    }
}
