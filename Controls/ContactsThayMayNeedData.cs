//------------------------------------------------------------------------------
// TrackerSQL v3.x — ContactsThayMayNeedData
// Data access / control type: ContactsThayMayNeedData.
//------------------------------------------------------------------------------

//- only form later versions #nullable disable
using System;

namespace TrackerSQL.Controls
{
    [Obsolete("DO NOT USE Comtrols use Models - MIGRATION IN PROGRESS", true)]
    public class ContactsThayMayNeedData
    {
        private CustomersTbl _CustomerData;
        private bool _RequiresPurchOrder;
        private ClientUsageTbl _ClientUsageData;
        private NextPreperationDateByAreaTbl _NextPreperationDateByAreaData;

        public ContactsThayMayNeedData()
        {
            this._CustomerData = new CustomersTbl();
            this._RequiresPurchOrder = false;
            this._ClientUsageData = new ClientUsageTbl();
            this._NextPreperationDateByAreaData = new NextPreperationDateByAreaTbl();
        }

        public CustomersTbl CustomerData
        {
            get => this._CustomerData;
            set => this._CustomerData = value;
        }

        public bool RequiresPurchOrder
        {
            get => this._RequiresPurchOrder;
            set => this._RequiresPurchOrder = value;
        }

        public ClientUsageTbl ClientUsageData
        {
            get => this._ClientUsageData;
            set => this._ClientUsageData = value;
        }

        public NextPreperationDateByAreaTbl NextPreperationDateByAreaData
        {
            get => this._NextPreperationDateByAreaData;
            set => this._NextPreperationDateByAreaData = value;
        }
    }
}
