using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Volts.Domain.Entities
{
    public class Position : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string GroupId { get; set; } = string.Empty;

        // Navigation properties
        public virtual Group Group { get; set; } = null!;
        public virtual ICollection<ShiftPosition> ShiftPositions { get; set; } = new List<ShiftPosition>();
    }

}
