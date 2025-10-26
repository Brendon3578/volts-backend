using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volts.Domain.Entities;
using Volts.Domain.Enums;

namespace Volts.Domain.Interfaces
{
    public interface IShiftPositionAssignmentRepository : IRepository<ShiftPositionAssignment>
    {
        Task<IEnumerable<ShiftPositionAssignment>> GetByUserIdAsync(string userId);
        Task<IEnumerable<ShiftPositionAssignment>> GetByShiftPositionIdAsync(string shiftPositionId);
        Task<IEnumerable<ShiftPositionAssignment>> GetByStatusAsync(VolunteerStatusEnum status);
        Task<ShiftPositionAssignment?> GetVolunteerApplicationAsync(string userId, string shiftPositionId);
    }
}
