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

            var userMembership = await _unitOfWork.OrganizationMembers.GetMembershipAsync(createdById, dto.OrganizationId);

            if (userMembership == null || userMembership.Role == OrganizationRoleEnum.MEMBER)
                throw new UnauthorizedAccessException("Você não tem permissão");

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

            var membership = await _unitOfWork.OrganizationMembers.GetMembershipAsync(userId, group.OrganizationId)
                ?? throw new UserHasNotPermissionException("User is not a member of the organization");

            if (membership.Role == OrganizationRoleEnum.MEMBER)
                throw new UserHasNotPermissionException("Only admin or leader can update a group");

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

            var membership = await _unitOfWork.OrganizationMembers.GetMembershipAsync(userId, group.OrganizationId)
                ?? throw new UserHasNotPermissionException("User is not a member of the organization");

            // só poder deletar se for não for volunteer, TODO: melhorar isso

            if (membership.Role == OrganizationRoleEnum.MEMBER)
                throw new UserHasNotPermissionException("Only admin or leader can delete a group");


            await _unitOfWork.Groups.DeleteAsync(id);

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
            return MapToCompleteViewDto(group);
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

        private static GroupCompleteViewDto MapToCompleteViewDto(Group group)
        {
            var upcomingShifts = group.Shifts?.Count(s => s.StartDate >= DateTime.UtcNow) ?? 0;

            return new GroupCompleteViewDto
            {
                Id = group.Id,
                Name = group.Name,
                Description = group.Description,
                OrganizationId = group.OrganizationId,
                OrganizationName = group.Organization?.Name ?? string.Empty,
                CreatedById = group.CreatedById,
                CreatedAt = group.CreatedAt,
                TotalShiftsCount = group.Shifts?.Count,
                UpcomingShiftsCount = upcomingShifts,
                Color = group.Color,
                Icon = group.Icon,
            };
        }

        public async Task<IEnumerable<GroupCompleteViewDto>> GetGroupsCompleteViewByOrganizationidAsync(string organizationId, string userId)
        {
            var groups = await _unitOfWork.Groups
                .GetGroupsCompleteViewByOrganizationidAsync(organizationId);

            return groups.Select(MapToCompleteViewDto);
        }
    }
}
