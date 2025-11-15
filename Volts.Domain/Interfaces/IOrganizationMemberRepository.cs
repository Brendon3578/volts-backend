using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volts.Domain.Entities;

namespace Volts.Domain.Interfaces
{
    public interface IOrganizationMemberRepository : IRepository<OrganizationMember>
    {
        Task<IEnumerable<OrganizationMember>> GetByOrganizationIdAsync(string organizationId);
        Task<IEnumerable<OrganizationMember>> GetWithMemberByUserIdAsync(string userId);
        Task<OrganizationMember?> GetMembershipAsync(string userId, string organizationId);
        Task<OrganizationMember> InviteMemberAsync(string organizationId, string userId, string? invitedById, Volts.Domain.Enums.OrganizationRoleEnum role);
        Task DeleteMembershipAsync(string memberId);
    }
}
