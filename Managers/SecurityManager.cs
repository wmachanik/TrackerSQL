using System;
using System.Linq;
using System.Web;
using System.Web.Security;
using TrackerDotNet.Classes;

namespace TrackerDotNet.Managers
{
    /// <summary>
    /// Centralized helper for user/role/security checks.
    /// Kept static for low-friction integration; can be swapped to DI later.
    /// </summary>
    public static class SecurityManager
    {
        private static readonly string[] AdminRoleAliases = { "Administrators", "Administrator" };

        /// <summary>
        /// True if current principal is authenticated.
        /// Safe for anonymous/token scenarios.
        /// </summary>
        public static bool IsAuthenticated =>
            HttpContext.Current?.User?.Identity?.IsAuthenticated ?? false;

        /// <summary>
        /// Returns current username or null if anonymous.
        /// </summary>
        public static string GetCurrentUserName() =>
            Membership.GetUser()?.UserName;

        /// <summary>
        /// Core admin test:
        /// 1. Exact match on configured admin username.
        /// 2. In any admin role alias (Administrators / Administrator).
        /// </summary>
        public static bool IsAdmin()
        {
            var user = Membership.GetUser();
            if (user == null) return false;

            try
            {
                if (!string.IsNullOrEmpty(SystemConstants.UserConstants.AdminUserName) &&
                    user.UserName.Equals(SystemConstants.UserConstants.AdminUserName,
                        StringComparison.OrdinalIgnoreCase))
                    return true;

                // Role provider can throw if not initialized in some edge cases; guard it.
                return AdminRoleAliases.Any(r => Roles.IsUserInRole(user.UserName, r));
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Returns true if current user is in ANY of the supplied roles.
        /// Automatically false for anonymous.
        /// </summary>
        public static bool IsInAnyRole(params string[] roles)
        {
            if (roles == null || roles.Length == 0) return false;
            var user = Membership.GetUser();
            if (user == null) return false;

            try
            {
                return roles.Any(r => Roles.IsUserInRole(user.UserName, r));
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Compound convenience: admin OR in any of the provided roles.
        /// </summary>
        public static bool IsAdminOrInRoles(params string[] roles) =>
            IsAdmin() || IsInAnyRole(roles);

        /// <summary>
        /// For pages that need an early hard-stop if not admin.
        /// (Lightweight; you can emit a log or redirect as needed.)
        /// </summary>
        public static bool DemandAdmin(Action onFail = null)
        {
            if (IsAdmin()) return true;
            onFail?.Invoke();
            return false;
        }
    }
}