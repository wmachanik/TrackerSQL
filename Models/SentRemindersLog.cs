using System;
namespace TrackerSQL.Models
{
    public class SentRemindersLog
    {
        public int ReminderID { get; set; }
        public int ContactID { get; set; }
        public DateTime? DateSentReminder { get; set; }
        public DateTime? NextPreparationDate { get; set; }
        public bool? ReminderSent { get; set; }
        public bool? HadAutoFulfilItem { get; set; }
        public bool? HadRecurringItems { get; set; }
    }
}
