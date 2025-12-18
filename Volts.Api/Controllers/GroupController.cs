using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volts.Application.DTOs.Group;
using Volts.Application.DTOs.Position;
using Volts.Application.Interfaces;
using Volts.Api.Extensions;
using Volts.Api.Attributes;
using Volts.Application.DTOs.Common;
using Microsoft.AspNetCore.Http;

namespace Volts.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class GroupsController : ControllerBase
    {
        private readonly IGroupService _groupService;

        public GroupsController(IGroupService groupService)
        {
            _groupService = groupService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<GroupDto>>> GetAll()
        {
            var groups = await _groupService.GetAllAsync();
            return Ok(groups);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<GroupDto>> GetById(string id)
        {

            var group = await _groupService.GetByIdAsync(id);

            return Ok(group);
        }

        [HttpGet("{id}/completeView")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorMessageDto), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GroupCompleteViewDto>> GetCompleteViewById(string id)
        {
            var userId = User.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ErrorMessageDto { message = "Token inválido" });

            var group = await _groupService.GetGroupCompleteViewByIdAsync(id, userId);
            if (group == null) return NotFound();
            return Ok(group);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorMessageDto), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<GroupDto>> Create([FromBody] CreateGroupDto dto)
        {
            var userId = User.GetUserId();
            if (string.IsNullOrEmpty(userId)) 
                return Unauthorized(new ErrorMessageDto { message = "Token inválido" });

            var created = await _groupService.CreateAsync(dto, userId);

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorMessageDto), StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<GroupDto>> Update(string id, [FromBody] UpdateGroupDto dto)
        {
            var userId = User.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ErrorMessageDto { message = "Token inválido" });

            var updated = await _groupService.UpdateAsync(id, dto, userId);
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

            await _groupService.DeleteAsync(id, userId);
            return NoContent();
        }

        [HttpGet("{id}/positions")]
        public async Task<ActionResult<IEnumerable<PositionDto>>> GetPositions(string id)
        {
            var positions = await _groupService.GetPositionsAsync(id);
            return Ok(positions);
        }
    }
}
