using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volts.Application.DTOs.Shift;
using Volts.Application.Interfaces;
using Volts.Api.Extensions;
using Volts.Application.Exceptions;
using Volts.Domain.Enums;

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
            try
            {
                var shifts = await _shiftService.GetByGroupIdAsync(id);
                return Ok(shifts);
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost]
        public async Task<ActionResult<ShiftDto>> Create([FromBody] CreateShiftDto dto)
        {
            var userId = User.GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized(new { message = "Token inválido" });

            var allowed = new[] { GroupRoleEnum.GROUP_LEADER, GroupRoleEnum.COORDINATOR };
            var hasPermission = await _shiftService.UserHasPermissionAsync(userId, dto.GroupId, allowed);
            if (!hasPermission) return Forbid();

            try
            {
                var created = await _shiftService.CreateAsync(dto, userId);
                return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ShiftDto>> GetById(string id)
        {
            var shift = await _shiftService.GetByIdAsync(id);
            if (shift == null) return NotFound();
            return Ok(shift);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ShiftDto>> Update(string id, [FromBody] UpdateShiftDto dto)
        {
            var userId = User.GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized(new { message = "Token inválido" });

            var shift = await _shiftService.GetByIdAsync(id);
            if (shift == null) return NotFound();

            var allowed = new[] { GroupRoleEnum.GROUP_LEADER, GroupRoleEnum.COORDINATOR };
            var hasPermission = await _shiftService.UserHasPermissionAsync(userId, shift.GroupId, allowed);
            if (!hasPermission) return Forbid();

            try
            {
                var updated = await _shiftService.UpdateAsync(id, dto, userId);
                return Ok(updated);
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
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
