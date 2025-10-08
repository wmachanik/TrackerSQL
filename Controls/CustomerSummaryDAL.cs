// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.control.CustomerSummaryDAL
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
    public class CustomerSummaryDAL
    {
        private const string CONST_SQL_SUMMARYDATA = "SELECT CustomersTbl.CustomerID, CustomersTbl.CompanyName, CustomersTbl.ContactFirstName, CustomersTbl.ContactLastName, CityTbl.City,  CustomersTbl.PhoneNumber, CustomersTbl.EmailAddress, PersonsTbl.Abreviation AS DeliveryBy, EquipTypeTbl.EquipTypeName, CustomersTbl.MachineSN,  CustomersTbl.autofulfill, CustomersTbl.enabled  FROM (((CustomersTbl LEFT OUTER JOIN PersonsTbl ON CustomersTbl.PreferedAgent = PersonsTbl.PersonID) LEFT OUTER JOIN  CityTbl ON CustomersTbl.City = CityTbl.ID) LEFT OUTER JOIN EquipTypeTbl ON CustomersTbl.EquipType = EquipTypeTbl.EquipTypeId) ";

        public static List<CustomerSummary> GetAllCustomerSummarys(string SortBy)
        {
            return CustomerSummaryDAL.GetAllCustomerSummarys(SortBy, -1, "");
        }

        public static List<CustomerSummary> GetAllCustomerSummarys(string SortBy, int IsEnabled)
        {
            return CustomerSummaryDAL.GetAllCustomerSummarys(SortBy, IsEnabled, "");
        }

        public static List<CustomerSummary> GetAllCustomerSummarys(
          string SortBy,
          int IsEnabled,
          string WhereFilter)
        {
            List<CustomerSummary> customerSummarys = new List<CustomerSummary>();
            TrackerDb trackerDb = new TrackerDb();
            string strSQL = "SELECT CustomersTbl.CustomerID, CustomersTbl.CompanyName, CustomersTbl.ContactFirstName, CustomersTbl.ContactLastName, CityTbl.City,  CustomersTbl.PhoneNumber, CustomersTbl.EmailAddress, PersonsTbl.Abreviation AS DeliveryBy, EquipTypeTbl.EquipTypeName, CustomersTbl.MachineSN,  CustomersTbl.autofulfill, CustomersTbl.enabled  FROM (((CustomersTbl LEFT OUTER JOIN PersonsTbl ON CustomersTbl.PreferedAgent = PersonsTbl.PersonID) LEFT OUTER JOIN  CityTbl ON CustomersTbl.City = CityTbl.ID) LEFT OUTER JOIN EquipTypeTbl ON CustomersTbl.EquipType = EquipTypeTbl.EquipTypeId) ";
            string str = "";
            switch (IsEnabled)
            {
                case 0:
                    str = " WHERE CustomersTbl.enabled = false";
                    break;
                case 1:
                    str += " WHERE CustomersTbl.enabled = true";
                    break;
            }
            if (!string.IsNullOrWhiteSpace(WhereFilter))
                str = (!string.IsNullOrWhiteSpace(str) ? str + " AND " : " WHERE ") + WhereFilter;
            if (!string.IsNullOrWhiteSpace(str))
                strSQL += str;
            if (!string.IsNullOrEmpty(SortBy))
                strSQL = $"{strSQL} ORDER BY {SortBy}";
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(strSQL);
            if (dataReader != null)
            {
                while (dataReader.Read())
                    customerSummarys.Add(new CustomerSummary()
                    {
                        CustomerID = Convert.ToInt32(dataReader["CustomerID"]),
                        CompanyName = dataReader["CompanyName"] == DBNull.Value ? "" : dataReader["CompanyName"].ToString(),
                        ContactFirstName = dataReader["ContactFirstName"] == DBNull.Value ? "" : dataReader["ContactFirstName"].ToString(),
                        ContactLastName = dataReader["ContactLastName"] == DBNull.Value ? "" : dataReader["ContactLastName"].ToString(),
                        City = dataReader["City"] == DBNull.Value ? "" : dataReader["City"].ToString(),
                        PhoneNumber = dataReader["PhoneNumber"] == DBNull.Value ? "" : dataReader["PhoneNumber"].ToString(),
                        EmailAddress = dataReader["EmailAddress"] == DBNull.Value ? "" : dataReader["EmailAddress"].ToString(),
                        DeliveryBy = dataReader["DeliveryBy"] == DBNull.Value ? "" : dataReader["DeliveryBy"].ToString(),
                        EquipTypeName = dataReader["EquipTypeName"] == DBNull.Value ? "" : dataReader["EquipTypeName"].ToString(),
                        MachineSN = dataReader["MachineSN"] == DBNull.Value ? "" : dataReader["MachineSN"].ToString(),
                        autofulfill = dataReader["autofulfill"] != DBNull.Value && Convert.ToBoolean(dataReader["autofulfill"]),
                        enabled = dataReader["enabled"] == DBNull.Value || Convert.ToBoolean(dataReader["enabled"])
                    });
                dataReader.Close();
            }
            trackerDb.Close();
            return customerSummarys;
        }
    }
}