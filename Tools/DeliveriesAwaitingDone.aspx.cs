using System;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerSQL.Classes;
using TrackerSQL.Managers;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Tools
{
    /// <summary>
    /// Administrators: driver deliveries whose order is still not done (being completed in the background, left for the
    /// office, failed or with items to follow), with Complete Now to run Order Done again. ?OrderID= highlights a row
    /// (the Delivery Sheet's red flag links here).
    /// </summary>
    public partial class DeliveriesAwaitingDone : Page
    {
        private int? _highlightOrderId;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!SecurityManager.IsAdmin())
            {
                pnlNotAdmin.Visible = true;
                pnlAwaiting.Visible = false;
                return;
            }
            if (int.TryParse(Request.QueryString["OrderID"], out int orderId) && orderId > 0)
                _highlightOrderId = orderId;
            if (!IsPostBack)
            {
                MobileDeliveryCompletionQueue.Kick();
                BindAwaiting();
            }
        }

        private void BindAwaiting()
        {
            try
            {
                gvAwaiting.DataSource = new MobileDeliveryManager().ListAwaitingDone();
                gvAwaiting.DataBind();
            }
            catch (Exception ex)
            {
                SetAwaitingMsg("Could not list the deliveries: " + ex.Message, true);
            }
        }

        protected void gvAwaiting_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow && _highlightOrderId.HasValue
                && ((MobileDeliveryProof)e.Row.DataItem).OrderID == _highlightOrderId.Value)
                e.Row.CssClass = (e.Row.CssClass + " awaiting-highlight").Trim();
        }

        protected void btnRefreshAwaiting_Click(object sender, EventArgs e)
        {
            if (!SecurityManager.IsAdmin())
                return;
            MobileDeliveryCompletionQueue.Kick();
            BindAwaiting();
        }

        protected void gvAwaiting_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "CompleteNow" || !SecurityManager.IsAdmin() || !long.TryParse(Convert.ToString(e.CommandArgument), out long proofId))
                return;

            string why = new MobileDeliveryManager().RetryCompletion(proofId, User.Identity.Name);
            if (why != null)
                SetAwaitingMsg(why, true);
            else
            {
                var repo = new MobileApiRepository();
                MobileDeliveryProof proof = repo.GetProof(proofId);
                for (int i = 0; i < 30 && proof?.Status == MobileDeliveryManager.StatusPending; i++)
                {
                    System.Threading.Thread.Sleep(500);
                    proof = repo.GetProof(proofId);
                }
                string order = "Order " + proof?.OrderID;
                switch (proof?.Status)
                {
                    case MobileDeliveryManager.StatusCompleted:
                    case MobileDeliveryManager.StatusAlreadyDone:
                        SetAwaitingMsg(order + " is done. " + proof.StatusMessage, false);
                        break;
                    case MobileDeliveryManager.StatusPending:
                        SetAwaitingMsg(order + " is still being completed; press Refresh in a moment.", false);
                        break;
                    default:
                        SetAwaitingMsg(order + ": " + proof?.StatusMessage, true);
                        break;
                }
            }
            BindAwaiting();
        }

        protected static string StatusText(object item)
        {
            return MobileDeliveryManager.AwaitingStatusText((MobileDeliveryProof)item);
        }

        protected static bool HasItemsToFollow(object item)
        {
            return !string.IsNullOrWhiteSpace(((MobileDeliveryProof)item).MissingItems);
        }

        protected static bool CanComplete(object item)
        {
            var proof = (MobileDeliveryProof)item;
            return string.IsNullOrWhiteSpace(proof.MissingItems) && proof.Status != MobileDeliveryManager.StatusPending;
        }

        private void SetAwaitingMsg(string text, bool error)
        {
            litAwaitingMessage.Text = "<p class=\"status-message " + (error ? "status-error" : "status-info") + "\">"
                + HttpUtility.HtmlEncode(text) + "</p>";
        }
    }
}
