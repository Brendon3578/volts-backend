using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Volts.Api.Attributes;
using Volts.Api.Extensions;
using Volts.Application.DTOs.Group;
using Volts.Application.DTOs.Organization;
using Volts.Application.Interfaces;
using Volts.Domain.Enums;

namespace Volts.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrganizationsController : ControllerBase
    {
        private readonly IOrganizationService _organizationService;
        private readonly IGroupService _groupService;

        public OrganizationsController(IOrganizationService organizationService, IGroupService groupService)
        {
            _organizationService = organizationService;
            _groupService = groupService;
        }

        [HttpPost("{id}/invite-member")]
        [NotLoggedAuthorize]
        public async Task<ActionResult<OrganizationMemberDto>> InviteMember(string id, [FromBody] InviteUserOrganizationDto dto)
        {
            var userId = User.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Token inválido" });

            var result = await _organizationService.InviteMemberAsync(id, dto, userId);
            return Ok(result);
        }

        [HttpGet("{id}/completeView")]
        [NotLoggedAuthorize]
        public async Task<ActionResult<OrganizationCompleteViewDto>> GetCompleteViewById(string id)
        {
            var userId = User.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Token inválido" });

            var org = await _organizationService.GetOrganizationCompleteViewByIdAsync(id, userId);
            if (org == null) return NotFound();
            return Ok(org);
        }

        [HttpGet("completeView")]
        [NotLoggedAuthorize]
        public async Task<ActionResult<IEnumerable<OrganizationCompleteViewDto>>> GetCompleteViewList()
        {
            var userId = User.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Token inválido" });

            var orgs = await _organizationService.GetOrganizationsCompleteViewAsync(userId);
            return Ok(orgs);
        }

        [HttpGet("{organizationId}/members")]
        [NotLoggedAuthorize]
        public async Task<ActionResult<IEnumerable<OrganizationMemberDto>>> GetMembers(string organizationId)
        {
            var userId = User.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Token inválido" });

            var members = await _organizationService.GetOrganizationMembersAsync(organizationId);
            return Ok(members);
        }

        [HttpPut("{organizationId}/members/{memberId}/role")]
        [NotLoggedAuthorize]
        public async Task<IActionResult> ChangeMemberRole(string organizationId, string memberId, [FromBody] ChangeOrganizationMemberRoleDto body)
        {
            var userId = User.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Token inválido" });

            await _organizationService.ChangeOrganizationMemberRoleAsync(organizationId, memberId, body.Role, userId);
            return Ok();
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrganizationDto>>> GetAll()
        {
            var organizations = await _organizationService.GetAllOrganizationsAsync();
            return Ok(organizations);
        }

        [HttpGet("available")]
        [NotLoggedAuthorize]
        public async Task<ActionResult<IEnumerable<OrganizationDto>>> GetAllAvailable()
        {
            var userId = User.GetUserId();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Token inválido" });

            var organizations = await _organizationService.GetAllOrganizationsAvailableAsync(userId);
            return Ok(organizations);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OrganizationDto>> GetById(string id)
        {
            var organization = await _organizationService.GetOrganizationByIdAsync(id);
            return Ok(organization);
        }

        [HttpGet("{id}/groups")]
        public async Task<ActionResult<IEnumerable<GroupDto>>> GetGroupsByOrganizationId(string id)
        {
            var groups = await _groupService.GetAllByOrganizationIdAsync(id);
            return Ok(groups);
        }


        [HttpGet("{organizationId}/Groups/completeView")]
        [NotLoggedAuthorize]
        public async Task<ActionResult<GroupCompleteViewDto>> GetGroupCompleteViewByOrganizationId(string organizationId)
        {
            var userId = User.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Token inválido" });

            var groups = await _groupService.GetGroupsCompleteViewByOrganizationidAsync(organizationId, userId);
            return Ok(groups);
        }



        [HttpGet("creator/{creatorId}")]
        [NotLoggedAuthorize]
        public async Task<ActionResult<IEnumerable<OrganizationDto>>> GetByCreator(string creatorId)
        {
            var organizations = await _organizationService.GetOrganizationsByCreatorAsync(creatorId);
            return Ok(organizations);
        }

        [HttpGet("me")]
        [NotLoggedAuthorize]
        public async Task<ActionResult<IEnumerable<OrganizationDto>>> GetOrganizationsForCurrentUserAsync()
        {
            var userId = User.GetUserId();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Token inválido" });

            var orgs = await _organizationService.GetOrganizationsForCurrentUserAsync(userId);

            return Ok(orgs);


        }

        [HttpPost]
        [NotLoggedAuthorize]
        public async Task<ActionResult<OrganizationDto>> Create([FromBody] CreateOrganizationDto dto)
        {
            var userId = User.GetUserId();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Token inválido" });


            var organization = await _organizationService.CreateOrganizationAsync(dto, userId);

            return Ok(organization);
        }

        [HttpPut("{id}")]
        [NotLoggedAuthorize]
        public async Task<ActionResult<OrganizationDto>> Update(string id, [FromBody] UpdateOrganizationDto dto)
        {
            var userId = User.GetUserId();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Token inválido" });

            var organization = await _organizationService.UpdateOrganizationAsync(id, dto, userId);
            return Ok(organization);
        }

        [HttpDelete("{id}")]
        [NotLoggedAuthorize]
        public async Task<IActionResult> Delete(string id)
        {
            var userId = User.GetUserId();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Token inválido" });

            await _organizationService.DeleteOrganizationAsync(id, userId);
            return NoContent();
        }

        [HttpPost("{id}/join")]
        [NotLoggedAuthorize]
        public async Task<IActionResult> Join(string id)
        {
            var userId = User.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Token inválido" });

            await _organizationService.JoinAsync(id, userId);
            return Ok();
        }

        [HttpPost("{id}/leave")]
        [NotLoggedAuthorize]
        public async Task<IActionResult> Leave(string id)
        {
            var userId = User.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Token inválido" });

            await _organizationService.LeaveAsync(id, userId);
            return Ok();
        }

        // TODO: validar de quando deletar único admin, deletar a organização
        [HttpDelete("{organizationId}/members/{memberId}")]
        [NotLoggedAuthorize]
        public async Task<IActionResult> RemoveMember(string organizationId, string memberId)
        {
            var userId = User.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Token inválido" });

            await _organizationService.RemoveMemberAsync(organizationId, memberId, userId);
            return NoContent();
        }

        // TODO: fazer controller de organization member, obs: ao deletar um membro deve deletar o groupmember dele também
    }
}
