using System;
namespace TrackerDotNet.Classes.Poco
{
    public class TempCoffeecheckupCustomer
    {
        public int TCCID { get; set; }
        public int? ContactID { get; set; }
        public string CompanyName { get; set; }
        public string ContactFirstName { get; set; }
        public string ContactAltFirstName { get; set; }
        public int? AreaID { get; set; }
        public string EmailAddress { get; set; }
        public string AltEmailAddress { get; set; }
        public int? ContactTypeID { get; set; }
        public int? EquipTypeID { get; set; }
        public bool? TypicallySecToo { get; set; }
        public int? PreferedAgentID { get; set; }
        public int? SalesAgentID { get; set; }
        public bool? UsesFilter { get; set; }
        public bool? Enabled { get; set; }
        public bool? AlwaysSendChkUp { get; set; }
        public int? ReminderCount { get; set; }
        public DateTime? NextPrepDate { get; set; }
        public DateTime? NextDeliveryDate { get; set; }
        public DateTime? NextCoffee { get; set; }
        public DateTime? NextClean { get; set; }
        public DateTime? NextFilter { get; set; }
        public DateTime? NextDescal { get; set; }
        public DateTime? NextService { get; set; }
        public bool? RequiresPurchOrder { get; set; }
    }
}
