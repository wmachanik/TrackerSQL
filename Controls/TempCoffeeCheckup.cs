// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.control.TempCoffeeCheckup
// Assembly: TrackerDotNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerDotNet.dll

using System;
using System.Collections.Generic;
using System.Data;
using TrackerDotNet.Classes;

//- only form later versions #nullable disable
namespace TrackerDotNet.Controls
{
    public class TempCoffeeCheckup
    {
        private const string CONST_SQL_SELECTALLCONTACTS = "SELECT TCCID, CustomerID, CompanyName, ContactFirstName, ContactAltFirstName, CityID, EmailAddress, AltEmailAddress, CustomerTypeID, EquipTypeID, TypicallySecToo,  PreferedAgentID, SalesAgentID, UsesFilter, [enabled], AlwaysSendChkUp, RequiresPurchOrder, ReminderCount, NextPrepDate, NextDeliveryDate, NextCoffee, NextClean, NextFilter,  NextDescal, NextService FROM TempCoffeecheckupCustomerTbl";
        private const string CONST_SQL_SELECTCONTACTITEMSBYCUST = "SELECT TCIID, CustomerID, ItemID, ItemQty, ItemPrepID, ItemPackagID, AutoFulfill, ReoccurOrderId  FROM TempCoffeecheckupItemsTbl WHERE CustomerID = ?";
        private const string CONST_SQL_INSERTNEWCONTACTS = "INSERT INTO TempCoffeecheckupCustomerTbl (CustomerID, CompanyName, ContactFirstName, ContactAltFirstName, CityID, EmailAddress, AltEmailAddress, CustomerTypeID, EquipTypeID, TypicallySecToo,   PreferedAgentID, SalesAgentID, UsesFilter, [enabled], AlwaysSendChkUp, RequiresPurchOrder, ReminderCount, NextPrepDate, NextDeliveryDate, NextCoffee, NextClean, NextFilter,   NextDescal, NextService) VALUES (?, ? ,? ,? ,? ,? ,? ,? ,? ,? ,? ,? ,? ,? ,? ,? ,? ,? ,? ,? ,? ,? ,? ,?)";
        private const string CONST_SQL_INSERTNEWITEMS = "INSERT INTO TempCoffeecheckupItemsTbl  (CustomerID, ItemID, ItemQty, ItemPrepID, ItemPackagID, AutoFulfill, ReoccurOrderId)  VALUES ( ?, ? ,? ,? ,? ,? ,? )";
        private const string CONST_SQL_DELETEALLCONTACTS = "DELETE * FROM TempCoffeecheckupCustomerTbl";
        private const string CONST_SQL_DELETEALLITEMS = "DELETE * FROM TempCoffeecheckupItemsTbl";

        public List<ContactToRemindDetails> GetAllContacts() => this.GetAllContacts(string.Empty);

        public List<ContactToRemindDetails> GetAllContacts(string SortBy)
        {
            List<ContactToRemindDetails> allContacts = new List<ContactToRemindDetails>();
            string strSQL = "SELECT TCCID, CustomerID, CompanyName, ContactFirstName, ContactAltFirstName, CityID, EmailAddress, AltEmailAddress, CustomerTypeID, EquipTypeID, TypicallySecToo,  PreferedAgentID, SalesAgentID, UsesFilter, [enabled], AlwaysSendChkUp, RequiresPurchOrder, ReminderCount, NextPrepDate, NextDeliveryDate, NextCoffee, NextClean, NextFilter,  NextDescal, NextService FROM TempCoffeecheckupCustomerTbl" + (!string.IsNullOrEmpty(SortBy) ? " ORDER BY " + SortBy : " ORDER BY CompanyName");
            TrackerDb trackerDb = new TrackerDb();
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(strSQL);
            if (dataReader != null)
            {
                while (dataReader.Read())
                    allContacts.Add(new ContactToRemindDetails()
                    {
                        TCCID = dataReader["TCCID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["TCCID"]),
                        CustomerID = dataReader["CustomerID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["CustomerID"]),
                        CompanyName = dataReader["CompanyName"] == DBNull.Value ? string.Empty : dataReader["CompanyName"].ToString(),
                        ContactFirstName = dataReader["ContactFirstName"] == DBNull.Value ? string.Empty : dataReader["ContactFirstName"].ToString(),
                        ContactAltFirstName = dataReader["ContactAltFirstName"] == DBNull.Value ? string.Empty : dataReader["ContactAltFirstName"].ToString(),
                        CityID = dataReader["CityID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["CityID"]),
                        EmailAddress = dataReader["EmailAddress"] == DBNull.Value ? string.Empty : dataReader["EmailAddress"].ToString(),
                        AltEmailAddress = dataReader["AltEmailAddress"] == DBNull.Value ? string.Empty : dataReader["AltEmailAddress"].ToString(),
                        CustomerTypeID = dataReader["CityID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["CityID"]),
                        EquipTypeID = dataReader["EquipTypeID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["EquipTypeID"]),
                        TypicallySecToo = dataReader["TypicallySecToo"] != DBNull.Value && Convert.ToBoolean(dataReader["TypicallySecToo"]),
                        PreferedAgentID = dataReader["PreferedAgentID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["PreferedAgentID"]),
                        SalesAgentID = dataReader["SalesAgentID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["SalesAgentID"]),
                        UsesFilter = dataReader["UsesFilter"] != DBNull.Value && Convert.ToBoolean(dataReader["UsesFilter"]),
                        enabled = dataReader["enabled"] != DBNull.Value && Convert.ToBoolean(dataReader["enabled"]),
                        AlwaysSendChkUp = dataReader["AlwaysSendChkUp"] != DBNull.Value && Convert.ToBoolean(dataReader["AlwaysSendChkUp"]),
                        RequiresPurchOrder = dataReader["RequiresPurchOrder"] != DBNull.Value && Convert.ToBoolean(dataReader["RequiresPurchOrder"]),
                        ReminderCount = dataReader["ReminderCount"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ReminderCount"]),
                        NextPrepDate = dataReader["NextPrepDate"] == DBNull.Value ? DateTime.MaxValue : Convert.ToDateTime(dataReader["NextPrepDate"]).Date,
                        NextDeliveryDate = dataReader["NextDeliveryDate"] == DBNull.Value ? DateTime.MaxValue : Convert.ToDateTime(dataReader["NextDeliveryDate"]).Date,
                        NextCoffee = dataReader["NextCoffee"] == DBNull.Value ? DateTime.MaxValue : Convert.ToDateTime(dataReader["NextCoffee"]).Date,
                        NextClean = dataReader["NextClean"] == DBNull.Value ? DateTime.MaxValue : Convert.ToDateTime(dataReader["NextClean"]).Date,
                        NextFilter = dataReader["NextFilter"] == DBNull.Value ? DateTime.MaxValue : Convert.ToDateTime(dataReader["NextFilter"]).Date,
                        NextDescal = dataReader["NextDescal"] == DBNull.Value ? DateTime.MaxValue : Convert.ToDateTime(dataReader["NextDescal"]).Date,
                        NextService = dataReader["NextService"] == DBNull.Value ? DateTime.MaxValue : Convert.ToDateTime(dataReader["NextService"]).Date
                    });
                dataReader.Close();
            }
            trackerDb.Close();
            return allContacts;
        }

        public List<ItemContactRequires> GetAllContactItems(long CustomerID, string SortBy)
        {
            List<ItemContactRequires> allContactItems = new List<ItemContactRequires>();
            if (CustomerID > 0L)
            {
                string strSQL = "SELECT TCIID, CustomerID, ItemID, ItemQty, ItemPrepID, ItemPackagID, AutoFulfill, ReoccurOrderId  FROM TempCoffeecheckupItemsTbl WHERE CustomerID = ?" + (!string.IsNullOrEmpty(SortBy) ? " ORDER BY " + SortBy : " ORDER BY ItemID");
                TrackerDb trackerDb = new TrackerDb();
                trackerDb.AddWhereParams((object)CustomerID, DbType.Int64, "@CustomerID");
                IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(strSQL);
                if (dataReader != null)
                {
                    while (dataReader.Read())
                    {
                        ItemContactRequires itemContactRequires = new ItemContactRequires()
                        {
                            TCIID = dataReader["TCIID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["TCIID"]),
                            CustomerID = dataReader[nameof(CustomerID)] == DBNull.Value ? 0 : Convert.ToInt32(dataReader[nameof(CustomerID)]),
                            ItemID = dataReader["ItemID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ItemID"]),
                            ItemQty = dataReader["ItemQty"] == DBNull.Value ? 0.0 : Convert.ToDouble(dataReader["ItemQty"]),
                            ItemPrepID = dataReader["ItemPrepID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ItemPrepID"]),
                            ItemPackagID = dataReader["ItemPackagID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ItemPackagID"]),
                            AutoFulfill = dataReader["AutoFulfill"] != DBNull.Value && Convert.ToBoolean(dataReader["AutoFulfill"]),
                            ReoccurID = dataReader["ReoccurOrderID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ReoccurOrderID"])
                        };
                        itemContactRequires.ReoccurOrder = itemContactRequires.ReoccurID > 0;
                        allContactItems.Add(itemContactRequires);
                    }
                    dataReader.Close();
                }
                trackerDb.Close();
            }
            return allContactItems;
        }

        public List<ContactToRemindWithItems> GetAllContactAndItems()
        {
            return this.GetAllContactAndItems(string.Empty);
        }

        public List<ContactToRemindWithItems> GetAllContactAndItems(string SortBy)
        {
            List<ContactToRemindWithItems> allContactAndItems = new List<ContactToRemindWithItems>();
            string strSQL = "SELECT TCCID, CustomerID, CompanyName, ContactFirstName, ContactAltFirstName, CityID, EmailAddress, AltEmailAddress, CustomerTypeID, EquipTypeID, TypicallySecToo,  PreferedAgentID, SalesAgentID, UsesFilter, [enabled], AlwaysSendChkUp, RequiresPurchOrder, ReminderCount, NextPrepDate, NextDeliveryDate, NextCoffee, NextClean, NextFilter,  NextDescal, NextService FROM TempCoffeecheckupCustomerTbl" + (!string.IsNullOrEmpty(SortBy) ? " ORDER BY " + SortBy : " ORDER BY CompanyName");
            TrackerDb trackerDb = new TrackerDb();
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(strSQL);
            if (dataReader != null)
            {
                while (dataReader.Read())
                {
                    ContactToRemindWithItems toRemindWithItems = new ContactToRemindWithItems();
                    toRemindWithItems.TCCID = dataReader["TCCID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["TCCID"]);
                    toRemindWithItems.CustomerID = dataReader["CustomerID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["CustomerID"]);
                    toRemindWithItems.CompanyName = dataReader["CompanyName"] == DBNull.Value ? string.Empty : dataReader["CompanyName"].ToString();
                    toRemindWithItems.ContactFirstName = dataReader["ContactFirstName"] == DBNull.Value ? string.Empty : dataReader["ContactFirstName"].ToString();
                    toRemindWithItems.ContactAltFirstName = dataReader["ContactAltFirstName"] == DBNull.Value ? string.Empty : dataReader["ContactAltFirstName"].ToString();
                    toRemindWithItems.CityID = dataReader["CityID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["CityID"]);
                    toRemindWithItems.EmailAddress = dataReader["EmailAddress"] == DBNull.Value ? string.Empty : dataReader["EmailAddress"].ToString();
                    toRemindWithItems.AltEmailAddress = dataReader["AltEmailAddress"] == DBNull.Value ? string.Empty : dataReader["AltEmailAddress"].ToString();
                    toRemindWithItems.CustomerTypeID = dataReader["CityID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["CityID"]);
                    toRemindWithItems.EquipTypeID = dataReader["EquipTypeID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["EquipTypeID"]);
                    toRemindWithItems.TypicallySecToo = dataReader["TypicallySecToo"] != DBNull.Value && Convert.ToBoolean(dataReader["TypicallySecToo"]);
                    toRemindWithItems.PreferedAgentID = dataReader["PreferedAgentID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["PreferedAgentID"]);
                    toRemindWithItems.SalesAgentID = dataReader["SalesAgentID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["SalesAgentID"]);
                    toRemindWithItems.UsesFilter = dataReader["UsesFilter"] != DBNull.Value && Convert.ToBoolean(dataReader["UsesFilter"]);
                    toRemindWithItems.enabled = dataReader["enabled"] != DBNull.Value && Convert.ToBoolean(dataReader["enabled"]);
                    toRemindWithItems.AlwaysSendChkUp = dataReader["AlwaysSendChkUp"] != DBNull.Value && Convert.ToBoolean(dataReader["AlwaysSendChkUp"]);
                    toRemindWithItems.RequiresPurchOrder = dataReader["RequiresPurchOrder"] != DBNull.Value && Convert.ToBoolean(dataReader["RequiresPurchOrder"]);
                    toRemindWithItems.ReminderCount = dataReader["ReminderCount"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ReminderCount"]);
                    toRemindWithItems.NextPrepDate = dataReader["NextPrepDate"] == DBNull.Value ? DateTime.MaxValue : Convert.ToDateTime(dataReader["NextPrepDate"]).Date;
                    toRemindWithItems.NextDeliveryDate = dataReader["NextDeliveryDate"] == DBNull.Value ? DateTime.MaxValue : Convert.ToDateTime(dataReader["NextDeliveryDate"]).Date;
                    toRemindWithItems.NextCoffee = dataReader["NextCoffee"] == DBNull.Value ? DateTime.MaxValue : Convert.ToDateTime(dataReader["NextCoffee"]).Date;
                    toRemindWithItems.NextClean = dataReader["NextClean"] == DBNull.Value ? DateTime.MaxValue : Convert.ToDateTime(dataReader["NextClean"]).Date;
                    toRemindWithItems.NextFilter = dataReader["NextFilter"] == DBNull.Value ? DateTime.MaxValue : Convert.ToDateTime(dataReader["NextFilter"]).Date;
                    toRemindWithItems.NextDescal = dataReader["NextDescal"] == DBNull.Value ? DateTime.MaxValue : Convert.ToDateTime(dataReader["NextDescal"]).Date;
                    toRemindWithItems.NextService = dataReader["NextService"] == DBNull.Value ? DateTime.MaxValue : Convert.ToDateTime(dataReader["NextService"]).Date;
                    toRemindWithItems.ItemsContactRequires = this.GetAllContactItems(toRemindWithItems.CustomerID, "");
                    allContactAndItems.Add(toRemindWithItems);
                }
                dataReader.Close();
            }
            trackerDb.Close();
            return allContactAndItems;
        }

        public bool InsertContacts(ContactToRemindDetails pHeaderData)
        {
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddParams((object)pHeaderData.CustomerID, DbType.Int64, "@CustomerID");
            trackerDb.AddParams((object)pHeaderData.CompanyName, DbType.String, "@CompanyName");
            trackerDb.AddParams((object)pHeaderData.ContactFirstName, DbType.String, "@ContactFirstName");
            trackerDb.AddParams((object)pHeaderData.ContactAltFirstName, DbType.String, "@ContactAltFirstName");
            trackerDb.AddParams((object)pHeaderData.CityID, DbType.Int32, "@CityID");
            trackerDb.AddParams((object)pHeaderData.EmailAddress, DbType.String, "@EmailAddress");
            trackerDb.AddParams((object)pHeaderData.AltEmailAddress, DbType.String, "@AltEmailAddress");
            trackerDb.AddParams((object)pHeaderData.CityID, DbType.Int32, "@CityID");
            trackerDb.AddParams((object)pHeaderData.EquipTypeID, DbType.Int32, "@EquipTypeID");
            trackerDb.AddParams((object)pHeaderData.TypicallySecToo, DbType.Boolean, "@TypicallySecToo");
            trackerDb.AddParams((object)pHeaderData.PreferedAgentID, DbType.Int32, "@PreferedAgentID");
            trackerDb.AddParams((object)pHeaderData.SalesAgentID, DbType.Int32, "@SalesAgentID");
            trackerDb.AddParams((object)pHeaderData.UsesFilter, DbType.Boolean, "@UsesFilter");
            trackerDb.AddParams((object)pHeaderData.enabled, DbType.Boolean, "@enabled");
            trackerDb.AddParams((object)pHeaderData.AlwaysSendChkUp, DbType.Boolean, "@AlwaysSendChkUp");
            trackerDb.AddParams((object)pHeaderData.RequiresPurchOrder, DbType.Boolean, "@RequiresPurchOrder");
            trackerDb.AddParams((object)pHeaderData.ReminderCount, DbType.Int32, "@ReminderCount");
            trackerDb.AddParams((object)pHeaderData.NextPrepDate, DbType.Date, "@NextPrepDate");
            trackerDb.AddParams((object)pHeaderData.NextDeliveryDate, DbType.Date, "@NextDeliveryDate");
            trackerDb.AddParams((object)pHeaderData.NextCoffee, DbType.Date, "@NextCoffee");
            trackerDb.AddParams((object)pHeaderData.NextClean, DbType.Date, "@NextClean");
            trackerDb.AddParams((object)pHeaderData.NextFilter, DbType.Date, "@NextFilter");
            trackerDb.AddParams((object)pHeaderData.NextDescal, DbType.Date, "@NextDescal");
            trackerDb.AddParams((object)pHeaderData.NextService, DbType.Date, "@NextService");
            bool flag = string.IsNullOrEmpty(trackerDb.ExecuteNonQuerySQL("INSERT INTO TempCoffeecheckupCustomerTbl (CustomerID, CompanyName, ContactFirstName, ContactAltFirstName, CityID, EmailAddress, AltEmailAddress, CustomerTypeID, EquipTypeID, TypicallySecToo,   PreferedAgentID, SalesAgentID, UsesFilter, [enabled], AlwaysSendChkUp, RequiresPurchOrder, ReminderCount, NextPrepDate, NextDeliveryDate, NextCoffee, NextClean, NextFilter,   NextDescal, NextService) VALUES (?, ? ,? ,? ,? ,? ,? ,? ,? ,? ,? ,? ,? ,? ,? ,? ,? ,? ,? ,? ,? ,? ,? ,?)"));
            trackerDb.Close();
            return flag;
        }

        public bool InsertContactItems(ItemContactRequires pLineData)
        {
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddParams((object)pLineData.CustomerID, DbType.Int64, "@CustomerID");
            trackerDb.AddParams((object)pLineData.ItemID, DbType.Int32, "@ItemID");
            trackerDb.AddParams((object)pLineData.ItemQty, DbType.Double, "@ItemQty");
            trackerDb.AddParams((object)pLineData.ItemPrepID, DbType.Int32, "@ItemPrepID");
            trackerDb.AddParams((object)pLineData.ItemPackagID, DbType.Int32, "@ItemPackagID");
            trackerDb.AddParams((object)pLineData.AutoFulfill, DbType.Boolean, "@AutoFulfill");
            trackerDb.AddParams((object)pLineData.ReoccurID, DbType.Int64, "@ReoccurID");
            bool flag = string.IsNullOrEmpty(trackerDb.ExecuteNonQuerySQL("INSERT INTO TempCoffeecheckupItemsTbl  (CustomerID, ItemID, ItemQty, ItemPrepID, ItemPackagID, AutoFulfill, ReoccurOrderId)  VALUES ( ?, ? ,? ,? ,? ,? ,? )"));
            trackerDb.Close();
            return flag;
        }

        public bool DeleteAllContactRecords()
        {
            TrackerDb trackerDb = new TrackerDb();
            bool flag = string.IsNullOrEmpty(trackerDb.ExecuteNonQuerySQL("DELETE * FROM TempCoffeecheckupCustomerTbl"));
            trackerDb.Close();
            return flag;
        }

        public bool DeleteAllContactItems()
        {
            TrackerDb trackerDb = new TrackerDb();
            bool flag = string.IsNullOrEmpty(trackerDb.ExecuteNonQuerySQL("DELETE * FROM TempCoffeecheckupItemsTbl"));
            trackerDb.Close();
            return flag;
        }
    }
}