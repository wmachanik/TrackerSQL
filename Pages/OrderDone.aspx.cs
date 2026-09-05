using System;
using System.Collections.Generic;
using System.Web;
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
            RegisterPostBackControls();

            if (!EnsureTempOrderSessionBound())
                return;

            if (!IsPostBack)
            {
                fvOrderDone.DataBind();
                gvOrderDoeLines.DataBind();
                SetDefaultRadioButtonFromDeliveryType();
                UpdateTrackingUi();
                SetStatus("Ready to confirm delivery.", isError: null);
            }
        }

        private void RegisterPostBackControls()
        {
            var scriptManager = ScriptManager.GetCurrent(Page);
            if (scriptManager == null)
                return;

            scriptManager.RegisterAsyncPostBackControl(btnDone);
            scriptManager.RegisterAsyncPostBackControl(rbtnSendConfirm);
            scriptManager.RegisterPostBackControl(btnCancel);
            scriptManager.RegisterPostBackControl(btnReturnToDeliveres);
        }

        private bool EnsureTempOrderSessionBound()
        {
            if (!TempOrderSession.TryResolve(out int headerId, out _))
            {
                SetStatus(MessageProvider.Get(MessageKeys.Order.NoTempOrder), isError: true);
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
                if (rbtnSendConfirm.Items.FindByValue(radioButtonValue) != null)
                    rbtnSendConfirm.SelectedValue = radioButtonValue;
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"OrderDone: Error setting delivery method: {ex.Message}");
                rbtnSendConfirm.SelectedValue = "done";
            }
        }

        protected void rbtnSendConfirm_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateTrackingUi();
            updtpnlOrderDone.Update();
        }

        private int? GetDeliveryPersonId()
        {
            int? headerId = TempOrderSession.GetHeaderId();
            if (!headerId.HasValue && TempOrderSession.TryResolve(out int resolvedHeaderId, out _))
                headerId = resolvedHeaderId;
            if (!headerId.HasValue)
                return null;
            return new TempOrdersHeaderRepository().GetById(headerId.Value)?.ToBeDeliveredByID;
        }

        private void UpdateTrackingUi()
        {
            bool need = OrderDoneManager.RequiresTrackingNumber(GetDeliveryPersonId(), rbtnSendConfirm.SelectedValue);
            pnlTracking.Visible = need;
            rfvTracking.Enabled = need;
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

        protected void ShowResults(string customerName, long customerId, CustomerUsageDisplay originalUsage)
        {
            pnlOrderDetails.Visible = false;
            tbxCustomerName.Text = customerName ?? string.Empty;

            var clientUsageList = new List<CustomerUsageDisplay>();
            if (originalUsage != null)
                clientUsageList.Add(originalUsage);

            var updatedUsage = new ContactsUsageRepository().GetByContactId((int)customerId);
            clientUsageList.Add(CustomerUsageDisplay.FromContactsUsage(updatedUsage));

            dgCustomerUsage.AutoGenerateColumns = false;
            dgCustomerUsage.DataSource = clientUsageList;
            dgCustomerUsage.DataBind();
            pnlCustomerDetailsUpdated.Visible = true;
        }

        protected void btnDone_Click(object sender, EventArgs e)
        {
            try
            {
                Label customerID = (Label)fvOrderDone.FindControl("CustomerIDLabel");
                Label companyName = (Label)fvOrderDone.FindControl("CompanyNameLabel");
                TextBox byDate = (TextBox)fvOrderDone.FindControl("ByDateTextBox");

                if (customerID == null || companyName == null || byDate == null)
                {
                    SetStatus("Could not read order header. Return to Delivery Sheet and try again.", isError: true);
                    updtpnlOrderDone.Update();
                    return;
                }

                if (!int.TryParse(customerID.Text, out int customerId) || customerId <= 0)
                {
                    SetStatus("Invalid contact on this order.", isError: true);
                    updtpnlOrderDone.Update();
                    return;
                }

                if (!DateTime.TryParse(byDate.Text, out DateTime deliveryDate))
                {
                    SetStatus("Please enter a valid delivery date.", isError: true);
                    updtpnlOrderDone.Update();
                    return;
                }

                UpdateTrackingUi();
                string trackingNumber = (tbxTrackingNumber.Text ?? string.Empty).Trim();
                if (pnlTracking.Visible && string.IsNullOrWhiteSpace(trackingNumber))
                {
                    SetStatus(MessageProvider.Get(MessageKeys.Order.TrackingRequired), isError: true);
                    updtpnlOrderDone.Update();
                    return;
                }

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

                SetStatus("Completing order...", isError: null);
                updtpnlOrderDone.Update();

                var result = OrderDoneManager.CompleteOrder(
                    customerId,
                    deliveryDate,
                    tbxStock.Text,
                    tbxCount.Text,
                    statusKey,
                    trackingNumber);

                bool emailWarn = !string.IsNullOrEmpty(result.Message)
                    && result.Message.IndexOf("email failed", StringComparison.OrdinalIgnoreCase) >= 0;

                if (result.Success && !emailWarn)
                {
                    SetStatus(result.Message, isError: false);
                    new showMessageBox(Page,
                        MessageProvider.Get(MessageKeys.Order.CompletedTitle),
                        MessageProvider.Format(MessageKeys.Order.CompletedSuccess, companyName.Text));
                }
                else if (result.Success)
                {
                    SetStatus(result.Message, isError: null);
                    new showMessageBox(Page,
                        MessageProvider.Get(MessageKeys.Order.CompletedTitle),
                        result.Message);
                }
                else
                {
                    SetStatus(result.Message, isError: true);
                    new showMessageBox(Page,
                        MessageProvider.Get(MessageKeys.Order.CompletedFailed),
                        result.Message);
                    updtpnlOrderDone.Update();
                    return;
                }

                ShowResults(companyName.Text, customerId, result.OriginalUsage);
                updtpnlOrderDone.Update();
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"OrderDone: btnDone_Click failed: {ex.Message}");
                SetStatus("Error completing order: " + ex.Message, isError: true);
                new showMessageBox(Page, MessageProvider.Get(MessageKeys.Order.CompletedFailed), ex.Message);
                updtpnlOrderDone.Update();
            }
        }

        protected void btnReturnToDeliveres_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Pages/DeliverySheet.aspx", false);
            Context.ApplicationInstance.CompleteRequest();
        }

        protected void btnCancel_Click(object sender, ImageClickEventArgs e)
        {
            TempOrderSession.CleanupCurrentTempOrder();
            Response.Redirect("~/Pages/DeliverySheet.aspx", false);
            Context.ApplicationInstance.CompleteRequest();
        }
    }
}
