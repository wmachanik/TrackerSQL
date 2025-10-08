// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.control.CustomerData
// Assembly: TrackerDotNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerDotNet.dll

//- only form later versions #nullable disable
namespace TrackerDotNet.Controls
{
    public class CustomerData
    {
        private long _CustomerID;
        private string _CompanyName;
        private string _ContactTitle;
        private string _ContactFirstName;
        private string _ContactLastName;
        private string _ContactAltFirstName;
        private string _ContactAltLastName;
        private string _Department;
        private string _BillingAddress;
        private int _City;
        private string _StateOrProvince;
        private string _PostalCode;
        private string _Region;
        private string _PhoneNumber;
        private string _Extension;
        private string _FaxNumber;
        private string _CellNumber;
        private string _EmailAddress;
        private string _AltEmailAddress;
        private string _ContractNo;
        private int _CustomerTypeID;
        private int _EquipType;
        private int _CoffeePreference;
        private double _PriPrefQty;
        private int _PrefPrepTypeID;
        private int _PrefPackagingID;
        private int _SecondaryPreference;
        private double _SecPrefQty;
        private bool _TypicallySecToo;
        private int _PreferedAgent;
        private int _SalesAgentID;
        private string _MachineSN;
        private bool _UsesFilter;
        private bool _autofulfill;
        private bool _enabled;
        private bool _PredictionDisabled;
        private bool _AlwaysSendChkUp;
        private bool _NormallyResponds;
        private int _ReminderCount;
        private string _Notes;

        public CustomerData()
        {
            this._CustomerID = 0;
            this._CompanyName = this._ContactTitle = this._ContactFirstName = this._ContactLastName = this._ContactAltFirstName = this._ContactAltLastName = this._Department = this._BillingAddress = "";
            this._City = 0;
            this._StateOrProvince = this._PostalCode = this._Region = this._PhoneNumber = this._Extension = this._FaxNumber = this._CellNumber = this._EmailAddress = this._AltEmailAddress = this._ContractNo = "";
            this._CustomerTypeID = this._EquipType = this._CoffeePreference = 0;
            this._PriPrefQty = 0.0;
            this._PrefPrepTypeID = this._PrefPackagingID = this._SecondaryPreference = 0;
            this._SecPrefQty = 0.0;
            this._TypicallySecToo = false;
            this._PreferedAgent = this._SalesAgentID = 0;
            this._MachineSN = "";
            this._UsesFilter = this._autofulfill = this._enabled = this._PredictionDisabled = this._AlwaysSendChkUp = this._NormallyResponds = false;
            this._ReminderCount = 0;
            this._Notes = "";
        }

        public long CustomerID
        {
            get => this._CustomerID;
            set => this._CustomerID = value;
        }

        public string CompanyName
        {
            get => this._CompanyName;
            set => this._CompanyName = value;
        }

        public string ContactTitle
        {
            get => this._ContactTitle;
            set => this._ContactTitle = value;
        }

        public string ContactFirstName
        {
            get => this._ContactFirstName;
            set => this._ContactFirstName = value;
        }

        public string ContactLastName
        {
            get => this._ContactLastName;
            set => this._ContactLastName = value;
        }

        public string ContactAltFirstName
        {
            get => this._ContactAltFirstName;
            set => this._ContactAltFirstName = value;
        }

        public string ContactAltLastName
        {
            get => this._ContactAltLastName;
            set => this._ContactAltLastName = value;
        }

        public string Department
        {
            get => this._Department;
            set => this._Department = value;
        }

        public string BillingAddress
        {
            get => this._BillingAddress;
            set => this._BillingAddress = value;
        }

        public int City
        {
            get => this._City;
            set => this._City = value;
        }

        public string StateOrProvince
        {
            get => this._StateOrProvince;
            set => this._StateOrProvince = value;
        }

        public string PostalCode
        {
            get => this._PostalCode;
            set => this._PostalCode = value;
        }

        public string Region
        {
            get => this._Region;
            set => this._Region = value;
        }

        public string PhoneNumber
        {
            get => this._PhoneNumber;
            set => this._PhoneNumber = value;
        }

        public string Extension
        {
            get => this._Extension;
            set => this._Extension = value;
        }

        public string FaxNumber
        {
            get => this._FaxNumber;
            set => this._FaxNumber = value;
        }

        public string CellNumber
        {
            get => this._CellNumber;
            set => this._CellNumber = value;
        }

        public string EmailAddress
        {
            get => this._EmailAddress;
            set => this._EmailAddress = value;
        }

        public string AltEmailAddress
        {
            get => this._AltEmailAddress;
            set => this._AltEmailAddress = value;
        }

        public string ContractNo
        {
            get => this._ContractNo;
            set => this._ContractNo = value;
        }

        public int CustomerTypeID
        {
            get => this._CustomerTypeID;
            set => this._CustomerTypeID = value;
        }

        public int EquipType
        {
            get => this._EquipType;
            set => this._EquipType = value;
        }

        public int CoffeePreference
        {
            get => this._CoffeePreference;
            set => this._CoffeePreference = value;
        }

        public double PriPrefQty
        {
            get => this._PriPrefQty;
            set => this._PriPrefQty = value;
        }

        public int PrefPrepTypeID
        {
            get => this._PrefPrepTypeID;
            set => this._PrefPrepTypeID = value;
        }

        public int PrefPackagingID
        {
            get => this._PrefPackagingID;
            set => this._PrefPackagingID = value;
        }

        public int SecondaryPreference
        {
            get => this._SecondaryPreference;
            set => this._SecondaryPreference = value;
        }

        public double SecPrefQty
        {
            get => this._SecPrefQty;
            set => this._SecPrefQty = value;
        }

        public bool TypicallySecToo
        {
            get => this._TypicallySecToo;
            set => this._TypicallySecToo = value;
        }

        public int PreferedAgent
        {
            get => this._PreferedAgent;
            set => this._PreferedAgent = value;
        }

        public int SalesAgentID
        {
            get => this._SalesAgentID;
            set => this._SalesAgentID = value;
        }

        public string MachineSN
        {
            get => this._MachineSN;
            set => this._MachineSN = value;
        }

        public bool UsesFilter
        {
            get => this._UsesFilter;
            set => this._UsesFilter = value;
        }

        public bool autofulfill
        {
            get => this._autofulfill;
            set => this._autofulfill = value;
        }

        public bool enabled
        {
            get => this._enabled;
            set => this._enabled = value;
        }

        public bool PredictionDisabled
        {
            get => this._PredictionDisabled;
            set => this._PredictionDisabled = value;
        }

        public bool AlwaysSendChkUp
        {
            get => this._AlwaysSendChkUp;
            set => this._AlwaysSendChkUp = value;
        }

        public bool NormallyResponds
        {
            get => this._NormallyResponds;
            set => this._NormallyResponds = value;
        }

        public int ReminderCount
        {
            get => this._ReminderCount;
            set => this._ReminderCount = value;
        }

        public string Notes
        {
            get => this._Notes;
            set => this._Notes = value;
        }
    }
}