using Microsoft.AspNetCore.Mvc;
using Promising_Generation_Bank_API.Data.Repositories;
using Promising_Generation_Bank_API.Data.Repositories.PromisingGenerationBank.Repositories;
using Promising_Generation_Bank_API.Enums;
using Promising_Generation_Bank_API.FinGuardAI.API.Utilities;
using Promising_Generation_Bank_API.Models;

namespace Promising_Generation_Bank_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuestsController : ControllerBase
    {
        private readonly QuestRepository _questRepo;
        private readonly ChildRepository _childRepo;
        private readonly TransactionRepository _transactionRepo;

        public QuestsController(QuestRepository questRepo, ChildRepository childRepo, TransactionRepository transactionRepo)
        {
            _questRepo = questRepo;
            _childRepo = childRepo;
            _transactionRepo = transactionRepo;
        }

        [HttpPost]
        public async Task<IActionResult> CreateQuest([FromBody] Quest quest)
        {
            quest.Status = QuestStatus.Pending;
            quest.CreatedAt = DateTime.UtcNow;

            await _questRepo.AddAsync(quest);
            await _questRepo.SaveChangesAsync();

            return Ok(ApiResponse<Quest>.SuccessResponse(quest, "Quest assigned successfully", ResultCode.Created));
        }

        [HttpPut("{id}/complete")]
        public async Task<IActionResult> CompleteQuest(int id)
        {
            var quest = await _questRepo.GetByIdAsync(id);
            if (quest == null)
                return NotFound(ApiResponse<object>.FailureResponse("Quest not found", ResultCode.NotFound));

            quest.Status = QuestStatus.Completed;
            _questRepo.Update(quest);
            await _questRepo.SaveChangesAsync();

            return Ok(ApiResponse<object>.SuccessResponse(null, "Quest sent to parent for approval", ResultCode.Success));
        }

        [HttpPut("{id}/approve")]
        public async Task<IActionResult> ApproveQuest(int id)
        {
            var quest = await _questRepo.GetByIdAsync(id);
            if (quest == null)
                return NotFound(ApiResponse<object>.FailureResponse("Quest not found", ResultCode.NotFound));

            if (quest.Status != QuestStatus.Completed)
                return BadRequest(ApiResponse<object>.FailureResponse("Quest is not completed yet or already approved", ResultCode.BadRequest));

            var child = await _childRepo.GetByIdAsync(quest.ChildId);
            if (child == null)
                return NotFound(ApiResponse<object>.FailureResponse("Child not found", ResultCode.NotFound));

            quest.Status = QuestStatus.Approved;
            _questRepo.Update(quest);

            child.SavingsBalance += quest.Amount;
            _childRepo.Update(child);

            var transaction = new Transaction
            {
                ChildId = child.Id,
                ActivityName = $"Quest Completion Reward: {quest.Title}",
                CashAmount = quest.Amount,
                Date = DateTime.UtcNow
            };
            await _transactionRepo.AddAsync(transaction);
            await _questRepo.SaveChangesAsync();

            var responseData = new { NewBalance = child.SavingsBalance };

            return Ok(ApiResponse<object>.SuccessResponse(responseData, "Quest approved and amount transferred to the child", ResultCode.Success));
        }
    }
}
