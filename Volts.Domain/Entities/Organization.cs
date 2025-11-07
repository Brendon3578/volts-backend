using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Volts.Domain.Entities
{
    public class Organization : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }

        public string? Color { get; set; }
        public string? ImageUrl { get; set; }
        public string? Icon { get; set; }

        public string CreatedById { get; set; } = string.Empty;

        // Navigation properties
        public virtual User CreatedBy { get; set; } = null!;
        public virtual ICollection<OrganizationMember> Members { get; set; } = new List<OrganizationMember>();
        public virtual ICollection<Group> Groups { get; set; } = new List<Group>();
    }
}
