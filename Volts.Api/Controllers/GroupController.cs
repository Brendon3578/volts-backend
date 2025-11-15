using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volts.Application.DTOs.Group;
using Volts.Application.DTOs.Position;
using Volts.Application.Interfaces;
using Volts.Api.Extensions;
using Volts.Api.Attributes;

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
        public async Task<ActionResult<GroupCompleteViewDto>> GetCompleteViewById(string id)
        {
            var userId = User.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Token inválido" });

            var group = await _groupService.GetGroupCompleteViewByIdAsync(id, userId);
            if (group == null) return NotFound();
            return Ok(group);
        }

        [HttpPost]
        public async Task<ActionResult<GroupDto>> Create([FromBody] CreateGroupDto dto)
        {
            var userId = User.GetUserId();
            if (string.IsNullOrEmpty(userId)) 
                return Unauthorized(new { message = "Token inválido" });

            var created = await _groupService.CreateAsync(dto, userId);

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<GroupDto>> Update(string id, [FromBody] UpdateGroupDto dto)
        {
            var userId = User.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Token inválido" });

            var updated = await _groupService.UpdateAsync(id, dto, userId);
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var userId = User.GetUserId();
            if (string.IsNullOrEmpty(userId)) 
                return Unauthorized(new { message = "Token inválido" });

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
