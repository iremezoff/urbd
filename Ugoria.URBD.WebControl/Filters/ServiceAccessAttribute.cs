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
                throw new AccessPermissionDeniedException("Отсутствует доступ к сервису УРБД для пользователя" + HttpContext.Current.User.Identity.Name);

            // Проверка на права администратора
            if (isAdmin && !user.IsAdmin)
                throw new AccessPermissionDeniedException("Отсутствуют права администратора для пользователя " + HttpContext.Current.User.Identity.Name);

            //Проверка на доступ к сервису
            int entityId = 0;
            if (limitedAccess && !int.TryParse((string)filterContext.RouteData.Values["id"], out entityId))
                throw new HttpException(404, "Отсутствует ID");
            else if (limitedAccess &&
                entityType == typeof(IBase) && !SessionStore.IsAllowedBase(user, entityId) || (entityType == typeof(IService) && !SessionStore.IsAllowedService(user, entityId)))
                throw new AccessPermissionDeniedException("Недостаточно разрешений для доступа к запрошенному сервису у пользователя " + HttpContext.Current.User.Identity.Name);
            else if (limitedAccess && entityType != typeof(IBase) && entityType != typeof(IService))
                throw new AccessPermissionDeniedException("Разрешение не определено для пользователя " + HttpContext.Current.User.Identity.Name);

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