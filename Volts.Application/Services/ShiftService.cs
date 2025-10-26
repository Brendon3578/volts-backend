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

            var shifts = await _unitOfWork.Shifts.GetByGroupIdWithPositionsAsync(groupId);

            return shifts.Select(MapToDto);
        }

        public async Task<ShiftDto> GetByIdAsync(string id)
        {
            var shift = await _unitOfWork.Shifts.GetWithPositionsAsync(id)
                ?? throw new NotFoundException("Shift not found");

            return MapToDto(shift);
        }


        public async Task<ShiftDto> CreateAsync(CreateShiftDto dto, string userId)
        {
            var group = await _unitOfWork.Groups.GetByIdAsync(dto.GroupId)
                ?? throw new NotFoundException("Group not found");

            if (!await IsGroupLeaderOrCoordinator(userId, dto.GroupId))
                throw new UserHasNotPermissionException("User is not leader or coordinator");

            await _unitOfWork.BeginTransactionAsync();

            try
            {
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

                // Cria as posições associadas
                if (dto.Positions?.Any() == true)
                {
                    var shiftPositions = new List<ShiftPosition>();

                    foreach (var positionDto in dto.Positions)
                    {
                        var positionExists = await _unitOfWork.Positions.ExistsAsync(positionDto.PositionId);
                        if (!positionExists)
                            throw new NotFoundException($"Position with ID {positionDto.PositionId} not found");

                        shiftPositions.Add(new ShiftPosition
                        {
                            Shift = shift, // associação direta (EF vai preencher ShiftId)
                            PositionId = positionDto.PositionId,
                            RequiredCount = positionDto.RequiredCount,
                            VolunteersCount = 0
                        });
                    }

                    await _unitOfWork.ShiftPositions.AddRangeAsync(shiftPositions);
                }

                await _unitOfWork.CommitTransactionAsync();

                var createdShift = await _unitOfWork.Shifts.GetWithPositionsAsync(shift.Id)
                    ?? throw new NotFoundException("Created shift could not be retrieved after commit");

                return MapToDto(createdShift);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
        }

        public async Task<ShiftDto> UpdateAsync(string id, UpdateShiftDto dto, string userId)
        {
            var shift = await _unitOfWork.Shifts.GetByIdAsync(id)
                ?? throw new NotFoundException("Shift not found");

            if (await IsGroupLeaderOrCoordinator(userId, shift.GroupId) == false)
                throw new UserHasNotPermissionException("User is not leader or coordinator");

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                if (dto.Date.HasValue) shift.Date = dto.Date.Value;
                if (dto.StartTime.HasValue) shift.StartTime = dto.StartTime.Value;
                if (dto.EndTime.HasValue) shift.EndTime = dto.EndTime.Value;
                if (!string.IsNullOrEmpty(dto.Title)) shift.Title = dto.Title;
                if (!string.IsNullOrEmpty(dto.Notes)) shift.Notes = dto.Notes;
                if (dto.Status.HasValue) shift.Status = dto.Status.Value;

                await _unitOfWork.Shifts.UpdateAsync(shift);
                await _unitOfWork.SaveChangesAsync();

                // Atualizar as posições se fornecidas
                if (dto.Positions != null)
                {
                    // Remover posições existentes
                    var existingPositions = await _unitOfWork.ShiftPositions.FindAsync(sp => sp.ShiftId == id);
                    foreach (var position in existingPositions)
                    {
                        await _unitOfWork.ShiftPositions.DeleteAsync(position.Id);
                    }
                    await _unitOfWork.SaveChangesAsync();

                    // Adicionar novas posições
                    foreach (var positionDto in dto.Positions)
                    {
                        var position = await _unitOfWork.Positions.GetByIdAsync(positionDto.PositionId)
                            ?? throw new NotFoundException($"Position with ID {positionDto.PositionId} not found");

                        var shiftPosition = new ShiftPosition
                        {
                            ShiftId = shift.Id,
                            PositionId = positionDto.PositionId,
                            RequiredCount = positionDto.RequiredCount,
                            VolunteersCount = 0
                        };

                        await _unitOfWork.ShiftPositions.AddAsync(shiftPosition);
                    }
                    await _unitOfWork.SaveChangesAsync();
                }

                await _unitOfWork.CommitTransactionAsync();

                var updatedShift = await _unitOfWork.Shifts.GetWithPositionsAsync(shift.Id)
                    ?? throw new NotFoundException("Created shift could not be retrieved after commit");

                return MapToDto(updatedShift);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync();
                throw;
            }
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
                UpdatedAt = shift.UpdatedAt,
                Positions = shift.ShiftPositions?
                    .Select(sp => new ShiftPositionDto
                    {
                        Id = sp.Id,
                        PositionId = sp.PositionId,
                        PositionName = sp.Position?.Name ?? string.Empty,
                        RequiredCount = sp.RequiredCount,
                        VolunteersCount = sp.VolunteersCount
                    }).ToList() ?? [] // evitar null se nao tiver nada
            };
        }
    }
}
