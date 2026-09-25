using System;

namespace TrackerSQL.Models
{
    /// <summary>One row in ContactChangeLogTbl — durable field / event history for a contact.</summary>
    public class ContactChangeLogEntry
    {
        public long ChangeLogID { get; set; }
        public int ContactID { get; set; }
        public DateTime ChangedAt { get; set; }
        public string ChangedBy { get; set; }
        public string Source { get; set; }
        public string Summary { get; set; }
        public string FieldName { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }

        public string ChangeDisplay
        {
            get
            {
                if (string.IsNullOrWhiteSpace(FieldName))
                    return Summary ?? string.Empty;
                string oldV = string.IsNullOrEmpty(OldValue) ? "(empty)" : OldValue;
                string newV = string.IsNullOrEmpty(NewValue) ? "(empty)" : NewValue;
                return oldV + " → " + newV;
            }
        }
    }
}
