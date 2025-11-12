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

        public async Task<IEnumerable<Shift>> GetByGroupIdWithPositionsAsync(string groupId)
        {
            return await _dbSet
                .Include(s => s.ShiftPositions)
                    .ThenInclude(sp => sp.Position)
                .Where(s => s.GroupId == groupId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Shift>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(s => s.StartDate >= startDate && s.StartDate <= endDate)
                .OrderBy(s => s.StartDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Shift>> GetByStatusAsync(ShiftStatusEnum status)
        {
            return await _dbSet
                .Where(s => s.Status == status)
                .OrderBy(s => s.StartDate)
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
