using System;

namespace TrackerDotNet.Classes.Poco
{
    public class Contact
    {
        public int ContactID { get; set; }
        public string CountryOrRegion { get; set; }
        public string CompanyName { get; set; }
        public string ContactTitle { get; set; }
        public string ContactFirstName { get; set; }
        public string ContactLastName { get; set; }
        public string ContactAltFirstName { get; set; }
        public string ContactAltLastName { get; set; }
        public string Department { get; set; }
        public string BillingAddress { get; set; }
        public int? Area { get; set; }
        public string StateOrProvince { get; set; }
        public string PostalCode { get; set; }
        public string CountryRegion { get; set; }
        public string PhoneNumber { get; set; }
        public string Extension { get; set; }
        public string FaxNumber { get; set; }
        public string CellNumber { get; set; }
        public string EmailAddress { get; set; }
        public string AltEmailAddress { get; set; }
        public string ContractNo { get; set; }
        public int? ContactTypeID { get; set; }
        public int? EquipTypeID { get; set; }
        public int? ItemPrefID { get; set; }
        public double? PriPrefQty { get; set; }
        public int? PrefItemPrepTypeID { get; set; }
        public int? PrefItemPackagingID { get; set; }
        public int? SecondaryItemPrefID { get; set; }
        public double? SecPrefQty { get; set; }
        public bool? TypicallySecToo { get; set; }
        public int? PreferedAgentID { get; set; }
        public int? SalesAgentID { get; set; }
        public string MachineSN { get; set; }
        public bool? UsesFilter { get; set; }
        public bool? AutoFulfill { get; set; }
        public bool? Enabled { get; set; }
        public bool? PredictionDisabled { get; set; }
        public bool? AlwaysSendChkUp { get; set; }
        public bool? NormallyResponds { get; set; }
        public int? ReminderCount { get; set; }
        public string Notes { get; set; }
        public bool? SendDeliveryConfirmation { get; set; }
        public DateTime? LastDateSentReminder { get; set; }
    }
}
