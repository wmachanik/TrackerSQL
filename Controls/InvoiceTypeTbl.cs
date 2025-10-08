// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.control.InvoiceTypeTbl
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
    public class InvoiceTypeTbl
    {
        public const int CONST_DEFAULT_INVOICETYPEID = 1;
        private const string CONST_SQL_SELECT = "SELECT InvoiceTypeID, InvoiceTypeDesc, Enabled, Notes FROM InvoiceTypeTbl";
        private const string CONST_SQL_INSERT = "INSERT INTO InvoiceTypeTbl (InvoiceTypeDesc, Enabled, Notes) VALUES ( ?, ?, ?)";
        private const string CONST_SQL_UPDATE = "UPDATE InvoiceTypeTbl SET InvoiceTypeDesc = ?, Enabled = ?, Notes = ? WHERE (InvoiceTypeID = ?)";
        private const string CONST_SQL_DELETE = "DELETE FROM InvoiceTypeTbl WHERE (InvoiceTypeID = ?)";
        private int _InvoiceTypeID;
        private string _InvoiceTypeDesc;
        private bool _Enabled;
        private string _Notes;

        public InvoiceTypeTbl()
        {
            this._InvoiceTypeID = 0;
            this._InvoiceTypeDesc = string.Empty;
            this._Enabled = false;
            this._Notes = string.Empty;
        }

        public int InvoiceTypeID
        {
            get => this._InvoiceTypeID;
            set => this._InvoiceTypeID = value;
        }

        public string InvoiceTypeDesc
        {
            get => this._InvoiceTypeDesc;
            set => this._InvoiceTypeDesc = value;
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
        public List<InvoiceTypeTbl> GetAll(string SortBy)
        {
            List<InvoiceTypeTbl> all = new List<InvoiceTypeTbl>();
            TrackerDb trackerDb = new TrackerDb();
            string strSQL = "SELECT InvoiceTypeID, InvoiceTypeDesc, Enabled, Notes FROM InvoiceTypeTbl";
            if (!string.IsNullOrEmpty(SortBy))
                strSQL = $"{strSQL} ORDER BY {SortBy}";
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(strSQL);
            if (dataReader != null)
            {
                while (dataReader.Read())
                    all.Add(new InvoiceTypeTbl()
                    {
                        InvoiceTypeID = dataReader["InvoiceTypeID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["InvoiceTypeID"]),
                        InvoiceTypeDesc = dataReader["InvoiceTypeDesc"] == DBNull.Value ? string.Empty : dataReader["InvoiceTypeDesc"].ToString(),
                        Enabled = dataReader["Enabled"] != DBNull.Value && Convert.ToBoolean(dataReader["Enabled"]),
                        Notes = dataReader["Notes"] == DBNull.Value ? string.Empty : dataReader["Notes"].ToString()
                    });
                dataReader.Close();
            }
            trackerDb.Close();
            return all;
        }

        [DataObjectMethod(DataObjectMethodType.Insert, true)]
        public string Insert(InvoiceTypeTbl pInvoiceTypeTbl)
        {
            string empty = string.Empty;
            string str;
            if (pInvoiceTypeTbl.InvoiceTypeID > 0)
            {
                str = this.Update(pInvoiceTypeTbl, 0);
            }
            else
            {
                TrackerDb trackerDb = new TrackerDb();
                trackerDb.AddParams((object)pInvoiceTypeTbl.InvoiceTypeDesc, DbType.String, "@InvoiceTypeDesc");
                trackerDb.AddParams((object)pInvoiceTypeTbl.Enabled, DbType.Boolean, "@Enabled");
                trackerDb.AddParams((object)pInvoiceTypeTbl.Notes, DbType.String, "@Notes");
                str = trackerDb.ExecuteNonQuerySQL("INSERT INTO InvoiceTypeTbl (InvoiceTypeDesc, Enabled, Notes) VALUES ( ?, ?, ?)");
                trackerDb.Close();
            }
            return str;
        }

        [DataObjectMethod(DataObjectMethodType.Update, true)]
        public string Update(InvoiceTypeTbl pInvoiceTypeTbl) => this.Update(pInvoiceTypeTbl, 0);

        [DataObjectMethod(DataObjectMethodType.Update, false)]
        public string Update(InvoiceTypeTbl pInvoiceTypeTbl, int pOrignal_InvoiceTypeID)
        {
            string empty = string.Empty;
            TrackerDb trackerDb = new TrackerDb();
            if (pOrignal_InvoiceTypeID > 0)
                trackerDb.AddWhereParams((object)pOrignal_InvoiceTypeID, DbType.Int32);
            else
                trackerDb.AddWhereParams((object)pInvoiceTypeTbl.InvoiceTypeID, DbType.Boolean, "@InvoiceTypeID");
            trackerDb.AddParams((object)pInvoiceTypeTbl.InvoiceTypeDesc, DbType.String, "@InvoiceTypeDesc");
            trackerDb.AddParams((object)pInvoiceTypeTbl.Enabled, DbType.Boolean, "@Enabled");
            trackerDb.AddParams((object)pInvoiceTypeTbl.Notes, DbType.String, "@Notes");
            string str = trackerDb.ExecuteNonQuerySQL("UPDATE InvoiceTypeTbl SET InvoiceTypeDesc = ?, Enabled = ?, Notes = ? WHERE (InvoiceTypeID = ?)");
            trackerDb.Close();
            return str;
        }

        [DataObjectMethod(DataObjectMethodType.Delete, true)]
        public string Delete(InvoiceTypeTbl pInvoiceType) => this.Delete(pInvoiceType.InvoiceTypeID);

        [DataObjectMethod(DataObjectMethodType.Delete, false)]
        public string Delete(int pInvoiceTypeID)
        {
            string empty = string.Empty;
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddWhereParams((object)pInvoiceTypeID, DbType.Int32, "@InvoiceTypeID");
            string str = trackerDb.ExecuteNonQuerySQL("DELETE FROM InvoiceTypeTbl WHERE (InvoiceTypeID = ?)");
            trackerDb.Close();
            return str;
        }
    }
}