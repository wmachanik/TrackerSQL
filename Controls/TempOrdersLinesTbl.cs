// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.control.TempOrdersLinesTbl
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
    public class TempOrdersLinesTbl
    {
        private const string CONST_SQL_SELECT = "SELECT TOLineID, TOHeaderID, ItemID, ServiceTypeID, Qty, PackagingID, OriginalOrderID FROM TempOrdersLinesTbl";
        private const string CONST_SQL_INSERT = "INSERT INTO TempOrdersLinesTbl (TOHeaderID, ItemID, ServiceTypeID, Qty, PackagingID, OriginalOrderID) VALUES (?, ?, ?, ?, ?, ?)";
        private const string CONST_SQL_DELETEALL = "DELETE * FROM TempOrdersLinesTbl";
        private const string CONST_SQL_DELETEBYID = "DELETE FROM TempOrdersLinesTbl WHERE (OriginalOrderID = ?)";
        private int _TOLineID;
        private int _TOHeaderID;
        private int _ItemID;
        private int _ServiceTypeID;
        private double _Qty;
        private int _PackagingID;
        private int _OriginalOrderID;

        public TempOrdersLinesTbl()
        {
            this._TOLineID = 0;
            this._TOHeaderID = 0;
            this._ItemID = 0;
            this._ServiceTypeID = 0;
            this._Qty = 0.0;
            this._PackagingID = 0;
            this._OriginalOrderID = 0;
        }

        public int TOLineID
        {
            get => this._TOLineID;
            set => this._TOLineID = value;
        }

        public int TOHeaderID
        {
            get => this._TOHeaderID;
            set => this._TOHeaderID = value;
        }

        public int ItemID
        {
            get => this._ItemID;
            set => this._ItemID = value;
        }

        public int ServiceTypeID
        {
            get => this._ServiceTypeID;
            set => this._ServiceTypeID = value;
        }

        public double Qty
        {
            get => this._Qty;
            set => this._Qty = value;
        }

        public int PackagingID
        {
            get => this._PackagingID;
            set => this._PackagingID = value;
        }

        public int OriginalOrderID
        {
            get => this._OriginalOrderID;
            set => this._OriginalOrderID = value;
        }

        public List<TempOrdersLinesTbl> GetAll(string SortBy)
        {
            List<TempOrdersLinesTbl> all = new List<TempOrdersLinesTbl>();
            string strSQL = "SELECT TOLineID, TOHeaderID, ItemID, ServiceTypeID, Qty, PackagingID, OriginalOrderID FROM TempOrdersLinesTbl";
            if (!string.IsNullOrEmpty(SortBy))
                strSQL = $"{strSQL} ORDER BY {SortBy}";
            TrackerDb trackerDb = new TrackerDb();
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(strSQL);
            if (dataReader != null)
            {
                while (dataReader.Read())
                    all.Add(new TempOrdersLinesTbl()
                    {
                        TOLineID = dataReader["TOLineID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["TOLineID"]),
                        TOHeaderID = dataReader["TOHeaderID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["TOHeaderID"]),
                        ItemID = dataReader["ItemID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ItemID"]),
                        ServiceTypeID = dataReader["ServiceTypeID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ServiceTypeID"]),
                        Qty = dataReader["Qty"] == DBNull.Value ? 0.0 : Convert.ToDouble(dataReader["Qty"]),
                        PackagingID = dataReader["PackagingID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["PackagingID"]),
                        OriginalOrderID = dataReader["OriginalOrderID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["OriginalOrderID"])
                    });
                dataReader.Close();
            }
            trackerDb.Close();
            return all;
        }

        public bool Insert(TempOrdersLinesTbl pLineData)
        {
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddParams((object)pLineData.TOHeaderID, DbType.Int64, "@TOHeaderID");
            trackerDb.AddParams((object)pLineData.ItemID, DbType.Int32, "@ItemID");
            trackerDb.AddParams((object)pLineData.ServiceTypeID, DbType.Int32, "@ServiceTypeID");
            trackerDb.AddParams((object)pLineData.Qty, DbType.Double, "@Qty");
            trackerDb.AddParams((object)pLineData.PackagingID, DbType.Int32, "@PackagingID");
            trackerDb.AddParams((object)pLineData.OriginalOrderID, DbType.Int32, "@OriginalOrderID");
            bool flag = string.IsNullOrEmpty(trackerDb.ExecuteNonQuerySQL("INSERT INTO TempOrdersLinesTbl (TOHeaderID, ItemID, ServiceTypeID, Qty, PackagingID, OriginalOrderID) VALUES (?, ?, ?, ?, ?, ?)"));
            trackerDb.Close();
            return flag;
        }

        public bool DeleteAllRecords()
        {
            TrackerDb trackerDb = new TrackerDb();
            bool flag = string.IsNullOrEmpty(trackerDb.ExecuteNonQuerySQL("DELETE * FROM TempOrdersLinesTbl"));
            trackerDb.Close();
            return flag;
        }

        public bool DeleteByOriginalID(long pOriginalId)
        {
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddWhereParams((object)pOriginalId, DbType.Int64);
            bool flag = string.IsNullOrEmpty(trackerDb.ExecuteNonQuerySQL("DELETE FROM TempOrdersLinesTbl WHERE (OriginalOrderID = ?)"));
            trackerDb.Close();
            return flag;
        }
    }
}