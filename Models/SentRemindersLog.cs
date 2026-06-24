using System;
namespace TrackerSQL.Models
{
    public class SentRemindersLog
    {
        public int ReminderID { get; set; }
        public int ContactID { get; set; }
        public DateTime? DateSentReminder { get; set; }
        public DateTime? NextPreperationDate { get; set; }
        public bool? ReminderSent { get; set; }
        public bool? HadAutoFulfilItem { get; set; }
        public bool? HadRecurrItems { get; set; }
    }
}
