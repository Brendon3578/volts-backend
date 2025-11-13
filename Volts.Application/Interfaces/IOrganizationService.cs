using System.Collections.Generic;
using System.Threading.Tasks;
using Volts.Application.DTOs.Organization;

namespace Volts.Application.Interfaces
{
    public interface IOrganizationService
    {
        Task<IEnumerable<OrganizationDto>> GetAllOrganizationsAsync();
        Task<IEnumerable<OrganizationDto>> GetAllOrganizationsAvailableAsync(string userId);
        Task<OrganizationDto> GetOrganizationByIdAsync(string id);
        Task<IEnumerable<OrganizationDto>> GetOrganizationsByCreatorAsync(string creatorId);
        Task<OrganizationDto> CreateOrganizationAsync(CreateOrganizationDto dto, string createdById);
        Task<OrganizationDto> UpdateOrganizationAsync(string id, UpdateOrganizationDto dto, string userId);
        Task<IEnumerable<OrganizationDto>> GetOrganizationsForCurrentUserAsync(string userId);
        Task DeleteOrganizationAsync(string id, string userId);
        Task JoinAsync(string organizationId, string userId);
        Task LeaveAsync(string organizationId, string userId);
        Task<OrganizationCompleteViewDto?> GetOrganizationCompleteViewByIdAsync(string id, string userId);
        Task<IEnumerable<OrganizationCompleteViewDto>> GetOrganizationsCompleteViewAsync(string userId);
        Task<IEnumerable<OrganizationMemberDto>> GetOrganizationMembersAsync(string organizationId);
        Task ChangeOrganizationMemberRoleAsync(string organizationId, string memberId, string role, string currentUserId);
        Task<OrganizationMemberDto> InviteMemberAsync(string organizationId, InviteUserOrganizationDto dto, string currentUserId);
    }
}
