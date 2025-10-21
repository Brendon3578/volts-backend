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
        public DateTime Date { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? Title { get; set; }
        public string? Notes { get; set; }
        public ShiftStatusEnum Status { get; set; }
        public string GroupId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
