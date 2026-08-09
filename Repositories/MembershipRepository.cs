using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

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
