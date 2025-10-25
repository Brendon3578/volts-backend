using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Volts.Application.DTOs.Group
{
    public class CreateGroupDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string OrganizationId { get; set; } = string.Empty;
        public string CreatedById { get; set; } = string.Empty;
        public string? Color { get; set; }
        public string? Icon { get; set; }
    }
}
