using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Data.Objects;
using Ugoria.URBD.WebControl.Models;

namespace Ugoria.URBD.WebControl.ViewModels
{
    public interface IPermission
    {
        int PermissionId { get; }
        int UserId { get; }
        bool AllowConfigure { get; }
    }

    public class PermissionViewModel : IPermission
    {
        public int PermissionId { get; set; }
        public int UserId { get; set; }
        public bool AllowConfigure { get; set; }
    }

    public class UserViewModel : IUser
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public bool IsAdmin { get; set; }
        public string Mail { get; set; }
        public string Phone { get; set; }
        public IEnumerable<IPermission> BasePermissions { get; set; }
        public IEnumerable<IPermission> ServicePermissions { get; set; }
        public bool IsActive { get; set; }
    }
}