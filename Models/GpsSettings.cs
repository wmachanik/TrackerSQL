using System;

namespace TrackerSQL.Models
{
    /// <summary>GpsSettingsTbl (single row, SettingsID = 1): System Preferences → GPS &amp; Drivers.</summary>
    public class GpsSettings
    {
        public const string DefaultDepotAddress = "Quaffee AT Buitenverwachting; Klein Constantia Rd; Constantia; Cape Town";
        public const string DefaultTileUrl = "https://tile.openstreetmap.org/{z}/{x}/{y}.png";
        public const string DefaultGeocodeBaseUrl = "https://api.heigit.org";

        public string GeocodeProvider { get; set; } = "HeiGIT";
        /// <summary>Never shown back on screen or written to logs.</summary>
        public string GeocodeApiKey { get; set; }
        public string GeocodeBaseUrl { get; set; } = DefaultGeocodeBaseUrl;
        public int GeocodeDailyLimit { get; set; } = 2500;
        public int GeocodePerMinute { get; set; } = 90;

        public string MapTileUrl { get; set; } = DefaultTileUrl;
        public string DepotAddress { get; set; } = DefaultDepotAddress;
        public decimal? DepotLatitude { get; set; }
        public decimal? DepotLongitude { get; set; }

        public bool TrackingEnabled { get; set; }
        public int PingIntervalMinutes { get; set; } = 10;
        public bool TrackingWeekdaysOnly { get; set; } = true;
        public string WorkHoursStart { get; set; } = "07:00";
        public string WorkHoursEnd { get; set; } = "18:00";
        public int PingRetentionDays { get; set; } = 30;

        public int AverageSpeedKmh { get; set; } = 30;
        public int ServiceMinutes { get; set; } = 5;
        public int FreshMinutes { get; set; } = 15;
        public int StaleMinutes { get; set; } = 30;

        public DateTime? UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }

        public bool HasApiKey => !string.IsNullOrWhiteSpace(GeocodeApiKey);
        public bool HasDepotPoint => DepotLatitude.HasValue && DepotLongitude.HasValue;
    }
}
