using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Ugoria.URBD.WebControl.Helpers;
using Ugoria.URBD.WebControl.Models;
using Ugoria.URBD.WebControl.ViewModels;

namespace Ugoria.URBD.WebControl
{
    [HubName("TransferHub")]
    public class TransferHub : Hub
    {
        public void SubscribeComponent(string componentName)
        {
            IUser currentUser = SessionStore.GetCurrentUser(Context.User.Identity.Name);

            if (currentUser.IsAdmin)
            {
                Groups.Add(Context.ConnectionId, string.Format("{0}.All", componentName));
            }
            else
            {
                foreach (IPermission permission in currentUser.BasePermissions)
                {
                    Groups.Add(Context.ConnectionId, string.Format("{0}.{1}", componentName, permission.EntityId));
                }
            }
        }

        public override System.Threading.Tasks.Task OnDisconnected()
        {
            return base.OnDisconnected();
        }

        public override System.Threading.Tasks.Task OnReconnected()
        {
            return base.OnReconnected();
        }
    }
}