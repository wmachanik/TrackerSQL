using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerSQL.Classes;
using TrackerSQL.Managers;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Pages
{
    public partial class OrderDone : Page
    {
        protected ScriptManager smOrderDone;
        protected UpdateProgress updtprgOrderDone;
        protected UpdatePanel updtpnlOrderDone;
        protected Panel pnlOrderDetails;
        protected FormView fvOrderDone;
        protected GridView gvOrderDoeLines;
        protected Button btnDone;
        protected Button btnCancel;
        protected TextBox tbxStock;
        protected RadioButtonList rbtnSendConfirm;
        protected TextBox tbxCount;
        protected Literal ltrlStatus;
        protected Panel pnlCustomerDetailsUpdated;
        protected Label tbxCustomerName;
        protected DataGrid dgCustomerUsage;
        protected Button btnReturnToDeliveres;
        protected ObjectDataSource odsOrderDoneHeader;
        protected ObjectDataSource odsOrderDoneLines;
        protected ObjectDataSource odsItemTypes;
        protected ObjectDataSource odsPackagingTypes;

        private string GetDeliveryMethodFromPersonID(int deliveryPersonID)
        {
            switch (deliveryPersonID)
            {
                case SystemConstants.DeliveryConstants.CourierDeliveryID:
                case SystemConstants.DeliveryConstants.ParcelDispatchID:
                    return "dispatched";
                case SystemConstants.DeliveryConstants.CollectionID:
                    return "collected";
                case SystemConstants.DeliveryConstants.DefaultDeliveryPersonID:
                default:
                    return "done";
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!EnsureTempOrderSessionBound())
                return;

            if (!IsPostBack)
            {
                fvOrderDone.DataBind();
                gvOrderDoeLines.DataBind();
                SetDefaultRadioButtonFromDeliveryType();
            }
        }

        private bool EnsureTempOrderSessionBound()
        {
            if (!TempOrderSession.TryResolve(out int headerId, out _))
            {
                ltrlStatus.Text = MessageProvider.Get(MessageKeys.Order.NoTempOrder);
                btnDone.Enabled = false;
                return false;
            }

            Session[SystemConstants.SessionConstants.TempOrderHeaderId] = headerId;
            return true;
        }

        private void SetDefaultRadioButtonFromDeliveryType()
        {
            try
            {
                int? headerId = TempOrderSession.GetHeaderId();
                if (!headerId.HasValue && TempOrderSession.TryResolve(out int resolvedHeaderId, out _))
                    headerId = resolvedHeaderId;

                if (!headerId.HasValue)
                    return;

                var tempOrderHeader = new TempOrdersHeaderRepository().GetById(headerId.Value);
                if (tempOrderHeader?.ToBeDeliveredByID == null)
                    return;

                string radioButtonValue = GetDeliveryMethodFromPersonID(tempOrderHeader.ToBeDeliveredByID.Value);
                rbtnSendConfirm.SelectedValue = radioButtonValue;
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"OrderDone: Error setting delivery method: {ex.Message}");
                rbtnSendConfirm.SelectedValue = "done";
            }
        }

        protected void ShowResults(string CustomerName, long pCustomerID, CustomerUsageDisplay pOriginalUsageData)
        {
            pnlOrderDetails.Visible = false;
            tbxCustomerName.Text = CustomerName;
            var clientUsageList = new List<CustomerUsageDisplay> { pOriginalUsageData };
            var updatedUsage = new ContactsUsageRepository().GetByContactId((int)pCustomerID);
            clientUsageList.Add(CustomerUsageDisplay.FromContactsUsage(updatedUsage));
            dgCustomerUsage.AutoGenerateColumns = false;
            dgCustomerUsage.DataSource = clientUsageList;
            dgCustomerUsage.DataBind();
            pnlCustomerDetailsUpdated.Visible = true;
        }

        protected void btnDone_Click(object sender, EventArgs e)
        {
            Label customerID = (Label)fvOrderDone.FindControl("CustomerIDLabel");
            Label companyName = (Label)fvOrderDone.FindControl("CompanyNameLabel");
            int customerId = Convert.ToInt32(customerID.Text);
            DateTime deliveryDate = Convert.ToDateTime(((TextBox)fvOrderDone.FindControl("ByDateTextBox")).Text);

            string statusKey = null;
            switch (rbtnSendConfirm.SelectedValue)
            {
                case "postbox":
                    statusKey = MessageKeys.Order.StatusPostbox;
                    break;
                case "dispatched":
                    statusKey = MessageKeys.Order.StatusDispatched;
                    break;
                case "collected":
                    statusKey = MessageKeys.Order.StatusCollected;
                    break;
                case "done":
                    statusKey = MessageKeys.Order.StatusDelivered;
                    break;
            }

            var result = OrderDoneManager.CompleteOrder(
                customerId,
                deliveryDate,
                tbxStock.Text,
                tbxCount.Text,
                statusKey);

            if (result.Success)
            {
                new showMessageBox(Page,
                    MessageProvider.Get(MessageKeys.Order.CompletedTitle),
                    MessageProvider.Format(MessageKeys.Order.CompletedSuccess, companyName.Text));
            }
            else
            {
                new showMessageBox(Page,
                    MessageProvider.Get(MessageKeys.Order.CompletedFailed),
                    result.Message);
            }

            ShowResults(companyName.Text, customerId, result.OriginalUsage);
        }

        protected void btnReturnToDeliveres_Click(object sender, EventArgs e)
        {
            Response.Redirect("DeliverySheet.aspx");
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            TempOrderSession.CleanupCurrentTempOrder();
            Response.Redirect("DeliverySheet.aspx");
        }
    }
}
