//------------------------------------------------------------------------------
// TrackerSQL v3.x — ContactToRemindWithItems
// Data access / control type: ContactToRemindWithItems.
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Data;
using TrackerSQL.Classes;

//- only form later versions #nullable disable
namespace TrackerSQL.Controls
{
    [Obsolete("DO NOT USE Comtrols use Models - MIGRATION IN PROGRESS", true)]
    public class ContactToRemindWithItems : ContactToRemindDetails
    {
        private const string CONST_SQL_CUSTOMERSUSAGE_SELECT = "SELECT CustomersTbl.CustomerID, CustomersTbl.CompanyName, CustomersTbl.ContactTitle,   CustomersTbl.ContactFirstName, CustomersTbl.ContactAltFirstName, CustomersTbl.PostalCode, CustomersTbl.EmailAddress,  CustomersTbl.AltEmailAddress, CustomersTbl.CustomerTypeID, CustomersTbl.PriPrefQty, CustomersTbl.TypicallySecToo,  CustomersTbl.PreferedAgent, CustomersTbl.SalesAgentID, CustomersTbl.UsesFilter, CustomersTbl.autofulfill,   CustomersTbl.enabled, CustomersTbl.PredictionDisabled, CustomersTbl.AlwaysSendChkUp, CustomersTbl.ReminderCount,  CustomersTbl.LastDateSentReminder, CustomersAccInfoTbl.RequiresPurchOrder,  NextPreperationDateByAreaTbl.AreaID, NextPreperationDateByAreaTbl.DeliveryDate, NextPreperationDateByAreaTbl.PreperationDate,  ClientUsageTbl.NextCoffeeBy, ClientUsageTbl.NextCleanOn, ClientUsageTbl.NextFilterEst, ClientUsageTbl.NextDescaleEst, ClientUsageTbl.NextServiceEst FROM (((CustomersAccInfoTbl RIGHT OUTER JOIN CustomersTbl ON CustomersAccInfoTbl.CustomerID = CustomersTbl.CustomerID) LEFT OUTER JOIN ClientUsageTbl ON CustomersTbl.CustomerID = ClientUsageTbl.CustomerID) LEFT OUTER JOIN NextPreperationDateByAreaTbl ON CustomersTbl.AreaID = NextPreperationDateByAreaTbl.AreaID) WHERE (CustomersTbl.CustomerID = ?)";
        private List<ItemContactRequires> _ItemsContactRequires;

        public ContactToRemindWithItems() => this._ItemsContactRequires = new List<ItemContactRequires>();

        public virtual List<ItemContactRequires> ItemsContactRequires
        {
            get => this._ItemsContactRequires;
            set => this._ItemsContactRequires = value;
        }

        public ContactToRemindWithItems GetCustomerDetails(long pCustomerID)
        {
            ContactToRemindWithItems customerDetails = (ContactToRemindWithItems)null;
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddWhereParams((object)pCustomerID, DbType.Int64);
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader("SELECT CustomersTbl.CustomerID, CustomersTbl.CompanyName, CustomersTbl.ContactTitle,   CustomersTbl.ContactFirstName, CustomersTbl.ContactAltFirstName, CustomersTbl.PostalCode, CustomersTbl.EmailAddress,  CustomersTbl.AltEmailAddress, CustomersTbl.CustomerTypeID, CustomersTbl.PriPrefQty, CustomersTbl.TypicallySecToo,  CustomersTbl.PreferedAgent, CustomersTbl.SalesAgentID, CustomersTbl.UsesFilter, CustomersTbl.autofulfill,   CustomersTbl.enabled, CustomersTbl.PredictionDisabled, CustomersTbl.AlwaysSendChkUp, CustomersTbl.ReminderCount,  CustomersTbl.LastDateSentReminder, CustomersAccInfoTbl.RequiresPurchOrder,  NextPreperationDateByAreaTbl.AreaID, NextPreperationDateByAreaTbl.DeliveryDate, NextPreperationDateByAreaTbl.PreperationDate,  ClientUsageTbl.NextCoffeeBy, ClientUsageTbl.NextCleanOn, ClientUsageTbl.NextFilterEst, ClientUsageTbl.NextDescaleEst, ClientUsageTbl.NextServiceEst FROM (((CustomersAccInfoTbl RIGHT OUTER JOIN CustomersTbl ON CustomersAccInfoTbl.CustomerID = CustomersTbl.CustomerID) LEFT OUTER JOIN ClientUsageTbl ON CustomersTbl.CustomerID = ClientUsageTbl.CustomerID) LEFT OUTER JOIN NextPreperationDateByAreaTbl ON CustomersTbl.AreaID = NextPreperationDateByAreaTbl.AreaID) WHERE (CustomersTbl.CustomerID = ?)");
            if (dataReader != null)
            {
                if (dataReader.Read())
                {
                    customerDetails = new ContactToRemindWithItems();
                    customerDetails.CustomerID = dataReader["CustomerID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["CustomerID"]);
                    customerDetails.CompanyName = dataReader["CompanyName"] == DBNull.Value ? string.Empty : dataReader["CompanyName"].ToString();
                    customerDetails.ContactTitle = dataReader["ContactTitle"] == DBNull.Value ? string.Empty : dataReader["ContactTitle"].ToString();
                    customerDetails.ContactFirstName = dataReader["ContactFirstName"] == DBNull.Value ? string.Empty : dataReader["ContactFirstName"].ToString();
                    customerDetails.ContactAltFirstName = dataReader["ContactAltFirstName"] == DBNull.Value ? string.Empty : dataReader["ContactAltFirstName"].ToString();
                    customerDetails.EmailAddress = dataReader["EmailAddress"] == DBNull.Value ? string.Empty : dataReader["EmailAddress"].ToString();
                    customerDetails.AltEmailAddress = dataReader["AltEmailAddress"] == DBNull.Value ? string.Empty : dataReader["AltEmailAddress"].ToString();
                    customerDetails.CustomerTypeID = dataReader["CustomerTypeID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["CustomerTypeID"]);
                    customerDetails.TypicallySecToo = dataReader["TypicallySecToo"] != DBNull.Value && Convert.ToBoolean(dataReader["TypicallySecToo"]);
                    customerDetails.PreferredAgentID = dataReader["PreferedAgent"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["PreferedAgent"]);
                    customerDetails.SalesAgentID = dataReader["SalesAgentID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["SalesAgentID"]);
                    customerDetails.UsesFilter = dataReader["UsesFilter"] != DBNull.Value && Convert.ToBoolean(dataReader["UsesFilter"]);
                    customerDetails.autofulfill = dataReader["autofulfill"] != DBNull.Value && Convert.ToBoolean(dataReader["autofulfill"]);
                    customerDetails.enabled = dataReader["enabled"] != DBNull.Value && Convert.ToBoolean(dataReader["enabled"]);
                    customerDetails.AlwaysSendChkUp = dataReader["AlwaysSendChkUp"] != DBNull.Value && Convert.ToBoolean(dataReader["AlwaysSendChkUp"]);
                    customerDetails.ReminderCount = dataReader["ReminderCount"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ReminderCount"]);
                    customerDetails.LastDateSentReminder = dataReader["LastDateSentReminder"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(dataReader["LastDateSentReminder"]).Date;
                    customerDetails.RequiresPurchOrder = dataReader["RequiresPurchOrder"] != DBNull.Value && Convert.ToBoolean(dataReader["RequiresPurchOrder"]);
                    customerDetails.AreaID = dataReader["AreaID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["AreaID"]);
                    customerDetails.NextDeliveryDate = dataReader["DeliveryDate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(dataReader["DeliveryDate"]).Date;
                    customerDetails.NextPreperationDate = dataReader["PreperationDate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(dataReader["PreperationDate"]).Date;
                    customerDetails.NextCoffee = dataReader["NextCoffeeBy"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(dataReader["NextCoffeeBy"]).Date;
                    customerDetails.NextClean = dataReader["NextCleanOn"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(dataReader["NextCleanOn"]).Date;
                    customerDetails.NextFilter = dataReader["NextFilterEst"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(dataReader["NextFilterEst"]).Date;
                    customerDetails.NextDescal = dataReader["NextDescaleEst"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(dataReader["NextDescaleEst"]).Date;
                    customerDetails.NextService = dataReader["NextServiceEst"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(dataReader["NextServiceEst"]).Date;
                }
                dataReader.Close();
            }
            trackerDb.Close();
            return customerDetails;
        }
    }
}
