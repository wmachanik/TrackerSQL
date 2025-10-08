using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using TrackerDotNet.classes;
using TrackerDotNet.control;

namespace TrackerDotNet.Pages
{
    public partial class NewOrder : System.Web.UI.Page
    {

        const string DV_CONTROL_CUSTOMER = "ddlCustomer";
        const string DV_CONTROL_ORDER_DATE = "tbxDetailOrderDate";
        const string DV_CONTROL_ROAST_DATE = "tbxDetailRoastDate";
        const string DV_CONTROL_REQUIRED_BY_DATE = "tbxRequiredByDate";
        const string DV_CONTROL_QTY = "tbxQuantity";
        const string DV_CONTROL_ITEMDESC = "ddlItemDesc";
        const string DV_CONTROL_DELIVERY_BY = "ddlDeliveryBy";
        const string DV_CONTROL_CONFIRMED = "cbxConfirmed";

        const string SV_ORDERVALUES = "Confirmed";

        struct OrderValues
        {
            public int CustomerID;
            public DateTime dtOrder;
            public DateTime dtRoast;
            public DateTime dtRequiredBy;
            //      public int PrefItemID;
            //      public double Qty;
            public int DeliverByID;
            public bool IsConfirmed;
        }

        bool ClientHasChanged;

        private void InitializeOrderVars()
        {
            TrackerTools tt = new classes.TrackerTools();
            DateTime dt = tt.GetClosestNextRoastDate(DateTime.Now);

            OrderValues _MyOrderValues = new OrderValues();

            _MyOrderValues.CustomerID = 0;
            _MyOrderValues.dtOrder = DateTime.Now;
            _MyOrderValues.dtRoast = dt;
            _MyOrderValues.dtRequiredBy = dt.AddDays(1);
            _MyOrderValues.DeliverByID = Convert.ToInt16(ConfigurationManager.AppSettings["DefaultDeliveryID"]);   // mzukisi?
            _MyOrderValues.IsConfirmed = true;

            Session[SV_ORDERVALUES] = _MyOrderValues;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                ClientHasChanged = false;
                if (Session[SV_ORDERVALUES] == null)
                    InitializeOrderVars();
            }
        }
        protected void dvOrderEdit_OnDataBound(object sender, EventArgs e)
        {
            DetailsView myDetailsView = (DetailsView)sender;
            if ((myDetailsView.CurrentMode == DetailsViewMode.Insert) || (myDetailsView.CurrentMode == DetailsViewMode.Edit))
            {
                if (ClientHasChanged)
                {

                    //      QOnT.classes.TrackerTools tt = new classes.TrackerTools();
                    //      DateTime dt = tt.GetClosestNextRoastDate(DateTime.Now);

                    // if there session has lost vars re-initializa
                    //        if (Session[SV_ORDERVALUES] == null)
                    //          InitializeOrderVars();
                    //
                    //        // retrieve values
                    //        OrderValues _MyOrderValues = (OrderValues) Session[SV_ORDERVALUES];
                    //        
                    // take values from session and set them
                    //((DropDownList)myDetailsView.FindControl(DV_CONTROL_CUSTOMER)).SelectedIndex = _MyOrderValues.CustomerID;
                    //((TextBox)myDetailsView.FindControl(DV_CONTROL_ORDER_DATE)).Text = _MyOrderValues.dtOrder.ToString("d");
                    //((TextBox)myDetailsView.FindControl(DV_CONTROL_ROAST_DATE)).Text = _MyOrderValues.dtRoast.ToString("d");
                    //((TextBox)myDetailsView.FindControl(DV_CONTROL_REQUIRED_BY_DATE)).Text = _MyOrderValues.dtRequiredBy.ToString("d");
                    //((DropDownList)myDetailsView.FindControl(DV_CONTROL_DELIVERY_BY)).SelectedIndex = _MyOrderValues.DeliverByID;
                    //((TextBox)myDetailsView.FindControl(DV_CONTROL_QTY)).Text = "1";
                    //((CheckBox)myDetailsView.FindControl(DV_CONTROL_CONFIRMED)).Checked = _MyOrderValues.IsConfirmed;
                }
            }

        }

        public static List<string> AddQtyAutoValues(string prefixText, int count)
        {
            double dQty = 0.25;
            List<string> QtyStrList = new List<string>();

            for (int i = 0; i < 100; i++)
            {
                QtyStrList.Add(Convert.ToString(dQty));
                dQty = dQty + .025;
            }
            return QtyStrList;
        }

        protected void dvOrderEdit_OnItemInserted(object sender, EventArgs e)
        {
            DetailsView myDetailsView = (DetailsView)sender;
            //      QOnT.classes.TrackerTools tt = new classes.TrackerTools();
            //      DateTime dt = tt.GetClosestNextRoastDate(DateTime.Now);

            // retrieve values
            OrderValues _MyOrderValues = new OrderValues();

            // take values from session and set them
            DropDownList _ddlCustomers = ((DropDownList)myDetailsView.FindControl(DV_CONTROL_CUSTOMER));
            _MyOrderValues.CustomerID = _ddlCustomers.SelectedIndex;
            _MyOrderValues.dtOrder = Convert.ToDateTime(((TextBox)myDetailsView.FindControl(DV_CONTROL_ORDER_DATE)).Text);
            _MyOrderValues.dtRoast = Convert.ToDateTime(((TextBox)myDetailsView.FindControl(DV_CONTROL_ROAST_DATE)).Text);
            _MyOrderValues.dtRequiredBy = Convert.ToDateTime(((TextBox)myDetailsView.FindControl(DV_CONTROL_REQUIRED_BY_DATE)).Text);
            _MyOrderValues.DeliverByID = ((DropDownList)myDetailsView.FindControl(DV_CONTROL_DELIVERY_BY)).SelectedIndex;
            _MyOrderValues.IsConfirmed = ((CheckBox)myDetailsView.FindControl(DV_CONTROL_CONFIRMED)).Checked;

            Session[SV_ORDERVALUES] = _MyOrderValues;
            ltrlStatus.Text = "Order added for customer: " + _ddlCustomers.SelectedValue;
        }

        protected void dvOrderEdit_PageIndexChanging(object sender, DetailsViewPageEventArgs e)
        {

        }

        protected void ddlCustomer_SelectedIndexChanged(object sender, EventArgs e)
        {
            TrackerTools tt = new classes.TrackerTools();
            DropDownList _ddlCustomers = (DropDownList)sender;
            // roast day vars
            DateTime _dtNextRoastDay;   // = Convert.ToDateTime(((TextBox)myDetailsView.FindControl(DV_CONTROL_ROAST_DATE)).Text); 
            DateTime _dtDelivery = DateTime.Now;
            // preference vars;
            Int64 _CustID = Convert.ToInt64(_ddlCustomers.SelectedValue);
            //int _preferredDlvryBy = 1; // = ((DropDownList)myDetailsView.FindControl(DV_CONTROL_DELIVERY_BY)).SelectedValue ;
            //int _preferredItem = 1; // = ((DropDownList)myDetailsView.FindControl(DV_CONTROL_ITEMDESC)).SelectedValue;
            //double _preferredQty = 1;

            _dtNextRoastDay = tt.GetNextRoastDateByCustomerID(_CustID, ref _dtDelivery);

            TrackerTools.ContactPreferedItems contactPreferedItems = tt.RetrieveCustomerPrefs(_CustID);
            

            ((TextBox)dvOrderEdit.FindControl(DV_CONTROL_ROAST_DATE)).Text = String.Format("{0:d}", _dtNextRoastDay);
            ((TextBox)dvOrderEdit.FindControl(DV_CONTROL_REQUIRED_BY_DATE)).Text = String.Format("{0:d}", _dtDelivery);

            DropDownList _ddl = (DropDownList)dvOrderEdit.FindControl(DV_CONTROL_DELIVERY_BY);
            int i = 0;
            while ((i < _ddl.Items.Count) && (_ddl.Items[i].Value != contactPreferedItems.PreferredDeliveryByID.ToString()))
            {
                i++;
            }
            if (i < _ddl.Items.Count)
                ((DropDownList)dvOrderEdit.FindControl(DV_CONTROL_DELIVERY_BY)).SelectedIndex = i;

            // ((DropDownList)dvOrderEdit.FindControl(DV_CONTROL_DELIVERY_BY)).SelectedValue = _preferredDlvryBy.ToString();
            //((TextBox)dvOrderEdit.FindControl(DV_CONTROL_QTY)).Text = "1";
            ((DropDownList)dvOrderEdit.FindControl(DV_CONTROL_ITEMDESC)).SelectedValue = contactPreferedItems.PreferedItem.ToString();
            ((TextBox)dvOrderEdit.FindControl(DV_CONTROL_QTY)).Text = String.Format("{0:N}", contactPreferedItems.PreferedQty);

        }

    }
}