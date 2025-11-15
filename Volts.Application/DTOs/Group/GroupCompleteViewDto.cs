using System;

namespace Volts.Application.DTOs.Group
{
    public class GroupCompleteViewDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string OrganizationId { get; set; } = string.Empty;
        public string OrganizationName { get; set; } = string.Empty;
        public string CreatedById { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int? TotalShiftsCount { get; set; }
        public int? UpcomingShiftsCount { get; set; }
        public string? Color { get; set; }
        public string? Icon { get; set; }
    }
}