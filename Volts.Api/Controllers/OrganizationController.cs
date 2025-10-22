using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Volts.Api.Extensions;
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

        public OrganizationsController(IOrganizationService organizationService)
        {
            _organizationService = organizationService;
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
            if (organization == null)
                return NotFound();

            return Ok(organization);
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

            if (await IsLeaderOrAdmin(userId, id) == false)
                return Forbid("Você não tem permissão para deletar esta organização.");


            try
            {
                var organization = await _organizationService.UpdateOrganizationAsync(id, dto);
                return Ok(organization);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(string id)
        {
            var userId = User.GetUserId();

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Token inválido" });

            if (await IsLeaderOrAdmin(userId, id) == false)
                return Forbid("Você não tem permissão para deletar esta organização.");

            await _organizationService.DeleteOrganizationAsync(id);
            return NoContent();
        }

        private async Task<bool> IsLeaderOrAdmin(string userId, string organizationId)
        {
            var hasPermission = await _organizationService.UserHasPermissionAsync(userId, organizationId,
            [
                OrganizationRoleEnum.LEADER,
                OrganizationRoleEnum.ADMIN
            ]);

            return hasPermission;
        }
    }
}
