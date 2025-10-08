// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.control.CustomerDAL
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
    public class CustomerDAL
    {
        private const string CONST_SELECTCUSTOMERS = "SELECT CustomerID, CompanyName, ContactTitle, ContactFirstName, ContactLastName, ContactAltFirstName, ContactAltLastName, Department, BillingAddress, City, StateOrProvince, PostalCode, [Country/Region] AS Region, PhoneNumber, Extension, FaxNumber, CellNumber, EmailAddress, AltEmailAddress, ContractNo, CustomerTypeID, EquipType, CoffeePreference, PriPrefQty, PrefPrepTypeID, PrefPackagingID, SecondaryPreference, SecPrefQty, TypicallySecToo, PreferedAgent, SalesAgentID, MachineSN, UsesFilter, autofulfill, enabled,PredictionDisabled, AlwaysSendChkUp, NormallyResponds, ReminderCount, Notes FROM CustomersTbl";

        public static List<CustomerData> GetAllCustomers(string SortBy)
        {
            List<CustomerData> allCustomers = new List<CustomerData>();
            TrackerDb trackerDb = new TrackerDb();
            string strSQL = "SELECT CustomerID, CompanyName, ContactTitle, ContactFirstName, ContactLastName, ContactAltFirstName, ContactAltLastName, Department, BillingAddress, City, StateOrProvince, PostalCode, [Country/Region] AS Region, PhoneNumber, Extension, FaxNumber, CellNumber, EmailAddress, AltEmailAddress, ContractNo, CustomerTypeID, EquipType, CoffeePreference, PriPrefQty, PrefPrepTypeID, PrefPackagingID, SecondaryPreference, SecPrefQty, TypicallySecToo, PreferedAgent, SalesAgentID, MachineSN, UsesFilter, autofulfill, enabled,PredictionDisabled, AlwaysSendChkUp, NormallyResponds, ReminderCount, Notes FROM CustomersTbl";
            if (!string.IsNullOrEmpty(SortBy))
                strSQL = $"{strSQL} ORDER BY {SortBy}";
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(strSQL);
            if (dataReader != null)
            {
                while (dataReader.Read())
                    allCustomers.Add(new CustomerData()
                    {
                        CustomerID = Convert.ToInt32(dataReader["CustomerID"]),
                        CompanyName = dataReader["CompanyName"] == DBNull.Value ? "" : dataReader["CompanyName"].ToString(),
                        ContactTitle = dataReader["ContactTitle"] == DBNull.Value ? "" : dataReader["ContactTitle"].ToString(),
                        ContactFirstName = dataReader["ContactFirstName"] == DBNull.Value ? "" : dataReader["ContactFirstName"].ToString(),
                        ContactLastName = dataReader["ContactLastName"] == DBNull.Value ? "" : dataReader["ContactLastName"].ToString(),
                        ContactAltFirstName = dataReader["ContactAltFirstName"] == DBNull.Value ? "" : dataReader["ContactAltFirstName"].ToString(),
                        ContactAltLastName = dataReader["ContactAltLastName"] == DBNull.Value ? "" : dataReader["ContactAltLastName"].ToString(),
                        Department = dataReader["Department"] == DBNull.Value ? "" : dataReader["Department"].ToString(),
                        BillingAddress = dataReader["BillingAddress"] == DBNull.Value ? "" : dataReader["BillingAddress"].ToString(),
                        City = dataReader["City"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["City"]),
                        StateOrProvince = dataReader["StateOrProvince"] == DBNull.Value ? "" : dataReader["StateOrProvince"].ToString(),
                        PostalCode = dataReader["PostalCode"] == DBNull.Value ? "" : dataReader["PostalCode"].ToString(),
                        Region = dataReader["Region"] == DBNull.Value ? "" : dataReader["Region"].ToString(),
                        PhoneNumber = dataReader["PhoneNumber"] == DBNull.Value ? "" : dataReader["PhoneNumber"].ToString(),
                        Extension = dataReader["Extension"] == DBNull.Value ? "" : dataReader["Extension"].ToString(),
                        FaxNumber = dataReader["FaxNumber"] == DBNull.Value ? "" : dataReader["FaxNumber"].ToString(),
                        CellNumber = dataReader["CellNumber"] == DBNull.Value ? "" : dataReader["CellNumber"].ToString(),
                        EmailAddress = dataReader["EmailAddress"] == DBNull.Value ? "" : dataReader["EmailAddress"].ToString(),
                        AltEmailAddress = dataReader["AltEmailAddress"] == DBNull.Value ? "" : dataReader["AltEmailAddress"].ToString(),
                        ContractNo = dataReader["ContractNo"] == DBNull.Value ? "" : dataReader["ContractNo"].ToString(),
                        CustomerTypeID = dataReader["CustomerTypeID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["CustomerTypeID"]),
                        EquipType = dataReader["EquipType"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["EquipType"]),
                        CoffeePreference = dataReader["CoffeePreference"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["CoffeePreference"]),
                        PriPrefQty = dataReader["PriPrefQty"] == DBNull.Value ? 0.0 : Convert.ToDouble(dataReader["PriPrefQty"]),
                        PrefPrepTypeID = dataReader["PrefPrepTypeID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["PrefPrepTypeID"]),
                        PrefPackagingID = dataReader["PrefPackagingID"] == DBNull.Value ? 0: Convert.ToInt32(dataReader["PrefPackagingID"]),
                        SecondaryPreference = dataReader["SecondaryPreference"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["SecondaryPreference"]),
                        SecPrefQty = dataReader["SecPrefQty"] == DBNull.Value ? 0.0 : Convert.ToDouble(dataReader["SecPrefQty"]),
                        TypicallySecToo = dataReader["TypicallySecToo"] != DBNull.Value && Convert.ToBoolean(dataReader["TypicallySecToo"]),
                        PreferedAgent = dataReader["PreferedAgent"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["PreferedAgent"]),
                        SalesAgentID = dataReader["SalesAgentID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["SalesAgentID"]),
                        MachineSN = dataReader["MachineSN"] == DBNull.Value ? "" : dataReader["MachineSN"].ToString(),
                        UsesFilter = dataReader["UsesFilter"] != DBNull.Value && Convert.ToBoolean(dataReader["UsesFilter"]),
                        autofulfill = dataReader["autofulfill"] != DBNull.Value && Convert.ToBoolean(dataReader["autofulfill"]),
                        enabled = dataReader["enabled"] == DBNull.Value || Convert.ToBoolean(dataReader["enabled"]),
                        PredictionDisabled = dataReader["PredictionDisabled"] != DBNull.Value && Convert.ToBoolean(dataReader["PredictionDisabled"]),
                        AlwaysSendChkUp = dataReader["AlwaysSendChkUp"] != DBNull.Value && Convert.ToBoolean(dataReader["AlwaysSendChkUp"]),
                        NormallyResponds = dataReader["NormallyResponds"] != DBNull.Value && Convert.ToBoolean(dataReader["NormallyResponds"]),
                        ReminderCount = dataReader["ReminderCount"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ReminderCount"]),
                        Notes = dataReader["ContactTitle"] == DBNull.Value ? "" : dataReader["Notes"].ToString()
                    });
                dataReader.Close();
            }
            trackerDb.Close();
            return allCustomers;
        }
    }
}