using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Volts.Domain.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        IOrganizationRepository Organizations { get; }
        IOrganizationMemberRepository OrganizationMembers { get; }
        IGroupRepository Groups { get; }
        IGroupMemberRepository GroupMembers { get; }
        IPositionRepository Positions { get; }
        IShiftRepository Shifts { get; }
        IShiftPositionRepository ShiftPositions { get; }
        IShiftPositionAssignmentRepository ShiftPositionAssignment { get; }

        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();

    }
}
