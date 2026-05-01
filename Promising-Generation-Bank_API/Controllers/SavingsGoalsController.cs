using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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


        [HttpPost("AddSavingsDeposit")] // تعديل هام: استخدم HttpPost للعمليات التي تعدل البيانات
        public async Task<IActionResult> AddSavingsDepositAsync(int goalId, decimal amount, int? childId)
        {
            // بدء Transaction لضمان تنفيذ كل العمليات معاً أو التراجع عنها معاً
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. جلب بيانات الطفل والخصم من حسابه (إذا تم تمرير childId)
                if (childId.HasValue)
                {
                    var child = await _context.Children.FindAsync(childId.Value);
                    if (child == null)
                    {
                        return NotFound(ApiResponse<bool>.FailureResponse("Child not found", ResultCode.NotFound));
                    }

                    // التأكد من أن رصيد الطفل يكفي لعملية الإيداع
                    // الرجاء تغيير 'Balance' أو 'WalletBalance' إلى اسم الخاصية الفعلي لديك في كلاس الطفل
                    if (child.SavingsBalance < amount)
                    {
                        return BadRequest(ApiResponse<bool>.FailureResponse("Insufficient balance in the child's account", ResultCode.BadRequest));
                    }

                    // خصم المبلغ من رصيد الطفل
                    child.SavingsBalance -= amount;
                }

                // 2. جلب الهدف من قاعدة البيانات
                var goal = await _context.SavingsGoals.FirstOrDefaultAsync(g => g.Id == goalId);

                if (goal == null)
                {
                    return NotFound(ApiResponse<bool>.FailureResponse("The Goal is not Found", ResultCode.NotFound));
                }

                // 3. تحديث المبلغ الحالي في الهدف
                goal.CurrentAmount += amount;

                // التحقق مما إذا كان الهدف قد اكتمل
                if (goal.CurrentAmount >= goal.TargetAmount)
                {
                    goal.IsCompleted = true;
                }

                // 4. إنشاء سجل العملية الجديد
                var savingsLog = new SavingsTransaction(goalId, amount, childId);

                // 5. الحفظ في قاعدة البيانات
                await _context.SavingsTransactions.AddAsync(savingsLog);

                // حفظ جميع التغييرات (خصم الرصيد، تحديث الهدف، إضافة السجل) دفعة واحدة
                await _context.SaveChangesAsync();

                // 6. تأكيد العملية (Commit)
                await transaction.CommitAsync();

                return Ok(ApiResponse<bool>.SuccessResponse(true, $"An amount of {amount} was added to the goal and deducted from the child's balance", ResultCode.Success));
            }
            catch (Exception e)
            {
                // في حال حدوث أي خطأ، يتم التراجع عن كل التغييرات
                await transaction.RollbackAsync();

                // يفضل استخدام 500 Internal Server Error في حال حدوث Exception بدلاً من NotFound
                return StatusCode(500, ApiResponse<bool>.FailureResponse(e.Message, ResultCode.InternalError));
            }
        }

        [HttpGet("GetSavingJourneys")]
        public async Task<IActionResult> GetSavingJourneys(int parentId)
        {
            var query = (from st in _context.SavingsTransactions
                         join s in _context.SavingsGoals on st.SavingsGoalId equals s.Id
                         join c in _context.Children on s.ChildId equals c.Id
                         join p in _context.Parents on c.ParentId equals parentId
                         group new { st, s, c } by st.Id into g
                         select new
                         {
                             goalName = g.FirstOrDefault().s.Name,
                             ChildName = g.FirstOrDefault().c.Name,
                             Amount = g.FirstOrDefault().st.Amount
                         });

            var result = await query.ToListAsync();

            if (result.Count == 0)
            {
                return Ok(ApiResponse<IEnumerable<object>>.FailureResponse("No Saving Journeys found for the specified child.", ResultCode.NotFound));
            }

            return Ok(ApiResponse<IEnumerable<object>>.SuccessResponse(result, "Get all Saving Journeys", ResultCode.Success));
        }
    }
}