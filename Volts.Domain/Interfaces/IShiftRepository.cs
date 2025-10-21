using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volts.Domain.Entities;
using Volts.Domain.Enums;

namespace Volts.Domain.Interfaces
{
    public interface IShiftRepository : IRepository<Shift>
    {
        Task<IEnumerable<Shift>> GetByGroupIdAsync(string groupId);
        Task<IEnumerable<Shift>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Shift>> GetByStatusAsync(ShiftStatusEnum status);
        Task<Shift?> GetWithPositionsAsync(string id);
    }
}
