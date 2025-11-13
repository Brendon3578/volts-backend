using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volts.Application.DTOs.User;
using Volts.Application.Exceptions;
using Volts.Application.Interfaces;
using Volts.Domain.Entities;
using Volts.Domain.Interfaces;

namespace Volts.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _unitOfWork.Users.GetAllAsync();
            return users.Select(MapToDto);
        }

        public async Task<UserDto?> GetUserByIdAsync(string id)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);
            return user != null ? MapToDto(user) : null;
        }

        public async Task<UserDto?> GetUserByEmailAsync(string email)
        {
            var user = await _unitOfWork.Users.GetByEmailAsync(email);
            return user != null ? MapToDto(user) : null;
        }

        public async Task<UserDto> CreateUserAsync(CreateUserDto dto)
        {
            var user = new User
            {
                Email = dto.Email,
                Name = dto.Name,
                Phone = dto.Phone,
                Bio = dto.Bio
            };

            await _unitOfWork.Users.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(user);
        }

        public async Task<UserDto> UpdateUserAsync(string id, UpdateUserDto dto)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(id);

            if (user == null)
                throw new KeyNotFoundException($"User with ID {id} not found");

            if (dto.Name != null) user.Name = dto.Name;
            if (dto.Phone != null) user.Phone = dto.Phone;
            if (dto.Bio != null) user.Bio = dto.Bio;

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return MapToDto(user);
        }

        public async Task DeleteUserAsync(string id)
        {
            await _unitOfWork.Users.DeleteAsync(id);
            await _unitOfWork.SaveChangesAsync();
        }

        private UserDto MapToDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                Name = user.Name,
                Phone = user.Phone,
                Bio = user.Bio,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }

        public async Task<List<UserOrganizationGroupsDto>> GetUserOrganizationsAndGroupsAsync(string userId)
        {
            // Verificar se o usuário existe
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                throw new UserHasNotPermissionException("Usuário não encontrado");

            // Buscar todas as organizações do usuário
            var organizationMembers = await _unitOfWork.OrganizationMembers.GetByUserIdAsync(userId);
            
            var result = new List<UserOrganizationGroupsDto>();
            
            foreach (var orgMember in organizationMembers)
            {

                var organization = orgMember.Organization;

                if (organization == null) // gambiarra
                    await _unitOfWork.Organizations.GetByIdAsync(orgMember.OrganizationId);
                    
                if (organization == null) continue;
                
                var orgDto = new UserOrganizationGroupsDto
                {
                    OrganizationId = organization.Id,
                    OrganizationName = organization.Name,
                    OrganizationDescription = organization.Description ?? string.Empty,
                    OrganizationUserRole = orgMember.Role.ToString(),
                    Groups = new List<UserGroupMemberDto>()
                };
                
                // Buscar grupos do usuário nesta organização
                var groupMembers = await _unitOfWork.GroupMembers.GetByUserAndOrganizationAsync(userId, organization.Id);
                
                foreach (var groupMember in groupMembers)
                {
                    var group = groupMember.Group;

                    if (group == null) // gambiarra 2, na teoria nao precisa desse código, mas deixei por segurança
                        group = await _unitOfWork.Groups.GetByIdAsync(groupMember.GroupId);

                    if (group == null)
                        continue;
                    
                    
                    var groupDto = new UserGroupMemberDto
                    {
                        GroupId = group.Id,
                        GroupName = group.Name,
                        GroupDescription = group.Description ?? string.Empty,
                        MemberId = groupMember.Id,
                        MemberName = user.Name,
                        MemberRole = groupMember.Role.ToString(),
                    };
                    
                    orgDto.Groups.Add(groupDto);
                }
                
                result.Add(orgDto);
            }
            
            return result;
        }
    }
}
