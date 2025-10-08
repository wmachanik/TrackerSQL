// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.control.ContactsThayMayNeedData
// Assembly: TrackerDotNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerDotNet.dll

//- only form later versions #nullable disable
namespace TrackerDotNet.Controls
{
    public class ContactsThayMayNeedData
    {
        private CustomersTbl _CustomerData;
        private bool _RequiresPurchOrder;
        private ClientUsageTbl _ClientUsageData;
        private NextRoastDateByCityTbl _NextRoastDateByCityData;

        public ContactsThayMayNeedData()
        {
            this._CustomerData = new CustomersTbl();
            this._RequiresPurchOrder = false;
            this._ClientUsageData = new ClientUsageTbl();
            this._NextRoastDateByCityData = new NextRoastDateByCityTbl();
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

        public NextRoastDateByCityTbl NextRoastDateByCityData
        {
            get => this._NextRoastDateByCityData;
            set => this._NextRoastDateByCityData = value;
        }
    }
}