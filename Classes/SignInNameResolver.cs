using System;
using System.Web.Security;

namespace TrackerSQL.Classes
{
    /// <summary>
    /// Maps what a person typed at sign-in to a membership user name.
    /// Portal contacts use their email as user name; staff may type their account email instead.
    /// </summary>
    public static class SignInNameResolver
    {
        /// <summary>The membership user name for the input, or the trimmed input when no mapping applies.</summary>
        public static string Resolve(string input)
        {
            string typed = (input ?? string.Empty).Trim();
            if (typed.Length == 0 || typed.IndexOf('@') < 1)
                return typed;

            try
            {
                if (Membership.GetUser(typed) != null)
                    return typed;

                string byEmail = Membership.GetUserNameByEmail(typed);
                return string.IsNullOrEmpty(byEmail) ? typed : byEmail;
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Login, "Sign-in name lookup failed: " + ex.Message);
                return typed;
            }
        }
    }
}
