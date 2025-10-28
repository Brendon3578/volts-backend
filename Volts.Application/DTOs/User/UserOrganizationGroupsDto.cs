using System.Collections.Generic;

namespace Volts.Application.DTOs.User
{
    public class UserOrganizationGroupsDto
{
    public string OrganizationId { get; set; } = string.Empty;
    public string OrganizationName { get; set; } = string.Empty;
    public string OrganizationDescription { get; set; } = string.Empty;
    public string OrganizationUserRole { get; set; } = string.Empty;
    public List<UserGroupMemberDto> Groups { get; set; } = new List<UserGroupMemberDto>();
}

    public class UserGroupMemberDto
    {
        public string GroupId { get; set; } = string.Empty;
        public string GroupName { get; set; } = string.Empty;
        public string GroupDescription { get; set; } = string.Empty;
        public string MemberId { get; set; } = string.Empty;
        public string MemberName { get; set; } = string.Empty;
        public string MemberRole { get; set; } = string.Empty;
    }
}