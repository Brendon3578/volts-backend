using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volts.Domain.Enums;

namespace Volts.Application.DTOs.Shift
{
    public class ShiftDto
    {
        public string Id { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Title { get; set; }
        public string? Notes { get; set; }
        public string Status { get; set; } = string.Empty;
        public string GroupId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<ShiftPositionDto> Positions { get; set; } = new List<ShiftPositionDto>();
    }
}
