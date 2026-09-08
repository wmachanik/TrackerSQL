using System;

namespace TrackerSQL.Models
{
    /// <summary>User choices from the Woo Import “Update contact” checklist.</summary>
    [Serializable]
    public class WooContactUpdateOptions
    {
        /// <summary>CareOfPrefix or UpdateName; blank uses General setting when company is updated.</summary>
        public string CompanyNameMode { get; set; }

        /// <summary>Apply company name policy (c/o or rename).</summary>
        public bool UpdateCompany { get; set; }

        /// <summary>Billing address, postcode, country/province, and missing area.</summary>
        public bool UpdateAddress { get; set; }

        /// <summary>Update PhoneNumber (Tel) from Woo — skipped when Woo matches CellNumber.</summary>
        public bool UpdatePhone { get; set; }

        /// <summary>Set AltEmailAddress from Woo email when it differs from primary and alt.</summary>
        public bool UpdateAltEmail { get; set; }

        /// <summary>First/last name from Woo shipping.</summary>
        public bool UpdatePersonNames { get; set; }
    }

    /// <summary>Which checklist rows to show (and default checked) for Update contact.</summary>
    [Serializable]
    public class WooContactUpdateOffer
    {
        public long WooOrderId { get; set; }
        public string WooOrderNumber { get; set; }
        public string SummaryHtml { get; set; }

        public bool ShowCompanyChoice { get; set; }
        public string WooCompanyName { get; set; }
        public string ContactCompanyName { get; set; }
        public string DefaultCompanyNameMode { get; set; }

        public bool OfferAddress { get; set; }
        public string AddressFrom { get; set; }
        public string AddressTo { get; set; }

        public bool OfferPhone { get; set; }
        public string PhoneFrom { get; set; }
        public string PhoneTo { get; set; }
        public string PhoneSkipReason { get; set; }

        public bool OfferAltEmail { get; set; }
        public string AltEmailFrom { get; set; }
        public string AltEmailTo { get; set; }

        public bool OfferPersonNames { get; set; }
        public string PersonNamesSummary { get; set; }

        public bool HasAnyOffer =>
            ShowCompanyChoice || OfferAddress || OfferPhone || OfferAltEmail || OfferPersonNames;
    }
}
