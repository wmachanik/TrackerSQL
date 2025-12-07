using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace TrackerSQL.Classes
{
    public static class UserPreferencesHelper
    {
        private const string SessionKey = "UserPreferences";
        private static readonly string DefaultTimeZone = ConfigHelper.GetString("AppTimeZoneId","South Africa Standard Time");

        public static UserPreferences GetCurrentPreferences()
        {
            if (HttpContext.Current == null || HttpContext.Current.User == null || !HttpContext.Current.User.Identity.IsAuthenticated)
                return GetDefaults();

            if (HttpContext.Current.Session[SessionKey] is UserPreferences cachedPrefs)
                return cachedPrefs;

            Guid userId = (Guid)Membership.GetUser()?.ProviderUserKey;
            EnsureUserPreferencesTableExists();
            UserPreferences prefs = LoadPreferencesFromDb(userId);

            if (prefs == null)
            {
                prefs = new UserPreferences { UserId = userId, TimeZoneId = DefaultTimeZone, LoadedOn = DateTime.UtcNow };
                SavePreferencesToDb(prefs);
            }

            HttpContext.Current.Session[SessionKey] = prefs;
            return prefs;
        }
        
        public static DateTime Now()
        {
            var prefs = GetCurrentPreferences();
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, prefs.GetTimeZoneInfo());
        }

        public static UserPreferences GetDefaults()
        {
            return new UserPreferences
            {
                TimeZoneId = DefaultTimeZone,
                Language = "en-ZA",
                LoadedOn = DateTime.UtcNow
            };
        }

        public static void EnsureUserPreferencesTableExists()
        {
            string connString = ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;
            string checkQuery = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'UserPreferences'";

            using (SqlConnection conn = new SqlConnection(connString))
            using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
            {
                conn.Open();
                int count = (int)checkCmd.ExecuteScalar();

                if (count == 0)
                {
                    string createQuery = @"CREATE TABLE UserPreferences (UserId UNIQUEIDENTIFIER PRIMARY KEY,"+
                                                         "TimeZoneId NVARCHAR(100) NOT NULL," +
                                                         "Language NVARCHAR(10) NOT NULL," +
                                                         "CreatedOn DATETIME NOT NULL, " +
                                                         "UpdatedOn DATETIME NOT NULL);";

                    using (SqlCommand createCmd = new SqlCommand(createQuery, conn))
                    {
                        createCmd.ExecuteNonQuery();
                        AppLogger.WriteLog("system", "✅ Created UserPreferences table in QOnTSecurity.");
                    }
                }
            }
        }
        public static UserPreferences GetCurrentPreferencesForUser(Guid userId)
        {
            EnsureUserPreferencesTableExists();

            var prefs = LoadPreferencesFromDb(userId);
            if (prefs == null)
            {
                prefs = new UserPreferences
                {
                    UserId = userId,
                    TimeZoneId = DefaultTimeZone,
                    Language = "en-ZA",
                    LoadedOn = DateTime.UtcNow
                };
                SavePreferencesToDb(prefs);
            }

            return prefs;
        }
        public static UserPreferences LoadPreferencesFromDb(Guid userId)
        {
            string connString = ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;
            string query = "SELECT UserId, TimeZoneId, Language FROM UserPreferences WHERE UserId = @UserId";

            using (SqlConnection conn = new SqlConnection(connString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                conn.Open();

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows)
                        return null;

                    reader.Read();
                    return new UserPreferences
                    {
                        UserId = reader.GetGuid(reader.GetOrdinal("UserId")),
                        TimeZoneId = reader.GetString(reader.GetOrdinal("TimeZoneId")),
                        Language = reader.GetString(reader.GetOrdinal("Language")),
                        LoadedOn = DateTime.UtcNow
                    };
                }
            }
        }

        public static void SavePreferencesToDb(UserPreferences prefs)
        {
            string connString = ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;
            string query = @"INSERT INTO UserPreferences (UserId, TimeZoneId, Language, CreatedOn, UpdatedOn)
                     VALUES (@UserId, @TimeZoneId, @Language, @CreatedOn, @UpdatedOn)";

            using (SqlConnection conn = new SqlConnection(connString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@UserId", prefs.UserId);
                cmd.Parameters.AddWithValue("@TimeZoneId", prefs.TimeZoneId ?? "South Africa Standard Time");
                cmd.Parameters.AddWithValue("@Language", prefs.Language ?? "en-ZA");
                cmd.Parameters.AddWithValue("@CreatedOn", DateTime.UtcNow);
                cmd.Parameters.AddWithValue("@UpdatedOn", DateTime.UtcNow);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
        public static void UpdatePreferencesInDb(UserPreferences prefs)
        {
            string connString = ConfigurationManager.ConnectionStrings["ApplicationServices"].ConnectionString;
            string query = @"UPDATE UserPreferences
                     SET TimeZoneId = @TimeZoneId,
                         Language = @Language,
                         UpdatedOn = @UpdatedOn
                     WHERE UserId = @UserId";

            using (SqlConnection conn = new SqlConnection(connString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@UserId", prefs.UserId);
                cmd.Parameters.AddWithValue("@TimeZoneId", prefs.TimeZoneId ?? "South Africa Standard Time");
                cmd.Parameters.AddWithValue("@Language", prefs.Language ?? "en-ZA");
                cmd.Parameters.AddWithValue("@UpdatedOn", DateTime.UtcNow);

                conn.Open();
                cmd.ExecuteNonQuery();
            }
        }
        public static void SaveOrUpdatePreferences(UserPreferences prefs)
        {
            var existing = LoadPreferencesFromDb(prefs.UserId);
            if (existing == null)
            {
                SavePreferencesToDb(prefs);
            }
            else
            {
                UpdatePreferencesInDb(prefs);
            }
        }


    }

}