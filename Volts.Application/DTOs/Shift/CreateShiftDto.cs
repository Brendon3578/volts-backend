using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Volts.Application.DTOs.Shift
{
    public class CreateShiftDto
    {
        public DateTime Date { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? Title { get; set; }
        public string? Notes { get; set; }
        public string GroupId { get; set; } = string.Empty;
        public List<CreateShiftPositionDto> Positions { get; set; } = new List<CreateShiftPositionDto>();
    }
}
