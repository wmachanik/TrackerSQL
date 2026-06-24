using System;

namespace TrackerSQL.Classes.Poco
{
    /// <summary>
    /// POCO for ContactsItemsPredicted
    /// Auto-generated: 2026-06-02 13:35:40
    /// Source: Controls\ClientUsageTbl.cs
    /// </summary>
    public class ContactsItemsPredicted : ILookupEntity
    {
        // TODO: Add properties from legacy class or database schema
        // Example:
        // public int ContactsItemsPredictedID { get; set; }
        // public string Name { get; set; }
        // public bool Enabled { get; set; }

        // ILookupEntity implementation
        public int ID 
        { 
            get 
            { 
                // TODO: Return the primary key property
                // Example: return ContactsItemsPredictedID;
                throw new NotImplementedException("Set ID property in ContactsItemsPredicted.cs");
            } 
        }

        public string DisplayText 
        { 
            get 
            { 
                // TODO: Return display text for dropdowns/lists
                // Example: return Name;
                throw new NotImplementedException("Set DisplayText property in ContactsItemsPredicted.cs");
            } 
        }
    }
}
