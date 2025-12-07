// Decompiled with JetBrains decompiler
// Type: TrackerSQL.control.ContactToRemindDetails
// Assembly: TrackerSQL, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerSQL.dll

using System;
using TrackerSQL.Classes;

//- only form later versions #nullable disable
namespace TrackerSQL.Controls    
{
    public class ContactToRemindDetails
    {
        private int _TCCID;
        private long _CustomerID; // Changed from int to long for consistency with other classes but shoudl be long as soojn as we move to 64 bit database
        private string _CompanyName;
        private string _ContactTitle;
        private string _ContactFirstName;
        private string _ContactAltFirstName;
        private int _CityID;
        private string _EmailAddress;
        private string _AltEmailAddress;
        private int _CustomerTypeID;
        private int _EquipTypeID;
        private bool _TypicallySecToo;
        private int _PreferedAgentID;
        private int _SalesAgentID;
        private bool _UsesFilter;
        private bool _autofulfill;
        private bool _enabled;
        private bool _AlwaysSendChkUp;
        private int _ReminderCount;
        private string _Notes;
        private bool _RequiresPurchOrder;
        private DateTime _LastDateSentReminder;
        private DateTime _NextPrepDate;
        private DateTime _NextDeliveryDate;
        private DateTime _NextCoffee;
        private DateTime _NextClean;
        private DateTime _NextFilter;
        private DateTime _NextDescal;
        private DateTime _NextService;

        public ContactToRemindDetails()
        {
            this._TCCID = 0;
            this._CustomerID = 0;
            this._CompanyName = string.Empty;
            this._ContactTitle = string.Empty;
            this._ContactFirstName = string.Empty;
            this._ContactAltFirstName = string.Empty;
            this._CityID = 0;
            this._EmailAddress = string.Empty;
            this._AltEmailAddress = string.Empty;
            this._CustomerTypeID = 0;
            this._EquipTypeID = 0;
            this._TypicallySecToo = false;
            this._PreferedAgentID = 0;
            this._SalesAgentID = 0;
            this._UsesFilter = false;
            this._enabled = this._autofulfill = false;
            this._AlwaysSendChkUp = this._RequiresPurchOrder = false;
            this._ReminderCount = 0;
            this._Notes = string.Empty;
            this._NextPrepDate = this._NextDeliveryDate = TimeZoneUtils.Now().Date;
            this._LastDateSentReminder = this._NextCoffee = this._NextClean = this._NextFilter = this._NextService = DateTime.MinValue;
        }

        public int TCCID
        {
            get => this._TCCID;
            set => this._TCCID = value;
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

        public string ContactAltFirstName
        {
            get => this._ContactAltFirstName;
            set => this._ContactAltFirstName = value;
        }

        public int CityID
        {
            get => this._CityID;
            set => this._CityID = value;
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

        public int CustomerTypeID
        {
            get => this._CustomerTypeID;
            set => this._CustomerTypeID = value;
        }

        public int EquipTypeID
        {
            get => this._EquipTypeID;
            set => this._EquipTypeID = value;
        }

        public bool TypicallySecToo
        {
            get => this._TypicallySecToo;
            set => this._TypicallySecToo = value;
        }

        public int PreferedAgentID
        {
            get => this._PreferedAgentID;
            set => this._PreferedAgentID = value;
        }

        public int SalesAgentID
        {
            get => this._SalesAgentID;
            set => this._SalesAgentID = value;
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

        public bool AlwaysSendChkUp
        {
            get => this._AlwaysSendChkUp;
            set => this._AlwaysSendChkUp = value;
        }

        public int ReminderCount
        {
            get => this._ReminderCount;
            set => this._ReminderCount = value;
        }
        public string Notes
        {
            get { return _Notes; }
            set { _Notes = value; }
        }
        public bool RequiresPurchOrder
        {
            get => this._RequiresPurchOrder;
            set => this._RequiresPurchOrder = value;
        }

        public DateTime LastDateSentReminder
        {
            get => this._LastDateSentReminder;
            set => this._LastDateSentReminder = value;
        }

        public DateTime NextPrepDate
        {
            get => this._NextPrepDate;
            set => this._NextPrepDate = value;
        }

        public DateTime NextDeliveryDate
        {
            get => this._NextDeliveryDate;
            set => this._NextDeliveryDate = value;
        }

        public DateTime NextCoffee
        {
            get => this._NextCoffee;
            set => this._NextCoffee = value;
        }

        public DateTime NextClean
        {
            get => this._NextClean;
            set => this._NextClean = value;
        }

        public DateTime NextFilter
        {
            get => this._NextFilter;
            set => this._NextFilter = value;
        }

        public DateTime NextDescal
        {
            get => this._NextDescal;
            set => this._NextDescal = value;
        }

        public DateTime NextService
        {
            get => this._NextService;
            set => this._NextService = value;
        }
    }
}