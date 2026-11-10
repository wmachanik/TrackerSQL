using System;
using System.Collections.Generic;

namespace TrackerSQL.Models
{
    public class QuaffeeLoversShopCheckResult
    {
        public int Checked { get; set; }
        public int AlreadyMember { get; set; }
        public int OnShopNotMember { get; set; }
        public int NotOnShop { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }

    public class QuaffeeLoversProcessResult
    {
        public bool Succeeded { get; set; }
        public string Message { get; set; }
        public int WooOrderCount { get; set; }
        public bool ThresholdMet { get; set; }
        public bool AlreadyRecorded { get; set; }
        public bool RoleGranted { get; set; }
        public bool AlreadyMemberOnWoo { get; set; }
        public bool WelcomeEmailSent { get; set; }
        public bool FoundOnWoo { get; set; }
        public string ShopRole { get; set; }
    }

    public class QuaffeeLoversContactStatus
    {
        public int ContactID { get; set; }
        public string CompanyName { get; set; }
        public string Email { get; set; }
        public int WooOrderCount { get; set; }
        public int OrderThreshold { get; set; }
        public bool ThresholdMet { get; set; }
        public bool FeatureEnabled { get; set; }
        public bool QuaffeeLoversMember { get; set; }
        public bool AlreadyMemberOnWoo { get; set; }
        public DateTime? WelcomeEmailSentUtc { get; set; }
        public DateTime? RoleGrantedUtc { get; set; }
        public string StatusLabel { get; set; }
        public string StatusTooltip { get; set; }
        public string TargetRole { get; set; }
        public string ShopRole { get; set; }
        public long? WooCustomerId { get; set; }
        public bool? HasWooCustomer { get; set; }
    }

    [Serializable]
    public class QuaffeeLoversInviteRow
    {
        public int ContactID { get; set; }
        public string CompanyName { get; set; }
        public string Email { get; set; }
        public int WooOrderCount { get; set; }
        public string WooRoleOnShop { get; set; }
        public bool AlreadyMemberOnWoo { get; set; }
        public bool WelcomeSent { get; set; }
        public bool Dismissed { get; set; }
        public bool Selected { get; set; }
        public string Notes { get; set; }
    }
}
