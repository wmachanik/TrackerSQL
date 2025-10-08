// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.control.OrderHeaderData
// Assembly: TrackerDotNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerDotNet.dll

using System;
using TrackerDotNet.Classes;

//- only form later versions #nullable disable
namespace TrackerDotNet.Controls
{
    public class OrderHeaderData
    {
        public const string CONST_BOUNDCustomerID = "BoundCustomerID";
        public const string CONST_BOUNDDELIVERYDATE = "BoundDeliveryDate";
        public const string CONST_BOUNDOLDDELIVERYDATE = "BoundOldDeliveryDate";
        public const string CONST_BOUNDNOTES = "BoundNotes";
        private long _otCustomerID;
        private int _otToBeDeliveredBy;
        private DateTime _otOrderDate;
        private DateTime _otRoastDate;
        private DateTime _otRequiredByDate;
        private bool _otConfirmed;
        private bool _otDone;
        private bool _otInvoiceDone;
        private string _otPurchaseOrder;
        private string _otNotes;

        public OrderHeaderData()
        {
            this._otCustomerID = this._otToBeDeliveredBy = 0;
            this._otPurchaseOrder = this._otNotes = string.Empty;
            this._otOrderDate = this._otRoastDate = this._otRequiredByDate = TimeZoneUtils.Now().Date;
            this._otConfirmed = true;
            this._otInvoiceDone = this._otDone = false;
        }

        public long CustomerID
        {
            get => this._otCustomerID;
            set => this._otCustomerID = value;
        }

        public int ToBeDeliveredBy
        {
            get => this._otToBeDeliveredBy;
            set => this._otToBeDeliveredBy = value;
        }

        public DateTime OrderDate
        {
            get => this._otOrderDate;
            set => this._otOrderDate = value;
        }

        public DateTime RoastDate
        {
            get => this._otRoastDate;
            set => this._otRoastDate = value;
        }

        public DateTime RequiredByDate
        {
            get => this._otRequiredByDate;
            set => this._otRequiredByDate = value;
        }

        public bool Confirmed
        {
            get => this._otConfirmed;
            set => this._otConfirmed = value;
        }

        public bool Done
        {
            get => this._otDone;
            set => this._otDone = value;
        }

        public bool InvoiceDone
        {
            get => this._otInvoiceDone;
            set => this._otInvoiceDone = value;
        }

        public string PurchaseOrder
        {
            get => this._otPurchaseOrder;
            set => this._otPurchaseOrder = value;
        }

        public string Notes
        {
            get => this._otNotes;
            set => this._otNotes = value;
        }
    }
}