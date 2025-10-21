﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volts.Domain.Entities;

namespace Volts.Domain.Interfaces
{
    public interface IGroupRepository : IRepository<Group>
    {
        Task<IEnumerable<Group>> GetByOrganizationIdAsync(string organizationId);
        Task<Group?> GetWithMembersAsync(string id);
        Task<Group?> GetWithShiftsAsync(string id);
        Task<Group?> GetWithPositionsAsync(string id);
    }
}
