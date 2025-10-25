using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Volts.Application.DTOs.Group;
using Volts.Application.Interfaces;
using Volts.Api.Extensions;
using Volts.Domain.Enums;
using Volts.Application.DTOs.Position;

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
            if (group == null) return NotFound();
            return Ok(group);
        }

        [HttpPost]
        public async Task<ActionResult<GroupDto>> Create([FromBody] CreateGroupDto dto)
        {
            var userId = User.GetUserId();

            if (string.IsNullOrEmpty(userId)) return Unauthorized(new { message = "Token inválido" });

            var created = await _groupService.CreateAsync(dto, userId);

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<GroupDto>> Update(string id, [FromBody] UpdateGroupDto dto)
        {
            var userId = User.GetUserId();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Token inválido" });

            var hasPermission = await IsCoordinatorOrGroupLeader(userId, id);

            if (hasPermission == false)
                return Forbid("Você não tem permissão para atualizar esse grupo");


            var updated = await _groupService.UpdateAsync(id, dto, userId);

            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var userId = User.GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized(new { message = "Token inválido" });


            var hasPermission = await IsCoordinatorOrGroupLeader(userId, id);

            if (hasPermission == false)
                return Forbid("Você não tem permissão para apagar esse grupo");


            try
            {
                await _groupService.DeleteAsync(id, userId);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpGet("{id}/members")]
        public async Task<ActionResult<IEnumerable<GroupMemberDto>>> GetMembers(string id)
        {
            var members = await _groupService.GetMembersAsync(id);

            return Ok(members);
        }

        [HttpPost("{id}/join")]
        public async Task<IActionResult> Join(string id)
        {
            var userId = User.GetUserId();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Token inválido" });

            await _groupService.JoinAsync(id, userId);

            return Ok();
        }

        [HttpPost("{id}/invite")]
        public async Task<IActionResult> InviteUser(string id, [FromBody] InviteUserGroupDto inviteDto)
        {
            var userId = User.GetUserId();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Token inválido" });

            await _groupService.InviteUserAsync(id, userId, inviteDto);

            return Ok();
        }


        [HttpPost("{id}/leave")]
        public async Task<IActionResult> Leave(string id)
        {
            var userId = User.GetUserId();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Token inválido" });

            await _groupService.LeaveAsync(id, userId);

            return Ok();
        }

        [HttpGet("{id}/positions")]
        public async Task<ActionResult<IEnumerable<PositionDto>>> GetPositions(string id)
        {
            var positions = await _groupService.GetPositionsAsync(id);
            return Ok(positions);
        }

        private async Task<bool> IsCoordinatorOrGroupLeader(string userId, string groupId)
        {
            var hasPermission = await _groupService.UserGroupHasPermissionAsync(userId, groupId, [
                GroupRoleEnum.GROUP_LEADER,
                GroupRoleEnum.COORDINATOR,
            ]);

            return hasPermission;
        }
    }
}
