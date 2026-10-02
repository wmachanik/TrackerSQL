using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using TrackerSQL.Classes;
using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    /// <summary>
    /// Fallback cleanup for migrated ASP.NET membership data.
    /// The standard provider remains the primary delete path.
    /// </summary>
    public class MembershipRepository
    {
        public bool CreateRole(string roleName, string applicationName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
                return false;

            string connectionString = GetConnectionString();
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        Guid? applicationId = GetApplicationId(
                            connection,
                            transaction,
                            string.IsNullOrWhiteSpace(applicationName) ? "/" : applicationName);
                        if (!applicationId.HasValue)
                        {
                            transaction.Rollback();
                            return false;
                        }

                        const string sql = @"
                            INSERT INTO dbo.aspnet_Roles
                                (ApplicationId, RoleId, RoleName, LoweredRoleName, Description)
                            VALUES
                                (@ApplicationId, @RoleId, @RoleName, LOWER(@RoleName), NULL)";

                        using (var command = new SqlCommand(sql, connection, transaction))
                        {
                            command.Parameters.Add("@ApplicationId", SqlDbType.UniqueIdentifier)
                                .Value = applicationId.Value;
                            command.Parameters.Add("@RoleId", SqlDbType.UniqueIdentifier)
                                .Value = Guid.NewGuid();
                            command.Parameters.Add("@RoleName", SqlDbType.NVarChar, 256)
                                .Value = roleName.Trim();
                            command.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        return true;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public bool DeleteUserAndRelatedData(string userName, string applicationName)
        {
            if (string.IsNullOrWhiteSpace(userName))
                return false;

            string connectionString = GetConnectionString();

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        Guid? userId = GetUserId(
                            connection,
                            transaction,
                            userName.Trim(),
                            string.IsNullOrWhiteSpace(applicationName) ? "/" : applicationName);
                        if (!userId.HasValue)
                        {
                            transaction.Rollback();
                            return false;
                        }

                        DeleteByUserId(connection, transaction, "dbo.UserPreferences", userId.Value);
                        DeleteByUserId(connection, transaction, "dbo.aspnet_PersonalizationPerUser", userId.Value);
                        DeleteByUserId(connection, transaction, "dbo.aspnet_Profile", userId.Value);
                        DeleteByUserId(connection, transaction, "dbo.aspnet_UsersInRoles", userId.Value);
                        DeleteByUserId(connection, transaction, "dbo.aspnet_Membership", userId.Value);

                        int deletedUsers = DeleteByUserId(
                            connection,
                            transaction,
                            "dbo.aspnet_Users",
                            userId.Value);

                        transaction.Commit();
                        return deletedUsers > 0;
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// Every login of the application with its roles, portal contact and driver app use.
        /// Membership dates are UTC; ContactUserLinkTbl / MobileDeviceTokensTbl times are server local.
        /// </summary>
        public List<UserAccountRow> ListAccounts(string applicationName, int onlineWindowMinutes)
        {
            var list = new List<UserAccountRow>();
            using (var connection = new SqlConnection(GetConnectionString()))
            {
                connection.Open();
                bool links = TableExists(connection, "ContactUserLinkTbl");
                bool tokens = TableExists(connection, "MobileDeviceTokensTbl");

                string sql = @"
SELECT u.UserId, u.UserName, m.Email, m.IsApproved, m.IsLockedOut, m.Comment,
       m.CreateDate, m.LastLoginDate, u.LastActivityDate,
       STUFF((SELECT ', ' + r.RoleName FROM dbo.aspnet_UsersInRoles ur
              INNER JOIN dbo.aspnet_Roles r ON r.RoleId = ur.RoleId
              WHERE ur.UserId = u.UserId ORDER BY r.RoleName
              FOR XML PATH(''), TYPE).value('.', 'nvarchar(max)'), 1, 2, '') AS Roles,
       " + (links ? "lnk.ContactID, c.CompanyName, lnk.LastLoginAt" : "CAST(NULL AS INT) AS ContactID, CAST(NULL AS NVARCHAR(100)) AS CompanyName, CAST(NULL AS DATETIME) AS LastLoginAt") + @",
       " + (tokens ? "app.LastUsedAt" : "CAST(NULL AS DATETIME) AS LastUsedAt") + @"
FROM dbo.aspnet_Users u
INNER JOIN dbo.aspnet_Membership m ON m.UserId = u.UserId
INNER JOIN dbo.aspnet_Applications a ON a.ApplicationId = u.ApplicationId
" + (links ? @"OUTER APPLY (SELECT TOP 1 l.ContactID, l.LastLoginAt FROM dbo.ContactUserLinkTbl l WHERE l.UserId = u.UserId ORDER BY l.LinkID) lnk
LEFT JOIN dbo.ContactsTbl c ON c.ContactID = lnk.ContactID
" : string.Empty) + (tokens ? @"OUTER APPLY (SELECT MAX(COALESCE(t.LastUsedAt, t.CreatedAt)) AS LastUsedAt FROM dbo.MobileDeviceTokensTbl t WHERE t.UserName = u.UserName) app
" : string.Empty) + @"WHERE a.LoweredApplicationName = LOWER(@ApplicationName)";

                DateTime utcNow = DateTime.UtcNow;
                using (var command = new SqlCommand(sql, connection))
                {
                    command.Parameters.Add("@ApplicationName", SqlDbType.NVarChar, 256).Value =
                        string.IsNullOrWhiteSpace(applicationName) ? "/" : applicationName;
                    using (var r = command.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            string roles = r["Roles"] as string ?? string.Empty;
                            var roleList = roles.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                .Select(x => x.Trim()).ToList();
                            DateTime createdUtc = Convert.ToDateTime(r["CreateDate"]);
                            DateTime loginUtc = Convert.ToDateTime(r["LastLoginDate"]);
                            DateTime activityUtc = Convert.ToDateTime(r["LastActivityDate"]);

                            var row = new UserAccountRow
                            {
                                UserId = (Guid)r["UserId"],
                                UserName = Convert.ToString(r["UserName"]),
                                Email = r["Email"] as string,
                                IsApproved = Convert.ToBoolean(r["IsApproved"]),
                                IsLockedOut = Convert.ToBoolean(r["IsLockedOut"]),
                                IsOnline = activityUtc > utcNow.AddMinutes(-Math.Max(1, onlineWindowMinutes)),
                                Comment = r["Comment"] as string,
                                Roles = roles,
                                IsAdmin = roleList.Any(x => x.Equals("Administrators", StringComparison.OrdinalIgnoreCase)
                                    || x.Equals("Admin", StringComparison.OrdinalIgnoreCase)),
                                ContactID = r["ContactID"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["ContactID"]),
                                CompanyName = r["CompanyName"] as string,
                                Created = TimeZoneUtils.ConvertUtcToUserZone(createdUtc)
                            };
                            row.IsContact = row.ContactID.HasValue
                                || roleList.Any(x => x.Equals(Managers.ContactPortalSchemaInstaller.ContactRoleName, StringComparison.OrdinalIgnoreCase));

                            row.LastUsed = row.Created;
                            row.LastUsedHow = "never";
                            if ((loginUtc - createdUtc).TotalSeconds > 1)
                            {
                                row.LastUsed = TimeZoneUtils.ConvertUtcToUserZone(loginUtc);
                                row.LastUsedHow = "Tracker";
                            }
                            if (r["LastLoginAt"] != DBNull.Value && Convert.ToDateTime(r["LastLoginAt"]) > row.LastUsed)
                            {
                                row.LastUsed = Convert.ToDateTime(r["LastLoginAt"]);
                                row.LastUsedHow = "portal";
                            }
                            if (r["LastUsedAt"] != DBNull.Value && Convert.ToDateTime(r["LastUsedAt"]) > row.LastUsed)
                            {
                                row.LastUsed = Convert.ToDateTime(r["LastUsedAt"]);
                                row.LastUsedHow = "driver app";
                            }
                            list.Add(row);
                        }
                    }
                }
            }
            return list;
        }

        /// <summary>
        /// What the membership delete leaves behind for a user: preferences, the Contact Portal link
        /// and driver app tokens (revoked, so a phone signed in as them stops working).
        /// </summary>
        public void DeleteUserLeftovers(Guid userId, string userName, string by)
        {
            using (var connection = new SqlConnection(GetConnectionString()))
            {
                connection.Open();
                if (TableExists(connection, "UserPreferences"))
                    Execute(connection, "DELETE FROM dbo.UserPreferences WHERE UserId = @UserId", userId, userName, by);
                if (TableExists(connection, "ContactUserLinkTbl"))
                    Execute(connection, "DELETE FROM dbo.ContactUserLinkTbl WHERE UserId = @UserId", userId, userName, by);
                if (TableExists(connection, "MobileDeviceTokensTbl"))
                    Execute(connection, @"UPDATE dbo.MobileDeviceTokensTbl SET RevokedAt = GETDATE(), RevokedBy = @By
                                          WHERE UserName = @UserName AND RevokedAt IS NULL", userId, userName, by);
            }
        }

        private static void Execute(SqlConnection connection, string sql, Guid userId, string userName, string by)
        {
            using (var command = new SqlCommand(sql, connection))
            {
                command.Parameters.Add("@UserId", SqlDbType.UniqueIdentifier).Value = userId;
                command.Parameters.Add("@UserName", SqlDbType.NVarChar, 256).Value = userName ?? string.Empty;
                command.Parameters.Add("@By", SqlDbType.NVarChar, 256).Value = (object)by ?? DBNull.Value;
                command.ExecuteNonQuery();
            }
        }

        private static bool TableExists(SqlConnection connection, string tableName)
        {
            using (var command = new SqlCommand("SELECT CASE WHEN OBJECT_ID(@Name, N'U') IS NULL THEN 0 ELSE 1 END", connection))
            {
                command.Parameters.Add("@Name", SqlDbType.NVarChar, 256).Value = "dbo." + tableName;
                return Convert.ToInt32(command.ExecuteScalar()) == 1;
            }
        }

        private static string GetConnectionString()
        {
            string connectionString =
                ConfigurationManager.ConnectionStrings["TrackerDataSQL"]?.ConnectionString;
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ConfigurationErrorsException(
                    "Connection string 'TrackerDataSQL' is missing or empty.");

            return connectionString;
        }

        private static Guid? GetApplicationId(
            SqlConnection connection,
            SqlTransaction transaction,
            string applicationName)
        {
            const string sql = @"
                SELECT ApplicationId
                FROM dbo.aspnet_Applications
                WHERE LoweredApplicationName = LOWER(@ApplicationName)";

            using (var command = new SqlCommand(sql, connection, transaction))
            {
                command.Parameters.Add("@ApplicationName", SqlDbType.NVarChar, 256)
                    .Value = applicationName;
                object value = command.ExecuteScalar();
                return value == null || value == DBNull.Value
                    ? (Guid?)null
                    : (Guid)value;
            }
        }

        private static Guid? GetUserId(
            SqlConnection connection,
            SqlTransaction transaction,
            string userName,
            string applicationName)
        {
            const string sql = @"
                SELECT u.UserId
                FROM dbo.aspnet_Users u
                INNER JOIN dbo.aspnet_Applications a ON a.ApplicationId = u.ApplicationId
                WHERE u.LoweredUserName = LOWER(@UserName)
                  AND a.LoweredApplicationName = LOWER(@ApplicationName)";

            using (var command = new SqlCommand(sql, connection, transaction))
            {
                command.Parameters.Add("@UserName", SqlDbType.NVarChar, 256).Value = userName;
                command.Parameters.Add("@ApplicationName", SqlDbType.NVarChar, 256).Value = applicationName;
                object value = command.ExecuteScalar();
                return value == null || value == DBNull.Value
                    ? (Guid?)null
                    : (Guid)value;
            }
        }

        private static int DeleteByUserId(
            SqlConnection connection,
            SqlTransaction transaction,
            string tableName,
            Guid userId)
        {
            // Table names are fixed constants controlled by this class; UserId remains parameterized.
            using (var command = new SqlCommand(
                "DELETE FROM " + tableName + " WHERE UserId = @UserId",
                connection,
                transaction))
            {
                command.Parameters.Add("@UserId", SqlDbType.UniqueIdentifier).Value = userId;
                return command.ExecuteNonQuery();
            }
        }
    }
}
