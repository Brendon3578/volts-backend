using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volts.Application.DTOs.Group;
using Volts.Application.Interfaces;
using Volts.Domain.Entities;
using Volts.Domain.Enums;
using Volts.Domain.Interfaces;
using Volts.Application.Exceptions;
using Volts.Application.DTOs.Position;

namespace Volts.Application.Services
{
    public class GroupService : IGroupService
    {
        private readonly IUnitOfWork _unitOfWork;

        public GroupService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<IEnumerable<GroupDto>> GetAllAsync()
        {
            var groups = await _unitOfWork.Groups.GetAllAsync();

            return groups.Select(MapToDto);
        }

        public async Task<IEnumerable<GroupDto>> GetAllByOrganizationIdAsync(string organizationId)
        {
            var groups = await _unitOfWork.Groups
                .GetByOrganizationIdAsync(organizationId);

            return groups.Select(g => MapToDto(g));
        }

        public async Task<GroupDto> GetByIdAsync(string id)
        {
            var group = await _unitOfWork.Groups.GetByIdAsync(id)
                ?? throw new NotFoundException("Group not found");

            return MapToDto(group);
        }

        public async Task<GroupDto> CreateAsync(CreateGroupDto dto, string createdById)
        {
            var userExists = await _unitOfWork.Users.ExistsAsync(createdById);
            var organizationExists = await _unitOfWork.Organizations.ExistsAsync(dto.OrganizationId);

            if (userExists == false)
                throw new NotFoundException($"User with ID not found");

            if (organizationExists == false)
                throw new NotFoundException($"Organization with Id not found");


            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var group = new Group
                {
                    Name = dto.Name,
                    Description = dto.Description,
                    OrganizationId = dto.OrganizationId,
                    CreatedById = createdById,
                    Color = dto.Color,
                    Icon = dto.Icon
                };

                await _unitOfWork.Groups.AddAsync(group);

                await _unitOfWork.SaveChangesAsync();

                var groupMembership = new GroupMember()
                {
                    Role = GroupRoleEnum.GROUP_LEADER,
                    UserId = createdById,
                    GroupId = group.Id,
                };

                await _unitOfWork.GroupMembers.AddAsync(groupMembership);

                // Aqui n�o chamamos SaveChangesAsync de novo ainda.
                // O CommitTransactionAsync vai cuidar disso internamente.
                await _unitOfWork.CommitTransactionAsync();

                return MapToDto(group);

            }
            catch (Exception)
            {
                await _unitOfWork.RollbackTransactionAsync();

                throw;
            }
        }

        public async Task<GroupDto> UpdateAsync(string id, UpdateGroupDto dto, string userId)
        {
            var group = await _unitOfWork.Groups.GetByIdAsync(id)
                ?? throw new NotFoundException("Group not found");

            if (dto.Name != null) group.Name = dto.Name;

            if (dto.Description != null) group.Description = dto.Description;

            if (dto.Color != null) group.Color = dto.Color;


            if (dto.Icon != null) group.Icon = dto.Icon;

            await _unitOfWork.Groups.UpdateAsync(group);

            await _unitOfWork.SaveChangesAsync();

            return MapToDto(group);
        }

        // TODO: validar regras de negócio de quando sair apagar o grupo se só tiver um user

        // TODO: ver se pode dar erro aqui
        public async Task DeleteAsync(string id, string userId)
        {
            var group = await _unitOfWork.Groups.GetByIdAsync(id)
                ?? throw new NotFoundException("Group not found");

            var membership = await _unitOfWork.GroupMembers.GetMembershipAsync(userId, group.Id)
                ?? throw new UserHasNotPermissionException("User is not a member of the group");

            // só poder deletar se for não for volunteer, TODO: melhorar isso

            if (membership.Role == GroupRoleEnum.VOLUNTEER)
                throw new UserHasNotPermissionException("Only coordinator or group leader can delete a group");


            await _unitOfWork.Groups.DeleteAsync(id);

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<GroupMemberDto>> GetMembersAsync(string groupId)
        {
            var group = await _unitOfWork.Groups.GetByIdAsync(groupId)
                ?? throw new NotFoundException("Group not found");

            var members = await _unitOfWork.GroupMembers.GetWithUserByGroupIdAsync(groupId);

            return members.Select(m => new GroupMemberDto
            {
                Id = m.Id,
                UserId = m.UserId,
                GroupId = m.GroupId,
                Role = m.Role.ToString(),
                JoinedAt = m.JoinedAt,
                UserName = m.User.Name,
                UserEmail = m.User.Email,
            });
        }


        public async Task JoinAsync(string groupId, string userId)
        {
            // check if already member
            var existing = await _unitOfWork.GroupMembers.GetMembershipAsync(userId, groupId);

            if (existing != null) return;

            var existingGroup = await _unitOfWork.Groups.GetByIdAsync(groupId)
                ?? throw new NotFoundException("Group not exists");

            var organizationMembership = await _unitOfWork.OrganizationMembers.GetMembershipAsync(userId, existingGroup.OrganizationId)
                ?? throw new UserHasNotPermissionException("User is not member of this organization");

            var membership = new GroupMember
            {
                UserId = userId,
                GroupId = groupId,
                Role = GroupRoleEnum.VOLUNTEER,
                JoinedAt = DateTime.UtcNow
            };

            await _unitOfWork.GroupMembers.AddAsync(membership);
            await _unitOfWork.SaveChangesAsync();
        }

        // TODO: fazer rota se for usar esse service
        public async Task InviteUserAsync(string groupId, string userId, InviteUserGroupDto inviteDto)
        {
            // check if already member
            var existing = await _unitOfWork.GroupMembers.GetMembershipAsync(userId, groupId);

            if (existing != null) return;

            var invitedUser = await _unitOfWork.Users.GetByEmailAsync(inviteDto.InvitedEmail);

            if (invitedUser == null)
                throw new NotFoundException("User not found with this email");

            var isInviterMember = await _unitOfWork.GroupMembers.GetMembershipAsync(invitedUser.Id, groupId);

            if (isInviterMember != null)
            {
                throw new UserHasNotPermissionException("Inviter is not member of this group!");
            }

            // aqui nao precisa validar inviterId pois na teoria ele j� est� autenticado

            var membership = new GroupMember
            {
                UserId = inviteDto.InvitedEmail,
                GroupId = groupId,
                Role = inviteDto.InviterRole,
                JoinedAt = DateTime.UtcNow,
                AddedById = userId,
            };

            await _unitOfWork.GroupMembers.AddAsync(membership);
            await _unitOfWork.SaveChangesAsync();

        }

        public async Task LeaveAsync(string groupId, string userId)
        {
            var membership = await _unitOfWork.GroupMembers.GetMembershipAsync(userId, groupId);

            if (membership == null) return;

            await _unitOfWork.GroupMembers.DeleteAsync(membership.Id);

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<PositionDto>> GetPositionsAsync(string groupId)
        {
            var positions = await _unitOfWork.Positions.GetByGroupIdAsync(groupId);

            return positions.Select(p => new PositionDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                GroupId = p.GroupId,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
            });
        }

        public async Task<GroupCompleteViewDto?> GetGroupCompleteViewByIdAsync(string id, string userId)
        {
            var group = await _unitOfWork.Groups.GetGroupCompleteViewByIdAsync(id);
            if (group == null) return null;

            // Calculate upcoming shifts (shifts that haven't happened yet)
            return MapToCompleteViewDto(group, userId);
        }

        private static GroupDto MapToDto(Group g) => new GroupDto
        {
            Id = g.Id,
            Name = g.Name,
            Description = g.Description,
            OrganizationId = g.OrganizationId,
            CreatedById = g.CreatedById,
            Color = g.Color,
            Icon = g.Icon,
            CreatedAt = g.CreatedAt,
            UpdatedAt = g.UpdatedAt
        };

        private static GroupCompleteViewDto MapToCompleteViewDto(Group group, string userId)
        {

            var upcomingShifts = group.Shifts?.Count(s => s.StartDate >= DateTime.UtcNow) ?? 0;

            var currentUserMembership = group.Members.FirstOrDefault(m => m.UserId == userId);
            var isCurrentUserJoinedGroup = currentUserMembership != null;

            return new GroupCompleteViewDto
            {
                Id = group.Id,
                Name = group.Name,
                Description = group.Description,
                OrganizationId = group.OrganizationId,
                OrganizationName = group.Organization?.Name ?? string.Empty,
                CreatedById = group.CreatedById,
                CreatedAt = group.CreatedAt,
                MemberCount = group.Members?.Count ?? 0,
                UpcomingShiftsCount = upcomingShifts,
                Color = group.Color,
                Icon = group.Icon,
                IsCurrentUserJoined = isCurrentUserJoinedGroup,
                CurrentUserRole = currentUserMembership?.Role.ToString() ?? string.Empty
            };
        }

        private async Task<bool> UserGroupHasPermissionAsync(string userId, string groupId, IEnumerable<GroupRoleEnum> allowedRoles)
        {
            var groupMembership = (await _unitOfWork.GroupMembers
                .FindOneAsync(gm => gm.UserId == userId && gm.GroupId == groupId));

            if (groupMembership == null)
                return false;

            return allowedRoles.Contains(groupMembership.Role);
        }

        private async Task ValidateUserPermissionAsync(string userId, string groupId, IEnumerable<GroupRoleEnum> allowedRoles)
        {
            bool hasPermission = await UserGroupHasPermissionAsync(userId, groupId, allowedRoles);
            if (!hasPermission)
                throw new UserHasNotPermissionException("User does not have the required permissions for this operation");
        }

        private async Task<bool> IsGroupLeaderOrCoordinator(string userId, string groupId)
        {
            return await UserGroupHasPermissionAsync(userId, groupId, new[] { GroupRoleEnum.GROUP_LEADER, GroupRoleEnum.COORDINATOR });
        }

        public async Task<IEnumerable<GroupCompleteViewDto>> GetGroupsCompleteViewByOrganizationidAsync(string organizationId, string userId)
        {
            var groups = await _unitOfWork.Groups
                .GetGroupsCompleteViewByOrganizationidAsync(organizationId);


            return groups.Select(g => MapToCompleteViewDto(g, userId));
        }
    }
}
