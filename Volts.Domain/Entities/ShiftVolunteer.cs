using Volts.Domain.Enums;

namespace Volts.Domain.Entities
{
    public class ShiftVolunteer : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;
        public string ShiftPositionId { get; set; } = string.Empty;
        public VolunteerStatusEnum Status { get; set; } = VolunteerStatusEnum.PENDING;
        public string? Notes { get; set; }
        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ConfirmedAt { get; set; }
        public DateTime? RejectedAt { get; set; }

        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual ShiftPosition ShiftPosition { get; set; } = null!;
    }
}