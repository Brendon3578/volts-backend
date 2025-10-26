using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volts.Domain.Enums;

namespace Volts.Application.DTOs.Shift
{
    public class UpdateShiftDto
    {
        public DateTime? Date { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string? Title { get; set; }
        public string? Notes { get; set; }
        public ShiftStatusEnum? Status { get; set; }
        public List<CreateShiftPositionDto>? Positions { get; set; }
    }
}
