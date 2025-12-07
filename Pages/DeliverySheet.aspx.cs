// Type: TrackerSQL.Pages.DeliverySheet
// Assembly: TrackerSQL, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerSQL.dll

using AjaxControlToolkit;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerSQL.Classes;
using TrackerSQL.Controls;

//- only form later versions #nullable disable
namespace TrackerSQL.Pages
{
    public partial class DeliverySheet : Page
    {
        public const string CONST_SESSION_SHEETDATE = "DeliverySheetDate";
        public const string CONST_SESSION_DELIVERTBY = "DeliverySheetDeliveryBy";
        public const string CONST_SESSION_DDLSHEETDATE_SELECTED = "DeliverySheetDateItemSelected";
        public const string CONST_SESSION_DDLDELIVERTBY_SELECTED = "DeliverySheetDeliveryByItemSelected";
        public const string CONST_SESSION_SHEETISPRINTING = "SheetIsPrinting";
        private const int CONST_ONLYAFEWDELIVERIES = 9;
        private const int CONST_ALOTOFDELIVERIES = 21;
        private const string CONST_ZZNAME_PREFIX = "_*:";
        //private const string CONST_IS_ZZNAME = "ZZ";
        protected ScriptManager smDelivery;
        protected Panel pnlDeliveryDate;
        protected UpdateProgress uprgDeliveryFilterBy;
        protected UpdatePanel upnlDeliveryFilterBy;
        protected DropDownList ddlActiveRoastDates;
        protected Button btnGo;
        protected Button btnRefresh;
        protected Label lblDeliveryBy;
        protected DropDownList ddlDeliveryBy;
        protected TextBox tbxFindClient;
        protected Button btnFind;
        protected Button btnPrint;
        protected HyperLink hlAddDeliveryItem;
        protected ObjectDataSource odsActiveRoastDates;
        protected UpdatePanel upnlDeliveryItems;
        protected Table tblDeliveries;
        protected TableHeaderCell thcReceivedBy;
        protected TableHeaderCell thcSignature;
        protected TableHeaderCell thcInStock;
        protected Table tblTotals;
        protected Label ltrlWhichDate;

        private void Page_PreInit(object sender, EventArgs e)
        {
            bool flag1 = false;
            bool flag2 = new CheckBrowser().fBrowserIsMobile();
            this.Session["RunningOnMoble"] = (object)flag2;
            if (this.Request.QueryString["Print"] != null)
                flag1 = this.Request.QueryString["Print"].ToString() == "Y";
            if (flag1)
            {
                this.MasterPageFile = "~/Print.master";
                this.Session[CONST_SESSION_SHEETISPRINTING] = (object)"Y";
            }
            else
            {
                //this.Session["RunningOnMoble"] = (object)flag2;
                this.MasterPageFile = "~/Site.master";
                this.Session[CONST_SESSION_SHEETISPRINTING] = (object)"N";
            }
        }

        protected void PageInitialize(bool pPrintForm)
        {
            this.btnPrint.Visible = !pPrintForm;
            this.pnlDeliveryDate.Visible = !pPrintForm;
            this.ltrlWhichDate.Visible = !pPrintForm;
            string pActiveDeliveryDate = this.Request.QueryString["DateValue"] == null ? "" : this.Request.QueryString["DateValue"];
            string pOnlyDeliveryBy = this.Request.QueryString["DeliveryBy"] == null ? "" : this.Request.QueryString["DeliveryBy"];
            if (string.IsNullOrEmpty(pActiveDeliveryDate) && this.Session[CONST_SESSION_SHEETDATE] != null)
            {
                pActiveDeliveryDate = (string)this.Session[CONST_SESSION_SHEETDATE];
                this.ltrlWhichDate.Text = pActiveDeliveryDate;
            }
            if (string.IsNullOrEmpty(pOnlyDeliveryBy) && this.Session[CONST_SESSION_DELIVERTBY] != null)
                pOnlyDeliveryBy = (string)this.Session[CONST_SESSION_DELIVERTBY];
            if (string.IsNullOrEmpty(pActiveDeliveryDate))
                return;
            this.BuildDeliverySheet(pPrintForm, pActiveDeliveryDate, pOnlyDeliveryBy);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            bool pPrintForm = this.Request.QueryString["Print"] != null && this.Request.QueryString["Print"].ToString() == "Y";
            if (this.Session[CONST_SESSION_DDLSHEETDATE_SELECTED] == null)
                this.Session[CONST_SESSION_DDLSHEETDATE_SELECTED] = (object)$"{TimeZoneUtils.Now().Date}";
            if (!this.IsPostBack)
            {
                Button control = (Button)this.pnlDeliveryDate.FindControl("btnFind");
                if (control != null)
                    this.Form.DefaultButton = control.UniqueID;
                this.PageInitialize(pPrintForm);
            }
            if (pPrintForm)
                return;
            this.tblDeliveries.Rows[0].Cells[2].Visible = false;
            this.tblDeliveries.Rows[0].Cells[3].Visible = false;
            this.tblDeliveries.Rows[0].Cells[5].Visible = false;
            if ((bool)this.Session["RunningOnMoble"])
                return;
            TableCellCollection cells = this.tblDeliveries.Rows[0].Cells;
            TableHeaderCell tableHeaderCell = new TableHeaderCell();
            tableHeaderCell.Text = "Action";
            TableHeaderCell cell = tableHeaderCell;
            cells.Add((TableCell)cell);
        }

        protected void BuildDeliverySheet()
        {
            if (this.ltrlWhichDate.Text.Length > 0)
            {
                string text = this.ltrlWhichDate.Text;
            }
            this.BuildDeliverySheet(false, this.ltrlWhichDate.Text.Length > 0 ? this.ltrlWhichDate.Text : "2012-01-01", this.ddlDeliveryBy.Items.Count <= 1 || this.ddlDeliveryBy.SelectedIndex <= 0 ? "" : this.ddlDeliveryBy.SelectedValue);
        }

        protected void BuildDeliverySheet(bool pPrintForm, string pActiveDeliveryDate, string pOnlyDeliveryBy)
        {
            this.Session[CONST_SESSION_DELIVERTBY] = pOnlyDeliveryBy;

            // Complete SQL with all columns
            string strSQL = @"SELECT DISTINCT OrdersTbl.OrderID, CustomersTbl.CompanyName AS CoName, OrdersTbl.CustomerID, " +
                              "OrdersTbl.OrderDate, OrdersTbl.RoastDate,OrdersTbl.ItemTypeID, ItemTypeTbl.ItemDesc, " +
                              "OrdersTbl.QuantityOrdered, ItemTypeTbl.ItemShortName, ItemTypeTbl.ItemEnabled, " +
                              "ItemTypeTbl.ReplacementID,  CityPrepDaysTbl.DeliveryOrder,  ItemTypeTbl.SortOrder, " +
                              "OrdersTbl.RequiredByDate, OrdersTbl.ToBeDeliveredBy, OrdersTbl.PurchaseOrder, OrdersTbl.Confirmed," +
                              "OrdersTbl.InvoiceDone, OrdersTbl.Done, OrdersTbl.Notes, PackagingTbl.Description AS PackDesc, " +
                              "PackagingTbl.BGColour, PersonsTbl.Abbreviation " +
                              "FROM ( ( " +
                                       "( CityPrepDaysTbl RIGHT OUTER JOIN CustomersTbl ON CityPrepDaysTbl.CityID = CustomersTbl.City )" +
                                        "RIGHT OUTER JOIN " +
                                        "( OrdersTbl LEFT OUTER JOIN PersonsTbl ON OrdersTbl.ToBeDeliveredBy = PersonsTbl.PersonID)" +
                                        " ON CustomersTbl.CustomerID = OrdersTbl.CustomerID" +
                                      ") LEFT OUTER JOIN PackagingTbl ON OrdersTbl.PackagingID = PackagingTbl.PackagingID) " +
                                    " LEFT OUTER JOIN ItemTypeTbl ON OrdersTbl.ItemTypeID = ItemTypeTbl.ItemTypeID " +
                              "WHERE (OrdersTbl.RequiredByDate = ?)";

            // Add date parameter using utility method
            if (!DateTime.TryParse(pActiveDeliveryDate, out DateTime reqDate))
            {
                reqDate = DateTime.Today;
            }
            using (TrackerDb trackerDb = new TrackerDb())
            {

                trackerDb.AddWhereParams(reqDate, DbType.DateTime);

                // Add delivery person filter if specified
                if (!string.IsNullOrEmpty(pOnlyDeliveryBy))
                {
                    strSQL += " AND OrdersTbl.ToBeDeliveredBy = ?";

                    if (int.TryParse(pOnlyDeliveryBy, out int deliveryById))
                    {
                        trackerDb.AddWhereParams(deliveryById, DbType.Int32);
                    }
                    else
                    {
                        trackerDb.AddWhereParams(DBNull.Value, DbType.Int32);
                    }
                }

                // Complete SQL
                strSQL += @" ORDER BY OrdersTbl.RequiredByDate, OrdersTbl.ToBeDeliveredBy, CityPrepDaysTbl.DeliveryOrder," +
                            "CustomersTbl.CompanyName, ItemTypeTbl.SortOrder";

                // Execute the query
                using (IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(strSQL))
                {
                    this.BuildDeliveryTable(dataReader, pPrintForm);
                }
                // Explicit close still happens through TrackerDb.Dispose()
            }

            //IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(strSQL);
            //this.BuildDeliveryTable(dataReader, pPrintForm);
            //dataReader.Close();
            //trackerDb.Close();
        }

        private string StripEmailOut(string pNotes)
        {
            int length = pNotes.IndexOf("[#");
            if (length >= 0)
            {
                int num = pNotes.IndexOf("#]");
                if (num >= 0)
                    pNotes = $"{pNotes.Substring(0, length)};{pNotes.Substring(num + 2)}";
            }
            return pNotes;
        }
        private void BuildDeliveryTable(IDataReader pDataReader, bool pPrintForm)
        {
            // Clear previous table rows and totals
            while (1 < this.tblDeliveries.Rows.Count)
                this.tblDeliveries.Rows.RemoveAt(1);
            this.tblTotals.Rows.Clear();

            // Prepare collections for delivery items, delivery by, and item totals
            var deliveryItemsList = new List<deliveryItems>();
            var sortedDictionary = new SortedDictionary<string, string>();
            var itemTotals = new Dictionary<string, ItemTotals>();

            // 1. Read all delivery items from the data reader
            ReadDeliveryItems(pDataReader, deliveryItemsList, sortedDictionary, itemTotals, pPrintForm);

            // 2. Reorder sundry items for display
            ReorderSundryItems(deliveryItemsList);

            // 3. Add delivery rows to the table
            AddDeliveryRows(deliveryItemsList, pPrintForm);

            // 4. Style the table rows for readability
            ApplyRowStyles();

            // 5. Build the totals summary table
            BuildTotalsTable(itemTotals);

            // 6. Update the delivery by dropdown if not printing
            if (!pPrintForm)
                UpdateDeliveryByDropdown(sortedDictionary);

            // 7. Update the UI panel
            this.upnlDeliveryItems.Update();
        }

        /// <summary>
        /// Reads delivery items from the data reader and populates collections.
        /// </summary>
        private void ReadDeliveryItems(
            IDataReader pDataReader,
            List<deliveryItems> deliveryItemsList,
            SortedDictionary<string, string> sortedDictionary,
            Dictionary<string, ItemTotals> itemTotals,
            bool pPrintForm)
        {
            string[] strArray = new string[8] { "", "dN", "d#", "g$", "cS", "s@", "!!", "??" };
            CustomersAccInfoTbl customersAccInfoTbl = new CustomersAccInfoTbl();
            string str1 = "";

            while (pDataReader.Read())
            {
                bool flag = false;
                deliveryItems deliveryItems1 = new deliveryItems();
                deliveryItems1.ContactID = pDataReader["CustomerID"].ToString();
                deliveryItems1.ContactCompany = pDataReader["CoName"].ToString();

                // Sundry customer handling
                if (deliveryItems1.ContactCompany.StartsWith(SystemConstants.CustomerConstants.SundryCustomerName))
                {
                    deliveryItems1.ContactID = SystemConstants.CustomerConstants.SundryCustomerNamePrefix;
                    flag = true;
                    string pNotes = pDataReader["Notes"].ToString();
                    if (pNotes.Contains(":"))
                        pNotes = pNotes.Remove(pNotes.IndexOf(":")).Trim();
                    string str2 = this.StripEmailOut(pNotes);
                    deliveryItems1.ContactCompany = "_*: " + str2;
                }

                // Notes handling
                if (pDataReader["Notes"].ToString().StartsWith("+"))
                {
                    deliveryItems1.ContactCompany = $"{deliveryItems1.ContactCompany}[{pDataReader["Notes"].ToString()}]";
                }

                // Invoice type prefix
                if (!deliveryItems1.ContactID.Equals(SystemConstants.CustomerConstants.SundryCustomerNamePrefix))
                {
                    long result = 0;
                    if (long.TryParse(deliveryItems1.ContactID, out result))
                    {
                        int customersInvoiceType = customersAccInfoTbl.GetCustomersInvoiceType(result);
                        if (customersInvoiceType > 1)
                            deliveryItems1.ContactCompany = $"{strArray[customersInvoiceType - 1]}]> {deliveryItems1.ContactCompany}";
                    }
                }

                // Done status
                deliveryItems1.Done = pDataReader["Done"] != DBNull.Value && (bool)pDataReader["Done"];
                if (deliveryItems1.Done)
                    deliveryItems1.ContactCompany = "<b>DONE</b>-> " + deliveryItems1.ContactCompany;

                // Delivery by dictionary
                if (!pPrintForm)
                {
                    if (!sortedDictionary.ContainsKey(pDataReader["ToBeDeliveredBy"].ToString()))
                        sortedDictionary[pDataReader["ToBeDeliveredBy"].ToString()] = pDataReader["Abbreviation"].ToString();
                    deliveryItems1.OrderDetailURL = $"{this.ResolveUrl("~/Pages/OrderDetail.aspx")}?CustomerID={HttpContext.Current.Server.UrlEncode(pDataReader["CustomerID"].ToString())}&DeliveryDate={pDataReader["RequiredByDate"]:d}&Notes={HttpContext.Current.Server.UrlEncode(pDataReader["Notes"].ToString())}";
                }

                deliveryItems1.Details = $"{pDataReader["RequiredByDate"]:d}, {pDataReader["Abbreviation"]}";
                deliveryItems1.InvoiceDone = pDataReader["InvoiceDone"] != DBNull.Value && (bool)pDataReader["InvoiceDone"];
                deliveryItems1.PurchaseOrder = pDataReader["PurchaseOrder"] == DBNull.Value ? string.Empty : pDataReader["PurchaseOrder"].ToString();

                // Item details and totals
                string key = pDataReader["ItemTypeID"].ToString();
                string str3 = pDataReader["ItemShortName"].ToString().Length > 0 ? pDataReader["ItemShortName"].ToString() : pDataReader["ItemDesc"].ToString();
                string str4 = str3;
                if (!bool.Parse(pDataReader["ItemEnabled"].ToString()))
                {
                    str3 = "<span style='background-color: RED; color: WHITE'>SOLD OUT</span> " + str3;
                    str4 = $">{str4}<";
                }
                int num2 = pDataReader["SortOrder"] == DBNull.Value ? 0 : (int)pDataReader["SortOrder"];
                if (num2 == 10)
                {
                    string pNotes = pDataReader["Notes"].ToString();
                    if (flag && pNotes.Contains(":"))
                        pNotes = pNotes.Substring(pNotes.IndexOf(":") + 1).Trim();
                    string str5 = this.StripEmailOut(pNotes);
                    str3 = $"{str3}: {str5}";
                }
                if (pDataReader["PackDesc"].ToString().Length > 0)
                    deliveryItems1.Items += $"<span style='background-color:{pDataReader["BGColour"]}; padding-top: 1px; padding-bottom:2px'>{pDataReader["QuantityOrdered"]}X{str3} ({pDataReader["PackDesc"]})</span>";
                else
                    deliveryItems1.Items += $"<span style='background-color:{pDataReader["BGColour"]}'>{pDataReader["QuantityOrdered"]}X{str3}</span>";

                // Totals calculation
                if (num2 != 10)
                {
                    if (itemTotals.ContainsKey(key))
                    {
                        itemTotals[key].TotalsQty += Convert.ToDouble(pDataReader["QuantityOrdered"]);
                    }
                    else
                    {
                        if (str3.Contains(":"))
                            str1 = str3.Remove(str3.IndexOf(":"));
                        itemTotals[key] = new ItemTotals()
                        {
                            ItemID = key,
                            ItemDesc = str4,
                            TotalsQty = Convert.ToDouble(pDataReader["QuantityOrdered"].ToString()),
                            ItemOrder = pDataReader["SortOrder"] == DBNull.Value ? 0 : Convert.ToInt32(pDataReader["SortOrder"].ToString())
                        };
                    }
                }
                deliveryItemsList.Add(deliveryItems1);
            }
            pDataReader.Close();
        }

        /// <summary>
        /// Reorders sundry items in the delivery list for display.
        /// </summary>
        private void ReorderSundryItems(List<deliveryItems> deliveryItemsList)
        {
            int num1 = deliveryItemsList.Count;
            for (int index1 = 0; index1 < num1; ++index1)
            {
                if (deliveryItemsList[index1].ContactCompany.StartsWith("_*:"))
                {
                    for (int index2 = index1 + 2; index2 < num1; ++index2)
                    {
                        if (deliveryItemsList[index2].ContactCompany.Equals(deliveryItemsList[index1].ContactCompany))
                        {
                            deliveryItems item = deliveryItemsList[index2];
                            deliveryItemsList.RemoveAt(index2);
                            deliveryItemsList.Insert(index1 + 1, item);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Adds delivery rows to the deliveries table.
        /// </summary>
        private void AddDeliveryRows(List<deliveryItems> deliveryItemsList, bool pPrintForm)
        {
            int index = 0;
            int num1 = deliveryItemsList.Count;
            while (index < num1)
            {
                TableRow row = new TableRow();

                // Details cell
                TableCell cellDetails = new TableCell();
                cellDetails.Text = deliveryItemsList[index].Details;
                if (pPrintForm)
                {
                    cellDetails.Font.Size = FontUnit.XSmall;
                    cellDetails.Text = cellDetails.Text.Remove(0, cellDetails.Text.IndexOf(",") + 1);
                }
                else
                    cellDetails.Text = $"<a class='plain' href='{deliveryItemsList[index].OrderDetailURL}'>{cellDetails.Text.Trim()}</a>";
                row.Cells.Add(cellDetails);

                // Company cell
                TableCell cellCompany = new TableCell();
                if (pPrintForm)
                {
                    string str6 = deliveryItemsList[index].ContactCompany;
                    if (str6.Contains("]>"))
                    {
                        int num3 = str6.IndexOf("]>");
                        str6 = str6.Substring(num3 + 3);
                    }
                    cellCompany.Text = str6;
                }
                else if (deliveryItemsList[index].ContactID == SystemConstants.CustomerConstants.SundryCustomerNamePrefix)
                {
                    cellCompany.Text = deliveryItemsList[index].ContactCompany;
                }
                else
                {
                    string contactCompany = deliveryItemsList[index].ContactCompany;
                    if (contactCompany.Contains("]>"))
                    {
                        int length = contactCompany.IndexOf("]>");
                        cellCompany.Text = $"{contactCompany.Substring(0, length)} - <a href='./CustomerDetails.aspx?ID={deliveryItemsList[index].ContactID}&'>{contactCompany.Substring(length + 3)}</a>";
                    }
                    else
                        cellCompany.Text = $"<a href='./CustomerDetails.aspx?ID={deliveryItemsList[index].ContactID}&'>{contactCompany}</a>";
                    cellCompany.CssClass = "wordwrap"; // Enable wrapping for long company names
                }
                row.Cells.Add(cellCompany);

                // ReceivedBy/Signature cells for print
                if (pPrintForm)
                {
                    TableCell cellReceivedBy = new TableCell();
                    cellReceivedBy.BorderStyle = BorderStyle.Solid;
                    cellReceivedBy.BorderWidth = Unit.Pixel(1);
                    cellReceivedBy.BorderColor = Color.Green;
                    row.Cells.Add(cellReceivedBy);

                    TableCell cellSignature = new TableCell();
                    cellSignature.BorderStyle = BorderStyle.Solid;
                    cellSignature.BorderWidth = Unit.Pixel(1);
                    cellSignature.BorderColor = Color.Green;
                    row.Cells.Add(cellSignature);
                }

                // Items cell
                TableCell cellItems = new TableCell();
                if (!string.IsNullOrWhiteSpace(deliveryItemsList[index].PurchaseOrder))
                    cellItems.Text = $"<b>[PO: {deliveryItemsList[index].PurchaseOrder}]</b>";
                if (!pPrintForm && deliveryItemsList[index].InvoiceDone)
                {
                    cellItems.Text = $"{cellItems.Text}{(string.IsNullOrEmpty(cellItems.Text) ? "" : " ")}<span style='background-color:green; color: white'>$Invcd$</span>";
                }
                string format = "<span  style='vertical-align:middle'> <a  href='{0}' class='plain'><img src='../images/imgButtons/EditButton.gif' alt='edit' /></a>";
                if (!deliveryItemsList[index].InvoiceDone)
                    format += "&nbsp<a href='{0}&Invoiced=Y' class='plain'><img src='../images/imgButtons/InvoicedButton.gif' alt='invcd' /></a></span>";
                if (!deliveryItemsList[index].Done)
                    format += "&nbsp<a href='{0}&Delivered=Y' class='plain'><img src='../images/imgButtons/DoneButton.gif' alt='dlvrd' /></a></span>";
                string str7 = string.Format(format, deliveryItemsList[index].OrderDetailURL);

                // Add all items for the same company
                do
                {
                    cellItems.Text = cellItems.Text + (string.IsNullOrEmpty(cellItems.Text) ? "" : "; ") + deliveryItemsList[index].Items.ToString();
                    ++index;
                }
                while (index < num1 && deliveryItemsList[index - 1].ContactCompany == deliveryItemsList[index].ContactCompany);

                row.Cells.Add(cellItems);

                // Extra cell for print
                if (pPrintForm)
                    row.Cells.Add(new TableCell());

                // Action cell for non-mobile, non-print
                bool flag = (bool)this.Session["RunningOnMoble"];
                if (!pPrintForm && !flag)
                    row.Cells.Add(new TableCell() { Text = str7 });

                this.tblDeliveries.Rows.Add(row);
            }
        }

        /// <summary>
        /// Applies height and font styles to the first column of each row.
        /// </summary>
        private void ApplyRowStyles()
        {
            Style s = new Style();
            if (this.tblDeliveries.Rows.Count < CONST_ONLYAFEWDELIVERIES)
                s.Height = new Unit(4.5, UnitType.Em);
            else if (this.tblDeliveries.Rows.Count > CONST_ALOTOFDELIVERIES)
            {
                s.Height = new Unit(0.3, UnitType.Em);
                s.Font.Size = new FontUnit(11.0, UnitType.Pixel);
            }
            else
                s.Height = new Unit(2.0, UnitType.Em);

            foreach (TableRow row in this.tblDeliveries.Rows)
                row.Cells[0].ApplyStyle(s);

            this.tblDeliveries.Rows[0].Cells[1].Text = $"To ({this.tblDeliveries.Rows.Count - 1})";
        }

        /// <summary>
        /// Builds the summary totals table.
        /// </summary>
        private void BuildTotalsTable(Dictionary<string, ItemTotals> itemTotals)
        {
            var dictionary = itemTotals.OrderBy(entry => entry.Value.ItemOrder)
                .ToDictionary(pair => pair.Key, pair => pair.Value);

            TableRow summaryHeaderRow = new TableHeaderRow();
            TableRow summaryItemsRow = new TableRow();
            TableHeaderCell cellHeader = new TableHeaderCell();
            cellHeader.Text = "Item";
            cellHeader.Font.Bold = true;
            summaryHeaderRow.Cells.Add(cellHeader);

            TableCell cellTotal = new TableCell();
            cellTotal.Text = "Total";
            cellTotal.Font.Bold = true;
            summaryItemsRow.Cells.Add(cellTotal);

            foreach (var keyValuePair in dictionary)
            {
                TableHeaderCell cellItem = new TableHeaderCell();
                cellItem.Text = keyValuePair.Value.ItemDesc;
                cellItem.Font.Bold = true;
                summaryHeaderRow.Cells.Add(cellItem);

                summaryItemsRow.Cells.Add(new TableCell()
                {
                    Text = $"{keyValuePair.Value.TotalsQty:0.00}",
                    HorizontalAlign = HorizontalAlign.Right
                });
            }
            this.tblTotals.Rows.Add(summaryHeaderRow);
            this.tblTotals.Rows.Add(summaryItemsRow);
        }

        /// <summary>
        /// Updates the delivery by dropdown and label visibility.
        /// </summary>
        private void UpdateDeliveryByDropdown(SortedDictionary<string, string> sortedDictionary)
        {
            bool flag = sortedDictionary.Count > 1;
            this.ddlDeliveryBy.Items.Clear();
            this.ddlDeliveryBy.Visible = flag;
            this.lblDeliveryBy.Visible = flag;
            if (flag)
            {
                this.ddlDeliveryBy.Items.Add(new ListItem()
                {
                    Text = "--- All ---",
                    Value = "%",
                    Selected = true
                });
                foreach (var keyValuePair in sortedDictionary)
                    this.ddlDeliveryBy.Items.Add(new ListItem()
                    {
                        Text = keyValuePair.Value,
                        Value = keyValuePair.Key
                    });
            }
        }
        // old method all in one not very SOLID
        //private void BuildDeliveryTable(IDataReader pDataReader, bool pPrintForm)
        //{
        //    while (1 < this.tblDeliveries.Rows.Count)
        //        this.tblDeliveries.Rows.RemoveAt(1);
        //    this.tblTotals.Rows.Clear();
        //    List<DeliverySheet.deliveryItems> deliveryItemsList = new List<DeliverySheet.deliveryItems>();
        //    SortedDictionary<string, string> sortedDictionary = new SortedDictionary<string, string>();
        //    string str1 = "";
        //    Dictionary<string, DeliverySheet.ItemTotals> source = new Dictionary<string, DeliverySheet.ItemTotals>();
        //    string[] strArray = new string[8] { "", "dN", "d#",  "g$", "cS", "s@", "!!", "??" };
        //    CustomersAccInfoTbl customersAccInfoTbl = new CustomersAccInfoTbl();
        //    int num1 = 0;
        //    while (pDataReader.Read())
        //    {
        //        bool flag = false;
        //        DeliverySheet.deliveryItems deliveryItems1 = new DeliverySheet.deliveryItems();
        //        deliveryItems1.ContactID = pDataReader["CustomerID"].ToString();
        //        deliveryItems1.ContactCompany = pDataReader["CoName"].ToString();
        //        if (deliveryItems1.ContactCompany.StartsWith(SystemConstants.CustomerConstants.SundryCustomerName))
        //        {
        //            deliveryItems1.ContactID = SystemConstants.CustomerConstants.SundryCustomerNamePrefix;
        //            flag = true;
        //            string pNotes = pDataReader["Notes"].ToString();
        //            if (pNotes.Contains(":"))
        //                pNotes = pNotes.Remove(pNotes.IndexOf(":")).Trim();
        //            string str2 = this.StripEmailOut(pNotes);
        //            deliveryItems1.ContactCompany = "_*: " + str2;
        //        }
        //        //else if (deliveryItems1.ContactCompany.StartsWith("Stock"))
        //        //    deliveryItems1.ContactCompany = "STK: " + pDataReader["Notes"].ToString();
        //        if (pDataReader["Notes"].ToString().StartsWith("+"))
        //        {
        //            DeliverySheet.deliveryItems deliveryItems2 = deliveryItems1;
        //            deliveryItems2.ContactCompany = $"{deliveryItems2.ContactCompany}[{pDataReader["Notes"].ToString()}]";
        //        }
        //        if (!deliveryItems1.ContactID.Equals(SystemConstants.CustomerConstants.SundryCustomerNamePrefix))
        //        {
        //            long result = 0;
        //            if (long.TryParse(deliveryItems1.ContactID, out result))
        //            {
        //                int customersInvoiceType = customersAccInfoTbl.GetCustomersInvoiceType(result);
        //                if (customersInvoiceType > 1)
        //                    deliveryItems1.ContactCompany = $"{strArray[customersInvoiceType - 1]}]> {deliveryItems1.ContactCompany}";
        //            }
        //        }
        //        deliveryItems1.Done = pDataReader["Done"] != DBNull.Value && (bool)pDataReader["Done"];
        //        if (deliveryItems1.Done)
        //            deliveryItems1.ContactCompany = "<b>DONE</b>-> " + deliveryItems1.ContactCompany;
        //        if (!pPrintForm)
        //        {
        //            if (!sortedDictionary.ContainsKey(pDataReader["ToBeDeliveredBy"].ToString()))
        //                sortedDictionary[pDataReader["ToBeDeliveredBy"].ToString()] = pDataReader["Abbreviation"].ToString();
        //            deliveryItems1.OrderDetailURL = $"{this.ResolveUrl("~/Pages/OrderDetail.aspx")}?{$"CustomerID={HttpContext.Current.Server.UrlEncode(pDataReader["CustomerID"].ToString())}&DeliveryDate={pDataReader["RequiredByDate"]:d}&Notes={HttpContext.Current.Server.UrlEncode(pDataReader["Notes"].ToString())}"}";
        //        }
        //        deliveryItems1.Details = $"{pDataReader["RequiredByDate"]:d}, {pDataReader["Abbreviation"]}";
        //        deliveryItems1.InvoiceDone = pDataReader["InvoiceDone"] != DBNull.Value && (bool)pDataReader["InvoiceDone"];
        //        deliveryItems1.PurchaseOrder = pDataReader["PurchaseOrder"] == DBNull.Value ? string.Empty : pDataReader["PurchaseOrder"].ToString();
        //        string key = pDataReader["ItemTypeID"].ToString();
        //        string str3 = pDataReader["ItemShortName"].ToString().Length > 0 ? pDataReader["ItemShortName"].ToString() : pDataReader["ItemDesc"].ToString();
        //        string str4 = str3;
        //        if (!bool.Parse(pDataReader["ItemEnabled"].ToString()))
        //        {
        //            str3 = "<span style='background-color: RED; color: WHITE'>SOLD OUT</span> " + str3;
        //            str4 = $">{str4}<";
        //        }
        //        int num2 = pDataReader["SortOrder"] == DBNull.Value ? 0 : (int)pDataReader["SortOrder"];
        //        if (num2 == 10)
        //        {
        //            string pNotes = pDataReader["Notes"].ToString();
        //            if (flag && pNotes.Contains(":"))
        //                pNotes = pNotes.Substring(pNotes.IndexOf(":") + 1).Trim();
        //            string str5 = this.StripEmailOut(pNotes);
        //            str3 = $"{str3}: {str5}";
        //        }
        //        if (pDataReader["PackDesc"].ToString().Length > 0)
        //            deliveryItems1.Items += $"<span style='background-color:{pDataReader["BGColour"]}; padding-top: 1px; padding-bottom:2px'>{pDataReader["QuantityOrdered"]}X{str3} ({pDataReader["PackDesc"]})</span>";
        //        else
        //            deliveryItems1.Items += $"<span style='background-color:{pDataReader["BGColour"]}'>{pDataReader["QuantityOrdered"]}X{str3}</span>";
        //        if (num2 != 10)
        //        {
        //            if (source.ContainsKey(key))
        //            {
        //                source[key].TotalsQty += Convert.ToDouble(pDataReader["QuantityOrdered"]);
        //            }
        //            else
        //            {
        //                if (str3.Contains(":"))
        //                    str1 = str3.Remove(str3.IndexOf(":"));
        //                source[key] = new DeliverySheet.ItemTotals()
        //                {
        //                    ItemID = key,
        //                    ItemDesc = str4,
        //                    TotalsQty = Convert.ToDouble(pDataReader["QuantityOrdered"].ToString()),
        //                    ItemOrder = pDataReader["SortOrder"] == DBNull.Value ? 0 : Convert.ToInt32(pDataReader["SortOrder"].ToString())
        //                };
        //            }
        //        }
        //        deliveryItemsList.Add(deliveryItems1);
        //        ++num1;
        //    }
        //    pDataReader.Close();
        //    for (int index1 = 0; index1 < num1; ++index1)
        //    {
        //        if (deliveryItemsList[index1].ContactCompany.StartsWith("_*:"))
        //        {
        //            for (int index2 = index1 + 2; index2 < num1; ++index2)
        //            {
        //                if (deliveryItemsList[index2].ContactCompany.Equals(deliveryItemsList[index1].ContactCompany))
        //                {
        //                    DeliverySheet.deliveryItems deliveryItems = deliveryItemsList[index2];
        //                    deliveryItemsList.RemoveAt(index2);
        //                    deliveryItemsList.Insert(index1 + 1, deliveryItems);
        //                }
        //            }
        //        }
        //    }
        //    int index = 0;
        //    while (index < num1)
        //    {
        //        TableRow row = new TableRow();
        //        TableCell cellDetails = new TableCell();
        //        cellDetails.Text = deliveryItemsList[index].Details;
        //        if (pPrintForm)
        //        {
        //            cellDetails.Font.Size = FontUnit.XSmall;
        //            cellDetails.Text = cellDetails.Text.Remove(0, cellDetails.Text.IndexOf(",") + 1);
        //        }
        //        else
        //            cellDetails.Text = $"<a class='plain' href='{deliveryItemsList[index].OrderDetailURL}'>{cellDetails.Text.Trim()}</a>";
        //        row.Cells.Add(cellDetails);
        //        TableCell cellCompany = new TableCell();
        //        if (pPrintForm)
        //        {
        //            string str6 = deliveryItemsList[index].ContactCompany;
        //            if (str6.Contains("]>"))
        //            {
        //                int num3 = str6.IndexOf("]>");
        //                str6 = str6.Substring(num3 + 3);
        //            }
        //            cellCompany.Text = str6;
        //        }
        //        else if (deliveryItemsList[index].ContactID == SystemConstants.CustomerConstants.SundryCustomerNamePrefix)
        //        {
        //            cellCompany.Text = deliveryItemsList[index].ContactCompany;
        //        }
        //        else
        //        {
        //            string contactCompany = deliveryItemsList[index].ContactCompany;
        //            if (contactCompany.Contains("]>"))
        //            {
        //                int length = contactCompany.IndexOf("]>");
        //                cellCompany.Text = $"{contactCompany.Substring(0, length)} - <a href='./CustomerDetails.aspx?ID={deliveryItemsList[index].ContactID}&'>{contactCompany.Substring(length + 3)}</a>";
        //            }
        //            else
        //                cellCompany.Text = $"<a href='./CustomerDetails.aspx?ID={deliveryItemsList[index].ContactID}&'>{contactCompany}</a>";
        //            cellCompany.CssClass = "wordwrap";
        //        }
        //        row.Cells.Add(cellCompany);
        //        if (pPrintForm)
        //        {
        //            TableCell cellReceivedBy = new TableCell();
        //            cellReceivedBy.BorderStyle = BorderStyle.Solid;
        //            cellReceivedBy.BorderWidth = Unit.Pixel(1);
        //            cellReceivedBy.BorderColor = Color.Green;
        //            row.Cells.Add(cellReceivedBy);
        //            TableCell cellSignature = new TableCell();
        //            cellSignature.BorderStyle = BorderStyle.Solid;
        //            cellSignature.BorderWidth = Unit.Pixel(1);
        //            cellSignature.BorderColor = Color.Green;
        //            row.Cells.Add(cellSignature);
        //        }
        //        TableCell cellItems = new TableCell();
        //        if (!string.IsNullOrWhiteSpace(deliveryItemsList[index].PurchaseOrder))
        //            cellItems.Text = $"<b>[PO: {deliveryItemsList[index].PurchaseOrder}]</b>";
        //        if (!pPrintForm && deliveryItemsList[index].InvoiceDone)
        //        {
        //            TableCell tableCell = cellItems;
        //            tableCell.Text = $"{tableCell.Text}{(string.IsNullOrEmpty(cellItems.Text) ? "" : " ")}<span style='background-color:green; color: white'>$Invcd$</span>";
        //        }
        //        string format = "<span  style='vertical-align:middle'> <a  href='{0}' class='plain'><img src='../images/imgButtons/EditButton.gif' alt='edit' /></a>";
        //        if (!deliveryItemsList[index].InvoiceDone)
        //            format += "&nbsp<a href='{0}&Invoiced=Y' class='plain'><img src='../images/imgButtons/InvoicedButton.gif' alt='invcd' /></a></span>";
        //        if (!deliveryItemsList[index].Done)
        //            format += "&nbsp<a href='{0}&Delivered=Y' class='plain'><img src='../images/imgButtons/DoneButton.gif' alt='dlvrd' /></a></span>";
        //        string str7 = string.Format(format, (object)deliveryItemsList[index].OrderDetailURL);
        //        do
        //        {
        //            TableCell tableCell = cellItems;
        //            tableCell.Text = tableCell.Text + (string.IsNullOrEmpty(cellItems.Text) ? "" : "; ") + deliveryItemsList[index].Items.ToString();
        //            ++index;
        //        }
        //        while (index < num1 && deliveryItemsList[index - 1].ContactCompany == deliveryItemsList[index].ContactCompany);
        //        row.Cells.Add(cellItems);
        //        if (pPrintForm)
        //            row.Cells.Add(new TableCell());
        //        bool flag = (bool)this.Session["RunningOnMoble"];
        //        if (!pPrintForm && !flag)
        //            row.Cells.Add(new TableCell() { Text = str7 });
        //        this.tblDeliveries.Rows.Add(row);
        //    }
        //    Style s = new Style();
        //    if (this.tblDeliveries.Rows.Count < CONST_ONLYAFEWDELIVERIES)
        //        s.Height = new Unit(4.5, UnitType.Em);
        //    else if (this.tblDeliveries.Rows.Count > CONST_ALOTOFDELIVERIES)
        //    {
        //        s.Height = new Unit(0.3, UnitType.Em);
        //        s.Font.Size = new FontUnit(11.0, UnitType.Pixel);
        //    }
        //    else
        //        s.Height = new Unit(2.0, UnitType.Em);
        //    foreach (TableRow row in this.tblDeliveries.Rows)
        //        row.Cells[0].ApplyStyle(s);
        //    this.tblDeliveries.Rows[0].Cells[1].Text = $"To ({this.tblDeliveries.Rows.Count - 1})";
        //    Dictionary<string, DeliverySheet.ItemTotals> dictionary = source.OrderBy<KeyValuePair<string, DeliverySheet.ItemTotals>, int>((System.Func<KeyValuePair<string, DeliverySheet.ItemTotals>, int>)(entry => entry.Value.ItemOrder)).ToDictionary<KeyValuePair<string, DeliverySheet.ItemTotals>, string, DeliverySheet.ItemTotals>((System.Func<KeyValuePair<string, DeliverySheet.ItemTotals>, string>)(pair => pair.Key), (System.Func<KeyValuePair<string, DeliverySheet.ItemTotals>, DeliverySheet.ItemTotals>)(pair => pair.Value));
        //    TableRow summaryHeaderRow = (TableRow)new TableHeaderRow();
        //    TableRow summaryItemsRow = new TableRow();
        //    TableHeaderCell cellHeader = new TableHeaderCell();
        //    cellHeader.Text = "Item";
        //    cellHeader.Font.Bold = true;
        //    summaryHeaderRow.Cells.Add((TableCell)cellHeader);
        //    TableCell cellTotal = new TableCell();
        //    cellTotal.Text = "Total";
        //    cellTotal.Font.Bold = true;
        //    summaryItemsRow.Cells.Add(cellTotal);
        //    foreach (KeyValuePair<string, DeliverySheet.ItemTotals> keyValuePair in dictionary)
        //    {
        //        TableHeaderCell cellItem = new TableHeaderCell();
        //        cellItem.Text = keyValuePair.Value.ItemDesc;
        //        cellItem.Font.Bold = true;
        //        summaryHeaderRow.Cells.Add((TableCell)cellItem);
        //        summaryItemsRow.Cells.Add(new TableCell()
        //        {
        //            Text = $"{keyValuePair.Value.TotalsQty:0.00}",
        //            HorizontalAlign = HorizontalAlign.Right
        //        });
        //    }
        //    this.tblTotals.Rows.Add(summaryHeaderRow);
        //    this.tblTotals.Rows.Add(summaryItemsRow);
        //    if (pPrintForm)
        //    {
        //        this.tblTotals.CssClass += " small";
        //    }
        //    else
        //    {
        //        bool flag = sortedDictionary.Count > 1;
        //        this.ddlDeliveryBy.Items.Clear();
        //        this.ddlDeliveryBy.Visible = flag;
        //        this.lblDeliveryBy.Visible = flag;
        //        if (flag)
        //        {
        //            this.ddlDeliveryBy.Items.Add(new ListItem()
        //            {
        //                Text = "--- All ---",
        //                Value = "%",
        //                Selected = true
        //            });
        //            foreach (KeyValuePair<string, string> keyValuePair in sortedDictionary)
        //                this.ddlDeliveryBy.Items.Add(new ListItem()
        //                {
        //                    Text = keyValuePair.Value,
        //                    Value = keyValuePair.Key
        //                });
        //        }
        //    }
        //    this.upnlDeliveryItems.Update();
        //}

        protected void btnPrint_Click(object sender, EventArgs e)
        {
            if (this.ddlActiveRoastDates == null || this.ddlActiveRoastDates.SelectedIndex <= 0)
                return;
            AppLogger.WriteLog("deliverysheet", "Printed delivery sheet");
            this.Session[CONST_SESSION_SHEETDATE] = (object)$"{Convert.ToDateTime(this.ddlActiveRoastDates.SelectedValue):yyyy-MM-dd}";
            this.Response.Redirect("~/Pages/DeliverySheet.aspx?Print=Y");
        }

        protected void ddlActiveRoastDates_SelectedIndexChanged(object sender, EventArgs e)
        {
            Session[CONST_SESSION_DDLSHEETDATE_SELECTED] = ddlActiveRoastDates.SelectedValue;
            this.ltrlWhichDate.Text = $"{Convert.ToDateTime(this.ddlActiveRoastDates.SelectedValue):yyyy-MM-dd}";
            this.Session[CONST_SESSION_SHEETDATE] = (object)this.ltrlWhichDate.Text;
            this.Session[CONST_SESSION_DELIVERTBY] = (object)string.Empty;
            this.Session[CONST_SESSION_DDLDELIVERTBY_SELECTED] = (object)string.Empty;
            this.ltrlWhichDate.Visible = true;
            if (string.IsNullOrEmpty(this.ltrlWhichDate.Text))
                return;
            Button control = (Button)this.pnlDeliveryDate.FindControl("btnGo");
            if (control != null)
                this.Form.DefaultButton = control.UniqueID;
            this.SetVarsAndBuildDeliverySheet();
            AppLogger.WriteLog("deliverysheet", $"Changed roast date to {ddlActiveRoastDates.SelectedValue}");
        }
        protected void tbCalendarDate_TextChanged(object sender, EventArgs e)
        {
            string selectedDate = tbCalendarDate.Text.Trim();
            if (DateTime.TryParse(selectedDate, out DateTime dt))
            {
                string value = dt.ToString("yyyy-MM-dd");
                var item = ddlActiveRoastDates.Items.FindByValue(value);
                if (item == null)
                {
                    // Add new date to dropdown (insert after the first item)
                    ddlActiveRoastDates.Items.Insert(1, new ListItem(dt.ToString("dd-MMM-yyyy (ddd)"), value));
                    ddlActiveRoastDates.SelectedIndex = 1;
                }
                else
                {
                    ddlActiveRoastDates.ClearSelection();
                    item.Selected = true;
                }
                // Update session and UI
                ddlActiveRoastDates_SelectedIndexChanged(ddlActiveRoastDates, EventArgs.Empty);
            }
        }
        protected void ddlActiveRoastDates_DataBound(object sender, EventArgs e)
        {
            // Only set the selected value if not a postback
            if (!IsPostBack)
            {
                string str1 = Session[CONST_SESSION_DDLSHEETDATE_SELECTED] != null ? ((string)Session[CONST_SESSION_DDLSHEETDATE_SELECTED]).Trim() : "";
                if (!string.IsNullOrEmpty(str1) && ddlActiveRoastDates.Items.FindByValue(str1) != null)
                {
                    ddlActiveRoastDates.SelectedValue = str1;
                }
                SetVarsAndBuildDeliverySheet();
            }
            
        }
            //bool flag = false;
            //string str1 = this.Session[CONST_SESSION_DDLSHEETDATE_SELECTED] != null ? ((string)this.Session[CONST_SESSION_DDLSHEETDATE_SELECTED]).Trim() : "";
            //string str2 = this.Session[CONST_SESSION_DDLDELIVERTBY_SELECTED] != null ? (string)this.Session[CONST_SESSION_DDLDELIVERTBY_SELECTED] : "";
            //if (!string.IsNullOrEmpty(str1) && this.ddlActiveRoastDates.Items.FindByValue(str1) != null)
            //{
            //    this.ddlActiveRoastDates.SelectedValue = str1;
            //    flag = true;
            //}
            //if (!string.IsNullOrEmpty(str2) && this.ddlDeliveryBy.Items.FindByValue(str2) != null)
            //{
            //    this.ddlDeliveryBy.SelectedValue = str2;
            //    flag = true;
            //}
            //if (!flag)
            //    return;
            //this.SetVarsAndBuildDeliverySheet();
        //}

        protected void ddlDeliveryBy_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.Session[CONST_SESSION_DDLDELIVERTBY_SELECTED] = (object)this.ddlDeliveryBy.SelectedValue;
            this.BuildDeliverySheet();
        }

        protected void btnRefresh_Click(object sender, EventArgs e)
        {
            this.Session[CONST_SESSION_DELIVERTBY] = (object)string.Empty;
            this.Session[CONST_SESSION_DDLDELIVERTBY_SELECTED] = (object)string.Empty;
            this.Session[CONST_SESSION_SHEETDATE] = (object)string.Empty;
            this.Response.Redirect("DeliverySheet.aspx");
        }

        protected void SetVarsAndBuildDeliverySheet()
        {
            this.ltrlWhichDate.Text = $"{Convert.ToDateTime(this.ddlActiveRoastDates.SelectedValue):yyyy-MM-dd}";
            this.Session[CONST_SESSION_SHEETDATE] = (object)this.ltrlWhichDate.Text;
            this.BuildDeliverySheet();
        }

        protected void btnGo_Click(object sender, EventArgs e)
        {
            if (this.ddlActiveRoastDates == null || this.ddlActiveRoastDates.SelectedIndex <= 0)
                return;
            this.SetVarsAndBuildDeliverySheet();
        }

        protected void btnFind_Click(object sender, EventArgs e)
        {
            AppLogger.WriteLog("deliverysheet", $"Searched for client: {tbxFindClient.Text}");
            string strSQL = $"SELECT DISTINCT OrdersTbl.OrderID, CustomersTbl.CompanyName AS CoName, OrdersTbl.CustomerID, OrdersTbl.OrderDate, OrdersTbl.RoastDate, OrdersTbl.ItemTypeID, ItemTypeTbl.ItemDesc, OrdersTbl.QuantityOrdered, ItemTypeTbl.ItemShortName, ItemTypeTbl.ItemEnabled, ItemTypeTbl.ReplacementID,  CityPrepDaysTbl.DeliveryOrder,  ItemTypeTbl.SortOrder, OrdersTbl.RequiredByDate, OrdersTbl.ToBeDeliveredBy, OrdersTbl.PurchaseOrder, OrdersTbl.Confirmed, OrdersTbl.InvoiceDone, OrdersTbl.Done, OrdersTbl.Notes, PackagingTbl.Description AS PackDesc, PackagingTbl.BGColour, PersonsTbl.Abbreviation FROM ((((CityPrepDaysTbl RIGHT OUTER JOIN CustomersTbl ON CityPrepDaysTbl.CityID = CustomersTbl.City) RIGHT OUTER JOIN  (OrdersTbl LEFT OUTER JOIN PersonsTbl ON OrdersTbl.ToBeDeliveredBy = PersonsTbl.PersonID) ON CustomersTbl.CustomerID = OrdersTbl.CustomerID) LEFT OUTER JOIN   PackagingTbl ON OrdersTbl.PackagingID = PackagingTbl.PackagingID) LEFT OUTER JOIN ItemTypeTbl ON OrdersTbl.ItemTypeID = ItemTypeTbl.ItemTypeID) WHERE (CustomersTbl.CompanyName LIKE '%{this.tbxFindClient.Text}%') AND (OrdersTbl.Done = false) ORDER BY OrdersTbl.RequiredByDate, OrdersTbl.ToBeDeliveredBy, CityPrepDaysTbl.DeliveryOrder, CustomersTbl.CompanyName, ItemTypeTbl.SortOrder";
            TrackerDb trackerDb = new TrackerDb();
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(strSQL);
            this.BuildDeliveryTable(dataReader, false);
            dataReader.Close();
            trackerDb.Close();
        }

        protected void tbxFindClient_OnTextChanged(object sender, EventArgs e)
        {
            Button control = (Button)this.pnlDeliveryDate.FindControl("btnFind");
            if (control != null)
                this.Form.DefaultButton = control.UniqueID;
            this.btnFind_Click(sender, e);
        }

        private class deliveryItems
        {
            private string _ContactID;
            private string _ContactCompany;
            private string _Details;
            private string _PurchaseOrder;
            private bool _Done;
            private bool _InvoiceDone;
            private string _Items;
            private string _OrderDetailURL;

            public deliveryItems()
            {
                this._ContactID = this._ContactCompany = this._Details = this._PurchaseOrder = this._Items = this._OrderDetailURL = string.Empty;
                this._Done = this._InvoiceDone = false;
            }

            public string ContactID
            {
                get => this._ContactID;
                set => this._ContactID = value;
            }

            public string ContactCompany
            {
                get => this._ContactCompany;
                set => this._ContactCompany = value;
            }

            public string Details
            {
                get => this._Details;
                set => this._Details = value;
            }

            public string PurchaseOrder
            {
                get => this._PurchaseOrder;
                set => this._PurchaseOrder = value;
            }

            public bool Done
            {
                get => this._Done;
                set => this._Done = value;
            }

            public bool InvoiceDone
            {
                get => this._InvoiceDone;
                set => this._InvoiceDone = value;
            }

            public string Items
            {
                get => this._Items;
                set => this._Items = value;
            }

            public string OrderDetailURL
            {
                get => this._OrderDetailURL;
                set => this._OrderDetailURL = value;
            }
        }

        private class ItemTotals
        {
            public string ItemID { get; set; }

            public string ItemDesc { get; set; }

            public double TotalsQty { get; set; }

            public int ItemOrder { get; set; }
        }
    }
}