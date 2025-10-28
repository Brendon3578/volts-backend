using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volts.Domain.Entities;

namespace Volts.Domain.Interfaces
{
    public interface IGroupMemberRepository : IRepository<GroupMember>
    {
        Task<IEnumerable<GroupMember>> GetByGroupIdAsync(string groupId);
        Task<IEnumerable<GroupMember>> GetByUserAndOrganizationAsync(string userId, string id);
        Task<IEnumerable<GroupMember>> GetByUserIdAsync(string userId);
        Task<GroupMember?> GetMembershipAsync(string userId, string groupId);
    }
}
