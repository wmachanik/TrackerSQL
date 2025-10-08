// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.control.OrderTblData
// Assembly: TrackerDotNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerDotNet.dll

using System;

//- only form later versions #nullable disable
namespace TrackerDotNet.Controls
{
    public class OrderTblData
    {
        private int _OrderID;
        private long _CustomerID;
        private DateTime _OrderDate;
        private DateTime _RoastDate;
        private int _ToBeDeliveredBy;
        private bool _Confirmed;
        private bool _Done;
        private bool _Packed;
        private bool _InvoiceDone;
        private string _PurchaseOrder;
        private string _Notes;
        private int _ItemTypeID;
        private double _QuantityOrdered;
        private DateTime _RequiredByDate;
        private int _PrepTypeID;
        private int _PackagingID;

        public OrderTblData()
        {
            this._OrderID = 0;
            this._CustomerID = 0;
            this._OrderDate = DateTime.MinValue;
            this._RoastDate = DateTime.MinValue;
            this._ToBeDeliveredBy = 0;
            this._Confirmed = false;
            this._Done = this._Packed = this._InvoiceDone = false;
            this._PurchaseOrder = this._Notes = string.Empty;
            this._ItemTypeID = 0;
            this._QuantityOrdered = 0.0;
            this._RequiredByDate = DateTime.MinValue;
            this._PrepTypeID = 0;
            this._PackagingID = 0;
        }

        public int OrderID
        {
            get => this._OrderID;
            set => this._OrderID = value;
        }

        public long CustomerID
        {
            get => this._CustomerID;
            set => this._CustomerID = value;
        }

        public DateTime OrderDate
        {
            get => this._OrderDate;
            set => this._OrderDate = value;
        }

        public DateTime RoastDate
        {
            get => this._RoastDate;
            set => this._RoastDate = value;
        }

        public int ToBeDeliveredBy
        {
            get => this._ToBeDeliveredBy;
            set => this._ToBeDeliveredBy = value;
        }

        public DateTime RequiredByDate
        {
            get => this._RequiredByDate;
            set => this._RequiredByDate = value;
        }

        public bool Confirmed
        {
            get => this._Confirmed;
            set => this._Confirmed = value;
        }

        public bool Done
        {
            get => this._Done;
            set => this._Done = value;
        }

        public bool Packed
        {
            get => this._Packed;
            set => this._Packed = value;
        }

        public bool InvoiceDone
        {
            get => this._InvoiceDone;
            set => this._InvoiceDone = value;
        }

        public string PurchaseOrder
        {
            get => this._PurchaseOrder;
            set => this._PurchaseOrder = value;
        }

        public string Notes
        {
            get => this._Notes;
            set => this._Notes = value;
        }

        public int ItemTypeID
        {
            get => this._ItemTypeID;
            set => this._ItemTypeID = value;
        }

        public double QuantityOrdered
        {
            get => this._QuantityOrdered;
            set => this._QuantityOrdered = value;
        }

        public int PrepTypeID
        {
            get => this._PrepTypeID;
            set => this._PrepTypeID = value;
        }

        public int PackagingID
        {
            get => this._PackagingID;
            set => this._PackagingID = value;
        }
    }
}
