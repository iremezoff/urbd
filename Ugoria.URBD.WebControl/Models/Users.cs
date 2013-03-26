using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Ugoria.URBD.WebControl.ViewModels;
using System.Data.Entity;
using System.Data.Objects;

namespace Ugoria.URBD.WebControl.Models
{
    public interface IUser
    {
        int UserId { get; }
        string UserName { get; }
        bool IsAdmin { get; }
        string Mail { get; }
        string Phone { get; }
        bool IsActive { get; }
        IEnumerable<IPermission> BasePermissions { get; }
        IEnumerable<IPermission> ServicePermissions { get; }
    }

    public partial class User : IUser
    {
        public int UserId
        {
            get { return user_id; }
        }

        public string UserName
        {
            get { return user_name; }
        }

        public bool IsAdmin
        {
            get { return is_admin.HasValue && is_admin.Value; }
        }

        public string Mail
        {
            get { return mail; }
        }

        public string Phone
        {
            get { return phone; }
        }

        public IEnumerable<IPermission> BasePermissions
        {
            get { return UserBasesPermission; }
        }

        public IEnumerable<IPermission> ServicePermissions
        {
            get { return UserServicesPermission; }
        }


        public bool IsActive
        {
            get { return is_active.HasValue && is_active.Value; }
        }
    }

    public partial class UserBasesPermission : IPermission
    {
        public int EntityId
        {
            get { return permission_id; }
        }

        public bool AllowConfigure
        {
            get { return allow_configure.HasValue && allow_configure.Value; }
        }

        public int UserId
        {
            get { return user_id; }
        }
    }

    public partial class UserServicesPermission : IPermission
    {
        public int EntityId
        {
            get { return permission_id; }
        }

        public bool AllowConfigure
        {
            get { return allow_configure.HasValue && allow_configure.Value; }
        }

        public int UserId
        {
            get { return user_id; }
        }
    }

    public interface IUsersRepository
    {
        IEnumerable<IUser> GetUsers();
        IUser GetUserByName(string username);
        void SaveUser(UserViewModel userVM);
    }

    public class UsersRepository : IUsersRepository
    {
        private URBD2Entities dataContext;
        private IEnumerable<User> cache;

        public UsersRepository(URBD2Entities dataContext)
        {
            this.dataContext = dataContext;
        }

        public IEnumerable<IUser> GetUsers()
        {
            cache = dataContext.User.Where(u => u.user_id != 1).OrderBy(u => u.user_name).Select(u => u).ToList();
            return cache;
        }

        public void SaveUser(UserViewModel userVM)
        {
            if (cache == null)
                cache = GetUsers().Select<IUser, User>(u => (User)u);
            if (userVM.UserId == 0)
            {
                dataContext.User.AddObject(new User
                {
                    user_name = userVM.UserName,
                    mail = userVM.Mail,
                    phone = userVM.Phone,
                    is_admin = userVM.IsAdmin,
                    is_active = true
                });
                return;
            }
            User user = cache.Where(u => u.UserId == userVM.UserId).SingleOrDefault();
            if (user == null)
                return;
            user.user_name = userVM.UserName;
            user.mail = userVM.Mail;
            user.phone = userVM.Phone;
            user.is_active = userVM.IsActive;
        }

        public IUser GetUserByName(string username)
        {
            var query = dataContext.User.Include("UserBasesPermission").Include("UserServicesPermission")
                .Where(u => u.user_name == username && u.is_active.HasValue && u.is_active.Value)
                .Select(u => new UserViewModel
                {
                    UserId = u.user_id,
                    UserName = u.user_name,
                    IsAdmin = u.is_admin.HasValue && u.is_admin.Value,
                    BasePermissions = u.UserBasesPermission.Select(p => new PermissionViewModel { EntityId = p.base_id, AllowConfigure = p.allow_configure.HasValue && p.allow_configure.Value }),
                    ServicePermissions = u.UserServicesPermission.Select(p => new PermissionViewModel { EntityId = p.service_id, AllowConfigure = p.allow_configure.HasValue && p.allow_configure.Value })
                });

            System.Diagnostics.Trace.WriteLine(((ObjectQuery)query).ToTraceString());
            if (query.Count() == 0)
                return null;
            return query.Single();// query.Select<User, IUser>(u => u).Single();
        }
    }
}