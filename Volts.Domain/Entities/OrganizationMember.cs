using Volts.Domain.Enums;

namespace Volts.Domain.Entities
{
    public class OrganizationMember : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;
        public string OrganizationId { get; set; } = string.Empty;
        public OrganizationRoleEnum Role { get; set; }
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        public string? InvitedById { get; set; }

        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual Organization Organization { get; set; } = null!;
        public virtual User? InvitedBy { get; set; }
    }
}