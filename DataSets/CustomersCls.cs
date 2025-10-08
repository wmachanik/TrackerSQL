// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.DataSets.CustomersCls
// Assembly: TrackerDotNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerDotNet.dll

//- only form later versions #nullable disable
namespace TrackerDotNet.DataSets
{
    public class CustomersCls
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
        private string _StateOrProvince;
        private string _PostalCode;
        private string _PhoneNumber;
        private string _Extension;
        private string _FaxNumber;
        private string _CellNumber;
        private string _EmailAddress;
        private string _AltEmailAddress;
        private string _CustomerType;
        private int _EquipTypeName;
        private int _CoffeePreference;
        private int _City;
        private int _PriPref;
        private int _SecPref;
        private double _PriPrefQty;
        private double _SecPrefQty;
        private int _Abreviation;
        private string _MachineSN;
        private bool _UsesFilter;
        private bool _Autofulfill;
        private bool _Enabled;
        private bool _PredictionDisabled;
        private bool _AlwaysSendChkUp;
        private bool _NormallyResponds;
        private string _Notes;

        public CustomersCls()
        {
            this._CustomerID = 0;
            this._CompanyName = this._ContactTitle = this._ContactFirstName = this._ContactLastName = this._ContactAltFirstName = this._ContactAltLastName = this._Department = this._BillingAddress = this._StateOrProvince = this._PostalCode = this._PhoneNumber = this._Extension = this._FaxNumber = this._CellNumber = this._EmailAddress = this._AltEmailAddress = this._CustomerType = "";
            this._EquipTypeName = this._CoffeePreference = this._City = this._PriPref = this._SecPref = 0;
            this._PriPrefQty = this._SecPrefQty = 0.0;
            this._Abreviation = 0;
            this._MachineSN = "";
            this._Enabled = true;
            this._UsesFilter = this._Autofulfill = this._PredictionDisabled = this._AlwaysSendChkUp = this._NormallyResponds = false;
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

        public string CustomerType
        {
            get => this._CustomerType;
            set => this._CustomerType = value;
        }

        public int EquipTypeName
        {
            get => this._EquipTypeName;
            set => this._EquipTypeName = value;
        }

        public int CoffeePreference
        {
            get => this._CoffeePreference;
            set => this._CoffeePreference = value;
        }

        public int City
        {
            get => this._City;
            set => this._City = value;
        }

        public int PriPref
        {
            get => this._PriPref;
            set => this._PriPref = value;
        }

        public int SecPref
        {
            get => this._SecPref;
            set => this._SecPref = value;
        }

        public double PriPrefQty
        {
            get => this._PriPrefQty;
            set => this._PriPrefQty = value;
        }

        public double SecPrefQty
        {
            get => this._SecPrefQty;
            set => this._SecPrefQty = value;
        }

        public int Abreviation
        {
            get => this._Abreviation;
            set => this._Abreviation = value;
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

        public bool Autofulfill
        {
            get => this._Autofulfill;
            set => this._Autofulfill = value;
        }

        public bool Enabled
        {
            get => this._Enabled;
            set => this._Enabled = value;
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

        public string Notes
        {
            get => this._Notes;
            set => this._Notes = value;
        }
    }
}