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
    public class OrganizationMemberRepository : Repository<OrganizationMember>, IOrganizationMemberRepository
    {
        public OrganizationMemberRepository(VoltsDbContext context) : base(context) { }

        public async Task<IEnumerable<OrganizationMember>> GetByOrganizationIdAsync(string organizationId)
        {
            return await _dbSet
                .Include(om => om.User)
                .Where(om => om.OrganizationId == organizationId)
                .ToListAsync();
        }

        public async Task<IEnumerable<OrganizationMember>> GetByUserIdAsync(string userId)
        {
            return await _dbSet
                .Include(om => om.Organization)
                .Where(om => om.UserId == userId)
                .ToListAsync();
        }

        public async Task<OrganizationMember?> GetMembershipAsync(string userId, string organizationId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(om => om.UserId == userId && om.OrganizationId == organizationId);
        }

        public async Task<OrganizationMember> InviteMemberAsync(string organizationId, string userId, string? invitedById, Volts.Domain.Enums.OrganizationRoleEnum role)
        {
            var membership = new OrganizationMember
            {
                OrganizationId = organizationId,
                UserId = userId,
                InvitedById = invitedById,
                Role = role,
                JoinedAt = DateTime.UtcNow
            };

            await _dbSet.AddAsync(membership);
            return membership;
        }
    }
}
