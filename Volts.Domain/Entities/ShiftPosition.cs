namespace Volts.Domain.Entities
{
    public class ShiftPosition : BaseEntity
    {
        public string ShiftId { get; set; } = string.Empty;
        public string PositionId { get; set; } = string.Empty;
        public int RequiredCount { get; set; } = 1;
        public int VolunteersCount { get; set; } = 0;

        // Navigation properties
        public virtual Shift Shift { get; set; } = null!;
        public virtual Position Position { get; set; } = null!;
        public virtual ICollection<ShiftPositionAssignment> Volunteers { get; set; } = new List<ShiftPositionAssignment>();
    }
}