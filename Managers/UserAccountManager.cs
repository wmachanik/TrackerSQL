using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Security;
using TrackerSQL.Classes;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Managers
{
    /// <summary>Logins for Administration → Manage Users: list, unused accounts and delete.</summary>
    public class UserAccountManager
    {
        /// <summary>Accounts not used (Tracker, portal or driver app) for this many years can be deleted in bulk.</summary>
        public const int UnusedYears = 2;

        private readonly MembershipRepository _repository = new MembershipRepository();

        public List<UserAccountRow> ListAccounts()
        {
            return _repository.ListAccounts(Membership.Provider?.ApplicationName ?? "/", Membership.UserIsOnlineTimeWindow);
        }

        public static DateTime UnusedBefore => TimeZoneUtils.Now().Date.AddYears(-UnusedYears);

        public static bool IsUnused(UserAccountRow row)
        {
            return row != null && row.LastUsed < UnusedBefore;
        }

        /// <summary>Unused accounts the bulk delete may remove: never administrators or the signed-in user.</summary>
        public List<UserAccountRow> ListDeletableUnused(string currentUserName)
        {
            return ListAccounts()
                .Where(IsUnused)
                .Where(row => !row.IsAdmin)
                .Where(row => !string.Equals(row.UserName, currentUserName, StringComparison.OrdinalIgnoreCase))
                .OrderBy(row => row.LastUsed)
                .ToList();
        }

        /// <summary>
        /// Deletes the login, its roles and profile, then its preferences, portal link and app tokens.
        /// Falls back to the FK-safe delete for migrated membership rows.
        /// </summary>
        public bool DeleteUser(string userName, string by, out string error)
        {
            error = null;
            if (string.IsNullOrWhiteSpace(userName))
            {
                error = "No user selected.";
                return false;
            }

            MembershipUser user = Membership.GetUser(userName, false);
            if (user == null)
            {
                error = "User '" + userName + "' was not found.";
                return false;
            }
            Guid userId = (Guid)user.ProviderUserKey;

            bool deleted;
            try
            {
                deleted = Membership.DeleteUser(user.UserName, deleteAllRelatedData: true);
            }
            catch (Exception ex) when (IsForeignKeyDeleteConflict(ex))
            {
                deleted = _repository.DeleteUserAndRelatedData(user.UserName, Membership.Provider?.ApplicationName ?? "/");
                if (deleted)
                    AppLogger.WriteLog(SystemConstants.LogTypes.Login,
                        "UserAccountManager: deleted migrated membership user '" + user.UserName + "' using the FK-safe cleanup.");
            }

            if (!deleted)
            {
                error = "Could not delete user '" + user.UserName + "'.";
                AppLogger.WriteLog(SystemConstants.LogTypes.Login, "UserAccountManager: delete of '" + user.UserName + "' failed.");
                return false;
            }

            try
            {
                _repository.DeleteUserLeftovers(userId, user.UserName, by);
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Login,
                    "UserAccountManager: '" + user.UserName + "' deleted but clean-up failed: " + ex.Message);
            }

            AppLogger.WriteLog(SystemConstants.LogTypes.Login,
                "UserAccountManager: deleted user '" + user.UserName + "' (by " + (by ?? "?") + ").");
            return true;
        }

        private static bool IsForeignKeyDeleteConflict(Exception exception)
        {
            for (Exception current = exception; current != null; current = current.InnerException)
            {
                if (current is SqlException sql && sql.Number == 547)
                    return true;
            }
            return false;
        }
    }
}
