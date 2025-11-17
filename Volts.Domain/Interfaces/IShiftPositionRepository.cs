using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volts.Domain.Entities;

namespace Volts.Domain.Interfaces
{
    public interface IShiftPositionRepository : IRepository<ShiftPosition>
    {
        Task<IEnumerable<ShiftPosition>> GetByShiftIdAsync(string shiftId);
        Task<ShiftPosition?> GetWithVolunteersAsync(string id);
        Task<bool> ExistsForPositionAsync(string positionId);
    }
}
