// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.control.PaymentTermsTbl
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
    public class PaymentTermsTbl
    {
        public const int CONST_DEFAULT_PAYMENTTERMID = 1;
        private const string CONST_SQL_SELECT = "SELECT PaymentTermID, PaymentTermDesc, PaymentDays, DayOfMonth, UseDays, Enabled, Notes FROM PaymentTermsTbl";
        private const string CONST_SQL_SELECTBYDESC = "SELECT PaymentTermID FROM PaymentTermsTbl WHERE PaymentTermDesc = ?";
        private const string CONST_SQL_INSERT = "INSERT INTO PaymentTermsTbl (PaymentTermDesc, PaymentDays, DayOfMonth, UseDays, Enabled, Notes) VALUES ( ?, ?, ?, ?, ?, ?)";
        private const string CONST_SQL_UPDATE = "UPDATE PaymentTermsTbl SET PaymentTermDesc = ?, PaymentDays = ?, DayOfMonth = ?, UseDays = ?, Enabled = ?, Notes = ? WHERE (PaymentTermID = ?)";
        private const string CONST_SQL_DELETE = "DELETE FROM PaymentTermsTbl WHERE (PaymentTermID = ?)";
        private int _PaymentTermID;
        private string _PaymentTermDesc;
        private int _PaymentDays;
        private int _DayOfMonth;
        private bool _UseDays;
        private bool _Enabled;
        private string _Notes;

        public PaymentTermsTbl()
        {
            this._PaymentTermID = 0;
            this._PaymentTermDesc = string.Empty;
            this._PaymentDays = 0;
            this._DayOfMonth = 0;
            this._UseDays = false;
            this._Enabled = false;
            this._Notes = string.Empty;
        }

        public int PaymentTermID
        {
            get => this._PaymentTermID;
            set => this._PaymentTermID = value;
        }

        public string PaymentTermDesc
        {
            get => this._PaymentTermDesc;
            set => this._PaymentTermDesc = value;
        }

        public int PaymentDays
        {
            get => this._PaymentDays;
            set => this._PaymentDays = value;
        }

        public int DayOfMonth
        {
            get => this._DayOfMonth;
            set => this._DayOfMonth = value;
        }

        public bool UseDays
        {
            get => this._UseDays;
            set => this._UseDays = value;
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
        public List<PaymentTermsTbl> GetAll(string SortBy)
        {
            List<PaymentTermsTbl> all = new List<PaymentTermsTbl>();
            TrackerDb trackerDb = new TrackerDb();
            string strSQL = "SELECT PaymentTermID, PaymentTermDesc, PaymentDays, DayOfMonth, UseDays, Enabled, Notes FROM PaymentTermsTbl";
            if (!string.IsNullOrEmpty(SortBy))
                strSQL = $"{strSQL} ORDER BY {SortBy}";
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(strSQL);
            if (dataReader != null)
            {
                while (dataReader.Read())
                    all.Add(new PaymentTermsTbl()
                    {
                        PaymentTermID = dataReader["PaymentTermID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["PaymentTermID"]),
                        PaymentTermDesc = dataReader["PaymentTermDesc"] == DBNull.Value ? string.Empty : dataReader["PaymentTermDesc"].ToString(),
                        PaymentDays = dataReader["PaymentDays"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["PaymentDays"]),
                        DayOfMonth = dataReader["DayOfMonth"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["DayOfMonth"]),
                        UseDays = dataReader["UseDays"] != DBNull.Value && Convert.ToBoolean(dataReader["UseDays"]),
                        Enabled = dataReader["Enabled"] != DBNull.Value && Convert.ToBoolean(dataReader["Enabled"]),
                        Notes = dataReader["Notes"] == DBNull.Value ? string.Empty : dataReader["Notes"].ToString()
                    });
                dataReader.Close();
            }
            trackerDb.Close();
            return all;
        }

        [DataObjectMethod(DataObjectMethodType.Select, false)]
        public int GetPaymentTermIDByDesc(string pPaymentTermDesc)
        {
            int paymentTermIdByDesc = 0;
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddWhereParams((object)pPaymentTermDesc, DbType.String, "@PaymentTermDesc");
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader("SELECT PaymentTermID FROM PaymentTermsTbl WHERE PaymentTermDesc = ?");
            if (dataReader != null)
            {
                if (dataReader.Read())
                    paymentTermIdByDesc = dataReader["PaymentTermID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["PaymentTermID"]);
                dataReader.Close();
            }
            trackerDb.Close();
            return paymentTermIdByDesc;
        }

        [DataObjectMethod(DataObjectMethodType.Insert, true)]
        public string Insert(PaymentTermsTbl pPaymentTermsTbl)
        {
            string empty = string.Empty;
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddParams((object)pPaymentTermsTbl.PaymentTermDesc, DbType.String, "@PaymentTermDesc");
            trackerDb.AddParams((object)pPaymentTermsTbl.PaymentDays, DbType.Int32, "@PaymentDays");
            trackerDb.AddParams((object)pPaymentTermsTbl.DayOfMonth, DbType.Int32, "@DayOfMonth");
            trackerDb.AddParams((object)pPaymentTermsTbl.UseDays, DbType.Boolean, "@UseDays");
            trackerDb.AddParams((object)pPaymentTermsTbl.Enabled, DbType.Boolean, "@Enabled");
            trackerDb.AddParams((object)pPaymentTermsTbl.Notes, DbType.String, "@Notes");
            string str = trackerDb.ExecuteNonQuerySQL("INSERT INTO PaymentTermsTbl (PaymentTermDesc, PaymentDays, DayOfMonth, UseDays, Enabled, Notes) VALUES ( ?, ?, ?, ?, ?, ?)");
            trackerDb.Close();
            return str;
        }

        [DataObjectMethod(DataObjectMethodType.Update, true)]
        public string Update(PaymentTermsTbl pPaymentTermsTbl) => this.Update(pPaymentTermsTbl, 0);

        [DataObjectMethod(DataObjectMethodType.Update, false)]
        public string Update(PaymentTermsTbl pPaymentTermsTbl, int pOrignal_PaymentTermID)
        {
            string empty = string.Empty;
            TrackerDb trackerDb = new TrackerDb();
            if (pOrignal_PaymentTermID > 0)
                trackerDb.AddWhereParams((object)pOrignal_PaymentTermID, DbType.Int32);
            else
                trackerDb.AddWhereParams((object)pPaymentTermsTbl.PaymentTermID, DbType.Boolean, "@PaymentTermID");
            trackerDb.AddParams((object)pPaymentTermsTbl.PaymentTermDesc, DbType.String, "@PaymentTermDesc");
            trackerDb.AddParams((object)pPaymentTermsTbl.PaymentDays, DbType.Int32, "@PaymentDays");
            trackerDb.AddParams((object)pPaymentTermsTbl.DayOfMonth, DbType.Int32, "@DayOfMonth");
            trackerDb.AddParams((object)pPaymentTermsTbl.UseDays, DbType.Boolean, "@UseDays");
            trackerDb.AddParams((object)pPaymentTermsTbl.Enabled, DbType.Boolean, "@Enabled");
            trackerDb.AddParams((object)pPaymentTermsTbl.Notes, DbType.String, "@Notes");
            string str = trackerDb.ExecuteNonQuerySQL("UPDATE PaymentTermsTbl SET PaymentTermDesc = ?, PaymentDays = ?, DayOfMonth = ?, UseDays = ?, Enabled = ?, Notes = ? WHERE (PaymentTermID = ?)");
            trackerDb.Close();
            return str;
        }

        [DataObjectMethod(DataObjectMethodType.Delete, true)]
        public string Delete(PaymentTermsTbl pPaymentTermsTbl)
        {
            return this.Delete(pPaymentTermsTbl.PaymentTermID);
        }

        [DataObjectMethod(DataObjectMethodType.Delete, false)]
        public string Delete(int pPaymentTermID)
        {
            string empty = string.Empty;
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddWhereParams((object)pPaymentTermID, DbType.Int32, "@PaymentTermID");
            string str = trackerDb.ExecuteNonQuerySQL("DELETE FROM PaymentTermsTbl WHERE (PaymentTermID = ?)");
            trackerDb.Close();
            return str;
        }
    }
}