using System;
using System.Web;
using System.Web.UI.WebControls;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Portal
{
    public partial class MyRepair : PortalPageBase
    {
        protected Literal litStatusNote, litJob, litLogged, litStatus, litLastUpdate, litMachine, litSerial, litFault, litNotes, litMessage;
        protected TextBox txtRequest;
        protected Button btnRequest, btnBack;

        private int RepairId
        {
            get
            {
                int.TryParse(Request.QueryString["RepairID"], out int id);
                return id;
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
                Bind();
        }

        private void Bind()
        {
            ContactPortalRepairView r = Portal.GetOwnRepair(RepairId);
            if (r == null)
            {
                Response.Redirect("~/Portal/MyRepairs.aspx", true);
                return;
            }

            litStatusNote.Text = HttpUtility.HtmlEncode(
                (string.IsNullOrWhiteSpace(r.StatusNote) ? string.Empty : r.StatusNote.Trim() + " ")
                + "Questions about this repair? Use Request A Change below.");
            litJob.Text = Display(r.JobCardNumber);
            litLogged.Text = Display(r.DateLoggedDisplay);
            litStatus.Text = StatusBadgeHtml(r.StatusDisplay, r.RepairStatusID == RepairsRepository.DoneStatusId);
            litLastUpdate.Text = Display(r.LastStatusChangeDisplay);
            litMachine.Text = Display(r.EquipTypeName);
            litSerial.Text = Display(r.EquipSerialNumber);
            litFault.Text = Display(r.FaultDisplay);
            litNotes.Text = Display(r.Notes);
        }

        private static string Display(string value)
        {
            return HttpUtility.HtmlEncode(string.IsNullOrWhiteSpace(value) ? "—" : value);
        }

        protected void btnBack_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Portal/MyRepairs.aspx", true);
        }

        protected void btnRequest_Click(object sender, EventArgs e)
        {
            long id = Portal.SubmitChangeRequest(
                ContactPortalChangeKinds.Repair, RepairId, txtRequest.Text, out string message);
            litMessage.Text = StatusHtml(message, id <= 0);
            if (id > 0)
                txtRequest.Text = string.Empty;
        }
    }
}
