using System.Text.RegularExpressions;

namespace Volts.Domain.Entities
{
    public class User : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? Bio { get; set; }
        public string Password { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public DateTime Birthdate { get; set; }


        // Navigation properties
        public virtual ICollection<Organization> OrganizationsCreated { get; set; } = new List<Organization>();
        public virtual ICollection<OrganizationMember> OrganizationMemberships { get; set; } = new List<OrganizationMember>();
        public virtual ICollection<Group> GroupsCreated { get; set; } = new List<Group>();
        public virtual ICollection<GroupMember> GroupMemberships { get; set; } = new List<GroupMember>();
        public virtual ICollection<GroupMember> GroupMembershipsAdded { get; set; } = new List<GroupMember>();
        public virtual ICollection<ShiftPositionAssignment> ShiftPositionAssignment { get; set; } = new List<ShiftPositionAssignment>();
        public virtual ICollection<OrganizationMember> OrganizationInvites { get; set; } = new List<OrganizationMember>();
    }
}