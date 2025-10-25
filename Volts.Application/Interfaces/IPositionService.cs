using System.Collections.Generic;
using System.Threading.Tasks;
using Volts.Application.DTOs.Position;
using Volts.Domain.Enums;

namespace Volts.Application.Interfaces
{
    public interface IPositionService
    {
        Task<IEnumerable<PositionDto>> GetByGroupIdAsync(string groupId);
        Task<PositionDto?> GetByIdAsync(string id);
        Task<PositionDto> CreateAsync(CreatePositionDto dto, string userId);
        Task<PositionDto> UpdateAsync(string id, UpdatePositionDto dto, string userId);
        Task DeleteAsync(string id, string userId);

        // Permission helper similar to OrganizationService
        Task<bool> UserHasPermissionAsync(string userId, string groupId, IEnumerable<GroupRoleEnum> allowedRoles);

        Task<bool> IsGroupLeaderOrCoordinator(string userId, string groupId);
    }
}
