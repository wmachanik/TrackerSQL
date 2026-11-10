using System;

namespace TrackerSQL.Models
{
    public class QuaffeeLoversGrant
    {
        public int GrantID { get; set; }
        public long ContactID { get; set; }
        public long? WooCustomerId { get; set; }
        public string MatchEmail { get; set; }
        public int WooOrderCount { get; set; }
        public string WooRoleAtGrant { get; set; }
        public bool AlreadyMemberOnWoo { get; set; }
        public DateTime? RoleGrantedUtc { get; set; }
        public DateTime? WelcomeEmailSentUtc { get; set; }
        public DateTime RecordedUtc { get; set; }
        public string RecordedBy { get; set; }
        public DateTime? InviteSentUtc { get; set; }
        public string InviteSentBy { get; set; }
        public DateTime? DismissedUtc { get; set; }
        public string DismissedBy { get; set; }
        public string Notes { get; set; }
    }
}
