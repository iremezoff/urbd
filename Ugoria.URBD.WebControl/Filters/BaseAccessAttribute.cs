using System;
using System.Web;
using System.Web.Mvc;
using Ugoria.URBD.WebControl.Helpers;
using Ugoria.URBD.WebControl.ViewModels;
using Ugoria.URBD.WebControl.Models;

namespace Ugoria.URBD.WebControl
{
    public class SecurityAccessAttribute : ActionFilterAttribute
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

            if (!isAdmin)
            {
                filterContext.Controller.ViewData["IsAdmin"] = false;
                // Проверка на доступ к управлению ИБ
                int entityId = 0;
                if (!int.TryParse((string)filterContext.RouteData.Values["id"], out entityId))
                    throw new HttpException(404, "Отсутствует ID");
                bool allow = false;

                if ((entityType == typeof(IBase) && !(allow = SessionStore.IsAllowedBase(user, entityId, isChange))))
                    throw new AccessPermissionDeniedException("Недостаточно разрешений для доступа к запрошенной ИБ у пользователя " + user.UserName);
                else if ((entityType == typeof(IService) && !(allow = SessionStore.IsAllowedService(user, entityId, isChange))))
                    throw new AccessPermissionDeniedException("Недостаточно разрешений для доступа к запрошенному сервису у пользователя " + user.UserName);
                else if (entityType != typeof(IBase) && entityType != typeof(IService))
                    throw new AccessPermissionDeniedException("Разрешение не определено для пользователя " + user.UserName);
                filterContext.Controller.ViewData["AllowChange"] = isChange && allow;
            }
            else
            {
                filterContext.Controller.ViewData["AllowChange"] = true;
                filterContext.Controller.ViewData["IsAdmin"] = true;
            }
            base.OnActionExecuting(filterContext);
        }

        private bool isAdmin = false;

        public bool IsAdmin
        {
            get { return isAdmin; }
            set { isAdmin = value; }
        }

        private bool isChange = false;

        public bool IsChange
        {
            get { return isChange; }
            set { isChange = value; }
        }

        private Type entityType = typeof(void);

        public SecurityAccessAttribute(Type entityType)
            : base()
        {
            this.entityType = entityType;
        }

        public SecurityAccessAttribute()
            : base()
        { }
    }
}