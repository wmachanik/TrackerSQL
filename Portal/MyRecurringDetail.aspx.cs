using System;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using TrackerSQL.Models;

namespace TrackerSQL.Portal
{
    public partial class MyRecurringDetail : PortalPageBase
    {
        protected Literal litStatus, litLastDate, litNextDate, litItemCount, litNotes, litMessage;
        protected GridView gvItems;
        protected TextBox txtRequest;
        protected Button btnRequest, btnBack;

        private int RecurringOrderId
        {
            get
            {
                int.TryParse(Request.QueryString["RecurringOrderID"], out int id);
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
            RecurringOrder r = Portal.GetOwnRecurring(RecurringOrderId);
            if (r == null)
            {
                Response.Redirect("~/Portal/MyRecurring.aspx", true);
                return;
            }

            var lines = Portal.GetOwnRecurring()
                .Where(s => s.RecurringOrderID == RecurringOrderId)
                .ToList();

            DateTime? lastDate = lines.Where(s => s.DateLastDone.HasValue)
                .Select(s => s.DateLastDone).DefaultIfEmpty(null).Max();
            DateTime? nextDate = lines.Where(s => s.NextDateRequired.HasValue)
                .Select(s => s.NextDateRequired).DefaultIfEmpty(null).Min();

            bool paused = r.Enabled == false;
            litStatus.Text = StatusBadgeHtml(paused ? "Paused" : "Active", paused);
            litLastDate.Text = Display(ContactPortalDisplay.FormatDate(lastDate));
            litNextDate.Text = Display(ContactPortalDisplay.FormatDate(nextDate));
            litItemCount.Text = lines.Count.ToString();
            litNotes.Text = Display(r.Notes);

            gvItems.DataSource = lines;
            gvItems.DataBind();
        }

        private static string Display(string value)
        {
            return HttpUtility.HtmlEncode(string.IsNullOrWhiteSpace(value) ? "—" : value);
        }

        protected void btnBack_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Portal/MyRecurring.aspx", true);
        }

        protected void btnRequest_Click(object sender, EventArgs e)
        {
            long id = Portal.SubmitChangeRequest(
                ContactPortalChangeKinds.Recurring, RecurringOrderId, txtRequest.Text, out string message);
            litMessage.Text = StatusHtml(message, id <= 0);
            if (id > 0)
                txtRequest.Text = string.Empty;
        }
    }
}
