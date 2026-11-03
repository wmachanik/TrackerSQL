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
using TrackerSQL.Classes;
using TrackerSQL.Managers;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Pages
{
    public partial class SendClientMail : Page
    {
        private const string SessionRecipients = "ClientMail.Recipients";
        private readonly ClientMailManager _mailManager = new ClientMailManager();

        protected void Page_Load(object sender, EventArgs e)
        {
            RegisterPostBackControls();
            ApplyEmailTestModeIndicator();

            if (!IsPostBack)
            {
                LoadLookups();
                LoadDefaultTexts();
                tbxTestEmail.Text = ConfigHelper.GetString("EmailTestRecipient", "warren@machanik.com");
                SetStatus("Set the message, apply filters, then send a test before the full run.", isError: null);
            }
        }

        private void RegisterPostBackControls()
        {
            var scriptManager = ScriptManager.GetCurrent(Page);
            if (scriptManager == null)
                return;
            scriptManager.RegisterAsyncPostBackControl(btnApplyFilters);
            scriptManager.RegisterAsyncPostBackControl(btnSendTest);
            scriptManager.RegisterAsyncPostBackControl(btnSend);
            scriptManager.RegisterAsyncPostBackControl(imgBtnEmailTestMode);
            scriptManager.RegisterPostBackControl(btnBack);
        }

        private void LoadLookups()
        {
            ddlContactType.Items.Clear();
            ddlContactType.Items.Add(new ListItem("(Any)", "0"));
            foreach (var t in new ContactTypesRepository().GetAll("ContactTypeDesc") ?? new List<ContactType>())
                ddlContactType.Items.Add(new ListItem(t.ContactTypeDesc, t.ContactTypeID.ToString()));

            ddlEquipType.Items.Clear();
            ddlEquipType.Items.Add(new ListItem("(Any)", "0"));
            foreach (var t in new EquipTypesRepository().GetAll("EquipTypeName") ?? new List<EquipType>())
                ddlEquipType.Items.Add(new ListItem(t.EquipTypeName, t.EquipTypeID.ToString()));

            ddlRecurring.Items.Clear();
            ddlRecurring.Items.Add(new ListItem("(Any)", ""));
            ddlRecurring.Items.Add(new ListItem("Yes", "1"));
            ddlRecurring.Items.Add(new ListItem("No", "0"));

            lstAreas.Items.Clear();
            foreach (var a in new AreasRepository().GetAll("AreaName") ?? new List<Area>())
                lstAreas.Items.Add(new ListItem(a.AreaName, a.AreaID.ToString()));

            lstLastItems.Items.Clear();
            var items = new ItemsRepository().GetAll("ItemEnabled DESC, ItemDesc") ?? new List<Item>();
            foreach (var item in items.Where(i => i.ItemEnabled != false).Take(400))
                lstLastItems.Items.Add(new ListItem(item.ItemDesc, item.ItemID.ToString()));

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

        private void ApplyEmailTestModeIndicator()
        {
            bool testMode = ConfigHelper.GetBool("EmailTestMode", false);
            string testRecipient = ConfigHelper.GetString("EmailTestRecipient", "warren@machanik.com");
            imgBtnEmailTestMode.Visible = testMode;
            if (testMode)
            {
                imgBtnEmailTestMode.ToolTip = "Email TEST MODE is ON. All sends go to " + testRecipient + ".";
                btnSend.ToolTip = "TEST MODE: emails go to " + testRecipient;
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
            try
            {
                var filter = ReadFilter();
                if (!ClientMailRepository.TryValidateCustomWhere(filter.CustomWhere, out string customError))
                {
                    SetStatus(customError, true);
                    upnlCompose.Update();
                    return;
                }

                var list = _mailManager.Search(filter);
                Session[SessionRecipients] = list;
                BindRecipients();
                SetStatus("Listed " + list.Count + " contact(s).", false);
                upnlCompose.Update();
                upnlRecipients.Update();
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.ClientMail, "Apply filters failed: " + ex.Message);
                SetStatus("Could not apply filters: " + ex.Message, true);
                upnlCompose.Update();
            }
        }

        protected void btnSendTest_Click(object sender, EventArgs e)
        {
            try
            {
                var list = GetRecipients();
                var sample = list.FirstOrDefault();
                var result = _mailManager.SendTest(
                    tbxTestEmail.Text,
                    sample,
                    tbxEmailSubject.Text,
                    tbxEmailHeader.Text,
                    tbxEmailBody.Text,
                    tbxEmailFooter.Text,
                    chkIncludeGreeting.Checked);

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

        protected void btnSend_Click(object sender, EventArgs e)
        {
            try
            {
                var list = GetRecipients();
                if (list.Count == 0)
                {
                    SetStatus("Apply Filters first — there is nobody to send to.", true);
                    upnlCompose.Update();
                    return;
                }

                int groupSize;
                int.TryParse(ddlGroupSize.SelectedValue, out groupSize);
                int delayMs;
                int.TryParse(ddlDelay.SelectedValue, out delayMs);

                SetStatus("Sending...", isError: null);
                upnlCompose.Update();

                var result = _mailManager.Send(
                    list,
                    tbxEmailSubject.Text,
                    tbxEmailHeader.Text,
                    tbxEmailBody.Text,
                    tbxEmailFooter.Text,
                    chkIncludeGreeting.Checked,
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

        private ClientMailFilter ReadFilter()
        {
            var filter = new ClientMailFilter
            {
                NameContains = tbxName.Text,
                EnabledOnly = chkEnabledOnly.Checked,
                MustHaveEmail = chkMustHaveEmail.Checked,
                CustomWhere = tbxCustomWhere.Text
            };

            int typeId;
            if (int.TryParse(ddlContactType.SelectedValue, out typeId) && typeId > 0)
                filter.ContactTypeID = typeId;

            int equipId;
            if (int.TryParse(ddlEquipType.SelectedValue, out equipId) && equipId > 0)
                filter.EquipTypeID = equipId;

            if (ddlRecurring.SelectedValue == "1")
                filter.IsRecurring = true;
            else if (ddlRecurring.SelectedValue == "0")
                filter.IsRecurring = false;

            filter.LastOrderedFrom = ParseDate(tbxLastOrderedFrom.Text);
            filter.LastOrderedTo = ParseDate(tbxLastOrderedTo.Text);
            filter.NextRequiredFrom = ParseDate(tbxNextFrom.Text);
            filter.NextRequiredTo = ParseDate(tbxNextTo.Text);

            filter.AreaIds = SelectedIds(lstAreas);
            filter.LastItemIds = SelectedIds(lstLastItems);
            return filter;
        }

        private static List<int> SelectedIds(ListBox list)
        {
            var ids = new List<int>();
            foreach (ListItem item in list.Items)
            {
                int id;
                if (item.Selected && int.TryParse(item.Value, out id) && id > 0)
                    ids.Add(id);
            }
            return ids;
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
            if (string.IsNullOrWhiteSpace(message))
            {
                pnlStatus.Attributes["class"] = "status-message";
                return;
            }
            if (isError == true)
                pnlStatus.Attributes["class"] = "status-message status-error";
            else if (isError == false)
                pnlStatus.Attributes["class"] = "status-message status-success";
            else
                pnlStatus.Attributes["class"] = "status-message status-info";
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
