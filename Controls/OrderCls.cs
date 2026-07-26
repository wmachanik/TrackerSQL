//------------------------------------------------------------------------------
// TrackerSQL v3.x — OrderCls
// Data access / control type: OrderCls.
//------------------------------------------------------------------------------

using System.Collections.Generic;

//- only form later versions #nullable disable
namespace TrackerSQL.Controls
{
    public class OrderCls
    {
        private OrderHeaderData _Header;
        private List<OrderDetailData> _Items;

        public OrderCls()
        {
            this._Header = new OrderHeaderData();
            List<OrderDetailData> orderDetailDataList = new List<OrderDetailData>();
        }

        public OrderHeaderData HeaderData
        {
            get => this._Header;
            set => this._Header = value;
        }

        public List<OrderDetailData> ItemsData
        {
            get => this._Items;
            set => this._Items = value;
        }
    }
}
