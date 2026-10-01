using System;
using System.Collections.Generic;
using System.Data;
using TrackerSQL.Classes;
using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    /// <summary>GpsSettingsTbl (single row) and GeoApiUsageTbl (lookups per service per day).</summary>
    public class GpsSettingsRepository
    {
        public const string GeocodeService = "GEOCODE";
        public const string AzureGeocodeService = "AZURE_GEOCODE";

        /// <summary>The usage counter for the chosen geocoder (each has its own daily count).</summary>
        public static string ServiceFor(GpsSettings s) => s != null && s.IsAzureMaps ? AzureGeocodeService : GeocodeService;

        /// <summary>Azure Maps map tiles fetched through ~/Handlers/MapTile.ashx (15 tiles = 1 billed transaction).</summary>
        public const string AzureTileService = "AZURE_TILES";
        public const int AzureTileDailyLimit = 15000;

        private bool MapProviderColumnExists()
        {
            try
            {
                using (var db = new TrackerSQLDb())
                    return db.ExecuteScalar<int>("SELECT CASE WHEN COL_LENGTH(N'dbo.GpsSettingsTbl', N'MapProvider') IS NULL THEN 0 ELSE 1 END") == 1;
            }
            catch
            {
                return false;
            }
        }

        public bool TableExists()
        {
            try
            {
                using (var db = new TrackerSQLDb())
                    return db.ExecuteScalar<int>("SELECT CASE WHEN OBJECT_ID(N'dbo.GpsSettingsTbl', N'U') IS NULL THEN 0 ELSE 1 END") == 1;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>The settings row, or defaults when the table / row is missing.</summary>
        public GpsSettings Get()
        {
            var s = new GpsSettings();
            if (!TableExists())
                return s;

            using (var db = new TrackerSQLDb())
            using (var r = db.ExecuteReader("SELECT TOP 1 * FROM dbo.GpsSettingsTbl WHERE SettingsID = 1"))
            {
                if (r == null || !r.Read())
                    return s;

                s.GeocodeProvider = Str(r, "GeocodeProvider") ?? s.GeocodeProvider;
                s.GeocodeApiKey = Str(r, "GeocodeApiKey");
                s.AzureMapsKey = HasColumn(r, "AzureMapsKey") ? Str(r, "AzureMapsKey") : null;
                s.GeocodeBaseUrl = Str(r, "GeocodeBaseUrl") ?? s.GeocodeBaseUrl;
                s.GeocodeDailyLimit = Int(r, "GeocodeDailyLimit", s.GeocodeDailyLimit);
                s.GeocodePerMinute = Int(r, "GeocodePerMinute", s.GeocodePerMinute);
                s.MapTileUrl = Str(r, "MapTileUrl") ?? s.MapTileUrl;
                s.MapProvider = (HasColumn(r, "MapProvider") ? Str(r, "MapProvider") : null) ?? s.MapProvider;
                s.DepotAddress = Str(r, "DepotAddress") ?? s.DepotAddress;
                s.DepotLatitude = Dec(r, "DepotLatitude");
                s.DepotLongitude = Dec(r, "DepotLongitude");
                s.TrackingEnabled = Bool(r, "TrackingEnabled");
                s.PingIntervalMinutes = Int(r, "PingIntervalMinutes", s.PingIntervalMinutes);
                s.TrackingWeekdaysOnly = Bool(r, "TrackingWeekdaysOnly");
                s.WorkHoursStart = Str(r, "WorkHoursStart") ?? s.WorkHoursStart;
                s.WorkHoursEnd = Str(r, "WorkHoursEnd") ?? s.WorkHoursEnd;
                s.PingRetentionDays = Int(r, "PingRetentionDays", s.PingRetentionDays);
                s.AverageSpeedKmh = Int(r, "AverageSpeedKmh", s.AverageSpeedKmh);
                s.ServiceMinutes = Int(r, "ServiceMinutes", s.ServiceMinutes);
                s.FreshMinutes = Int(r, "FreshMinutes", s.FreshMinutes);
                s.StaleMinutes = Int(r, "StaleMinutes", s.StaleMinutes);
                s.UpdatedAt = r["UpdatedAt"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["UpdatedAt"]);
                s.UpdatedBy = Str(r, "UpdatedBy");
            }
            return s;
        }

        /// <summary>
        /// Saves everything. The HeiGIT key is only written when <paramref name="replaceKey"/> is true,
        /// the Azure Maps key only when <paramref name="replaceAzureKey"/> is true.
        /// </summary>
        public void Save(GpsSettings s, bool replaceKey, string userName, bool replaceAzureKey = false)
        {
            if (s == null) throw new ArgumentNullException(nameof(s));

            string sql = @"
IF NOT EXISTS (SELECT 1 FROM dbo.GpsSettingsTbl WHERE SettingsID = 1)
    INSERT INTO dbo.GpsSettingsTbl (SettingsID) VALUES (1);
UPDATE dbo.GpsSettingsTbl SET
    GeocodeProvider = @GeocodeProvider,
    " + (replaceKey ? "GeocodeApiKey = @GeocodeApiKey," : string.Empty) + @"
    " + (replaceAzureKey ? "AzureMapsKey = @AzureMapsKey," : string.Empty) + @"
    GeocodeBaseUrl = @GeocodeBaseUrl,
    GeocodeDailyLimit = @GeocodeDailyLimit,
    GeocodePerMinute = @GeocodePerMinute,
    MapTileUrl = @MapTileUrl,
    " + (MapProviderColumnExists() ? "MapProvider = @MapProvider," : string.Empty) + @"
    DepotAddress = @DepotAddress,
    DepotLatitude = @DepotLatitude,
    DepotLongitude = @DepotLongitude,
    TrackingEnabled = @TrackingEnabled,
    PingIntervalMinutes = @PingIntervalMinutes,
    TrackingWeekdaysOnly = @TrackingWeekdaysOnly,
    WorkHoursStart = @WorkHoursStart,
    WorkHoursEnd = @WorkHoursEnd,
    PingRetentionDays = @PingRetentionDays,
    AverageSpeedKmh = @AverageSpeedKmh,
    ServiceMinutes = @ServiceMinutes,
    FreshMinutes = @FreshMinutes,
    StaleMinutes = @StaleMinutes,
    UpdatedAt = GETDATE(),
    UpdatedBy = @UpdatedBy
WHERE SettingsID = 1;";

            var p = new List<DBParameter>
            {
                P("@GeocodeProvider", s.GeocodeProvider ?? "HeiGIT", DbType.AnsiString),
                P("@GeocodeBaseUrl", s.GeocodeBaseUrl ?? GpsSettings.DefaultGeocodeBaseUrl, DbType.String),
                P("@GeocodeDailyLimit", s.GeocodeDailyLimit, DbType.Int32),
                P("@GeocodePerMinute", s.GeocodePerMinute, DbType.Int32),
                P("@MapTileUrl", s.MapTileUrl ?? GpsSettings.DefaultTileUrl, DbType.String),
                P("@MapProvider", s.IsAzureMap ? GpsSettings.ProviderAzureMaps : GpsSettings.MapOsm, DbType.AnsiString),
                P("@DepotAddress", s.DepotAddress ?? GpsSettings.DefaultDepotAddress, DbType.String),
                P("@DepotLatitude", s.DepotLatitude, DbType.Decimal),
                P("@DepotLongitude", s.DepotLongitude, DbType.Decimal),
                P("@TrackingEnabled", s.TrackingEnabled, DbType.Boolean),
                P("@PingIntervalMinutes", s.PingIntervalMinutes, DbType.Int32),
                P("@TrackingWeekdaysOnly", s.TrackingWeekdaysOnly, DbType.Boolean),
                P("@WorkHoursStart", s.WorkHoursStart ?? "07:00", DbType.AnsiString),
                P("@WorkHoursEnd", s.WorkHoursEnd ?? "18:00", DbType.AnsiString),
                P("@PingRetentionDays", s.PingRetentionDays, DbType.Int32),
                P("@AverageSpeedKmh", s.AverageSpeedKmh, DbType.Int32),
                P("@ServiceMinutes", s.ServiceMinutes, DbType.Int32),
                P("@FreshMinutes", s.FreshMinutes, DbType.Int32),
                P("@StaleMinutes", s.StaleMinutes, DbType.Int32),
                P("@UpdatedBy", userName, DbType.String)
            };
            if (replaceKey)
            {
                string key = string.IsNullOrWhiteSpace(s.GeocodeApiKey) ? null : s.GeocodeApiKey.Trim();
                p.Add(P("@GeocodeApiKey", key, DbType.String));
            }
            if (replaceAzureKey)
            {
                string key = string.IsNullOrWhiteSpace(s.AzureMapsKey) ? null : s.AzureMapsKey.Trim();
                p.Add(P("@AzureMapsKey", key, DbType.String));
            }

            using (var db = new TrackerSQLDb())
                db.ExecuteNonQuery(sql, p);
        }

        public void SaveDepotPoint(decimal latitude, decimal longitude)
        {
            using (var db = new TrackerSQLDb())
                db.ExecuteNonQuery(
                    "UPDATE dbo.GpsSettingsTbl SET DepotLatitude = @Lat, DepotLongitude = @Lng WHERE SettingsID = 1",
                    new List<DBParameter> { P("@Lat", latitude, DbType.Decimal), P("@Lng", longitude, DbType.Decimal) });
        }

        public int GetCallsToday(string service)
        {
            try
            {
                using (var db = new TrackerSQLDb())
                    return db.ExecuteScalar<int>(
                        "SELECT ISNULL(SUM(Calls), 0) FROM dbo.GeoApiUsageTbl WHERE UsageDate = CAST(GETDATE() AS DATE) AND Service = @Service",
                        new List<DBParameter> { P("@Service", service, DbType.AnsiString) });
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>Counts one call against today's cap. False (nothing counted) when the cap is already reached.</summary>
        public bool TryCountCall(string service, int dailyLimit)
        {
            const string sql = @"
SET XACT_ABORT ON;
DECLARE @Today DATE = CAST(GETDATE() AS DATE);
DECLARE @Counted INT;
BEGIN TRAN;
IF NOT EXISTS (SELECT 1 FROM dbo.GeoApiUsageTbl WITH (UPDLOCK, HOLDLOCK) WHERE UsageDate = @Today AND Service = @Service)
    INSERT INTO dbo.GeoApiUsageTbl (UsageDate, Service, Calls) VALUES (@Today, @Service, 0);
UPDATE dbo.GeoApiUsageTbl SET Calls = Calls + 1
WHERE UsageDate = @Today AND Service = @Service AND Calls < @Limit;
SET @Counted = @@ROWCOUNT;
COMMIT;
SELECT @Counted;";
            using (var db = new TrackerSQLDb())
                return db.ExecuteScalar<int>(sql, new List<DBParameter>
                {
                    P("@Service", service, DbType.AnsiString),
                    P("@Limit", Math.Max(0, dailyLimit), DbType.Int32)
                }) > 0;
        }

        /// <summary>Marks today's cap as used up (the provider answered "daily limit reached").</summary>
        public void MarkDailyLimitReached(string service, int dailyLimit)
        {
            using (var db = new TrackerSQLDb())
                db.ExecuteNonQuery(
                    "UPDATE dbo.GeoApiUsageTbl SET Calls = @Limit WHERE UsageDate = CAST(GETDATE() AS DATE) AND Service = @Service AND Calls < @Limit",
                    new List<DBParameter> { P("@Service", service, DbType.AnsiString), P("@Limit", Math.Max(0, dailyLimit), DbType.Int32) });
        }

        private static DBParameter P(string name, object value, DbType type)
        {
            return new DBParameter { ParamName = name, DataValue = value, DataDbType = type };
        }

        private static bool HasColumn(IDataRecord r, string col)
        {
            for (int i = 0; i < r.FieldCount; i++)
            {
                if (string.Equals(r.GetName(i), col, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        private static string Str(IDataRecord r, string col)
        {
            object v = r[col];
            return v == DBNull.Value ? null : Convert.ToString(v);
        }

        private static int Int(IDataRecord r, string col, int fallback)
        {
            object v = r[col];
            return v == DBNull.Value ? fallback : Convert.ToInt32(v);
        }

        private static bool Bool(IDataRecord r, string col)
        {
            object v = r[col];
            return v != DBNull.Value && Convert.ToBoolean(v);
        }

        private static decimal? Dec(IDataRecord r, string col)
        {
            object v = r[col];
            return v == DBNull.Value ? (decimal?)null : Convert.ToDecimal(v);
        }
    }
}
