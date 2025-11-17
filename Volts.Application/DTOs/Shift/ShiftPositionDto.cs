using System;

namespace Volts.Application.DTOs.Shift
{
    public class ShiftPositionDto
    {
        public string Id { get; set; } = string.Empty;
        public string PositionId { get; set; } = string.Empty;
        public int RequiredCount { get; set; } = 1;
        public int VolunteersCount { get; set; } = 0;
        public string PositionName { get; set; } = string.Empty;
        public string PositionDescription { get; set; } = string.Empty;
    }
}