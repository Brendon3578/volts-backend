using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volts.Application.DTOs.Shift;
using Volts.Application.Interfaces;
using Volts.Api.Extensions;

namespace Volts.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ShiftsController : ControllerBase
    {
        private readonly IShiftService _shiftService;

        public ShiftsController(IShiftService shiftService)
        {
            _shiftService = shiftService;
        }

        [HttpGet("groups/{id}/shifts")]
        public async Task<ActionResult<IEnumerable<ShiftDto>>> GetByGroupId(string id)
        {
            var shifts = await _shiftService.GetByGroupIdAsync(id);
            return Ok(shifts);
        }

        [HttpPost]
        public async Task<ActionResult<ShiftDto>> Create([FromBody] CreateShiftDto dto)
        {
            var userId = User.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Token inválido" });

            var created = await _shiftService.CreateAsync(dto, userId);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ShiftDto>> GetById(string id)
        {
            var shift = await _shiftService.GetByIdAsync(id);
            return Ok(shift);
        }

        [HttpGet("{id}/completeView")]
        [HttpGet("{id}/complete-view")]
        public async Task<ActionResult<ShiftCompleteViewDto>> GetCompleteView(string id)
        {
            var userId = User.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Token inválido" });

            var view = await _shiftService.GetCompleteViewAsync(id, userId);
            return Ok(view);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ShiftDto>> Update(string id, [FromBody] UpdateShiftDto dto)
        {
            var userId = User.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Token inválido" });

            var updated = await _shiftService.UpdateAsync(id, dto, userId);
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var userId = User.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Token inválido" });

            await _shiftService.DeleteAsync(id, userId);
            return NoContent();
        }
    }
}
