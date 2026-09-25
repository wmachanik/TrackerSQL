using System;
using System.Data.SqlClient;
using System.Web.Security;
using TrackerSQL.Classes;
using TrackerSQL.Repositories;

namespace TrackerSQL.Managers
{
    /// <summary>
    /// Role helpers that work against the migrated membership schema.
    /// </summary>
    public static class MembershipRoleManager
    {
        /// <summary>
        /// Uses the standard role provider first. The migrated OtterDb schema lacks
        /// the RoleId default expected by aspnet_Roles_CreateRole, so error 515 falls back to
        /// an explicit transactional insert with a generated GUID.
        /// </summary>
        public static void CreateRoleCompatible(string roleName, string logContext)
        {
            try
            {
                Roles.CreateRole(roleName);
            }
            catch (Exception ex)
            {
                if (!ContainsSqlError(ex, 515))
                    throw;

                string applicationName = Roles.Provider?.ApplicationName ?? "/";
                if (!new MembershipRepository().CreateRole(roleName, applicationName))
                    throw;

                AppLogger.WriteLog(SystemConstants.LogTypes.Login,
                    logContext + ": created role '" + roleName
                    + "' using migrated-schema RoleId fallback.");
            }
        }

        /// <summary>Creates the role when missing. Throws if it still cannot be created.</summary>
        public static void EnsureRoleExists(string roleName, string logContext)
        {
            if (Roles.RoleExists(roleName))
                return;
            CreateRoleCompatible(roleName, logContext);
        }

        private static bool ContainsSqlError(Exception exception, int errorNumber)
        {
            for (Exception current = exception; current != null; current = current.InnerException)
            {
                var sqlException = current as SqlException;
                if (sqlException != null && sqlException.Number == errorNumber)
                    return true;
            }
            return false;
        }
    }
}
