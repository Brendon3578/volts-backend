using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volts.Application.DTOs.Shift;
using Volts.Application.Exceptions;
using Volts.Application.Interfaces;
using Volts.Domain.Entities;
using Volts.Domain.Enums;
using Volts.Domain.Interfaces;

namespace Volts.Application.Services
{
    public class ShiftService : IShiftService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ShiftService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<ShiftDto>> GetByGroupIdAsync(string groupId)
        {
            var group = await _unitOfWork.Groups.GetByIdAsync(groupId)
                ?? throw new NotFoundException("Group not found");

            var shifts = await _unitOfWork.Shifts.GetByGroupIdAsync(groupId);
            return shifts.Select(MapToDto);
        }

        public async Task<ShiftDto?> GetByIdAsync(string id)
        {
            var shift = await _unitOfWork.Shifts.GetByIdAsync(id);
            return shift == null ? null : MapToDto(shift);
        }

        public async Task<ShiftDto> CreateAsync(CreateShiftDto dto, string userId)
        {
            var group = await _unitOfWork.Groups.GetByIdAsync(dto.GroupId)
                ?? throw new NotFoundException("Group not found");

            if (await IsGroupLeaderOrCoordinator(userId, dto.GroupId) == false)
                throw new UserHasNotPermissionException("User is not leader or coordinator");

            var shift = new Shift
            {
                Date = dto.Date,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                Title = dto.Title,
                Notes = dto.Notes,
                GroupId = dto.GroupId,
                Status = ShiftStatusEnum.OPEN
            };

            await _unitOfWork.Shifts.AddAsync(shift);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(shift);
        }

        public async Task<ShiftDto> UpdateAsync(string id, UpdateShiftDto dto, string userId)
        {
            var shift = await _unitOfWork.Shifts.GetByIdAsync(id)
                ?? throw new NotFoundException("Shift not found");

            if (await IsGroupLeaderOrCoordinator(userId, shift.GroupId) == false)
                throw new UserHasNotPermissionException("User is not leader or coordinator");

            if (dto.Date.HasValue) shift.Date = dto.Date.Value;
            if (dto.StartTime.HasValue) shift.StartTime = dto.StartTime.Value;
            if (dto.EndTime.HasValue) shift.EndTime = dto.EndTime.Value;
            if (!string.IsNullOrEmpty(dto.Title)) shift.Title = dto.Title;
            if (!string.IsNullOrEmpty(dto.Notes)) shift.Notes = dto.Notes;
            if (dto.Status.HasValue) shift.Status = dto.Status.Value;

            await _unitOfWork.Shifts.UpdateAsync(shift);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(shift);
        }

        public async Task DeleteAsync(string id, string userId)
        {
            var shift = await _unitOfWork.Shifts.GetByIdAsync(id)
                ?? throw new NotFoundException("Shift not found");

            if (await IsGroupLeaderOrCoordinator(userId, shift.GroupId) == false)
                throw new UserHasNotPermissionException("User is not leader or coordinator");

            await _unitOfWork.Shifts.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }

        private async Task<bool> UserHasPermissionAsync(string userId, string groupId, IEnumerable<GroupRoleEnum> allowedRoles)
        {
            var member = await _unitOfWork.GroupMembers
                .FindOneAsync(m => m.UserId == userId && m.GroupId == groupId);

            return member != null && allowedRoles.Contains(member.Role);
        }

        private Task<bool> IsGroupLeaderOrCoordinator(string userId, string groupId)
        {
            return UserHasPermissionAsync(userId, groupId, [GroupRoleEnum.GROUP_LEADER, GroupRoleEnum.COORDINATOR]);
        }

        private static ShiftDto MapToDto(Shift shift)
        {
            return new ShiftDto
            {
                Id = shift.Id,
                Date = shift.Date,
                StartTime = shift.StartTime,
                EndTime = shift.EndTime,
                Title = shift.Title,
                Notes = shift.Notes,
                Status = shift.Status,
                GroupId = shift.GroupId,
                CreatedAt = shift.CreatedAt,
                UpdatedAt = shift.UpdatedAt
            };
        }
    }

}
