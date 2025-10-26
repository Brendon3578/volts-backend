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
    public class ShiftPositionAssignmentRepository : Repository<ShiftPositionAssignment>, IShiftPositionAssignmentRepository
    {
        public ShiftPositionAssignmentRepository(VoltsDbContext context) : base(context) { }

        public async Task<IEnumerable<ShiftPositionAssignment>> GetByUserIdAsync(string userId)
        {
            return await _dbSet
                .Include(sv => sv.ShiftPosition)
                    .ThenInclude(sp => sp.Shift)
                .Where(sv => sv.UserId == userId)
                .ToListAsync();
        }

        public async Task<IEnumerable<ShiftPositionAssignment>> GetByShiftPositionIdAsync(string shiftPositionId)
        {
            return await _dbSet
                .Include(sv => sv.User)
                .Where(sv => sv.ShiftPositionId == shiftPositionId)
                .ToListAsync();
        }

        public async Task<IEnumerable<ShiftPositionAssignment>> GetByStatusAsync(VolunteerStatusEnum status)
        {
            return await _dbSet
                .Include(sv => sv.User)
                .Include(sv => sv.ShiftPosition)
                .Where(sv => sv.Status == status)
                .ToListAsync();
        }

        public async Task<ShiftPositionAssignment?> GetVolunteerApplicationAsync(string userId, string shiftPositionId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(sv => sv.UserId == userId && sv.ShiftPositionId == shiftPositionId);
        }
    }
}
