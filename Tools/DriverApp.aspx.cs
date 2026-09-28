using System;
using System.Globalization;
using System.IO;
using System.Net.Mail;
using System.Web;
using System.Web.UI;
using TrackerSQL.Managers;
using TrackerSQL.Models;

namespace TrackerSQL.Tools
{
    /// <summary>
    /// Where drivers get the Quaffee Driver app: the Android APK (built by TrackerDriver "npm run apk") or the web app at /driver/.
    /// Administrators also set how Tracker handles deliveries sent from the app.
    /// </summary>
    public partial class DriverApp : Page
    {
        private const string ApkPath = "~/Downloads/QuaffeeDriver.apk";

        protected string ApkUrl => ResolveUrl(ApkPath);

        protected string PageUrl => Absolute(Request.Url.AbsolutePath);

        protected string WebAppUrl => Absolute(ResolveUrl("~/driver/"));

        protected void Page_Load(object sender, EventArgs e)
        {
            pnlSettings.Visible = SecurityManager.IsAdmin();
            if (pnlSettings.Visible && !IsPostBack)
                BindSettings();

            var apk = new FileInfo(Server.MapPath(ApkPath));
            if (!apk.Exists)
            {
                pnlApk.Visible = false;
                pnlNoApk.Visible = true;
                return;
            }

            litApkInfo.Text = string.Format(CultureInfo.InvariantCulture, "{0:0.0} MB, built {1:dd MMM yyyy HH:mm}",
                apk.Length / 1048576.0, apk.LastWriteTime);
        }

        private void BindSettings()
        {
            var settings = MobileApiSettingsManager.Load(out bool saved);
            chkRunDone.Checked = settings.RunDoneOnDelivery;
            chkSendConfirmation.Checked = settings.SendDeliveryConfirmation;
            chkNotesToOffice.Checked = settings.AllowNotesToOffice;
            txtOfficeEmail.Text = settings.OfficeEmail ?? string.Empty;
            litOrdersEmail.Text = HttpUtility.HtmlEncode(MobileApiSettingsManager.OrdersEmail);
            litSettingsInfo.Text = saved && settings.UpdatedAt.HasValue
                ? HttpUtility.HtmlEncode("Last saved " + settings.UpdatedAt.Value.ToString("dd MMM yyyy HH:mm", CultureInfo.InvariantCulture)
                    + (string.IsNullOrWhiteSpace(settings.UpdatedBy) ? string.Empty : " by " + settings.UpdatedBy))
                : "Not saved yet (showing the defaults).";
        }

        protected void btnSaveSettings_Click(object sender, EventArgs e)
        {
            if (!SecurityManager.IsAdmin())
                return;

            string office = txtOfficeEmail.Text.Trim();
            if (office.Length > 0 && !IsEmail(office))
            {
                SetMsg("The office email address is not valid.", true);
                return;
            }

            var settings = new MobileApiSettings
            {
                RunDoneOnDelivery = chkRunDone.Checked,
                SendDeliveryConfirmation = chkSendConfirmation.Checked,
                AllowNotesToOffice = chkNotesToOffice.Checked,
                OfficeEmail = office
            };

            try
            {
                if (MobileApiSettingsManager.Save(settings, User.Identity.Name))
                    SetMsg("Settings saved. Drivers' apps pick them up the next time they refresh.", false);
                else
                    SetMsg("Could not save the settings.", true);
            }
            catch (Exception ex)
            {
                SetMsg("Could not save the settings: " + ex.Message, true);
            }
            BindSettings();
        }

        private static bool IsEmail(string value)
        {
            try
            {
                return new MailAddress(value).Address.Equals(value, StringComparison.OrdinalIgnoreCase);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private void SetMsg(string text, bool error)
        {
            litMessage.Text = "<p class=\"status-message " + (error ? "status-error" : "status-info") + "\">"
                + HttpUtility.HtmlEncode(text) + "</p>";
        }

        private string Absolute(string path)
        {
            return Request.Url.GetLeftPart(UriPartial.Authority) + path;
        }
    }
}
