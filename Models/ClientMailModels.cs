using System;
using System.Collections.Generic;

namespace TrackerSQL.Models
{
    public class ClientMailRecipient
    {
        public int ContactID { get; set; }
        public string CompanyName { get; set; }
        public string ContactFirstName { get; set; }
        public string ContactLastName { get; set; }
        public string EmailAddress { get; set; }
        public string AltEmailAddress { get; set; }
        public string ContactTypeDesc { get; set; }
        public string EquipTypeName { get; set; }
        public string AreaName { get; set; }
        public DateTime? LastOrderDate { get; set; }
        public DateTime? NextCoffeeBy { get; set; }
        public string LastItemsOrdered { get; set; }
        public bool IsRecurring { get; set; }

        public string ToEmail
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(EmailAddress))
                    return EmailAddress.Trim();
                return (AltEmailAddress ?? string.Empty).Trim();
            }
        }

        public string DisplayName
        {
            get
            {
                string first = (ContactFirstName ?? string.Empty).Trim();
                if (!string.IsNullOrEmpty(first))
                    return first;
                return CompanyName ?? string.Empty;
            }
        }
    }

    public class ClientMailFilter
    {
        public string NameContains { get; set; }
        public int? ContactTypeID { get; set; }
        public DateTime? LastOrderedFrom { get; set; }
        public DateTime? LastOrderedTo { get; set; }
        public DateTime? NextRequiredFrom { get; set; }
        public DateTime? NextRequiredTo { get; set; }
        public int? EquipTypeID { get; set; }
        public List<int> AreaIds { get; set; }
        /// <summary>null = any, true = has enabled recurring order, false = none.</summary>
        public bool? IsRecurring { get; set; }
        public List<int> LastItemIds { get; set; }
        public string CustomWhere { get; set; }
        public bool EnabledOnly { get; set; } = true;
        public bool MustHaveEmail { get; set; } = true;
    }
}
