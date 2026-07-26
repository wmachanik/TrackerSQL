using System;
using System.Linq;
using System.Web;
using System.Web.Security;
using TrackerSQL.Classes;

namespace TrackerSQL.Managers
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
        /// 1. Exact match on configured admin username (forms identity and/or Membership).
        /// 2. In any admin role alias (Administrators / Administrator) via principal or Roles API.
        /// </summary>
        public static bool IsAdmin()
        {
            if (!IsAuthenticated) return false;

            try
            {
                string identityName = HttpContext.Current?.User?.Identity?.Name;
                string membershipName = null;
                try
                {
                    membershipName = Membership.GetUser()?.UserName;
                }
                catch
                {
                    // Membership provider can fail while forms auth still works.
                }

                string adminUserName = SystemConstants.UserConstants.AdminUserName;
                if (!string.IsNullOrEmpty(adminUserName))
                {
                    if (!string.IsNullOrEmpty(identityName) &&
                        identityName.Equals(adminUserName, StringComparison.OrdinalIgnoreCase))
                        return true;

                    if (!string.IsNullOrEmpty(membershipName) &&
                        membershipName.Equals(adminUserName, StringComparison.OrdinalIgnoreCase))
                        return true;
                }

                var principal = HttpContext.Current?.User;
                if (principal != null && AdminRoleAliases.Any(r => principal.IsInRole(r)))
                    return true;

                string roleUserName = !string.IsNullOrEmpty(membershipName) ? membershipName : identityName;
                if (string.IsNullOrEmpty(roleUserName))
                    return false;

                // Role provider can throw if not initialized in some edge cases; guard it.
                return AdminRoleAliases.Any(r => Roles.IsUserInRole(roleUserName, r));
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
