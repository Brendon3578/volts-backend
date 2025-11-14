using Volts.Domain.Enums;

namespace Volts.Application.DTOs.Organization
{
    public class InviteUserOrganizationDto
    {
        public string InvitedEmail { get; set; } = string.Empty;
        public string InviterRole { get; set; } = string.Empty;
    }
}
