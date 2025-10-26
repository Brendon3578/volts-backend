using System.Collections.Generic;
using System.Threading.Tasks;
using Volts.Application.DTOs.Position;

namespace Volts.Application.Interfaces
{
    public interface IPositionService
    {
        Task<IEnumerable<PositionDto>> GetByGroupIdAsync(string groupId);
        Task<PositionDto> GetByIdAsync(string id);
        Task<PositionDto> CreateAsync(CreatePositionDto dto, string userId);
        Task<PositionDto> UpdateAsync(string id, UpdatePositionDto dto, string userId);
        Task DeleteAsync(string id, string userId);
    }
}
