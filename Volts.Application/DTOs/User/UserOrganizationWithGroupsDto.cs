using System.Collections.Generic;

namespace Volts.Application.DTOs.User
{
    public class UserOrganizationWithGroupsDto
    {
        public string OrganizationId { get; set; } = string.Empty;
        public string OrganizationName { get; set; } = string.Empty;
        public string OrganizationDescription { get; set; } = string.Empty;
        public string OrganizationUserRole { get; set; } = string.Empty;
        public List<SimpleGroupsDto> Groups { get; set; } = new List<SimpleGroupsDto>();
    }

    public class SimpleGroupsDto
    {
        public string GroupId { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;
        public string GroupDescription { get; set; } = string.Empty;
    }
}