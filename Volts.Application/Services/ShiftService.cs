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

            var shifts = await _unitOfWork.Shifts.GetByGroupIdWithPositionsAndShiftPositionAsync(groupId);

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


            if (await IsAdminOrLeader(userId, group) == false)
                throw new UserHasNotPermissionException("User is not leader or coordinator");


            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var shift = new Shift
                {
                    StartDate = dto.StartDate,
                    EndDate = dto.EndDate,
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

            var group = await _unitOfWork.Groups.GetByIdAsync(shift.GroupId)
                ?? throw new NotFoundException("Group not found");

            if (await IsAdminOrLeader(userId, group) == false)
                throw new UserHasNotPermissionException("User is not leader or coordinator");

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                if (dto.StartDate.HasValue) shift.StartDate = dto.StartDate.Value;
                if (dto.EndDate.HasValue) shift.EndDate = dto.EndDate.Value;
                if (!string.IsNullOrEmpty(dto.Title)) shift.Title = dto.Title;
                if (!string.IsNullOrEmpty(dto.Notes)) shift.Notes = dto.Notes;

                shift.Status = ShiftStatusEnum.OPEN; // abrir novamente

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

            var group = await _unitOfWork.Groups.GetByIdAsync(shift.GroupId)
                ?? throw new NotFoundException("Group not found");

            if (await IsAdminOrLeader(userId, group) == false)
                throw new UserHasNotPermissionException("User is not leader or admin");

            await _unitOfWork.Shifts.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }

        private async Task<bool> UserHasPermissionAsync(string userId, Group group, IEnumerable<OrganizationRoleEnum> allowedRoles)
        {
            var memberShip = await _unitOfWork.OrganizationMembers.GetMembershipAsync(userId, group.OrganizationId);
            ;
            return memberShip != null && allowedRoles.Contains(memberShip.Role);
        }

        private Task<bool> IsAdminOrLeader(string userId, Group group)
        {
            return UserHasPermissionAsync(userId, group, [OrganizationRoleEnum.LEADER, OrganizationRoleEnum.ADMIN]);
        }

        private static ShiftDto MapToDto(Shift shift)
        {
            return new ShiftDto
            {
                Id = shift.Id,
                StartDate = shift.StartDate,
                EndDate = shift.EndDate,
                Title = shift.Title,
                Notes = shift.Notes,
                Status = shift.Status.ToString(),
                GroupId = shift.GroupId,
                CreatedAt = shift.CreatedAt,
                UpdatedAt = shift.UpdatedAt,
                Positions = shift.ShiftPositions?
                    .Select(sp => new ShiftPositionDto
                    {
                        Id = sp.Id,
                        PositionId = sp.PositionId,
                        PositionName = sp.Position?.Name ?? string.Empty,
                        PositionDescription = sp.Position?.Description ?? string.Empty,
                        RequiredCount = sp.RequiredCount,
                        VolunteersCount = sp.Volunteers?.Count(v => v.Status == VolunteerStatusEnum.CONFIRMED || v.Status == VolunteerStatusEnum.PENDING) ?? 0,
                    }).ToList() ?? [] // evitar null se nao tiver nada
            };
        }

        public async Task<ShiftCompleteViewDto> GetCompleteViewAsync(string shiftId, string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new UserHasNotPermissionException("User is not authenticated");

            var shift = await _unitOfWork.Shifts.GetShiftCompleteViewAsync(shiftId)
                ?? throw new NotFoundException("Shift not found");

            var group = await _unitOfWork.Groups.GetByIdAsync(shift.GroupId)
                ?? throw new NotFoundException("Group not found");

            var membership = await _unitOfWork.OrganizationMembers.GetMembershipAsync(userId, group.OrganizationId);
            if (membership == null)
                throw new UserHasNotPermissionException("User does not belong to the organization");

            var dto = new ShiftCompleteViewDto
            {
                Id = shift.Id,
                Title = shift.Title ?? string.Empty,
                Notes = shift.Notes,
                StartDate = shift.StartDate,
                EndDate = shift.EndDate,
                Status = shift.Status.ToString(),
                GroupId = shift.GroupId,
                CreatedAt = shift.CreatedAt,
                UpdatedAt = shift.UpdatedAt,
                Positions = shift.ShiftPositions?
                    .Select(sp => new ShiftPositionCompleteViewDto
                    {
                        Id = sp.Id,
                        PositionId = sp.PositionId,
                        PositionName = sp.Position?.Name ?? string.Empty,
                        PositionDescription = sp.Position?.Description ?? string.Empty,
                        RequiredCount = sp.RequiredCount,
                        VolunteersCount = sp.Volunteers?.Count(v => v.Status == VolunteerStatusEnum.CONFIRMED || v.Status == VolunteerStatusEnum.PENDING) ?? 0,
                        Volunteers = sp.Volunteers?
                            .Select(a => new ShiftVolunteerDto
                            {
                                Id = a.Id,
                                UserName = a.User?.Name ?? string.Empty,
                                UserEmail = a.User?.Email ?? string.Empty,
                                UserId = a.User?.Id ?? string.Empty,
                                Notes = a.Notes,
                                Status = a.Status.ToString()
                            }).ToList() ?? new List<ShiftVolunteerDto>()
                    }).ToList() ?? new List<ShiftPositionCompleteViewDto>()
            };

            return dto;
        }

        public async Task UpdateShiftStatusAsync(string id, UpdateShiftStatusDto dto, string userId)
        {
            var shift = await _unitOfWork.Shifts.GetByIdAsync(id)
                ?? throw new NotFoundException("Shift not found");

            var group = await _unitOfWork.Groups.GetByIdAsync(shift.GroupId)
                ?? throw new NotFoundException("Group not found");

            if (await IsAdminOrLeader(userId, group) == false)
                throw new UserHasNotPermissionException("User is not leader or coordinator");

            if (!Enum.TryParse<ShiftStatusEnum>(dto.NewStatus, true, out var shiftStatus))
                throw new ArgumentException("Invalid status provided");

            shift.Status = shiftStatus;

            await _unitOfWork.Shifts.UpdateAsync(shift);

            await _unitOfWork.SaveChangesAsync();
        }
    }
}
