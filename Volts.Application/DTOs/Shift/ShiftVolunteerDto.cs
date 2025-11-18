using Volts.Domain.Enums;

namespace Volts.Application.DTOs.Shift
{
    public class ShiftVolunteerDto
    {
        public string Id { get; set; } = string.Empty; // assignment id
        public string UserName { get; set; } = string.Empty;
        public string UserEmail { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public string? Status { get; set; } = string.Empty;
    }
}