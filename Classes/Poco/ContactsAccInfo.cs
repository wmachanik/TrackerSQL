using System;

namespace TrackerDotNet.Classes.Poco
{
    public class ContactsAccInfo
    {
        public int ContactsAccInfoID { get; set; }
        public int ContactID { get; set; }
        public bool? RequiresPurchOrder { get; set; }
        public string ContactVATNo { get; set; }
        public string BillAddr1 { get; set; }
        public string BillAddr2 { get; set; }
        public string BillAddr3 { get; set; }
        public string BillAddr4 { get; set; }
        public string BillAddr5 { get; set; }
        public string ShipAddr1 { get; set; }
        public string ShipAddr2 { get; set; }
        public string ShipAddr3 { get; set; }
        public string ShipAddr4 { get; set; }
        public string ShipAddr5 { get; set; }
        public string AccEmail { get; set; }
        public string AltAccEmail { get; set; }
        public int? PaymentTermID { get; set; }
        public double? Limit { get; set; }
        public string FullCoName { get; set; }
        public string AccFirstName { get; set; }
        public string AccLastName { get; set; }
        public string AltAccFirstName { get; set; }
        public string AltAccLastName { get; set; }
        public int? PriceLevelID { get; set; }
        public int? InvoiceTypeID { get; set; }
        public string RegNo { get; set; }
        public string BankAccNo { get; set; }
        public string BankBranch { get; set; }
        public bool? Enabled { get; set; }
        public string Notes { get; set; }
    }
}
