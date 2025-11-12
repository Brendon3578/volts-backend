using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Volts.Application.DTOs.Group
{
    public class UpdateGroupDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Color { get; set; }
        public string? Icon { get; set; }
        public string? OrganizationId { get; set; } = string.Empty; // não vai mudar aqui, deixei apenas para validação no front

    }
}
