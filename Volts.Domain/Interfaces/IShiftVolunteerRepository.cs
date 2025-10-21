using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volts.Domain.Entities;
using Volts.Domain.Enums;

namespace Volts.Domain.Interfaces
{
    public interface IShiftVolunteerRepository : IRepository<ShiftVolunteer>
    {
        Task<IEnumerable<ShiftVolunteer>> GetByUserIdAsync(string userId);
        Task<IEnumerable<ShiftVolunteer>> GetByShiftPositionIdAsync(string shiftPositionId);
        Task<IEnumerable<ShiftVolunteer>> GetByStatusAsync(VolunteerStatusEnum status);
        Task<ShiftVolunteer?> GetVolunteerApplicationAsync(string userId, string shiftPositionId);
    }
}
