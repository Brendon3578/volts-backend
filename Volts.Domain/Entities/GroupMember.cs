using Volts.Domain.Enums;

namespace Volts.Domain.Entities
{
    public class GroupMember : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;
        public string GroupId { get; set; } = string.Empty;
        public GroupRoleEnum Role { get; set; }
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        public string? AddedById { get; set; }

        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual Group Group { get; set; } = null!;
        public virtual User? AddedBy { get; set; }
    }
}