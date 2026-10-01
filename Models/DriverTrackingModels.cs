using System.Collections.Generic;

namespace TrackerSQL.Models
{
    // JSON for Tools/DriverTracking.aspx (Handlers/DriverTrackingData.ashx), serialised camelCase.
    // Times are South African time "HH:mm".

    public class DriverTrackingBoard
    {
        public string Day { get; set; }
        public string Now { get; set; }
        public bool TrackingEnabled { get; set; }
        public int FreshMinutes { get; set; }
        public int StaleMinutes { get; set; }
        public double? DepotLat { get; set; }
        public double? DepotLng { get; set; }
        public List<DriverTrackingDriver> Drivers { get; set; } = new List<DriverTrackingDriver>();
    }

    public class DriverTrackingDriver
    {
        public int PersonId { get; set; }
        public string Name { get; set; }
        public string Abbreviation { get; set; }
        public double? Lat { get; set; }
        public double? Lng { get; set; }
        public double? Accuracy { get; set; }
        /// <summary>Time of the latest position ("HH:mm"), null when none today.</summary>
        public string LastSeen { get; set; }
        public int? MinutesAgo { get; set; }
        /// <summary>APP_OPEN, PERIODIC or DELIVERY_COMPLETE.</summary>
        public string Source { get; set; }
        /// <summary>green (fresh), orange (stale), red (old) or grey (no position today).</summary>
        public string Colour { get; set; }
        /// <summary>On route, Finished, Offline or No stops.</summary>
        public string Status { get; set; }
        public int StopsDone { get; set; }
        public int StopsLeft { get; set; }
        /// <summary>Remaining stops without a GPS point (no ETA).</summary>
        public int StopsNoPoint { get; set; }
        /// <summary>Estimated time the last stop is done ("HH:mm").</summary>
        public string FinishEta { get; set; }
        /// <summary>The ETAs start from the driver's position, else from the depot.</summary>
        public bool FromDepot { get; set; }
        public List<DriverTrackingStop> Stops { get; set; } = new List<DriverTrackingStop>();
    }

    public class DriverTrackingStop
    {
        public int Seq { get; set; }
        public int ContactId { get; set; }
        public string Company { get; set; }
        public string Area { get; set; }
        public int Orders { get; set; }
        public bool Done { get; set; }
        public double? Lat { get; set; }
        public double? Lng { get; set; }
        public string Eta { get; set; }
    }
}
