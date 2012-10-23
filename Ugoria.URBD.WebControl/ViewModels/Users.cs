using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Data.Objects;
using Ugoria.URBD.WebControl.Models;

namespace Ugoria.URBD.WebControl.ViewModels
{
    public interface IUserRepository
    {
        IUser GetUser(string userName);
    }

    public interface IUser
    {
        int UserId { get; }
        string UserName { get; }
        bool IsAdmin { get; }
        IEnumerable<IPermission> Permissions { get; }
    }

    public interface IPermission
    {
        int ServiceId { get; }
        bool IsNotify { get; }
        IEnumerable<int> Bases { get; }
    }


    public class PermissionViewModel : IPermission
    {
        public int ServiceId { get; set; }
        public bool IsNotify { get; set; }
        public IEnumerable<int> Bases { get; set; }
    }

    public class UserViewModel : IUser
    {
        public int UserId { get; set; }

        public string UserName { get; set; }

        public bool IsAdmin { get; set; }

        public IEnumerable<IPermission> Permissions { get; set; }
    }


    public class UserRepository : IUserRepository
    {
        private readonly URBD2Entities dataContext;

        internal UserRepository(URBD2Entities dataContext)
        {
            this.dataContext = dataContext;
        }

        public IUser GetUser(string userName)
        {
            var query = dataContext.User.Include("UserServicesPermission.Service.Base").Where(u => u.user_name == userName).Select(u => new UserViewModel
            {
                UserId = u.user_id,
                UserName = u.user_name,
                IsAdmin = u.is_admin.HasValue && u.is_admin.Value,
                Permissions = u.UserServicesPermission.Select(p => new PermissionViewModel
                {
                    ServiceId = p.service_id.Value,
                    IsNotify = p.is_notify.HasValue && p.is_notify.Value,
                    Bases = p.Service.Base.Select(b => b.base_id)
                })
            });
            System.Diagnostics.Trace.WriteLine(((ObjectQuery)query).ToTraceString());
            if (query.Count() == 0)
                return null;
            return query.Single();// query.Select<User, IUser>(u => u).Single();

        }
    }
}