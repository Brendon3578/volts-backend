using System.Collections.Generic;

namespace Volts.Application.DTOs.Shift
{
    public class ShiftPositionCompleteViewDto
    {
        public string Id { get; set; } = string.Empty;
        public string PositionId { get; set; } = string.Empty;
        public string PositionName { get; set; } = string.Empty;
        public string PositionDescription { get; set; } = string.Empty;
        public int RequiredCount { get; set; }
        public int VolunteersCount { get; set; }
        public List<ShiftVolunteerDto> Volunteers { get; set; } = new();
    }
}