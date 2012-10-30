using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.AccessControl;
using System.Security.Principal;
using System.IO;

namespace Ugoria.URBD.Shared
{
    public static class AccessCheck
    {
        public static bool IsRuleAllow(string path, FileSystemRights right)
        {
            try
            {
                FileSecurity dirSecurity = File.GetAccessControl(path, AccessControlSections.Access);
                WindowsIdentity sec = WindowsIdentity.GetCurrent(false);

                bool isAllowAccess = false;
                bool isDenyAccess = false;
                foreach (FileSystemAccessRule rule in dirSecurity.GetAccessRules(true, true, typeof(SecurityIdentifier)))
                {
                    // проверка разрешающего права
                    if ((rule.FileSystemRights & right) == right
                        && (rule.IdentityReference == sec.User || sec.Groups.Contains(rule.IdentityReference))
                        && rule.AccessControlType == AccessControlType.Allow)
                    {
                        isAllowAccess = true;
                    }
                    // проверка запрещающего правила
                    if ((rule.FileSystemRights & right) == right
                        && (rule.IdentityReference == sec.User || sec.Groups.Contains(rule.IdentityReference))
                        && rule.AccessControlType == AccessControlType.Deny)
                    {
                        isDenyAccess = true;
                    }
                }
                return isAllowAccess && !isDenyAccess;
            }
            catch (Exception ex)
            {
            }
            return false;
        }
    }
}
