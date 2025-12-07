using System;
namespace TrackerDotNet.Classes.Poco
{
    public class SentRemindersLog
    {
        public int ReminderID { get; set; }
        public int ContactID { get; set; }
        public DateTime? DateSentReminder { get; set; }
        public DateTime? NextPrepDate { get; set; }
        public bool? ReminderSent { get; set; }
        public bool? HadAutoFulfilItem { get; set; }
        public bool? HadRecurrItems { get; set; }
    }
}
