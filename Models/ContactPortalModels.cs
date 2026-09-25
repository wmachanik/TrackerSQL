using System;
using System.Collections.Generic;

namespace TrackerSQL.Models
{
    public class ContactUserLink
    {
        public int LinkID { get; set; }
        public Guid UserId { get; set; }
        public int ContactID { get; set; }
        public bool MustChangePassword { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
    }

    public static class ContactPortalChangeKinds
    {
        public const string Contact = "Contact";
        public const string Order = "Order";
        public const string Repair = "Repair";
        public const string Recurring = "Recurring";
    }

    public static class ContactPortalChangeStatuses
    {
        public const string Open = "Open";
        public const string Done = "Done";
        public const string Rejected = "Rejected";
    }

    public class ContactPortalChangeRequest
    {
        public long RequestID { get; set; }
        public int ContactID { get; set; }
        public Guid? UserId { get; set; }
        public string Kind { get; set; }
        public long? RelatedId { get; set; }
        public string RequestText { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string ResolvedBy { get; set; }
        public string StaffNote { get; set; }

        /// <summary>Joined for staff queue.</summary>
        public string CompanyName { get; set; }
    }

    /// <summary>Other contact sharing the invite email (for staff to open and review).</summary>
    public class ContactPortalEmailConflict
    {
        public int ContactID { get; set; }
        public string CompanyName { get; set; }
        public bool Enabled { get; set; }
        /// <summary>True when this contact already owns the portal login for the email.</summary>
        public bool HoldsPortalLogin { get; set; }
    }

    public class ContactPortalInviteResult
    {
        public bool Succeeded { get; set; }
        public string Message { get; set; }
        public System.Collections.Generic.List<ContactPortalEmailConflict> OtherContacts { get; set; }
            = new System.Collections.Generic.List<ContactPortalEmailConflict>();
    }

    /// <summary>Display helpers for Contact Portal text.</summary>
    public static class ContactPortalDisplay
    {
        /// <summary>"waiting for part" → "Waiting For Part"; underscores become spaces.</summary>
        public static string TitleCase(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;
            string cleaned = text.Replace('_', ' ').Trim();
            return System.Globalization.CultureInfo.InvariantCulture.TextInfo.ToTitleCase(cleaned.ToLowerInvariant());
        }

        public static string FormatDate(DateTime? value)
        {
            return value.HasValue && value.Value.Year > 1900 ? value.Value.ToString("dd MMM yyyy") : string.Empty;
        }
    }

    /// <summary>Repair as shown to the customer — lookup names resolved, no internal IDs.</summary>
    public class ContactPortalRepairView
    {
        public int RepairID { get; set; }
        public string JobCardNumber { get; set; }
        public DateTime? DateLogged { get; set; }
        public DateTime? LastStatusChange { get; set; }
        public string EquipTypeName { get; set; }
        public string EquipSerialNumber { get; set; }
        public string FaultName { get; set; }
        public string FaultNotes { get; set; }
        public int RepairStatusID { get; set; }
        public string StatusName { get; set; }
        public string StatusNote { get; set; }
        public string Notes { get; set; }

        public string StatusDisplay => ContactPortalDisplay.TitleCase(StatusName);

        public string FaultDisplay
        {
            get
            {
                string name = (FaultName ?? string.Empty).Trim();
                string notes = (FaultNotes ?? string.Empty).Trim();
                if (name.Length == 0) return notes;
                if (notes.Length == 0 || string.Equals(name, notes, StringComparison.OrdinalIgnoreCase)) return name;
                return name + " — " + notes;
            }
        }

        public string DateLoggedDisplay => ContactPortalDisplay.FormatDate(DateLogged);
        public string LastStatusChangeDisplay => ContactPortalDisplay.FormatDate(LastStatusChange);
    }

    /// <summary>Order line as shown to the customer — item and packaging names.</summary>
    public class ContactPortalOrderLine
    {
        public int OrderID { get; set; }
        public string ItemDesc { get; set; }
        public double Qty { get; set; }
        public string PackagingDesc { get; set; }

        /// <summary>"SecretCoffee × 0.25 (250g Beans)".</summary>
        public string Display
        {
            get
            {
                string text = string.IsNullOrWhiteSpace(ItemDesc) ? "(item)" : ItemDesc.Trim();
                if (Qty > 0) text += " × " + Qty.ToString("0.###");
                if (!string.IsNullOrWhiteSpace(PackagingDesc)) text += " (" + PackagingDesc.Trim() + ")";
                return text;
            }
        }
    }

    /// <summary>Courier dispatch details for a customer's order.</summary>
    /// <summary>What "Repeat My Last Order" would place: items, area-schedule dates and any clash.</summary>
    public class ContactPortalRepeatOrderPreview
    {
        public List<ContactPortalOrderLine> Lines { get; set; } = new List<ContactPortalOrderLine>();
        public DateTime PrepDate { get; set; }
        public DateTime DeliveryDate { get; set; }
        public string DeliveryDateDisplay => ContactPortalDisplay.FormatDate(DeliveryDate);
        public bool RequiresPurchaseOrder { get; set; }
        /// <summary>An order already exists for this contact on the delivery date (0 when none).</summary>
        public int ExistingOrderId { get; set; }
    }

    /// <summary>When the customer next needs coffee: the next recurring delivery, or the reminder estimate.</summary>
    public class ContactPortalNextCoffee
    {
        public bool FromRecurring { get; set; }
        public DateTime Date { get; set; }
        public bool IsDueNow { get; set; }
        public string DateDisplay => ContactPortalDisplay.FormatDate(Date);
    }

    /// <summary>Online shop order number, and the shop's "view order" page when the contact has a shop account.</summary>
    public class ContactPortalShopOrder
    {
        public string OrderNumber { get; set; }
        public string ViewUrl { get; set; }
    }

    public class ContactPortalCourierInfo
    {
        public string CourierName { get; set; }
        public string WaybillNumber { get; set; }
        public string TrackingUrl { get; set; }
        public DateTime DispatchedAt { get; set; }
    }

    public class ContactPortalSettings
    {
        public int SettingsID { get; set; } = 1;
        public string EditableContactFields { get; set; }
            = "PhoneNumber,CellNumber,EmailAddress,AltEmailAddress";
        public DateTime UpdatedAt { get; set; }
        public string UpdatedBy { get; set; }
    }
}
