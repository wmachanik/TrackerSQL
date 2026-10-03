using System;

namespace TrackerSQL.Models
{
    /// <summary>GpsSettingsTbl (single row, SettingsID = 1): System Preferences → GPS &amp; Drivers.</summary>
    public class GpsSettings
    {
        public const string DefaultDepotAddress = "Quaffee AT Buitenverwachting; Klein Constantia Rd; Constantia; Cape Town";
        public const string DefaultTileUrl = "https://tile.openstreetmap.org/{z}/{x}/{y}.png";
        public const string DefaultGeocodeBaseUrl = "https://api.heigit.org";

        public const string ProviderHeigit = "HeiGIT";
        public const string ProviderAzureMaps = "AzureMaps";
        public const string MapOsm = "OSM";
        /// <summary>Azure Maps geocodes may be kept up to 6 months when stored outside Azure; looked up again before then.</summary>
        public const int AzureMapsKeepDays = 180;
        public const int AzureMapsRefreshDaysBefore = 30;

        public string GeocodeProvider { get; set; } = ProviderHeigit;
        /// <summary>HeiGIT key. Never shown back on screen or written to logs.</summary>
        public string GeocodeApiKey { get; set; }
        /// <summary>Azure Maps subscription key. Never shown back on screen or written to logs.</summary>
        public string AzureMapsKey { get; set; }
        public string GeocodeBaseUrl { get; set; } = DefaultGeocodeBaseUrl;
        public int GeocodeDailyLimit { get; set; } = 2500;
        public int GeocodePerMinute { get; set; } = 90;

        public string MapTileUrl { get; set; } = DefaultTileUrl;
        /// <summary>Map shown in the GPS popup: <see cref="MapOsm"/> (MapTileUrl) or <see cref="ProviderAzureMaps"/>.</summary>
        public string MapProvider { get; set; } = MapOsm;
        public string DepotAddress { get; set; } = DefaultDepotAddress;
        public decimal? DepotLatitude { get; set; }
        public decimal? DepotLongitude { get; set; }

        public bool TrackingEnabled { get; set; }
        public int PingIntervalMinutes { get; set; } = 10;
        public bool TrackingWeekdaysOnly { get; set; } = true;
        public string WorkHoursStart { get; set; } = "07:00";
        public string WorkHoursEnd { get; set; } = "18:00";
        public int PingRetentionDays { get; set; } = 30;

        /// <summary>Order each driver's stops by HeiGIT road driving times (needs the HeiGIT key); off = straight lines.</summary>
        public bool UseRoadTimes { get; set; } = true;

        public int AverageSpeedKmh { get; set; } = 30;
        public int ServiceMinutes { get; set; } = 5;
        public int FreshMinutes { get; set; } = 15;
        public int StaleMinutes { get; set; } = 30;

        public DateTime? UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }

        public bool IsAzureMaps => string.Equals(GeocodeProvider, ProviderAzureMaps, StringComparison.OrdinalIgnoreCase);
        public bool HasHeigitKey => !string.IsNullOrWhiteSpace(GeocodeApiKey);
        public bool HasAzureMapsKey => !string.IsNullOrWhiteSpace(AzureMapsKey);
        /// <summary>The chosen geocoder has its key.</summary>
        public bool HasApiKey => IsAzureMaps ? HasAzureMapsKey : HasHeigitKey;
        public string ProviderDisplay => IsAzureMaps ? "Azure Maps" : "HeiGIT (OpenStreetMap)";
        public bool IsAzureMap => string.Equals(MapProvider, ProviderAzureMaps, StringComparison.OrdinalIgnoreCase);
        /// <summary>Azure Maps chosen for the map and its key is saved; otherwise the popup uses MapTileUrl.</summary>
        public bool UseAzureMap => IsAzureMap && HasAzureMapsKey;
        public bool HasDepotPoint => DepotLatitude.HasValue && DepotLongitude.HasValue;
        /// <summary>Road driving times are switched on and the HeiGIT key is saved.</summary>
        public bool RoadTimesAvailable => UseRoadTimes && HasHeigitKey;
    }
}
