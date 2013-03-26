using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Principal;
using Ugoria.URBD.WebControl.Models;
using Ugoria.URBD.WebControl.ViewModels;

namespace Ugoria.URBD.WebControl.Helpers
{
    public static class SessionStore
    {
        public static bool IsAllowedBase(IUser user, int baseId, bool isChange = false)
        {
            if (user.IsAdmin)
                return true;
            return user.BasePermissions.Any(bp => bp.EntityId == baseId && (!isChange || isChange==bp.AllowConfigure));
        }

        public static bool IsAllowedService(IUser user, int serviceId, bool isChange = false)
        {
            if (user.IsAdmin)
                return true;
            return user.ServicePermissions.Any(bp => bp.EntityId == serviceId && (!isChange || isChange == bp.AllowConfigure));
        }

        public static IUser GetCurrentUser()
        {
            if (HttpContext.Current.Session["current_user"] != null)
                return (IUser)HttpContext.Current.Session["current_user"];
            IUser user = GetCurrentUser(HttpContext.Current.User.Identity.Name);
            HttpContext.Current.Session["current_user"] = user;
            return user;
        }

        public static IUser GetCurrentUser(string username)
        {            
            URBD2Entities dataContext = new URBD2Entities();
            IUsersRepository userRepo = new UsersRepository(dataContext);
            IUser user = userRepo.GetUserByName(username);
            return user;
        }
    }
}