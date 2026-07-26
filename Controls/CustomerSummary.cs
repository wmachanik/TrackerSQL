//------------------------------------------------------------------------------
// TrackerSQL v3.x — CustomerSummary
// Data access / control type: CustomerSummary.
//------------------------------------------------------------------------------

//- only form later versions #nullable disable
using System;

namespace TrackerSQL.Controls
{
    [Obsolete("DO NOT USE Comtrols use Models - MIGRATION IN PROGRESS", true)]
    public class CustomerSummary
    {
        private long _CustomerID;
        private string _CompanyName;
        private string _ContactFirstName;
        private string _ContactLastName;
        private string _AreaName;
        private string _PhoneNumber;
        private string _EmailAddress;
        private string _DeliveryBy;
        private string _EquipTypeName;
        private string _MachineSN;
        private bool _autofulfill;
        private bool _enabled;

        public CustomerSummary()
        {
            this._CustomerID = 0;
            this._CompanyName = string.Empty;
            this._ContactFirstName = string.Empty;
            this._ContactLastName = string.Empty;
            this._AreaName = string.Empty;
            this._PhoneNumber = string.Empty;
            this._EmailAddress = string.Empty;
            this._DeliveryBy = string.Empty;
            this._EquipTypeName = string.Empty;
            this._MachineSN = string.Empty;
            this._autofulfill = false;
            this.enabled = false;
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

        public string AreaName
        {
            get => this._AreaName;
            set => this._AreaName = value;
        }

        public string PhoneNumber
        {
            get => this._PhoneNumber;
            set => this._PhoneNumber = value;
        }

        public string EmailAddress
        {
            get => this._EmailAddress;
            set => this._EmailAddress = value;
        }

        public string DeliveryBy
        {
            get => this._DeliveryBy;
            set => this._DeliveryBy = value;
        }

        public string EquipTypeName
        {
            get => this._EquipTypeName;
            set => this._EquipTypeName = value;
        }

        public string MachineSN
        {
            get => this._MachineSN;
            set => this._MachineSN = value;
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
    }
}
