using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Volts.Api.Attributes;
using Volts.Api.Extensions;
using Volts.Application.DTOs.Common;
using Volts.Application.DTOs.Group;
using Volts.Application.DTOs.User;
using Volts.Application.Interfaces;
using Volts.Domain.Entities;

namespace Volts.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [NotLoggedAuthorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UserController> _logger;

        public UserController(IUserService userService, ILogger<UserController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        /// <summary>
        /// Retorna dados do usuário autenticado
        /// </summary>
        [HttpGet("me")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserDto>> GetMe()
        {
            // Obter ID do usuário do token JWT
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new ErrorMessageDto { message = "Token inválido" });
            }

            var user = await _userService.GetUserByIdAsync(userId.ToString());

            if (user == null)
            {
                return NotFound(new ErrorMessageDto { message = "Usuário não encontrado" });
            }

            return Ok(user);
        }

        /// <summary>
        /// Retorna todas as organizações e grupos do usuário autenticado
        /// </summary>
        [HttpGet("organizations")]
        [ProducesResponseType(typeof(List<UserOrganizationWithGroupsDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<UserOrganizationWithGroupsDto>>> GetUserOrganizationsAndGroups()
        {
            var userId = User.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ErrorMessageDto { message = "Token inválido" });

            var result = await _userService.GetUserOrganizationsAndGroupsAsync(userId);
            return Ok(result);
        }

        /// <summary>
        /// Atualiza o perfil do usuário autenticado
        /// </summary>
        [HttpPut("me")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<UserDto>> UpdateMe([FromBody] UpdateUserProfileDto dto)
        {
            var userId = User.GetUserId();
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ErrorMessageDto { message = "Token inválido" });

            var updated = await _userService.UpdateUserProfileAsync(userId, dto);
            return Ok(updated);
        }
    }
}
