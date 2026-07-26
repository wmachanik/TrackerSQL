//------------------------------------------------------------------------------
// TrackerSQL v3.x — OrderCheckData
// Data access / control type: OrderCheckData.
//------------------------------------------------------------------------------

using System;

//- only form later versions #nullable disable
namespace TrackerSQL.Controls
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