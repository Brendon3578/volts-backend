using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Volts.Application.Exceptions
{
    public class UserHasNotPermissionException : Exception
    {
        public UserHasNotPermissionException() : base() { }
        public UserHasNotPermissionException(string message) : base(message) { }
        public UserHasNotPermissionException(string message, Exception inner) : base(message, inner) { }
    }
}
