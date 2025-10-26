using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volts.Application.DTOs.Organization;
using Volts.Application.Interfaces;
using Volts.Application.Exceptions;
using Volts.Domain.Entities;
using Volts.Domain.Enums;
using Volts.Domain.Interfaces;

namespace Volts.Application.Services
{
    public class OrganizationService : IOrganizationService
    {
        private readonly IUnitOfWork _unitOfWork;

        public OrganizationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<OrganizationDto>> GetAllOrganizationsAsync()
        {
            var organizations = await _unitOfWork.Organizations.GetAllAsync();
            return organizations.Select(MapToDto);
        }

        public async Task<OrganizationDto> GetOrganizationByIdAsync(string id)
        {
            var organization = await _unitOfWork.Organizations.GetByIdAsync(id);
            if (organization == null)
                throw new NotFoundException($"Organization with ID {id} not found");
                
            return MapToDto(organization);
        }

        public async Task<IEnumerable<OrganizationDto>> GetOrganizationsByCreatorAsync(string creatorId)
        {
            var organizations = await _unitOfWork.Organizations.GetByCreatorIdAsync(creatorId);
            return organizations.Select(MapToDto);
        }

        public async Task<OrganizationDto> CreateOrganizationAsync(CreateOrganizationDto dto, string createdById)
        {
            var userExists = await _unitOfWork.Users.ExistsAsync(createdById);

            if (userExists == false)
            {
                throw new NotFoundException($"User with ID not found");
            }

            // atomicidade
            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var organization = new Organization
                {
                    Name = dto.Name,
                    Description = dto.Description,
                    Email = dto.Email,
                    Phone = dto.Phone,
                    Address = dto.Address,
                    CreatedById = createdById
                };

                await _unitOfWork.Organizations.AddAsync(organization);
                await _unitOfWork.SaveChangesAsync();

                var organizationMembership = new OrganizationMember()
                {
                    Role = OrganizationRoleEnum.ADMIN,
                    UserId = createdById,
                    OrganizationId = organization.Id
                };

                await _unitOfWork.OrganizationMembers.AddAsync(organizationMembership);

                // Aqui não chamamos SaveChangesAsync de novo ainda.
                // O CommitTransactionAsync vai cuidar disso internamente.
                await _unitOfWork.CommitTransactionAsync();

                return MapToDto(organization);
            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<OrganizationDto> UpdateOrganizationAsync(string id, UpdateOrganizationDto dto, string userId)
        {
            var organization = await _unitOfWork.Organizations.GetByIdAsync(id);
            if (organization == null)
                throw new NotFoundException($"Organization with ID {id} not found");

            await ValidateUserPermissionAsync(userId, id, new[] { OrganizationRoleEnum.ADMIN, OrganizationRoleEnum.LEADER });

            if (dto.Name != null) organization.Name = dto.Name;
            if (dto.Description != null) organization.Description = dto.Description;
            if (dto.Email != null) organization.Email = dto.Email;
            if (dto.Phone != null) organization.Phone = dto.Phone;
            if (dto.Address != null) organization.Address = dto.Address;

            await _unitOfWork.Organizations.UpdateAsync(organization);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(organization);
        }

        public async Task DeleteOrganizationAsync(string id, string userId)
        {
            var organization = await _unitOfWork.Organizations.GetByIdAsync(id);
            if (organization == null)
                throw new NotFoundException($"Organization with ID {id} not found");

            await ValidateUserPermissionAsync(userId, id, new[] { OrganizationRoleEnum.ADMIN });

            await _unitOfWork.Organizations.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }

        private static OrganizationDto MapToDto(Organization organization)
        {
            return new OrganizationDto
            {
                Id = organization.Id,
                Name = organization.Name,
                Description = organization.Description,
                Email = organization.Email,
                Phone = organization.Phone,
                Address = organization.Address,
                CreatedById = organization.CreatedById,
                CreatedAt = organization.CreatedAt,
                UpdatedAt = organization.UpdatedAt
            };
        }

        private async Task<bool> UserHasPermissionAsync(string userId, string organizationId, IEnumerable<OrganizationRoleEnum> allowedRoles)
        {
            var member = (await _unitOfWork.OrganizationMembers
                .FindOneAsync(om => om.UserId == userId && om.OrganizationId == organizationId));

            if (member == null) return false;

            return allowedRoles.Contains(member.Role);
        }
        
        private async Task ValidateUserPermissionAsync(string userId, string organizationId, IEnumerable<OrganizationRoleEnum> allowedRoles)
        {
            bool hasPermission = await UserHasPermissionAsync(userId, organizationId, allowedRoles);
            if (!hasPermission)
                throw new UserHasNotPermissionException("User does not have permission to perform this action");
        }

        public async Task<IEnumerable<OrganizationDto>> GetOrganizationsForCurrentUserAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID inválido.");

            // Busca todas as organizações onde o usuário é membro
            var memberships = await _unitOfWork.OrganizationMembers
                .FindAsync(member => member.UserId == userId);

            // Evita erro se o usuário não estiver em nenhuma organização
            if (memberships.Any() == false)
                return [];

            // Extrai os IDs das organizações
            var organizationIds = memberships.Select(m => m.OrganizationId).ToList();

            // Busca todas as organizações correspondentes
            var organizations = await _unitOfWork.Organizations
                .FindAsync(org => organizationIds.Contains(org.Id));

            // Mapeia para DTOs
            var organizationDtos = organizations.Select(MapToDto).ToList();

            return organizationDtos;
        }

        // Join / Leave implementations
        public async Task JoinAsync(string organizationId, string userId)
        {
            var organization = await _unitOfWork.Organizations.GetByIdAsync(organizationId)
                ?? throw new NotFoundException("Organization not found");

            var existing = await _unitOfWork.OrganizationMembers.GetMembershipAsync(userId, organizationId);

            if (existing != null) return; // already member, idempotent

            var membership = new OrganizationMember
            {
                UserId = userId,
                OrganizationId = organizationId,
                Role = OrganizationRoleEnum.MEMBER,
                JoinedAt = DateTime.UtcNow
            };

            await _unitOfWork.OrganizationMembers.AddAsync(membership);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task LeaveAsync(string organizationId, string userId)
        {
            var organization = await _unitOfWork.Organizations.GetByIdAsync(organizationId)
                ?? throw new NotFoundException("Organization not found");

            var membership = await _unitOfWork.OrganizationMembers.GetMembershipAsync(userId, organizationId);
            if (membership == null)
                return; // idempotent

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // Remove o membro que saiu
                await _unitOfWork.OrganizationMembers.DeleteAsync(membership.Id);

                // Pega os membros restantes
                var remainingMembers = await _unitOfWork.OrganizationMembers.FindAsync(m => m.OrganizationId == organizationId);

                // Se não sobrou ninguém, deleta a organização
                if (!remainingMembers.Any())
                {
                    await DeleteOrganizationIfEmptyAsync(organizationId);
                }
                else
                {
                    // Se não há ADMINs, promove LEADERs a ADMIN
                    await PromoteLeadersIfNoAdminsAsync(remainingMembers);
                }

                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        /// <summary>
        /// Deleta a organização se não houver membros restantes.
        /// </summary>
        private async Task DeleteOrganizationIfEmptyAsync(string organizationId)
        {
            await _unitOfWork.Organizations.DeleteAsync(organizationId);
        }

        /// <summary>
        /// Se não houver ADMINs, promove todos os LEADERs a ADMINs.
        /// </summary>
        private async Task PromoteLeadersIfNoAdminsAsync(IEnumerable<OrganizationMember> members)
        {
            bool hasAdmins = members.Any(m => m.Role == OrganizationRoleEnum.ADMIN);
            if (hasAdmins)
                return;

            var leaders = members
                .Where(m => m.Role == OrganizationRoleEnum.LEADER)
                .ToList();

            if (leaders.Count == 0)
                return;

            foreach (var leader in leaders)
                leader.Role = OrganizationRoleEnum.ADMIN;

            await _unitOfWork.OrganizationMembers.UpdateRangeAsync(leaders);
        }
    }
}
