using Microsoft.AspNetCore.Mvc;
using Volts.Application.DTOs.Authentication;
using Volts.Application.Interfaces;

namespace Volts.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        /// <summary>
        /// Cadastra um novo usuário
        /// </summary>
        [HttpPost("register")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto registerDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        message = "Dados inválidos",
                        errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                    });
                }

                var response = await _authService.RegisterAsync(registerDto);

                return CreatedAtAction(
                    nameof(UserController.GetMe),
                    "User",
                    null,
                    new
                    {
                        message = "Usuário cadastrado com sucesso",
                        data = response
                    }
                );
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, $"Erro ao fazer o login: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao cadastrar usuário");
                return StatusCode(500, new { message = "Erro interno do servidor" });
            }
        }

        /// <summary>
        /// Autentica usuário e retorna token JWT
        /// </summary>
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        message = "Dados inválidos",
                        errors = ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage))
                    });
                }

                var response = await _authService.LoginAsync(loginDto);

                return Ok(new
                {
                    message = "Login realizado com sucesso",
                    data = response
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Tentativa de login falhou para: {Email}", loginDto.Email);
                return BadRequest(new { message = "Erro ao realizar Login!" });
            }

        }
    }
}
