// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.control.DeliveryItemsTbl
// Assembly: TrackerDotNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerDotNet.dll

using System;
using TrackerDotNet.Classes;

//- only form later versions #nullable disable
namespace TrackerDotNet.Controls
{
    public class DeliveryItemsTbl
    {
        private int _otOrderID;
        private int _otCompanyId;
        private int _otItemTypeID;
        private int _itReplacementID;
        private int _cpdDeliveryOrder;
        private int _itSortOrder;
        private int _ptBGColour;
        private string _ctCompanyName;
        private string _itItemDesc;
        private string _itItemShortName;
        private string _otToBeDeliveredBy;
        private string _otNotes;
        private string _PackDesc;
        private string _ptAbreviation;
        private double _otQuantityOrdered;
        private DateTime _otOrderDate;
        private DateTime _otRoastDate;
        private DateTime _otRequiredDate;
        private bool _itItemEnabled;
        private bool _otConfirmed;
        private bool _otDone;

        public DeliveryItemsTbl()
        {
            this._otOrderID = this._otCompanyId = this._otItemTypeID = this._itReplacementID = 0;
            this._cpdDeliveryOrder = this._itSortOrder = this._ptBGColour = 0;
            this._ctCompanyName = this._itItemDesc = this._itItemShortName = this._otToBeDeliveredBy = this._otNotes = this._PackDesc = this._ptAbreviation = "";
            this._otQuantityOrdered = 0.0;
            this._otOrderDate = this._otRoastDate = this._otRequiredDate = TimeZoneUtils.Now().Date;
            this._itItemEnabled = this._otConfirmed = true;
            this._otDone = false;
        }

        public int otOrderID
        {
            get => this._otOrderID;
            set => this._otOrderID = value;
        }

        public int otCompanyId
        {
            get => this._otCompanyId;
            set => this._otCompanyId = value;
        }

        public int otItemTypeID
        {
            get => this._otItemTypeID;
            set => this._otItemTypeID = value;
        }

        public int itReplacementID
        {
            get => this._itReplacementID;
            set => this._itReplacementID = value;
        }

        public int ptBGColour
        {
            get => this._ptBGColour;
            set => this._ptBGColour = value;
        }

        public int cpdDeliveryOrder
        {
            get => this._cpdDeliveryOrder;
            set => this._cpdDeliveryOrder = value;
        }

        public int itSortOrder
        {
            get => this._itSortOrder;
            set => this._itSortOrder = value;
        }

        public string ctCompanyName
        {
            get => this._ctCompanyName;
            set => this._ctCompanyName = value;
        }

        public string itItemDesc
        {
            get => this._itItemDesc;
            set => this._itItemDesc = value;
        }

        public string itItemShortName
        {
            get => this._itItemShortName;
            set => this._itItemShortName = value;
        }

        public string otToBeDeliveredBy
        {
            get => this._otToBeDeliveredBy;
            set => this._otToBeDeliveredBy = value;
        }

        public string otNotes
        {
            get => this._otNotes;
            set => this._otNotes = value;
        }

        public string PackDesc
        {
            get => this._PackDesc;
            set => this._PackDesc = value;
        }

        public string ptAbreviation
        {
            get => this._ptAbreviation;
            set => this._ptAbreviation = value;
        }

        public double otQuantityOrdered
        {
            get => this._otQuantityOrdered;
            set => this._otQuantityOrdered = value;
        }

        public DateTime otOrderDate
        {
            get => this._otOrderDate;
            set => this._otOrderDate = value;
        }

        public DateTime otRoastDate
        {
            get => this._otRoastDate;
            set => this._otRoastDate = value;
        }

        public DateTime otRequiredDate
        {
            get => this._otRequiredDate;
            set => this._otRequiredDate = value;
        }

        public bool itItemEnabled
        {
            get => this._itItemEnabled;
            set => this._itItemEnabled = value;
        }

        public bool otConfirmed
        {
            get => this._otConfirmed;
            set => this._otConfirmed = value;
        }

        public bool otDone
        {
            get => this._otDone;
            set => this._otDone = value;
        }
    }
}