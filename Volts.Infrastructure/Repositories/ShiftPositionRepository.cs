using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volts.Domain.Entities;
using Volts.Domain.Interfaces;
using Volts.Infrastructure.Data;

namespace Volts.Infrastructure.Repositories
{
    public class ShiftPositionRepository : Repository<ShiftPosition>, IShiftPositionRepository
    {
        public ShiftPositionRepository(VoltsDbContext context) : base(context) { }

        public async Task<IEnumerable<ShiftPosition>> GetByShiftIdAsync(string shiftId)
        {
            return await _dbSet
                .Include(sp => sp.Position)
                .Where(sp => sp.ShiftId == shiftId)
                .ToListAsync();
        }

        public async Task<ShiftPosition?> GetWithVolunteersAsync(string id)
        {
            return await _dbSet
                .Include(sp => sp.Volunteers)
                    .ThenInclude(v => v.User)
                .FirstOrDefaultAsync(sp => sp.Id == id);
        }


    }
}
