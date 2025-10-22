using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Volts.Api.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string? GetUserId(this ClaimsPrincipal user) =>
            user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        public static string? GetEmail(this ClaimsPrincipal user) =>
            user.FindFirst(ClaimTypes.Email)?.Value;

        public static string? GetUserName(this ClaimsPrincipal user) =>
            user.Identity?.Name;
    }
}
