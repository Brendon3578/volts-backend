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
    }
}
