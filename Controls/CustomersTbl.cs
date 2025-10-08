// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.control.CustomersTbl
// Assembly: TrackerDotNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerDotNet.dll

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Web;
using System.Web.Configuration;
using TrackerDotNet.Classes;

//- only form later versions #nullable disable
namespace TrackerDotNet.Controls
{
    public class CustomersTbl
    {
        public const long CONST_CustomerID_GENERALOROTHER = 9;
        public const string CONST_STR_CustomerID_GENERALOROTHER = "9";
        private const int CONST_MAXREMINDERS = 10;
        //private const string CONST_CONSTRING = "Tracker08ConnectionString";
        private const string CONST_SQL_CUSTOMERS_SELECT = "SELECT CustomerID, CompanyName, ContactTitle, ContactFirstName, ContactLastName, ContactAltFirstName, ContactAltLastName, Department, BillingAddress, City, StateOrProvince AS Province, PostalCode,  [Country/Region] AS Region, PhoneNumber, Extension, FaxNumber, CellNumber, EmailAddress, AltEmailAddress, ContractNo, CustomerTypeID, EquipType, CoffeePreference, PriPrefQty, PrefPrepTypeID, PrefPackagingID,  SecondaryPreference, SecPrefQty, TypicallySecToo, PreferedAgent, SalesAgentID, MachineSN,  UsesFilter, autofulfill, enabled, PredictionDisabled, AlwaysSendChkUp, NormallyResponds,  ReminderCount, LastDateSentReminder, Notes FROM CustomersTbl";
        private const string CONST_SQL_CUSTOMERS_INSERT = "INSERT INTO CustomersTbl (CompanyName, ContactTitle, ContactFirstName, ContactLastName, ContactAltFirstName, ContactAltLastName, Department, BillingAddress, City, StateOrProvince, PostalCode, [Country/Region], PhoneNumber, Extension, FaxNumber, CellNumber, EmailAddress, AltEmailAddress, ContractNo, CustomerTypeID, EquipType, CoffeePreference, PriPrefQty, PrefPrepTypeID, PrefPackagingID,  SecondaryPreference, SecPrefQty, TypicallySecToo, PreferedAgent, SalesAgentID, MachineSN, UsesFilter, autofulfill, enabled, PredictionDisabled,  AlwaysSendChkUp, NormallyResponds, ReminderCount, Notes) VALUES (?,?,?,?,?, ?,?,?,?,?, ?,?,?,?,?, ?,?,?,?,?, ?,?,?,?,?, ?,?,?,?,?, ?,?,?,?,?, ?,?,?,?)";
        private const string CONST_SQL_CUSTOMERS_UPDATE = "UPDATE CustomersTbl SET CompanyName = ?, ContactTitle = ?, ContactFirstName = ?, ContactLastName = ?,  ContactAltFirstName = ?, ContactAltLastName = ?, Department = ?, BillingAddress = ?, City = ?,  StateOrProvince = ?, PostalCode = ?, [Country/Region] = ?, PhoneNumber = ?, Extension = ?,  FaxNumber = ?, CellNumber = ?, EmailAddress = ?, AltEmailAddress = ?, ContractNo = ?, CustomerTypeID = ?, EquipType = ?, CoffeePreference = ?, PriPrefQty = ?, PrefPrepTypeID = ?, PrefPackagingID = ?,  SecondaryPreference = ?, SecPrefQty = ?, TypicallySecToo = ?, PreferedAgent = ?, SalesAgentID = ?,  MachineSN = ?, UsesFilter = ?, autofulfill = ?, enabled = ?, PredictionDisabled = ?,  AlwaysSendChkUp = ?, NormallyResponds = ?, ReminderCount = ?, Notes = ? WHERE CustomersTbl.CustomerID = ?";
        private const string CONST_SQL_CUSTOMERS_SELECT_REMINDERCOUNT = "SELECT ReminderCount FROM CustomersTbl WHERE CustomersTbl.CustomerID = ?";
        private const string CONST_SQL_CUSTOMERS_SELECT_EQUIPINFO = "SELECT EquipType, MachineSN FROM CustomersTbl WHERE (CustomersTbl.CustomerID = ?)";
        private const string CONST_SQL_CUSTOMERS_INC_REMINDERCOUNT = "UPDATE CustomersTbl SET ReminderCount = ReminderCount + 1 WHERE CustomersTbl.CustomerID = ?";
        private const string CONST_SQL_CUSTOMERS_SETLASTSENTDATE_INCREMINDERCOUNT = "UPDATE CustomersTbl SET LastDateSentReminder = ?, ReminderCount = ReminderCount + 1 WHERE CustomersTbl.CustomerID = ?";
        private const string CONST_SQL_CUSTOMERS_DISABLE = "UPDATE CustomersTbl SET [enabled] = false, Notes = ? + [Notes] WHERE (CustomerID = ?)";
        private const string CONST_SQL_CUSTOMERS_DISABLEIFREMINDERTOHIGH = "UPDATE CustomersTbl SET enabled = false, Notes = ? + [Notes] WHERE (CustomerID = ?) AND (ReminderCount > ?)";
        private const string CONST_SQL_CUSTOMERS_UPDATE_EQUIPINFO = "UPDATE CustomersTbl SET EquipType =?, MachineSN = ? WHERE (CustomersTbl.CustomerID = ?)";
        private const string CONST_SQL_SELECTONBYCUSTOMERSNAME = "SELECT CustomerID, CompanyName, ContactFirstName, ContactLastName, ContactAltFirstName, ContactAltLastName, Department, BillingAddress, City, StateOrProvince AS Province, PostalCode,  PhoneNumber, FaxNumber, CellNumber, EmailAddress, AltEmailAddress,  PreferedAgent, SalesAgentID, MachineSN, enabled FROM CustomersTbl WHERE (CompanyName = ?)";
        private const string CONST_SQL_SELECTONCUSTOMERSNAME = "SELECT CustomerID, CompanyName, ContactFirstName, ContactLastName, ContactAltFirstName, ContactAltLastName, Department, BillingAddress, City, StateOrProvince AS Province, PostalCode,  PhoneNumber, FaxNumber, CellNumber, EmailAddress, AltEmailAddress,  PreferedAgent, SalesAgentID, MachineSN, enabled FROM CustomersTbl WHERE (CompanyName ALIKE '?')";
        private const string CONST_SQL_SELECTONCUSTOMERSEMAIL = "SELECT CustomerID, CompanyName, ContactFirstName, ContactLastName, ContactAltFirstName, ContactAltLastName, Department, BillingAddress, City, StateOrProvince AS Province, PostalCode,  PhoneNumber, FaxNumber, CellNumber, EmailAddress, AltEmailAddress,  PreferedAgent, SalesAgentID, MachineSN, enabled FROM CustomersTbl WHERE (EmailAddress ALIKE '?') OR (AltEmailAddress ALIKE '?')";
        private const string CONST_SQL_CUSTOMERS_RESET_REMINDERCOUNT = "UPDATE CustomersTbl SET ReminderCount = 0";
        private const string CONST_SQL_CUSTOMERS_RESET_REMINDERCOUNT_FORCEENABLE = ", enabled = true";
        private const string CONST_SQL_CUSTOMERS_RESET_REMINDERCOUNT_WHERE = " WHERE CustomerID = ?";
        private long _CustomerID;
        private string _CompanyName;
        private string _ContactTitle;
        private string _ContactFirstName;
        private string _ContactLastName;
        private string _ContactAltFirstName;
        private string _ContactAltLastName;
        private string _Department;
        private string _BillingAddress;
        private int _City;
        private string _Province;
        private string _PostalCode;
        private string _Region;
        private string _PhoneNumber;
        private string _Extension;
        private string _FaxNumber;
        private string _CellNumber;
        private string _EmailAddress;
        private string _AltEmailAddress;
        private string _ContractNo;
        private int _CustomerTypeID;
        private int _EquipType;
        private int _CoffeePreference;
        private double _PriPrefQty;
        private int _PrefPrepTypeID;
        private int _PrefPackagingID;
        private int _SecondaryPreference;
        private double _SecPrefQty;
        private bool _TypicallySecToo;
        private int _PreferedAgent;
        private int _SalesAgentID;
        private string _MachineSN;
        private bool _UsesFilter;
        private bool _autofulfill;
        private bool _enabled;
        private bool _PredictionDisabled;
        private bool _AlwaysSendChkUp;
        private bool _NormallyResponds;
        private int _ReminderCount;
        private DateTime _LastDateSentReminder;
        private string _Notes;

        public CustomersTbl()
        {
            this._CustomerID = 0;
            this._CompanyName = string.Empty;
            this._ContactTitle = string.Empty;
            this._ContactFirstName = string.Empty;
            this._ContactLastName = string.Empty;
            this._ContactAltFirstName = string.Empty;
            this._ContactAltLastName = string.Empty;
            this._Department = string.Empty;
            this._BillingAddress = string.Empty;
            this._City = 0;
            this._Province = string.Empty;
            this._PostalCode = string.Empty;
            this._Region = string.Empty;
            this._PhoneNumber = string.Empty;
            this._Extension = string.Empty;
            this._FaxNumber = string.Empty;
            this._CellNumber = string.Empty;
            this._EmailAddress = string.Empty;
            this._AltEmailAddress = string.Empty;
            this._ContractNo = string.Empty;
            this._CustomerTypeID = 0;
            this._EquipType = 0;
            this._CoffeePreference = 0;
            this._PriPrefQty = 1.0;
            this._PrefPrepTypeID = 0;
            this._PrefPackagingID = 0;
            this._SecondaryPreference = 0;
            this._SecPrefQty = 1.0;
            this._TypicallySecToo = false;
            this._PreferedAgent = 0;
            this._SalesAgentID = 0;
            this._MachineSN = string.Empty;
            this._UsesFilter = false;
            this._autofulfill = false;
            this._enabled = false;
            this._PredictionDisabled = false;
            this._AlwaysSendChkUp = false;
            this._NormallyResponds = false;
            this._ReminderCount = 0;
            this._LastDateSentReminder = DateTime.MinValue;
            this._Notes = string.Empty;
        }

        public long CustomerID
        {
            get => this._CustomerID;
            set => this._CustomerID = value;
        }

        public string CompanyName
        {
            get => this._CompanyName;
            set => this._CompanyName = value;
        }

        public string ContactTitle
        {
            get => this._ContactTitle;
            set => this._ContactTitle = value;
        }

        public string ContactFirstName
        {
            get => this._ContactFirstName;
            set => this._ContactFirstName = value;
        }

        public string ContactLastName
        {
            get => this._ContactLastName;
            set => this._ContactLastName = value;
        }

        public string ContactAltFirstName
        {
            get => this._ContactAltFirstName;
            set => this._ContactAltFirstName = value;
        }

        public string ContactAltLastName
        {
            get => this._ContactAltLastName;
            set => this._ContactAltLastName = value;
        }

        public string Department
        {
            get => this._Department;
            set => this._Department = value;
        }

        public string BillingAddress
        {
            get => this._BillingAddress;
            set => this._BillingAddress = value;
        }

        public int City
        {
            get => this._City;
            set => this._City = value;
        }

        public string Province
        {
            get => this._Province;
            set => this._Province = value;
        }

        public string PostalCode
        {
            get => this._PostalCode;
            set => this._PostalCode = value;
        }

        public string Region
        {
            get => this._Region;
            set => this._Region = value;
        }

        public string PhoneNumber
        {
            get => this._PhoneNumber;
            set => this._PhoneNumber = value;
        }

        public string Extension
        {
            get => this._Extension;
            set => this._Extension = value;
        }

        public string FaxNumber
        {
            get => this._FaxNumber;
            set => this._FaxNumber = value;
        }

        public string CellNumber
        {
            get => this._CellNumber;
            set => this._CellNumber = value;
        }

        public string EmailAddress
        {
            get => this._EmailAddress;
            set => this._EmailAddress = value;
        }

        public string AltEmailAddress
        {
            get => this._AltEmailAddress;
            set => this._AltEmailAddress = value;
        }

        public string ContractNo
        {
            get => this._ContractNo;
            set => this._ContractNo = value;
        }

        public int CustomerTypeID
        {
            get => this._CustomerTypeID;
            set => this._CustomerTypeID = value;
        }

        public int EquipType
        {
            get => this._EquipType;
            set => this._EquipType = value;
        }

        public int CoffeePreference
        {
            get => this._CoffeePreference;
            set => this._CoffeePreference = value;
        }

        public double PriPrefQty
        {
            get => this._PriPrefQty;
            set => this._PriPrefQty = value;
        }

        public int PrefPrepTypeID
        {
            get => this._PrefPrepTypeID;
            set => this._PrefPrepTypeID = value;
        }

        public int PrefPackagingID
        {
            get => this._PrefPackagingID;
            set => this._PrefPackagingID = value;
        }

        public int SecondaryPreference
        {
            get => this._SecondaryPreference;
            set => this._SecondaryPreference = value;
        }

        public double SecPrefQty
        {
            get => this._SecPrefQty;
            set => this._SecPrefQty = value;
        }

        public bool TypicallySecToo
        {
            get => this._TypicallySecToo;
            set => this._TypicallySecToo = value;
        }

        public int PreferedAgent
        {
            get => this._PreferedAgent;
            set => this._PreferedAgent = value;
        }

        public int SalesAgentID
        {
            get => this._SalesAgentID;
            set => this._SalesAgentID = value;
        }

        public string MachineSN
        {
            get => this._MachineSN;
            set => this._MachineSN = value;
        }

        public bool UsesFilter
        {
            get => this._UsesFilter;
            set => this._UsesFilter = value;
        }

        public bool autofulfill
        {
            get => this._autofulfill;
            set => this._autofulfill = value;
        }

        public bool enabled
        {
            get => this._enabled;
            set => this._enabled = value;
        }

        public bool PredictionDisabled
        {
            get => this._PredictionDisabled;
            set => this._PredictionDisabled = value;
        }

        public bool AlwaysSendChkUp
        {
            get => this._AlwaysSendChkUp;
            set => this._AlwaysSendChkUp = value;
        }

        public bool NormallyResponds
        {
            get => this._NormallyResponds;
            set => this._NormallyResponds = value;
        }

        public int ReminderCount
        {
            get => this._ReminderCount;
            set => this._ReminderCount = value;
        }

        public DateTime LastDateSentReminder
        {
            get => this._LastDateSentReminder;
            set => this._LastDateSentReminder = value;
        }

        public string Notes
        {
            get => this._Notes;
            set => this._Notes = value;
        }

        private static CustomersTbl MoveReaderDataToCustomersTblData(IDataReader pDataReader)
        {
            return new CustomersTbl()
            {
                CustomerID = Convert.ToInt32(pDataReader["CustomerID"]),
                CompanyName = pDataReader["CompanyName"] == DBNull.Value ? "" : pDataReader["CompanyName"].ToString(),
                ContactTitle = pDataReader["ContactTitle"] == DBNull.Value ? "" : pDataReader["ContactTitle"].ToString(),
                ContactFirstName = pDataReader["ContactFirstName"] == DBNull.Value ? "" : pDataReader["ContactFirstName"].ToString(),
                ContactLastName = pDataReader["ContactLastName"] == DBNull.Value ? "" : pDataReader["ContactLastName"].ToString(),
                ContactAltFirstName = pDataReader["ContactAltFirstName"] == DBNull.Value ? "" : pDataReader["ContactAltFirstName"].ToString(),
                ContactAltLastName = pDataReader["ContactAltLastName"] == DBNull.Value ? "" : pDataReader["ContactAltLastName"].ToString(),
                Department = pDataReader["Department"] == DBNull.Value ? "" : pDataReader["Department"].ToString(),
                BillingAddress = pDataReader["BillingAddress"] == DBNull.Value ? "" : pDataReader["BillingAddress"].ToString(),
                City = pDataReader["City"] == DBNull.Value ? 0 : Convert.ToInt32(pDataReader["City"]),
                Province = pDataReader["Province"] == DBNull.Value ? "" : pDataReader["Province"].ToString(),
                PostalCode = pDataReader["PostalCode"] == DBNull.Value ? "" : pDataReader["PostalCode"].ToString(),
                Region = pDataReader["Region"] == DBNull.Value ? "" : pDataReader["Region"].ToString(),
                PhoneNumber = pDataReader["PhoneNumber"] == DBNull.Value ? "" : pDataReader["PhoneNumber"].ToString(),
                Extension = pDataReader["Extension"] == DBNull.Value ? "" : pDataReader["Extension"].ToString(),
                FaxNumber = pDataReader["FaxNumber"] == DBNull.Value ? "" : pDataReader["FaxNumber"].ToString(),
                CellNumber = pDataReader["CellNumber"] == DBNull.Value ? "" : pDataReader["CellNumber"].ToString(),
                EmailAddress = pDataReader["EmailAddress"] == DBNull.Value ? "" : pDataReader["EmailAddress"].ToString(),
                AltEmailAddress = pDataReader["AltEmailAddress"] == DBNull.Value ? "" : pDataReader["AltEmailAddress"].ToString(),
                ContractNo = pDataReader["ContractNo"] == DBNull.Value ? "" : pDataReader["ContractNo"].ToString(),
                CustomerTypeID = pDataReader["CustomerTypeID"] == DBNull.Value ? 0 : Convert.ToInt32(pDataReader["CustomerTypeID"]),
                EquipType = pDataReader["EquipType"] == DBNull.Value ? 0 : Convert.ToInt32(pDataReader["EquipType"]),
                CoffeePreference = pDataReader["CoffeePreference"] == DBNull.Value ? 0 : Convert.ToInt32(pDataReader["CoffeePreference"]),
                PriPrefQty = pDataReader["PriPrefQty"] == DBNull.Value ? 1.0 : Convert.ToDouble(pDataReader["PriPrefQty"]),
                PrefPrepTypeID = pDataReader["PrefPrepTypeID"] == DBNull.Value ? 0 : Convert.ToInt32(pDataReader["PrefPrepTypeID"]),
                PrefPackagingID = pDataReader["PrefPackagingID"] == DBNull.Value ? 0 : Convert.ToInt32(pDataReader["PrefPackagingID"]),
                SecondaryPreference = pDataReader["SecondaryPreference"] == DBNull.Value ? 0 : Convert.ToInt32(pDataReader["SecondaryPreference"]),
                SecPrefQty = pDataReader["SecPrefQty"] == DBNull.Value ? 1.0 : Convert.ToDouble(pDataReader["SecPrefQty"]),
                TypicallySecToo = pDataReader["TypicallySecToo"] != DBNull.Value && Convert.ToBoolean(pDataReader["TypicallySecToo"]),
                PreferedAgent = pDataReader["PreferedAgent"] == DBNull.Value ? 0 : Convert.ToInt32(pDataReader["PreferedAgent"]),
                SalesAgentID = pDataReader["SalesAgentID"] == DBNull.Value ? 0 : Convert.ToInt32(pDataReader["SalesAgentID"]),
                MachineSN = pDataReader["MachineSN"] == DBNull.Value ? "" : pDataReader["MachineSN"].ToString(),
                UsesFilter = pDataReader["UsesFilter"] != DBNull.Value && Convert.ToBoolean(pDataReader["UsesFilter"]),
                autofulfill = pDataReader["autofulfill"] != DBNull.Value && Convert.ToBoolean(pDataReader["autofulfill"]),
                enabled = pDataReader["enabled"] != DBNull.Value && Convert.ToBoolean(pDataReader["enabled"]),
                PredictionDisabled = pDataReader["PredictionDisabled"] != DBNull.Value && Convert.ToBoolean(pDataReader["PredictionDisabled"]),
                AlwaysSendChkUp = pDataReader["AlwaysSendChkUp"] != DBNull.Value && Convert.ToBoolean(pDataReader["AlwaysSendChkUp"]),
                NormallyResponds = pDataReader["NormallyResponds"] != DBNull.Value && Convert.ToBoolean(pDataReader["NormallyResponds"]),
                ReminderCount = pDataReader["ReminderCount"] == DBNull.Value ? 0 : Convert.ToInt32(pDataReader["ReminderCount"]),
                LastDateSentReminder = pDataReader["LastDateSentReminder"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(pDataReader["LastDateSentReminder"]).Date,
                Notes = pDataReader["Notes"] == DBNull.Value ? "" : pDataReader["Notes"].ToString()
            };
        }

        private CustomersTbl MoveReaderDataToSummaryCustomerData(IDataReader pDataReader)
        {
            return new CustomersTbl()
            {
                CustomerID = Convert.ToInt32(pDataReader["CustomerID"]),
                CompanyName = pDataReader["CompanyName"] == DBNull.Value ? "" : pDataReader["CompanyName"].ToString(),
                ContactFirstName = pDataReader["ContactFirstName"] == DBNull.Value ? "" : pDataReader["ContactFirstName"].ToString(),
                ContactLastName = pDataReader["ContactLastName"] == DBNull.Value ? "" : pDataReader["ContactLastName"].ToString(),
                ContactAltFirstName = pDataReader["ContactAltFirstName"] == DBNull.Value ? "" : pDataReader["ContactAltFirstName"].ToString(),
                ContactAltLastName = pDataReader["ContactAltLastName"] == DBNull.Value ? "" : pDataReader["ContactAltLastName"].ToString(),
                Department = pDataReader["Department"] == DBNull.Value ? "" : pDataReader["Department"].ToString(),
                BillingAddress = pDataReader["BillingAddress"] == DBNull.Value ? "" : pDataReader["BillingAddress"].ToString(),
                City = pDataReader["City"] == DBNull.Value ? 0 : Convert.ToInt32(pDataReader["City"]),
                Province = pDataReader["Province"] == DBNull.Value ? "" : pDataReader["Province"].ToString(),
                PostalCode = pDataReader["PostalCode"] == DBNull.Value ? "" : pDataReader["PostalCode"].ToString(),
                PhoneNumber = pDataReader["PhoneNumber"] == DBNull.Value ? "" : pDataReader["PhoneNumber"].ToString(),
                FaxNumber = pDataReader["FaxNumber"] == DBNull.Value ? "" : pDataReader["FaxNumber"].ToString(),
                CellNumber = pDataReader["CellNumber"] == DBNull.Value ? "" : pDataReader["CellNumber"].ToString(),
                EmailAddress = pDataReader["EmailAddress"] == DBNull.Value ? "" : pDataReader["EmailAddress"].ToString(),
                AltEmailAddress = pDataReader["AltEmailAddress"] == DBNull.Value ? "" : pDataReader["AltEmailAddress"].ToString(),
                PreferedAgent = pDataReader["PreferedAgent"] == DBNull.Value ? 0 : Convert.ToInt32(pDataReader["PreferedAgent"]),
                SalesAgentID = pDataReader["SalesAgentID"] == DBNull.Value ? 0 : Convert.ToInt32(pDataReader["SalesAgentID"]),
                MachineSN = pDataReader["MachineSN"] == DBNull.Value ? "" : pDataReader["MachineSN"].ToString(),
                enabled = pDataReader["enabled"] != DBNull.Value && Convert.ToBoolean(pDataReader["enabled"])
            };
        }

        public List<CustomersTbl> GetAllCustomers(string SortBy)
        {
            List<CustomersTbl> allCustomers = new List<CustomersTbl>();
            TrackerDb trackerDb = new TrackerDb();
            string strSQL = "SELECT CustomerID, CompanyName, ContactTitle, ContactFirstName, ContactLastName, ContactAltFirstName, ContactAltLastName, Department, BillingAddress, City, StateOrProvince AS Province, PostalCode,  [Country/Region] AS Region, PhoneNumber, Extension, FaxNumber, CellNumber, EmailAddress, AltEmailAddress, ContractNo, CustomerTypeID, EquipType, CoffeePreference, PriPrefQty, PrefPrepTypeID, PrefPackagingID,  SecondaryPreference, SecPrefQty, TypicallySecToo, PreferedAgent, SalesAgentID, MachineSN,  UsesFilter, autofulfill, enabled, PredictionDisabled, AlwaysSendChkUp, NormallyResponds,  ReminderCount, LastDateSentReminder, Notes FROM CustomersTbl";
            if (!string.IsNullOrEmpty(SortBy))
                strSQL = $"{strSQL} ORDER BY {SortBy}";
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(strSQL);
            if (dataReader != null)
            {
                while (dataReader.Read())
                    allCustomers.Add(CustomersTbl.MoveReaderDataToCustomersTblData(dataReader));
                dataReader.Close();
            }
            trackerDb.Close();
            return allCustomers;
        }
        /// <summary>
        /// Gets all customers with company names formatted for ComboBox binding.
        /// Disabled customers are prefixed with underscore, following the same pattern as ItemTypeTbl.GetAllItemDesc()
        /// </summary>
        [DataObjectMethod(DataObjectMethodType.Select)]
        public List<CustomersTbl> GetAllCustomerNames()
        {
            return GetAllCustomerNames(string.Empty);
        }
        /// <summary>
        /// Gets all customers with company names formatted for ComboBox binding with custom sorting.
        /// Disabled customers are prefixed with underscore, following the same pattern as ItemTypeTbl.GetAllItemDesc()
        /// </summary>
        public List<CustomersTbl> GetAllCustomerNames(string sortBy)
        {
            List<CustomersTbl> customerNames = new List<CustomersTbl>();
            TrackerDb trackerDb = new TrackerDb();

            // Use the same IIF pattern as ItemTypeTbl - prefix disabled customers with underscore
            string sql = "SELECT CustomerID, IIF(enabled, CompanyName, '_' + CompanyName) AS CompanyName FROM CustomersTbl";

            // Add sorting - default is enabled first, then alphabetical (same as ItemTypeTbl pattern)
            if (!string.IsNullOrEmpty(sortBy))
            {
                sql += $" ORDER BY {sortBy}";
            }
            else
            {
                sql += " ORDER BY IIF(enabled,1,0) DESC, CompanyName ASC";
            }

            IDataReader reader = trackerDb.ExecuteSQLGetDataReader(sql);
            if (reader != null)
            {
                while (reader.Read())
                {
                    customerNames.Add(new CustomersTbl
                    {
                        CustomerID = reader["CustomerID"] == DBNull.Value ? 0 : Convert.ToInt64(reader["CustomerID"]),
                        CompanyName = reader["CompanyName"] == DBNull.Value ? string.Empty : reader["CompanyName"].ToString()
                    });
                }
                reader.Close();
            }
            trackerDb.Close();

            return customerNames;
        }

        public CustomersTbl GetCustomerByCustomerID(long pCustomerID)
        {
            return this.GetCustomerByCustomerID(pCustomerID, "");
        }

        public CustomersTbl GetCustomerByCustomerID(long pCustomerID, string pSortBy)
        {
            CustomersTbl customersByCustomerID = new CustomersTbl();
            TrackerDb trackerDb = new TrackerDb();
            string strSQL = "SELECT CustomerID, CompanyName, ContactTitle, ContactFirstName, ContactLastName, ContactAltFirstName, ContactAltLastName, Department, BillingAddress, City, StateOrProvince AS Province, PostalCode,  [Country/Region] AS Region, PhoneNumber, Extension, FaxNumber, CellNumber, EmailAddress, AltEmailAddress, ContractNo, CustomerTypeID, EquipType, CoffeePreference, PriPrefQty, PrefPrepTypeID, PrefPackagingID,  SecondaryPreference, SecPrefQty, TypicallySecToo, PreferedAgent, SalesAgentID, MachineSN,  UsesFilter, autofulfill, enabled, PredictionDisabled, AlwaysSendChkUp, NormallyResponds,  ReminderCount, LastDateSentReminder, Notes FROM CustomersTbl";
            if (pCustomerID > 0L)
            {
                strSQL += " WHERE CustomerID = ? ";
                trackerDb.AddWhereParams((object)pCustomerID, DbType.Int64, "@CustomerID");
            }
            if (!string.IsNullOrEmpty(pSortBy))
                strSQL = $"{strSQL} ORDER BY {pSortBy}";
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(strSQL);
            if (dataReader != null)
            {
                if (dataReader.Read())
                    customersByCustomerID = CustomersTbl.MoveReaderDataToCustomersTblData(dataReader);
                dataReader.Close();
            }
            trackerDb.Close();
            return customersByCustomerID;
        }

        public bool InsertCustomer(CustomersTbl pThisCustomerTblData)
        {
            string empty = string.Empty;
            return this.InsertCustomer(pThisCustomerTblData, ref empty);
        }

        public bool InsertCustomer(CustomersTbl pThisCustomerTblData, ref string pErrorStr)
        {
            pErrorStr = "";
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddParams((object)pThisCustomerTblData.CompanyName, DbType.String, "@CompanyName");
            trackerDb.AddParams((object)pThisCustomerTblData.ContactTitle, DbType.String, "@ContactTitle");
            trackerDb.AddParams((object)pThisCustomerTblData.ContactFirstName, DbType.String, "@ContactFirstName");
            trackerDb.AddParams((object)pThisCustomerTblData.ContactLastName, DbType.String, "@ContactLastName");
            trackerDb.AddParams((object)pThisCustomerTblData.ContactAltFirstName, DbType.String, "@ContactAltFirstName");
            trackerDb.AddParams((object)pThisCustomerTblData.ContactAltLastName, DbType.String, "@ContactAltLastName");
            trackerDb.AddParams((object)pThisCustomerTblData.Department, DbType.String, "@Department");
            trackerDb.AddParams((object)pThisCustomerTblData.BillingAddress, DbType.String, "@BillingAddress");
            trackerDb.AddParams((object)pThisCustomerTblData.City, DbType.Int32, "@City");
            trackerDb.AddParams((object)pThisCustomerTblData.Province, DbType.String, "@Province");
            trackerDb.AddParams((object)pThisCustomerTblData.PostalCode, DbType.String, "@PostalCode");
            trackerDb.AddParams((object)pThisCustomerTblData.Region, DbType.String, "@Region");
            trackerDb.AddParams((object)pThisCustomerTblData.PhoneNumber, DbType.String, "@PhoneNumber");
            trackerDb.AddParams((object)pThisCustomerTblData.Extension, DbType.String, "@Extension");
            trackerDb.AddParams((object)pThisCustomerTblData.FaxNumber, DbType.String, "@FaxNumber");
            trackerDb.AddParams((object)pThisCustomerTblData.CellNumber, DbType.String, "@CellNumber");
            trackerDb.AddParams((object)pThisCustomerTblData.EmailAddress, DbType.String, "@EmailAddress");
            trackerDb.AddParams((object)pThisCustomerTblData.AltEmailAddress, DbType.String, "@AltEmailAddress");
            trackerDb.AddParams((object)pThisCustomerTblData.ContractNo, DbType.String, "@ContractNo");
            trackerDb.AddParams((object)pThisCustomerTblData.CustomerTypeID, DbType.Int32, "@CustomerTypeID");
            trackerDb.AddParams((object)pThisCustomerTblData.EquipType, DbType.Int32, "@EquipType");
            trackerDb.AddParams((object)pThisCustomerTblData.CoffeePreference, DbType.Int32, "@CoffeePreference");
            trackerDb.AddParams((object)pThisCustomerTblData.PriPrefQty, DbType.Double, "@PriPrefQty");
            trackerDb.AddParams((object)pThisCustomerTblData.PrefPrepTypeID, DbType.Int32, "@PrefPrepTypeID");
            trackerDb.AddParams((object)pThisCustomerTblData.PrefPackagingID, DbType.Int32, "@PrefPackagingID");
            trackerDb.AddParams((object)pThisCustomerTblData.SecondaryPreference, DbType.String, "@SecondaryPreference");
            trackerDb.AddParams((object)pThisCustomerTblData.SecPrefQty, DbType.Double, "@SecPrefQty");
            trackerDb.AddParams((object)pThisCustomerTblData.TypicallySecToo, DbType.Boolean, "@TypicallySecToo");
            trackerDb.AddParams((object)pThisCustomerTblData.PreferedAgent, DbType.Int32, "@PreferedAgent");
            trackerDb.AddParams((object)pThisCustomerTblData.SalesAgentID, DbType.Int32, "@SalesAgentID");
            trackerDb.AddParams((object)pThisCustomerTblData.MachineSN, DbType.String, "@MachineSN");
            trackerDb.AddParams((object)pThisCustomerTblData.UsesFilter, DbType.Boolean, "@UsesFilter");
            trackerDb.AddParams((object)pThisCustomerTblData.autofulfill, DbType.Boolean, "@autofulfill");
            trackerDb.AddParams((object)pThisCustomerTblData.enabled, DbType.Boolean, "@enabled");
            trackerDb.AddParams((object)pThisCustomerTblData.PredictionDisabled, DbType.Boolean, "@PredictionDisabled");
            trackerDb.AddParams((object)pThisCustomerTblData.AlwaysSendChkUp, DbType.Boolean, "@AlwaysSendChkUp");
            trackerDb.AddParams((object)pThisCustomerTblData.NormallyResponds, DbType.Boolean, "@NormallyResponds");
            trackerDb.AddParams((object)pThisCustomerTblData.ReminderCount, DbType.Int32, "@ReminderCount");
            trackerDb.AddParams((object)pThisCustomerTblData.Notes, DbType.String, "@Notes");
            pErrorStr = trackerDb.ExecuteNonQuerySQL("INSERT INTO CustomersTbl (CompanyName, ContactTitle, ContactFirstName, ContactLastName, ContactAltFirstName, ContactAltLastName, Department, BillingAddress, City, StateOrProvince, PostalCode, [Country/Region], PhoneNumber, Extension, FaxNumber, CellNumber, EmailAddress, AltEmailAddress, ContractNo, CustomerTypeID, EquipType, CoffeePreference, PriPrefQty, PrefPrepTypeID, PrefPackagingID,  SecondaryPreference, SecPrefQty, TypicallySecToo, PreferedAgent, SalesAgentID, MachineSN, UsesFilter, autofulfill, enabled, PredictionDisabled,  AlwaysSendChkUp, NormallyResponds, ReminderCount, Notes) VALUES (?,?,?,?,?, ?,?,?,?,?, ?,?,?,?,?, ?,?,?,?,?, ?,?,?,?,?, ?,?,?,?,?, ?,?,?,?,?, ?,?,?,?)");
            bool flag = string.IsNullOrEmpty(pErrorStr);
            trackerDb.Close();
            return flag;
        }

        public string UpdateCustomer(CustomersTbl pThisCustomerTblData, long CustomerIDToUpdate)
        {
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddParams((object)pThisCustomerTblData.CompanyName, DbType.String);
            trackerDb.AddParams((object)pThisCustomerTblData.ContactTitle, DbType.String);
            trackerDb.AddParams((object)pThisCustomerTblData.ContactFirstName, DbType.String);
            trackerDb.AddParams((object)pThisCustomerTblData.ContactLastName, DbType.String);
            trackerDb.AddParams((object)pThisCustomerTblData.ContactAltFirstName, DbType.String);
            trackerDb.AddParams((object)pThisCustomerTblData.ContactAltLastName, DbType.String);
            trackerDb.AddParams((object)pThisCustomerTblData.Department, DbType.String);
            trackerDb.AddParams((object)pThisCustomerTblData.BillingAddress, DbType.String);
            trackerDb.AddParams((object)pThisCustomerTblData.City, DbType.Int32);
            trackerDb.AddParams((object)pThisCustomerTblData.Province, DbType.String);
            trackerDb.AddParams((object)pThisCustomerTblData.PostalCode, DbType.String);
            trackerDb.AddParams((object)pThisCustomerTblData.Region, DbType.String);
            trackerDb.AddParams((object)pThisCustomerTblData.PhoneNumber, DbType.String);
            trackerDb.AddParams((object)pThisCustomerTblData.Extension, DbType.String);
            trackerDb.AddParams((object)pThisCustomerTblData.FaxNumber, DbType.String);
            trackerDb.AddParams((object)pThisCustomerTblData.CellNumber, DbType.String);
            trackerDb.AddParams((object)pThisCustomerTblData.EmailAddress, DbType.String);
            trackerDb.AddParams((object)pThisCustomerTblData.AltEmailAddress, DbType.String);
            trackerDb.AddParams((object)pThisCustomerTblData.ContractNo, DbType.String);
            trackerDb.AddParams((object)pThisCustomerTblData.CustomerTypeID, DbType.Int32);
            trackerDb.AddParams((object)pThisCustomerTblData.EquipType, DbType.Int32);
            trackerDb.AddParams((object)pThisCustomerTblData.CoffeePreference, DbType.Int32);
            trackerDb.AddParams((object)pThisCustomerTblData.PriPrefQty, DbType.Double);
            trackerDb.AddParams((object)pThisCustomerTblData.PrefPrepTypeID, DbType.Int32);
            trackerDb.AddParams((object)pThisCustomerTblData.PrefPackagingID, DbType.Int32);
            trackerDb.AddParams((object)pThisCustomerTblData.SecondaryPreference, DbType.Int32);
            trackerDb.AddParams((object)pThisCustomerTblData.SecPrefQty, DbType.Double);
            trackerDb.AddParams((object)pThisCustomerTblData.TypicallySecToo, DbType.Boolean);
            trackerDb.AddParams((object)pThisCustomerTblData.PreferedAgent, DbType.Int32);
            trackerDb.AddParams((object)pThisCustomerTblData.SalesAgentID, DbType.Int32);
            trackerDb.AddParams((object)pThisCustomerTblData.MachineSN, DbType.String);
            trackerDb.AddParams((object)pThisCustomerTblData.UsesFilter, DbType.Boolean);
            trackerDb.AddParams((object)pThisCustomerTblData.autofulfill, DbType.Boolean);
            trackerDb.AddParams((object)pThisCustomerTblData.enabled, DbType.Boolean);
            trackerDb.AddParams((object)pThisCustomerTblData.PredictionDisabled, DbType.Boolean);
            trackerDb.AddParams((object)pThisCustomerTblData.AlwaysSendChkUp, DbType.Boolean);
            trackerDb.AddParams((object)pThisCustomerTblData.NormallyResponds, DbType.Boolean);
            trackerDb.AddParams((object)pThisCustomerTblData.ReminderCount, DbType.Int32);
            trackerDb.AddParams((object)pThisCustomerTblData.Notes, DbType.String);
            trackerDb.AddWhereParams((object)CustomerIDToUpdate, DbType.Int64);
            string str = trackerDb.ExecuteNonQuerySQL("UPDATE CustomersTbl SET CompanyName = ?, ContactTitle = ?, ContactFirstName = ?, ContactLastName = ?,  ContactAltFirstName = ?, ContactAltLastName = ?, Department = ?, BillingAddress = ?, City = ?,  StateOrProvince = ?, PostalCode = ?, [Country/Region] = ?, PhoneNumber = ?, Extension = ?,  FaxNumber = ?, CellNumber = ?, EmailAddress = ?, AltEmailAddress = ?, ContractNo = ?, CustomerTypeID = ?, EquipType = ?, CoffeePreference = ?, PriPrefQty = ?, PrefPrepTypeID = ?, PrefPackagingID = ?,  SecondaryPreference = ?, SecPrefQty = ?, TypicallySecToo = ?, PreferedAgent = ?, SalesAgentID = ?,  MachineSN = ?, UsesFilter = ?, autofulfill = ?, enabled = ?, PredictionDisabled = ?,  AlwaysSendChkUp = ?, NormallyResponds = ?, ReminderCount = ?, Notes = ? WHERE CustomersTbl.CustomerID = ?");
            trackerDb.Close();
            return str;
        }

        public int GetReminderCount(long pCustomerID)
        {
            int reminderCount = SystemConstants.DatabaseConstants.InvalidID;
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddWhereParams((object)pCustomerID, DbType.Int64);
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader("SELECT ReminderCount FROM CustomersTbl WHERE CustomersTbl.CustomerID = ?");
            if (dataReader != null)
            {
                if (dataReader.Read())
                    reminderCount = dataReader["ReminderCount"] == DBNull.Value ? -1 : Convert.ToInt32(dataReader["ReminderCount"]);
                dataReader.Dispose();
            }
            trackerDb.Close();
            return reminderCount;
        }

        public string SetEquipDetailsIfEmpty(int pEquipType, string pMachineSN, long pCustomerID)
        {
            string str1 = string.Empty;
            int num = 0;
            string str2 = string.Empty;
            bool flag = false;
            TrackerDb trackerDb1 = new TrackerDb();
            trackerDb1.AddWhereParams((object)pCustomerID, DbType.Int64);
            IDataReader dataReader = trackerDb1.ExecuteSQLGetDataReader("SELECT EquipType, MachineSN FROM CustomersTbl WHERE (CustomersTbl.CustomerID = ?)");
            if (dataReader != null)
            {
                if (dataReader.Read())
                {
                    num = dataReader["EquipType"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["EquipType"]);
                    str2 = dataReader["MachineSN"] == DBNull.Value ? string.Empty : dataReader["MachineSN"].ToString();
                }
                dataReader.Dispose();
            }
            trackerDb1.Close();
            if (pEquipType > 0 && num.Equals(0))
                flag = true;
            else if (num > 0)
                pEquipType = num;
            if (!string.IsNullOrEmpty(pMachineSN) && string.IsNullOrEmpty(str2))
                flag = true;
            else if (!string.IsNullOrEmpty(str2))
                pMachineSN = str2;
            if (flag)
            {
                TrackerDb trackerDb2 = new TrackerDb();
                trackerDb2.AddParams((object)pEquipType, DbType.Int32);
                trackerDb2.AddParams((object)pMachineSN, DbType.String);
                trackerDb2.AddWhereParams((object)pCustomerID, DbType.Int64);
                str1 = trackerDb2.ExecuteNonQuerySQL("UPDATE CustomersTbl SET EquipType =?, MachineSN = ? WHERE (CustomersTbl.CustomerID = ?)");
                trackerDb2.Close();
            }
            return str1;
        }

        public bool IncrementReminderCount(long pCustomerID)
        {
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddWhereParams((object)pCustomerID, DbType.Int64);
            bool flag = string.IsNullOrEmpty(trackerDb.ExecuteNonQuerySQLWithParams("UPDATE CustomersTbl SET ReminderCount = ReminderCount + 1 WHERE CustomersTbl.CustomerID = ?", trackerDb.WhereParams));
            trackerDb.Close();
            return flag;
        }

        public string SetSentReminderAndIncrementReminderCount(DateTime pLastSentDate, long pCustomerID)
        {
            string empty = string.Empty;
            TrackerDb trackerDb = new TrackerDb();
            string strSQL = "UPDATE CustomersTbl SET LastDateSentReminder = ?, ReminderCount = ReminderCount + 1 WHERE CustomersTbl.CustomerID = ?";
            trackerDb.AddParams((object)pLastSentDate, DbType.Date);
            trackerDb.AddWhereParams((object)pCustomerID, DbType.Int64);
            string str = trackerDb.ExecuteNonQuerySQLWithParams(strSQL, trackerDb.Params, trackerDb.WhereParams);
            trackerDb.Close();
            return str;
        }

        public bool ResetReminderCount(long pCustomerID, bool pForceEnable)
        {
            string str = "UPDATE CustomersTbl SET ReminderCount = 0";
            if (pForceEnable)
                str += ", enabled = true";
            string strSQL = str + " WHERE CustomerID = ?";
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddWhereParams((object)pCustomerID, DbType.Int64);
            bool flag = string.IsNullOrWhiteSpace(trackerDb.ExecuteNonQuerySQLWithParams(strSQL, trackerDb.WhereParams));
            trackerDb.Close();
            return flag;
        }

        public bool DisableCustomer(long customerId)
        {
            string currentUser = HttpContext.Current?.User?.Identity?.Name ?? "System";
            string notes = $"Customer disabled by {currentUser} on {TimeZoneUtils.Now():d}\n";
            return DisableCustomer(customerId, notes);
        }

        public bool DisableCustomer(long customerId, string notes)
        {
            string currentUser = HttpContext.Current?.User?.Identity?.Name ?? "System";

            // If notes are provided, append user and date info
            if (!string.IsNullOrEmpty(notes))
            {
                notes = $"{notes}\nDisabled by {currentUser} on {TimeZoneUtils.Now():d}\n";
            }

            string sql = CONST_SQL_CUSTOMERS_DISABLE;
            TrackerDb trackerDb = new TrackerDb();

            try
            {
                trackerDb.AddParams((object)notes, DbType.String);
                trackerDb.AddWhereParams((object)customerId, DbType.Int64);

                string result = trackerDb.ExecuteNonQuerySQL(sql);
                bool success = string.IsNullOrEmpty(result);

                // Log the action
                if (success)
                {
                    var customer = GetCustomerByCustomerID(customerId);
                    string logMessage = $"Customer {customer?.CompanyName ?? customerId.ToString()} disabled by {currentUser}";
                    if (!string.IsNullOrEmpty(notes))
                    {
                        logMessage += $". Notes: {notes}";
                    }
                    AppLogger.WriteLog(SystemConstants.LogTypes.Customers, logMessage);
                }
                else
                {
                    AppLogger.WriteLog("error",
                        $"Failed to disable customer {customerId}. Error: {result}");
                }

                return success;
            }
            catch (Exception ex)
            {
                string errorMsg = $"Error disabling customer {customerId}: {ex.Message}";
                AppLogger.WriteLog("error", errorMsg);
                return false;
            }
            finally
            {
                trackerDb.Close();
            }
        }

        public bool DisableCustomerIfReminderToHigh(long pCustomerID, int pReminderCount)
        {
            string strSQL = "UPDATE CustomersTbl SET enabled = false, Notes = ? + [Notes] WHERE (CustomerID = ?) AND (ReminderCount > ?)";
            TrackerDb trackerDb = new TrackerDb();
            string pDataValue = $"Customer set to disabled: {TimeZoneUtils.Now():d} \n";
            trackerDb.AddParams((object)pDataValue, DbType.String);
            trackerDb.AddWhereParams((object)pCustomerID, DbType.Int64);
            trackerDb.AddWhereParams((object)pReminderCount, DbType.Int32);
            bool high = string.IsNullOrWhiteSpace(trackerDb.ExecuteNonQuerySQL(strSQL));
            trackerDb.Close();
            return high;
        }

        public CustomersTbl GetCustomerByName(string pCustomerName)
        {
            CustomersTbl customerByName = new CustomersTbl();
            TrackerDb trackerDb = new TrackerDb();
            string strSQL = "SELECT CustomerID, CompanyName, ContactFirstName, ContactLastName, ContactAltFirstName, ContactAltLastName, Department, BillingAddress, City, StateOrProvince AS Province, PostalCode,  PhoneNumber, FaxNumber, CellNumber, EmailAddress, AltEmailAddress,  PreferedAgent, SalesAgentID, MachineSN, enabled FROM CustomersTbl WHERE (CompanyName = ?)";
            trackerDb.AddWhereParams((object)pCustomerName);
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(strSQL);
            if (dataReader != null)
            {
                if (dataReader.Read())
                    customerByName = this.MoveReaderDataToSummaryCustomerData(dataReader);
                dataReader.Close();
            }
            trackerDb.Close();
            return customerByName;
        }

        public List<CustomersTbl> GetAllCustomerWithNameLIKE(string pCustomerName)
        {
            List<CustomersTbl> customerWithNameLike = new List<CustomersTbl>();
            TrackerDb trackerDb = new TrackerDb();
            if (!pCustomerName.Contains("%"))
                pCustomerName = $"%{pCustomerName}%";
            string strSQL = "SELECT CustomerID, CompanyName, ContactFirstName, ContactLastName, ContactAltFirstName, ContactAltLastName, Department, BillingAddress, City, StateOrProvince AS Province, PostalCode,  PhoneNumber, FaxNumber, CellNumber, EmailAddress, AltEmailAddress,  PreferedAgent, SalesAgentID, MachineSN, enabled FROM CustomersTbl WHERE (CompanyName ALIKE '?')";
            trackerDb.AddWhereParams((object)pCustomerName);
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(strSQL);
            if (dataReader != null)
            {
                while (dataReader.Read())
                    customerWithNameLike.Add(this.MoveReaderDataToSummaryCustomerData(dataReader));
                dataReader.Close();
            }
            trackerDb.Close();
            return customerWithNameLike;
        }

        public List<CustomersTbl> GetAllCustomerWithEmailLIKE(string pEmail)
        {
            List<CustomersTbl> customerWithEmailLike = new List<CustomersTbl>();
            TrackerDb trackerDb = new TrackerDb();
            if (!pEmail.Contains("%"))
                pEmail += "%";
            string strSQL = "SELECT CustomerID, CompanyName, ContactFirstName, ContactLastName, ContactAltFirstName, ContactAltLastName, Department, BillingAddress, City, StateOrProvince AS Province, PostalCode,  PhoneNumber, FaxNumber, CellNumber, EmailAddress, AltEmailAddress,  PreferedAgent, SalesAgentID, MachineSN, enabled FROM CustomersTbl WHERE (EmailAddress ALIKE '?') OR (AltEmailAddress ALIKE '?')".Replace("?", pEmail);
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(strSQL);
            if (dataReader != null)
            {
                while (dataReader.Read())
                    customerWithEmailLike.Add(this.MoveReaderDataToSummaryCustomerData(dataReader));
                dataReader.Close();
            }
            trackerDb.Close();
            return customerWithEmailLike;
        }

        public string ResetReminderCount(long customerID)
        {
            string result = string.Empty;
            TrackerDb trackerDb = new TrackerDb();

            try
            {
                trackerDb.AddWhereParams(customerID, DbType.Int64);
                result = trackerDb.ExecuteNonQuerySQL("UPDATE CustomersTbl SET ReminderCount = 0, LastDateSentReminder = NULL WHERE CustomerID = ?");
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Database, $"Error resetting reminder count for customer {customerID}: {ex.Message}");
                result = ex.Message;
            }
            finally
            {
                trackerDb.Close();
            }

            return result;
        }
        private static ConcurrentDictionary<long, string> _customerNameCache;
        private static DateTime _lastCacheRefresh = DateTime.MinValue;
        private static readonly object _cacheLock = new object();
        private static readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(1);  //-> make it a minutes so that if anythign changes we can refresh, but if a table is being displayed it is fine

        public static string GetCustomerNameById(long customerId)
        {
            EnsureCustomerCache();
            if (_customerNameCache.TryGetValue(customerId, out var name))
                return name;
            return string.Empty;
        }

        private static void EnsureCustomerCache()
        {
            // Refresh cache every 30 minutes or if not loaded
            if (_customerNameCache == null || (DateTime.Now - _lastCacheRefresh) > _cacheDuration)
            {
                lock (_cacheLock)
                {
                    if (_customerNameCache == null || (DateTime.Now - _lastCacheRefresh) > _cacheDuration)
                    {
                        var dict = new ConcurrentDictionary<long, string>();
                        var allCustomers = new CustomersTbl().GetAllCustomers("CompanyName");
                        foreach (var cust in allCustomers)
                        {
                            dict[cust.CustomerID] = cust.CompanyName;
                        }
                        _customerNameCache = dict;
                        _lastCacheRefresh = DateTime.Now;
                    }
                }
            }
        }
    }
}