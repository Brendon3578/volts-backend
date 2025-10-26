using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrganizationDto>>> GetAll()
        {
            var organizations = await _organizationService.GetAllOrganizationsAsync();
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
            var organization = await _organizationService.GetOrganizationByIdAsync(id);
            var groups = await _groupService.GetAllByOrganizationIdAsync(organization.Id);
            return Ok(groups);
        }



        [HttpGet("creator/{creatorId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<OrganizationDto>>> GetByCreator(string creatorId)
        {
            var organizations = await _organizationService.GetOrganizationsByCreatorAsync(creatorId);
            return Ok(organizations);
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<OrganizationDto>>> GetOrganizationsForCurrentUserAsync()
        {
            var userId = User.GetUserId();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Token inválido" });

            var orgs = await _organizationService.GetOrganizationsForCurrentUserAsync(userId);

            return Ok(orgs);


        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<OrganizationDto>> Create([FromBody] CreateOrganizationDto dto)
        {
            var userId = User.GetUserId();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Token inválido" });


            var organization = await _organizationService.CreateOrganizationAsync(dto, userId);

            return Ok(organization);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<OrganizationDto>> Update(string id, [FromBody] UpdateOrganizationDto dto)
        {
            var userId = User.GetUserId();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Token inválido" });

            var organization = await _organizationService.UpdateOrganizationAsync(id, dto, userId);
            return Ok(organization);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(string id)
        {
            var userId = User.GetUserId();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Token inválido" });

            await _organizationService.DeleteOrganizationAsync(id, userId);
            return NoContent();
        }

        [HttpPost("{id}/join")]
        [Authorize]
        public async Task<IActionResult> Join(string id)
        {
            var userId = User.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Token inválido" });

            await _organizationService.JoinAsync(id, userId);
            return Ok();
        }

        [HttpPost("{id}/leave")]
        [Authorize]
        public async Task<IActionResult> Leave(string id)
        {
            var userId = User.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Token inválido" });

            await _organizationService.LeaveAsync(id, userId);
            return Ok();
        }


    }
}
