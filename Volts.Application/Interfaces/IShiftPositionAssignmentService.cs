using System.Collections.Generic;
using System.Threading.Tasks;
using Volts.Application.DTOs.ShiftPositionAssignment;

namespace Volts.Application.Interfaces
{
    public interface IShiftPositionAssignmentService
    {
        Task<IEnumerable<ShiftPositionAssignmentDto>> GetByShiftIdAsync(string shiftId, string userId);
        Task<IEnumerable<ShiftPositionAssignmentDto>> GetByShiftPositionIdAsync(string shiftPositionId, string userId);
        Task<ShiftPositionAssignmentDto> GetByIdAsync(string id, string userId);
        Task<ShiftPositionAssignmentDto> ApplyAsync(string shiftPositionId, string userId, CreateShiftPositionAssignmentDto dto);
        Task<ShiftPositionAssignmentDto> ConfirmAsync(string id, string userId);
        Task<ShiftPositionAssignmentDto> CancelAsync(string id, string userId);
        Task DeleteAsync(string id, string userId);
    }
}