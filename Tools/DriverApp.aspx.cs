using System;
using System.Globalization;
using System.IO;
using System.Net.Mail;
using System.Web;
using System.Web.UI;
using TrackerSQL.Classes;
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

            MobileAppRelease release = DriverAppReleaseManager.Latest();
            litApkInfo.Text = HttpUtility.HtmlEncode(string.Format(CultureInfo.InvariantCulture, "{0}{1:0.0} MB, built {2:dd MMM yyyy HH:mm}",
                release != null ? "version " + release.Version + ", " : string.Empty, apk.Length / 1048576.0, apk.LastWriteTime));
        }

        private void BindSettings()
        {
            var settings = MobileApiSettingsManager.Load(out bool saved);
            chkRunDone.Checked = settings.RunDoneOnDelivery;
            chkSendConfirmation.Checked = settings.SendDeliveryConfirmation;
            chkNotesToOffice.Checked = settings.AllowNotesToOffice;
            txtOfficeEmail.Text = settings.OfficeEmail ?? string.Empty;
            txtLandlines.Text = string.Join(", ", MobileApiSettingsManager.ParsePrefixes(settings.LandlinePrefixes ?? MobileApiSettingsManager.DefaultLandlinePrefixes));
            txtTerms.Text = MobileApiSettingsManager.DeliveryTerms(settings);
            txtTermsUrl.Text = MobileApiSettingsManager.TermsUrl(settings);
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

            string terms = txtTerms.Text.Trim();
            string termsUrl = txtTermsUrl.Text.Trim();
            if (termsUrl.Length > 0 && !(Uri.TryCreate(termsUrl, UriKind.Absolute, out Uri link) && (link.Scheme == Uri.UriSchemeHttps || link.Scheme == Uri.UriSchemeHttp)))
            {
                SetMsg("The terms link must be a full web address, e.g. https://quaffee.co.za/...", true);
                return;
            }

            MobileApiSettings before = MobileApiSettingsManager.Load(out _);
            string landlines = string.Join(", ", MobileApiSettingsManager.ParsePrefixes(txtLandlines.Text));
            var settings = new MobileApiSettings
            {
                RunDoneOnDelivery = chkRunDone.Checked,
                SendDeliveryConfirmation = chkSendConfirmation.Checked,
                AllowNotesToOffice = chkNotesToOffice.Checked,
                OfficeEmail = office,
                LandlinePrefixes = landlines,
                DeliveryTerms = terms == MobileApiSettingsManager.DefaultDeliveryTerms ? null : terms,
                TermsUrl = termsUrl == MobileApiSettingsManager.DefaultTermsUrl ? null : termsUrl
            };

            try
            {
                if (MobileApiSettingsManager.Save(settings, User.Identity.Name))
                {
                    string old = string.Join(", ", MobileApiSettingsManager.ParsePrefixes(before.LandlinePrefixes ?? MobileApiSettingsManager.DefaultLandlinePrefixes));
                    if (old != landlines)
                        AppLogger.WriteLog(SystemConstants.LogTypes.System, "Driver App landline prefixes changed from '" + old + "' to '" + landlines + "'", User.Identity.Name);
                    if (MobileApiSettingsManager.DeliveryTerms(before) != MobileApiSettingsManager.DeliveryTerms(settings)
                        || MobileApiSettingsManager.TermsUrl(before) != MobileApiSettingsManager.TermsUrl(settings))
                        AppLogger.WriteLog(SystemConstants.LogTypes.System, "Driver App delivery terms changed to '" + MobileApiSettingsManager.DeliveryTerms(settings)
                            + "' (" + MobileApiSettingsManager.TermsUrl(settings) + ")", User.Identity.Name);
                    SetMsg("Settings saved. Drivers' apps pick them up the next time they refresh.", false);
                }
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
