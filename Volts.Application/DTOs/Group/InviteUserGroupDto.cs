using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volts.Domain.Enums;

namespace Volts.Application.DTOs.Group
{
    public class InviteUserGroupDto
    {
        public string InvitedEmail { get; set; } = string.Empty;
        public GroupRoleEnum InviterRole;
    }
}
