// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.control.OrderDetailData
// Assembly: TrackerDotNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerDotNet.dll

//- only form later versions #nullable disable
namespace TrackerDotNet.Controls
{
    public class OrderDetailData
    {
        private int _otItemTypeID;
        private int _otOrderID;
        private int _otPackagingID;
        private double _otQuantityOrdered;

        public OrderDetailData()
        {
            this._otItemTypeID = 0;
            this._otOrderID = 0;
            this._otPackagingID = 0;
            this._otQuantityOrdered = 0.0;
        }

        public int ItemTypeID
        {
            get => this._otItemTypeID;
            set => this._otItemTypeID = value;
        }

        public int PackagingID
        {
            get => this._otPackagingID;
            set => this._otPackagingID = value;
        }

        public int OrderID
        {
            get => this._otOrderID;
            set => this._otOrderID = value;
        }

        public double QuantityOrdered
        {
            get => this._otQuantityOrdered;
            set => this._otQuantityOrdered = value;
        }
    }
}