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
    public class GroupMemberRepository : Repository<GroupMember>, IGroupMemberRepository
    {
        public GroupMemberRepository(VoltsDbContext context) : base(context) { }

        public async Task<IEnumerable<GroupMember>> GetByGroupIdAsync(string groupId)
        {
            return await _dbSet
                .Include(gm => gm.User)
                .Where(gm => gm.GroupId == groupId)
                .ToListAsync();
        }

        public async Task<IEnumerable<GroupMember>> GetByUserIdAsync(string userId)
        {
            return await _dbSet
                .Include(gm => gm.Group)
                .Where(gm => gm.UserId == userId)
                .ToListAsync();
        }

        public async Task<GroupMember?> GetMembershipAsync(string userId, string groupId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(gm => gm.UserId == userId && gm.GroupId == groupId);
        }
    }
   }
