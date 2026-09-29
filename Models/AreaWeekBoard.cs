using System.Collections.Generic;

namespace TrackerSQL.Models
{
    /// <summary>
    /// Lookups → Delivery Days: which areas are delivered on each weekday, in delivery order.
    /// </summary>
    public class AreaWeekBoard
    {
        /// <summary>Monday first, Sunday last.</summary>
        public List<AreaWeekDay> Days { get; set; } = new List<AreaWeekDay>();

        /// <summary>Areas with no prep-day rows — never delivered.</summary>
        public List<Area> Unassigned { get; set; } = new List<Area>();
    }

    public class AreaWeekDay
    {
        /// <summary>1=Sun..7=Sat (same numbering as AreaPrepDaysTbl.PrepDayOfWeekID).</summary>
        public int DayOfWeekID { get; set; }
        public string DayName { get; set; }
        public List<AreaWeekCard> Cards { get; set; } = new List<AreaWeekCard>();

        /// <summary>Areas the website promises for this day that have no prep-day row delivering on it.</summary>
        public List<AreaPromiseGap> PromiseGaps { get; set; } = new List<AreaPromiseGap>();
    }

    /// <summary>One AreaPrepDaysTbl row shown in the column of its delivery weekday.</summary>
    public class AreaWeekCard
    {
        public int AreaPrepDaysID { get; set; }
        public int AreaID { get; set; }
        public string AreaName { get; set; }
        public int PrepDayOfWeekID { get; set; }
        public int DeliveryDelayDays { get; set; }
        public int? DeliveryOrder { get; set; }

        /// <summary>Another card on the same day has the same delivery order.</summary>
        public bool OrderTied { get; set; }

        /// <summary>The same area is delivered more than once on this day (two prep rows land here).</summary>
        public bool DuplicateOnDay { get; set; }

        /// <summary>Short text for the delivery promise rules that match this area/day (empty when none).</summary>
        public string PromiseText { get; set; }
    }

    public class AreaPromiseGap
    {
        public int AreaID { get; set; }
        public string AreaName { get; set; }
        public string PromiseText { get; set; }
    }
}
