using System;
using Volts.Domain.Enums;

namespace Volts.Application.DTOs.ShiftPositionAssignment
{
    public class ShiftPositionAssignmentDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string ShiftPositionId { get; set; } = string.Empty;
        public string PositionName { get; set; } = string.Empty;
        public VolunteerStatusEnum Status { get; set; }
        public string? Notes { get; set; }
        public DateTime AppliedAt { get; set; }
        public DateTime? ConfirmedAt { get; set; }
        public DateTime? RejectedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}