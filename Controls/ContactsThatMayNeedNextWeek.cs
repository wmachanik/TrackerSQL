// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.control.ContactsThatMayNeedNextWeek
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
    public class ContactsThatMayNeedNextWeek
    {
        private const string CONST_SELECT_CONTACTSTHATMAYNEEDNEXTWEEK = "SELECT CustomersTbl.CustomerID AS ContactID, CustomersTbl.CompanyName, CustomersTbl.ContactTitle, CustomersTbl.ContactFirstName,  CustomersTbl.ContactLastName, CustomersTbl.ContactAltFirstName, CustomersTbl.ContactAltLastName, CustomersTbl.Department, CustomersTbl.BillingAddress, CustomersTbl.City, CustomersTbl.PostalCode, CustomersTbl.PreferedAgent, CustomersTbl.SalesAgentID, CustomersTbl.PhoneNumber, CustomersTbl.Extension, CustomersTbl.FaxNumber, CustomersTbl.CellNumber, CustomersTbl.EmailAddress,  CustomersTbl.AltEmailAddress, CustomersTbl.UsesFilter, CustomersTbl.EquipType, CustomersTbl.CustomerTypeID, CustomersTbl.TypicallySecToo, CustomersTbl.PriPrefQty, CustomersTbl.SecPrefQty, CustomersTbl.ReminderCount,  CustomersTbl.autofulfill, CustomersTbl.AlwaysSendChkUp, CustomersAccInfoTbl.RequiresPurchOrder, CustomersTbl.enabled, CustomersTbl.Notes,  ClientUsageTbl.LastCupCount, ClientUsageTbl.NextCoffeeBy, ClientUsageTbl.NextCleanOn, ClientUsageTbl.NextFilterEst,  ClientUsageTbl.NextDescaleEst, ClientUsageTbl.NextServiceEst, ClientUsageTbl.DailyConsumption,  NextRoastDateByCityTbl.PreperationDate, NextRoastDateByCityTbl.DeliveryDate, NextRoastDateByCityTbl.NextPreperationDate, NextRoastDateByCityTbl.NextDeliveryDate FROM ((((CustomersTbl INNER JOIN ClientUsageTbl ON CustomersTbl.CustomerID = ClientUsageTbl.CustomerID) LEFT OUTER JOIN CustomersAccInfoTbl ON CustomersTbl.CustomerID = CustomersAccInfoTbl.CustomerID) LEFT OUTER JOIN NextRoastDateByCityTbl ON CustomersTbl.City = NextRoastDateByCityTbl.CityID) LEFT OUTER JOIN ItemNoStockItemQry ON CustomersTbl.CoffeePreference = ItemNoStockItemQry.ItemTypeID)  WHERE ((LastDateSentReminder IS Null) OR (LastDateSentReminder <> ?)) AND (CustomersTbl.enabled=True)  AND (CustomersTbl.PredictionDisabled=False)  AND ((ClientUsageTbl.NextCoffeeBy > ?) AND ((NextRoastDateByCityTbl.NextDeliveryDate<=DateAdd('d', 9, ClientUsageTbl.NextCoffeeBy)) OR CustomersTbl.AlwaysSendChkUp=True) ) AND (NOT Exists  (SELECT  OrdersTbl.CustomerID FROM OrdersTbl   WHERE (OrdersTbl.CustomerID=CustomersTbl.CustomerID) AND (OrdersTbl.RoastDate>=Date() AND   OrdersTbl.RoastDate<=DateAdd('d',9,Date()))\t ))  ORDER BY CustomersTbl.CompanyName";

        public List<ContactsThayMayNeedData> GetContactsThatMayNeedNextWeek(int reminderWindowDays = 9)
        {
            List<ContactsThayMayNeedData> thatMayNeedNextWeek = new List<ContactsThayMayNeedData>();
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddWhereParams((object)TimeZoneUtils.Now().Date, DbType.Date);
            SysDataTbl sysDataTbl = new SysDataTbl();
            trackerDb.AddWhereParams((object)sysDataTbl.GetMinReminderDate().Date, DbType.Date);

            // Use string interpolation to inject the window into the SQL
            string sql = $@"
                SELECT CustomersTbl.CustomerID AS ContactID, CustomersTbl.CompanyName, CustomersTbl.ContactTitle, CustomersTbl.ContactFirstName,
                CustomersTbl.ContactLastName, CustomersTbl.ContactAltFirstName, CustomersTbl.ContactAltLastName, CustomersTbl.Department, CustomersTbl.BillingAddress, CustomersTbl.City,
                CustomersTbl.PostalCode, CustomersTbl.PreferedAgent, CustomersTbl.SalesAgentID, CustomersTbl.PhoneNumber, CustomersTbl.Extension, CustomersTbl.FaxNumber, CustomersTbl.CellNumber,
                CustomersTbl.EmailAddress,  CustomersTbl.AltEmailAddress, CustomersTbl.UsesFilter, CustomersTbl.EquipType, CustomersTbl.CustomerTypeID, CustomersTbl.TypicallySecToo, CustomersTbl.PriPrefQty,
                CustomersTbl.SecPrefQty, CustomersTbl.ReminderCount,  CustomersTbl.autofulfill, CustomersTbl.AlwaysSendChkUp, CustomersAccInfoTbl.RequiresPurchOrder, CustomersTbl.enabled,
                CustomersTbl.Notes,  ClientUsageTbl.LastCupCount, ClientUsageTbl.NextCoffeeBy, ClientUsageTbl.NextCleanOn, ClientUsageTbl.NextFilterEst,  ClientUsageTbl.NextDescaleEst,
                ClientUsageTbl.NextServiceEst, ClientUsageTbl.DailyConsumption,  NextRoastDateByCityTbl.PreperationDate, NextRoastDateByCityTbl.DeliveryDate,
                NextRoastDateByCityTbl.NextPreperationDate, NextRoastDateByCityTbl.NextDeliveryDate
                FROM ((((CustomersTbl INNER JOIN ClientUsageTbl ON CustomersTbl.CustomerID = ClientUsageTbl.CustomerID)
                LEFT OUTER JOIN CustomersAccInfoTbl ON CustomersTbl.CustomerID = CustomersAccInfoTbl.CustomerID)
                LEFT OUTER JOIN NextRoastDateByCityTbl ON CustomersTbl.City = NextRoastDateByCityTbl.CityID)
                LEFT OUTER JOIN ItemNoStockItemQry ON CustomersTbl.CoffeePreference = ItemNoStockItemQry.ItemTypeID)
                WHERE ((LastDateSentReminder IS Null) OR (LastDateSentReminder <> ?)) AND (CustomersTbl.enabled=True)
                AND (CustomersTbl.PredictionDisabled=False)  AND ((ClientUsageTbl.NextCoffeeBy > ?)
                AND ((NextRoastDateByCityTbl.NextDeliveryDate<=DateAdd('d', {reminderWindowDays}, ClientUsageTbl.NextCoffeeBy))
                OR CustomersTbl.AlwaysSendChkUp=True) ) AND
                (NOT Exists (SELECT  OrdersTbl.CustomerID FROM OrdersTbl
                WHERE (OrdersTbl.CustomerID=CustomersTbl.CustomerID) AND (OrdersTbl.RoastDate>=Date()
                AND OrdersTbl.RoastDate<=DateAdd('d',{reminderWindowDays},Date())) )) 
                ORDER BY CustomersTbl.CompanyName";

            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(sql);
            if (dataReader != null)
            {
                while (dataReader.Read())
                    thatMayNeedNextWeek.Add(new ContactsThayMayNeedData()
                    {
                        CustomerData = {
            CustomerID = dataReader["ContactID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ContactID"]),
            CompanyName = dataReader["CompanyName"] == DBNull.Value ? string.Empty : dataReader["CompanyName"].ToString(),
            ContactTitle = dataReader["ContactTitle"] == DBNull.Value ? string.Empty : dataReader["ContactTitle"].ToString(),
            ContactFirstName = dataReader["ContactFirstName"] == DBNull.Value ? string.Empty : dataReader["ContactFirstName"].ToString(),
            ContactLastName = dataReader["ContactLastName"] == DBNull.Value ? string.Empty : dataReader["ContactLastName"].ToString(),
            ContactAltFirstName = dataReader["ContactAltFirstName"] == DBNull.Value ? string.Empty : dataReader["ContactAltFirstName"].ToString(),
            ContactAltLastName = dataReader["ContactAltLastName"] == DBNull.Value ? string.Empty : dataReader["ContactAltLastName"].ToString(),
            Department = dataReader["Department"] == DBNull.Value ? string.Empty : dataReader["Department"].ToString(),
            BillingAddress = dataReader["BillingAddress"] == DBNull.Value ? string.Empty : dataReader["BillingAddress"].ToString(),
            City = dataReader["City"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["City"]),
            PostalCode = dataReader["PostalCode"] == DBNull.Value ? string.Empty : dataReader["PostalCode"].ToString(),
            PreferedAgent = dataReader["PreferedAgent"] == DBNull.Value ? 3 : Convert.ToInt32(dataReader["PreferedAgent"]),
            SalesAgentID = dataReader["SalesAgentID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["SalesAgentID"]),
            PhoneNumber = dataReader["PhoneNumber"] == DBNull.Value ? string.Empty : dataReader["PhoneNumber"].ToString(),
            Extension = dataReader["Extension"] == DBNull.Value ? string.Empty : dataReader["Extension"].ToString(),
            FaxNumber = dataReader["FaxNumber"] == DBNull.Value ? string.Empty : dataReader["FaxNumber"].ToString(),
            CellNumber = dataReader["CellNumber"] == DBNull.Value ? string.Empty : dataReader["CellNumber"].ToString(),
            EmailAddress = dataReader["EmailAddress"] == DBNull.Value ? string.Empty : dataReader["EmailAddress"].ToString(),
            AltEmailAddress = dataReader["AltEmailAddress"] == DBNull.Value ? string.Empty : dataReader["AltEmailAddress"].ToString(),
            EquipType = dataReader["EquipType"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["EquipType"]),
            CustomerTypeID = dataReader["CustomerTypeID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["CustomerTypeID"]),
            PriPrefQty = dataReader["PriPrefQty"] == DBNull.Value ? 0.0 : Convert.ToDouble(dataReader["PriPrefQty"]),
            SecPrefQty = dataReader["SecPrefQty"] == DBNull.Value ? 0.0 : Convert.ToDouble(dataReader["SecPrefQty"]),
            ReminderCount = dataReader["ReminderCount"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ReminderCount"]),
            autofulfill = dataReader["autofulfill"] != DBNull.Value && Convert.ToBoolean(dataReader["autofulfill"]),
            UsesFilter = dataReader["UsesFilter"] != DBNull.Value && Convert.ToBoolean(dataReader["UsesFilter"]),
            TypicallySecToo = dataReader["TypicallySecToo"] != DBNull.Value && Convert.ToBoolean(dataReader["TypicallySecToo"]),
            AlwaysSendChkUp = dataReader["AlwaysSendChkUp"] != DBNull.Value && Convert.ToBoolean(dataReader["AlwaysSendChkUp"]),
            enabled = dataReader["enabled"] != DBNull.Value && Convert.ToBoolean(dataReader["enabled"]),
            Notes = dataReader["Notes"] == DBNull.Value ? string.Empty : dataReader["Notes"].ToString()
          },
                        RequiresPurchOrder = dataReader["RequiresPurchOrder"] != DBNull.Value && Convert.ToBoolean(dataReader["RequiresPurchOrder"]),
                        ClientUsageData = {
            LastCupCount = dataReader["LastCupCount"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["LastCupCount"]),
            NextCoffeeBy = dataReader["NextCoffeeBy"] == DBNull.Value ? TimeZoneUtils.Now().Date : Convert.ToDateTime(dataReader["NextCoffeeBy"]).Date,
            NextCleanOn = dataReader["NextCleanOn"] == DBNull.Value ? TimeZoneUtils.Now().Date : Convert.ToDateTime(dataReader["NextCleanOn"]).Date,
            NextFilterEst = dataReader["NextFilterEst"] == DBNull.Value ? TimeZoneUtils.Now().Date : Convert.ToDateTime(dataReader["NextFilterEst"]).Date,
            NextDescaleEst = dataReader["NextDescaleEst"] == DBNull.Value ? TimeZoneUtils.Now().Date : Convert.ToDateTime(dataReader["NextDescaleEst"]).Date,
            NextServiceEst = dataReader["NextServiceEst"] == DBNull.Value ? TimeZoneUtils.Now().Date : Convert.ToDateTime(dataReader["NextServiceEst"]).Date,
            DailyConsumption = dataReader["DailyConsumption"] == DBNull.Value ? 0.0 : Convert.ToDouble(dataReader["DailyConsumption"])
          },
                        NextRoastDateByCityData = {
            PrepDate = dataReader["PreperationDate"] == DBNull.Value ? TimeZoneUtils.Now().Date : Convert.ToDateTime(dataReader["PreperationDate"]).Date,
            DeliveryDate = dataReader["DeliveryDate"] == DBNull.Value ? TimeZoneUtils.Now().Date : Convert.ToDateTime(dataReader["DeliveryDate"]).Date,
            NextPrepDate = dataReader["NextPreperationDate"] == DBNull.Value ? TimeZoneUtils.Now().Date : Convert.ToDateTime(dataReader["NextPreperationDate"]).Date,
            NextDeliveryDate = dataReader["NextDeliveryDate"] == DBNull.Value ? TimeZoneUtils.Now().Date : Convert.ToDateTime(dataReader["NextDeliveryDate"]).Date
          }
                    });
                dataReader.Dispose();
            }
            trackerDb.Close();
            return thatMayNeedNextWeek;
        }
    }
}