using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volts.Domain.Entities;

namespace Volts.Domain.Interfaces
{
    public interface IOrganizationRepository : IRepository<Organization>
    {
        Task<IEnumerable<Organization>> GetByCreatorIdAsync(string creatorId);
        Task<Organization?> GetWithMembersAsync(string id);
        Task<Organization?> GetWithGroupsAsync(string id);
    }

}
