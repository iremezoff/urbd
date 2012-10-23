using System;
using System.Web;
using System.Web.Mvc;
using Ugoria.URBD.WebControl.Helpers;
using Ugoria.URBD.WebControl.ViewModels;
using Ugoria.URBD.WebControl.Models;

namespace Ugoria.URBD.WebControl
{
    public class ServiceAccessAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            IUser user = SessionStore.GetCurrentUser();
            // Проверка на доступ к сервису
            if (user == null)
                throw new AccessPermissionDeniedException(HttpContext.Current.User, "Отсутствует доступ к сервису УРБД");

            // Проверка на права администратора
            if (isAdmin && !user.IsAdmin)
                throw new AccessPermissionDeniedException(HttpContext.Current.User, "Отсутствуют права администратора");

            //Проверка на доступ к сервису
            int entityId = 0;
            if (limitedAccess && !int.TryParse((string)filterContext.RouteData.Values["id"], out entityId))
                throw new AccessPermissionDeniedException(HttpContext.Current.User, "Отсутствует ID");
            else if (limitedAccess &&
                entityType == typeof(IBase) && !SessionStore.IsAllowedBase(user, entityId) || (entityType == typeof(IService) && !SessionStore.IsAllowedBase(user, entityId)))
                throw new AccessPermissionDeniedException(HttpContext.Current.User, "Недостаточно разрешений для доступа к запрошенному сервису");
            else if (limitedAccess && entityType != typeof(IBase) && entityType != typeof(IService))
                throw new AccessPermissionDeniedException(HttpContext.Current.User, "Разрешение не определено");

            base.OnActionExecuting(filterContext);
        }

        private bool isAdmin = false;

        public bool IsAdmin
        {
            get { return isAdmin; }
            set { isAdmin = value; }
        }

        private bool limitedAccess = false;

        public bool LimitedAccess
        {
            get { return limitedAccess; }
            set { limitedAccess = value; }
        }

        private Type entityType = typeof(void);

        public ServiceAccessAttribute(Type entityType)
            : base()
        {
            this.entityType = entityType;
        }

        public ServiceAccessAttribute()
            : base()
        { }
    }
}