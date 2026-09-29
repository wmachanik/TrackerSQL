using System.Collections.Generic;

namespace TrackerSQL.Models
{
    /// <summary>
    /// Lookups → Delivery Days: which areas are delivered on each weekday, in delivery order.
    /// Each area has one delivery order; every day lists its areas in that order.
    /// </summary>
    public class AreaWeekBoard
    {
        /// <summary>Monday first, Sunday last.</summary>
        public List<AreaWeekDay> Days { get; set; } = new List<AreaWeekDay>();

        /// <summary>Every area with a delivery day, in delivery order (DayOfWeekID 0).</summary>
        public AreaWeekDay AllAreas { get; set; } = new AreaWeekDay();

        /// <summary>Areas with no prep-day rows — never delivered.</summary>
        public List<Area> Unassigned { get; set; } = new List<Area>();
    }

    public class AreaWeekDay
    {
        /// <summary>1=Sun..7=Sat (same numbering as AreaPrepDaysTbl.PrepDayOfWeekID); 0 = all areas.</summary>
        public int DayOfWeekID { get; set; }
        public string DayName { get; set; }
        public List<AreaWeekCard> Cards { get; set; } = new List<AreaWeekCard>();

        /// <summary>Areas the website promises for this day that have no prep-day row delivering on it.</summary>
        public List<AreaPromiseGap> PromiseGaps { get; set; } = new List<AreaPromiseGap>();
    }

    /// <summary>One area in the list of a delivery weekday (or of all areas).</summary>
    public class AreaWeekCard
    {
        public int AreaID { get; set; }
        public string AreaName { get; set; }

        /// <summary>The area's delivery order (the same on all of its prep-day rows).</summary>
        public int? DeliveryOrder { get; set; }

        /// <summary>The area's prep-day rows that deliver on this day (all of them on the all-areas list).</summary>
        public List<AreaWeekPrep> Preps { get; set; } = new List<AreaWeekPrep>();

        /// <summary>Delivery weekdays of the area (1=Sun..7=Sat), Monday first.</summary>
        public List<int> DeliveryDays { get; set; } = new List<int>();

        /// <summary>The same area is delivered more than once on this day (two prep rows land here).</summary>
        public bool DuplicateOnDay { get; set; }

        /// <summary>Short text for the delivery promise rules that match this area/day (empty when none).</summary>
        public string PromiseText { get; set; }
    }

    public class AreaWeekPrep
    {
        public int PrepDayOfWeekID { get; set; }
        public int DeliveryDelayDays { get; set; }
    }

    public class AreaPromiseGap
    {
        public int AreaID { get; set; }
        public string AreaName { get; set; }
        public string PromiseText { get; set; }
    }
}
