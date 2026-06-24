using System;
using System.Collections.Generic;

namespace TrackerSQL.Models
{
    public class ContactToRemindDetails
    {
        public int TCCID { get; set; }
        public long CustomerID { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string ContactTitle { get; set; } = string.Empty;
        public string ContactFirstName { get; set; } = string.Empty;
        public string ContactAltFirstName { get; set; } = string.Empty;
        public int AreaID { get; set; }
        public string EmailAddress { get; set; } = string.Empty;
        public string AltEmailAddress { get; set; } = string.Empty;
        public int CustomerTypeID { get; set; }
        public int EquipTypeID { get; set; }
        public bool TypicallySecToo { get; set; }
        public int PreferredAgentID { get; set; }
        public int SalesAgentID { get; set; }
        public bool UsesFilter { get; set; }
        public bool autofulfill { get; set; }
        public bool enabled { get; set; }
        public bool AlwaysSendChkUp { get; set; }
        public int ReminderCount { get; set; }
        public string Notes { get; set; } = string.Empty;
        public bool RequiresPurchOrder { get; set; }
        public DateTime LastDateSentReminder { get; set; }
        public DateTime NextPreperationDate { get; set; }
        public DateTime NextDeliveryDate { get; set; }
        public DateTime NextCoffee { get; set; }
        public DateTime NextClean { get; set; }
        public DateTime NextFilter { get; set; }
        public DateTime NextDescal { get; set; }
        public DateTime NextService { get; set; }
    }

    public class ContactToRemindWithItems : ContactToRemindDetails
    {
        public List<ItemContactRequires> ItemsContactRequires { get; set; } = new List<ItemContactRequires>();
    }

    public class ItemContactRequires
    {
        public int TCIID { get; set; }
        public int ItemID { get; set; }
        public long CustomerID { get; set; }
        public double ItemQty { get; set; }
        public int ItemPrepID { get; set; }
        public int ItemPackagID { get; set; }
        public bool AutoFulfill { get; set; }
        public bool ReoccurOrder { get; set; }
        public int ReoccurID { get; set; }
    }

    public class CustomerCheckupData
    {
        public long CustomerID { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string ContactFirstName { get; set; } = string.Empty;
        public string ContactAltFirstName { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public string AltEmailAddress { get; set; } = string.Empty;
        public int AreaID { get; set; }
        public int CustomerTypeID { get; set; }
        public bool Enabled { get; set; }
        public int EquipTypeID { get; set; }
        public bool TypicallySecToo { get; set; }
        public int PreferredAgentID { get; set; }
        public int SalesAgentID { get; set; }
        public bool UsesFilter { get; set; }
        public bool AlwaysSendChkUp { get; set; }
        public bool AutoFulfill { get; set; }
        public int ReminderCount { get; set; }
        public DateTime NextCoffee { get; set; }
        public DateTime NextClean { get; set; }
        public DateTime NextDescal { get; set; }
        public DateTime NextFilter { get; set; }
        public DateTime NextService { get; set; }
        public DateTime NextPreperationDate { get; set; }
        public DateTime NextDeliveryDate { get; set; }
    }

    public class CustomerTypicalItem
    {
        public int ItemID { get; set; }
        public double Quantity { get; set; }
        public int PackagingID { get; set; }
    }

    public class CoffeeOrderData
    {
        public long CustomerID { get; set; }
        public long OrderID { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime RequiredByDate { get; set; }
        public int ItemTypeID { get; set; }
    }

    public class ContactMayNeedReminder
    {
        public long ContactID { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string ContactFirstName { get; set; } = string.Empty;
        public string ContactAltFirstName { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public string AltEmailAddress { get; set; } = string.Empty;
        public int AreaID { get; set; }
        public int ContactTypeID { get; set; }
        public int EquipTypeID { get; set; }
        public bool TypicallySecToo { get; set; }
        public int PreferredAgentID { get; set; }
        public int SalesAgentID { get; set; }
        public bool UsesFilter { get; set; }
        public bool AutoFulfill { get; set; }
        public bool AlwaysSendChkUp { get; set; }
        public bool Enabled { get; set; }
        public int ReminderCount { get; set; }
        public bool RequiresPurchOrder { get; set; }
        public DateTime NextCoffeeBy { get; set; }
        public DateTime NextCleanOn { get; set; }
        public DateTime NextFilterEst { get; set; }
        public DateTime NextDescaleEst { get; set; }
        public DateTime NextServiceEst { get; set; }
        public DateTime PrepDate { get; set; }
        public DateTime DeliveryDate { get; set; }
        public DateTime NextPreperationDate { get; set; }
        public DateTime NextDeliveryDate { get; set; }
    }

    public class SendCheckEmailTexts
    {
        public int SCEMTID { get; set; }
        public string Header { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public string Footer { get; set; } = string.Empty;
        public DateTime DateLastChange { get; set; }
        public string Notes { get; set; } = string.Empty;
    }
}
