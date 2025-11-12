using System.Collections.Generic;
using System.Threading.Tasks;
using Volts.Application.DTOs.Group;
using Volts.Application.DTOs.Position;

namespace Volts.Application.Interfaces
{
    public interface IGroupService
    {
        Task<IEnumerable<GroupDto>> GetAllAsync();
        Task<IEnumerable<GroupDto>> GetAllByOrganizationIdAsync(string organizationId);
        Task<GroupDto> GetByIdAsync(string id);
        Task<GroupDto> CreateAsync(CreateGroupDto dto, string createdById);
        Task<GroupDto> UpdateAsync(string id, UpdateGroupDto dto, string userId);
        Task DeleteAsync(string id, string userId);
        Task<IEnumerable<GroupMemberDto>> GetMembersAsync(string groupId);
        Task JoinAsync(string groupId, string userId);
        Task InviteUserAsync(string groupId, string userId, InviteUserGroupDto inviteDto);
        Task LeaveAsync(string groupId, string userId);
        Task<IEnumerable<PositionDto>> GetPositionsAsync(string groupId);
        Task<GroupCompleteViewDto?> GetGroupCompleteViewByIdAsync(string id, string userId);
        Task<IEnumerable<GroupCompleteViewDto>> GetGroupsCompleteViewByOrganizationidAsync(string organizationId, string userId);
    }
}
