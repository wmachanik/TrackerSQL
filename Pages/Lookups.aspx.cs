//------------------------------------------------------------------------------
// TrackerSQL v3.x — Lookups
// WebForms page code-behind for Lookups.
//------------------------------------------------------------------------------

using AjaxControlToolkit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

//- only form later versions #nullable disable
namespace TrackerSQL.Pages
{
    public partial class Lookups : Page
    {
        private const string CONST_ITEMSEARCHSESIONVAR = "SearchItemContains";
        private const int CONST_BGCOLOURCOL = 4;
        protected ScriptManager scmLookup;
        protected UpdateProgress uprgLookup;
        protected Panel pnlLookups;
        protected System.Web.UI.HtmlControls.HtmlGenericControl pnlLookupStatus;
        protected Label lblStatus;
        protected TabContainer tabcLookup;
        protected TabPanel tabpnlItems;
        protected UpdatePanel upnlItems;
        protected TextBox tbxItemSearch;
        protected Button btnGon;
        protected Button btnReset;
        protected GridView gvItems;
        protected ObjectDataSource odsItemUnits;
        protected TabPanel tabpnlPeople;
        protected UpdatePanel upnlPeople;
        protected GridView gvPeople;
        protected TabPanel tabpnlEquipment;
        protected UpdatePanel upnlEquipment;
        protected GridView gvEquipment;
        protected TabPanel tabpnlAreas;
        protected UpdatePanel upnlAreas;
        protected GridView gvAreas;
        protected GridView gvAreaDays;
        protected TabPanel tabpnlPackaging;
        protected UpdatePanel UpdatePanel1;
        protected GridView gvPackaging;
        protected TabPanel tabInvoiceTypes;
        protected UpdateProgress gvInvoiceTypesUpdateProgress;
        protected UpdatePanel gvInvoiceTypesUpdatePanel;
        protected GridView gvInvoiceTypes;
        protected TabPanel tabPaymentTerms;
        protected UpdateProgress PaymentTermsUpdateProgress;
        protected UpdatePanel gvPaymentTermsUpdatePanel;
        protected GridView gvPaymentTerms;
        protected TabPanel tabPriceLevels;
        protected UpdateProgress PriceLevelUpdateProgress;
        protected UpdatePanel gvPriceLevelsUpdatePanel;
        protected GridView gvPriceLevels;
        protected SqlDataSource sdsUserNames;


        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {
                this.tabcLookup.ActiveTabIndex = 0;

                // gvAreaDays is not always populated; protect against out-of-range
                if (this.gvAreaDays != null && this.gvAreaDays.Rows != null && this.gvAreaDays.Rows.Count > 1)
                    this.gvAreaDays.SelectedIndex = 1;
                
                // Initialize Items grid with repository pattern
                Session["SearchItemContains"] = "%"; // Default: show all
                BindItemsGrid();
                
                // Initialize Areas grid with repository pattern
                BindAreasGrid();

                // Manual binding for remaining tabs (ObjectDataSource removed)
                BindPeopleGrid();
                BindEquipmentGrid();
                BindPackagingGrid();
                BindInvoiceTypesGrid();
                BindPaymentTermsGrid();
                BindPriceLevelsGrid();
                BindRepairStatusesGrid();
            }
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            if (pnlLookupStatus != null)
                pnlLookupStatus.Visible = !string.IsNullOrWhiteSpace(lblStatus?.Text);
        }

        private void BindPeopleGrid()
        {
            try
            {
                var repo = new PersonsRepository();
                string sortBy = ViewState["PeopleSortExpression"] as string ?? "Abbreviation";
                var people = repo.GetAll(sortBy);
                gvPeople.DataSource = people;
                gvPeople.DataBind();
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Error loading people: " + ex.Message;
            }
        }

        private void BindEquipmentGrid()
        {
            try
            {
                var repo = new EquipTypesRepository();
                string sortBy = ViewState["EquipTypesSortExpression"] as string ?? "EquipTypeName";
                var equip = repo.GetAll(sortBy);
                gvEquipment.DataSource = equip;
                gvEquipment.DataBind();
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Error loading equipment: " + ex.Message;
            }
        }

        private void BindPackagingGrid()
        {
            try
            {
                var repo = new ItemPackagingsRepository();
                string sortBy = ViewState["PackagingSortExpression"] as string ?? "ItemPackagingDesc";
                var packagings = repo.GetAll(sortBy);
                gvPackaging.DataSource = packagings;
                gvPackaging.DataBind();
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Error loading packaging: " + ex.Message;
            }
        }

        private void BindInvoiceTypesGrid()
        {
            try
            {
                var repo = new InvoiceTypesRepository();
                string sortBy = ViewState["InvoiceTypesSortExpression"] as string ?? "InvoiceTypeDesc";
                var invoiceTypes = repo.GetAll(sortBy);
                gvInvoiceTypes.DataSource = invoiceTypes;
                gvInvoiceTypes.DataBind();
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Error loading invoice types: " + ex.Message;
            }
        }

        private void BindPaymentTermsGrid()
        {
            try
            {
                var repo = new PaymentTermsRepository();
                string sortBy = ViewState["PaymentTermsSortExpression"] as string ?? "PaymentTermDesc";
                var terms = repo.GetAll(sortBy);
                gvPaymentTerms.DataSource = terms;
                gvPaymentTerms.DataBind();
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Error loading payment terms: " + ex.Message;
            }
        }

        private void BindPriceLevelsGrid()
        {
            try
            {
                var repo = new PriceLevelsRepository();
                string sortBy = ViewState["PriceLevelsSortExpression"] as string ?? "PriceLevelDesc";
                var levels = repo.GetAll(sortBy);
                gvPriceLevels.DataSource = levels;
                gvPriceLevels.DataBind();
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Error loading price levels: " + ex.Message;
            }
        }

        protected void gvPeople_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvPeople.PageIndex = e.NewPageIndex;
            BindPeopleGrid();
        }

        protected void gvPeople_Sorting(object sender, GridViewSortEventArgs e)
        {
            ViewState["PeopleSortExpression"] = e.SortExpression;
            BindPeopleGrid();
        }

        protected void gvEquipment_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvEquipment.PageIndex = e.NewPageIndex;
            BindEquipmentGrid();
        }

        protected void gvEquipment_Sorting(object sender, GridViewSortEventArgs e)
        {
            ViewState["EquipTypesSortExpression"] = e.SortExpression;
            BindEquipmentGrid();
        }

        protected void gvEquipment_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gvEquipment.EditIndex = e.NewEditIndex;
            BindEquipmentGrid();
        }

        protected void gvEquipment_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvEquipment.EditIndex = -1;
            BindEquipmentGrid();
        }

        protected void gvEquipment_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            try
            {
                int equipTypeId = Convert.ToInt32(gvEquipment.DataKeys[e.RowIndex].Value);
                GridViewRow row = gvEquipment.Rows[e.RowIndex];

                var tbxName = (TextBox)row.FindControl("EquipTypeNameTextBox");
                var tbxDesc = (TextBox)row.FindControl("EquipTypeDescTextBox");

                var equipType = new EquipType
                {
                    EquipTypeID = equipTypeId,
                    EquipTypeName = tbxName?.Text ?? "",
                    EquipTypeDescription = tbxDesc?.Text ?? ""
                };

                var repo = new EquipTypesRepository();
                repo.Update(equipType);

                gvEquipment.EditIndex = -1;
                BindEquipmentGrid();
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Error updating equipment type: " + ex.Message;
            }
        }

        protected void gvPackaging_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvPackaging.PageIndex = e.NewPageIndex;
            BindPackagingGrid();
        }

        protected void gvPackaging_Sorting(object sender, GridViewSortEventArgs e)
        {
            ViewState["PackagingSortExpression"] = e.SortExpression;
            BindPackagingGrid();
        }

        protected void gvPackaging_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gvPackaging.EditIndex = e.NewEditIndex;
            BindPackagingGrid();
        }

        protected void gvPackaging_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvPackaging.EditIndex = -1;
            BindPackagingGrid();
        }

        protected void gvPackaging_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            try
            {
                int packagingId = Convert.ToInt32(gvPackaging.DataKeys[e.RowIndex].Value);
                GridViewRow row = gvPackaging.Rows[e.RowIndex];

                var tbxDesc = (TextBox)row.FindControl("TextBoxDescription");
                var tbxNotes = (TextBox)row.FindControl("TextBoxAdditionalNotes");
                var tbxBGColour = (TextBox)row.FindControl("TextBoxBGColour");
                var tbxColour = (TextBox)row.FindControl("TextBoxColour");
                var tbxSymbol = (TextBox)row.FindControl("TextBoxSymbol");

                var packaging = new ItemPackaging
                {
                    ItemPackagingID = packagingId,
                    ItemPackagingDesc = tbxDesc?.Text ?? "",
                    AdditionalNotes = tbxNotes?.Text ?? "",
                    BGColour = tbxBGColour?.Text ?? "",
                    Colour = tbxColour != null && !string.IsNullOrEmpty(tbxColour.Text) ? (int?)Convert.ToInt32(tbxColour.Text) : null,
                    Symbol = tbxSymbol?.Text ?? ""
                };

                var repo = new ItemPackagingsRepository();
                repo.Update(packaging);

                gvPackaging.EditIndex = -1;
                BindPackagingGrid();
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Error updating packaging: " + ex.Message;
            }
        }

        protected void gvInvoiceTypes_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvInvoiceTypes.PageIndex = e.NewPageIndex;
            BindInvoiceTypesGrid();
        }

        protected void gvInvoiceTypes_Sorting(object sender, GridViewSortEventArgs e)
        {
            ViewState["InvoiceTypesSortExpression"] = e.SortExpression;
            BindInvoiceTypesGrid();
        }

        protected void gvInvoiceTypes_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gvInvoiceTypes.EditIndex = e.NewEditIndex;
            BindInvoiceTypesGrid();
        }

        protected void gvInvoiceTypes_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvInvoiceTypes.EditIndex = -1;
            BindInvoiceTypesGrid();
        }

        protected void gvInvoiceTypes_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            try
            {
                int invoiceTypeId = Convert.ToInt32(gvInvoiceTypes.DataKeys[e.RowIndex].Value);
                GridViewRow row = gvInvoiceTypes.Rows[e.RowIndex];

                var tbxDesc = (TextBox)row.FindControl("InvoiceTypeDescTextBox");
                var cbxEnabled = (CheckBox)row.FindControl("EnabledCheckBox");
                var tbxNotes = (TextBox)row.FindControl("NotesTextBox");

                var invoiceType = new InvoiceType
                {
                    InvoiceTypeID = invoiceTypeId,
                    InvoiceTypeDesc = tbxDesc?.Text ?? "",
                    Enabled = cbxEnabled != null && cbxEnabled.Checked,
                    Notes = tbxNotes?.Text ?? ""
                };

                var repo = new InvoiceTypesRepository();
                repo.Update(invoiceType);

                gvInvoiceTypes.EditIndex = -1;
                BindInvoiceTypesGrid();
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Error updating invoice type: " + ex.Message;
            }
        }

        protected void gvPaymentTerms_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvPaymentTerms.PageIndex = e.NewPageIndex;
            BindPaymentTermsGrid();
        }

        protected void gvPaymentTerms_Sorting(object sender, GridViewSortEventArgs e)
        {
            ViewState["PaymentTermsSortExpression"] = e.SortExpression;
            BindPaymentTermsGrid();
        }

        protected void gvPaymentTerms_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gvPaymentTerms.EditIndex = e.NewEditIndex;
            BindPaymentTermsGrid();
        }

        protected void gvPaymentTerms_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvPaymentTerms.EditIndex = -1;
            BindPaymentTermsGrid();
        }

        protected void gvPaymentTerms_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            try
            {
                int paymentTermId = Convert.ToInt32(gvPaymentTerms.DataKeys[e.RowIndex].Value);
                GridViewRow row = gvPaymentTerms.Rows[e.RowIndex];

                var tbxDesc = (TextBox)row.FindControl("PaymentTermDescTextBox");
                var tbxPaymentDays = (TextBox)row.FindControl("PaymentDaysTextBox");
                var tbxDayOfMonth = (TextBox)row.FindControl("DayOfMonthTextBox");
                var cbxUseDays = (CheckBox)row.FindControl("UseDaysCheckBox");
                var cbxEnabled = (CheckBox)row.FindControl("EnabledCheckBox");
                var tbxNotes = (TextBox)row.FindControl("NotesTextBox");

                var paymentTerm = new PaymentTerm
                {
                    PaymentTermID = paymentTermId,
                    PaymentTermDesc = tbxDesc?.Text ?? "",
                    PaymentDays = tbxPaymentDays != null && !string.IsNullOrEmpty(tbxPaymentDays.Text) ? (int?)Convert.ToInt32(tbxPaymentDays.Text) : null,
                    DayOfMonth = tbxDayOfMonth != null && !string.IsNullOrEmpty(tbxDayOfMonth.Text) ? (byte?)Convert.ToByte(tbxDayOfMonth.Text) : null,
                    UseDays = cbxUseDays != null && cbxUseDays.Checked,
                    Enabled = cbxEnabled == null || cbxEnabled.Checked,
                    Notes = tbxNotes?.Text ?? ""
                };

                var repo = new PaymentTermsRepository();
                repo.Update(paymentTerm);

                gvPaymentTerms.EditIndex = -1;
                BindPaymentTermsGrid();
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Error updating payment term: " + ex.Message;
            }
        }

        protected void gvPriceLevels_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvPriceLevels.PageIndex = e.NewPageIndex;
            BindPriceLevelsGrid();
        }

        protected void gvPriceLevels_Sorting(object sender, GridViewSortEventArgs e)
        {
            ViewState["PriceLevelsSortExpression"] = e.SortExpression;
            BindPriceLevelsGrid();
        }

        protected void gvPriceLevels_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gvPriceLevels.EditIndex = e.NewEditIndex;
            BindPriceLevelsGrid();
        }

        protected void gvPriceLevels_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvPriceLevels.EditIndex = -1;
            BindPriceLevelsGrid();
        }

        protected void gvPriceLevels_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            try
            {
                int priceLevelId = Convert.ToInt32(gvPriceLevels.DataKeys[e.RowIndex].Value);
                GridViewRow row = gvPriceLevels.Rows[e.RowIndex];

                var tbxDesc = (TextBox)row.FindControl("PriceLevelDescTextBox");
                var tbxFactor = (TextBox)row.FindControl("PricingFactorTextBox");
                var cbxEnabled = (CheckBox)row.FindControl("EnabledCheckBox");
                var tbxNotes = (TextBox)row.FindControl("NotesTextBox");

                var priceLevel = new PriceLevel
                {
                    PriceLevelID = priceLevelId,
                    PriceLevelDesc = tbxDesc?.Text ?? "",
                    PricingFactor = tbxFactor != null && !string.IsNullOrEmpty(tbxFactor.Text) ? Convert.ToDouble(tbxFactor.Text) : 1.0,
                    Enabled = cbxEnabled != null && cbxEnabled.Checked,
                    Notes = tbxNotes?.Text ?? ""
                };

                var repo = new PriceLevelsRepository();
                repo.Update(priceLevel);

                gvPriceLevels.EditIndex = -1;
                BindPriceLevelsGrid();
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Error updating price level: " + ex.Message;
            }
        }

        private void BindRepairStatusesGrid(bool forceRefresh = false)
        {
            try
            {
                List<RepairStatus> statuses;
                
                // If we're in edit mode and not forcing refresh, reuse cached data to maintain row position
                if (!forceRefresh && gvRepairStatuses.EditIndex >= 0 && Session["RepairStatusesGridData"] != null)
                {
                    statuses = (List<RepairStatus>)Session["RepairStatusesGridData"];
                }
                else
                {
                    var repo = new RepairStatusesRepository();
                    string sortBy = ViewState["RepairStatusesSortExpression"] as string ?? "SortOrder";
                    statuses = repo.GetAll(sortBy);
                    
                    // Cache the data for edit operations
                    Session["RepairStatusesGridData"] = statuses;
                }
                
                gvRepairStatuses.DataSource = statuses;
                gvRepairStatuses.DataBind();
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Error loading repair statuses: " + ex.Message;
            }
        }

        private int ExecNonQuery(string sql, List<TrackerSQL.Classes.DBParameter> parameters)
        {
            using (var db = new TrackerSQL.Classes.TrackerSQLDb())
            {
                return db.ExecuteNonQuery(sql, parameters);
            }
        }

        protected void gvRepairStatuses_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvRepairStatuses.PageIndex = e.NewPageIndex;
            gvRepairStatuses.EditIndex = -1; // Exit edit mode when changing pages
            BindRepairStatusesGrid(forceRefresh: true);
        }

        protected void gvRepairStatuses_Sorting(object sender, GridViewSortEventArgs e)
        {
            ViewState["RepairStatusesSortExpression"] = e.SortExpression;
            gvRepairStatuses.EditIndex = -1; // Exit edit mode when sorting
            BindRepairStatusesGrid(forceRefresh: true);
        }

        protected void gvRepairStatuses_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gvRepairStatuses.EditIndex = e.NewEditIndex;
            BindRepairStatusesGrid();
        }

        protected void gvRepairStatuses_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvRepairStatuses.EditIndex = -1;
            // Force refresh when canceling to ensure clean state
            BindRepairStatusesGrid(forceRefresh: true);
        }

        protected void gvRepairStatuses_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            try
            {
                int id = Convert.ToInt32(gvRepairStatuses.DataKeys[e.RowIndex].Value);
                GridViewRow row = gvRepairStatuses.Rows[e.RowIndex];

                var tbxStatusDesc = (TextBox)row.FindControl("tbxStatusDesc");
                var tbxStatusNote = (TextBox)row.FindControl("tbxStatusNote");

                bool emailClient = false;
                // CheckBoxField doesn't generate a named control reliably; use Cells index.
                // Columns: ID(0), Status(1), EmailClient(2), SortOrder(3), StatusNote(4), Buttons(5)
                if (row.Cells.Count > 2 && row.Cells[2].Controls.Count > 0)
                {
                    var cbx = row.Cells[2].Controls.OfType<CheckBox>().FirstOrDefault();
                    if (cbx != null) emailClient = cbx.Checked;
                }

                int sortOrder = 0;
                if (row.Cells.Count > 3)
                {
                    var sortText = row.Cells[3].Text;
                    int.TryParse(sortText, out sortOrder);
                }

                var status = new RepairStatus
                {
                    RepairStatusID = id,
                    RepairStatusDesc = tbxStatusDesc != null ? tbxStatusDesc.Text : string.Empty,
                    EmailContact = emailClient,
                    SortOrder = sortOrder,
                    StatusNote = tbxStatusNote != null ? tbxStatusNote.Text : string.Empty
                };

                var sql = "UPDATE RepairStatusesTbl SET RepairStatusDesc=@d, EmailContact=@e, SortOrder=@s, StatusNote=@n WHERE RepairStatusID=@id";
                var p = new List<TrackerSQL.Classes.DBParameter>
                {
                    new TrackerSQL.Classes.DBParameter { ParamName = "@d", DataDbType = System.Data.DbType.String, DataValue = status.RepairStatusDesc },
                    new TrackerSQL.Classes.DBParameter { ParamName = "@e", DataDbType = System.Data.DbType.Boolean, DataValue = status.EmailContact ?? false },
                    new TrackerSQL.Classes.DBParameter { ParamName = "@s", DataDbType = System.Data.DbType.Int32, DataValue = status.SortOrder ?? 0 },
                    new TrackerSQL.Classes.DBParameter { ParamName = "@n", DataDbType = System.Data.DbType.String, DataValue = status.StatusNote ?? string.Empty },
                    new TrackerSQL.Classes.DBParameter { ParamName = "@id", DataDbType = System.Data.DbType.Int32, DataValue = status.RepairStatusID },
                };
                ExecNonQuery(sql, p);

                gvRepairStatuses.EditIndex = -1;
                // Force refresh after update to get latest data from database
                BindRepairStatusesGrid(forceRefresh: true);
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Update failed: " + ex.Message;
            }
        }

        protected void gvRepairStatuses_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            try
            {
                int id = Convert.ToInt32(gvRepairStatuses.DataKeys[e.RowIndex].Value);
                var sql = "DELETE FROM RepairStatusesTbl WHERE RepairStatusID=@id";
                var p = new List<TrackerSQL.Classes.DBParameter>
                {
                    new TrackerSQL.Classes.DBParameter { ParamName = "@id", DataDbType = System.Data.DbType.Int32, DataValue = id }
                };
                ExecNonQuery(sql, p);
                // Force refresh after delete to update the list
                BindRepairStatusesGrid(forceRefresh: true);
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Delete failed: " + ex.Message;
            }
        }

        protected void gvRepairStatuses_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (!string.Equals(e.CommandName, "Insert", StringComparison.OrdinalIgnoreCase))
                return;

            try
            {
                var tbxStatusDesc = (TextBox)gvRepairStatuses.FooterRow.FindControl("tbxStatusDescFooter");
                var tbxStatusNote = (TextBox)gvRepairStatuses.FooterRow.FindControl("tbxStatusNoteFooter");

                var status = new RepairStatus
                {
                    RepairStatusDesc = tbxStatusDesc != null ? tbxStatusDesc.Text : string.Empty,
                    StatusNote = tbxStatusNote != null ? tbxStatusNote.Text : string.Empty,
                    EmailContact = false,
                    SortOrder = 0
                };

                var sql = "INSERT INTO RepairStatusesTbl (RepairStatusDesc, EmailContact, SortOrder, StatusNote) VALUES (@d, @e, @s, @n)";
                var p = new List<TrackerSQL.Classes.DBParameter>
                {
                    new TrackerSQL.Classes.DBParameter { ParamName = "@d", DataDbType = System.Data.DbType.String, DataValue = status.RepairStatusDesc },
                    new TrackerSQL.Classes.DBParameter { ParamName = "@e", DataDbType = System.Data.DbType.Boolean, DataValue = status.EmailContact ?? false },
                    new TrackerSQL.Classes.DBParameter { ParamName = "@s", DataDbType = System.Data.DbType.Int32, DataValue = status.SortOrder ?? 0 },
                    new TrackerSQL.Classes.DBParameter { ParamName = "@n", DataDbType = System.Data.DbType.String, DataValue = status.StatusNote ?? string.Empty }
                };
                ExecNonQuery(sql, p);
                // Force refresh after insert to get new record from database
                BindRepairStatusesGrid(forceRefresh: true);
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Insert failed: " + ex.Message;
            }
        }

        protected void gvItems_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (!e.CommandName.Equals("AddItem"))
                return;
            try
            {
                TextBox control1 = (TextBox)this.gvItems.FooterRow.FindControl("tbxItem");
                TextBox control2 = (TextBox)this.gvItems.FooterRow.FindControl("tbxSKU");
                CheckBox control3 = (CheckBox)this.gvItems.FooterRow.FindControl("cbxItemEnabled");
                TextBox control4 = (TextBox)this.gvItems.FooterRow.FindControl("tbxItemCharacteristics");
                TextBox control5 = (TextBox)this.gvItems.FooterRow.FindControl("tbxItemDetail");
                DropDownList control6 = (DropDownList)this.gvItems.FooterRow.FindControl("ddlServiceType");
                DropDownList control7 = (DropDownList)this.gvItems.FooterRow.FindControl("ddlReplacement");
                TextBox control8 = (TextBox)this.gvItems.FooterRow.FindControl("tbxItemShortName");
                TextBox control9 = (TextBox)this.gvItems.FooterRow.FindControl("tbxSortOrder");
                TextBox control10 = (TextBox)this.gvItems.FooterRow.FindControl("tbxUnitsPerQty");
                DropDownList control11 = (DropDownList)this.gvItems.FooterRow.FindControl("ddlUnits");
                
                // Use Repository Pattern instead of SqlDataSource
                var newItem = new Item
                {
                    ItemDesc = control1.Text,
                    SKU = control2.Text,
                    ItemEnabled = control3.Checked,
                    ItemsCharacteritics = control4.Text,
                    ItemDetail = control5.Text,
                    ItemServiceTypeID = Convert.ToInt32(control6.SelectedValue),
                    ReplacementItemID = Convert.ToInt32(control7.SelectedValue),
                    ItemShortName = control8.Text,
                    SortOrder = Convert.ToInt32(control9.Text),
                    UnitsPerQty = Convert.ToDouble(control10.Text),
                    ItemUnitID = Convert.ToInt32(control11.SelectedValue)
                };
                
                var repo = new ItemsRepository();
                repo.Insert(newItem);
                
                BindItemsGrid(); // Refresh grid
            }
            catch (Exception ex)
            {
                this.lblStatus.Text = "Error adding record: " + ex.Message;
            }
        }

        // Items Grid - Paging Event Handler
        protected void gvItems_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvItems.PageIndex = e.NewPageIndex;
            gvItems.EditIndex = -1; // Exit edit mode when changing pages
            BindItemsGrid(forceRefresh: true);
        }

        // Items Grid - Sorting Event Handler
        protected void gvItems_Sorting(object sender, GridViewSortEventArgs e)
        {
            // Store sort expression in ViewState
            ViewState["ItemsSortExpression"] = e.SortExpression;
            gvItems.EditIndex = -1; // Exit edit mode when sorting
            BindItemsGrid(forceRefresh: true);
        }

        protected void gvItems_RowEditing(object sender, GridViewEditEventArgs e)
        {
            // Refresh the cached dataset for the current filter before entering edit mode.
            // This prevents the grid from rebinding to a partial list (symptom: rows below disappear).
            BindItemsGrid(forceRefresh: true);
            gvItems.EditIndex = e.NewEditIndex;
            BindItemsGrid();
        }

        protected void gvItems_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvItems.EditIndex = -1;
            // Force refresh when canceling to ensure clean state
            BindItemsGrid(forceRefresh: true);
        }

        protected void gvItems_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            try
            {
                int itemId = Convert.ToInt32(gvItems.DataKeys[e.RowIndex].Value);
                GridViewRow row = gvItems.Rows[e.RowIndex];

                var tbxItem = (TextBox)row.FindControl("tbxEItem");
                var tbxSKU = (TextBox)row.FindControl("tbxESKU");
                var cbxItemEnabled = (CheckBox)row.FindControl("cbxItemEnabled");
                var tbxItemCharacteristics = (TextBox)row.FindControl("tbxItemCharacteristics");
                var tbxItemDetail = (TextBox)row.FindControl("tbxItemDetail");
                var ddlServiceType = (DropDownList)row.FindControl("ddlServiceType");
                var ddlReplacement = (DropDownList)row.FindControl("ddlReplacement");
                var tbxItemShortName = (TextBox)row.FindControl("tbxItemShortName");
                var tbxSortOrder = (TextBox)row.FindControl("tbxSortOrder");
                var tbxUnitsPerQty = (TextBox)row.FindControl("tbxUnitsPerQtyr");
                var ddlUnits = (DropDownList)row.FindControl("ddlUnits");

                var item = new Item
                {
                    ItemID = itemId,
                    ItemDesc = tbxItem?.Text ?? "",
                    SKU = tbxSKU?.Text ?? "",
                    ItemEnabled = cbxItemEnabled != null && cbxItemEnabled.Checked,
                    ItemsCharacteritics = tbxItemCharacteristics?.Text ?? "",
                    ItemDetail = tbxItemDetail?.Text ?? "",
                    ItemServiceTypeID = ddlServiceType != null ? Convert.ToInt32(ddlServiceType.SelectedValue) : 0,
                    ReplacementItemID = ddlReplacement != null ? (int?)Convert.ToInt32(ddlReplacement.SelectedValue) : null,
                    ItemShortName = tbxItemShortName?.Text ?? "",
                    SortOrder = tbxSortOrder != null ? (int?)Convert.ToInt32(tbxSortOrder.Text) : null,
                    UnitsPerQty = tbxUnitsPerQty != null ? Convert.ToDouble(tbxUnitsPerQty.Text) : 1.0,
                    ItemUnitID = ddlUnits != null ? (int?)Convert.ToInt32(ddlUnits.SelectedValue) : null
                };

                var repo = new ItemsRepository();
                repo.Update(item);

                gvItems.EditIndex = -1;
                // Force refresh after update to get latest data from database
                BindItemsGrid(forceRefresh: true);
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Error updating item: " + ex.Message;
            }
        }

        // Helper method to bind Items grid with repository pattern
        private void BindItemsGrid(bool forceRefresh = false)
        {
            try
            {
                List<Item> items;
                
                // If we're in edit mode and not forcing refresh, reuse cached data to maintain row position
                if (!forceRefresh && gvItems.EditIndex >= 0 && Session["ItemsGridData"] != null)
                {
                    items = (List<Item>)Session["ItemsGridData"];
                }
                else
                {
                    var repo = new ItemsRepository();
                    string sortBy = ViewState["ItemsSortExpression"] as string ?? "SortOrder";
                    
                    // Get search filter from session if exists
                    string searchFilter = Session["SearchItemContains"] as string;
                    
                    if (!string.IsNullOrEmpty(searchFilter) && searchFilter != "%")
                    {
                        // TODO: Implement search in repository
                        items = repo.GetAll(sortBy);
                        // For now, filter in memory (not ideal, but works)
                        items = items.Where(i => i.ItemDesc != null && 
                                                i.ItemDesc.IndexOf(searchFilter.Replace("%", ""), 
                                                StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                    }
                    else
                    {
                        items = repo.GetAll(sortBy);
                    }
                    
                    // Cache the data for edit operations
                    Session["ItemsGridData"] = items;
                }
                
                gvItems.DataSource = items;
                gvItems.DataBind();
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Error loading items: " + ex.Message;
            }
        }

        // Items Search - Go Button Handler
        protected void btnGo_Click(object sender, EventArgs e)
        {
            try
            {
                string searchTerm = tbxItemSearch.Text.Trim();
                if (string.IsNullOrEmpty(searchTerm))
                {
                    Session["SearchItemContains"] = "%"; // Show all
                }
                else
                {
                    Session["SearchItemContains"] = searchTerm;
                }
                
                gvItems.PageIndex = 0; // Reset to first page
                gvItems.EditIndex = -1; // Exit edit mode if active
                BindItemsGrid(forceRefresh: true); // Force refresh for new search
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Error searching: " + ex.Message;
            }
        }

        // Items Search - Reset Button Handler
        protected void btnReset_Click(object sender, EventArgs e)
        {
            try
            {
                tbxItemSearch.Text = string.Empty;
                Session["SearchItemContains"] = "%"; // Show all
                gvItems.PageIndex = 0; // Reset to first page
                gvItems.EditIndex = -1; // Exit edit mode if active
                BindItemsGrid(forceRefresh: true); // Force refresh for reset
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Error resetting: " + ex.Message;
            }
        }

        // Items Search - TextBox TextChanged Handler
        protected void tbxItemSearch_TextChanged(object sender, EventArgs e)
        {
            // Trigger search when text changes
            btnGo_Click(sender, e);
        }

        // Areas Grid - Paging Event Handler
        protected void gvAreas_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvAreas.PageIndex = e.NewPageIndex;
            BindAreasGrid();
        }

        // Areas Grid - Sorting Event Handler
        protected void gvAreas_Sorting(object sender, GridViewSortEventArgs e)
        {
            ViewState["AreasSortExpression"] = e.SortExpression;
            BindAreasGrid();
        }

        protected void gvAreas_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gvAreas.EditIndex = e.NewEditIndex;
            BindAreasGrid();
        }

        protected void gvAreas_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvAreas.EditIndex = -1;
            BindAreasGrid();
        }

        protected void gvAreas_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            try
            {
                int areaId = Convert.ToInt32(gvAreas.DataKeys[e.RowIndex].Value);
                GridViewRow row = gvAreas.Rows[e.RowIndex];

                var tbxAreaName = (TextBox)row.FindControl("tbxAreaName");

                var area = new Area
                {
                    AreaID = areaId,
                    AreaName = tbxAreaName?.Text ?? ""
                };

                var repo = new AreasRepository();
                repo.Update(area);

                gvAreas.EditIndex = -1;
                BindAreasGrid();
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Error updating area: " + ex.Message;
            }
        }

        // Helper method to bind Cities grid with repository pattern
        private void BindAreasGrid()
        {
            try
            {
                var repo = new AreasRepository();
                string sortBy = ViewState["AreasSortExpression"] as string ?? "AreaName";
                
                var cities = repo.GetAll(sortBy);
                
                gvAreas.DataSource = cities;
                gvAreas.DataBind();
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Error loading areas: " + ex.Message;
            }
        }

        protected void gvPeople_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            try
            {
                GridViewRow row = gvPeople.Rows[e.RowIndex];

                var tbxPerson = (TextBox)row.FindControl("tbxPersonName");
                var tbxAbbreviation = (TextBox)row.FindControl("tbxAbbreviation");
                var cbxEnabled = (CheckBox)row.FindControl("cbxEnabled");
                var ddlDayOfWeek = (DropDownList)row.FindControl("ddlDayOfWeek");
                var ddlSecurityNames = (DropDownList)row.FindControl("ddlSecurityNames");

                var pPerson = new Person
                {
                    PersonID = Convert.ToInt32(gvPeople.DataKeys[e.RowIndex].Value),
                    PersonName = tbxPerson?.Text ?? "",
                    Abbreviation = tbxAbbreviation?.Text ?? "",
                    Enabled = cbxEnabled != null && cbxEnabled.Checked,
                    NormalDeliveryDoW = ddlDayOfWeek != null ? (int?)Convert.ToInt32(ddlDayOfWeek.SelectedValue) : 0,
                    SecurityUsername = ddlSecurityNames != null ? ddlSecurityNames.SelectedValue : ""
                };

                // SAFEGUARD: if selected username no longer exists, force blank
                if (ddlSecurityNames != null && ddlSecurityNames.Items.FindByValue(pPerson.SecurityUsername) == null)
                {
                    pPerson.SecurityUsername = string.Empty;
                }

                var repo = new PersonsRepository();
                repo.Update(pPerson);

                gvPeople.EditIndex = -1;
                BindPeopleGrid();
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Error updating record: " + ex.Message;
            }
        }
        protected void gvPeople_RowEditing(object sender, GridViewEditEventArgs e)
        {
            try
            {
                gvPeople.EditIndex = e.NewEditIndex;
                BindPeopleGrid();
                upnlPeople.Update();
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"RowEditing error: {ex.Message}";
            }
        }
        protected void gvPeople_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) return;

            // ddlDayOfWeek is present in both ItemTemplate and EditItemTemplate.
            // If the bound NormalDeliveryDoW value is null/out of range, ASP.NET will throw
            // "SelectedValue is invalid" during binding. Force a safe value.
            var ddlDayOfWeek = (DropDownList)e.Row.FindControl("ddlDayOfWeek");
            if (ddlDayOfWeek != null)
            {
                var dowObj = DataBinder.Eval(e.Row.DataItem, "NormalDeliveryDoW");
                var dowValue = dowObj == null || dowObj == DBNull.Value ? "0" : dowObj.ToString();
                if (string.IsNullOrWhiteSpace(dowValue))
                    dowValue = "0";

                if (ddlDayOfWeek.Items.FindByValue(dowValue) != null)
                {
                    ddlDayOfWeek.SelectedValue = dowValue;
                }
                else
                {
                    // Out-of-range value. Default to 'Any Day'.
                    ddlDayOfWeek.SelectedValue = "0";
                }
            }

            bool isEdit = (e.Row.RowState & DataControlRowState.Edit) != 0;
            if (!isEdit) return;

            var ddl = (DropDownList)e.Row.FindControl("ddlSecurityNames");
            if (ddl == null) return;

            string current = (DataBinder.Eval(e.Row.DataItem, "SecurityUsername") as string ?? "").Trim();

            if (string.IsNullOrEmpty(current))
            {
                // Blank / n/a
                if (ddl.Items.FindByValue("") != null)
                    ddl.SelectedValue = "";
                return;
            }

            // Try exact match first
            var existing = ddl.Items.FindByValue(current);
            if (existing != null)
            {
                ddl.SelectedValue = current;
                return;
            }

            // Case-insensitive fallback
            ListItem caseInsensitive = null;
            foreach (ListItem li in ddl.Items)
            {
                if (string.Equals(li.Value, current, StringComparison.OrdinalIgnoreCase))
                {
                    caseInsensitive = li;
                    break;
                }
            }
            if (caseInsensitive != null)
            {
                ddl.SelectedValue = caseInsensitive.Value;
                return;
            }

            // Orphaned username ? inject a marker item
            ddl.Items.Insert(0, new ListItem(current + " (missing)", current));
            ddl.SelectedValue = current;
        }
        protected void gvPeople_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            try
            {
                if (e.CommandName.Equals("AddItem"))
                {
                     var personTbx = (TextBox)gvPeople.FooterRow.FindControl("tbxPersonName");
                    var abrvTbx = (TextBox)gvPeople.FooterRow.FindControl("tbxAbbreviation");
                    var enabledCbx = (CheckBox)gvPeople.FooterRow.FindControl("cbxEnabled");
                    var dowDdl = (DropDownList)gvPeople.FooterRow.FindControl("ddlDayOfWeek");
                    var userDdl = (DropDownList)gvPeople.FooterRow.FindControl("ddlSecurityNames");

                    if (personTbx == null || abrvTbx == null || enabledCbx == null || dowDdl == null || userDdl == null)
                    {
                        lblStatus.Text = "Footer controls missing.";
                        return;
                    }

                    var newPerson = new Person
                    {
                        PersonName = personTbx.Text,
                        Abbreviation = abrvTbx.Text,
                        Enabled = enabledCbx.Checked,
                        NormalDeliveryDoW = Convert.ToInt32(dowDdl.SelectedValue),
                        SecurityUsername = userDdl.SelectedValue
                    };

                    // If user value not in list (shouldn't happen here, but safe)
                    if (userDdl.Items.FindByValue(newPerson.SecurityUsername) == null)
                        newPerson.SecurityUsername = string.Empty;

                    var repo = new PersonsRepository();
                    repo.Insert(newPerson);
                    BindPeopleGrid();
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Command error: " + ex.Message;
            }
        }
        

        protected void dvItems_ItemInserted(object sender, DetailsViewInsertedEventArgs e)
        {
            this.gvItems.FooterRow.Enabled = false;
            this.gvItems.DataBind();
        }

        protected void InsertItemButton_Click(object sender, EventArgs e)
        {
            this.gvItems.FooterRow.Enabled = true;
            this.gvItems.DataBind();
        }

        protected void gvEquipment_UpdateButton_Click(EventArgs e) => this.Response.Write("Do update");

        protected void sdsCities_Selecting(object sender, SqlDataSourceSelectingEventArgs e)
        {
        }

        protected void gvEquipment_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        protected void gvEquipment_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (!e.CommandName.Equals("Insert"))
                return;
            try
            {
                TextBox control1 = (TextBox)this.gvEquipment.FooterRow.FindControl("EquipTypeNameTextBox");
                TextBox control2 = (TextBox)this.gvEquipment.FooterRow.FindControl("EquipTypeDescTextBox");
                
                var equipType = new EquipType
                {
                    EquipTypeName = control1.Text,
                    EquipTypeDescription = control2.Text
                };
                
                var repo = new EquipTypesRepository();
                repo.Insert(equipType);
                
                this.gvEquipment.DataBind();
            }
            catch (Exception ex)
            {
                this.lblStatus.Text = "Error adding record: " + ex.Message;
            }
        }

        protected void odsEquipTypes_OnInserting(object source, ObjectDataSourceMethodEventArgs e)
        {
            IDictionary inputParameters = (IDictionary)e.InputParameters;
            
            var equipType = new EquipType
            {
                EquipTypeName = inputParameters[(object)"EquipTypeName"].ToString(),
                EquipTypeDescription = inputParameters[(object)"EquipTypeDesc"].ToString()
            };
            
            inputParameters.Clear();
            inputParameters.Add((object)"equipType", (object)equipType);
        }

        private void DoItemSearch()
        {
            string text = this.tbxItemSearch.Text;
            this.Session["SearchItemContains"] = !string.IsNullOrEmpty(text) ? (object)$"%{text}%" : (object)"%";
            // sdsItems removed (repository pattern enforced). Use the new search/bind pipeline.
            BindItemsGrid();
            this.upnlItems.Update();
        }

        // REMOVED: Duplicate tbxItemSearch_TextChanged - now defined earlier with new implementation
        // REMOVED: Duplicate btnGo_Click - now defined earlier with new implementation

        protected void gvPackaging_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (!e.Row.RowType.Equals((object)DataControlRowType.DataRow))
                return;
            ItemPackaging dataItem = (ItemPackaging)e.Row.DataItem;

            // Handle BGColour column (index 3)
            if (!string.IsNullOrEmpty(dataItem.BGColour))
            {
                try
                {
                    Color bgColor = ColorTranslator.FromHtml(dataItem.BGColour);
                    e.Row.Cells[3].BackColor = bgColor;

                    // Set contrasting text color for BGColour column
                    int brightness = (int)(bgColor.R * 0.299 + bgColor.G * 0.587 + bgColor.B * 0.114);
                    e.Row.Cells[3].ForeColor = brightness > 128 ? Color.Black : Color.White;
                }
                catch (Exception ex)
                {
                    this.lblStatus.Text = ex.Message;
                }
            }

            // Handle Colour column (index 4) - convert int to hex and display
            if (dataItem.Colour.HasValue && dataItem.Colour.Value != 0)
            {
                try
                {
                    // Convert integer to hex color
                    Color foreColor = Color.FromArgb(dataItem.Colour.Value);
                    string hexValue = $"#{foreColor.R:X2}{foreColor.G:X2}{foreColor.B:X2}";

                    // Set the hex value as text and apply the color as background
                    e.Row.Cells[4].Text = hexValue;
                    e.Row.Cells[4].BackColor = foreColor;

                    // Set contrasting text color for Colour column
                    int brightness = (int)(foreColor.R * 0.299 + foreColor.G * 0.587 + foreColor.B * 0.114);
                    e.Row.Cells[4].ForeColor = brightness > 128 ? Color.Black : Color.White;
                }
                catch (Exception ex)
                {
                    this.lblStatus.Text = ex.Message;
                }
            }
        }

        protected void gvPackaging_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (!e.CommandName.Equals("Insert") && !e.CommandName.Equals("AddItem"))
                return;
            try
            {
                TextBox controlDescription = (TextBox)this.gvPackaging.FooterRow.FindControl("TextBoxDescription");
                TextBox controlAdditionalNotes = (TextBox)this.gvPackaging.FooterRow.FindControl("TextBoxAdditionalNotes");
                TextBox controlBGColour = (TextBox)this.gvPackaging.FooterRow.FindControl("TextBoxBGColour");
                TextBox controlColour = (TextBox)this.gvPackaging.FooterRow.FindControl("TextBoxColour");
                TextBox controlSymbol = (TextBox)this.gvPackaging.FooterRow.FindControl("TextBoxSymbol");
                
                var packaging = new ItemPackaging
                {
                    ItemPackagingDesc = controlDescription.Text,
                    AdditionalNotes = controlAdditionalNotes.Text,
                    BGColour = controlBGColour.Text,
                    Colour = string.IsNullOrEmpty(controlColour.Text) ? 0 : Convert.ToInt32(controlColour.Text),
                    Symbol = controlSymbol.Text
                };
                
                var repo = new ItemPackagingsRepository();
                repo.Insert(packaging);
                BindPackagingGrid();
            }
            catch (Exception ex)
            {
                this.lblStatus.Text = "Error adding record: " + ex.Message;
            }
        }

        protected void ColorPickerExtBGColour_OnClientColorSelectionChanged(object sender, EventArgs e)
        {
            TextBox control = (TextBox)this.gvPackaging.FindControl("TextBoxBGColour");
            control.Text = "#" + control.Text;
        }

        // REMOVED: Duplicate btnReset_Click - now defined earlier with new implementation

        protected void gvAreas_OnRowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (!e.CommandName.Equals("AddArea"))
                return;
            try
            {
                TextBox control = (TextBox)this.gvAreas.FooterRow.FindControl("tbxAreaName");
                
                // Use Repository Pattern instead of SqlDataSource
                var newArea = new Area
                {
                    AreaName = control.Text,
                };
                
                var repo = new AreasRepository();
                repo.Insert(newArea);
                
                BindAreasGrid(); // Refresh grid
            }
            catch (Exception ex)
            {
                this.lblStatus.Text = "Error adding record: " + ex.Message;
            }
        }

        protected void gvAreas_OnSelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.gvAreas.SelectedDataKey.Values.Count <= 0)
                return;
            this.gvAreaDays.Visible = true;
            BindAreaDaysGrid();
        }

        private void BindAreaDaysGrid()
        {
            try
            {
                if (gvAreas.SelectedDataKey == null || gvAreas.SelectedDataKey.Value == null)
                {
                    gvAreaDays.Visible = false;
                    return;
                }

                int areaId = Convert.ToInt32(gvAreas.SelectedDataKey.Value);
                var repo = new AreaPrepDaysRepository();
                var filtered = repo.GetAll("DeliveryOrder, PrepDayOfWeekID")
                    .Where(x => x.AreaID == areaId)
                    .ToList();

                gvAreaDays.DataSource = filtered;
                gvAreaDays.DataBind();
                if (filtered.Count == 0)
                    lblStatus.Text = "No delivery days for this area yet — use Add Prep Day below.";
                else if (lblStatus.Text != null && lblStatus.Text.StartsWith("No delivery days", StringComparison.Ordinal))
                    lblStatus.Text = string.Empty;
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Error loading area prep days: " + ex.Message;
            }
        }

        protected void btnAddAreaDay_Click(object sender, EventArgs e)
        {
            try
            {
                DropDownList control1 = FindControlRecursive(gvAreaDays, "ddlPreperationDoW") as DropDownList;
                TextBox control2 = FindControlRecursive(gvAreaDays, "tbxDeliveryDelay") as TextBox;
                TextBox control3 = FindControlRecursive(gvAreaDays, "tbxDeliveryOrder") as TextBox;

                if (control1 == null || control2 == null || control3 == null)
                {
                    lblStatus.Text = "Could not find delivery-day fields to add.";
                    return;
                }

                if (gvAreas.SelectedDataKey == null || gvAreas.SelectedDataKey.Value == null)
                {
                    lblStatus.Text = "Select an area first.";
                    return;
                }

                int areaId = Convert.ToInt32(this.gvAreas.SelectedDataKey.Value);

                var prepDay = new AreaPrepDays
                {
                    AreaID = areaId,
                    PrepDayOfWeekID = string.IsNullOrEmpty(control1.SelectedValue) ? (byte?)null : Convert.ToByte(control1.SelectedValue),
                    DeliveryDelayDays = string.IsNullOrEmpty(control2.Text) ? (short?)null : Convert.ToInt16(control2.Text),
                    DeliveryOrder = string.IsNullOrEmpty(control3.Text) ? (short?)null : Convert.ToInt16(control3.Text)
                };

                var sql = "INSERT INTO AreaPrepDaysTbl (AreaID, PrepDayOfWeekID, DeliveryDelayDays, DeliveryOrder) VALUES (@a, @p, @d, @o)";
                var p = new List<TrackerSQL.Classes.DBParameter>
                {
                    new TrackerSQL.Classes.DBParameter { ParamName = "@a", DataDbType = System.Data.DbType.Int32, DataValue = prepDay.AreaID },
                    new TrackerSQL.Classes.DBParameter { ParamName = "@p", DataDbType = System.Data.DbType.Byte, DataValue = (object)prepDay.PrepDayOfWeekID ?? DBNull.Value },
                    new TrackerSQL.Classes.DBParameter { ParamName = "@d", DataDbType = System.Data.DbType.Int16, DataValue = (object)prepDay.DeliveryDelayDays ?? DBNull.Value },
                    new TrackerSQL.Classes.DBParameter { ParamName = "@o", DataDbType = System.Data.DbType.Int16, DataValue = (object)prepDay.DeliveryOrder ?? DBNull.Value }
                };
                ExecNonQuery(sql, p);

                BindAreaDaysGrid();
                this.upnlAreas.Update();
            }
            catch (Exception ex)
            {
                this.lblStatus.Text = "Error adding area prep day: " + ex.Message;
            }
        }

        protected void gvAreaDays_OnRowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            // The actual update logic is in gvAreaDays_RowCommand
            // This method just needs to reset the EditIndex
            gvAreaDays.EditIndex = -1;
            BindAreaDaysGrid();
        }

        protected void gvAreaDays_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gvAreaDays.EditIndex = e.NewEditIndex;
            BindAreaDaysGrid();
        }

        protected void gvAreaDays_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvAreaDays.EditIndex = -1;
            BindAreaDaysGrid();
        }

        protected void gvAreaDays_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            // The actual delete logic is handled in gvAreaDays_RowCommand
            // This method exists to satisfy the event binding
        }

        protected void gvAreaDays_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            try
            {
                if (e.CommandName.Equals("Update") || e.CommandName.Equals("AddAreaDays"))
                {
                    GridViewRow gridViewRow = e.CommandName.Equals("Update") ? this.gvAreaDays.Rows[this.gvAreaDays.EditIndex] : this.gvAreaDays.FooterRow;
                    DropDownList control1 = (DropDownList)gridViewRow.FindControl("ddlPreperationDoW");
                    TextBox control2 = (TextBox)gridViewRow.FindControl("tbxDeliveryDelay");
                    TextBox control3 = (TextBox)gridViewRow.FindControl("tbxDeliveryOrder");
                    var controlId = (HiddenField)gridViewRow.FindControl("AreaPrepDaysIDHidden");

                    int areaId = Convert.ToInt32(this.gvAreas.SelectedDataKey.Value);

                    var prepDay = new AreaPrepDays
                    {
                        AreaID = areaId,
                        PrepDayOfWeekID = string.IsNullOrEmpty(control1.SelectedValue) ? (byte?)null : Convert.ToByte(control1.SelectedValue),
                        DeliveryDelayDays = string.IsNullOrEmpty(control2.Text) ? (short?)null : Convert.ToInt16(control2.Text),
                        DeliveryOrder = string.IsNullOrEmpty(control3.Text) ? (short?)null : Convert.ToInt16(control3.Text)
                    };

                    if (e.CommandName.Equals("Update"))
                    {
                        prepDay.AreaPrepDaysID = controlId != null ? Convert.ToInt32(controlId.Value) : 0;
                        var sql = "UPDATE AreaPrepDaysTbl SET AreaID=@a, PrepDayOfWeekID=@p, DeliveryDelayDays=@d, DeliveryOrder=@o WHERE AreaPrepDaysID=@id";
                        var p = new List<TrackerSQL.Classes.DBParameter>
                        {
                            new TrackerSQL.Classes.DBParameter { ParamName = "@a", DataDbType = System.Data.DbType.Int32, DataValue = prepDay.AreaID },
                            new TrackerSQL.Classes.DBParameter { ParamName = "@p", DataDbType = System.Data.DbType.Byte, DataValue = (object)prepDay.PrepDayOfWeekID ?? DBNull.Value },
                            new TrackerSQL.Classes.DBParameter { ParamName = "@d", DataDbType = System.Data.DbType.Int16, DataValue = (object)prepDay.DeliveryDelayDays ?? DBNull.Value },
                            new TrackerSQL.Classes.DBParameter { ParamName = "@o", DataDbType = System.Data.DbType.Int16, DataValue = (object)prepDay.DeliveryOrder ?? DBNull.Value },
                            new TrackerSQL.Classes.DBParameter { ParamName = "@id", DataDbType = System.Data.DbType.Int32, DataValue = prepDay.AreaPrepDaysID },
                        };
                        ExecNonQuery(sql, p);
                    }
                    else
                    {
                        var sql = "INSERT INTO AreaPrepDaysTbl (AreaID, PrepDayOfWeekID, DeliveryDelayDays, DeliveryOrder) VALUES (@a, @p, @d, @o)";
                        var p = new List<TrackerSQL.Classes.DBParameter>
                        {
                            new TrackerSQL.Classes.DBParameter { ParamName = "@a", DataDbType = System.Data.DbType.Int32, DataValue = prepDay.AreaID },
                            new TrackerSQL.Classes.DBParameter { ParamName = "@p", DataDbType = System.Data.DbType.Byte, DataValue = (object)prepDay.PrepDayOfWeekID ?? DBNull.Value },
                            new TrackerSQL.Classes.DBParameter { ParamName = "@d", DataDbType = System.Data.DbType.Int16, DataValue = (object)prepDay.DeliveryDelayDays ?? DBNull.Value },
                            new TrackerSQL.Classes.DBParameter { ParamName = "@o", DataDbType = System.Data.DbType.Int16, DataValue = (object)prepDay.DeliveryOrder ?? DBNull.Value }
                        };
                        ExecNonQuery(sql, p);
                    }

                    this.gvAreaDays.EditIndex = -1;
                    BindAreaDaysGrid();
                    this.upnlAreas.Update();
                }
                else if (e.CommandName.Equals("Delete"))
                {
                    var controlId = (HiddenField)((Control)e.CommandSource).NamingContainer.FindControl("AreaPrepDaysIDHidden");
                    int id = controlId != null ? Convert.ToInt32(controlId.Value) : 0;
                    var sql = "DELETE FROM AreaPrepDaysTbl WHERE AreaPrepDaysID=@id";
                    var p = new List<TrackerSQL.Classes.DBParameter>
                    {
                        new TrackerSQL.Classes.DBParameter { ParamName = "@id", DataDbType = System.Data.DbType.Int32, DataValue = id }
                    };
                    ExecNonQuery(sql, p);
                    BindAreaDaysGrid();
                    this.upnlAreas.Update();
                }
            }
            catch (Exception ex)
            {
                this.lblStatus.Text = "Error updating area prep days: " + ex.Message;
            }
        }

        public string GetPrepDayName(object prepDayOfWeekId)
        {
            if (prepDayOfWeekId == null || prepDayOfWeekId == DBNull.Value)
                return string.Empty;

            if (!int.TryParse(prepDayOfWeekId.ToString(), out int dayId) || dayId < 1 || dayId > 7)
                return string.Empty;

            string[] names =
            {
                "", "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"
            };
            return names[dayId];
        }

        public string GetDeliveryDay(string pPredDoW, string pDeliveryDelay)
        {
            int result1 = 0;
            int result2 = 0;
            if (!int.TryParse(pPredDoW, out result1))
                result1 = 1;
            if (!int.TryParse(pDeliveryDelay, out result2))
                result2 = 1;
            int num = result1 + result2;
            string[] strArray = new string[7]
            {"Sun",
                "Mon",
                "Tue",
                "Wed",
                "Thu",
                "Fri",
                "Sat"
            };
            if (num > 7)
                num -= 7;
            return strArray[num - 1];
        }

        protected void gvInvoiceTypes_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            GridViewRow namingContainer = (GridViewRow)((Control)e.CommandSource).NamingContainer;
            if (namingContainer == null)
                return;
            TextBox controlTypeDesc = (TextBox)namingContainer.FindControl("InvoiceTypeDescTextBox");
            if (controlTypeDesc == null || string.IsNullOrEmpty(controlTypeDesc.Text))
                return;
            
            var controlInvoiceTypeID = (HiddenField)namingContainer.FindControl("InvoiceTypeIDHidden");
            int invoiceTypeID = controlInvoiceTypeID != null ? Convert.ToInt32(controlInvoiceTypeID.Value) : 0;
            
            var repo = new InvoiceTypesRepository();
            
            if (e.CommandName.Equals("Delete"))
            {
                repo.Delete(invoiceTypeID);
            }
            else
            {
                CheckBox controlEnabled = (CheckBox)namingContainer.FindControl("EnabledCheckBox");
                TextBox controlNotes = (TextBox)namingContainer.FindControl("NotesTextBox");
                
                var invoiceType = new InvoiceType
                {
                    InvoiceTypeID = invoiceTypeID,
                    InvoiceTypeDesc = controlTypeDesc.Text,
                    Enabled = controlEnabled != null && controlEnabled.Checked,
                    Notes = controlNotes != null ? controlNotes.Text : string.Empty
                };
                
                if (e.CommandName.Equals("Add") || e.CommandName.Equals("Insert"))
                    repo.Insert(invoiceType);
                else if (e.CommandName.Equals("Update"))
                    repo.Update(invoiceType);
            }
            BindInvoiceTypesGrid();
        }

        protected void gvPriceLevels_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            GridViewRow namingContainer = (GridViewRow)((Control)e.CommandSource).NamingContainer;
            if (namingContainer == null)
                return;
            TextBox controlDesc = (TextBox)namingContainer.FindControl("PriceLevelDescTextBox");
            if (controlDesc == null || string.IsNullOrEmpty(controlDesc.Text))
                return;
            
            TextBox controlFactor = (TextBox)namingContainer.FindControl("PricingFactorTextBox");
            CheckBox controlEnabled = (CheckBox)namingContainer.FindControl("EnabledCheckBox");
            TextBox controlNotes = (TextBox)namingContainer.FindControl("NotesTextBox");
            var controlID = (HiddenField)namingContainer.FindControl("hdnPriceLevelID");
            
            var priceLevel = new PriceLevel
            {
                PriceLevelID = controlID != null ? Convert.ToInt32(controlID.Value) : 0,
                PriceLevelDesc = controlDesc.Text,
                PricingFactor = controlFactor != null ? (double)Convert.ToSingle(controlFactor.Text) : 1.0,
                Enabled = controlEnabled != null && controlEnabled.Checked,
                Notes = controlNotes != null ? controlNotes.Text : string.Empty
            };
            
            var repo = new PriceLevelsRepository();
            
            if (e.CommandName.Equals("Add") || e.CommandName.Equals("Insert"))
                repo.Insert(priceLevel);
            else if (e.CommandName.Equals("Update"))
                repo.Update(priceLevel);
            else if (e.CommandName.Equals("Delete"))
                repo.Delete(priceLevel.PriceLevelID);
                
            BindPriceLevelsGrid();
        }

        protected void gvPaymentTerms_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            GridViewRow namingContainer = (GridViewRow)((Control)e.CommandSource).NamingContainer;
            if (namingContainer == null)
                return;
            var controlDesc = (TextBox)namingContainer.FindControl("PaymentTermDescTextBox");
            if (controlDesc == null || string.IsNullOrEmpty(controlDesc.Text))
                return;
            
            var controlPaymentDays = (TextBox)namingContainer.FindControl("PaymentDaysTextBox");
            var controlDayOfMonth = (TextBox)namingContainer.FindControl("DayOfMonthTextBox");
            var controlUseDays = (CheckBox)namingContainer.FindControl("UseDaysCheckBox");
            var controlEnabled = (CheckBox)namingContainer.FindControl("EnabledCheckBox");
            var controlNotes = (TextBox)namingContainer.FindControl("NotesTextBox");
            var controlPaymentTermID = (HiddenField)namingContainer.FindControl("PaymentTermIDHidden");
            
            var paymentTerm = new PaymentTerm
            {
                PaymentTermID = controlPaymentTermID != null ? Convert.ToInt32(controlPaymentTermID.Value) : 0,
                PaymentTermDesc = controlDesc.Text,
                PaymentDays = controlPaymentDays != null ? Convert.ToInt32(controlPaymentDays.Text) : 0,
                DayOfMonth = controlDayOfMonth != null ? Convert.ToInt32(controlDayOfMonth.Text) : 0,
                UseDays = controlUseDays != null && controlUseDays.Checked,
                Enabled = controlEnabled == null || controlEnabled.Checked,
                Notes = controlNotes != null ? controlNotes.Text : string.Empty
            };
            
            var repo = new PaymentTermsRepository();
            
            if (e.CommandName.Equals("Add") || e.CommandName.Equals("Insert"))
                repo.Insert(paymentTerm);
            else if (e.CommandName.Equals("Update"))
                repo.Update(paymentTerm);
            else if (e.CommandName.Equals("Delete"))
                repo.Delete(paymentTerm.PaymentTermID);
                
            BindPaymentTermsGrid();
        }

        private static Control FindControlRecursive(Control root, string id)
        {
            if (root == null || string.IsNullOrEmpty(id))
                return null;

            if (string.Equals(root.ID, id, StringComparison.Ordinal))
                return root;

            foreach (Control child in root.Controls)
            {
                Control found = FindControlRecursive(child, id);
                if (found != null)
                    return found;
            }

            return null;
        }
    }
}
