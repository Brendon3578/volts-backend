using System.Collections.Generic;
using System.Threading.Tasks;
using Volts.Application.DTOs.Shift;
using Volts.Domain.Enums;

namespace Volts.Application.Interfaces
{
    public interface IShiftService
    {
        Task<IEnumerable<ShiftDto>> GetByGroupIdAsync(string groupId);
        Task<ShiftDto?> GetByIdAsync(string id);
        Task<ShiftDto> CreateAsync(CreateShiftDto dto, string userId);
        Task<ShiftDto> UpdateAsync(string id, UpdateShiftDto dto, string userId);
        Task DeleteAsync(string id, string userId);

        Task<bool> UserHasPermissionAsync(string userId, string groupId, IEnumerable<GroupRoleEnum> allowedRoles);
    }
}
