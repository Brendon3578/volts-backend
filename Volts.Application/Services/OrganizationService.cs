using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Volts.Application.DTOs.Organization;
using Volts.Application.Exceptions;
using Volts.Application.Interfaces;
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

        public async Task<IEnumerable<OrganizationDto>> GetAllOrganizationsAvailableAsync(string userId)
        {
            var availableOrganizations = await _unitOfWork.Organizations.GetAvailableToEnter(userId);

            return availableOrganizations.Select(MapToDto);
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

        private static OrganizationCompleteViewDto MapToCompleteViewDto(Organization organization, string userId)
        {
            var isJoined = organization.Members.Any(m => m.UserId == userId);
            var currentUserRole = organization.Members
                .FirstOrDefault(m => m.UserId == userId)?.Role.ToString() ?? string.Empty;

            return new OrganizationCompleteViewDto
            {
                Id = organization.Id,
                Name = organization.Name,
                Description = organization.Description,
                Email = organization.Email,
                Phone = organization.Phone,
                Address = organization.Address,
                CreatedById = organization.CreatedById,
                CreatedAt = organization.CreatedAt,
                UpdatedAt = organization.UpdatedAt,
                IsCurrentUserJoined = isJoined,
                MemberCount = organization.Members.Count,
                CurrentUserOrganizationRole = currentUserRole
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
                return; // idempotente

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                // Remove o membro que saiu
                await _unitOfWork.OrganizationMembers.DeleteAsync(membership.Id);

                // Pega os membros restantes
                var remainingMembers = await _unitOfWork.OrganizationMembers.FindAsync(m => m.OrganizationId == organizationId);

                // Remove manualmente o membro atual, caso ainda esteja na lista por não ter sido persistido
                remainingMembers = remainingMembers
                    .Where(m => m.UserId != userId)
                    .ToList();

                // Se não sobrou ninguém, deleta a organização
                if (!remainingMembers.Any())
                {
                    await DeleteOrganizationIfEmptyAsync(organizationId);
                }
                else
                {
                    // Verifica papéis dos membros restantes
                    var hasAdmins = remainingMembers.Any(m => m.Role == OrganizationRoleEnum.ADMIN);
                    var hasLeaders = remainingMembers.Any(m => m.Role == OrganizationRoleEnum.LEADER);

                    if (!hasAdmins && !hasLeaders)
                    {
                        // Nenhum admin nem líder: deleta a organização
                        await DeleteOrganizationIfEmptyAsync(organizationId);
                    }
                    else if (!hasAdmins && hasLeaders)
                    {
                        // Sem admins, mas há líderes → promove para admin
                        await PromoteLeadersToAdminsAsync(remainingMembers);
                    }
                }

                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task RemoveMemberAsync(string organizationId, string memberId, string currentUserId)
        {
            // Permissão: ADMIN ou LEADER
            var currentMember = await _unitOfWork.OrganizationMembers.GetMembershipAsync(currentUserId, organizationId)
                ?? throw new UserHasNotPermissionException("Current user is not a member of the organization");

            if (currentMember.Role == OrganizationRoleEnum.MEMBER)
                throw new UserHasNotPermissionException("Você não tem permissão.");

            // Verificar membro
            var member = await _unitOfWork.OrganizationMembers.GetByIdAsync(memberId);
            
            if (member == null || member.OrganizationId != organizationId)
                throw new NotFoundException("Membro não encontrado na organização");

            // Admins não podem remover a si mesmos
            if (currentMember.Role == OrganizationRoleEnum.ADMIN && member.UserId == currentUserId)
                throw new UserHasNotPermissionException("Admin não pode remover a si mesmo");

            // Remover relacionamento de membro com a organização
            await _unitOfWork.OrganizationMembers.DeleteMembershipAsync(memberId);

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<OrganizationMemberDto> InviteMemberAsync(string organizationId, InviteUserOrganizationDto dto, string currentUserId)
        {
            if (string.IsNullOrWhiteSpace(dto.InvitedEmail))
                throw new ArgumentException("InvitedEmail inválido");

            // Validar permissão do usuário atual (ADMIN ou LEADER)
            await ValidateUserPermissionAsync(currentUserId, organizationId, new[] { OrganizationRoleEnum.ADMIN, OrganizationRoleEnum.LEADER });

            // Buscar usuário por email
            var invitedUser = await _unitOfWork.Users.GetByEmailAsync(dto.InvitedEmail);
            if (invitedUser == null)
                throw new NotFoundException("Usuário não encontrado");

            // Se já é membro, retornar idempotente
            var existingMember = await _unitOfWork.OrganizationMembers.GetMembershipAsync(invitedUser.Id, organizationId);
            if (existingMember != null)
            {
                return new OrganizationMemberDto
                {
                    Id = existingMember.Id,
                    UserId = existingMember.UserId,
                    OrganizationId = existingMember.OrganizationId,
                    Role = existingMember.Role.ToString(),
                    JoinedAt = existingMember.JoinedAt,
                    UserName = invitedUser.Name,
                    UserEmail = invitedUser.Email
                };
            }

            if (!Enum.TryParse<OrganizationRoleEnum>(dto.InviterRole, true, out var roleToAssign))
                throw new ArgumentException("Invalid role provided");


            // Persistir relacionamento do novo membro
            var membership = await _unitOfWork.OrganizationMembers.InviteMemberAsync(
                organizationId,
                invitedUser.Id,
                currentUserId,
                roleToAssign
            );

            await _unitOfWork.SaveChangesAsync();

            return new OrganizationMemberDto
            {
                Id = membership.Id,
                UserId = membership.UserId,
                OrganizationId = membership.OrganizationId,
                Role = membership.Role.ToString(),
                JoinedAt = membership.JoinedAt,
                UserName = invitedUser.Name,
                UserEmail = invitedUser.Email
            };
        }

        /// <summary>
        /// Deleta a organização se não houver membros restantes.
        /// </summary>
        private async Task DeleteOrganizationIfEmptyAsync(string organizationId)
        {
            await _unitOfWork.Organizations.DeleteAsync(organizationId);
        }

        /// <summary>
        /// Promove todos os líderes para administradores.
        /// </summary>
        private async Task PromoteLeadersToAdminsAsync(IEnumerable<OrganizationMember> members)
        {
            var leaders = members
                .Where(m => m.Role == OrganizationRoleEnum.LEADER)
                .ToList();

            if (leaders.Count == 0)
                return;

            foreach (var leader in leaders)
                leader.Role = OrganizationRoleEnum.ADMIN;

            await _unitOfWork.OrganizationMembers.UpdateRangeAsync(leaders);
        }


        public async Task<OrganizationCompleteViewDto?> GetOrganizationCompleteViewByIdAsync(string id, string userId)
        {
            var organization = await _unitOfWork.Organizations.GetWithMembersAsync(id);
            if (organization == null) return null;
            return MapToCompleteViewDto(organization, userId);
        }

        public async Task<IEnumerable<OrganizationCompleteViewDto>> GetOrganizationsCompleteViewAsync(string userId)
        {
            var organizations = await _unitOfWork.Organizations.GetAllWithMembersAsync();
            return organizations.Select(o => MapToCompleteViewDto(o, userId));
        }

        public async Task<IEnumerable<OrganizationMemberDto>> GetOrganizationMembersAsync(string organizationId)
        {
            var members = await _unitOfWork.OrganizationMembers.GetByOrganizationIdAsync(organizationId);
            return members.Select(m => new OrganizationMemberDto
            {
                Id = m.Id,
                UserId = m.UserId,
                OrganizationId = m.OrganizationId,
                Role = m.Role.ToString(),
                JoinedAt = m.JoinedAt,
                UserName = m.User?.Name ?? string.Empty,
                UserEmail = m.User?.Email ?? string.Empty
            });
        }

        public async Task ChangeOrganizationMemberRoleAsync(string organizationId, string memberId, string role, string currentUserId)
        {
            // Validar permissão: ADMIN ou LEADER
            var currentMember = await _unitOfWork.OrganizationMembers.GetMembershipAsync(currentUserId, organizationId);
            if (currentMember == null || (currentMember.Role != OrganizationRoleEnum.ADMIN && currentMember.Role != OrganizationRoleEnum.LEADER))
            {
                throw new UserHasNotPermissionException("User does not have permission to perform this action");
            }

            // Validar member alvo
            var member = await _unitOfWork.OrganizationMembers.GetByIdAsync(memberId)
                ?? throw new NotFoundException("Organization member not found");

            if (member.OrganizationId != organizationId)
                throw new ArgumentException("Member does not belong to this organization");

            // Parse de role
            if (!Enum.TryParse<OrganizationRoleEnum>(role, true, out var newRole))
                throw new ArgumentException("Invalid role provided");

            if (currentMember.Role == OrganizationRoleEnum.LEADER)
            {
                if (member.Role == OrganizationRoleEnum.ADMIN)
                    throw new UserHasNotPermissionException("Leader não pode alterar ADMIN");

                if (newRole == OrganizationRoleEnum.ADMIN)
                    throw new UserHasNotPermissionException("Leader não pode promover para ADMIN");
            }

            member.Role = newRole;
            await _unitOfWork.OrganizationMembers.UpdateAsync(member);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<OrganizationUserRoleDto> GetUserRoleAsync(string organizationId, string userId)
        {
            var membership = await _unitOfWork.OrganizationMembers.GetMembershipAsync(userId, organizationId);
            if (membership == null)
                throw new NotFoundException("User is not a member of the organization");

            return new OrganizationUserRoleDto
            {
                UserId = membership.UserId,
                Role = membership.Role.ToString()
            };
        }
    }
}
