using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Volts.Domain.Enums;

namespace Volts.Domain.Entities
{
    public class Shift : BaseEntity
    {
        public DateTime Date { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? Title { get; set; }
        public string? Notes { get; set; }
        public ShiftStatusEnum Status { get; set; } = ShiftStatusEnum.OPEN;
        public string GroupId { get; set; } = string.Empty;

        // Navigation properties
        public virtual Group Group { get; set; } = null!;
        public virtual ICollection<ShiftPosition> ShiftPositions { get; set; } = new List<ShiftPosition>();
    }
}
