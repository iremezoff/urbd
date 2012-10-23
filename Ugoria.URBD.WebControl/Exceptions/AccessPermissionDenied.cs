using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Principal;

namespace Ugoria.URBD.WebControl
{
    public class AccessPermissionDeniedException : HttpException
    {
        private string user;

        public string User
        {
            get { return user; }
        }

        public AccessPermissionDeniedException(IPrincipal user, string message)
            : base(message)
        {
            this.user = user.Identity.Name;
        }
    }
}