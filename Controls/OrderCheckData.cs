// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.control.OrderCheckData
// Assembly: TrackerDotNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerDotNet.dll

using System;

//- only form later versions #nullable disable
namespace TrackerDotNet.Controls
{
    public class OrderCheckData
    {
        private int _OrderID;   // shoudl be long, but since we are using 32-bit access causes issues
        private long _CustomerID;
        private int _ItemTypeID;
        private DateTime _RequiredByDate;

        public OrderCheckData()
        {
            this._OrderID = 0;
            this._CustomerID = 0;
            this._ItemTypeID = 0;
            this._RequiredByDate = DateTime.MinValue;
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

        public int ItemTypeID
        {
            get => this._ItemTypeID;
            set => this._ItemTypeID = value;
        }

        public DateTime RequiredByDate
        {
            get => this._RequiredByDate;
            set => this._RequiredByDate = value;
        }
    }
}