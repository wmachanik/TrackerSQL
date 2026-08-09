using System;
using System.Collections.Generic;
using System.Drawing;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerSQL.Classes;
using TrackerSQL.Managers;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

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

        protected ScriptManager smDelivery;
        protected Panel pnlDeliveryShell;
        protected Panel pnlDeliveryDate;
        protected UpdateProgress uprgDelivery;
        protected DropDownList ddlActivePrepDates;
        protected Button btnGo;
        protected Button btnRefresh;
        protected Button btnBack;
        protected Label lblDeliveryBy;
        protected DropDownList ddlDeliveryBy;
        protected TextBox tbxFindClient;
        protected Button btnFind;
        protected Button btnPrint;
        protected HyperLink hlAddDeliveryItem;
        protected UpdatePanel upnlDeliveryItems;
        protected Table tblDeliveries;
        protected TableHeaderCell thcReceivedBy;
        protected TableHeaderCell thcSignature;
        protected TableHeaderCell thcInStock;
        protected Table tblTotals;
        protected Label ltrlWhichDate;
        protected System.Web.UI.HtmlControls.HtmlGenericControl pnlStatus;
        protected Literal ltrlStatus;

        /*
         * NOTE:
         * If tbCalendarDate is declared in DeliverySheet.aspx.designer.cs,
         * do not declare it here as well.
         *
         * If this file does not compile because tbCalendarDate is missing,
         * uncomment the line below.
         */
        // protected TextBox tbCalendarDate;

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

            // Print uses Print.master and must stay plain (no page-tone chrome).
            if (this.pnlDeliveryShell != null)
            {
                this.pnlDeliveryShell.CssClass = pPrintForm
                    ? string.Empty
                    : "simpleForm page-tone-panel page-tone-delivery";
            }

            string pActiveDeliveryDate = this.Request.QueryString["DateValue"] == null
                ? ""
                : this.Request.QueryString["DateValue"];

            string pOnlyDeliveryBy = this.Request.QueryString["DeliveryBy"] == null
                ? ""
                : this.Request.QueryString["DeliveryBy"];

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
            bool pPrintForm =
                this.Request.QueryString["Print"] != null &&
                this.Request.QueryString["Print"].ToString() == "Y";

            if (this.Session[CONST_SESSION_DDLSHEETDATE_SELECTED] == null)
                this.Session[CONST_SESSION_DDLSHEETDATE_SELECTED] = TimeZoneUtils.Now().Date.ToString("yyyy-MM-dd");

            if (!this.IsPostBack)
            {
                // Enter key should load the selected delivery date (Go), not Find.
                // Defaulting to Find caused date AutoPostBack / Go to be overwritten by a
                // contact search whenever the Find box had leftover text.
                if (this.btnGo != null)
                    this.Form.DefaultButton = this.btnGo.UniqueID;

                this.BindActivePrepDates();
                this.PageInitialize(pPrintForm);
            }

            if (pPrintForm)
                return;

            // Hide print-only columns on the normal page.
            if (this.tblDeliveries.Rows.Count > 0 && this.tblDeliveries.Rows[0].Cells.Count > 5)
            {
                this.tblDeliveries.Rows[0].Cells[2].Visible = false;
                this.tblDeliveries.Rows[0].Cells[3].Visible = false;
                this.tblDeliveries.Rows[0].Cells[5].Visible = false;
            }

            if (this.Session["RunningOnMoble"] != null && (bool)this.Session["RunningOnMoble"])
                return;

            // Add an Action header for non-mobile, non-print views.
            if (this.tblDeliveries.Rows.Count > 0)
            {
                TableCellCollection cells = this.tblDeliveries.Rows[0].Cells;

                if (cells.Count == 0 || cells[cells.Count - 1].Text != "Action")
                {
                    TableHeaderCell tableHeaderCell = new TableHeaderCell();
                    tableHeaderCell.Text = "Action";
                    cells.Add((TableCell)tableHeaderCell);
                }
            }
        }

        private void BindActivePrepDates()
        {
            var repo = new DeliverySheetRepository();
            var result = repo.GetActiveDeliveryDates();

            if (!result.Success)
            {
                ShowPageStatus(result.ErrorMessage, true);
                ddlActivePrepDates.DataSource = null;
                ddlActivePrepDates.DataBind();
                return;
            }

            ClearPageStatus();
            ddlActivePrepDates.DataSource = result.Items;
            ddlActivePrepDates.DataBind();

            if (result.Items.Count == 0)
                ShowPageStatus("No open delivery dates found. There are no undelivered orders with a required-by date.", false);
        }

        private void ShowPageStatus(string message, bool isError)
        {
            if (pnlStatus == null || ltrlStatus == null)
                return;

            bool hasMessage = !string.IsNullOrWhiteSpace(message);
            pnlStatus.Visible = hasMessage;
            ltrlStatus.Text = hasMessage ? HttpUtility.HtmlEncode(message) : string.Empty;
            pnlStatus.Attributes["class"] = isError
                ? "status-message status-error"
                : "status-message status-info";
        }

        private void ClearPageStatus()
        {
            if (pnlStatus != null)
            {
                pnlStatus.Visible = false;
                pnlStatus.Attributes["class"] = "status-message";
            }

            if (ltrlStatus != null)
                ltrlStatus.Text = string.Empty;
        }

        protected void BuildDeliverySheet()
        {
            if (this.ltrlWhichDate.Text.Length > 0)
            {
                string text = this.ltrlWhichDate.Text;
            }

            this.BuildDeliverySheet(
                false,
                this.ltrlWhichDate.Text.Length > 0 ? this.ltrlWhichDate.Text : "2012-01-01",
                this.ddlDeliveryBy.Items.Count <= 1 || this.ddlDeliveryBy.SelectedIndex <= 0
                    ? ""
                    : this.ddlDeliveryBy.SelectedValue);
        }

        protected void BuildDeliverySheet(bool pPrintForm, string pActiveDeliveryDate, string pOnlyDeliveryBy)
        {
            this.Session[CONST_SESSION_DELIVERTBY] = pOnlyDeliveryBy;

            // Parse the selected delivery date.
            // If the value cannot be parsed, fall back to today's date.
            DateTime requiredDate;

            if (!DateTime.TryParse(pActiveDeliveryDate, out requiredDate))
            {
                requiredDate = TimeZoneUtils.Now().Date;
            }

            // Add delivery person filter if specified.
            // The dropdown uses "%" / empty value to mean "all delivery people".
            int parsedDeliveryById;
            int? deliveryById = null;

            if (!string.IsNullOrEmpty(pOnlyDeliveryBy) &&
                pOnlyDeliveryBy != "%" &&
                int.TryParse(pOnlyDeliveryBy, out parsedDeliveryById))
            {
                deliveryById = parsedDeliveryById;
            }

            // SQL has been moved out of the page into DeliverySheetRepository.
            // This page now asks the repository for delivery sheet rows only.
            var repo = new DeliverySheetRepository();
            var queryResult = repo.GetDeliverySheetRows(requiredDate, deliveryById);

            if (!queryResult.Success)
            {
                ShowPageStatus("Could not load delivery sheet: " + queryResult.ErrorMessage, true);
                BuildDeliveryTable(new DeliverySheetBuildResult(), pPrintForm);
                return;
            }

            if (queryResult.Items.Count == 0)
            {
                ShowPageStatus($"No deliveries found for {requiredDate:yyyy-MM-dd}.", false);
                AppLogger.WriteLog("deliverysheet",
                    $"No delivery rows for {requiredDate:yyyy-MM-dd} (deliveryBy={(deliveryById?.ToString() ?? "all")}).");
            }
            else
                ClearPageStatus();

            var accInfoRepository = new ContactsAccInfoRepository();
            var manager = new DeliverySheetManager(
                contactId => accInfoRepository.GetInvoiceTypeIdByContactId((int)contactId) ?? 0);

            var buildResult = manager.Build(queryResult.Items, !pPrintForm);

            // UI rendering stays in the WebForms page.
            this.BuildDeliveryTable(buildResult, pPrintForm);
        }

        /*
         * NOTE:
         * The old delivery item processing methods were moved out of the WebForms page.
         *
         * Moved to DeliverySheetManager:
         * - StripEmailOut
         * - ReadDeliveryItems logic
         * - Sundry contact handling
         * - Invoice type prefix handling
         * - Item HTML generation
         * - Totals calculation
         * - Sundry item reordering
         *
         * The page should now only handle WebForms UI rendering.
         */

        private void BuildDeliveryTable(DeliverySheetBuildResult buildResult, bool pPrintForm)
        {
            // Clear previous table rows and totals
            while (1 < this.tblDeliveries.Rows.Count)
                this.tblDeliveries.Rows.RemoveAt(1);

            this.tblTotals.Rows.Clear();

            // Prepare collections for delivery items, delivery by, and item totals.
            // These are now prepared by DeliverySheetManager instead of being read
            // directly from an IDataReader in the page.
            if (buildResult == null)
                buildResult = new DeliverySheetBuildResult();

            // 1. Delivery items have already been read from SQL by the repository
            //    and transformed by the manager.

            // 2. Sundry item reordering is now handled by DeliverySheetManager.

            // 3. Add delivery rows to the table
            AddDeliveryRows(buildResult.Items, pPrintForm);

            // 4. Style the table rows for readability
            ApplyRowStyles();

            // 5. Build the totals summary table
            BuildTotalsTable(buildResult.Totals);

            // 6. Update the delivery by dropdown if not printing
            if (!pPrintForm)
                UpdateDeliveryByDropdown(buildResult.DeliveryPeople);

            // 7. Update the UI panel
            this.upnlDeliveryItems.Update();
        }

        /// <summary>
        /// Adds delivery rows to the deliveries table.
        /// </summary>
        private void AddDeliveryRows(List<DeliverySheetDisplayItem> deliveryItemsList, bool pPrintForm)
        {
            int index = 0;
            int num1 = deliveryItemsList.Count;

            while (index < num1)
            {
                TableRow row = new TableRow();

                DeliverySheetDisplayItem currentItem = deliveryItemsList[index];
                string orderDetailUrl = BuildOrderDetailUrl(currentItem);

                if (currentItem.Done)
                {
                    row.BackColor = Color.FromArgb(0xFD, 0xF0, 0xE6); // light orange — delivered (green = action)
                    row.ToolTip = "Delivered";
                }

                // Details cell
                TableCell cellDetails = new TableCell();
                cellDetails.Text = currentItem.Details;

                if (pPrintForm)
                {
                    cellDetails.Font.Size = FontUnit.XSmall;

                    if (cellDetails.Text.Contains(","))
                        cellDetails.Text = cellDetails.Text.Remove(0, cellDetails.Text.IndexOf(",") + 1);
                }
                else
                {
                    cellDetails.Text =
                        $"<a class='plain' href='{orderDetailUrl}'>{cellDetails.Text.Trim()}</a>";
                }

                row.Cells.Add(cellDetails);

                // Contact cell
                TableCell cellContact = new TableCell();

                if (pPrintForm)
                {
                    string contactName = currentItem.ContactName;

                    if (contactName.Contains("]>"))
                    {
                        int num3 = contactName.IndexOf("]>");
                        contactName = contactName.Substring(num3 + 3);
                    }

                    cellContact.Text = currentItem.Done ? "done · " + contactName : contactName;
                }
                else if (currentItem.ContactID == SystemConstants.CustomerConstants.SundryCustomerNamePrefix)
                {
                    cellContact.Text = FormatDeliveryStatusBadge(currentItem.Done) + currentItem.ContactName;
                }
                else
                {
                    string contactName = currentItem.ContactName;

                    if (contactName.Contains("]>"))
                    {
                        int length = contactName.IndexOf("]>");

                        cellContact.Text =
                            FormatDeliveryStatusBadge(currentItem.Done) +
                            $"{contactName.Substring(0, length)} - " +
                            $"<a href='./ContactDetails.aspx?ID={currentItem.ContactID}'>{contactName.Substring(length + 3)}</a>";
                    }
                    else
                    {
                        cellContact.Text =
                            FormatDeliveryStatusBadge(currentItem.Done) +
                            $"<a href='./ContactDetails.aspx?ID={currentItem.ContactID}'>{contactName}</a>";
                    }

                    cellContact.CssClass = "wordwrap";
                }

                row.Cells.Add(cellContact);

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

                if (!string.IsNullOrWhiteSpace(currentItem.PurchaseOrder))
                    cellItems.Text = $"<b>[PO: {currentItem.PurchaseOrder}]</b>";

                if (!pPrintForm && currentItem.InvoiceDone)
                {
                    cellItems.Text =
                        $"{cellItems.Text}{(string.IsNullOrEmpty(cellItems.Text) ? "" : " ")}" +
                        "<span class='status-badge is-enabled'>invoiced</span>";
                }

                string str7 = BuildActionHtml(orderDetailUrl, currentItem);

                // Add all items for the same contact
                do
                {
                    cellItems.Text =
                        cellItems.Text +
                        (string.IsNullOrEmpty(cellItems.Text) ? "" : "; ") +
                        deliveryItemsList[index].Items;

                    ++index;
                }
                while (index < num1 &&
                       deliveryItemsList[index - 1].OrderID == deliveryItemsList[index].OrderID);

                row.Cells.Add(cellItems);

                // Extra cell for print
                if (pPrintForm)
                    row.Cells.Add(new TableCell());

                // Action cell for non-mobile, non-print
                bool flag =
                    this.Session["RunningOnMoble"] != null &&
                    (bool)this.Session["RunningOnMoble"];

                if (!pPrintForm && !flag)
                    row.Cells.Add(new TableCell() { Text = str7 });

                this.tblDeliveries.Rows.Add(row);
            }
        }

        /// <summary>
        /// Builds the OrderDetail URL for the delivery row action links.
        /// </summary>
        private string BuildOrderDetailUrl(DeliverySheetDisplayItem item)
        {
            return $"{this.ResolveUrl("~/Pages/OrderDetail.aspx")}?OrderID={item.OrderID}";
        }

        /// <summary>
        /// Builds the action buttons for edit, invoice done, and delivered.
        /// </summary>
        private string BuildActionHtml(string orderDetailUrl, DeliverySheetDisplayItem item)
        {
            string html =
                "<span style='vertical-align:middle'> " +
                $"<a href='{orderDetailUrl}' class='plain'><img src='../images/imgButtons/EditButton.gif' alt='edit' /></a>";

            if (!item.InvoiceDone)
            {
                html +=
                    $"&nbsp;<a href='{orderDetailUrl}&Invoiced=Y' class='plain'><img src='../images/imgButtons/InvoicedButton.gif' alt='invcd' /></a>";
            }

            if (!item.Done)
            {
                html +=
                    $"&nbsp;<a href='{orderDetailUrl}&Delivered=Y' class='plain'><img src='../images/imgButtons/DoneButton.gif' alt='dlvrd' /></a>";
            }

            html += "</span>";

            return html;
        }

        private static string FormatDeliveryStatusBadge(bool isDone)
        {
            // Orange/red chip — green (is-enabled) means "do this", not completed.
            return isDone
                ? "<span class='status-badge is-done'>done</span> "
                : string.Empty;
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
        private void BuildTotalsTable(List<DeliverySheetTotal> itemTotals)
        {
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

            foreach (var itemTotal in itemTotals)
            {
                TableHeaderCell cellItem = new TableHeaderCell();
                cellItem.Text = itemTotal.ItemDesc;
                cellItem.Font.Bold = true;
                summaryHeaderRow.Cells.Add(cellItem);

                summaryItemsRow.Cells.Add(new TableCell()
                {
                    Text = SystemConstants.FormatConstants.FormatQuantity(itemTotal.TotalQty),
                    HorizontalAlign = HorizontalAlign.Right
                });
            }

            this.tblTotals.Rows.Add(summaryHeaderRow);
            this.tblTotals.Rows.Add(summaryItemsRow);
        }

        /// <summary>
        /// Updates the delivery by dropdown and label visibility.
        /// </summary>
        private void UpdateDeliveryByDropdown(List<DeliveryPersonOption> deliveryPeople)
        {
            bool flag = deliveryPeople != null && deliveryPeople.Count > 1;

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

                foreach (var person in deliveryPeople)
                {
                    this.ddlDeliveryBy.Items.Add(new ListItem()
                    {
                        Text = person.Abbreviation,
                        Value = person.PersonID
                    });
                }
            }
        }

        /*
         * LEGACY REFERENCE:
         * The old all-in-one BuildDeliveryTable method was removed from this file.
         *
         * Old responsibilities were split as follows:
         * - SQL/data access: DeliverySheetRepository
         * - Business/transformation logic: DeliverySheetManager
         * - UI rendering: DeliverySheet.aspx.cs
         *
         * Do not restore the old IDataReader-based method.
         */

        protected void btnPrint_Click(object sender, EventArgs e)
        {
            if (this.ddlActivePrepDates == null || this.ddlActivePrepDates.SelectedIndex <= 0)
                return;

            AppLogger.WriteLog("deliverysheet", "Printed delivery sheet");

            this.Session[CONST_SESSION_SHEETDATE] =
                TryGetSelectedDeliveryDate(out DateTime deliveryDate)
                    ? deliveryDate.ToString("yyyy-MM-dd")
                    : string.Empty;

            this.Response.Redirect("~/Pages/DeliverySheet.aspx?Print=Y");
        }

        protected void ddlActivePrepDates_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadSheetForSelectedDate("Changed delivery date");
        }

        protected void tbCalendarDate_TextChanged(object sender, EventArgs e)
        {
            string selectedDate = tbCalendarDate.Text.Trim();

            if (!DateTime.TryParse(selectedDate, out DateTime dt))
                return;

            string value = dt.Date.ToString("yyyy-MM-dd");
            ddlActivePrepDates.ClearSelection();

            ListItem item = ddlActivePrepDates.Items.FindByValue(value);
            if (item == null)
            {
                ddlActivePrepDates.Items.Insert(1, new ListItem(dt.ToString("dd-MMM-yyyy (ddd)"), value));
                ddlActivePrepDates.Items[1].Selected = true;
            }
            else
            {
                item.Selected = true;
            }

            ddlActivePrepDates_SelectedIndexChanged(ddlActivePrepDates, EventArgs.Empty);
        }

        protected void ddlActivePrepDates_DataBound(object sender, EventArgs e)
        {
            NormalizeActivePrepDateListValues();

            if (!IsPostBack)
            {
                SelectDeliveryDateFromSession();

                if (TryGetSelectedDeliveryDate(out _))
                    SetVarsAndBuildDeliverySheet();
            }

            /*
             * LEGACY REFERENCE:
             *
             * Old code also restored ddlDeliveryBy from session here.
             * The delivery-by dropdown is now rebuilt from DeliverySheetManager output
             * after delivery rows are loaded.
             */
        }

        private void NormalizeActivePrepDateListValues()
        {
            if (ddlActivePrepDates == null)
                return;

            foreach (ListItem item in ddlActivePrepDates.Items)
            {
                if (string.IsNullOrWhiteSpace(item.Value))
                    continue;

                if (DateTime.TryParse(item.Value, out DateTime parsedDate))
                    item.Value = parsedDate.Date.ToString("yyyy-MM-dd");
            }
        }

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

        protected void btnBack_Click(object sender, EventArgs e)
        {
            this.Response.Redirect("~/Default.aspx");
        }

        private bool TryGetSelectedDeliveryDate(out DateTime deliveryDate)
        {
            deliveryDate = TimeZoneUtils.Now().Date;

            if (this.ddlActivePrepDates == null || this.ddlActivePrepDates.SelectedIndex <= 0)
                return false;

            string selectedValue = this.ddlActivePrepDates.SelectedValue;
            if (string.IsNullOrWhiteSpace(selectedValue))
                return false;

            return DateTime.TryParse(selectedValue, out deliveryDate);
        }

        private void SelectDeliveryDateFromSession()
        {
            ddlActivePrepDates.ClearSelection();

            string sessionValue = Session[CONST_SESSION_DDLSHEETDATE_SELECTED] as string;
            if (!string.IsNullOrWhiteSpace(sessionValue) && DateTime.TryParse(sessionValue, out DateTime targetDate))
                sessionValue = targetDate.Date.ToString("yyyy-MM-dd");

            ListItem match = !string.IsNullOrWhiteSpace(sessionValue)
                ? ddlActivePrepDates.Items.FindByValue(sessionValue)
                : null;

            if (match != null)
            {
                match.Selected = true;
                return;
            }

            if (ddlActivePrepDates.Items.Count > 1)
                ddlActivePrepDates.Items[1].Selected = true;
        }

        protected void SetVarsAndBuildDeliverySheet()
        {
            if (!TryGetSelectedDeliveryDate(out DateTime deliveryDate))
                return;

            this.ltrlWhichDate.Text = deliveryDate.ToString("yyyy-MM-dd");
            this.Session[CONST_SESSION_SHEETDATE] = this.ltrlWhichDate.Text;
            // Date navigation must show the whole day — not a leftover "By" person filter.
            this.Session[CONST_SESSION_DELIVERTBY] = string.Empty;
            this.Session[CONST_SESSION_DDLDELIVERTBY_SELECTED] = string.Empty;
            if (this.ddlDeliveryBy != null && this.ddlDeliveryBy.Items.Count > 0)
            {
                this.ddlDeliveryBy.ClearSelection();
                if (this.ddlDeliveryBy.Items.FindByValue("%") != null)
                    this.ddlDeliveryBy.SelectedValue = "%";
                else
                    this.ddlDeliveryBy.SelectedIndex = 0;
            }

            this.BuildDeliverySheet(false, this.ltrlWhichDate.Text, string.Empty);
        }

        /// <summary>
        /// Selects the dropdown date (if needed), clears Find noise, and rebuilds the sheet.
        /// Used by dropdown AutoPostBack, calendar pick, and Go.
        /// </summary>
        private void LoadSheetForSelectedDate(string logAction)
        {
            if (!TryGetSelectedDeliveryDate(out DateTime deliveryDate))
            {
                ShowPageStatus("Select a delivery date first.", false);
                return;
            }

            string dateValue = deliveryDate.ToString("yyyy-MM-dd");
            Session[CONST_SESSION_DDLSHEETDATE_SELECTED] = dateValue;
            this.ltrlWhichDate.Text = dateValue;
            this.ltrlWhichDate.Visible = true;

            // Leaving Find text in place caused DefaultButton/Find to replace the date sheet
            // with a contact search on the same postback.
            if (this.tbxFindClient != null)
                this.tbxFindClient.Text = string.Empty;

            if (this.btnGo != null)
                this.Form.DefaultButton = this.btnGo.UniqueID;

            this.SetVarsAndBuildDeliverySheet();

            if (!string.IsNullOrEmpty(logAction))
                AppLogger.WriteLog("deliverysheet", $"{logAction} to {dateValue}");
        }

        protected void btnGo_Click(object sender, EventArgs e)
        {
            LoadSheetForSelectedDate("Go delivery date");
        }

        protected void btnFind_Click(object sender, EventArgs e)
        {
            string searchText = this.tbxFindClient != null ? this.tbxFindClient.Text.Trim() : string.Empty;
            if (string.IsNullOrEmpty(searchText))
            {
                // Empty Find / accidental DefaultButton click → load selected date instead.
                LoadSheetForSelectedDate(null);
                return;
            }

            AppLogger.WriteLog("deliverysheet", $"Searched for contact: {searchText}");

            // Search SQL has moved to DeliverySheetRepository.
            // This removes the old inline SQL and avoids SQL injection from the search textbox.
            var repo = new DeliverySheetRepository();
            var queryResult = repo.SearchDeliverySheetRowsByContact(searchText);

            if (!queryResult.Success)
            {
                ShowPageStatus("Contact search failed: " + queryResult.ErrorMessage, true);
                BuildDeliveryTable(new DeliverySheetBuildResult(), false);
                return;
            }

            if (queryResult.Items.Count == 0)
                ShowPageStatus($"No open deliveries found matching '{searchText}'.", false);
            else
                ClearPageStatus();

            var accInfoRepository = new ContactsAccInfoRepository();
            var manager = new DeliverySheetManager(
                contactId => accInfoRepository.GetInvoiceTypeIdByContactId((int)contactId) ?? 0);

            var buildResult = manager.Build(queryResult.Items, true);

            this.BuildDeliveryTable(buildResult, false);
        }

        protected void tbxFindClient_OnTextChanged(object sender, EventArgs e)
        {
            this.btnFind_Click(sender, e);
        }

        /*
         * NOTE:
         * The old private nested classes deliveryItems and ItemTotals were removed.
         *
         * Replaced by Models:
         * - DeliverySheetDisplayItem
         * - DeliverySheetTotal
         * - DeliveryPersonOption
         * - DeliverySheetBuildResult
         */
    }
}
