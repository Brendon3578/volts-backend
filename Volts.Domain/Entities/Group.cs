namespace Volts.Domain.Entities
{
    public class Group : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string OrganizationId { get; set; } = string.Empty;
        public string CreatedById { get; set; } = string.Empty;
        public string? Color { get; set; }
        public string? ImageUrl { get; set; }
        public string? Icon { get; set; }

        // Navigation properties
        public virtual Organization Organization { get; set; } = null!;
        public virtual User CreatedBy { get; set; } = null!;
        public virtual ICollection<GroupMember> Members { get; set; } = new List<GroupMember>();
        public virtual ICollection<Position> Positions { get; set; } = new List<Position>();
        public virtual ICollection<Shift> Shifts { get; set; } = new List<Shift>();
    }
}