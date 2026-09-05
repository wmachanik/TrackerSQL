using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using TrackerSQL.Classes;
using TrackerSQL.Managers;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Tools
{
    public partial class SystemPreferences : Page
    {
        private readonly SysDataRepository _sysDataRepo = new SysDataRepository();
        private readonly ItemServiceTypesRepository _itemServiceTypesRepo = new ItemServiceTypesRepository();
        private readonly WooCommerceSettingsManager _wooManager = new WooCommerceSettingsManager();

        private const string VsSection = "SysPrefs.Section";
        private const string VsWizard = "SysPrefs.Wizard";
        private const string WizardCookieName = "TrackerWooWizardStep";
        private const string SectionCookieName = "TrackerSysPrefsSection";
        private const int WizardStepCount = 6;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!UserCanManagePreferences())
            {
                pnlSysPrefs.Visible = false;
                pnlAccessDenied.Visible = true;
                lblAccessDenied.Text = MessageProvider.Get(MessageKeys.SystemPreferences.AccessDenied);
                return;
            }

            if (!IsPostBack)
            {
                BindStaticLabels();
                int section = 0;
                bool startWizard = false;
                if (!TryReadSectionQuery(out section, out startWizard))
                    TryReadSectionCookie(out section);

                ShowSection(section);
                dvSystemData.DataBind();
                RefreshWooHome();
                if (startWizard && section == 1 && !_wooManager.GetPreferencesHeader().WooCommerceEnabled)
                    StartWizardAtStep(0, announceResume: false);
                else
                    TryResumeWizardFromCookie();
            }
            else
            {
                ApplyNavHighlight(GetSection());
            }
        }

        /// <summary>Supports ?section=woo|1 and &amp;wizard=1 from Mapping / menu deep-links.</summary>
        private bool TryReadSectionQuery(out int section, out bool startWizard)
        {
            section = 0;
            startWizard = false;
            string raw = Request.QueryString["section"];
            string wiz = Request.QueryString["wizard"];
            if (string.IsNullOrWhiteSpace(raw) && string.IsNullOrWhiteSpace(wiz))
                return false;

            if (!string.IsNullOrWhiteSpace(raw))
            {
                if (string.Equals(raw, "woo", StringComparison.OrdinalIgnoreCase)
                    || string.Equals(raw, "woocommerce", StringComparison.OrdinalIgnoreCase)
                    || raw == "1")
                    section = 1;
                else if (raw == "0" || string.Equals(raw, "general", StringComparison.OrdinalIgnoreCase))
                    section = 0;
                else if (!int.TryParse(raw, out section) || section < 0 || section > 1)
                    section = 0;
            }

            startWizard = wiz == "1"
                || string.Equals(wiz, "true", StringComparison.OrdinalIgnoreCase)
                || string.Equals(wiz, "yes", StringComparison.OrdinalIgnoreCase);
            if (startWizard)
                section = 1;
            return true;
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            if (!UserCanManagePreferences())
                return;
            WireClientHelpers();
            UpdateWizardProgress();
        }

        private bool UserCanManagePreferences()
        {
            var user = Context?.User;
            return user != null && user.Identity != null && user.Identity.IsAuthenticated &&
                   (user.IsInRole("Administrators") || user.IsInRole("Admin"));
        }

        private void BindStaticLabels()
        {
            litPageTitle.Text = MessageProvider.Get(MessageKeys.SystemPreferences.PageTitle);
            litPageSubtitle.Text = MessageProvider.Get(MessageKeys.SystemPreferences.PageSubtitle);
            litSectionsHeading.Text = MessageProvider.Get(MessageKeys.SystemPreferences.SectionsHeading);
            btnNavGeneral.Text = MessageProvider.Get(MessageKeys.SystemPreferences.NavGeneral);
            btnNavWoo.Text = MessageProvider.Get(MessageKeys.SystemPreferences.NavWooCommerce);
            litWooSectionTitle.Text = MessageProvider.Get(MessageKeys.WooCommerce.SectionTitle);
            btnStartWizard.Text = MessageProvider.Get(MessageKeys.WooCommerce.StartWizard);
            btnDisableWoo.Text = MessageProvider.Get(MessageKeys.WooCommerce.DisableButton);
            btnEnableWoo.Text = MessageProvider.Get(MessageKeys.WooCommerce.ButtonEnableNow);
            litEnableNextHint.Text = MessageProvider.Get(MessageKeys.WooCommerce.EnableNextStepHint);
            btnDisableWoo.Text = MessageProvider.Get(MessageKeys.WooCommerce.DisableButton);
            btnRerunWizard.Text = MessageProvider.Get(MessageKeys.WooCommerce.RerunWizard);
            litDisableHint.Text = MessageProvider.Get(MessageKeys.WooCommerce.DisableHint);
            litPhase2Note.Text = MessageProvider.Get(MessageKeys.WooCommerce.Phase2ComingSoon);
            hlWooMapping.Text = MessageProvider.Get(MessageKeys.WooCommerce.Phase2OpenMapping);
            btnSaveWooCreds.Text = MessageProvider.Get(MessageKeys.WooCommerce.ButtonSaveCredentials);
            btnTestWoo.Text = MessageProvider.Get(MessageKeys.WooCommerce.ButtonTestConnection);
            litLblStoreUrl.Text = MessageProvider.Get(MessageKeys.WooCommerce.LabelStoreUrl);
            litLblAdminUrl.Text = MessageProvider.Get(MessageKeys.WooCommerce.LabelAdminUrl);
            litLblKey.Text = MessageProvider.Get(MessageKeys.WooCommerce.LabelConsumerKey);
            litLblSecret.Text = MessageProvider.Get(MessageKeys.WooCommerce.LabelConsumerSecret);

            litWizPrepTitle.Text = MessageProvider.Get(MessageKeys.WooCommerce.WizardStepPrepTitle);
            litWizPrepBody.Text = MessageProvider.Get(MessageKeys.WooCommerce.WizardStepPrepBody);
            litWizSchemaTitle.Text = MessageProvider.Get(MessageKeys.WooCommerce.WizardStepSchemaTitle);
            litWizSchemaBody.Text = MessageProvider.Get(MessageKeys.WooCommerce.WizardStepSchemaBody);
            litWizCredsTitle.Text = MessageProvider.Get(MessageKeys.WooCommerce.WizardStepCredentialsTitle);
            litWizTestTitle.Text = MessageProvider.Get(MessageKeys.WooCommerce.WizardStepTestTitle);
            litWizTestBody.Text = MessageProvider.Get(MessageKeys.WooCommerce.WizardStepTestBody);
            litWizOptionsTitle.Text = MessageProvider.Get(MessageKeys.WooCommerce.WizardStepOptionsTitle);
            litWizFinishTitle.Text = MessageProvider.Get(MessageKeys.WooCommerce.WizardStepFinishTitle);
            litWizFinishBody.Text = MessageProvider.Get(MessageKeys.WooCommerce.WizardStepFinishBody);
            litOpenMappingAfterFinish.Text = MessageProvider.Get(MessageKeys.WooCommerce.OpenMappingAfterEnable);
            litOpenMappingAfterEnable.Text = MessageProvider.Get(MessageKeys.WooCommerce.OpenMappingAfterEnable);
            litWizLblStore.Text = MessageProvider.Get(MessageKeys.WooCommerce.LabelStoreUrl);
            litWizLblAdmin.Text = MessageProvider.Get(MessageKeys.WooCommerce.LabelAdminUrl);
            litWizLblKey.Text = MessageProvider.Get(MessageKeys.WooCommerce.LabelConsumerKey);
            litWizLblSecret.Text = MessageProvider.Get(MessageKeys.WooCommerce.LabelConsumerSecret);
            litWizLblCat.Text = MessageProvider.Get(MessageKeys.WooCommerce.LabelCategoryMode);
            litWizLblDispatch.Text = MessageProvider.Get(MessageKeys.WooCommerce.LabelDispatchIds);
            litWizLblTracking.Text = MessageProvider.Get(MessageKeys.WooCommerce.LabelTrackingRequired);
            btnEnsureSchema.Text = MessageProvider.Get(MessageKeys.WooCommerce.ButtonEnsureSchema);
            btnWizSaveCreds.Text = MessageProvider.Get(MessageKeys.WooCommerce.ButtonSaveCredentials);
            btnWizTest.Text = MessageProvider.Get(MessageKeys.WooCommerce.ButtonTestConnection);
            btnWizSaveOptions.Text = MessageProvider.Get(MessageKeys.WooCommerce.ButtonSaveCredentials);
            btnWizFinish.Text = MessageProvider.Get(MessageKeys.WooCommerce.ButtonFinish);
            btnWizBack.Text = MessageProvider.Get(MessageKeys.WooCommerce.ButtonBack);
            btnWizNext.Text = MessageProvider.Get(MessageKeys.WooCommerce.ButtonNext);
            btnWizCancel.Text = MessageProvider.Get(MessageKeys.WooCommerce.ButtonCancelWizard);

            if (ddlCategoryMode.Items.Count > 0)
                ddlCategoryMode.Items[0].Text = MessageProvider.Get(MessageKeys.WooCommerce.CategoryModeAll);

            litProgressLabel.Text = MessageProvider.Get(MessageKeys.WooCommerce.ProgressLabel);
            litProg1.Text = MessageProvider.Get(MessageKeys.WooCommerce.ProgressStep1);
            litProg2.Text = MessageProvider.Get(MessageKeys.WooCommerce.ProgressStep2);
            litProg3.Text = MessageProvider.Get(MessageKeys.WooCommerce.ProgressStep3);
            litProg4.Text = MessageProvider.Get(MessageKeys.WooCommerce.ProgressStep4);
            litProg5.Text = MessageProvider.Get(MessageKeys.WooCommerce.ProgressStep5);
            litProg6.Text = MessageProvider.Get(MessageKeys.WooCommerce.ProgressStep6);

            string show = MessageProvider.Get(MessageKeys.WooCommerce.ButtonShowSecret);
            string hide = MessageProvider.Get(MessageKeys.WooCommerce.ButtonHideSecret);
            ConfigureToggleButton(btnToggleKeyHome, show, hide);
            ConfigureToggleButton(btnToggleSecretHome, show, hide);
            ConfigureToggleButton(btnToggleKeyWiz, show, hide);
            ConfigureToggleButton(btnToggleSecretWiz, show, hide);
        }

        private static void ConfigureToggleButton(Button btn, string show, string hide)
        {
            btn.Text = show;
            btn.Attributes["data-show"] = show;
            btn.Attributes["data-hide"] = hide;
            btn.Attributes["aria-pressed"] = "false";
        }

        private void WireClientHelpers()
        {
            string showJs = "trackerToggleSecret('{0}', this); return false;";
            btnToggleKeyHome.OnClientClick = string.Format(showJs, txtConsumerKey.ClientID);
            btnToggleSecretHome.OnClientClick = string.Format(showJs, txtConsumerSecret.ClientID);
            btnToggleKeyWiz.OnClientClick = string.Format(showJs, txtWizKey.ClientID);
            btnToggleSecretWiz.OnClientClick = string.Format(showJs, txtWizSecret.ClientID);

            txtStoreUrl.Attributes["onblur"] =
                "trackerFillAdminFromStore('" + txtStoreUrl.ClientID + "','" + txtAdminUrl.ClientID + "', false);";
            txtWizStoreUrl.Attributes["onblur"] =
                "trackerFillAdminFromStore('" + txtWizStoreUrl.ClientID + "','" + txtWizAdminUrl.ClientID + "', false);";
            txtAdminUrl.Attributes["oninput"] = "trackerMarkAdminManual('" + txtAdminUrl.ClientID + "');";
            txtWizAdminUrl.Attributes["oninput"] = "trackerMarkAdminManual('" + txtWizAdminUrl.ClientID + "');";
        }

        private int GetSection()
        {
            object v = ViewState[VsSection];
            return v == null ? 0 : (int)v;
        }

        private void ShowSection(int index)
        {
            if (index < 0) index = 0;
            if (index > 1) index = 1;
            ViewState[VsSection] = index;
            mvSections.ActiveViewIndex = index;
            ApplyNavHighlight(index);
            PersistSectionCookie(index);
            if (index == 1)
                RefreshWooHome();
        }

        private void PersistSectionCookie(int section)
        {
            var cookie = new HttpCookie(SectionCookieName, section.ToString())
            {
                HttpOnly = true,
                Expires = DateTime.Now.AddDays(30),
                Path = "/"
            };
            if (Request.IsSecureConnection)
                cookie.Secure = true;
            Response.Cookies.Set(cookie);
        }

        private bool TryReadSectionCookie(out int section)
        {
            section = 0;
            HttpCookie cookie = Request.Cookies[SectionCookieName];
            if (cookie == null || string.IsNullOrWhiteSpace(cookie.Value))
                return false;
            if (!int.TryParse(cookie.Value.Trim(), out section))
                return false;
            if (section < 0 || section > 1)
                section = 0;
            return true;
        }

        private void ApplyNavHighlight(int index)
        {
            btnNavGeneral.CssClass = index == 0 ? "sys-prefs-tab is-active" : "sys-prefs-tab";
            btnNavWoo.CssClass = index == 1 ? "sys-prefs-tab is-active" : "sys-prefs-tab";
        }

        protected void btnNavGeneral_Click(object sender, EventArgs e)
        {
            ShowSection(0);
        }

        protected void btnNavWoo_Click(object sender, EventArgs e)
        {
            ShowSection(1);
        }

        private void RefreshWooHome()
        {
            bool wizard = ViewState[VsWizard] != null && (bool)ViewState[VsWizard];
            pnlWizard.Visible = wizard;
            pnlWooHome.Visible = !wizard;

            var hdr = _wooManager.GetPreferencesHeader();
            bool enabled = hdr.WooCommerceEnabled;
            litWooStatus.Text = enabled
                ? MessageProvider.Get(MessageKeys.WooCommerce.IntegrationEnabled)
                : MessageProvider.Get(MessageKeys.WooCommerce.IntegrationDisabled);

            // Wizard start only when not yet enabled; re-run sits with Disable after setup is done.
            btnStartWizard.Text = MessageProvider.Get(MessageKeys.WooCommerce.StartWizard);
            btnStartWizard.Visible = !enabled && !wizard;
            hlWooMapping.Visible = enabled && !wizard;
            pnlWooSettings.Visible = enabled || _wooManager.IsSchemaReady();

            var settings = _wooManager.IsSchemaReady() ? _wooManager.GetSettings() : null;
            bool canEnable = !enabled
                && settings != null
                && settings.LastConnectionTestOk == true
                && _wooManager.HasSavedConsumerKey()
                && _wooManager.HasSavedConsumerSecret();
            pnlEnableNext.Visible = canEnable && !wizard;
            pnlDisableNext.Visible = enabled && !wizard;
            if (btnRerunWizard != null)
                btnRerunWizard.Visible = enabled && !wizard;

            if (pnlWooSettings.Visible && !wizard)
            {
                var s = settings ?? _wooManager.GetSettings();
                txtStoreUrl.Text = s.StoreBaseUrl ?? string.Empty;
                string admin = s.AdminBaseUrl ?? string.Empty;
                if (string.IsNullOrWhiteSpace(admin) ||
                    string.Equals(admin.TrimEnd('/'), (s.StoreBaseUrl ?? string.Empty).TrimEnd('/'), StringComparison.OrdinalIgnoreCase))
                {
                    admin = WooCommerceApiClient.SuggestAdminUrl(s.StoreBaseUrl);
                }
                txtAdminUrl.Text = admin;
                txtConsumerKey.Attributes["placeholder"] = _wooManager.HasSavedConsumerKey()
                    ? MessageProvider.Get(MessageKeys.WooCommerce.SecretSavedPlaceholder)
                    : string.Empty;
                txtConsumerSecret.Attributes["placeholder"] = _wooManager.HasSavedConsumerSecret()
                    ? MessageProvider.Get(MessageKeys.WooCommerce.SecretSavedPlaceholder)
                    : string.Empty;

                string hint = _wooManager.GetMaskedConsumerKeyHint();
                litSavedKeyHint.Text = string.IsNullOrEmpty(hint)
                    ? string.Empty
                    : MessageProvider.Format(MessageKeys.WooCommerce.SavedKeyHint, hint);
            }
            else if (litSavedKeyHint != null)
            {
                litSavedKeyHint.Text = string.Empty;
            }
        }

        protected void btnStartWizard_Click(object sender, EventArgs e)
        {
            StartWizardAtStep(0, announceResume: false);
        }

        protected void btnWizCancel_Click(object sender, EventArgs e)
        {
            ViewState[VsWizard] = false;
            ClearWizardCookie();
            RefreshWooHome();
            SetStatus(string.Empty, null);
        }

        protected void btnWizBack_Click(object sender, EventArgs e)
        {
            if (mvWizard.ActiveViewIndex > 0)
                mvWizard.ActiveViewIndex--;
            PersistWizardStepCookie();
            UpdateWizardNavButtons();
        }

        protected void btnWizNext_Click(object sender, EventArgs e)
        {
            if (mvWizard.ActiveViewIndex == 2)
                EnsureAdminUrlFromStore(txtWizStoreUrl, txtWizAdminUrl);

            if (mvWizard.ActiveViewIndex < mvWizard.Views.Count - 1)
                mvWizard.ActiveViewIndex++;
            PersistWizardStepCookie();
            UpdateWizardNavButtons();
        }

        private void UpdateWizardNavButtons()
        {
            btnWizBack.Enabled = mvWizard.ActiveViewIndex > 0;
            btnWizNext.Enabled = mvWizard.ActiveViewIndex < mvWizard.Views.Count - 1;
        }

        private void TryResumeWizardFromCookie()
        {
            var hdr = _wooManager.GetPreferencesHeader();
            if (hdr.WooWizardCompleted && hdr.WooCommerceEnabled)
            {
                ClearWizardCookie();
                return;
            }

            int step;
            if (!TryReadWizardCookie(out step))
                return;

            ShowSection(1);
            StartWizardAtStep(step, announceResume: true);
        }

        private void StartWizardAtStep(int step, bool announceResume)
        {
            if (step < 0) step = 0;
            if (step >= WizardStepCount) step = WizardStepCount - 1;

            ViewState[VsWizard] = true;
            mvWizard.ActiveViewIndex = step;
            RefreshWooHome();
            PersistWizardStepCookie();
            UpdateWizardNavButtons();

            if (announceResume)
                SetStatus(MessageProvider.Format(MessageKeys.WooCommerce.WizardResumed, step + 1), false);
            else
                SetStatus(string.Empty, null);
        }

        private void PersistWizardStepCookie()
        {
            var cookie = new HttpCookie(WizardCookieName, mvWizard.ActiveViewIndex.ToString())
            {
                HttpOnly = true,
                Expires = DateTime.Now.AddDays(14),
                Path = "/"
            };
            if (Request.IsSecureConnection)
                cookie.Secure = true;
            Response.Cookies.Set(cookie);
        }

        private void ClearWizardCookie()
        {
            if (Response.Cookies[WizardCookieName] != null)
                Response.Cookies[WizardCookieName].Expires = DateTime.Now.AddDays(-1);
            var expired = new HttpCookie(WizardCookieName, string.Empty)
            {
                Expires = DateTime.Now.AddDays(-1),
                Path = "/"
            };
            Response.Cookies.Set(expired);
        }

        private bool TryReadWizardCookie(out int step)
        {
            step = 0;
            HttpCookie cookie = Request.Cookies[WizardCookieName];
            if (cookie == null || string.IsNullOrWhiteSpace(cookie.Value))
                return false;
            if (!int.TryParse(cookie.Value.Trim(), out step))
                return false;
            if (step < 0 || step >= WizardStepCount)
                step = 0;
            return true;
        }

        private void UpdateWizardProgress()
        {
            if (pnlWizard == null || !pnlWizard.Visible)
                return;

            int active = mvWizard.ActiveViewIndex;
            SetProgressStepState(liProg1, 0, active);
            SetProgressStepState(liProg2, 1, active);
            SetProgressStepState(liProg3, 2, active);
            SetProgressStepState(liProg4, 3, active);
            SetProgressStepState(liProg5, 4, active);
            SetProgressStepState(liProg6, 5, active);
        }

        private static void SetProgressStepState(HtmlGenericControl li, int index, int active)
        {
            string css = "sys-prefs-progress-step";
            if (index < active) css += " is-done";
            if (index == active) css += " is-current";
            li.Attributes["class"] = css;
        }

        private static void EnsureAdminUrlFromStore(TextBox storeBox, TextBox adminBox)
        {
            if (storeBox == null || adminBox == null)
                return;
            string store = WooCommerceApiClient.NormalizeStoreBaseUrl(storeBox.Text);
            if (string.IsNullOrEmpty(store))
                return;
            string suggested = WooCommerceApiClient.SuggestAdminUrl(store);
            string current = (adminBox.Text ?? string.Empty).Trim().TrimEnd('/');
            if (string.IsNullOrWhiteSpace(current) ||
                string.Equals(current, store, StringComparison.OrdinalIgnoreCase))
            {
                adminBox.Text = suggested;
            }
        }

        protected void btnEnsureSchema_Click(object sender, EventArgs e)
        {
            var result = _wooManager.EnsureSchema();
            if (result.Succeeded)
                WooCommerceSettingsManager.InvalidateSchemaCache();
            if (result.Succeeded)
                SetStatus(MessageProvider.Format(MessageKeys.WooCommerce.SchemaOk, result.CommandsRun), false);
            else
                SetStatus(MessageProvider.Format(MessageKeys.WooCommerce.SchemaFailed, result.Message), true);
        }

        protected void btnWizSaveCreds_Click(object sender, EventArgs e)
        {
            EnsureAdminUrlFromStore(txtWizStoreUrl, txtWizAdminUrl);
            SaveCredentials(txtWizStoreUrl.Text, txtWizAdminUrl.Text, txtWizKey.Text, txtWizSecret.Text, true, true);
            PersistWizardStepCookie();
        }

        protected void btnSaveWooCreds_Click(object sender, EventArgs e)
        {
            EnsureAdminUrlFromStore(txtStoreUrl, txtAdminUrl);
            bool replaceKey = !string.IsNullOrWhiteSpace(txtConsumerKey.Text);
            bool replaceSecret = !string.IsNullOrWhiteSpace(txtConsumerSecret.Text);
            SaveCredentials(txtStoreUrl.Text, txtAdminUrl.Text, txtConsumerKey.Text, txtConsumerSecret.Text, replaceKey, replaceSecret);
            txtConsumerKey.Text = string.Empty;
            txtConsumerSecret.Text = string.Empty;
            RefreshWooHome();
        }

        private void SaveCredentials(string store, string admin, string key, string secret, bool replaceKey, bool replaceSecret)
        {
            try
            {
                if (!WooCommerceSecretProtector.HasCryptoKeyConfigured())
                {
                    SetStatus(MessageProvider.Get(MessageKeys.WooCommerce.CryptoKeyMissing), true);
                    return;
                }

                store = WooCommerceApiClient.NormalizeStoreBaseUrl(store);
                if (string.IsNullOrWhiteSpace(admin) ||
                    string.Equals(admin.Trim().TrimEnd('/'), store, StringComparison.OrdinalIgnoreCase))
                {
                    admin = WooCommerceApiClient.SuggestAdminUrl(store);
                }

                _wooManager.SaveConnectionSettings(store, admin, key, secret, replaceKey, replaceSecret, CurrentUserName());
                SetStatus(MessageProvider.Get(MessageKeys.WooCommerce.CredentialsSavedKeepBlank), false);
            }
            catch (Exception ex)
            {
                SetStatus(ex.Message, true);
                AppLogger.WriteLog("woo", "Save credentials failed: " + ex.Message);
            }
        }

        protected void btnWizTest_Click(object sender, EventArgs e)
        {
            EnsureAdminUrlFromStore(txtWizStoreUrl, txtWizAdminUrl);
            RunTest(txtWizStoreUrl.Text, txtWizKey.Text, txtWizSecret.Text);
        }

        protected void btnTestWoo_Click(object sender, EventArgs e)
        {
            EnsureAdminUrlFromStore(txtStoreUrl, txtAdminUrl);
            RunTest(txtStoreUrl.Text, txtConsumerKey.Text, txtConsumerSecret.Text);
        }

        private void RunTest(string storeUrl, string keyOverride, string secretOverride)
        {
            try
            {
                var result = _wooManager.TestConnection(storeUrl, keyOverride, secretOverride, CurrentUserName());
                if (result.Succeeded)
                {
                    SetStatus(MessageProvider.Format(MessageKeys.WooCommerce.TestOk, result.Detail), false);
                    RefreshWooHome(); // reveal Enable as next action when integration is still off
                }
                else
                {
                    SetStatus(MessageProvider.Format(MessageKeys.WooCommerce.TestFailed, result.Detail), true);
                    RefreshWooHome();
                }
            }
            catch (Exception ex)
            {
                SetStatus(MessageProvider.Format(MessageKeys.WooCommerce.TestFailed, ex.Message), true);
            }
        }

        protected void btnWizSaveOptions_Click(object sender, EventArgs e)
        {
            try
            {
                _wooManager.SaveBehaviourDefaults(
                    ddlCategoryMode.SelectedValue,
                    txtDispatchIds.Text,
                    chkTrackingRequired.Checked,
                    CurrentUserName());
                SetStatus(MessageProvider.Get(MessageKeys.WooCommerce.CredentialsSaved), false);
            }
            catch (Exception ex)
            {
                SetStatus(ex.Message, true);
            }
        }

        protected void btnWizFinish_Click(object sender, EventArgs e)
        {
            try
            {
                _wooManager.CompleteWizard(CurrentUserName());
                ViewState[VsWizard] = false;
                ClearWizardCookie();
                bool openMapping = chkOpenMappingAfterFinish.Checked;
                RefreshWooHome();
                SetStatus(MessageProvider.Get(MessageKeys.WooCommerce.FinishOk), false);
                if (openMapping)
                    NavigateToWooMapping();
            }
            catch (InvalidOperationException)
            {
                SetStatus(MessageProvider.Get(MessageKeys.WooCommerce.FinishBlockedNoTest), true);
            }
            catch (Exception ex)
            {
                SetStatus(ex.Message, true);
            }
        }

        protected void btnEnableWoo_Click(object sender, EventArgs e)
        {
            try
            {
                // Same gate as wizard Finish: requires a successful Test connection.
                _wooManager.CompleteWizard(CurrentUserName());
                ClearWizardCookie();
                bool openMapping = chkOpenMappingAfterEnable.Checked;
                RefreshWooHome();
                SetStatus(MessageProvider.Get(MessageKeys.WooCommerce.FinishOk), false);
                if (openMapping)
                    NavigateToWooMapping();
            }
            catch (InvalidOperationException)
            {
                SetStatus(MessageProvider.Get(MessageKeys.WooCommerce.FinishBlockedNoTest), true);
            }
            catch (Exception ex)
            {
                SetStatus(ex.Message, true);
            }
        }

        private void NavigateToWooMapping()
        {
            string url = ResolveUrl("~/Tools/WooCommerceMapping.aspx");
            var sm = ScriptManager.GetCurrent(Page);
            if (sm != null && sm.IsInAsyncPostBack)
            {
                ScriptManager.RegisterStartupScript(
                    this,
                    GetType(),
                    "wooGoMapping",
                    "window.location.href='" + HttpUtility.JavaScriptStringEncode(url) + "';",
                    true);
            }
            else
            {
                Response.Redirect(url, endResponse: false);
                Context.ApplicationInstance.CompleteRequest();
            }
        }

        protected void btnDisableWoo_Click(object sender, EventArgs e)
        {
            try
            {
                _wooManager.SetWooEnabled(false, CurrentUserName());
                RefreshWooHome();
                SetStatus(MessageProvider.Get(MessageKeys.WooCommerce.IntegrationDisabled), false);
            }
            catch (Exception ex)
            {
                SetStatus(ex.Message, true);
            }
        }

        private string CurrentUserName()
        {
            return Context?.User?.Identity?.Name ?? "system";
        }

        private void SetStatus(string message, bool? isError)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                lblMessage.Visible = false;
                lblMessage.Text = string.Empty;
                return;
            }

            lblMessage.Visible = true;
            lblMessage.Text = message;
            lblMessage.CssClass = isError == true
                ? "status-message status-error"
                : isError == false
                    ? "status-message status-success"
                    : "status-message status-info";
        }

        #region General SysData (unchanged behaviour)

        public List<SysData> GetSystemDataForBinding()
        {
            try
            {
                SysData sysData = _sysDataRepo.GetSystemData();
                var list = new List<SysData>();
                if (sysData != null)
                    list.Add(sysData);
                return list;
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System, "GetSystemDataForBinding failed: " + ex.Message);
                return new List<SysData>();
            }
        }

        public void UpdateSystemData(SysData updatedData)
        {
            if (updatedData == null)
                updatedData = new SysData { ID = 1 };
            else
                updatedData.ID = 1;

            _sysDataRepo.UpdateSystemData(updatedData);
            AppLogger.WriteLog(SystemConstants.LogTypes.System, "System preferences (general) saved");
        }

        public string GetItemServiceTypeName(int? itemServiceTypeId)
        {
            if (!itemServiceTypeId.HasValue || itemServiceTypeId <= 0)
                return "(none)";

            try
            {
                ItemServiceType itemServiceType = _itemServiceTypesRepo.GetById(itemServiceTypeId.Value);
                if (itemServiceType != null && !string.IsNullOrEmpty(itemServiceType.ItemServiceTypeName))
                    return itemServiceType.ItemServiceTypeName;
                return "(ID: " + itemServiceTypeId + ")";
            }
            catch
            {
                return "(error)";
            }
        }

        protected void dvSystemData_ModeChanging(object sender, DetailsViewModeEventArgs e)
        {
            dvSystemData.ChangeMode(e.NewMode);
            dvSystemData.DataBind();
        }

        protected void dvSystemData_ItemUpdated(object sender, DetailsViewUpdatedEventArgs e)
        {
            if (e.Exception == null)
            {
                SetStatus(MessageProvider.Get(MessageKeys.SystemPreferences.GeneralSaved), false);
                dvSystemData.ChangeMode(DetailsViewMode.ReadOnly);
                dvSystemData.DataBind();
            }
            else
            {
                SetStatus(e.Exception.Message, true);
                e.ExceptionHandled = true;
            }
        }

        protected void dvSystemData_DataBound(object sender, EventArgs e)
        {
            // Keep status if set by update; otherwise leave alone.
        }

        #endregion
    }
}
