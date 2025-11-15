using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volts.Application.DTOs.User;

namespace Volts.Application.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<UserDto?> GetUserByIdAsync(string id);
        Task<UserDto?> GetUserByEmailAsync(string email);
        Task<UserDto> CreateUserAsync(CreateUserDto dto);
        Task<UserDto> UpdateUserAsync(string id, UpdateUserDto dto);
        Task DeleteUserAsync(string id);
        Task<List<UserOrganizationWithGroupsDto>> GetUserOrganizationsAndGroupsAsync(string userId);
    }
}
