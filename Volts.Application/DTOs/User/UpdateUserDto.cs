using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Volts.Application.DTOs.User
{
    public class UpdateUserDto
    {
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? Bio { get; set; }
    }
}
