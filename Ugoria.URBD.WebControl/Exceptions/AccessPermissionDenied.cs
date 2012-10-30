using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Principal;

namespace Ugoria.URBD.WebControl
{
    public class AccessPermissionDeniedException : HttpException
    {
        public AccessPermissionDeniedException(string message)
            : base(message)
        {
        }
    }
}