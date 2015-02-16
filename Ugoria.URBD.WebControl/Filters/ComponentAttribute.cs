using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Ugoria.URBD.WebControl.Models;

namespace Ugoria.URBD.WebControl.Filters
{
    public class ComponentAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            IComponentsRepository componentRepo = new ComponentsRepository(new URBD2Entities());

            IEnumerable<IComponent> components = componentRepo.GetComponents();

            List<IComponent> menu = new List<IComponent>();
            foreach (var component in components)
            {
                if (component.Name.Equals(filterContext.RouteData.Values["action"]))
                    filterContext.Controller.ViewData["ComponentHead"] = component.Description;
                else
                    menu.Add(component);
            }
            filterContext.Controller.ViewData["Components"] = menu;

            base.OnActionExecuting(filterContext);
        }
    }
}