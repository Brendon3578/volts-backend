using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Volts.Api.Extensions;
using Volts.Application.DTOs.ShiftPositionAssignment;
using Volts.Application.Interfaces;

namespace Volts.Api.Controllers
{
    [ApiController]
    [Route("api/Shifts")] //OBS: aqui foi definido manualmente /Shifts/ TODO: depois melhorar essa rota
    [Authorize]
    public class ShiftPositionAssignmentController : ControllerBase
    {
        private readonly IShiftPositionAssignmentService _assignmentService;

        public ShiftPositionAssignmentController(IShiftPositionAssignmentService assignmentService)
        {
            _assignmentService = assignmentService;
        }

        [HttpGet("shifts/{id}/assignments")]
        public async Task<IActionResult> GetByShiftId(string id)
        {
            var userId = User.GetUserId();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Token inválido" });

            var assignments = await _assignmentService.GetByShiftIdAsync(id, userId);
            return Ok(assignments);
        }

        [HttpGet("shift-positions/{id}/assignments")]
        public async Task<IActionResult> GetByShiftPositionId(string id)
        {
            var userId = User.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Token inválido" });

            var assignments = await _assignmentService.GetByShiftPositionIdAsync(id, userId);
            return Ok(assignments);
        }

        [HttpGet("assignments/{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var userId = User.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Token inválido" });

            var assignment = await _assignmentService.GetByIdAsync(id, userId);
            return Ok(assignment);
        }

        [HttpPost("shift-positions/{id}/apply")]
        public async Task<IActionResult> Apply(string id, [FromBody] CreateShiftPositionAssignmentDto dto)
        {
            var userId = User.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Token inválido" });

            var assignment = await _assignmentService.ApplyAsync(id, userId, dto);
            return CreatedAtAction(nameof(GetById), new { id = assignment.Id }, assignment);
        }

        [HttpPut("assignments/{id}/confirm")]
        public async Task<IActionResult> Confirm(string id)
        {
            var userId = User.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Token inválido" });

            var assignment = await _assignmentService.ConfirmAsync(id, userId);
            return Ok(assignment);
        }

        [HttpPut("assignments/{id}/cancel")]
        public async Task<IActionResult> Cancel(string id)
        {
            var userId = User.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Token inválido" });

            var assignment = await _assignmentService.CancelAsync(id, userId);
            return Ok(assignment);
        }

        [HttpDelete("assignments/{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var userId = User.GetUserId();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Token inválido" });

            await _assignmentService.DeleteAsync(id, userId);
            return NoContent();
        }
    }
}