using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volts.Api.Extensions;
using Volts.Application.DTOs.Position;
using Volts.Application.Exceptions;
using Volts.Application.Interfaces;
using Volts.Application.Services;
using Volts.Domain.Entities;
using Volts.Domain.Enums;

namespace Volts.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PositionsController : ControllerBase
    {
        private readonly IPositionService _positionService;

        public PositionsController(IPositionService positionService)
        {
            _positionService = positionService;
        }

        [HttpGet("groups/{id}/positions")]
        public async Task<ActionResult<IEnumerable<PositionDto>>> GetByGroupId(string id)
        {
            var positions = await _positionService.GetByGroupIdAsync(id);

            return Ok(positions);

        }

        [HttpPost]
        public async Task<ActionResult<PositionDto>> Create([FromBody] CreatePositionDto dto)
        {
            var userId = User.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Token inválido" });

            // permission check in controller via service helper
            var hasPermission = await _positionService.IsGroupLeaderOrCoordinator(userId, dto.GroupId);
            if (!hasPermission) return Forbid();

            var createdPosition = await _positionService.CreateAsync(dto, userId);

            return Ok(createdPosition);

        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PositionDto>> GetById(string id)
        {
            var pos = await _positionService.GetByIdAsync(id);

            if (pos == null) return NotFound();

            return Ok(pos);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<PositionDto>> Update(string id, [FromBody] UpdatePositionDto dto)
        {
            var userId = User.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Token inválido" });

            // check permission using service helper
            var position = await _positionService.GetByIdAsync(id);

            if (position == null)
                return NotFound();

            var hasPermission = await _positionService.IsGroupLeaderOrCoordinator(userId, position.GroupId);
            if (!hasPermission) return Forbid();



            var updated = await _positionService.UpdateAsync(id, dto, userId);
            return Ok(updated);

        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var userId = User.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Token inválido" });

            var position = await _positionService.GetByIdAsync(id);
            if (position == null) return NotFound();

            var hasPermission = await _positionService.IsGroupLeaderOrCoordinator(userId, position.GroupId);
            if (!hasPermission) return Forbid();


            await _positionService.DeleteAsync(id, userId);
            return NoContent();

        }
    }
}
