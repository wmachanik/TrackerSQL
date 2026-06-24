using System;

namespace TrackerSQL.Classes.Poco
{
    /// <summary>
    /// POCO class for Log
    /// Generated from LogTbl
    /// </summary>
    public class Log
    {
        public int LogID { get; set; }
        public DateTime DateAdded { get; set; }
        public int UserID { get; set; }
        public int SectionID { get; set; }
        public int TranactionTypeID { get; set; }
        public long CustomerID { get; set; }
        public string Details { get; set; }
        public string Notes { get; set; }
    }
}
