using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volts.Api.Extensions;
using Volts.Application.DTOs.Common;
using Volts.Application.DTOs.Position;
using Volts.Application.Interfaces;

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
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorMessageDto), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<PositionDto>> Create([FromBody] CreatePositionDto dto)
        {
            var userId = User.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ErrorMessageDto { message = "Token inválido" });

            var createdPosition = await _positionService.CreateAsync(dto, userId);
            return Ok(createdPosition);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PositionDto>> GetById(string id)
        {
            var position = await _positionService.GetByIdAsync(id);
            return Ok(position);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorMessageDto), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<PositionDto>> Update(string id, [FromBody] UpdatePositionDto dto)
        {
            var userId = User.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ErrorMessageDto { message = "Token inválido" });

            var updated = await _positionService.UpdateAsync(id, dto, userId);
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorMessageDto), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Delete(string id)
        {
            var userId = User.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ErrorMessageDto { message = "Token inválido" });

            await _positionService.DeleteAsync(id, userId);
            return NoContent();
        }
    }
}
