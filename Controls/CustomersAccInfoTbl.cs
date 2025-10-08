// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.control.CustomersAccInfoTbl
// Assembly: TrackerDotNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerDotNet.dll

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using TrackerDotNet.Classes;

//- only form later versions #nullable disable
namespace TrackerDotNet.Controls
{
    public class CustomersAccInfoTbl
    {
        public const int CONST_DEFAULTPAYMENTTERMID = 5;
        public const int CONST_DEFAULTPRICELEVEL = 1;
        private const string CONST_SQL_SELECT = "SELECT CustomersAccInfoID, CustomerID, RequiresPurchOrder, CustomerVATNo, BillAddr1, BillAddr2, BillAddr3, BillAddr4,  BillAddr5, ShipAddr1, ShipAddr2, ShipAddr3, ShipAddr4, ShipAddr5, AccEmail, AltAccEmail,  PaymentTermID, Limit, FullCoName, AccFirstName, AccLastName, AltAccFirstName, AltAccLastName, PriceLevelID,  InvoiceTypeID, RegNo, BankAccNo, BankBranch, Enabled, Notes FROM CustomersAccInfoTbl";
        private const string CONST_SQL_SELECTBYCUSTID = "SELECT CustomersAccInfoID, RequiresPurchOrder, CustomerVATNo, BillAddr1, BillAddr2, BillAddr3, BillAddr4,  BillAddr5, ShipAddr1, ShipAddr2, ShipAddr3, ShipAddr4, ShipAddr5, AccEmail, AltAccEmail,  PaymentTermID, Limit, FullCoName, AccFirstName, AccLastName, AltAccFirstName, AltAccLastName, PriceLevelID,  InvoiceTypeID, RegNo, BankAccNo, BankBranch, Enabled, Notes FROM CustomersAccInfoTbl WHERE (CustomerID = ?)";
        private const string CONST_SQL_SELECTPaymentTermID_BYCUSTID = "SELECT PaymentTermID FROM CustomersAccInfoTbl WHERE (CustomerID = ?)";
        private const string CONST_SQL_INSERT = "INSERT INTO CustomersAccInfoTbl (CustomerID, RequiresPurchOrder, CustomerVATNo, BillAddr1, BillAddr2, BillAddr3, BillAddr4, BillAddr5,  ShipAddr1, ShipAddr2, ShipAddr3, ShipAddr4, ShipAddr5, AccEmail, AltAccEmail, PaymentTermID,  Limit, FullCoName, AccFirstName, AccLastName, AltAccFirstName, AltAccLastName, PriceLevelID, InvoiceTypeID,  RegNo, BankAccNo, BankBranch, Enabled, Notes) VALUES ( ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";
        private const string CONST_SQL_UPDATE = "UPDATE CustomersAccInfoTbl SET CustomerID = ?, RequiresPurchOrder = ?, CustomerVATNo = ?, BillAddr1 = ?, BillAddr2 = ?, BillAddr3 = ?, BillAddr4 = ?, BillAddr5 = ?,  ShipAddr1 = ?, ShipAddr2 = ?, ShipAddr3 = ?, ShipAddr4 = ?, ShipAddr5 = ?, AccEmail = ?, AltAccEmail = ?, PaymentTermID = ?,  Limit = ?, FullCoName = ?, AccFirstName = ?, AccLastName = ?, AltAccFirstName = ?, AltAccLastName = ?, PriceLevelID = ?, InvoiceTypeID = ?,  RegNo = ?, BankAccNo = ?, BankBranch = ?, Enabled = ?, Notes = ? WHERE (CustomersAccInfoID = ?)";
        private const string CONST_SQL_DELETE = "DELETE FROM CustomersAccInfoTbl WHERE (CustomersAccInfoID = ?)";
        private const string CONST_SQL_GETCUSTOMERSINVOICETYPE = "SELECT InvoiceTypeID FROM CustomersAccInfoTbl WHERE (CustomerID = ?)";
        private int _CustomersAccInfoID;
        private long _CustomerID;
        private bool _RequiresPurchOrder;
        private string _CustomerVATNo;
        private string _BillAddr1;
        private string _BillAddr2;
        private string _BillAddr3;
        private string _BillAddr4;
        private string _BillAddr5;
        private string _ShipAddr1;
        private string _ShipAddr2;
        private string _ShipAddr3;
        private string _ShipAddr4;
        private string _ShipAddr5;
        private string _AccEmail;
        private string _AltAccEmail;
        private int _PaymentTermID;
        private double _Limit;
        private string _FullCoName;
        private string _AccFirstName;
        private string _AccLastName;
        private string _AltAccFirstName;
        private string _AltAccLastName;
        private int _PriceLevelID;
        private int _InvoiceTypeID;
        private string _RegNo;
        private string _BankAccNo;
        private string _BankBranch;
        private bool _Enabled;
        private string _Notes;

        public CustomersAccInfoTbl()
        {
            this._CustomersAccInfoID = 0;
            this._CustomerID = 0;
            this._RequiresPurchOrder = false;
            this._CustomerVATNo = string.Empty;
            this._BillAddr1 = string.Empty;
            this._BillAddr2 = string.Empty;
            this._BillAddr3 = string.Empty;
            this._BillAddr4 = string.Empty;
            this._BillAddr5 = string.Empty;
            this._ShipAddr1 = string.Empty;
            this._ShipAddr2 = string.Empty;
            this._ShipAddr3 = string.Empty;
            this._ShipAddr4 = string.Empty;
            this._ShipAddr5 = string.Empty;
            this._AccEmail = string.Empty;
            this._AltAccEmail = string.Empty;
            this._PaymentTermID = 1;
            this._Limit = 0.0;
            this._FullCoName = string.Empty;
            this._AccFirstName = string.Empty;
            this._AccLastName = string.Empty;
            this._AltAccFirstName = string.Empty;
            this._AltAccLastName = string.Empty;
            this._PriceLevelID = 1;
            this._InvoiceTypeID = 1;
            this._RegNo = string.Empty;
            this._BankAccNo = string.Empty;
            this._BankBranch = string.Empty;
            this._Enabled = false;
            this._Notes = string.Empty;
        }

        public int CustomersAccInfoID
        {
            get => this._CustomersAccInfoID;
            set => this._CustomersAccInfoID = value;
        }

        public long CustomerID
        {
            get => this._CustomerID;
            set => this._CustomerID = value;
        }

        public bool RequiresPurchOrder
        {
            get => this._RequiresPurchOrder;
            set => this._RequiresPurchOrder = value;
        }

        public string CustomerVATNo
        {
            get => this._CustomerVATNo;
            set => this._CustomerVATNo = value;
        }

        public string BillAddr1
        {
            get => this._BillAddr1;
            set => this._BillAddr1 = value;
        }

        public string BillAddr2
        {
            get => this._BillAddr2;
            set => this._BillAddr2 = value;
        }

        public string BillAddr3
        {
            get => this._BillAddr3;
            set => this._BillAddr3 = value;
        }

        public string BillAddr4
        {
            get => this._BillAddr4;
            set => this._BillAddr4 = value;
        }

        public string BillAddr5
        {
            get => this._BillAddr5;
            set => this._BillAddr5 = value;
        }

        public string ShipAddr1
        {
            get => this._ShipAddr1;
            set => this._ShipAddr1 = value;
        }

        public string ShipAddr2
        {
            get => this._ShipAddr2;
            set => this._ShipAddr2 = value;
        }

        public string ShipAddr3
        {
            get => this._ShipAddr3;
            set => this._ShipAddr3 = value;
        }

        public string ShipAddr4
        {
            get => this._ShipAddr4;
            set => this._ShipAddr4 = value;
        }

        public string ShipAddr5
        {
            get => this._ShipAddr5;
            set => this._ShipAddr5 = value;
        }

        public string AccEmail
        {
            get => this._AccEmail;
            set => this._AccEmail = value;
        }

        public string AltAccEmail
        {
            get => this._AltAccEmail;
            set => this._AltAccEmail = value;
        }

        public int PaymentTermID
        {
            get => this._PaymentTermID;
            set => this._PaymentTermID = value;
        }

        public double Limit
        {
            get => this._Limit;
            set => this._Limit = value;
        }

        public string FullCoName
        {
            get => this._FullCoName;
            set => this._FullCoName = value;
        }

        public string AccFirstName
        {
            get => this._AccFirstName;
            set => this._AccFirstName = value;
        }

        public string AccLastName
        {
            get => this._AccLastName;
            set => this._AccLastName = value;
        }

        public string AltAccFirstName
        {
            get => this._AltAccFirstName;
            set => this._AltAccFirstName = value;
        }

        public string AltAccLastName
        {
            get => this._AltAccLastName;
            set => this._AltAccLastName = value;
        }

        public int PriceLevelID
        {
            get => this._PriceLevelID;
            set => this._PriceLevelID = value;
        }

        public int InvoiceTypeID
        {
            get => this._InvoiceTypeID;
            set => this._InvoiceTypeID = value;
        }

        public string RegNo
        {
            get => this._RegNo;
            set => this._RegNo = value;
        }

        public string BankAccNo
        {
            get => this._BankAccNo;
            set => this._BankAccNo = value;
        }

        public string BankBranch
        {
            get => this._BankBranch;
            set => this._BankBranch = value;
        }

        public bool Enabled
        {
            get => this._Enabled;
            set => this._Enabled = value;
        }

        public string Notes
        {
            get => this._Notes;
            set => this._Notes = value;
        }

        [DataObjectMethod(DataObjectMethodType.Select, true)]
        public List<CustomersAccInfoTbl> GetAll(string SortBy)
        {
            List<CustomersAccInfoTbl> all = new List<CustomersAccInfoTbl>();
            TrackerDb trackerDb = new TrackerDb();
            string strSQL = "SELECT CustomersAccInfoID, CustomerID, RequiresPurchOrder, CustomerVATNo, BillAddr1, BillAddr2, BillAddr3, BillAddr4,  BillAddr5, ShipAddr1, ShipAddr2, ShipAddr3, ShipAddr4, ShipAddr5, AccEmail, AltAccEmail,  PaymentTermID, Limit, FullCoName, AccFirstName, AccLastName, AltAccFirstName, AltAccLastName, PriceLevelID,  InvoiceTypeID, RegNo, BankAccNo, BankBranch, Enabled, Notes FROM CustomersAccInfoTbl";
            if (!string.IsNullOrEmpty(SortBy))
                strSQL = $"{strSQL} ORDER BY {SortBy}";
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(strSQL);
            if (dataReader != null)
            {
                while (dataReader.Read())
                    all.Add(new CustomersAccInfoTbl()
                    {
                        CustomersAccInfoID = dataReader["CustomersAccInfoID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["CustomersAccInfoID"]),
                        CustomerID = dataReader["CustomerID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["CustomerID"]),
                        RequiresPurchOrder = dataReader["RequiresPurchOrder"] != DBNull.Value && Convert.ToBoolean(dataReader["RequiresPurchOrder"]),
                        CustomerVATNo = dataReader["CustomerVATNo"] == DBNull.Value ? string.Empty : dataReader["CustomerVATNo"].ToString(),
                        BillAddr1 = dataReader["BillAddr1"] == DBNull.Value ? string.Empty : dataReader["BillAddr1"].ToString(),
                        BillAddr2 = dataReader["BillAddr2"] == DBNull.Value ? string.Empty : dataReader["BillAddr2"].ToString(),
                        BillAddr3 = dataReader["BillAddr3"] == DBNull.Value ? string.Empty : dataReader["BillAddr3"].ToString(),
                        BillAddr4 = dataReader["BillAddr4"] == DBNull.Value ? string.Empty : dataReader["BillAddr4"].ToString(),
                        BillAddr5 = dataReader["BillAddr5"] == DBNull.Value ? string.Empty : dataReader["BillAddr5"].ToString(),
                        ShipAddr1 = dataReader["ShipAddr1"] == DBNull.Value ? string.Empty : dataReader["ShipAddr1"].ToString(),
                        ShipAddr2 = dataReader["ShipAddr2"] == DBNull.Value ? string.Empty : dataReader["ShipAddr2"].ToString(),
                        ShipAddr3 = dataReader["ShipAddr3"] == DBNull.Value ? string.Empty : dataReader["ShipAddr3"].ToString(),
                        ShipAddr4 = dataReader["ShipAddr4"] == DBNull.Value ? string.Empty : dataReader["ShipAddr4"].ToString(),
                        ShipAddr5 = dataReader["ShipAddr5"] == DBNull.Value ? string.Empty : dataReader["ShipAddr5"].ToString(),
                        AccEmail = dataReader["AccEmail"] == DBNull.Value ? string.Empty : dataReader["AccEmail"].ToString(),
                        AltAccEmail = dataReader["AltAccEmail"] == DBNull.Value ? string.Empty : dataReader["AltAccEmail"].ToString(),
                        PaymentTermID = dataReader["PaymentTermID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["PaymentTermID"]),
                        Limit = dataReader["Limit"] == DBNull.Value ? 0.0 : Convert.ToDouble(dataReader["Limit"]),
                        FullCoName = dataReader["FullCoName"] == DBNull.Value ? string.Empty : dataReader["FullCoName"].ToString(),
                        AccFirstName = dataReader["AccFirstName"] == DBNull.Value ? string.Empty : dataReader["AccFirstName"].ToString(),
                        AccLastName = dataReader["AccLastName"] == DBNull.Value ? string.Empty : dataReader["AccLastName"].ToString(),
                        AltAccFirstName = dataReader["AltAccFirstName"] == DBNull.Value ? string.Empty : dataReader["AltAccFirstName"].ToString(),
                        AltAccLastName = dataReader["AltAccLastName"] == DBNull.Value ? string.Empty : dataReader["AltAccLastName"].ToString(),
                        PriceLevelID = dataReader["PriceLevelID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["PriceLevelID"]),
                        InvoiceTypeID = dataReader["InvoiceTypeID"] == DBNull.Value ? 1 : Convert.ToInt32(dataReader["InvoiceTypeID"]),
                        RegNo = dataReader["RegNo"] == DBNull.Value ? string.Empty : dataReader["RegNo"].ToString(),
                        BankAccNo = dataReader["BankAccNo"] == DBNull.Value ? string.Empty : dataReader["BankAccNo"].ToString(),
                        BankBranch = dataReader["BankBranch"] == DBNull.Value ? string.Empty : dataReader["BankBranch"].ToString(),
                        Enabled = dataReader["Enabled"] == DBNull.Value || Convert.ToBoolean(dataReader["Enabled"]),
                        Notes = dataReader["Notes"] == DBNull.Value ? string.Empty : dataReader["Notes"].ToString()
                    });
                dataReader.Close();
            }
            trackerDb.Close();
            return all;
        }

        [DataObjectMethod(DataObjectMethodType.Select, true)]
        public CustomersAccInfoTbl GetByCustomerID(long pCustomerID)
        {
            CustomersAccInfoTbl byCustomerID = new CustomersAccInfoTbl();
            byCustomerID.CustomerID = pCustomerID;
            string strSQL = "SELECT CustomersAccInfoID, RequiresPurchOrder, CustomerVATNo, BillAddr1, BillAddr2, BillAddr3, BillAddr4,  BillAddr5, ShipAddr1, ShipAddr2, ShipAddr3, ShipAddr4, ShipAddr5, AccEmail, AltAccEmail,  PaymentTermID, Limit, FullCoName, AccFirstName, AccLastName, AltAccFirstName, AltAccLastName, PriceLevelID,  InvoiceTypeID, RegNo, BankAccNo, BankBranch, Enabled, Notes FROM CustomersAccInfoTbl WHERE (CustomerID = ?)";
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddWhereParams((object)pCustomerID, DbType.Int64, "@CustomerID");
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(strSQL);
            if (dataReader != null)
            {
                if (dataReader.Read())
                {
                    byCustomerID.CustomersAccInfoID = dataReader["CustomersAccInfoID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["CustomersAccInfoID"]);
                    byCustomerID.RequiresPurchOrder = dataReader["RequiresPurchOrder"] != DBNull.Value && Convert.ToBoolean(dataReader["RequiresPurchOrder"]);
                    byCustomerID.CustomerVATNo = dataReader["CustomerVATNo"] == DBNull.Value ? string.Empty : dataReader["CustomerVATNo"].ToString();
                    byCustomerID.BillAddr1 = dataReader["BillAddr1"] == DBNull.Value ? string.Empty : dataReader["BillAddr1"].ToString();
                    byCustomerID.BillAddr2 = dataReader["BillAddr2"] == DBNull.Value ? string.Empty : dataReader["BillAddr2"].ToString();
                    byCustomerID.BillAddr3 = dataReader["BillAddr3"] == DBNull.Value ? string.Empty : dataReader["BillAddr3"].ToString();
                    byCustomerID.BillAddr4 = dataReader["BillAddr4"] == DBNull.Value ? string.Empty : dataReader["BillAddr4"].ToString();
                    byCustomerID.BillAddr5 = dataReader["BillAddr5"] == DBNull.Value ? string.Empty : dataReader["BillAddr5"].ToString();
                    byCustomerID.ShipAddr1 = dataReader["ShipAddr1"] == DBNull.Value ? string.Empty : dataReader["ShipAddr1"].ToString();
                    byCustomerID.ShipAddr2 = dataReader["ShipAddr2"] == DBNull.Value ? string.Empty : dataReader["ShipAddr2"].ToString();
                    byCustomerID.ShipAddr3 = dataReader["ShipAddr3"] == DBNull.Value ? string.Empty : dataReader["ShipAddr3"].ToString();
                    byCustomerID.ShipAddr4 = dataReader["ShipAddr4"] == DBNull.Value ? string.Empty : dataReader["ShipAddr4"].ToString();
                    byCustomerID.ShipAddr5 = dataReader["ShipAddr5"] == DBNull.Value ? string.Empty : dataReader["ShipAddr5"].ToString();
                    byCustomerID.AccEmail = dataReader["AccEmail"] == DBNull.Value ? string.Empty : dataReader["AccEmail"].ToString();
                    byCustomerID.AltAccEmail = dataReader["AltAccEmail"] == DBNull.Value ? string.Empty : dataReader["AltAccEmail"].ToString();
                    byCustomerID.PaymentTermID = dataReader["PaymentTermID"] == DBNull.Value ? byCustomerID.PaymentTermID : Convert.ToInt32(dataReader["PaymentTermID"]);
                    byCustomerID.Limit = dataReader["Limit"] == DBNull.Value ? 0.0 : Convert.ToDouble(dataReader["Limit"]);
                    byCustomerID.FullCoName = dataReader["FullCoName"] == DBNull.Value ? string.Empty : dataReader["FullCoName"].ToString();
                    byCustomerID.AccFirstName = dataReader["AccFirstName"] == DBNull.Value ? string.Empty : dataReader["AccFirstName"].ToString();
                    byCustomerID.AccLastName = dataReader["AccLastName"] == DBNull.Value ? string.Empty : dataReader["AccLastName"].ToString();
                    byCustomerID.AltAccFirstName = dataReader["AltAccFirstName"] == DBNull.Value ? string.Empty : dataReader["AltAccFirstName"].ToString();
                    byCustomerID.AltAccLastName = dataReader["AltAccLastName"] == DBNull.Value ? string.Empty : dataReader["AltAccLastName"].ToString();
                    byCustomerID.PriceLevelID = dataReader["PriceLevelID"] == DBNull.Value ? byCustomerID.PriceLevelID : Convert.ToInt32(dataReader["PriceLevelID"]);
                    byCustomerID.InvoiceTypeID = dataReader["InvoiceTypeID"] == DBNull.Value ? byCustomerID.InvoiceTypeID : Convert.ToInt32(dataReader["InvoiceTypeID"]);
                    byCustomerID.RegNo = dataReader["RegNo"] == DBNull.Value ? string.Empty : dataReader["RegNo"].ToString();
                    byCustomerID.BankAccNo = dataReader["BankAccNo"] == DBNull.Value ? string.Empty : dataReader["BankAccNo"].ToString();
                    byCustomerID.BankBranch = dataReader["BankBranch"] == DBNull.Value ? string.Empty : dataReader["BankBranch"].ToString();
                    byCustomerID.Enabled = dataReader["Enabled"] == DBNull.Value || Convert.ToBoolean(dataReader["Enabled"]);
                    byCustomerID.Notes = dataReader["Notes"] == DBNull.Value ? string.Empty : dataReader["Notes"].ToString();
                }
                dataReader.Close();
            }
            trackerDb.Close();
            return byCustomerID;
        }

        public int GetByPaymentTypeIDByCustomerID(long pCustomerID)
        {
            int typeIdByCustomerID = 0;
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddWhereParams((object)pCustomerID, DbType.Int64, "@CustomerID");
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader("SELECT PaymentTermID FROM CustomersAccInfoTbl WHERE (CustomerID = ?)");
            if (dataReader != null)
            {
                if (dataReader.Read())
                    typeIdByCustomerID = dataReader["PaymentTermID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["PaymentTermID"]);
                dataReader.Close();
            }
            trackerDb.Close();
            return typeIdByCustomerID;
        }

        [DataObjectMethod(DataObjectMethodType.Insert, true)]
        public string Insert(CustomersAccInfoTbl pCustomersAccInfoTbl)
        {
            string empty = string.Empty;
            string str;
            if (pCustomersAccInfoTbl.CustomerID == 0L)
            {
                str = "error: customer number cannot be 0";
            }
            else
            {
                TrackerDb trackerDb = new TrackerDb();
                trackerDb.AddParams((object)pCustomersAccInfoTbl.CustomerID, DbType.Int64, "@CustomerID");
                trackerDb.AddParams((object)pCustomersAccInfoTbl.RequiresPurchOrder, DbType.Int32, "@RequiresPurchOrder");
                trackerDb.AddParams((object)pCustomersAccInfoTbl.CustomerVATNo, DbType.String, "@CustomerVATNo");
                trackerDb.AddParams((object)pCustomersAccInfoTbl.BillAddr1, DbType.String, "@BillAddr1");
                trackerDb.AddParams((object)pCustomersAccInfoTbl.BillAddr2, DbType.String, "@BillAddr2");
                trackerDb.AddParams((object)pCustomersAccInfoTbl.BillAddr3, DbType.String, "@BillAddr3");
                trackerDb.AddParams((object)pCustomersAccInfoTbl.BillAddr4, DbType.String, "@BillAddr4");
                trackerDb.AddParams((object)pCustomersAccInfoTbl.BillAddr5, DbType.String, "@BillAddr5");
                trackerDb.AddParams((object)pCustomersAccInfoTbl.ShipAddr1, DbType.String, "@ShipAddr1");
                trackerDb.AddParams((object)pCustomersAccInfoTbl.ShipAddr2, DbType.String, "@ShipAddr2");
                trackerDb.AddParams((object)pCustomersAccInfoTbl.ShipAddr3, DbType.String, "@ShipAddr3");
                trackerDb.AddParams((object)pCustomersAccInfoTbl.ShipAddr4, DbType.String, "@ShipAddr4");
                trackerDb.AddParams((object)pCustomersAccInfoTbl.ShipAddr5, DbType.String, "@ShipAddr5");
                trackerDb.AddParams((object)pCustomersAccInfoTbl.AccEmail, DbType.String, "@AccEmail");
                trackerDb.AddParams((object)pCustomersAccInfoTbl.AltAccEmail, DbType.String, "@AltAccEmail");
                trackerDb.AddParams((object)pCustomersAccInfoTbl.PaymentTermID, DbType.Int32, "@PaymentTermID");
                trackerDb.AddParams((object)pCustomersAccInfoTbl.Limit, DbType.Currency, "@Limit");
                trackerDb.AddParams((object)pCustomersAccInfoTbl.FullCoName, DbType.String, "@FullCoName");
                trackerDb.AddParams((object)pCustomersAccInfoTbl.AccFirstName, DbType.String, "@AccFirstName");
                trackerDb.AddParams((object)pCustomersAccInfoTbl.AccLastName, DbType.String, "@AccLastName");
                trackerDb.AddParams((object)pCustomersAccInfoTbl.AltAccFirstName, DbType.String, "@AltAccFirstName");
                trackerDb.AddParams((object)pCustomersAccInfoTbl.AltAccLastName, DbType.String, "@AltAccLastName");
                trackerDb.AddParams((object)pCustomersAccInfoTbl.PriceLevelID, DbType.Int32, "@PriceLevelID");
                trackerDb.AddParams((object)pCustomersAccInfoTbl.InvoiceTypeID, DbType.Int32, "@InvoiceTypeID");
                trackerDb.AddParams((object)pCustomersAccInfoTbl.RegNo, DbType.String, "@RegNo");
                trackerDb.AddParams((object)pCustomersAccInfoTbl.BankAccNo, DbType.String, "@BankAccNo");
                trackerDb.AddParams((object)pCustomersAccInfoTbl.BankBranch, DbType.String, "@BankBranch");
                trackerDb.AddParams((object)pCustomersAccInfoTbl.Enabled, DbType.Boolean, "@Enabled");
                trackerDb.AddParams((object)pCustomersAccInfoTbl.Notes, DbType.String, "@Notes");
                str = trackerDb.ExecuteNonQuerySQL("INSERT INTO CustomersAccInfoTbl (CustomerID, RequiresPurchOrder, CustomerVATNo, BillAddr1, BillAddr2, BillAddr3, BillAddr4, BillAddr5,  ShipAddr1, ShipAddr2, ShipAddr3, ShipAddr4, ShipAddr5, AccEmail, AltAccEmail, PaymentTermID,  Limit, FullCoName, AccFirstName, AccLastName, AltAccFirstName, AltAccLastName, PriceLevelID, InvoiceTypeID,  RegNo, BankAccNo, BankBranch, Enabled, Notes) VALUES ( ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)");
                trackerDb.Close();
            }
            return str;
        }

        [DataObjectMethod(DataObjectMethodType.Update, true)]
        public string Update(CustomersAccInfoTbl pCustomersAccInfoTbl)
        {
            return this.Update(pCustomersAccInfoTbl, 0L);
        }

        [DataObjectMethod(DataObjectMethodType.Update, false)]
        public string Update(CustomersAccInfoTbl pCustomersAccInfoTbl, long pOrignal_CustomersAccInfoID)
        {
            string empty = string.Empty;
            TrackerDb trackerDb = new TrackerDb();
            string str;
            if (pCustomersAccInfoTbl.CustomerID == 0L)
            {
                str = "error: CustomerID cannot be 0";
            }
            else
            {
                if (pOrignal_CustomersAccInfoID > 0L)
                    trackerDb.AddWhereParams((object)pOrignal_CustomersAccInfoID, DbType.Int64);
                else
                    trackerDb.AddWhereParams((object)pCustomersAccInfoTbl.CustomersAccInfoID, DbType.Int64, "@CustomersAccInfoID");
                trackerDb.AddParams((object)pCustomersAccInfoTbl.CustomerID, DbType.Int64, "@CustomerID");
                trackerDb.AddParams((object)pCustomersAccInfoTbl.RequiresPurchOrder, DbType.Int32, "@RequiresPurchOrder");
                trackerDb.AddParams((object)pCustomersAccInfoTbl.CustomerVATNo, DbType.String, "@CustomerVATNo");
                trackerDb.AddParams((object)pCustomersAccInfoTbl.BillAddr1, DbType.String, "@BillAddr1");
                trackerDb.AddParams((object)pCustomersAccInfoTbl.BillAddr2, DbType.String, "@BillAddr2");
                trackerDb.AddParams((object)pCustomersAccInfoTbl.BillAddr3, DbType.String, "@BillAddr3");
                trackerDb.AddParams((object)pCustomersAccInfoTbl.BillAddr4, DbType.String, "@BillAddr4");
                trackerDb.AddParams((object)pCustomersAccInfoTbl.BillAddr5, DbType.String, "@BillAddr5");
                trackerDb.AddParams((object)pCustomersAccInfoTbl.ShipAddr1, DbType.String, "@ShipAddr1");
                trackerDb.AddParams((object)pCustomersAccInfoTbl.ShipAddr2, DbType.String, "@ShipAddr2");
                trackerDb.AddParams((object)pCustomersAccInfoTbl.ShipAddr3, DbType.String, "@ShipAddr3");
                trackerDb.AddParams((object)pCustomersAccInfoTbl.ShipAddr4, DbType.String, "@ShipAddr4");
                trackerDb.AddParams((object)pCustomersAccInfoTbl.ShipAddr5, DbType.String, "@ShipAddr5");
                trackerDb.AddParams((object)pCustomersAccInfoTbl.AccEmail, DbType.String, "@AccEmail");
                trackerDb.AddParams((object)pCustomersAccInfoTbl.AltAccEmail, DbType.String, "@AltAccEmail");
                trackerDb.AddParams((object)pCustomersAccInfoTbl.PaymentTermID, DbType.Int32, "@PaymentTermID");
                trackerDb.AddParams((object)pCustomersAccInfoTbl.Limit, DbType.Double, "@Limit");
                trackerDb.AddParams((object)pCustomersAccInfoTbl.FullCoName, DbType.String, "@FullCoName");
                trackerDb.AddParams((object)pCustomersAccInfoTbl.AccFirstName, DbType.String, "@AccFirstName");
                trackerDb.AddParams((object)pCustomersAccInfoTbl.AccLastName, DbType.String, "@AccLastName");
                trackerDb.AddParams((object)pCustomersAccInfoTbl.AltAccFirstName, DbType.String, "@AltAccFirstName");
                trackerDb.AddParams((object)pCustomersAccInfoTbl.AltAccLastName, DbType.String, "@AltAccLastName");
                trackerDb.AddParams((object)pCustomersAccInfoTbl.PriceLevelID, DbType.Int32, "@PriceLevelID");
                trackerDb.AddParams((object)pCustomersAccInfoTbl.InvoiceTypeID, DbType.Int32, "@InvoiceTypeID");
                trackerDb.AddParams((object)pCustomersAccInfoTbl.RegNo, DbType.String, "@RegNo");
                trackerDb.AddParams((object)pCustomersAccInfoTbl.BankAccNo, DbType.String, "@BankAccNo");
                trackerDb.AddParams((object)pCustomersAccInfoTbl.BankBranch, DbType.String, "@BankBranch");
                trackerDb.AddParams((object)pCustomersAccInfoTbl.Enabled, DbType.Boolean, "@Enabled");
                trackerDb.AddParams((object)pCustomersAccInfoTbl.Notes, DbType.String, "@Notes");
                str = trackerDb.ExecuteNonQuerySQL("UPDATE CustomersAccInfoTbl SET CustomerID = ?, RequiresPurchOrder = ?, CustomerVATNo = ?, BillAddr1 = ?, BillAddr2 = ?, BillAddr3 = ?, BillAddr4 = ?, BillAddr5 = ?,  ShipAddr1 = ?, ShipAddr2 = ?, ShipAddr3 = ?, ShipAddr4 = ?, ShipAddr5 = ?, AccEmail = ?, AltAccEmail = ?, PaymentTermID = ?,  Limit = ?, FullCoName = ?, AccFirstName = ?, AccLastName = ?, AltAccFirstName = ?, AltAccLastName = ?, PriceLevelID = ?, InvoiceTypeID = ?,  RegNo = ?, BankAccNo = ?, BankBranch = ?, Enabled = ?, Notes = ? WHERE (CustomersAccInfoID = ?)");
                trackerDb.Close();
            }
            return str;
        }

        [DataObjectMethod(DataObjectMethodType.Delete, true)]
        public string Delete(CustomersAccInfoTbl pCustomersAccInfoTbl)
        {
            return this.Delete(pCustomersAccInfoTbl.CustomersAccInfoID);
        }

        [DataObjectMethod(DataObjectMethodType.Delete, false)]
        public string Delete(int pCustomersAccInfoID)
        {
            string empty = string.Empty;
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddWhereParams((object)pCustomersAccInfoID, DbType.Int32, "@CustomersAccInfoID");
            string str = trackerDb.ExecuteNonQuerySQL("DELETE FROM CustomersAccInfoTbl WHERE (CustomersAccInfoID = ?)");
            trackerDb.Close();
            return str;
        }

        public int GetCustomersInvoiceType(long pCustomerID)
        {
            int customersInvoiceType = 1;
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddWhereParams((object)pCustomerID, DbType.Int64);
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader("SELECT InvoiceTypeID FROM CustomersAccInfoTbl WHERE (CustomerID = ?)");
            if (dataReader != null)
            {
                if (dataReader.Read())
                    customersInvoiceType = dataReader["InvoiceTypeID"] == DBNull.Value ? 1 : Convert.ToInt32(dataReader["InvoiceTypeID"]);
                dataReader.Close();
            }
            trackerDb.Close();
            return customersInvoiceType;
        }
    }
}