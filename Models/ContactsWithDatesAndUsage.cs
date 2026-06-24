using System;

namespace TrackerSQL.Classes.Poco
{
    /// <summary>
    /// POCO for ContactsWithDatesAndUsage
    /// Auto-generated: 2026-06-02 13:35:40
    /// Source: Controls\CustomersWithDatesAndUsageTbl.cs
    /// </summary>
    public class ContactsWithDatesAndUsage : ILookupEntity
    {
        // TODO: Add properties from legacy class or database schema
        // Example:
        // public int ContactsWithDatesAndUsageID { get; set; }
        // public string Name { get; set; }
        // public bool Enabled { get; set; }

        // ILookupEntity implementation
        public int ID 
        { 
            get 
            { 
                // TODO: Return the primary key property
                // Example: return ContactsWithDatesAndUsageID;
                throw new NotImplementedException("Set ID property in ContactsWithDatesAndUsage.cs");
            } 
        }

        public string DisplayText 
        { 
            get 
            { 
                // TODO: Return display text for dropdowns/lists
                // Example: return Name;
                throw new NotImplementedException("Set DisplayText property in ContactsWithDatesAndUsage.cs");
            } 
        }
    }
}
