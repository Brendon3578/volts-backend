using System;
using Volts.Domain.Enums;

namespace Volts.Application.DTOs.Group
{
    public class GroupMemberDto
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string GroupId { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime JoinedAt { get; set; }
    }
}
