// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.control.OrderCls
// Assembly: TrackerDotNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerDotNet.dll

using System.Collections.Generic;

//- only form later versions #nullable disable
namespace TrackerDotNet.Controls
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