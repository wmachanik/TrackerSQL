//------------------------------------------------------------------------------
// TrackerSQL v3.x — OrderDetailData
// Data access / control type: OrderDetailData.
//------------------------------------------------------------------------------

//- only form later versions #nullable disable
namespace TrackerSQL.Controls
{
    public class OrderDetailData
    {
        private int _otItemTypeID;
        private int _otOrderLineID;
        private int _otOrderID;
        private int _otPackagingID;
        private double _otQuantityOrdered;

        public OrderDetailData()
        {
            this._otItemTypeID = 0;
            this._otOrderLineID = 0;
            this._otOrderID = 0;
            this._otPackagingID = 0;
            this._otQuantityOrdered = 0.0;
        }

        public int OrderLineID
        {
            get => this._otOrderLineID;
            set => this._otOrderLineID = value;
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
