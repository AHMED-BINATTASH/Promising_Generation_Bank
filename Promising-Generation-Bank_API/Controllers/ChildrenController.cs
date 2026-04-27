using Microsoft.AspNetCore.Mvc;
using Promising_Generation_Bank_API.Data.Repositories;
using Promising_Generation_Bank_API.Data.Repositories.PromisingGenerationBank.Repositories;
using Promising_Generation_Bank_API.FinGuardAI.API.Utilities;
using Promising_Generation_Bank_API.Models;

namespace Promising_Generation_Bank_API.Controllers
{


    [ApiController]
    [Route("api/[controller]")]
    public class ChildrenController : ControllerBase
    {
        private readonly ChildRepository _childRepo;
        private readonly TransactionRepository _transactionRepo;

        public ChildrenController(ChildRepository childRepo, TransactionRepository transactionRepo)
        {
            _childRepo = childRepo;
            _transactionRepo = transactionRepo;
        }

        [HttpGet("{id}/dashboard")]
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
    }
}

