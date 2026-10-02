using System;

namespace TrackerSQL.Models
{
    /// <summary>One login on Administration → Manage Users (membership plus portal link and driver app use).</summary>
    public class UserAccountRow
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public bool IsApproved { get; set; }
        public bool IsLockedOut { get; set; }
        public bool IsOnline { get; set; }
        public string Comment { get; set; }
        /// <summary>Role names, comma separated.</summary>
        public string Roles { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsContact { get; set; }
        public int? ContactID { get; set; }
        public string CompanyName { get; set; }
        public DateTime Created { get; set; }
        /// <summary>Latest of the Tracker sign-in, portal sign-in and driver app use; the created date when never used.</summary>
        public DateTime LastUsed { get; set; }
        /// <summary>"Tracker", "portal", "driver app" or "never".</summary>
        public string LastUsedHow { get; set; }

        public bool NeverUsed => LastUsedHow == "never";
    }
}
