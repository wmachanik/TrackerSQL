// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.control.CustomersWithDatesAndUsageTbl
// Assembly: TrackerDotNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerDotNet.dll

using System;
using System.Data;
using TrackerDotNet.Classes;

//- only form later versions #nullable disable
namespace TrackerDotNet.Controls
{
    public class CustomersWithDatesAndUsageTbl
    {
        private const string CONST_SQL_CUSTOMERSUSAGE_SELECT = "SELECT CustomersTbl.CustomerID, CustomersTbl.CompanyName, CustomersTbl.ContactTitle, CustomersTbl.ContactFirstName, CustomersTbl.ContactLastName,  CustomersTbl.ContactAltFirstName, CustomersTbl.ContactAltLastName, CustomersTbl.Department, CustomersTbl.BillingAddress, CustomersTbl.City,  CustomersTbl.StateOrProvince AS Province, CustomersTbl.PostalCode, CustomersTbl.[Country/Region] AS Region, CustomersTbl.PhoneNumber,  CustomersTbl.Extension, CustomersTbl.FaxNumber, CustomersTbl.CellNumber, CustomersTbl.EmailAddress, CustomersTbl.AltEmailAddress,  CustomersTbl.ContractNo, CustomersTbl.CustomerTypeID, CustomersTbl.EquipType, CustomersTbl.CoffeePreference, CustomersTbl.PriPrefQty, CustomersTbl.PrefPrepTypeID, CustomersTbl.PrefPackagingID, CustomersTbl.SecondaryPreference, CustomersTbl.SecPrefQty, CustomersTbl.TypicallySecToo,  CustomersTbl.PreferedAgent, CustomersTbl.SalesAgentID, CustomersTbl.MachineSN, CustomersTbl.UsesFilter, CustomersTbl.autofulfill, CustomersTbl.enabled, CustomersTbl.PredictionDisabled, CustomersTbl.AlwaysSendChkUp, CustomersTbl.NormallyResponds, CustomersTbl.ReminderCount,  CustomersTbl.LastDateSentReminder, CustomersTbl.Notes, NextRoastDateByCityTbl.CityID, NextRoastDateByCityTbl.DeliveryDate,  NextRoastDateByCityTbl.PreperationDate, NextRoastDateByCityTbl.DeliveryOrder, LastCupCount, NextCoffeeBy, NextCleanOn, NextFilterEst, NextDescaleEst, NextServiceEst, DailyConsumption, FilterAveCount, DescaleAveCount, ServiceAveCount, CleanAveCount  FROM  (NextRoastDateByCityTbl RIGHT OUTER JOIN CustomersTbl ON NextRoastDateByCityTbl.CityID = CustomersTbl.City), ClientUsageTbl.CustomerID = CustomersTbl.CustomerID  WHERE (CustomersTbl.CustomerID = ?)";
        private CustomersTbl _Customer;
        private NextRoastDateByCityTbl _NextRoastDateByCity;
        private ClientUsageTbl _ClientUsage;

        public CustomersWithDatesAndUsageTbl()
        {
            this._Customer = new CustomersTbl();
            this._NextRoastDateByCity = new NextRoastDateByCityTbl();
            this._ClientUsage = new ClientUsageTbl();
        }

        public CustomersTbl Customer
        {
            get => this._Customer;
            set => this._Customer = value;
        }

        public NextRoastDateByCityTbl NextRoastDateByCity
        {
            get => this._NextRoastDateByCity;
            set => this._NextRoastDateByCity = value;
        }

        public ClientUsageTbl ClientUsage
        {
            get => this._ClientUsage;
            set => this._ClientUsage = value;
        }

        public CustomersWithDatesAndUsageTbl GetCustomerWithDatesAndUsage(long pCustomerID)
        {
            CustomersWithDatesAndUsageTbl withDatesAndUsage = (CustomersWithDatesAndUsageTbl)null;
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddWhereParams((object)pCustomerID, DbType.Int64);
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader("SELECT CustomersTbl.CustomerID, CustomersTbl.CompanyName, CustomersTbl.ContactTitle, CustomersTbl.ContactFirstName, CustomersTbl.ContactLastName,  CustomersTbl.ContactAltFirstName, CustomersTbl.ContactAltLastName, CustomersTbl.Department, CustomersTbl.BillingAddress, CustomersTbl.City,  CustomersTbl.StateOrProvince AS Province, CustomersTbl.PostalCode, CustomersTbl.[Country/Region] AS Region, CustomersTbl.PhoneNumber,  CustomersTbl.Extension, CustomersTbl.FaxNumber, CustomersTbl.CellNumber, CustomersTbl.EmailAddress, CustomersTbl.AltEmailAddress,  CustomersTbl.ContractNo, CustomersTbl.CustomerTypeID, CustomersTbl.EquipType, CustomersTbl.CoffeePreference, CustomersTbl.PriPrefQty, CustomersTbl.PrefPrepTypeID, CustomersTbl.PrefPackagingID, CustomersTbl.SecondaryPreference, CustomersTbl.SecPrefQty, CustomersTbl.TypicallySecToo,  CustomersTbl.PreferedAgent, CustomersTbl.SalesAgentID, CustomersTbl.MachineSN, CustomersTbl.UsesFilter, CustomersTbl.autofulfill, CustomersTbl.enabled, CustomersTbl.PredictionDisabled, CustomersTbl.AlwaysSendChkUp, CustomersTbl.NormallyResponds, CustomersTbl.ReminderCount,  CustomersTbl.LastDateSentReminder, CustomersTbl.Notes, NextRoastDateByCityTbl.CityID, NextRoastDateByCityTbl.DeliveryDate,  NextRoastDateByCityTbl.PreperationDate, NextRoastDateByCityTbl.DeliveryOrder, LastCupCount, NextCoffeeBy, NextCleanOn, NextFilterEst, NextDescaleEst, NextServiceEst, DailyConsumption, FilterAveCount, DescaleAveCount, ServiceAveCount, CleanAveCount  FROM  (NextRoastDateByCityTbl RIGHT OUTER JOIN CustomersTbl ON NextRoastDateByCityTbl.CityID = CustomersTbl.City), ClientUsageTbl.CustomerID = CustomersTbl.CustomerID  WHERE (CustomersTbl.CustomerID = ?)");
            if (dataReader != null)
            {
                if (dataReader.Read())
                {
                    withDatesAndUsage = new CustomersWithDatesAndUsageTbl();
                    withDatesAndUsage.Customer.CustomerID = dataReader["CustomerID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["CustomerID"]);
                    withDatesAndUsage.Customer.CompanyName = dataReader["CompanyName"] == DBNull.Value ? string.Empty : dataReader["CompanyName"].ToString();
                    withDatesAndUsage.Customer.ContactTitle = dataReader["ContactTitle"] == DBNull.Value ? string.Empty : dataReader["ContactTitle"].ToString();
                    withDatesAndUsage.Customer.ContactFirstName = dataReader["ContactFirstName"] == DBNull.Value ? string.Empty : dataReader["ContactFirstName"].ToString();
                    withDatesAndUsage.Customer.ContactLastName = dataReader["ContactLastName"] == DBNull.Value ? string.Empty : dataReader["ContactLastName"].ToString();
                    withDatesAndUsage.Customer.ContactAltFirstName = dataReader["ContactAltFirstName"] == DBNull.Value ? string.Empty : dataReader["ContactAltFirstName"].ToString();
                    withDatesAndUsage.Customer.ContactAltLastName = dataReader["ContactAltLastName"] == DBNull.Value ? string.Empty : dataReader["ContactAltLastName"].ToString();
                    withDatesAndUsage.Customer.Department = dataReader["Department"] == DBNull.Value ? string.Empty : dataReader["Department"].ToString();
                    withDatesAndUsage.Customer.BillingAddress = dataReader["BillingAddress"] == DBNull.Value ? string.Empty : dataReader["BillingAddress"].ToString();
                    withDatesAndUsage.Customer.City = dataReader["City"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["City"]);
                    withDatesAndUsage.Customer.Province = dataReader["Province"] == DBNull.Value ? string.Empty : dataReader["Province"].ToString();
                    withDatesAndUsage.Customer.PostalCode = dataReader["PostalCode"] == DBNull.Value ? string.Empty : dataReader["PostalCode"].ToString();
                    withDatesAndUsage.Customer.Region = dataReader["Region"] == DBNull.Value ? string.Empty : dataReader["Region"].ToString();
                    withDatesAndUsage.Customer.PhoneNumber = dataReader["PhoneNumber"] == DBNull.Value ? string.Empty : dataReader["PhoneNumber"].ToString();
                    withDatesAndUsage.Customer.Extension = dataReader["Extension"] == DBNull.Value ? string.Empty : dataReader["Extension"].ToString();
                    withDatesAndUsage.Customer.FaxNumber = dataReader["FaxNumber"] == DBNull.Value ? string.Empty : dataReader["FaxNumber"].ToString();
                    withDatesAndUsage.Customer.CellNumber = dataReader["CellNumber"] == DBNull.Value ? string.Empty : dataReader["CellNumber"].ToString();
                    withDatesAndUsage.Customer.EmailAddress = dataReader["EmailAddress"] == DBNull.Value ? string.Empty : dataReader["EmailAddress"].ToString();
                    withDatesAndUsage.Customer.AltEmailAddress = dataReader["AltEmailAddress"] == DBNull.Value ? string.Empty : dataReader["AltEmailAddress"].ToString();
                    withDatesAndUsage.Customer.ContractNo = dataReader["ContractNo"] == DBNull.Value ? string.Empty : dataReader["ContractNo"].ToString();
                    withDatesAndUsage.Customer.CustomerTypeID = dataReader["CustomerTypeID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["CustomerTypeID"]);
                    withDatesAndUsage.Customer.EquipType = dataReader["EquipType"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["EquipType"]);
                    withDatesAndUsage.Customer.CoffeePreference = dataReader["CoffeePreference"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["CoffeePreference"]);
                    withDatesAndUsage.Customer.PriPrefQty = dataReader["PriPrefQty"] == DBNull.Value ? 0.0 : Convert.ToDouble(dataReader["PriPrefQty"]);
                    withDatesAndUsage.Customer.PrefPrepTypeID = dataReader["PrefPrepTypeID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["PrefPrepTypeID"]);
                    withDatesAndUsage.Customer.PrefPackagingID = dataReader["PrefPackagingID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["PrefPackagingID"]);
                    withDatesAndUsage.Customer.SecondaryPreference = dataReader["SecondaryPreference"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["SecondaryPreference"]);
                    withDatesAndUsage.Customer.SecPrefQty = dataReader["SecPrefQty"] == DBNull.Value ? 0.0 : Convert.ToDouble(dataReader["SecPrefQty"]);
                    withDatesAndUsage.Customer.TypicallySecToo = dataReader["TypicallySecToo"] != DBNull.Value && Convert.ToBoolean(dataReader["TypicallySecToo"]);
                    withDatesAndUsage.Customer.PreferedAgent = dataReader["PreferedAgent"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["PreferedAgent"]);
                    withDatesAndUsage.Customer.SalesAgentID = dataReader["SalesAgentID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["SalesAgentID"]);
                    withDatesAndUsage.Customer.MachineSN = dataReader["MachineSN"] == DBNull.Value ? string.Empty : dataReader["MachineSN"].ToString();
                    withDatesAndUsage.Customer.UsesFilter = dataReader["UsesFilter"] != DBNull.Value && Convert.ToBoolean(dataReader["UsesFilter"]);
                    withDatesAndUsage.Customer.autofulfill = dataReader["autofulfill"] != DBNull.Value && Convert.ToBoolean(dataReader["autofulfill"]);
                    withDatesAndUsage.Customer.enabled = dataReader["enabled"] != DBNull.Value && Convert.ToBoolean(dataReader["enabled"]);
                    withDatesAndUsage.Customer.AlwaysSendChkUp = dataReader["AlwaysSendChkUp"] != DBNull.Value && Convert.ToBoolean(dataReader["AlwaysSendChkUp"]);
                    withDatesAndUsage.Customer.NormallyResponds = dataReader["NormallyResponds"] != DBNull.Value && Convert.ToBoolean(dataReader["NormallyResponds"]);
                    withDatesAndUsage.Customer.ReminderCount = dataReader["xxx"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["xxx"]);
                    withDatesAndUsage.Customer.LastDateSentReminder = dataReader["LastDateSentReminder"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(dataReader["LastDateSentReminder"]);
                    withDatesAndUsage.Customer.Notes = dataReader["Notes"] == DBNull.Value ? string.Empty : dataReader["Notes"].ToString();
                    withDatesAndUsage.NextRoastDateByCity.CityID = dataReader["CityID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["CityID"]);
                    withDatesAndUsage.NextRoastDateByCity.DeliveryDate = dataReader["DeliveryDate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(dataReader["DeliveryDate"]).Date;
                    withDatesAndUsage.NextRoastDateByCity.PrepDate = dataReader["PreperationDate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(dataReader["PreperationDate"]).Date;
                    withDatesAndUsage.NextRoastDateByCity.DeliveryOrder = dataReader["DeliveryOrder"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["DeliveryOrder"]);
                    withDatesAndUsage.ClientUsage.LastCupCount = dataReader["LastCupCount"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["LastCupCount"]);
                    withDatesAndUsage.ClientUsage.NextCoffeeBy = dataReader["NextCoffeeBy"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(dataReader["NextCoffeeBy"]).Date;
                    withDatesAndUsage.ClientUsage.NextCleanOn = dataReader["NextCleanOn"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(dataReader["NextCleanOn"]).Date;
                    withDatesAndUsage.ClientUsage.NextFilterEst = dataReader["NextFilterEst "] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(dataReader["NextFilterEst"]).Date;
                    withDatesAndUsage.ClientUsage.NextDescaleEst = dataReader["NextDescaleEst"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(dataReader["NextDescaleEst"]).Date;
                    withDatesAndUsage.ClientUsage.NextServiceEst = dataReader["NextServiceEst"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(dataReader["NextServiceEst"]).Date;
                    withDatesAndUsage.ClientUsage.DailyConsumption = dataReader["DailyConsumption"] == DBNull.Value ? 0.0 : Convert.ToDouble(dataReader["DailyConsumption"]);
                    withDatesAndUsage.ClientUsage.FilterAveCount = dataReader["FilterAveCount"] == DBNull.Value ? 0.0 : Convert.ToDouble(dataReader["FilterAveCount"]);
                    withDatesAndUsage.ClientUsage.DescaleAveCount = dataReader["DescaleAveCount"] == DBNull.Value ? 0.0 : Convert.ToDouble(dataReader["DescaleAveCount"]);
                    withDatesAndUsage.ClientUsage.ServiceAveCount = dataReader["ServiceAveCount"] == DBNull.Value ? 0.0 : Convert.ToDouble(dataReader["ServiceAveCount"]);
                    withDatesAndUsage.ClientUsage.CleanAveCount = dataReader["CleanAveCount"] == DBNull.Value ? 0.0 : Convert.ToDouble(dataReader["CleanAveCount"]);
                }
                dataReader.Close();
            }
            trackerDb.Close();
            return withDatesAndUsage;
        }
    }
}