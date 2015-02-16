using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.AccessControl;
using System.Security.Principal;
using System.IO;
using System.Security;
using System.Net;

namespace Ugoria.URBD.Shared
{
    public static class SecureHelper
    {
        public static SecureString ConvertToSecureString(string line)
        {
            SecureString secureString = new SecureString();
            foreach (char sym in line)
                secureString.AppendChar(sym);
            return secureString;
        }

        public static bool IsRuleAllow(string path, FileSystemRights right)
        {
            string currentUser = WindowsIdentity.GetCurrent().Name;
            // для системных учетных записей
            if (currentUser.IndexOf(@"NT AUTHORITY\SYSTEM") >= 0 || currentUser.IndexOf(@"NT AUTHORITY\NETWORK SERVICE") >= 0)
                return true;
            return IsRuleAllow(path, SecureHelper.ConvertUserdomainToClassic(WindowsIdentity.GetCurrent().Name), right);
        }

        public static bool IsRuleAllow(string path, string identity, FileSystemRights right)
        {
            try
            {
                FileSecurity dirSecurity = File.GetAccessControl(path, AccessControlSections.Access);
                WindowsIdentity sec = new WindowsIdentity(identity);

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
            catch (Exception)
            {
            }
            return false;
        }

        public static string ConvertUserdomainToClassic(string userdomain)
        {
            if (userdomain.IndexOf("@") >= 0 || userdomain.StartsWith("NT AUTHORITY", StringComparison.InvariantCultureIgnoreCase))
                return userdomain;
            string[] spl = userdomain.Split(new string[] { @"\" }, StringSplitOptions.RemoveEmptyEntries);

            if (spl.Length > 1)
                return string.Format("{0}@{1}", spl[1], spl[0]);
            return string.Format("{0}@localhost", spl[0]);
        }
    }
}
