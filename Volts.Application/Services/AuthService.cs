using BCrypt.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Volts.Application.DTOs.Authentication;
using Volts.Application.DTOs.User;
using Volts.Application.Exceptions;
using Volts.Application.Interfaces;
using Volts.Domain.Entities;
using Volts.Domain.Interfaces;

namespace Volts.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;

        public AuthService(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterUserDto registerDto)
        {
           var existsUserWithSameEmail = await _unitOfWork.Users.EmailExistsAsync(registerDto.Email);
            if (existsUserWithSameEmail)
            {
                throw new InvalidOperationException("Email já cadastrado!");
            }

            var isPasswordConfirmed = registerDto.Password == registerDto.ConfirmPassword;

            if (isPasswordConfirmed == false)
            {
                throw new InvalidOperationException("Senhas não conferem");
            }

            if (registerDto.AcceptedTerms == false)
            {
                throw new InvalidOperationException("Termos nao aceitos");
            }



            var hashedPassword = HashPassword(registerDto.Password);

            var user = new User
            {
                Name = registerDto.Name,
                Email = registerDto.Email,
                Password = hashedPassword,
                Birthdate = registerDto.Birthdate,
                Gender = registerDto.Gender,
            };

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            var token = GenerateJwtToken(user);
            var expiresAt = DateTime.UtcNow.AddHours(
                double.Parse(_configuration["Jwt:ExpirationHours"] ?? "24")
            );

            return new AuthResponseDto
            {
                Token = token,
                ExpiresAt = expiresAt,
                User = MapToUserDto(user)
            };

        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            // Buscar usuário por email
            var user = await _unitOfWork.Users.GetByEmailAsync(loginDto.Email);

            if (user == null || VerifyPassword(loginDto.Password, user.Password) == false)
            {
                throw new UserHasNotPermissionException("Email ou senha inválidos");
            }

            // Gerar token
            var token = GenerateJwtToken(user);
            var expiresAt = DateTime.UtcNow.AddHours(
                double.Parse(_configuration["Jwt:ExpirationHours"] ?? "24")
            );

            return new AuthResponseDto
            {
                Token = token,
                ExpiresAt = expiresAt,
                User = MapToUserDto(user)
            };
        }




        public string HashPassword(string password)
        {
            // Usando BCrypt com work factor 12 (2^12 = 4096 iterações)
            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
        }

        public bool VerifyPassword(string password, string passwordHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }

        private string GenerateJwtToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"] ??
                    throw new InvalidOperationException("JWT SecretKey não configurada"))
            );

            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Name, user.Name),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(
                    double.Parse(_configuration["Jwt:ExpirationHours"] ?? "24")
                ),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static UserDto MapToUserDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                CreatedAt = user.CreatedAt
            };
        }
    }
}
