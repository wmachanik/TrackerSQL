// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.Pages.OrderDone
// Assembly: TrackerDotNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerDotNet.dll

using AjaxControlToolkit;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.OleDb;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerDotNet.Managers;
using TrackerDotNet.Classes;
using TrackerDotNet.Controls;

//- only form later versions #nullable disable
namespace TrackerDotNet.Pages
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
        protected SqlDataSource sdsOrderDoneHeader;
        protected SqlDataSource sdsOrderDoneLines;
        protected SqlDataSource sdsItemTypes;
        protected SqlDataSource sdsPackagingTypes;

        // Add this helper method using existing constants
        private string GetDeliveryMethodFromPersonID(int deliveryPersonID)
        {
            // Use existing constants from SystemConstants.DeliveryConstants
            switch (deliveryPersonID)
            {
                case SystemConstants.DeliveryConstants.CourierDeliveryID:
                    return "dispatched"; // Courier = dispatched
                case SystemConstants.DeliveryConstants.ParcelDispatchID:
                    return "dispatched"; // Courier = dispatched
                case SystemConstants.DeliveryConstants.CollectionID:
                    return "collected"; // Courier = dispatched
                case SystemConstants.DeliveryConstants.DefaultDeliveryPersonID:
                    return "done"; // Squad (SQ) = delivered
                default:
                    return "done"; // Default case, assume delivered
            }
        }

        //protected override void OnInit(EventArgs e)
        //{
        //    base.OnInit(e);
        //    // Ensure mode set before automatic data binding
        //    fvOrderDone.DefaultMode = FormViewMode.Edit;
        //}
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                var tempHeader = new TempOrdersHeaderTbl().GetFirst();
                if (tempHeader != null)
                {
                    sdsOrderDoneHeader.SelectParameters["CustomerID"].DefaultValue = tempHeader.CustomerID.ToString();
                }
                fvOrderDone.DataBind();
                SetDefaultRadioButtonFromDeliveryType();
            }
        }
        // Method to set radio button based on delivery person type
        private void SetDefaultRadioButtonFromDeliveryType()
        {
            try
            {
                // Get the delivery person ID from the temp order using the control class
                TempOrdersHeaderTbl tempOrderHeader = new TempOrdersHeaderTbl().GetFirst();

                if (tempOrderHeader != null)
                {
                    // Map delivery person ID to radio button value using existing constants
                    string radioButtonValue = GetDeliveryMethodFromPersonID(tempOrderHeader.ToBeDeliveredByID);

                    // Set the radio button selection
                    rbtnSendConfirm.SelectedValue = radioButtonValue;

                    AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"OrderDone: Set delivery method to '{radioButtonValue}' based on delivery person ID '{tempOrderHeader.ToBeDeliveredByID}'");
                }
                else
                {
                    // No temp order found, keep default
                    AppLogger.WriteLog(SystemConstants.LogTypes.Orders, "OrderDone: No temp order found, keeping default radio button selection");
                }
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Orders, $"OrderDone: Error setting delivery method: {ex.Message}");
                // Fall back to default selection if there's an error
                rbtnSendConfirm.SelectedValue = "done";
            }
        }
        private bool TempOrderExists(string customerID)
        {
            string query = "SELECT COUNT(*) FROM TempOrdersHeaderTbl WHERE CustomerID = @CustomerID";

            using (var conn = new OleDbConnection(ConfigurationManager.ConnectionStrings[SystemConstants.DatabaseConstants.ConnectionStringName].ConnectionString))
            using (var cmd = new OleDbCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@CustomerID", customerID);
                conn.Open();
                int count = (int)cmd.ExecuteScalar();
                return count > 0;
            }
        }

        protected void ShowResults(string CustomerName, long pCustomerID, ClientUsageTbl pOriginalUsageData) 
        {
            this.pnlOrderDetails.Visible = false;
            this.tbxCustomerName.Text = CustomerName;
            List<ClientUsageTbl> clientUsageTblList = new List<ClientUsageTbl>();
            clientUsageTblList.Add(pOriginalUsageData);
            clientUsageTblList.Add(new ClientUsageTbl().GetUsageData(pCustomerID));
            this.dgCustomerUsage.AutoGenerateColumns = false;
            this.dgCustomerUsage.DataSource = (object)clientUsageTblList;
            this.dgCustomerUsage.DataBind();
            this.pnlCustomerDetailsUpdated.Visible = true;
        }

        //private bool SendDeliveredEmail(long pCustomerID, string pMessage)
        //{
        //    bool flag = false;
        //    CustomersTbl customersByCustomerID = new CustomersTbl().GetCustomerByCustomerID(pCustomerID);
        //    if (customersByCustomerID.EmailAddress.Contains("@") || customersByCustomerID.AltEmailAddress.Contains("@"))
        //    {
        //        string empty = string.Empty;
        //        EmailCls emailCls = new EmailCls();
        //        emailCls.SetLegacyEmailSubject("Confirmation email");
        //        string pObj1;
        //        if (customersByCustomerID.EmailAddress.Contains("@"))
        //        {
        //            emailCls.SetLegacyEmailTo(customersByCustomerID.EmailAddress);
        //            if (customersByCustomerID.AltEmailAddress.Contains("@"))
        //            {
        //                emailCls.SetLegacyEmailCC(customersByCustomerID.AltEmailAddress);
        //                string str = !string.IsNullOrEmpty(customersByCustomerID.ContactFirstName) ? customersByCustomerID.ContactFirstName : string.Empty;
        //                if (!string.IsNullOrEmpty(str) && !string.IsNullOrEmpty(customersByCustomerID.ContactAltFirstName))
        //                    str += " and ";
        //                pObj1 = str + (string.IsNullOrEmpty(customersByCustomerID.ContactAltFirstName) ? customersByCustomerID.ContactAltFirstName : string.Empty);
        //            }
        //            else
        //            {
        //                emailCls.SetLegacyEmailTo(customersByCustomerID.AltEmailAddress);
        //                pObj1 = empty + (string.IsNullOrEmpty(customersByCustomerID.ContactAltFirstName) ? customersByCustomerID.ContactAltFirstName : string.Empty);
        //            }
        //        }
        //        else
        //        {
        //            emailCls.SetLegacyEmailTo(customersByCustomerID.AltEmailAddress);
        //            pObj1 = empty + (string.IsNullOrEmpty(customersByCustomerID.ContactAltFirstName) ? customersByCustomerID.ContactAltFirstName : string.Empty);
        //        }
        //        if (string.IsNullOrEmpty(pObj1))
        //            pObj1 = "coffee lover";
        //        emailCls.AddFormatToLegacyEmailBody("To {0},<br />,<br />", (object)pObj1);
        //        emailCls.AddStrAndNewLineToLegacyEmailBody($"Just a quick note to notify you that Quaffee has {pMessage}<br />");
        //        emailCls.AddStrAndNewLineToLegacyEmailBody("Thank you for your support.<br />");
        //        emailCls.AddStrAndNewLineToLegacyEmailBody("Sincerely Quaffee Team (orders@quaffee.co.za)");
        //        flag = emailCls.SendLegacyEmail();
        //    }
        //    return flag;
        //}

        protected void btnDone_Click(object sender, EventArgs e)
        {
            Label customerID = (Label)this.fvOrderDone.FindControl("CustomerIDLabel");
            Label companyName = (Label)this.fvOrderDone.FindControl("CompanyNameLabel");
            int customerId = Convert.ToInt32(customerID.Text);
            DateTime deliveryDate = Convert.ToDateTime(((TextBox)this.fvOrderDone.FindControl("ByDateTextBox")).Text);

            string statusKey = null;
            switch (this.rbtnSendConfirm.SelectedValue)
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
                this.tbxStock.Text,
                this.tbxCount.Text,
                statusKey
            );

            if (result.Success)
            {
                new showMessageBox(this.Page, 
                    MessageProvider.Get(MessageKeys.Order.CompletedTitle),
                    MessageProvider.Format(MessageKeys.Order.CompletedSuccess, companyName.Text));
            }
            else
            {
                new showMessageBox(this.Page,
                    MessageProvider.Get(MessageKeys.Order.CompletedFailed),
                    result.Message);
            }

            this.ShowResults(companyName.Text, customerId, result.OriginalUsage);
        }

        protected void btnReturnToDeliveres_Click(object sender, EventArgs e)
        {
            this.Response.Redirect("DeliverySheet.aspx");
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            this.Response.Redirect("DeliverySheet.aspx");
        }
    }
}