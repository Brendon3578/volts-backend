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
    public class ShiftVolunteerRepository : Repository<ShiftVolunteer>, IShiftVolunteerRepository
    {
        public ShiftVolunteerRepository(VoltsDbContext context) : base(context) { }

        public async Task<IEnumerable<ShiftVolunteer>> GetByUserIdAsync(string userId)
        {
            return await _dbSet
                .Include(sv => sv.ShiftPosition)
                    .ThenInclude(sp => sp.Shift)
                .Where(sv => sv.UserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<ShiftVolunteer>> GetByShiftPositionIdAsync(string shiftPositionId)
        {
            return await _dbSet
                .Include(sv => sv.User)
                .Where(sv => sv.ShiftPositionId == shiftPositionId)
                .ToListAsync();
        }

        public async Task<IEnumerable<ShiftVolunteer>> GetByStatusAsync(VolunteerStatusEnum status)
        {
            return await _dbSet
                .Include(sv => sv.User)
                .Include(sv => sv.ShiftPosition)
                .Where(sv => sv.Status == status)
                .ToListAsync();
        }

        public async Task<ShiftVolunteer?> GetVolunteerApplicationAsync(string userId, string shiftPositionId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(sv => sv.UserId == userId && sv.ShiftPositionId == shiftPositionId);
        }
    }
}
