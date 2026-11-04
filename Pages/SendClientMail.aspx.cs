//------------------------------------------------------------------------------
// TrackerSQL v3.x — SendClientMail
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using TrackerSQL.Classes;
using TrackerSQL.Managers;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Pages
{
    public partial class SendClientMail : Page
    {
        private const string SessionRecipients = "ClientMail.Recipients";
        private const string SessionRules = "ClientMail.Rules";
        private const string FilterCookieName = "ClientMailFilters";
        private const int FilterCookieMinutes = 1;
        private readonly ClientMailManager _mailManager = new ClientMailManager();
        private readonly ClientMailTemplateRepository _templates = new ClientMailTemplateRepository();

        protected void Page_Load(object sender, EventArgs e)
        {
            RegisterPostBackControls();
            ApplyEmailTestModeIndicator();

            if (!IsPostBack)
            {
                LoadStaticLists();
                BindSavedMessages(0);
                LoadDefaultTexts();
                tbxTestEmail.Text = ConfigHelper.GetString("EmailTestRecipient", "warren@machanik.com");
                BindFieldOperators();
                ShowValueEditors();
                if (TryRestoreFilters())
                {
                    BindRules();
                    ApplyCurrentFilters(showComposeStatus: false);
                    SetStatus("Filters restored from your last visit (kept for one minute).", false);
                }
                else
                {
                    Session[SessionRules] = new List<ClientMailRule>();
                    BindRules();
                    SetStatus("Write the message (defaults loaded), add filter conditions if you need them, then Apply Filters.", isError: null);
                }
            }
        }

        private void RegisterPostBackControls()
        {
            var scriptManager = ScriptManager.GetCurrent(Page);
            if (scriptManager == null)
                return;
            scriptManager.RegisterAsyncPostBackControl(btnReloadDefaults);
            scriptManager.RegisterAsyncPostBackControl(btnSaveMessage);
            scriptManager.RegisterAsyncPostBackControl(ddlSavedMessages);
            scriptManager.RegisterAsyncPostBackControl(btnOpenTest);
            scriptManager.RegisterAsyncPostBackControl(btnOpenSend);
            scriptManager.RegisterAsyncPostBackControl(btnSendTestConfirm);
            scriptManager.RegisterAsyncPostBackControl(btnSendConfirm);
            scriptManager.RegisterAsyncPostBackControl(imgBtnEmailTestMode);
            scriptManager.RegisterPostBackControl(btnBack);
            scriptManager.RegisterPostBackControl(btnExportCsv);
        }

        private void LoadStaticLists()
        {
            ddlJoin.Items.Clear();
            ddlJoin.Items.Add(new ListItem("And", "And"));
            ddlJoin.Items.Add(new ListItem("Or", "Or"));

            ddlGroupOpen.Items.Clear();
            ddlGroupOpen.Items.Add(new ListItem("", ""));
            ddlGroupOpen.Items.Add(new ListItem("(", "("));
            ddlGroupClose.Items.Clear();
            ddlGroupClose.Items.Add(new ListItem("", ""));
            ddlGroupClose.Items.Add(new ListItem(")", ")"));

            ddlField.Items.Clear();
            ddlField.Items.Add(new ListItem("Name", ClientMailFields.Name));
            ddlField.Items.Add(new ListItem("Email (Primary)", ClientMailFields.EmailPrimary));
            ddlField.Items.Add(new ListItem("Email (All)", ClientMailFields.EmailAll));
            ddlField.Items.Add(new ListItem("Contact Type", ClientMailFields.ContactType));
            ddlField.Items.Add(new ListItem("Date Last Ordered", ClientMailFields.LastOrdered));
            ddlField.Items.Add(new ListItem("Next Items Required", ClientMailFields.NextRequired));
            ddlField.Items.Add(new ListItem("Type Of Equipment", ClientMailFields.Equipment));
            ddlField.Items.Add(new ListItem("Area", ClientMailFields.Area));
            ddlField.Items.Add(new ListItem("Is Recurring Client", ClientMailFields.Recurring));
            ddlField.Items.Add(new ListItem("Last Item Ordered", ClientMailFields.LastItem));
            ddlField.Items.Add(new ListItem("Last Item Type", ClientMailFields.LastItemType));
            ddlField.Items.Add(new ListItem("Custom Where", ClientMailFields.CustomWhere));

            ddlGroupSize.Items.Clear();
            foreach (int n in new[] { 5, 10, 15, 20, 50, 100 })
                ddlGroupSize.Items.Add(new ListItem(n.ToString(), n.ToString()));
            ddlGroupSize.SelectedValue = "10";

            ddlDelay.Items.Clear();
            ddlDelay.Items.Add(new ListItem("None", "0"));
            ddlDelay.Items.Add(new ListItem("1 Second", "1000"));
            ddlDelay.Items.Add(new ListItem("2 Seconds", "2000"));
            ddlDelay.Items.Add(new ListItem("5 Seconds", "5000"));
            ddlDelay.Items.Add(new ListItem("10 Seconds", "10000"));
            ddlDelay.SelectedValue = "2000";
        }

        private void LoadDefaultTexts()
        {
            tbxEmailSubject.Text = MessageProvider.Get(MessageKeys.ClientMail.DefaultSubject);
            tbxEmailHeader.Text = MessageProvider.Get(MessageKeys.ClientMail.DefaultHeader);
            tbxEmailBody.Text = MessageProvider.Get(MessageKeys.ClientMail.DefaultBody);
            tbxEmailFooter.Text = MessageProvider.Get(MessageKeys.ClientMail.DefaultFooter);
        }

        protected void btnReloadDefaults_Click(object sender, EventArgs e)
        {
            LoadDefaultTexts();
            tbxTemplateName.Text = string.Empty;
            if (ddlSavedMessages.Items.FindByValue("0") != null)
                ddlSavedMessages.SelectedValue = "0";
            SetStatus("Default header, message, footer and subject reloaded.", false);
            upnlCompose.Update();
        }

        private void BindSavedMessages(int selectedId)
        {
            ddlSavedMessages.Items.Clear();
            ddlSavedMessages.Items.Add(new ListItem("(New Message)", "0"));
            foreach (var t in _templates.GetAll())
                ddlSavedMessages.Items.Add(new ListItem(t.TemplateName, t.ClientMailTemplateID.ToString()));
            string value = selectedId.ToString();
            if (ddlSavedMessages.Items.FindByValue(value) != null)
                ddlSavedMessages.SelectedValue = value;
        }

        protected void ddlSavedMessages_SelectedIndexChanged(object sender, EventArgs e)
        {
            int id;
            if (!int.TryParse(ddlSavedMessages.SelectedValue, out id) || id <= 0)
            {
                tbxTemplateName.Text = string.Empty;
                upnlCompose.Update();
                return;
            }

            var t = _templates.GetById(id);
            if (t == null)
            {
                SetStatus("That saved message was not found.", true);
                upnlCompose.Update();
                return;
            }

            tbxTemplateName.Text = t.TemplateName;
            tbxEmailSubject.Text = t.Subject;
            tbxEmailHeader.Text = t.HeaderText ?? string.Empty;
            tbxEmailBody.Text = t.BodyText ?? string.Empty;
            tbxEmailFooter.Text = t.FooterText ?? string.Empty;
            SetStatus("Loaded saved message: " + t.TemplateName, false);
            upnlCompose.Update();
        }

        protected void btnSaveMessage_Click(object sender, EventArgs e)
        {
            string name = (tbxTemplateName.Text ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(name))
            {
                SetStatus("Enter a Name for the saved message.", true);
                upnlCompose.Update();
                return;
            }

            int selectedId;
            int.TryParse(ddlSavedMessages.SelectedValue, out selectedId);

            var template = new ClientMailTemplate
            {
                ClientMailTemplateID = selectedId,
                TemplateName = name,
                Subject = tbxEmailSubject.Text,
                HeaderText = tbxEmailHeader.Text,
                BodyText = tbxEmailBody.Text,
                FooterText = tbxEmailFooter.Text
            };

            try
            {
                if (selectedId > 0)
                {
                    if (!_templates.Update(template))
                    {
                        SetStatus("Could not update that saved message.", true);
                        upnlCompose.Update();
                        return;
                    }
                    BindSavedMessages(selectedId);
                    SetStatus("Updated saved message: " + name, false);
                }
                else
                {
                    int newId = _templates.Insert(template);
                    BindSavedMessages(newId);
                    SetStatus("Saved new message: " + name, false);
                }
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.ClientMail, "Save message failed: " + ex.Message);
                SetStatus("Could not save the message. The name may already be used. " + ex.Message, true);
            }
            upnlCompose.Update();
        }

        private void BindFieldOperators()
        {
            string field = ddlField.SelectedValue;
            ddlOperator.Items.Clear();
            if (field == ClientMailFields.Name || field == ClientMailFields.EmailPrimary || field == ClientMailFields.EmailAll)
            {
                ddlOperator.Items.Add(new ListItem("Contains", ClientMailOps.Contains));
                ddlOperator.Items.Add(new ListItem("Does Not Contain", ClientMailOps.NotContains));
                ddlOperator.Items.Add(new ListItem("Starts With", ClientMailOps.StartsWith));
                ddlOperator.Items.Add(new ListItem("Does Not Start With", ClientMailOps.NotStartsWith));
                ddlOperator.Items.Add(new ListItem("Equals", ClientMailOps.EqualsOp));
                ddlOperator.Items.Add(new ListItem("Does Not Equal", ClientMailOps.NotEquals));
            }
            else if (field == ClientMailFields.LastOrdered || field == ClientMailFields.NextRequired)
            {
                ddlOperator.Items.Add(new ListItem("On Or After", ClientMailOps.OnOrAfter));
                ddlOperator.Items.Add(new ListItem("On Or Before", ClientMailOps.OnOrBefore));
                ddlOperator.Items.Add(new ListItem("Between", ClientMailOps.Between));
                ddlOperator.Items.Add(new ListItem("In The Last N Days", ClientMailOps.InLastDays));
            }
            else if (field == ClientMailFields.CustomWhere)
            {
                ddlOperator.Items.Add(new ListItem("Matches", ClientMailOps.Matches));
            }
            else if (field == ClientMailFields.Recurring)
            {
                ddlOperator.Items.Add(new ListItem("Is", ClientMailOps.Is));
            }
            else
            {
                ddlOperator.Items.Add(new ListItem("Is", ClientMailOps.Is));
                ddlOperator.Items.Add(new ListItem("Is Not", ClientMailOps.IsNot));
            }
        }

        protected void ddlField_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindFieldOperators();
            FillLookup();
            ShowValueEditors();
        }

        protected void ddlOperator_SelectedIndexChanged(object sender, EventArgs e)
        {
            ShowValueEditors();
        }

        private void FillLookup()
        {
            ddlRuleLookup.Items.Clear();
            string field = ddlField.SelectedValue;
            if (field == ClientMailFields.ContactType)
            {
                foreach (var t in new ContactTypesRepository().GetAll("ContactTypeDesc") ?? new List<ContactType>())
                    ddlRuleLookup.Items.Add(new ListItem(t.ContactTypeDesc, t.ContactTypeID.ToString()));
            }
            else if (field == ClientMailFields.Equipment)
            {
                foreach (var t in new EquipTypesRepository().GetAll("EquipTypeName") ?? new List<EquipType>())
                    ddlRuleLookup.Items.Add(new ListItem(t.EquipTypeName, t.EquipTypeID.ToString()));
            }
            else if (field == ClientMailFields.Area)
            {
                foreach (var a in new AreasRepository().GetAll("AreaName") ?? new List<Area>())
                    ddlRuleLookup.Items.Add(new ListItem(a.AreaName, a.AreaID.ToString()));
            }
            else if (field == ClientMailFields.LastItem)
            {
                var items = new ItemsRepository().GetAll("ItemEnabled DESC, ItemDesc") ?? new List<Item>();
                foreach (var item in items.Where(i => i.ItemEnabled != false).Take(400))
                    ddlRuleLookup.Items.Add(new ListItem(item.ItemDesc, item.ItemID.ToString()));
            }
            else if (field == ClientMailFields.LastItemType)
            {
                foreach (var t in new ItemServiceTypesRepository().GetAll("ItemServiceTypeName") ?? new List<ItemServiceType>())
                    ddlRuleLookup.Items.Add(new ListItem(t.ItemServiceTypeName, t.ItemServiceTypeID.ToString()));
            }
            else if (field == ClientMailFields.Recurring)
            {
                ddlRuleLookup.Items.Add(new ListItem("Yes", "1"));
                ddlRuleLookup.Items.Add(new ListItem("No", "0"));
            }
        }

        private void ShowValueEditors()
        {
            string field = ddlField.SelectedValue;
            string op = ddlOperator.SelectedValue;
            phValueText.Visible = false;
            phValueDate.Style["display"] = "none";
            phValueBetween.Style["display"] = "none";
            phValueDays.Visible = false;
            phValueLookup.Visible = false;
            phValueCustom.Visible = false;
            pnlCustomTip.Visible = false;

            if (field == ClientMailFields.Name || field == ClientMailFields.EmailPrimary || field == ClientMailFields.EmailAll)
                phValueText.Visible = true;
            else if (field == ClientMailFields.CustomWhere)
            {
                phValueCustom.Visible = true;
                pnlCustomTip.Visible = true;
            }
            else if (field == ClientMailFields.LastOrdered || field == ClientMailFields.NextRequired)
            {
                if (op == ClientMailOps.Between)
                    phValueBetween.Style["display"] = "inline-flex";
                else if (op == ClientMailOps.InLastDays)
                    phValueDays.Visible = true;
                else
                    phValueDate.Style["display"] = "inline-flex";
            }
            else
            {
                if (ddlRuleLookup.Items.Count == 0)
                    FillLookup();
                phValueLookup.Visible = true;
            }
        }

        protected void btnAddRule_Click(object sender, EventArgs e)
        {
            var rules = GetRules();
            var rule = new ClientMailRule
            {
                Id = (rules.Count == 0 ? 1 : rules.Max(r => r.Id) + 1),
                Join = rules.Count == 0 ? "" : ddlJoin.SelectedValue,
                Field = ddlField.SelectedValue,
                Operator = ddlOperator.SelectedValue,
                GroupOpen = ddlGroupOpen.SelectedValue == "(",
                GroupClose = ddlGroupClose.SelectedValue == ")"
            };

            if (!TryReadRuleValues(rule, out string err))
            {
                SetFilterStatus(err, true);
                return;
            }

            rule.Display = BuildDisplay(rule);
            rules.Add(rule);
            Session[SessionRules] = rules;
            RememberFilters();
            BindRules();
            SetFilterStatus("Added condition. Apply Filters to refresh the list.", false);
        }

        private bool TryReadRuleValues(ClientMailRule rule, out string error)
        {
            error = null;
            string field = rule.Field;
            if (field == ClientMailFields.Name || field == ClientMailFields.EmailPrimary || field == ClientMailFields.EmailAll)
            {
                rule.Value = tbxRuleValue.Text;
                if (string.IsNullOrWhiteSpace(rule.Value))
                {
                    error = field == ClientMailFields.Name ? "Enter a name to match." : "Enter an email to match.";
                    return false;
                }
                return true;
            }
            if (field == ClientMailFields.CustomWhere)
            {
                rule.Value = tbxRuleCustom.Text;
                return ClientMailRepository.TryValidateCustomWhere(rule.Value, out error);
            }
            if (field == ClientMailFields.LastOrdered || field == ClientMailFields.NextRequired)
            {
                if (rule.Operator == ClientMailOps.Between)
                {
                    rule.Value = tbxRuleDateFrom.Text;
                    rule.Value2 = tbxRuleDateTo.Text;
                    if (ParseDate(rule.Value) == null || ParseDate(rule.Value2) == null)
                    {
                        error = "Enter both dates.";
                        return false;
                    }
                }
                else if (rule.Operator == ClientMailOps.InLastDays)
                {
                    rule.Value = tbxRuleDays.Text;
                    int days;
                    if (!int.TryParse(rule.Value, out days) || days < 0)
                    {
                        error = "Enter the number of days.";
                        return false;
                    }
                }
                else
                {
                    rule.Value = tbxRuleDate.Text;
                    if (ParseDate(rule.Value) == null)
                    {
                        error = "Enter a date.";
                        return false;
                    }
                }
                return true;
            }

            if (ddlRuleLookup.SelectedItem == null)
            {
                error = "Choose a value.";
                return false;
            }
            rule.Value = ddlRuleLookup.SelectedValue;
            return true;
        }

        private string BuildDisplay(ClientMailRule rule)
        {
            string join = string.IsNullOrEmpty(rule.Join) ? "" : rule.Join + " ";
            string field = ddlField.SelectedItem != null ? ddlField.SelectedItem.Text : rule.Field;
            string op = ddlOperator.SelectedItem != null ? ddlOperator.SelectedItem.Text : rule.Operator;
            string val;
            if (phValueLookup.Visible && ddlRuleLookup.SelectedItem != null)
                val = ddlRuleLookup.SelectedItem.Text;
            else if (rule.Operator == ClientMailOps.Between)
                val = rule.Value + " to " + rule.Value2;
            else if (rule.Operator == ClientMailOps.InLastDays)
                val = rule.Value + " days";
            else
                val = rule.Value;
            string open = rule.GroupOpen ? "(" : "";
            string close = rule.GroupClose ? ")" : "";
            return join + open + field + " " + op + " " + val + close;
        }

        protected void gvRules_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "RemoveRule")
                return;
            int id;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out id))
                return;
            var rules = GetRules();
            rules.RemoveAll(r => r.Id == id);
            if (rules.Count > 0)
                rules[0].Join = "";
            Session[SessionRules] = rules;
            RememberFilters();
            BindRules();
        }

        protected void btnClearRules_Click(object sender, EventArgs e)
        {
            Session[SessionRules] = new List<ClientMailRule>();
            ForgetFilters();
            BindRules();
            SetFilterStatus("Conditions cleared.", false);
        }

        private void BindRules()
        {
            var rules = GetRules();
            gvRules.DataSource = rules;
            gvRules.DataBind();
            ddlJoin.Enabled = rules.Count > 0;
        }

        private List<ClientMailRule> GetRules()
        {
            return Session[SessionRules] as List<ClientMailRule> ?? new List<ClientMailRule>();
        }

        private void ApplyEmailTestModeIndicator()
        {
            bool testMode = ConfigHelper.GetBool("EmailTestMode", false);
            string testRecipient = ConfigHelper.GetString("EmailTestRecipient", "warren@machanik.com");
            imgBtnEmailTestMode.Visible = testMode;
            if (testMode)
            {
                imgBtnEmailTestMode.ToolTip = "Email TEST MODE is ON. All sends go to " + testRecipient + ".";
                btnOpenSend.ToolTip = "TEST MODE: emails go to " + testRecipient;
            }
        }

        protected void imgBtnEmailTestMode_Click(object sender, ImageClickEventArgs e)
        {
            string testRecipient = ConfigHelper.GetString("EmailTestRecipient", "warren@machanik.com");
            new showMessageBox(this.Page, "Email Test Mode",
                "EmailTestMode is ON in Web.config.\n\nMails will NOT go to contacts.\nThey are redirected to:\n" + testRecipient);
        }

        protected void btnApplyFilters_Click(object sender, EventArgs e)
        {
            RememberFilters();
            ApplyCurrentFilters(showComposeStatus: true);
        }

        private void ApplyCurrentFilters(bool showComposeStatus)
        {
            try
            {
                var filter = new ClientMailFilter
                {
                    EnabledOnly = chkEnabledOnly.Checked,
                    MustHaveEmail = chkMustHaveEmail.Checked,
                    Rules = GetRules()
                };
                var list = _mailManager.Search(filter);
                Session[SessionRecipients] = list;
                BindRecipients();
                SetFilterStatus("Listed " + list.Count + " contact(s).", false);
                if (showComposeStatus)
                {
                    SetStatus("Recipient list updated: " + list.Count + " contact(s).", false);
                    upnlCompose.Update();
                }
                upnlRecipients.Update();
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.ClientMail, "Apply filters failed: " + ex.Message);
                SetFilterStatus("Could not apply filters: " + ex.Message, true);
            }
        }

        private sealed class FilterCookieState
        {
            public bool EnabledOnly { get; set; }
            public bool MustHaveEmail { get; set; }
            public List<ClientMailRule> Rules { get; set; }
        }

        private void RememberFilters()
        {
            try
            {
                var state = new FilterCookieState
                {
                    EnabledOnly = chkEnabledOnly.Checked,
                    MustHaveEmail = chkMustHaveEmail.Checked,
                    Rules = GetRules()
                };
                string json = JsonConvert.SerializeObject(state);
                if (json.Length > 3500)
                    return;

                var cookie = new HttpCookie(FilterCookieName, HttpUtility.UrlEncode(json))
                {
                    Expires = DateTime.Now.AddMinutes(FilterCookieMinutes),
                    HttpOnly = true,
                    Path = "/"
                };
                Response.Cookies.Set(cookie);
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.ClientMail, "Could not write filter cookie: " + ex.Message);
            }
        }

        private void ForgetFilters()
        {
            var cookie = new HttpCookie(FilterCookieName)
            {
                Expires = DateTime.Now.AddDays(-1),
                Path = "/"
            };
            Response.Cookies.Set(cookie);
        }

        private bool TryRestoreFilters()
        {
            HttpCookie cookie = Request.Cookies[FilterCookieName];
            if (cookie == null || string.IsNullOrWhiteSpace(cookie.Value))
                return false;

            try
            {
                var state = JsonConvert.DeserializeObject<FilterCookieState>(HttpUtility.UrlDecode(cookie.Value));
                if (state == null)
                    return false;

                chkEnabledOnly.Checked = state.EnabledOnly;
                chkMustHaveEmail.Checked = state.MustHaveEmail;
                Session[SessionRules] = state.Rules ?? new List<ClientMailRule>();
                RememberFilters();
                return true;
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.ClientMail, "Could not read filter cookie: " + ex.Message);
                return false;
            }
        }

        protected void btnOpenTest_Click(object sender, EventArgs e)
        {
            mpeTest.Show();
            upnlCompose.Update();
        }

        protected void btnOpenSend_Click(object sender, EventArgs e)
        {
            if (GetRecipients().Count == 0)
            {
                SetStatus("Apply Filters first — there is nobody to send to.", true);
                upnlCompose.Update();
                return;
            }
            mpeSend.Show();
            upnlCompose.Update();
        }

        protected void btnExportCsv_Click(object sender, EventArgs e)
        {
            var list = GetRecipients();
            if (list.Count == 0)
            {
                SetStatus("Apply Filters first — there is nobody to export.", true);
                upnlCompose.Update();
                return;
            }

            string csv = _mailManager.BuildCsv(list);
            string fileName = "ClientMail-" + DateTime.Now.ToString("yyyyMMdd-HHmm") + ".csv";
            Response.Clear();
            Response.Buffer = true;
            Response.ContentType = "text/csv";
            Response.AddHeader("Content-Disposition", "attachment; filename=\"" + fileName + "\"");
            Response.Charset = "utf-8";
            Response.Write('\uFEFF');
            Response.Write(csv);
            Response.End();
        }

        protected void btnSendTestConfirm_Click(object sender, EventArgs e)
        {
            try
            {
                var sample = GetRecipients().FirstOrDefault();
                var result = _mailManager.SendTest(
                    tbxTestEmail.Text,
                    sample,
                    tbxEmailSubject.Text,
                    tbxEmailHeader.Text,
                    tbxEmailBody.Text,
                    tbxEmailFooter.Text);

                mpeTest.Hide();
                if (result.IsSuccess)
                    SetStatus(AppendEmailTestModeNote("Test email sent to " + tbxTestEmail.Text.Trim() + "."), false);
                else
                    SetStatus("Test send failed: " + result.ErrorMessage, true);
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.ClientMail, "Test send failed: " + ex.Message);
                SetStatus("Test send failed: " + ex.Message, true);
            }
            upnlCompose.Update();
        }

        protected void btnSendConfirm_Click(object sender, EventArgs e)
        {
            try
            {
                var list = GetRecipients();
                if (list.Count == 0)
                {
                    mpeSend.Hide();
                    SetStatus("Apply Filters first — there is nobody to send to.", true);
                    upnlCompose.Update();
                    return;
                }

                int groupSize;
                int.TryParse(ddlGroupSize.SelectedValue, out groupSize);
                int delayMs;
                int.TryParse(ddlDelay.SelectedValue, out delayMs);

                mpeSend.Hide();
                SetStatus("Sending...", isError: null);
                upnlCompose.Update();

                var result = _mailManager.Send(
                    list,
                    tbxEmailSubject.Text,
                    tbxEmailHeader.Text,
                    tbxEmailBody.Text,
                    tbxEmailFooter.Text,
                    chkCcOrdersEmail.Checked,
                    groupSize,
                    delayMs);

                string status = $"Complete — sent: {result.TotalSent}, failed: {result.TotalFailed}.";
                if (!string.IsNullOrWhiteSpace(result.ErrorMessage))
                    status += " " + result.ErrorMessage;
                SetStatus(AppendEmailTestModeNote(status), result.TotalFailed > 0);
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.ClientMail, "Send failed: " + ex.Message);
                SetStatus("Send failed: " + ex.Message, true);
            }
            upnlCompose.Update();
        }

        protected void btnBack_Click(object sender, ImageClickEventArgs e)
        {
            Response.Redirect("~/Default.aspx");
        }

        protected void gvRecipients_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvRecipients.PageIndex = e.NewPageIndex;
            BindRecipients();
        }

        protected void gvRecipients_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "Exclude")
                return;

            int id;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out id))
                return;

            var list = GetRecipients();
            list.RemoveAll(c => c.ContactID == id);
            Session[SessionRecipients] = list;
            BindRecipients();
            SetStatus("Excluded contact. " + list.Count + " remaining.", false);
            upnlCompose.Update();
            upnlRecipients.Update();
        }

        private void BindRecipients()
        {
            var list = GetRecipients();
            gvRecipients.DataSource = list;
            gvRecipients.DataBind();
            lblRecipientCount.Text = list.Count == 0
                ? "No contacts listed."
                : list.Count.ToString() + " contact(s) will receive this mail.";
        }

        private List<ClientMailRecipient> GetRecipients()
        {
            return Session[SessionRecipients] as List<ClientMailRecipient> ?? new List<ClientMailRecipient>();
        }

        private static DateTime? ParseDate(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;
            DateTime d;
            if (DateTime.TryParse(text, CultureInfo.CurrentCulture, DateTimeStyles.None, out d))
                return d.Date;
            if (DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out d))
                return d.Date;
            return null;
        }

        private void SetStatus(string message, bool? isError)
        {
            ltrlStatus.Text = HttpUtility.HtmlEncode(message ?? string.Empty);
            ApplyStatusClass(pnlStatus, message, isError);
        }

        private void SetFilterStatus(string message, bool? isError)
        {
            ltrlFilterStatus.Text = HttpUtility.HtmlEncode(message ?? string.Empty);
            ApplyStatusClass(pnlFilterStatus, message, isError);
        }

        private static void ApplyStatusClass(System.Web.UI.HtmlControls.HtmlGenericControl panel, string message, bool? isError)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                panel.Attributes["class"] = "status-message";
                return;
            }
            if (isError == true)
                panel.Attributes["class"] = "status-message status-error";
            else if (isError == false)
                panel.Attributes["class"] = "status-message status-success";
            else
                panel.Attributes["class"] = "status-message status-info";
        }

        private static string AppendEmailTestModeNote(string status)
        {
            if (!ConfigHelper.GetBool("EmailTestMode", false))
                return status;
            string testRecipient = ConfigHelper.GetString("EmailTestRecipient", "warren@machanik.com");
            return status + " Email TEST MODE is ON — messages go to " + testRecipient + ".";
        }
    }
}
