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

        public ParentsController(ParentRepository parentRepo)
        {
            _parentRepo = parentRepo;
        }

        [HttpGet("GetParent")]
        public async Task<IActionResult> GetParentProfile(int id)
        {
            var parent = await _parentRepo.GetByIdAsync(id);
            if (parent == null) return NotFound(ApiResponse<Parent>.FailureResponse("Parent not found", ResultCode.NotFound));

            return Ok(ApiResponse<Parent>.SuccessResponse(parent, "Parent profile retrieved", ResultCode.Found));
        }

        
        [HttpPost("Add")]
        public async Task<IActionResult> CreateParent([FromBody] Parent parent)
        {
            var newParent = await _parentRepo.AddAsync(parent);
            return Ok(ApiResponse<Parent>.SuccessResponse(newParent, "Family account created", ResultCode.Created));
        }

        [HttpPut("Update")]
        public async Task<IActionResult> UpdateParent([FromBody] Parent parent)
        {
            var updated = await _parentRepo.UpdateAsync(parent.Id, parent);
            return Ok(ApiResponse<Parent>.SuccessResponse(updated, "Profile updated successfully", ResultCode.Success));
        }
    }
}