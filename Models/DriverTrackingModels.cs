using System.Collections.Generic;

namespace TrackerSQL.Models
{
    // JSON for Tools/DriverTracking.aspx (Handlers/DriverTrackingData.ashx), serialised camelCase.
    // Times are South African time "HH:mm".

    public class DriverTrackingBoard
    {
        public string Day { get; set; }
        /// <summary>True when <see cref="Day"/> is today (live ETAs and auto-refresh).</summary>
        public bool IsToday { get; set; }
        public string Now { get; set; }
        public bool TrackingEnabled { get; set; }
        public int FreshMinutes { get; set; }
        public int StaleMinutes { get; set; }
        public double? DepotLat { get; set; }
        public double? DepotLng { get; set; }
        public List<DriverTrackingDriver> Drivers { get; set; } = new List<DriverTrackingDriver>();
        /// <summary>Orders on the route after resolving deliver-by (diagnostics).</summary>
        public int StopCount { get; set; }
        /// <summary>Raw delivery sheet lines for the day (diagnostics).</summary>
        public int SheetRows { get; set; }
        /// <summary>Distinct orders on RequiredByDate (diagnostics).</summary>
        public int OrderCount { get; set; }
    }

    /// <summary>One order due on a day (driver tracking load from OrdersTbl).</summary>
    public class DriverTrackingOrderLine
    {
        public int OrderId { get; set; }
        public int ContactId { get; set; }
        public string CompanyName { get; set; }
        public int? ToBeDeliveredById { get; set; }
        public int? DeliverToAddressId { get; set; }
        public string DeliveryByAbbreviation { get; set; }
        public string AreaName { get; set; }
        public bool Done { get; set; }
        public string DeliverToLabel { get; set; }
        public string DeliverToPerson { get; set; }
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
        /// <summary>Estimated time back at the depot after the last stop ("HH:mm").</summary>
        public string FinishEta { get; set; }
        /// <summary>True when today's ETAs are held until the driver leaves the depot (or delivers).</summary>
        public bool WaitingDepot { get; set; }
        /// <summary>The ETAs start from the driver's position, else from the depot (no phone position today).</summary>
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
        /// <summary>Done without the driver going there, e.g. "Collected by the client".</summary>
        public string NotAtClient { get; set; }
        public double? Lat { get; set; }
        public double? Lng { get; set; }
        public string Eta { get; set; }
        /// <summary>Return leg to the depot (not a customer stop).</summary>
        public bool IsDepot { get; set; }
    }
}
