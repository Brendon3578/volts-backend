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

        public async Task<PositionDto> GetByIdAsync(string id)
        {
            var position = await _unitOfWork.Positions.GetByIdAsync(id)
                ?? throw new NotFoundException("Position not found");

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
            // Buscar a posição
            var position = await _unitOfWork.Positions.GetByIdAsync(id)
                ?? throw new NotFoundException("Position not found");

            // Buscar grupo ao qual a posição pertence
            var group = await _unitOfWork.Groups.GetByIdAsync(position.GroupId)
                ?? throw new NotFoundException("Group not found");

            // Buscar a membership do usuário dentro da ORGANIZAÇÃO do grupo
            var membership = await _unitOfWork.OrganizationMembers
                .GetMembershipAsync(userId, group.OrganizationId)
                ?? throw new UserHasNotPermissionException("User is not a member of the organization");

            // Validação de permissão — apenas ADMIN e LEADER
            if (!UserHasPermissionToManagePositions(membership.Role))
                throw new UserHasNotPermissionException("Insufficient role to update the position");

            // Atualizar os campos permitidos
            if (!string.IsNullOrWhiteSpace(dto.Name))
                position.Name = dto.Name;

            if (dto.Description != null)
                position.Description = dto.Description;

            // Atualizar a posição
            await _unitOfWork.Positions.UpdateAsync(position);
            await _unitOfWork.SaveChangesAsync();

            // Retornar DTO atualizado
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

        public async Task DeleteAsync(string positionId, string performedByUserId)
        {
            // Buscar posição
            var position = await _unitOfWork.Positions.GetByIdAsync(positionId)
                ?? throw new NotFoundException("Position not found");

            // Buscar grupo correto (usando position.GroupId!)
            var group = await _unitOfWork.Groups.GetByIdAsync(position.GroupId)
                ?? throw new NotFoundException("Group not found");

            // Verificar se o usuário pertence à ORGANIZAÇÃO
            var membership = await _unitOfWork.OrganizationMembers
                .GetMembershipAsync(performedByUserId, group.OrganizationId)
                ?? throw new UserHasNotPermissionException("User is not part of this organization");

            // Checar permissões
            if (!UserHasPermissionToManagePositions(membership.Role))
                throw new UserHasNotPermissionException("Insufficient permissions to delete a position");

            // Remover posição
            await _unitOfWork.Positions.DeleteAsync(positionId);

            // Commit padrão do UnitOfWork
            await _unitOfWork.SaveChangesAsync();
        }

        private static bool UserHasPermissionToManagePositions(OrganizationRoleEnum role)
        {
            return role is OrganizationRoleEnum.ADMIN or OrganizationRoleEnum.LEADER;
        }
    }
}
