using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Volts.Application.DTOs.Common
{
    public class ErrorMessageDto
    {
        public string message { get; set; } = string.Empty;
        public IEnumerable<string>? errors { get; set; } = null;
    }
}
