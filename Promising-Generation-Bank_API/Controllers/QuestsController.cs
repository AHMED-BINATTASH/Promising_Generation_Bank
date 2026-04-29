using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Promising_Generation_Bank_API.AgentComponents;
using Promising_Generation_Bank_API.Data;
using Promising_Generation_Bank_API.Data.Repositories;
using Promising_Generation_Bank_API.DTOs;
using Promising_Generation_Bank_API.Enums;
using Promising_Generation_Bank_API.Models;

namespace Promising_Generation_Bank_API.Controllers
{
    [ApiController]
    [Route("api/Quests")]
    public class QuestsController : ControllerBase
    {
        private readonly AgentService _agentService;
        private readonly QuestRepository _questRepo;
        private readonly AppDbContext _context;
        public QuestsController(AgentService agentService, QuestRepository questRepo, AppDbContext context)
        {
            _agentService = agentService;
            _questRepo = questRepo;
            _context = context;
        }

        // Get all Releted Quests to Parent
        [HttpGet("GetParentQuestsByParentId")]
        public async Task<IActionResult> GetParentQuests(int parentId)
        {
            var quests = await _questRepo.GetQuestsByParentAsync(parentId);
            return Ok(ApiResponse<IEnumerable<Quest>>.SuccessResponse(quests, "Quests retrieved for parent dashboard", ResultCode.Success));
        }

        // Get all Quests of child by ID 
        [HttpGet("GetApprovedChildQuestsByChildId")]
        public async Task<IActionResult> GetChildQuests(int childId)
        {
            var quests = await _questRepo.GetActiveQuestsForChildAsync(childId);
            return Ok(ApiResponse<IEnumerable<Quest>>.SuccessResponse(quests, "Available quests for child retrieved", ResultCode.Success));
        }

        [HttpPost("Add")]
        public async Task<IActionResult> AddQuest([FromBody] Quest quest)
        {
            var result = await _questRepo.AddAsync(quest);
            return Ok(ApiResponse<Quest>.SuccessResponse(result, "New quest created successfully", ResultCode.Created));
        }

        [HttpDelete("Delete")]
        public async Task<IActionResult> DeleteQuest(int id)
        {
            await _questRepo.DeleteAsync(id);
            return Ok(ApiResponse<string>.SuccessResponse(null, "Quest deleted successfully", ResultCode.Success));
        }
        [HttpGet("GetTotalChildBalanceByChildId")]
        public async Task<IActionResult> GetTotalChildBalance(int childId)
        {
            var totalBalance = await _context.Children
                .Where(c => c.Id == childId)  
                .SumAsync(c => c.SavingsBalance);

            return Ok(ApiResponse<decimal>.SuccessResponse(totalBalance, "Total child balance calculated", ResultCode.Success));
        }

        // 1. إجمالي رصيد العائلة
        [HttpGet("GetTotalFamilyBalance")]
        public async Task<IActionResult> GetTotalFamilyBalance(int parentId)
        {
            var totalBalance = await _context.Children
                .Where(c => c.ParentId == parentId)
                .SumAsync(c => c.SavingsBalance);

            return Ok(ApiResponse<decimal>.SuccessResponse(totalBalance, "Total family balance calculated", ResultCode.Success));
        }

        // 2. ما تم كسبه هذا الأسبوع
        [HttpGet("GetEarnedThisWeekFromQuests")]
        public async Task<IActionResult> GetEarnedThisWeekFromQuests(int parentId)
        {

            var oneWeekAgo = DateTime.UtcNow.AddDays(-7);


            var earned = await _context.Quests
                .Where(q => q.ParentId == parentId
                         && q.Status == QuestStatus.Approved
                         && q.CreatedAt >= oneWeekAgo)
                .SumAsync(q => q.Amount);

            return Ok(ApiResponse<decimal>.SuccessResponse(earned, "Weekly earnings calculated from Quests", ResultCode.Success));
        }

        // 3. المهام التي تنتظر الموافقة (عدد فقط)
        [HttpGet("GetPendingApprovalsCount")]
        public async Task<IActionResult> GetPendingApprovalsCount(int parentId)
        {
            var count = await _context.Quests
                .CountAsync(q => q.ParentId == parentId && q.Status == QuestStatus.Completed);

            return Ok(ApiResponse<int>.SuccessResponse(count, "Pending approvals count retrieved", ResultCode.Success));
        }

        // 4. عدد الأطفال النشطين حالياً
        [HttpGet("GetActiveChildrenCount")]
        public async Task<IActionResult> GetActiveChildrenCount(int parentId)
        {
            var count = await _context.Children
                .Where(q => q.ParentId == parentId)
                .CountAsync();

            return Ok(ApiResponse<int>.SuccessResponse(count, "Active children count retrieved", ResultCode.Success));
        }

        [HttpPut("{id}/Complete")]
        public async Task<IActionResult> CompleteQuest(int id)
        {
            bool isCompleted = await _questRepo.MakeQuestCompletedAsync(id);

            if (!isCompleted)
            {
                return NotFound(ApiResponse<bool>.FailureResponse("Quest not found or could not be completed", ResultCode.NotFound));
            }

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Quest completed successfully and transaction recorded", ResultCode.Success));
        }

        [HttpPut("{id}/Approve")]
        public async Task<IActionResult> ApproveQuest(int id)
        {
            bool isApproved = await _questRepo.MakeQuestApprovedAsync(id);

            if (!isApproved)
            {
                return NotFound(ApiResponse<bool>.FailureResponse("Quest not found or could not be approved", ResultCode.NotFound));
            }

            return Ok(ApiResponse<bool>.SuccessResponse(true, "Quest approved and child balance updated successfully", ResultCode.Success));
        }

        [HttpPost("generate")]
        public async Task<IActionResult> GenerateQuests([FromBody] QuestRequestDto request)
        {
            // 1. Validation using ApiResponse
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(ApiResponse<object>.FailureResponse(
                    "Message details are required to generate quests",
                    ResultCode.BadRequest));
            }

            // 2. Session Management
            string sessionId = string.IsNullOrWhiteSpace(request.SessionId)
                ? Guid.NewGuid().ToString()
                : request.SessionId;

            try
            {
                // 3. Call AI Agent Service
                string jsonResult = await _agentService.RunAgentAsync(sessionId, request.Message);

                // 4. ✨ The Professional Solution: 
                // We parse the string into an object so that it's serialized correctly 
                // inside the ApiResponse wrapper without double escaping.
                var generatedData = System.Text.Json.JsonSerializer.Deserialize<object>(jsonResult);

                return Ok(ApiResponse<object>.SuccessResponse(
                    generatedData,
                    "Quests generated successfully by AI",
                    ResultCode.Success));
            }
            catch (Exception ex)
            {
                // 5. Global Error Handling
                return StatusCode(500, ApiResponse<object>.FailureResponse(
                    $"An error occurred while generating quests: {ex.Message}",
                    ResultCode.InternalError));
            }
        }

    }
}
