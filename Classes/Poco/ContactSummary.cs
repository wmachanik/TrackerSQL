using System;

namespace TrackerDotNet.Classes.Poco
{
    /// <summary>
    /// Lightweight summary projection for listing contacts.
    /// NOTE: We intentionally keep legacy property names (CustomerID etc.)
    /// to minimise page markup changes while migrating from Customers to Contacts.
    /// </summary>
    public class ContactSummary
    {
        public int CustomerID { get; set; }            // Alias for ContactID
        public string CompanyName { get; set; }
        public string ContactFirstName { get; set; }
        public string ContactLastName { get; set; }
        public string City { get; set; }                // Area/City display name
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public string DeliveryBy { get; set; }          // Abbreviation of preferred agent
        public string EquipTypeName { get; set; }
        public string MachineSN { get; set; }
        public bool autofulfill { get; set; }
        public bool enabled { get; set; }
    }
}
