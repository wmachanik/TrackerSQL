using System;
using System.Collections.Generic;

namespace TrackerSQL.Models
{
    public class DeliverySheetOrderRow
    {
        public int OrderID { get; set; }

        public int ContactID { get; set; }

        public string ContactName { get; set; }

        public DateTime? OrderDate { get; set; }

        public DateTime? PrepDate { get; set; }

        public DateTime RequiredByDate { get; set; }

        public int ItemID { get; set; }

        public string ItemDesc { get; set; }

        public string ItemShortName { get; set; }

        public double QtyOrdered { get; set; }

        public bool ItemEnabled { get; set; }

        public int? ReplacementID { get; set; }

        public int DeliveryOrder { get; set; }

        public int SortOrder { get; set; }

        public int? ToBeDeliveredByID { get; set; }

        public string PurchaseOrder { get; set; }

        public bool Confirmed { get; set; }

        public bool InvoiceDone { get; set; }

        public bool Done { get; set; }

        public string Notes { get; set; }

        public string PackDesc { get; set; }

        public string BGColour { get; set; }

        public string DeliveryByAbbreviation { get; set; }

        /// <summary>The contact's area (empty when none).</summary>
        public string AreaName { get; set; }

        /// <summary>The area's own pill code (empty = automatic).</summary>
        public string AreaShortCode { get; set; }

        /// <summary>Order flag (order's own, else the contact's account type).</summary>
        public int? InvoiceTypeID { get; set; }

        public DeliverySheetOrderRow()
        {
            ContactName = string.Empty;
            ItemDesc = string.Empty;
            ItemShortName = string.Empty;
            PurchaseOrder = string.Empty;
            Notes = string.Empty;
            PackDesc = string.Empty;
            BGColour = string.Empty;
            DeliveryByAbbreviation = string.Empty;
            AreaName = string.Empty;
            AreaShortCode = string.Empty;
        }
    }

    public class DeliverySheetDisplayItem
    {
        public int OrderID { get; set; }

        public string ContactID { get; set; }

        public string ContactName { get; set; }

        public string Details { get; set; }

        public string PurchaseOrder { get; set; }

        public bool Done { get; set; }

        public bool InvoiceDone { get; set; }

        public string Items { get; set; }

        public int ContactIDValue { get; set; }

        public DateTime RequiredByDate { get; set; }

        public string Notes { get; set; }

        /// <summary>Order flag pill HTML (empty when the flag has no pill, e.g. Standard).</summary>
        public string FlagPillHtml { get; set; }

        /// <summary>Area pill HTML, screen only (empty when the option is off or there is no area).</summary>
        public string AreaPillHtml { get; set; }

        public DeliverySheetDisplayItem()
        {
            ContactID = string.Empty;
            ContactName = string.Empty;
            Details = string.Empty;
            PurchaseOrder = string.Empty;
            Items = string.Empty;
            Notes = string.Empty;
            FlagPillHtml = string.Empty;
            AreaPillHtml = string.Empty;
        }
    }

    public class DeliverySheetTotal
    {
        public string ItemID { get; set; }

        public string ItemDesc { get; set; }

        public double TotalQty { get; set; }

        public int ItemOrder { get; set; }

        public DeliverySheetTotal()
        {
            ItemID = string.Empty;
            ItemDesc = string.Empty;
        }
    }

    public class DeliveryPersonOption
    {
        public string PersonID { get; set; }

        public string Abbreviation { get; set; }

        public DeliveryPersonOption()
        {
            PersonID = string.Empty;
            Abbreviation = string.Empty;
        }
    }

    public class DeliverySheetBuildResult
    {
        public DeliverySheetBuildResult()
        {
            Items = new List<DeliverySheetDisplayItem>();
            Totals = new List<DeliverySheetTotal>();
            DeliveryPeople = new List<DeliveryPersonOption>();
        }

        public List<DeliverySheetDisplayItem> Items { get; set; }

        public List<DeliverySheetTotal> Totals { get; set; }

        public List<DeliveryPersonOption> DeliveryPeople { get; set; }
    }
}