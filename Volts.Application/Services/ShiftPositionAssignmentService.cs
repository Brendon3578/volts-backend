using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volts.Application.DTOs.ShiftPositionAssignment;
using Volts.Application.Exceptions;
using Volts.Application.Interfaces;
using Volts.Domain.Entities;
using Volts.Domain.Enums;
using Volts.Domain.Interfaces;

namespace Volts.Application.Services
{
    public class ShiftPositionAssignmentService : IShiftPositionAssignmentService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ShiftPositionAssignmentService(
            IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<ShiftPositionAssignmentDto>> GetByShiftIdAsync(string shiftId, string userId)
        {
            var shift = await _unitOfWork.Shifts.GetByIdAsync(shiftId);
            if (shift == null)
                throw new NotFoundException("Turno não encontrado");

            // Verificar se o usuário é membro do grupo
            var memberShip = await _unitOfWork.GroupMembers.GetMembershipAsync(userId, shift.GroupId)
                ?? throw new UserHasNotPermissionException("Você não tem permissão para visualizar as inscrições deste turno");

            // Buscar todas as posições do turno
            var shiftPositions = await _unitOfWork.ShiftPositions.GetByShiftIdAsync(shiftId);
            
            // Buscar todas as inscrições para essas posições
            var assignments = new List<ShiftPositionAssignment>();
            foreach (var position in shiftPositions)
            {
                var positionAssignments = await _unitOfWork.ShiftPositionAssignment.GetByShiftPositionIdAsync(position.Id);
                assignments.AddRange(positionAssignments);
            }

            return assignments.Select(MapToDto);
        }

        public async Task<IEnumerable<ShiftPositionAssignmentDto>> GetByShiftPositionIdAsync(string shiftPositionId, string userId)
        {
            var shiftPosition = await _unitOfWork.ShiftPositions.GetByIdAsync(shiftPositionId);
            if (shiftPosition == null)
                throw new NotFoundException("Posição de turno não encontrada");

            var shift = await _unitOfWork.Shifts.GetByIdAsync(shiftPosition.ShiftId);
            if (shift == null)
                throw new NotFoundException("Turno não encontrado");

            // Verificar se o usuário é membro do grupo
            var memberShip = await _unitOfWork.GroupMembers.GetMembershipAsync(userId, shift.GroupId)
                ?? throw new UserHasNotPermissionException("Você não tem permissão para visualizar as inscrições deste turno");

            var assignments = await _unitOfWork.ShiftPositionAssignment.GetByShiftPositionIdAsync(shiftPositionId);
            return assignments.Select(MapToDto);
        }

        public async Task<ShiftPositionAssignmentDto> GetByIdAsync(string id, string userId)
        {
            var assignment = await _unitOfWork.ShiftPositionAssignment.GetByIdAsync(id);
            if (assignment == null)
                throw new NotFoundException("Inscrição não encontrada");

            var shiftPosition = await _unitOfWork.ShiftPositions.GetByIdAsync(assignment.ShiftPositionId);
            if (shiftPosition == null)
                throw new NotFoundException("Posição de turno não encontrada");

            var shift = await _unitOfWork.Shifts.GetByIdAsync(shiftPosition.ShiftId);
            if (shift == null)
                throw new NotFoundException("Turno não encontrado");

            // Verificar se o usuário é membro do grupo ou é o próprio voluntário
            var membership = await _unitOfWork.GroupMembers.GetMembershipAsync(userId, shift.GroupId);
            var isVolunteer = assignment.UserId == userId;
            
            if (membership == null && isVolunteer == false)
                throw new UserHasNotPermissionException("Você não tem permissão para visualizar esta inscrição");

            return MapToDto(assignment);
        }

        public async Task<ShiftPositionAssignmentDto> ApplyAsync(string shiftPositionId, string userId, CreateShiftPositionAssignmentDto dto)
        {
            var shiftPosition = await _unitOfWork.ShiftPositions.GetByIdAsync(shiftPositionId);
            if (shiftPosition == null)
                throw new NotFoundException("Posição de turno não encontrada");

            var shift = await _unitOfWork.Shifts.GetByIdAsync(shiftPosition.ShiftId);
            if (shift == null)
                throw new NotFoundException("Turno não encontrado");

            // Verificar se o usuário é membro do grupo
            var memberShip = await _unitOfWork.GroupMembers.GetMembershipAsync(userId, shift.GroupId)
                ?? throw new UserHasNotPermissionException("Você não tem permissão para visualizar as inscrições deste turno");

            // Verificar se o usuário já se inscreveu para esta posição
            var existingApplication = await _unitOfWork.ShiftPositionAssignment.GetVolunteerApplicationAsync(userId, shiftPositionId);
            if (existingApplication != null)
                throw new InvalidOperationException("Você já se inscreveu para esta posição");

            // Verificar se o turno já está completo
            var assignments = await _unitOfWork.ShiftPositionAssignment.GetByShiftPositionIdAsync(shiftPositionId);
            var confirmedAssignments = assignments.Count(a => a.Status == VolunteerStatusEnum.CONFIRMED);
            
            if (confirmedAssignments >= shiftPosition.RequiredCount)
                throw new InvalidOperationException("Esta posição já está com todas as vagas preenchidas");

            // Criar a inscrição
            var assignment = new ShiftPositionAssignment
            {
                UserId = userId,
                ShiftPositionId = shiftPositionId,
                Status = VolunteerStatusEnum.PENDING,
                Notes = dto.Notes,
                AppliedAt = DateTime.UtcNow
            };

            await _unitOfWork.ShiftPositionAssignment.AddAsync(assignment);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(assignment);
        }

        public async Task<ShiftPositionAssignmentDto> ConfirmAsync(string id, string userId)
        {
            var assignment = await _unitOfWork.ShiftPositionAssignment.GetByIdAsync(id);
            if (assignment == null)
                throw new NotFoundException("Inscrição não encontrada");

            var shiftPosition = await _unitOfWork.ShiftPositions.GetByIdAsync(assignment.ShiftPositionId);
            if (shiftPosition == null)
                throw new NotFoundException("Posição de turno não encontrada");

            var shift = await _unitOfWork.Shifts.GetByIdAsync(shiftPosition.ShiftId);
            if (shift == null)
                throw new NotFoundException("Turno não encontrado");

            // Verificar se o usuário é coordenador ou líder do grupo
            var member = await _unitOfWork.GroupMembers.GetMembershipAsync(userId, shift.GroupId);

            if (member == null || (member.Role != GroupRoleEnum.COORDINATOR && member.Role != GroupRoleEnum.GROUP_LEADER))
                throw new UserHasNotPermissionException("Você não tem permissão para confirmar inscrições");

            // Verificar se a inscrição já está confirmada
            if (assignment.Status == VolunteerStatusEnum.CONFIRMED)
                throw new InvalidOperationException("Esta inscrição já está confirmada");

            // Verificar se o turno já está completo
            var assignments = await _unitOfWork.ShiftPositionAssignment.GetByShiftPositionIdAsync(assignment.ShiftPositionId);
            var confirmedAssignments = assignments.Count(a => a.Status == VolunteerStatusEnum.CONFIRMED);
            
            if (confirmedAssignments >= shiftPosition.RequiredCount)
                throw new InvalidOperationException("Esta posição já está com todas as vagas preenchidas");

            // Confirmar a inscrição
            assignment.Status = VolunteerStatusEnum.CONFIRMED;
            assignment.ConfirmedAt = DateTime.UtcNow;

            await _unitOfWork.ShiftPositionAssignment.UpdateAsync(assignment);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(assignment);
        }

        public async Task<ShiftPositionAssignmentDto> CancelAsync(string id, string userId)
        {
            var assignment = await _unitOfWork.ShiftPositionAssignment.GetByIdAsync(id);
            if (assignment == null)
                throw new NotFoundException("Inscrição não encontrada");

            var shiftPosition = await _unitOfWork.ShiftPositions.GetByIdAsync(assignment.ShiftPositionId);
            if (shiftPosition == null)
                throw new NotFoundException("Posição de turno não encontrada");

            var shift = await _unitOfWork.Shifts.GetByIdAsync(shiftPosition.ShiftId);
            if (shift == null)
                throw new NotFoundException("Turno não encontrado");

            // Verificar se o usuário é o próprio voluntário ou coordenador/líder do grupo
            var isVolunteer = assignment.UserId == userId;
            var member = await _unitOfWork.GroupMembers.GetMembershipAsync(userId, shift.GroupId);
            var isCoordinatorOrLeader = member != null && (member.Role == GroupRoleEnum.COORDINATOR || member.Role == GroupRoleEnum.GROUP_LEADER);
            
            if (!isVolunteer && !isCoordinatorOrLeader)
                throw new UserHasNotPermissionException("Você não tem permissão para cancelar esta inscrição");

            // Verificar se a inscrição já está cancelada
            if (assignment.Status == VolunteerStatusEnum.CANCELED)
                throw new InvalidOperationException("Esta inscrição já está cancelada");

            // Cancelar a inscrição
            assignment.Status = VolunteerStatusEnum.CANCELED;
            assignment.RejectedAt = DateTime.UtcNow;

            await _unitOfWork.ShiftPositionAssignment.UpdateAsync(assignment);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(assignment);
        }

        public async Task DeleteAsync(string id, string userId)
        {
            var assignment = await _unitOfWork.ShiftPositionAssignment.GetByIdAsync(id);
            if (assignment == null)
                throw new NotFoundException("Inscrição não encontrada");

            var shiftPosition = await _unitOfWork.ShiftPositions.GetByIdAsync(assignment.ShiftPositionId);
            if (shiftPosition == null)
                throw new NotFoundException("Posição de turno não encontrada");

            var shift = await _unitOfWork.Shifts.GetByIdAsync(shiftPosition.ShiftId);
            if (shift == null)
                throw new NotFoundException("Turno não encontrado");

            // Verificar se o usuário é coordenador ou líder do grupo
            var member = await _unitOfWork.GroupMembers.GetMembershipAsync(userId, shift.GroupId);

            if (member == null || (member.Role != GroupRoleEnum.COORDINATOR && member.Role != GroupRoleEnum.GROUP_LEADER))
                throw new UserHasNotPermissionException("Você não tem permissão para remover inscrições");

            await _unitOfWork.ShiftPositionAssignment.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }

        private ShiftPositionAssignmentDto MapToDto(ShiftPositionAssignment assignment)
        {
            return new ShiftPositionAssignmentDto
            {
                Id = assignment.Id,
                UserId = assignment.UserId,
                UserName = assignment.User?.Name ?? string.Empty,
                ShiftPositionId = assignment.ShiftPositionId,
                PositionName = assignment.ShiftPosition?.Position?.Name ?? string.Empty,
                Status = assignment.Status,
                Notes = assignment.Notes,
                AppliedAt = assignment.AppliedAt,
                ConfirmedAt = assignment.ConfirmedAt,
                RejectedAt = assignment.RejectedAt,
                CreatedAt = assignment.CreatedAt,
                UpdatedAt = assignment.UpdatedAt
            };
        }
    }
}