// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.control.ClientUsageFromTempOrder
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
    public class ClientUsageFromTempOrder
    {
        public const string CONST_STR_SELECT = "SELECT TempOrdersHeaderTbl.CustomerID, TempOrdersLinesTbl.ItemID, TempOrdersLinesTbl.ServiceTypeID, TempOrdersLinesTbl.Qty,  ItemTypeTbl.UnitsPerQty, TempOrdersLinesTbl.PackagingID FROM  ((TempOrdersHeaderTbl INNER JOIN TempOrdersLinesTbl ON TempOrdersHeaderTbl.TOHeaderID = TempOrdersLinesTbl.TOHeaderID)  LEFT OUTER JOIN ItemTypeTbl ON TempOrdersLinesTbl.ItemID = ItemTypeTbl.ItemTypeID) WHERE TempOrdersHeaderTbl.CustomerID = ? AND TempOrdersLinesTbl.ServiceTypeID <> 17";
        private long _CustomerID;
        private int _ItemID;
        private int _ServiceTypeID;
        private double _Qty;
        private double _UnitsPerQty;
        private int _PackagingID;

        public ClientUsageFromTempOrder()
        {
            this._CustomerID = 0;
            this._ItemID = 0;
            this._ServiceTypeID = 0;
            this._Qty = 0.0;
            this._UnitsPerQty = 0.0;
            this._PackagingID = 0;
        }

        public long CustomerID
        {
            get => this._CustomerID;
            set => this._CustomerID = value;
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

        public double UnitsPerQty
        {
            get => this._UnitsPerQty;
            set => this._UnitsPerQty = value;
        }

        public int PackagingID
        {
            get => this._PackagingID;
            set => this._PackagingID = value;
        }

        public List<ClientUsageFromTempOrder> GetAll(long pCustomerID)
        {
            return this.GetAll(pCustomerID, "TempOrdersLinesTbl.ServiceTypeID");
        }

        public List<ClientUsageFromTempOrder> GetAll(long pCustomerID, string SortBy)
        {
            List<ClientUsageFromTempOrder> all = new List<ClientUsageFromTempOrder>();
            TrackerDb trackerDb = new TrackerDb();
            string strSQL = "SELECT TempOrdersHeaderTbl.CustomerID, TempOrdersLinesTbl.ItemID, TempOrdersLinesTbl.ServiceTypeID, TempOrdersLinesTbl.Qty,  ItemTypeTbl.UnitsPerQty, TempOrdersLinesTbl.PackagingID FROM  ((TempOrdersHeaderTbl INNER JOIN TempOrdersLinesTbl ON TempOrdersHeaderTbl.TOHeaderID = TempOrdersLinesTbl.TOHeaderID)  LEFT OUTER JOIN ItemTypeTbl ON TempOrdersLinesTbl.ItemID = ItemTypeTbl.ItemTypeID) WHERE TempOrdersHeaderTbl.CustomerID = ? AND TempOrdersLinesTbl.ServiceTypeID <> 17";
            if (!string.IsNullOrEmpty(SortBy))
                strSQL = $"{strSQL} ORDER BY {SortBy}";
            trackerDb.AddWhereParams((object)pCustomerID, DbType.Int64, "@CustomerID");
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(strSQL);
            if (dataReader != null)
            {
                while (dataReader.Read())
                    all.Add(new ClientUsageFromTempOrder()
                    {
                        CustomerID = dataReader["CustomerID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["CustomerID"]),
                        ItemID = dataReader["ItemID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ItemID"]),
                        ServiceTypeID = dataReader["ServiceTypeID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ServiceTypeID"]),
                        Qty = dataReader["Qty"] == DBNull.Value ? 0.0 : Convert.ToDouble(dataReader["Qty"]),
                        UnitsPerQty = dataReader["UnitsPerQty"] == DBNull.Value ? 1.0 : Convert.ToDouble(dataReader["UnitsPerQty"]),
                        PackagingID = dataReader["PackagingID"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["PackagingID"])
                    });
                dataReader.Close();
            }
            trackerDb.Close();
            return all;
        }
    }
}