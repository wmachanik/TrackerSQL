using System;
namespace TrackerDotNet.Classes.Poco
{
    public class SendCheckupEmailText
    {
        public int SCEMTID { get; set; }
        public string HeaderText { get; set; }
        public string BodyText { get; set; }
        public string FooterText { get; set; }
        public DateTime? DateLastChange { get; set; }
        public string Notes { get; set; }
    }
}
