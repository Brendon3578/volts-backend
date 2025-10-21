using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volts.Domain.Entities;
using Volts.Domain.Enums;
using Volts.Domain.Interfaces;
using Volts.Infrastructure.Data;

namespace Volts.Infrastructure.Repositories
{
    public class ShiftRepository : Repository<Shift>, IShiftRepository
    {
        public ShiftRepository(VoltsDbContext context) : base(context) { }

        public async Task<IEnumerable<Shift>> GetByGroupIdAsync(string groupId)
        {
            return await _dbSet
                .Where(s => s.GroupId == groupId)
                .OrderBy(s => s.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<Shift>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(s => s.Date >= startDate && s.Date <= endDate)
                .OrderBy(s => s.Date)
                .ToListAsync();
        }

        public async Task<IEnumerable<Shift>> GetByStatusAsync(ShiftStatusEnum status)
        {
            return await _dbSet
                .Where(s => s.Status == status)
                .OrderBy(s => s.Date)
                .ToListAsync();
        }

        public async Task<Shift?> GetWithPositionsAsync(string id)
        {
            return await _dbSet
                .Include(s => s.ShiftPositions)
                    .ThenInclude(sp => sp.Position)
                .FirstOrDefaultAsync(s => s.Id == id);
        }
    }
}
