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
    public class OrganizationRepository : Repository<Organization>, IOrganizationRepository
    {
        public OrganizationRepository(VoltsDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Organization>> GetByCreatorIdAsync(string creatorId)
        {
            return await _dbSet
                .Where(o => o.CreatedById == creatorId)
                .ToListAsync();
        }

        public async Task<Organization?> GetWithGroupsAsync(string id)
        {
            return await _dbSet
                .Include(o => o.Groups)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<Organization?> GetWithMembersAsync(string id)
        {
            return await _dbSet.Include(o => o.Members)
                .ThenInclude(m => m.User)
                .FirstOrDefaultAsync(o => o.Id == id);
        }
    }
}
