// Decompiled with JetBrains decompiler
// Type: TrackerSQL.control.ContactsThayMayNeedData
// Assembly: TrackerSQL, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerSQL.dll

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
