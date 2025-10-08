// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.Account.Login
// Assembly: TrackerDotNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerDotNet.dll

using System;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerDotNet.Classes;

//- only form later versions #nullable disable
namespace TrackerDotNet.Account
{
    public partial class Login : Page
    {
        protected HyperLink RegisterHyperLink;
        protected System.Web.UI.WebControls.Login LoginUser;

        protected void Page_Load(object sender, EventArgs e)
        {
            this.RegisterHyperLink.NavigateUrl = "Register.aspx?ReturnUrl=" + HttpUtility.UrlEncode(this.Request.QueryString["ReturnUrl"]);
        }
        protected void LoginUser_LoggedIn(object sender, EventArgs e)
        {
            string username = LoginUser.UserName;

            MembershipUser user = Membership.GetUser(username);
            if (user == null) return;

            Guid userId = (Guid)user.ProviderUserKey;

            try
            {
                // Ensure table exists -> this is done in GetCurrentPreferences anyway
                // TrackerDotNet.Classes.UserPreferencesHelper.EnsureUserPreferencesTableExists();

                // Get preferences or fallback
                var prefs = TrackerDotNet.Classes.UserPreferencesHelper.GetCurrentPreferencesForUser(userId);

                // Store in Session
                Session["UserPreferences"] = prefs;
                Session["UserTimeZoneInfo"] = prefs.GetTimeZoneInfo(); // <-- Set this first

                // log that they are logged in
                DateTime userNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Session["UserTimeZoneInfo"] as TimeZoneInfo);
                TrackerDotNet.Classes.AppLogger.WriteLog(SystemConstants.LogTypes.Login, $"User '{LoginUser.UserName}' logged in at {userNow:yyyy-MM-dd HH:mm:ss} ({(Session["UserTimeZoneInfo"] as TimeZoneInfo)?.Id})");

                // Optional shortcut: store just the TimeZoneInfo
                Session["UserTimeZoneInfo"] = prefs.GetTimeZoneInfo();
            }
            catch (Exception ex)
            {
                // Fallback to default time zone only if DB fails
                string defaultTzId = System.Configuration.ConfigurationManager.AppSettings["AppTimeZoneId"];
                var defaultZone = TimeZoneInfo.FindSystemTimeZoneById(defaultTzId ?? "South Africa Standard Time");

                Session["UserPreferences"] = new TrackerDotNet.Classes.UserPreferences
                {
                    UserId = userId,
                    TimeZoneId = defaultZone.Id,
                    Language = "en-ZA",
                    LoadedOn = DateTime.UtcNow
                };

                Session["UserTimeZoneInfo"] = defaultZone;

                TrackerDotNet.Classes.AppLogger.WriteLog(SystemConstants.LogTypes.Login, $"Fallback to default zone for {username}: {ex.Message}");
            }
        }
    }

}