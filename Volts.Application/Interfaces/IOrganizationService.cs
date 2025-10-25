using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volts.Application.DTOs.Organization;
using Volts.Domain.Enums;

namespace Volts.Application.Interfaces
{
    public interface IOrganizationService
    {
        Task<IEnumerable<OrganizationDto>> GetAllOrganizationsAsync();
        Task<OrganizationDto?> GetOrganizationByIdAsync(string id);
        Task<IEnumerable<OrganizationDto>> GetOrganizationsByCreatorAsync(string creatorId);
        Task<OrganizationDto> CreateOrganizationAsync(CreateOrganizationDto dto, string createdById);
        Task<OrganizationDto> UpdateOrganizationAsync(string id, UpdateOrganizationDto dto);

        Task<IEnumerable<OrganizationDto>> GetOrganizationsForCurrentUserAsync(string userId);


        Task DeleteOrganizationAsync(string id);


        // permissões
        Task<bool> UserHasPermissionAsync(string userId, string organizationId, IEnumerable<OrganizationRoleEnum> allowedRoles);

        // Join / Leave
        Task JoinAsync(string organizationId, string userId);
        Task LeaveAsync(string organizationId, string userId);
    }
}
