using System;
using System.Collections.Generic;
using Volts.Domain.Enums;

namespace Volts.Application.DTOs.Shift
{
    public class ShiftCompleteViewDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string GroupId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<ShiftPositionCompleteViewDto> Positions { get; set; } = new();
    }
}