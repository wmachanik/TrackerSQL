using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using TrackerSQL.Models;

namespace TrackerSQL.Portal
{
    public partial class MyContact : PortalPageBase
    {
        protected Literal litMessage, litDetailsHelp, litReminderState, litPrefsMessage, litRequestMessage;
        protected TextBox txtCompany, txtTitle, txtFirst, txtLast, txtAltFirst, txtAltLast;
        protected TextBox txtDept, txtAddress, txtPostal, txtProvince;
        protected TextBox txtPhone, txtCell, txtEmail, txtAltEmail, txtRequest;
        protected Button btnSave, btnRequest;
        protected CheckBox chkReminders;
        protected PlaceHolder phRecurringLock;
        protected System.Web.UI.UpdatePanel upDetails, upPrefs, upRequest;

        private HashSet<string> _editable;

        protected void Page_Load(object sender, EventArgs e)
        {
            _editable = Portal.GetEditableFields();
            if (!IsPostBack)
            {
                BindContact();
                BindReminderState();
            }
            ApplyEditableState();
        }

        private void BindContact()
        {
            Contact c = Portal.GetOwnContact();
            if (c == null) return;
            txtCompany.Text = c.CompanyName;
            txtTitle.Text = c.ContactTitle;
            txtFirst.Text = c.ContactFirstName;
            txtLast.Text = c.ContactLastName;
            txtAltFirst.Text = c.ContactAltFirstName;
            txtAltLast.Text = c.ContactAltLastName;
            txtDept.Text = c.Department;
            txtAddress.Text = c.BillingAddress;
            txtPostal.Text = c.PostalCode;
            txtProvince.Text = c.StateOrProvince;
            txtPhone.Text = c.PhoneNumber;
            txtCell.Text = c.CellNumber;
            txtEmail.Text = c.EmailAddress;
            txtAltEmail.Text = c.AltEmailAddress;
        }

        private void BindReminderState()
        {
            bool on = Portal.AreOwnRemindersEnabled();
            bool locked = Portal.HasActiveRecurring();
            chkReminders.Checked = on;
            chkReminders.Enabled = !locked;
            phRecurringLock.Visible = locked;
            litReminderState.Text = StatusBadgeHtml(on ? "On" : "Off", !on);
        }

        private void ApplyEditableState()
        {
            SetEditable(txtCompany, "CompanyName");
            SetEditable(txtTitle, "ContactTitle");
            SetEditable(txtFirst, "ContactFirstName");
            SetEditable(txtLast, "ContactLastName");
            SetEditable(txtAltFirst, "ContactAltFirstName");
            SetEditable(txtAltLast, "ContactAltLastName");
            SetEditable(txtDept, "Department");
            SetEditable(txtAddress, "BillingAddress");
            SetEditable(txtPostal, "PostalCode");
            SetEditable(txtProvince, "StateOrProvince");
            SetEditable(txtPhone, "PhoneNumber");
            SetEditable(txtCell, "CellNumber");
            SetEditable(txtEmail, "EmailAddress");
            SetEditable(txtAltEmail, "AltEmailAddress");

            bool anyEditable = _editable != null && _editable.Count > 0;
            btnSave.Visible = anyEditable;
            litDetailsHelp.Text = anyEditable
                ? "You can change the white fields and save. For grey fields or anything else, use Request A Change below."
                : "To change any of these details, use Request A Change below.";
        }

        private void SetEditable(TextBox box, string field)
        {
            bool ok = _editable != null && _editable.Contains(field);
            box.ReadOnly = !ok;
            box.CssClass = ok ? "sys-prefs-input" : "sys-prefs-input portal-readonly";
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            var updates = new Contact
            {
                CompanyName = txtCompany.Text,
                ContactTitle = txtTitle.Text,
                ContactFirstName = txtFirst.Text,
                ContactLastName = txtLast.Text,
                ContactAltFirstName = txtAltFirst.Text,
                ContactAltLastName = txtAltLast.Text,
                Department = txtDept.Text,
                BillingAddress = txtAddress.Text,
                PostalCode = txtPostal.Text,
                StateOrProvince = txtProvince.Text,
                PhoneNumber = txtPhone.Text,
                CellNumber = txtCell.Text,
                EmailAddress = txtEmail.Text,
                AltEmailAddress = txtAltEmail.Text
            };

            if (!Portal.SaveAllowedContactFields(updates, out string error))
            {
                litMessage.Text = StatusHtml(error ?? "Save failed.", true);
                return;
            }

            litMessage.Text = StatusHtml("Your details have been saved.", false);
            BindContact();
            ApplyEditableState();
        }

        protected void chkReminders_CheckedChanged(object sender, EventArgs e)
        {
            bool ok = Portal.SetOwnReminders(chkReminders.Checked, out string message);
            litPrefsMessage.Text = StatusHtml(message, !ok);
            BindReminderState();
        }

        protected void btnRequest_Click(object sender, EventArgs e)
        {
            long id = Portal.SubmitChangeRequest(
                ContactPortalChangeKinds.Contact, null, txtRequest.Text, out string message);
            litRequestMessage.Text = StatusHtml(message, id <= 0);
            if (id > 0)
                txtRequest.Text = string.Empty;
        }
    }
}
