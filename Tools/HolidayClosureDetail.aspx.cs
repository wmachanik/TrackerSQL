using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerSQL.Classes;
using TrackerSQL.Managers;

namespace TrackerSQL.Tools
{
    public partial class HolidayClosureDetail : Page
    {
        private const string DefaultReturnUrl = "~/Tools/HolidayClosures.aspx";

        private readonly HolidayClosureManager _manager = new HolidayClosureManager();

        private int CurrentClosureId
        {
            get
            {
                if (int.TryParse(hdnClosureId.Value, out int id) && id > 0)
                    return id;
                return 0;
            }
            set => hdnClosureId.Value = value > 0 ? value.ToString() : string.Empty;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            RegisterPostBackControls();

            if (!IsPostBack)
            {
                string idStr = Request.QueryString["ID"];
                if (!string.IsNullOrEmpty(idStr) && int.TryParse(idStr, out int id) && id > 0)
                    LoadRecord(id);
                else
                    ConfigureForNew();
            }

            UpdateDeleteVisibility();
        }

        private void RegisterPostBackControls()
        {
            var scriptManager = ScriptManager.GetCurrent(Page);
            if (scriptManager == null)
                return;

            // Save stays async so UpdateProgress can show; redirect actions are full postbacks.
            scriptManager.RegisterAsyncPostBackControl(btnSave);
            scriptManager.RegisterPostBackControl(btnSaveReturn);
            scriptManager.RegisterPostBackControl(btnDelete);
            scriptManager.RegisterPostBackControl(btnBack);
        }

        private void ConfigureForNew()
        {
            CurrentClosureId = 0;
            litPageTitle.Text = "New Closure";
            litPanelTitle.Text = "New Holiday / Closure";
            txtDays.Text = "1";
            chkPrep.Checked = true;
            chkDelivery.Checked = true;
            SelectStrategy("Forward");
            SetStatus("Enter the closure details and click Save.", isError: null);
            UpdateDeleteVisibility();
        }

        private void LoadRecord(int id)
        {
            var rec = _manager.GetById(id);
            if (rec == null)
            {
                ConfigureForNew();
                SetStatus("Record not found. You can create a new closure instead.", isError: true);
                return;
            }

            CurrentClosureId = rec.HolidayClosureID;
            litPageTitle.Text = "Edit Closure";
            litPanelTitle.Text = "Edit Holiday / Closure";
            txtDate.Text = rec.ClosureDate.ToString("yyyy-MM-dd");
            txtDays.Text = HolidayClosureManager.GetDaysClosed(rec).ToString();
            SelectStrategy(rec.ShiftStrategy);
            chkPrep.Checked = HolidayClosureManager.AppliesPrep(rec);
            chkDelivery.Checked = HolidayClosureManager.AppliesDelivery(rec);
            txtDesc.Text = rec.Description ?? string.Empty;
            SetStatus("Editing closure for " + FormatClosureDate(rec.ClosureDate) + ".", isError: null);
            UpdateDeleteVisibility();
        }

        private static string FormatClosureDate(DateTime date)
        {
            return date.ToString("yyyy-MM-dd");
        }

        private void SelectStrategy(string strategy)
        {
            string value = string.IsNullOrWhiteSpace(strategy) ? "Forward" : strategy.Trim();
            ListItem match = ddlStrategy.Items.FindByValue(value)
                ?? ddlStrategy.Items.FindByText(value);

            if (match == null)
            {
                foreach (ListItem item in ddlStrategy.Items)
                {
                    if (string.Equals(item.Value, value, StringComparison.OrdinalIgnoreCase)
                        || string.Equals(item.Text, value, StringComparison.OrdinalIgnoreCase))
                    {
                        match = item;
                        break;
                    }
                }
            }

            ddlStrategy.ClearSelection();
            if (match != null)
                match.Selected = true;
            else
                ddlStrategy.SelectedIndex = 0;
        }

        private void UpdateDeleteVisibility()
        {
            btnDelete.Visible = CurrentClosureId > 0;
            btnDelete.Enabled = CurrentClosureId > 0;
        }

        private void SetStatus(string message, bool? isError)
        {
            ltrlStatus.Text = message ?? string.Empty;

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

        private void RedirectToList()
        {
            Response.Redirect(DefaultReturnUrl, false);
            Context.ApplicationInstance.CompleteRequest();
        }

        /// <summary>Persists the form. Returns false when validation/save failed (status already set).</summary>
        private bool TrySave(bool updatePanel)
        {
            try
            {
                if (!DateTime.TryParse(txtDate.Text, out DateTime startDate))
                {
                    SetStatus("Please enter a valid start date (yyyy-MM-dd).", isError: true);
                    if (updatePanel)
                        upnlDetail.Update();
                    return false;
                }

                if (!int.TryParse(txtDays.Text, out int days) || days < 1)
                    days = 1;

                string strategy = ddlStrategy.SelectedValue;
                if (string.IsNullOrWhiteSpace(strategy))
                    strategy = "Forward";

                if (CurrentClosureId <= 0)
                {
                    int newId = _manager.InsertReturningId(
                        startDate,
                        days,
                        chkPrep.Checked,
                        chkDelivery.Checked,
                        strategy,
                        txtDesc.Text,
                        out string err);

                    if (newId <= 0)
                    {
                        SetStatus(err ?? "Save failed.", isError: true);
                        if (updatePanel)
                            upnlDetail.Update();
                        return false;
                    }

                    CurrentClosureId = newId;
                    litPageTitle.Text = "Edit Closure";
                    litPanelTitle.Text = "Edit Holiday / Closure";
                    UpdateDeleteVisibility();
                    SetStatus("Closure saved for " + FormatClosureDate(startDate) + ".", isError: false);
                    if (updatePanel)
                        upnlDetail.Update();
                    return true;
                }

                bool updated = _manager.Update(
                    CurrentClosureId,
                    startDate,
                    days,
                    chkPrep.Checked,
                    chkDelivery.Checked,
                    strategy,
                    txtDesc.Text,
                    out string updateErr);

                if (!updated)
                {
                    SetStatus(updateErr ?? "Save failed.", isError: true);
                    if (updatePanel)
                        upnlDetail.Update();
                    return false;
                }

                SetStatus("Closure updated for " + FormatClosureDate(startDate) + ".", isError: false);
                if (updatePanel)
                    upnlDetail.Update();
                return true;
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System,
                    "HolidayClosureDetail Save failed: " + ex.Message);
                SetStatus("Save failed: " + ex.Message, isError: true);
                if (updatePanel)
                    upnlDetail.Update();
                return false;
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            TrySave(updatePanel: true);
        }

        protected void btnSaveReturn_Click(object sender, EventArgs e)
        {
            if (TrySave(updatePanel: false))
                RedirectToList();
        }

        protected void btnBack_Click(object sender, ImageClickEventArgs e)
        {
            RedirectToList();
        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            int id = CurrentClosureId;
            if (id <= 0)
            {
                SetStatus("Nothing to delete.", isError: true);
                return;
            }

            if (!_manager.Delete(id, out string err))
            {
                SetStatus("Delete failed: " + (err ?? "unknown error"), isError: true);
                return;
            }

            RedirectToList();
        }
    }
}
