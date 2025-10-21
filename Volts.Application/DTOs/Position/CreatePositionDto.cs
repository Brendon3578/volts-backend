using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Volts.Application.DTOs.Position
{
    public class CreatePositionDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string GroupId { get; set; } = string.Empty;
    }
}
