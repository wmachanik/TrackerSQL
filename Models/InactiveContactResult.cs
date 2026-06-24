using System;

namespace TrackerSQL.Models
{
    /// <summary>
    /// Result model for inactive contacts that qualify for disabling
    /// (no orders in past 3+ years or never ordered)
    /// </summary>
    public class InactiveContactResult
    {
        /// <summary>Contact ID (formerly CustomerID)</summary>
        public int ContactID { get; set; }

        /// <summary>Company name for display</summary>
        public string CompanyName { get; set; }

        /// <summary>Last order date as formatted string (yyyy-MM-dd or "(none)")</summary>
        public string LastOrderDate { get; set; }
    }
}