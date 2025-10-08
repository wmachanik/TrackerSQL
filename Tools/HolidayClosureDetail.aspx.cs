using System;
using System.Linq;
using TrackerDotNet.Controls;
using TrackerDotNet.Classes;

namespace TrackerDotNet.Tools
{
    public partial class HolidayClosureDetail : System.Web.UI.Page
    {
        private readonly HolidayClosureProvider _provider = new HolidayClosureProvider();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string idStr = Request.QueryString["ID"];
                int id;
                if (!string.IsNullOrEmpty(idStr) && int.TryParse(idStr, out id))
                {
                    LoadRecord(id);
                }
            }
        }

        private void LoadRecord(int id)
        {
            // Broad range fetch (multi-day aware)
            var all = _provider.GetRange(DateTime.Today.AddYears(-10), DateTime.Today.AddYears(10));
            var rec = all.FirstOrDefault(h => h.ID == id);
            if (rec == null)
            {
                lblMsg.Text = "Record not found.";
                btnDelete.Enabled = false;
                return;
            }

            lblID.Text = rec.ID.ToString();
            txtDate.Text = rec.ClosureDate.ToString("yyyy-MM-dd");
            int days = rec.DaysClosed < 1 ? 1 : rec.DaysClosed;
            txtDays.Text = days.ToString();
            ddlStrategy.SelectedValue = string.IsNullOrEmpty(rec.ShiftStrategy) ? "Forward" : rec.ShiftStrategy;
            chkPrep.Checked = rec.AppliesToPrep;
            chkDelivery.Checked = rec.AppliesToDelivery;
            txtDesc.Text = rec.Description;
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            DateTime d;
            if (!DateTime.TryParse(txtDate.Text, out d))
            {
                lblMsg.Text = "Invalid date";
                return;
            }
            int days;
            if (!int.TryParse(txtDays.Text, out days) || days < 1) days = 1;

            string idStr = lblID.Text;
            if (string.IsNullOrEmpty(idStr))
            {
                string err;
                bool ok = _provider.Insert(d, days, chkPrep.Checked, chkDelivery.Checked,
                                           ddlStrategy.SelectedValue, txtDesc.Text, out err);
                lblMsg.Text = ok ? "Inserted." : "Insert failed: " + err;
                if (ok) lblID.Text = ""; // remain in create mode
            }
            else
            {
                int id;
                if (int.TryParse(idStr, out id))
                {
                    string err;
                    if (_provider.Delete(id, out err))
                    {
                        string insErr;
                        bool ok = _provider.Insert(d, days, chkPrep.Checked, chkDelivery.Checked,
                                                   ddlStrategy.SelectedValue, txtDesc.Text, out insErr);
                        lblMsg.Text = ok ? "Updated." : "Update failed: " + insErr;
                    }
                    else
                    {
                        lblMsg.Text = "Delete failed: " + err;
                    }
                }
            }
        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            int id;
            if (int.TryParse(lblID.Text, out id))
            {
                string err;
                if (_provider.Delete(id, out err))
                {
                    lblMsg.Text = "Deleted.";
                    btnDelete.Enabled = false;
                }
                else
                {
                    lblMsg.Text = "Error: " + err;
                }
            }
        }
    }
}