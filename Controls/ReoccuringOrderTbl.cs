// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.control.ReoccuringOrderTbl
// Assembly: TrackerDotNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerDotNet.dll

using System;
using TrackerDotNet.Classes;

//- only form later versions #nullable disable
namespace TrackerDotNet.Controls
{
    public class ReoccuringOrderTbl
    {
        private int _ReoccuringOrderID;
        private long _CustomerID;
        private int _ReoccuranceTypeID;
        private int _ReoccuranceValue;
        private int _ItemRequiredID;
        private double _QtyRequired;
        private DateTime _DateLastDone;
        private DateTime _NextDateRequired;
        private DateTime _RequireUntilDate;
        private int _PackagingID;
        private bool _Enabled;
        private string _Notes;

        public ReoccuringOrderTbl()
        {
            this._ReoccuringOrderID = 0;
            this._CustomerID = 0;
            this._ReoccuranceTypeID = 0;
            this._ReoccuranceValue = 0;
            this._ItemRequiredID = 0;
            this._QtyRequired = 0.0;
            this._DateLastDone = SystemConstants.DatabaseConstants.SystemMinDate;
            this._NextDateRequired = TimeZoneUtils.Now().Date;
            this._RequireUntilDate = SystemConstants.DatabaseConstants.SystemMinDate;
            this._PackagingID = 0;
            this._Enabled = false;
            this._Notes = string.Empty;
        }

        public int ReoccuringOrderID
        {
            get => this._ReoccuringOrderID;
            set => this._ReoccuringOrderID = value;
        }

        public long CustomerID
        {
            get => this._CustomerID;
            set => this._CustomerID = value;
        }

        public int ReoccuranceTypeID
        {
            get => this._ReoccuranceTypeID;
            set => this._ReoccuranceTypeID = value;
        }

        public int ReoccuranceValue
        {
            get => this._ReoccuranceValue;
            set => this._ReoccuranceValue = value;
        }

        public int ItemRequiredID
        {
            get => this._ItemRequiredID;
            set => this._ItemRequiredID = value;
        }

        public double QtyRequired
        {
            get => this._QtyRequired;
            set => this._QtyRequired = value;
        }

        public DateTime DateLastDone
        {
            get => this._DateLastDone;
            set => this._DateLastDone = value;
        }

        public DateTime NextDateRequired
        {
            get => this._NextDateRequired;
            set => this._NextDateRequired = value;
        }

        public DateTime RequireUntilDate
        {
            get => this._RequireUntilDate;
            set => this._RequireUntilDate = value;
        }

        public int PackagingID
        {
            get => this._PackagingID;
            set => this._PackagingID = value;
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
    }
}