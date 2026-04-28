using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Promising_Generation_Bank_API.AgentComponents;
using Promising_Generation_Bank_API.Data;
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
        private readonly AgentService _agentService;
        private readonly QuestRepository _questRepo;
        private readonly AppDbContext _context;
        public QuestsController(AgentService agentService, QuestRepository questRepo, AppDbContext context)
        {
            _agentService = agentService;
            _questRepo = questRepo;
            _context = context;
        }

        // [Parent View] - عرض كل المهام لإدارة الأب
        [HttpGet("parent/{parentId}")]
        public async Task<IActionResult> GetParentQuests(int parentId)
        {
            var quests = await _questRepo.GetQuestsByParentAsync(parentId);
            return Ok(ApiResponse<IEnumerable<Quest>>.SuccessResponse(quests, "Quests retrieved for parent dashboard", ResultCode.Success));
        }

        // [Child View] - عرض المهام النشطة فقط للطفل (Quest Map)
        [HttpGet("child/{childId}")]
        public async Task<IActionResult> GetChildQuests(int childId)
        {
            var quests = await _questRepo.GetActiveQuestsForChildAsync(childId);
            return Ok(ApiResponse<IEnumerable<Quest>>.SuccessResponse(quests, "Available quests for child retrieved", ResultCode.Success));
        }

        // [Parent/Child] - إضافة مهمة جديدة
        [HttpPost("Add")]
        public async Task<IActionResult> CreateQuest([FromBody] Quest quest)
        {
            var result = await _questRepo.AddAsync(quest);
            return Ok(ApiResponse<Quest>.SuccessResponse(result, "New quest created successfully", ResultCode.Created));
        }

        // [Child Action] - تحديث حالة المهمة (تفعيل الـ Confetti في الفرونت إند عند النجاح)
        [HttpPatch("{id}/complete")]
        public async Task<IActionResult> CompleteQuest(int id)
        {
            var updatedQuest = await _questRepo.UpdateStatusAsync(id, "Completed");
            if (updatedQuest == null) return NotFound(ApiResponse<Quest>.FailureResponse("Quest not found", ResultCode.NotFound));

            return Ok(ApiResponse<Quest>.SuccessResponse(updatedQuest, "Quest marked as completed! Ready for confetti!", ResultCode.Success));
        }

        [HttpPatch("{id}/Approved")]
        public async Task<IActionResult> ApprovedQuest(int id)
        {
            var updatedQuest = await _questRepo.UpdateStatusAsync(id, "Approved");
            if (updatedQuest == null) return NotFound(ApiResponse<Quest>.FailureResponse("Quest not found", ResultCode.NotFound));

            return Ok(ApiResponse<Quest>.SuccessResponse(updatedQuest, "Quest marked as completed! Ready for confetti!", ResultCode.Success));
        }

        // [Parent Action] - حذف مهمة
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuest(int id)
        {
            await _questRepo.DeleteAsync(id);
            return Ok(ApiResponse<string>.SuccessResponse(null, "Quest deleted successfully", ResultCode.Success));
        }


        // 1. إجمالي رصيد العائلة
        [HttpGet("parent/{parentId}/total-balance")]
        public async Task<IActionResult> GetTotalFamilyBalance(int parentId)
        {
            var totalBalance = await _context.Children
                .Where(c => c.ParentId == parentId)
                .SumAsync(c => c.SavingsBalance);

            return Ok(ApiResponse<decimal>.SuccessResponse(totalBalance, "Total family balance calculated", ResultCode.Success));
        }

        // 2. ما تم كسبه هذا الأسبوع
        [HttpGet("parent/{parentId}/earned-this-week-from-quests")]
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
        [HttpGet("parent/{parentId}/pending-approvals-count")]
        public async Task<IActionResult> GetPendingApprovalsCount(int parentId)
        {
            var count = await _context.Quests
                .CountAsync(q => q.ParentId == parentId && q.Status == QuestStatus.Completed);

            return Ok(ApiResponse<int>.SuccessResponse(count, "Pending approvals count retrieved", ResultCode.Success));
        }

        // 4. عدد الأطفال النشطين حالياً
        [HttpGet("parent/{parentId}/active-children-count")]
        public async Task<IActionResult> GetActiveChildrenCount(int parentId)
        {
            var count = await _context.Quests
                .Where(q => q.ParentId == parentId && q.Status == QuestStatus.Pending)
                .Select(q => q.ChildId)
                .Distinct()
                .CountAsync();

            return Ok(ApiResponse<int>.SuccessResponse(count, "Active children count retrieved", ResultCode.Success));
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
        // DTO (Data Transfer Object) لاستقبال بيانات الطلب بوضوح
        public class QuestRequestDto
        {
            public string? SessionId { get; set; } // اختياري (يمكن أن يكون ID الأبمن قاعدة البيانات)
            public string Message { get; set; } = string.Empty; // إلزامي (مثل: "أعطني مهام لطفلي عمره 8 سنوات")
        }
    }
}
