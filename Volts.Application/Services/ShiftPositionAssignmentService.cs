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

            // aqui nao precisa por enquanto de validação por role
            // await TryGetOrganizationMembership(shift, userId);

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

            // var shift = await _unitOfWork.Shifts.GetByIdAsync(shiftPosition.ShiftId);
            // if (shift == null)
            //     throw new NotFoundException("Turno não encontrado");
            // 
            // aqui nao precisa por enquanto de validação por role
            // await TryGetOrganizationMembership(shift, userId);

            var assignments = await _unitOfWork.ShiftPositionAssignment.GetByShiftPositionIdAsync(shiftPositionId);
            return assignments.Select(MapToDto);
        }

        public async Task<ShiftPositionAssignmentDto> GetByIdAsync(string id, string userId)
        {
            var assignment = await _unitOfWork.ShiftPositionAssignment.GetByIdAsync(id)
                ?? throw new NotFoundException("Inscrição não encontrada");

            //var shiftPosition = await _unitOfWork.ShiftPositions.GetByIdAsync(assignment.ShiftPositionId);
            //if (shiftPosition == null)
            //    throw new NotFoundException("Posição de turno não encontrada");
            //
            //var shift = await _unitOfWork.Shifts.GetByIdAsync(shiftPosition.ShiftId);
            //if (shift == null)
            //    throw new NotFoundException("Turno não encontrado");

            // Verificar se o usuário é membro do grupo ou é o próprio voluntário
            //var membership = await _unitOfWork.GroupMembers.GetMembershipAsync(userId, shift.GroupId);

            return MapToDto(assignment);
        }

        public async Task<ShiftPositionAssignmentDto> ApplyAsync(string shiftPositionId, string userId, CreateShiftPositionAssignmentDto dto)
        {
            var shiftPosition = await _unitOfWork.ShiftPositions.GetByIdAsync(shiftPositionId)
                ?? throw new NotFoundException("Posição de turno não encontrada");

            var shift = await _unitOfWork.Shifts.GetByIdAsync(shiftPosition.ShiftId)
                ??throw new NotFoundException("Turno não encontrado");


            var memberShip = await TryGetOrganizationMembership(shift, userId); // Verificar se o usuário é membro do grupo
            
            // Verificar se o usuário já se inscreveu para esta posição
            var existingApplication = await _unitOfWork.ShiftPositionAssignment.GetVolunteerApplicationAsync(userId, shiftPositionId);

            var isAlreadyApplied = existingApplication != null;

            if (isAlreadyApplied)
                throw new InvalidOperationException("Você já se inscreveu para esta posição");

            // Verificar se o turno já está completo
            var assignments = await _unitOfWork.ShiftPositionAssignment.GetByShiftPositionIdAsync(shiftPositionId);

            // 1. aqui verifica as que já estão confirmadas
            var confirmedAssignments = assignments.Count(a => a.Status == VolunteerStatusEnum.CONFIRMED);
            if (confirmedAssignments >= shiftPosition.RequiredCount)
                throw new InvalidOperationException("Esta posição já está com todas as vagas preenchidas");


            // 2. aqui se atingir o máximo, já bloqueia
            if (assignments.Count() >= shiftPosition.RequiredCount)
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
            var assignment = await _unitOfWork.ShiftPositionAssignment.GetByIdAsync(id)
                ?? throw new NotFoundException("Inscrição não encontrada");


            var shiftPosition = await _unitOfWork.ShiftPositions.GetByIdAsync(assignment.ShiftPositionId)
                ?? throw new NotFoundException("Posição de turno não encontrada");

            var shift = await _unitOfWork.Shifts.GetByIdAsync(shiftPosition.ShiftId)
                ?? throw new NotFoundException("Turno não encontrado");


            // Verificar se a inscrição já está confirmada
            if (assignment.Status == VolunteerStatusEnum.CONFIRMED)
                throw new InvalidOperationException("Esta inscrição já está confirmada");

            // aqui, regra de negócio que verifica se todas as aplicações já foram confirmadas, porém ignorei pra facilitar o uso
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

            var assignment = await _unitOfWork.ShiftPositionAssignment.GetByIdAsync(id)
                ?? throw new NotFoundException("Inscrição não encontrada");

            // Verificar se a inscrição já está cancelada
            if (assignment.Status == VolunteerStatusEnum.CANCELLED)
                throw new InvalidOperationException("Esta inscrição já está cancelada");

            var shiftPosition = await _unitOfWork.ShiftPositions.GetByIdAsync(assignment.ShiftPositionId)
                ?? throw new NotFoundException("Posição de turno não encontrada");

            var shift = await _unitOfWork.Shifts.GetByIdAsync(shiftPosition.ShiftId)
                ?? throw new NotFoundException("Turno não encontrado");

            // Verificar se o usuário é o próprio voluntário ou coordenador/líder do grupo
            var isUserCreator = assignment.UserId == userId;

            var member = await TryGetOrganizationMembership(shift, userId);

            var isLeaderOrAdmin = member.Role == OrganizationRoleEnum.ADMIN || member.Role == OrganizationRoleEnum.LEADER;
            
            if (!isUserCreator && !isLeaderOrAdmin)
                throw new UserHasNotPermissionException("Você não tem permissão para cancelar esta inscrição");



            // Cancelar a inscrição
            assignment.Status = VolunteerStatusEnum.CANCELLED;
            assignment.RejectedAt = DateTime.UtcNow;

            await _unitOfWork.ShiftPositionAssignment.UpdateAsync(assignment);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(assignment);
        }

        public async Task DeleteAsync(string id, string userId)
        {
            var assignment = await _unitOfWork.ShiftPositionAssignment.GetByIdAsync(id)
                ??throw new NotFoundException("Inscrição não encontrada");

            var shiftPosition = await _unitOfWork.ShiftPositions.GetByIdAsync(assignment.ShiftPositionId)
                ?? throw new NotFoundException("Posição de turno não encontrada");

            var shift = await _unitOfWork.Shifts.GetByIdAsync(shiftPosition.ShiftId)
                ?? throw new NotFoundException("Turno não encontrado");

            // Verificar se o usuário é coordenador ou líder do grupo ou criador da própria aplicação
            var isUserCreator = assignment.UserId == userId;

            var member = await TryGetOrganizationMembership(shift, userId);

            var isLeaderOrAdmin = member.Role == OrganizationRoleEnum.ADMIN || member.Role == OrganizationRoleEnum.LEADER;

            if (!isUserCreator && !isLeaderOrAdmin)
                throw new UserHasNotPermissionException("Você não tem permissão para cancelar esta inscrição");


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


        private async Task<OrganizationMember> TryGetOrganizationMembership(Shift shift, string userId)
        {
            var group = await _unitOfWork.Groups.GetByIdAsync(shift.GroupId)
                ?? throw new NotFoundException("Group not found!");

            var memberShip = await _unitOfWork.OrganizationMembers.GetMembershipAsync(userId, group.OrganizationId)
                ?? throw new NotFoundException("User is not member of this organization");

            return memberShip;
        }
    }
}