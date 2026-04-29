using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Promising_Generation_Bank_API.Data;
using Promising_Generation_Bank_API.Data.Repositories;
using Promising_Generation_Bank_API.Data.Repositories.PromisingGenerationBank.Repositories;
using Promising_Generation_Bank_API.Models;

namespace Promising_Generation_Bank_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChildrenController : ControllerBase
    {
        private readonly ChildRepository _childRepo;
        private readonly TransactionRepository _transactionRepo;
        private readonly AppDbContext _context;

        public ChildrenController(AppDbContext context,ChildRepository childRepo, TransactionRepository transactionRepo)
        {
            _childRepo = childRepo;
            _transactionRepo = transactionRepo;
            _context = context;
        }

        [HttpPost("Add")]
        public async Task<IActionResult> AddChild([FromBody] Child child)
        {
            var newChild = await _childRepo.AddAsync(child);
            return Ok(ApiResponse<Child>.SuccessResponse(newChild, "Child profile created", ResultCode.Created));
        }

        [HttpPut("Update")]
        public async Task<IActionResult> UpdateChild(int id, [FromBody] Child child)
        {
            var updatedChild = await _childRepo.UpdateAsync(id, child);
            return Ok(ApiResponse<Child>.SuccessResponse(updatedChild, "Child profile updated", ResultCode.Success));
        }

        [HttpDelete("Delete")]
        public async Task<IActionResult> DeleteChild(int id)
        {
            await _childRepo.DeleteAsync(id);
            return Ok(ApiResponse<string>.SuccessResponse(null, "Child profile removed", ResultCode.Success));
        }

        [HttpGet("{id}/Get")]
        public async Task<IActionResult> GetChildDashboard(int id)
        {
            var childData = await _childRepo.GetChildDashboardDataAsync(id);
            if (childData == null)
                return NotFound(ApiResponse<Child>.FailureResponse("Child account not found", ResultCode.NotFound));

            return Ok(ApiResponse<Child>.SuccessResponse(childData, "Child dashboard data retrieved successfully", ResultCode.Found));
        }

        [HttpGet("{id}/transactions")]
        public async Task<IActionResult> GetChildTransactions(int id)
        {
            var transactions = await _transactionRepo.GetRecentTransactionsForChildAsync(id);

            return Ok(ApiResponse<IEnumerable<Transaction>>.SuccessResponse(transactions, "Financial record retrieved successfully", ResultCode.Success));
        }

        [HttpGet("Richest")]
        public async Task<IActionResult> GetRichestChild()
        {
            var richestChild = await _childRepo.GetRichestChildAsync();

            if (richestChild == null)
                return NotFound(ApiResponse<Child>.FailureResponse("No children found in the database", ResultCode.NotFound));

            return Ok(ApiResponse<Child>.SuccessResponse(richestChild, "Richest child retrieved successfully", ResultCode.Found));
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

        [HttpGet("GetTotalChildBalanceByChildId")]
        public async Task<IActionResult> GetTotalChildBalance(int childId)
        {
            var totalBalance = await _context.Children
                .Where(c => c.Id == childId)
                .SumAsync(c => c.SavingsBalance);

            return Ok(ApiResponse<decimal>.SuccessResponse(totalBalance, "Total child balance calculated", ResultCode.Success));
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAllChildren(int parentId)
        {
            var children = await _context.Children
                .Where(c => c.ParentId == parentId)
                .ToListAsync();

            return Ok(ApiResponse<List<Child>>.SuccessResponse(children, "Children retrieved successfully", ResultCode.Success));
        }
    }
}

