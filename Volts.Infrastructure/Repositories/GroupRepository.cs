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
    public class GroupRepository : Repository<Group>, IGroupRepository
    {
        public GroupRepository(VoltsDbContext context) : base(context) { }

        public async Task<IEnumerable<Group>> GetByOrganizationIdAsync(string organizationId)
        {
            return await _dbSet
                .Where(g => g.OrganizationId == organizationId)
                .ToListAsync();
        }

        public async Task<Group?> GetWithShiftsAsync(string id)
        {
            return await _dbSet
                .Include(g => g.Shifts)
                .FirstOrDefaultAsync(g => g.Id == id);
        }

        public async Task<Group?> GetWithPositionsAsync(string id)
        {
            return await _dbSet
                .Include(g => g.Positions)
                .FirstOrDefaultAsync(g => g.Id == id);
        }

        public async Task<Group?> GetGroupCompleteViewByIdAsync(string id)
        {
            return await _dbSet
                .Include(g => g.Organization)
                .Include(g => g.Shifts)
                .FirstOrDefaultAsync(g => g.Id == id);
        }

        public async Task<IEnumerable<Group>> GetGroupsCompleteViewByOrganizationidAsync(string organizationId)
        {
            return await _dbSet
                .Where(g => g.OrganizationId == organizationId)
                .Include(g => g.Organization)
                .Include(g => g.Shifts)
                .ToListAsync();
        }
    }
}
