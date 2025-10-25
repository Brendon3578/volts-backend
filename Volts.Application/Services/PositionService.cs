using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volts.Application.DTOs.Position;
using Volts.Application.Exceptions;
using Volts.Application.Interfaces;
using Volts.Domain.Entities;
using Volts.Domain.Interfaces;
using Volts.Domain.Enums;

namespace Volts.Application.Services
{
    public class PositionService : IPositionService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PositionService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<PositionDto>> GetByGroupIdAsync(string groupId)
        {
            var group = await _unitOfWork.Groups.GetByIdAsync(groupId)
                ?? throw new NotFoundException("Group not found");

            var positions = await _unitOfWork.Positions.GetByGroupIdAsync(groupId);

            return positions.Select(p => new PositionDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                GroupId = p.GroupId,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt
            });
        }

        public async Task<PositionDto?> GetByIdAsync(string id)
        {
            var position = await _unitOfWork.Positions.GetByIdAsync(id);
            if (position == null) return null;

            return new PositionDto
            {
                Id = position.Id,
                Name = position.Name,
                Description = position.Description,
                GroupId = position.GroupId,
                CreatedAt = position.CreatedAt,
                UpdatedAt = position.UpdatedAt
            };
        }

        public async Task<PositionDto> CreateAsync(CreatePositionDto dto, string userId)
        {
            var group = await _unitOfWork.Groups.GetByIdAsync(dto.GroupId) ??
                throw new NotFoundException("Group not found");

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var position = new Position
                {
                    Name = dto.Name,
                    Description = dto.Description,
                    GroupId = dto.GroupId
                };

                await _unitOfWork.Positions.AddAsync(position);
                await _unitOfWork.CommitTransactionAsync();

                return new PositionDto
                {
                    Id = position.Id,
                    Name = position.Name,
                    Description = position.Description,
                    GroupId = position.GroupId,
                    CreatedAt = position.CreatedAt,
                    UpdatedAt = position.UpdatedAt
                };
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<PositionDto> UpdateAsync(string id, UpdatePositionDto dto, string userId)
        {
            var position = await _unitOfWork.Positions.GetByIdAsync(id)
                ?? throw new NotFoundException("Position not found");

            // check permissions on the position's group
            var membership = await _unitOfWork.GroupMembers.GetMembershipAsync(userId, position.GroupId);

            if (membership == null)
                throw new UnauthorizedAccessException("User is not a member of the group");

            if (membership.Role != GroupRoleEnum.GROUP_LEADER && membership.Role != GroupRoleEnum.COORDINATOR)
                throw new UnauthorizedAccessException("Insufficient role to update position");

            if (dto.Name != null) position.Name = dto.Name;
            if (dto.Description != null) position.Description = dto.Description;

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                await _unitOfWork.Positions.UpdateAsync(position);
                await _unitOfWork.CommitTransactionAsync();

                return new PositionDto
                {
                    Id = position.Id,
                    Name = position.Name,
                    Description = position.Description,
                    GroupId = position.GroupId,
                    CreatedAt = position.CreatedAt,
                    UpdatedAt = position.UpdatedAt
                };
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task DeleteAsync(string id, string userId)
        {
            var position = await _unitOfWork.Positions.GetByIdAsync(id)
                ?? throw new NotFoundException("Position not found");

            var membership = await _unitOfWork.GroupMembers.GetMembershipAsync(userId, position.GroupId);
            if (membership == null)
                throw new UnauthorizedAccessException("User is not a member of the group");

            if (membership.Role != GroupRoleEnum.GROUP_LEADER && membership.Role != GroupRoleEnum.COORDINATOR)
                throw new UnauthorizedAccessException("Insufficient role to delete position");

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                await _unitOfWork.Positions.DeleteAsync(id);
                await _unitOfWork.CommitTransactionAsync();
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<bool> UserHasPermissionAsync(string userId, string groupId, IEnumerable<GroupRoleEnum> allowedRoles)
        {
            var member = (await _unitOfWork.GroupMembers
                .FindOneAsync(gm => gm.UserId == userId && gm.GroupId == groupId));

            if (member == null) return false;

            return allowedRoles.Contains(member.Role);
        }

        public async Task<bool> IsGroupLeaderOrCoordinator(string userId, string groupId)
        {
            var hasPermission = await UserHasPermissionAsync(userId, groupId,
            [GroupRoleEnum.GROUP_LEADER, GroupRoleEnum.COORDINATOR]);

            return hasPermission;
        }
    }
}
