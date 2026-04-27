using Microsoft.AspNetCore.Mvc;
using Promising_Generation_Bank_API.Data.Repositories.PromisingGenerationBank.Repositories;
using Promising_Generation_Bank_API.Models;
using Promising_Generation_Bank_API.FinGuardAI.API.Utilities;

namespace Promising_Generation_Bank_API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ParentsController : ControllerBase
    {
        private readonly ParentRepository _parentRepo;
        private readonly ChildRepository _childRepo;

        public ParentsController(ParentRepository parentRepo, ChildRepository childRepo)
        {
            _parentRepo = parentRepo;
            _childRepo = childRepo;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetParentProfile(int id)
        {
            var parent = await _parentRepo.GetParentWithChildrenAsync(id);
            if (parent == null)
                return NotFound(ApiResponse<Parent>.FailureResponse("Parent account not found", ResultCode.NotFound));

            return Ok(ApiResponse<Parent>.SuccessResponse(parent, "Parent data retrieved successfully", ResultCode.Found));
        }

        [HttpPost]
        public async Task<IActionResult> CreateParent([FromBody] Parent parent)
        {
            parent.TotalFamilyBalance = 0;
            parent.EarnedThisWeek = 0;

            await _parentRepo.AddAsync(parent);
            await _parentRepo.SaveChangesAsync();

            return Ok(ApiResponse<Parent>.SuccessResponse(parent, "Parent account created successfully", ResultCode.Created));
        }

        [HttpPost("{parentId}/children")]
        public async Task<IActionResult> AddChild(int parentId, [FromBody] Child child)
        {
            var parent = await _parentRepo.GetByIdAsync(parentId);
            if (parent == null)
                return NotFound(ApiResponse<Child>.FailureResponse("Parent account not found", ResultCode.NotFound));

            child.ParentId = parentId;
            child.SavingsBalance = 0;

            await _childRepo.AddAsync(child);

            parent.ActiveChildren += 1;
            _parentRepo.Update(parent);

            await _childRepo.SaveChangesAsync();

            return Ok(ApiResponse<Child>.SuccessResponse(child, "Child added successfully", ResultCode.Created));
        }
    }
}
