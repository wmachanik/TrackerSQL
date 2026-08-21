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
using TrackerSQL.Classes;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

//- only form later versions #nullable disable
namespace TrackerSQL.Pages
{
    public partial class Lookups : Page
    {
        private const string DefaultItemsSort = "SortOrder, ItemDesc";
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
        protected TextBox tbxPeopleSearch;
        protected Button btnPeopleGo;
        protected Button btnPeopleReset;
        protected GridView gvPeople;
        protected TabPanel tabpnlEquipment;
        protected UpdatePanel upnlEquipment;
        protected TextBox tbxEquipSearch;
        protected Button btnEquipGo;
        protected Button btnEquipReset;
        protected GridView gvEquipment;
        protected TabPanel tabpnlAreas;
        protected UpdatePanel upnlAreas;
        protected TextBox tbxAreaSearch;
        protected Button btnAreaGo;
        protected Button btnAreaReset;
        protected GridView gvAreas;
        protected GridView gvAreaDays;
        protected TabPanel tabpnlPackaging;
        protected UpdatePanel upnlPackaging;
        protected UpdatePanel upnlLookupStatus;
        protected TextBox tbxPackagingSearch;
        protected Button btnPackagingGo;
        protected Button btnPackagingReset;
        protected GridView gvPackaging;
        protected TabPanel tabInvoiceTypes;
        protected UpdateProgress gvInvoiceTypesUpdateProgress;
        protected UpdatePanel gvInvoiceTypesUpdatePanel;
        protected TextBox tbxInvoiceTypeSearch;
        protected Button btnInvoiceTypeGo;
        protected Button btnInvoiceTypeReset;
        protected GridView gvInvoiceTypes;
        protected TabPanel tabPaymentTerms;
        protected UpdateProgress PaymentTermsUpdateProgress;
        protected UpdatePanel gvPaymentTermsUpdatePanel;
        protected TextBox tbxPaymentTermSearch;
        protected Button btnPaymentTermGo;
        protected Button btnPaymentTermReset;
        protected GridView gvPaymentTerms;
        protected TabPanel tabPriceLevels;
        protected UpdateProgress PriceLevelUpdateProgress;
        protected UpdatePanel gvPriceLevelsUpdatePanel;
        protected TextBox tbxPriceLevelSearch;
        protected Button btnPriceLevelGo;
        protected Button btnPriceLevelReset;
        protected GridView gvPriceLevels;
        protected TabPanel tabpnlRepairStatuses;
        protected UpdatePanel upnlRepairStatuses;
        protected TextBox tbxRepairStatusSearch;
        protected Button btnRepairStatusGo;
        protected Button btnRepairStatusReset;
        protected GridView gvRepairStatuses;
        protected TabPanel tabpnlSortOrders;
        protected UpdatePanel upnlSortOrders;
        protected GridView gvSortOrders;
        protected SqlDataSource sdsUserNames;

        // Per-request caches for Items grid lookup dropdowns (filled once per bind)
        private List<ItemUnit> _itemUnitsCache;
        private List<ItemServiceType> _itemServiceTypesCache;
        private List<OrderItemLookup> _replacementItemsCache;
        private ItemSortOrdersRepository _sortOrdersRepo;

        private ItemSortOrdersRepository GetSortOrdersRepo()
        {
            if (_sortOrdersRepo == null)
                _sortOrdersRepo = new ItemSortOrdersRepository();
            return _sortOrdersRepo;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            RegisterLookupPostBackControls();

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
                BindSortOrdersGrid();
            }
        }

        private void RegisterLookupPostBackControls()
        {
            var sm = ScriptManager.GetCurrent(Page);
            if (sm == null)
                return;

            // TabContainer + nested UpdatePanels: search Go/Reset need explicit registration.
            foreach (Control btn in new Control[]
            {
                btnRepairStatusGo, btnRepairStatusReset,
                btnPeopleGo, btnPeopleReset,
                btnPackagingGo, btnPackagingReset,
                btnInvoiceTypeGo, btnInvoiceTypeReset,
                btnPaymentTermGo, btnPaymentTermReset,
                btnPriceLevelGo, btnPriceLevelReset
            })
            {
                if (btn != null)
                    sm.RegisterAsyncPostBackControl(btn);
            }
        }

        protected void Page_PreRender(object sender, EventArgs e)
        {
            ApplyLookupStatusUi();

            // Async tab edits set lblStatus outside their own UpdatePanel — include the
            // shared status strip in the response whenever it has text.
            var sm = ScriptManager.GetCurrent(Page);
            if (sm != null && sm.IsInAsyncPostBack && !string.IsNullOrWhiteSpace(lblStatus?.Text))
                upnlLookupStatus?.Update();
        }

        /// <summary>
        /// Single page-level status under the tabs. Sets colour (error / success / info)
        /// and refreshes the status UpdatePanel so async tab edits show the message.
        /// </summary>
        private void SetLookupStatus(string message, bool? isError = null)
        {
            _lookupStatusIsError = isError;
            if (lblStatus != null)
                lblStatus.Text = message ?? string.Empty;
            ApplyLookupStatusUi();
            upnlLookupStatus?.Update();
        }

        private bool? _lookupStatusIsError;

        private void ApplyLookupStatusUi()
        {
            if (pnlLookupStatus == null)
                return;

            bool hasMessage = !string.IsNullOrWhiteSpace(lblStatus?.Text);
            pnlLookupStatus.Visible = hasMessage;
            if (!hasMessage)
            {
                pnlLookupStatus.Attributes["class"] = "status-message";
                return;
            }

            bool isError = _lookupStatusIsError
                ?? InferLookupStatusIsError(lblStatus.Text);
            if (isError)
                pnlLookupStatus.Attributes["class"] = "status-message status-error";
            else if (_lookupStatusIsError == false)
                pnlLookupStatus.Attributes["class"] = "status-message status-success";
            else
                pnlLookupStatus.Attributes["class"] = "status-message status-info";
        }

        private static bool InferLookupStatusIsError(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;
            return text.IndexOf("error", StringComparison.OrdinalIgnoreCase) >= 0
                || text.IndexOf("fail", StringComparison.OrdinalIgnoreCase) >= 0
                || text.StartsWith("Could not", StringComparison.OrdinalIgnoreCase)
                || text.StartsWith("Footer controls missing", StringComparison.OrdinalIgnoreCase);
        }

        private void BindPeopleGrid()
        {
            try
            {
                var repo = new PersonsRepository();
                string sortBy = ViewState["PeopleSortExpression"] as string ?? "Abbreviation";
                var people = repo.GetAll(sortBy);

                string searchFilter = tbxPeopleSearch != null
                    ? tbxPeopleSearch.Text.Trim()
                    : string.Empty;
                if (!string.IsNullOrEmpty(searchFilter))
                {
                    people = people.Where(p =>
                        (p.PersonName != null
                            && p.PersonName.IndexOf(searchFilter, StringComparison.OrdinalIgnoreCase) >= 0)
                        || (p.Abbreviation != null
                            && p.Abbreviation.IndexOf(searchFilter, StringComparison.OrdinalIgnoreCase) >= 0))
                        .ToList();
                }

                gvPeople.DataSource = people;
                gvPeople.DataBind();
            }
            catch (Exception ex)
            {
                SetLookupStatus("Error loading people: " + ex.Message, true);
            }
        }

        protected void btnPeopleGo_Click(object sender, EventArgs e)
        {
            gvPeople.EditIndex = -1;
            gvPeople.PageIndex = 0;
            BindPeopleGrid();
        }

        protected void btnPeopleReset_Click(object sender, EventArgs e)
        {
            if (tbxPeopleSearch != null)
                tbxPeopleSearch.Text = string.Empty;
            gvPeople.EditIndex = -1;
            gvPeople.PageIndex = 0;
            BindPeopleGrid();
        }

        protected void tbxPeopleSearch_TextChanged(object sender, EventArgs e)
        {
            btnPeopleGo_Click(sender, e);
        }

        private void BindEquipmentGrid()
        {
            try
            {
                var repo = new EquipTypesRepository();
                string sortBy = ViewState["EquipTypesSortExpression"] as string ?? "EquipTypeName";
                var equip = repo.GetAll(sortBy);

                // Filter is always re-read from the textbox so edit row indexes
                // stay in sync with what is on screen (same approach as Items).
                string searchFilter = tbxEquipSearch != null
                    ? tbxEquipSearch.Text.Trim()
                    : string.Empty;

                if (!string.IsNullOrEmpty(searchFilter))
                {
                    equip = equip.Where(t =>
                        (t.EquipTypeName != null
                            && t.EquipTypeName.IndexOf(searchFilter, StringComparison.OrdinalIgnoreCase) >= 0)
                        || (t.EquipTypeDescription != null
                            && t.EquipTypeDescription.IndexOf(searchFilter, StringComparison.OrdinalIgnoreCase) >= 0))
                        .ToList();
                }

                gvEquipment.DataSource = equip;
                gvEquipment.DataBind();
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Error loading equipment: " + ex.Message;
            }
        }

        protected void btnEquipGo_Click(object sender, EventArgs e)
        {
            gvEquipment.EditIndex = -1;
            gvEquipment.PageIndex = 0;
            BindEquipmentGrid();
        }

        protected void btnEquipReset_Click(object sender, EventArgs e)
        {
            if (tbxEquipSearch != null)
                tbxEquipSearch.Text = string.Empty;
            gvEquipment.EditIndex = -1;
            gvEquipment.PageIndex = 0;
            BindEquipmentGrid();
        }

        protected void tbxEquipSearch_TextChanged(object sender, EventArgs e)
        {
            btnEquipGo_Click(sender, e);
        }

        private void BindPackagingGrid()
        {
            try
            {
                var repo = new ItemPackagingsRepository();
                string sortBy = ViewState["PackagingSortExpression"] as string ?? "ItemPackagingDesc";
                var packagings = repo.GetAll(sortBy);

                string searchFilter = tbxPackagingSearch != null
                    ? tbxPackagingSearch.Text.Trim()
                    : string.Empty;
                if (!string.IsNullOrEmpty(searchFilter))
                {
                    packagings = packagings.Where(p =>
                        p.ItemPackagingDesc != null
                        && p.ItemPackagingDesc.IndexOf(searchFilter, StringComparison.OrdinalIgnoreCase) >= 0)
                        .ToList();
                }

                gvPackaging.DataSource = packagings;
                gvPackaging.DataBind();
            }
            catch (Exception ex)
            {
                SetLookupStatus("Error loading packaging: " + ex.Message, true);
            }
        }

        protected void btnPackagingGo_Click(object sender, EventArgs e)
        {
            gvPackaging.EditIndex = -1;
            gvPackaging.PageIndex = 0;
            BindPackagingGrid();
        }

        protected void btnPackagingReset_Click(object sender, EventArgs e)
        {
            if (tbxPackagingSearch != null)
                tbxPackagingSearch.Text = string.Empty;
            gvPackaging.EditIndex = -1;
            gvPackaging.PageIndex = 0;
            BindPackagingGrid();
        }

        protected void tbxPackagingSearch_TextChanged(object sender, EventArgs e)
        {
            btnPackagingGo_Click(sender, e);
        }

        private void BindInvoiceTypesGrid()
        {
            try
            {
                var repo = new InvoiceTypesRepository();
                string sortBy = ViewState["InvoiceTypesSortExpression"] as string ?? "InvoiceTypeDesc";
                var invoiceTypes = repo.GetAll(sortBy);

                // Textbox ViewState is authoritative after search (same as Items tab).
                string searchFilter = tbxInvoiceTypeSearch != null
                    ? tbxInvoiceTypeSearch.Text.Trim()
                    : string.Empty;
                if (!string.IsNullOrEmpty(searchFilter))
                {
                    invoiceTypes = invoiceTypes.Where(t =>
                        t.InvoiceTypeDesc != null
                        && t.InvoiceTypeDesc.IndexOf(searchFilter, StringComparison.OrdinalIgnoreCase) >= 0)
                        .ToList();
                }

                gvInvoiceTypes.DataSource = invoiceTypes;
                gvInvoiceTypes.DataBind();
            }
            catch (Exception ex)
            {
                SetLookupStatus("Error loading invoice types: " + ex.Message, true);
            }
        }

        protected void btnInvoiceTypeGo_Click(object sender, EventArgs e)
        {
            gvInvoiceTypes.EditIndex = -1;
            gvInvoiceTypes.PageIndex = 0;
            BindInvoiceTypesGrid();
            gvInvoiceTypesUpdatePanel?.Update();
        }

        protected void btnInvoiceTypeReset_Click(object sender, EventArgs e)
        {
            if (tbxInvoiceTypeSearch != null)
                tbxInvoiceTypeSearch.Text = string.Empty;
            gvInvoiceTypes.EditIndex = -1;
            gvInvoiceTypes.PageIndex = 0;
            BindInvoiceTypesGrid();
            gvInvoiceTypesUpdatePanel?.Update();
        }

        protected void tbxInvoiceTypeSearch_TextChanged(object sender, EventArgs e)
        {
            btnInvoiceTypeGo_Click(sender, e);
        }

        private void BindPaymentTermsGrid()
        {
            try
            {
                var repo = new PaymentTermsRepository();
                string sortBy = ViewState["PaymentTermsSortExpression"] as string ?? "PaymentTermDesc";
                var terms = repo.GetAll(sortBy);

                string searchFilter = tbxPaymentTermSearch != null
                    ? tbxPaymentTermSearch.Text.Trim()
                    : string.Empty;
                if (!string.IsNullOrEmpty(searchFilter))
                {
                    terms = terms.Where(t =>
                        t.PaymentTermDesc != null
                        && t.PaymentTermDesc.IndexOf(searchFilter, StringComparison.OrdinalIgnoreCase) >= 0)
                        .ToList();
                }

                gvPaymentTerms.DataSource = terms;
                gvPaymentTerms.DataBind();
            }
            catch (Exception ex)
            {
                SetLookupStatus("Error loading payment terms: " + ex.Message, true);
            }
        }

        protected void btnPaymentTermGo_Click(object sender, EventArgs e)
        {
            gvPaymentTerms.EditIndex = -1;
            gvPaymentTerms.PageIndex = 0;
            BindPaymentTermsGrid();
        }

        protected void btnPaymentTermReset_Click(object sender, EventArgs e)
        {
            if (tbxPaymentTermSearch != null)
                tbxPaymentTermSearch.Text = string.Empty;
            gvPaymentTerms.EditIndex = -1;
            gvPaymentTerms.PageIndex = 0;
            BindPaymentTermsGrid();
        }

        protected void tbxPaymentTermSearch_TextChanged(object sender, EventArgs e)
        {
            btnPaymentTermGo_Click(sender, e);
        }

        private void BindPriceLevelsGrid()
        {
            try
            {
                var repo = new PriceLevelsRepository();
                string sortBy = ViewState["PriceLevelsSortExpression"] as string ?? "PriceLevelDesc";
                var levels = repo.GetAll(sortBy);

                string searchFilter = tbxPriceLevelSearch != null
                    ? tbxPriceLevelSearch.Text.Trim()
                    : string.Empty;
                if (!string.IsNullOrEmpty(searchFilter))
                {
                    levels = levels.Where(l =>
                        l.PriceLevelDesc != null
                        && l.PriceLevelDesc.IndexOf(searchFilter, StringComparison.OrdinalIgnoreCase) >= 0)
                        .ToList();
                }

                gvPriceLevels.DataSource = levels;
                gvPriceLevels.DataBind();
            }
            catch (Exception ex)
            {
                SetLookupStatus("Error loading price levels: " + ex.Message, true);
            }
        }

        protected void btnPriceLevelGo_Click(object sender, EventArgs e)
        {
            gvPriceLevels.EditIndex = -1;
            gvPriceLevels.PageIndex = 0;
            BindPriceLevelsGrid();
        }

        protected void btnPriceLevelReset_Click(object sender, EventArgs e)
        {
            if (tbxPriceLevelSearch != null)
                tbxPriceLevelSearch.Text = string.Empty;
            gvPriceLevels.EditIndex = -1;
            gvPriceLevels.PageIndex = 0;
            BindPriceLevelsGrid();
        }

        protected void tbxPriceLevelSearch_TextChanged(object sender, EventArgs e)
        {
            btnPriceLevelGo_Click(sender, e);
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
            if (upnlPackaging != null)
                upnlPackaging.Update();
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

                if (!TryNormalizeHexColour(tbxBGColour?.Text, out string bgColour)
                    || !TryNormalizeHexColour(tbxColour?.Text, out string colour))
                {
                    e.Cancel = true;
                    SetLookupStatus("Colours must be hex values like #FF0000 (or leave blank).", true);
                    return;
                }

                var packaging = new ItemPackaging
                {
                    ItemPackagingID = packagingId,
                    ItemPackagingDesc = tbxDesc?.Text ?? "",
                    AdditionalNotes = tbxNotes?.Text ?? "",
                    BGColour = bgColour,
                    Colour = string.IsNullOrEmpty(colour) ? null : colour,
                    Symbol = tbxSymbol?.Text ?? ""
                };

                var repo = new ItemPackagingsRepository();
                repo.Update(packaging);

                gvPackaging.EditIndex = -1;
                BindPackagingGrid();
                SetLookupStatus("Packaging updated.", false);
            }
            catch (Exception ex)
            {
                e.Cancel = true;
                SetLookupStatus("Error updating packaging: " + ex.Message, true);
                AppLogger.WriteLog(SystemConstants.LogTypes.System,
                    "Lookups packaging update failed: " + ex.Message);
            }
        }

        /// <summary>
        /// HTML hex for packaging colours (#RRGGBB) — DeliverySheet applies BGColour
        /// directly as background-color on item spans.
        /// Blank input → empty string (valid). Invalid input → false.
        /// </summary>
        private static bool TryNormalizeHexColour(string text, out string normalized)
        {
            normalized = string.Empty;
            if (string.IsNullOrWhiteSpace(text))
                return true;

            string value = text.Trim().TrimStart('#').Trim();
            if (value.Length == 0)
                return true;

            // Legacy Access RGB int stored as digits (e.g. 16711680) → #RRGGBB
            if (int.TryParse(value, out int rgbInt) && value.All(char.IsDigit))
            {
                normalized = "#" + (rgbInt & 0xFFFFFF).ToString("X6");
                return true;
            }

            if (value.Length == 3)
            {
                // #RGB → #RRGGBB
                value = string.Concat(value[0], value[0], value[1], value[1], value[2], value[2]);
            }
            else if (value.Length == 8)
            {
                // Strip leading alpha if present (AARRGGBB → RRGGBB)
                value = value.Substring(2);
            }

            if (value.Length != 6)
                return false;

            foreach (char c in value)
            {
                bool hexDigit = (c >= '0' && c <= '9')
                    || (c >= 'a' && c <= 'f')
                    || (c >= 'A' && c <= 'F');
                if (!hexDigit)
                    return false;
            }

            normalized = "#" + value.ToUpperInvariant();
            return true;
        }

        private static string NormalizeHexColour(string text)
        {
            return TryNormalizeHexColour(text, out string normalized) ? normalized : string.Empty;
        }

        private static bool IsBlankOrHex(string text)
        {
            return TryNormalizeHexColour(text, out _);
        }

        protected void gvInvoiceTypes_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvInvoiceTypes.PageIndex = e.NewPageIndex;
            gvInvoiceTypes.EditIndex = -1;
            BindInvoiceTypesGrid();
            gvInvoiceTypesUpdatePanel?.Update();
        }

        protected void gvInvoiceTypes_Sorting(object sender, GridViewSortEventArgs e)
        {
            ViewState["InvoiceTypesSortExpression"] = e.SortExpression;
            gvInvoiceTypes.EditIndex = -1;
            BindInvoiceTypesGrid();
            gvInvoiceTypesUpdatePanel?.Update();
        }

        protected void gvInvoiceTypes_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gvInvoiceTypes.EditIndex = e.NewEditIndex;
            BindInvoiceTypesGrid();
            gvInvoiceTypesUpdatePanel?.Update();
        }

        protected void gvInvoiceTypes_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvInvoiceTypes.EditIndex = -1;
            BindInvoiceTypesGrid();
            gvInvoiceTypesUpdatePanel?.Update();
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
                SetLookupStatus("Invoice type updated.", false);
                gvInvoiceTypesUpdatePanel?.Update();
            }
            catch (Exception ex)
            {
                SetLookupStatus("Error updating invoice type: " + ex.Message, true);
                gvInvoiceTypesUpdatePanel?.Update();
            }
        }

        protected void gvInvoiceTypes_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            try
            {
                int invoiceTypeID = Convert.ToInt32(gvInvoiceTypes.DataKeys[e.RowIndex].Value);
                new InvoiceTypesRepository().Delete(invoiceTypeID);
                gvInvoiceTypes.EditIndex = -1;
                BindInvoiceTypesGrid();
                SetLookupStatus("Invoice type deleted.", false);
                gvInvoiceTypesUpdatePanel?.Update();
            }
            catch (Exception ex)
            {
                SetLookupStatus("Error deleting invoice type: " + ex.Message, true);
                gvInvoiceTypesUpdatePanel?.Update();
            }
        }

        protected void gvInvoiceTypes_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow && e.Row.RowType != DataControlRowType.Footer)
                return;

            // TabContainer + nested UpdatePanel: full postback for row actions so
            // edit after search refreshes reliably (same approach as Items / Repair Statuses).
            var sm = ScriptManager.GetCurrent(Page);
            if (sm == null)
                return;

            foreach (string id in new[] { "btnInvUpdate", "btnInvCancel", "btnInvEdit", "btnInvDelete", "btnInvAdd" })
            {
                Control btn = e.Row.FindControl(id);
                if (btn != null)
                    sm.RegisterPostBackControl(btn);
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

                    string searchFilter = tbxRepairStatusSearch != null
                        ? tbxRepairStatusSearch.Text.Trim()
                        : string.Empty;
                    if (!string.IsNullOrEmpty(searchFilter))
                    {
                        statuses = statuses.Where(s =>
                            (s.RepairStatusDesc != null
                                && s.RepairStatusDesc.IndexOf(searchFilter, StringComparison.OrdinalIgnoreCase) >= 0)
                            || (s.StatusNote != null
                                && s.StatusNote.IndexOf(searchFilter, StringComparison.OrdinalIgnoreCase) >= 0))
                            .ToList();
                    }
                    
                    // Cache the data for edit operations
                    Session["RepairStatusesGridData"] = statuses;
                }
                
                gvRepairStatuses.DataSource = statuses;
                gvRepairStatuses.DataBind();
            }
            catch (Exception ex)
            {
                SetLookupStatus("Error loading repair statuses: " + ex.Message, true);
            }
        }

        protected void btnRepairStatusGo_Click(object sender, EventArgs e)
        {
            gvRepairStatuses.EditIndex = -1;
            gvRepairStatuses.PageIndex = 0;
            BindRepairStatusesGrid(forceRefresh: true);
            upnlRepairStatuses?.Update();
        }

        protected void btnRepairStatusReset_Click(object sender, EventArgs e)
        {
            if (tbxRepairStatusSearch != null)
                tbxRepairStatusSearch.Text = string.Empty;
            gvRepairStatuses.EditIndex = -1;
            gvRepairStatuses.PageIndex = 0;
            BindRepairStatusesGrid(forceRefresh: true);
            upnlRepairStatuses?.Update();
        }

        protected void tbxRepairStatusSearch_TextChanged(object sender, EventArgs e)
        {
            btnRepairStatusGo_Click(sender, e);
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
            upnlRepairStatuses?.Update();
        }

        protected void gvRepairStatuses_Sorting(object sender, GridViewSortEventArgs e)
        {
            ViewState["RepairStatusesSortExpression"] = e.SortExpression;
            gvRepairStatuses.EditIndex = -1; // Exit edit mode when sorting
            BindRepairStatusesGrid(forceRefresh: true);
            upnlRepairStatuses?.Update();
        }

        protected void gvRepairStatuses_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gvRepairStatuses.EditIndex = e.NewEditIndex;
            BindRepairStatusesGrid();
            upnlRepairStatuses?.Update();
        }

        protected void gvRepairStatuses_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvRepairStatuses.EditIndex = -1;
            // Force refresh when canceling to ensure clean state
            BindRepairStatusesGrid(forceRefresh: true);
            upnlRepairStatuses?.Update();
        }

        protected void gvRepairStatuses_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            try
            {
                int id = Convert.ToInt32(gvRepairStatuses.DataKeys[e.RowIndex].Value);
                GridViewRow row = gvRepairStatuses.Rows[e.RowIndex];

                var tbxStatusDesc = (TextBox)row.FindControl("tbxStatusDesc");
                var tbxStatusNote = (TextBox)row.FindControl("tbxStatusNote");
                var cbxEmailContact = (CheckBox)row.FindControl("cbxEmailContact");
                var tbxSortOrder = (TextBox)row.FindControl("tbxSortOrder");

                int sortOrder = 0;
                if (tbxSortOrder != null)
                    int.TryParse(tbxSortOrder.Text, out sortOrder);

                var status = new RepairStatus
                {
                    RepairStatusID = id,
                    RepairStatusDesc = tbxStatusDesc != null ? tbxStatusDesc.Text : string.Empty,
                    EmailContact = cbxEmailContact != null && cbxEmailContact.Checked,
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
                SetLookupStatus("Repair status updated.", false);
                upnlRepairStatuses?.Update();
            }
            catch (Exception ex)
            {
                SetLookupStatus("Update failed: " + ex.Message, true);
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
                SetLookupStatus("Repair status deleted.", false);
                upnlRepairStatuses?.Update();
            }
            catch (Exception ex)
            {
                SetLookupStatus("Delete failed: " + ex.Message, true);
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
                var cbxEmailContact = (CheckBox)gvRepairStatuses.FooterRow.FindControl("cbxEmailContactFooter");
                var tbxSortOrder = (TextBox)gvRepairStatuses.FooterRow.FindControl("tbxSortOrderFooter");

                int sortOrder = 0;
                if (tbxSortOrder != null)
                    int.TryParse(tbxSortOrder.Text, out sortOrder);

                var status = new RepairStatus
                {
                    RepairStatusDesc = tbxStatusDesc != null ? tbxStatusDesc.Text : string.Empty,
                    StatusNote = tbxStatusNote != null ? tbxStatusNote.Text : string.Empty,
                    EmailContact = cbxEmailContact != null && cbxEmailContact.Checked,
                    SortOrder = sortOrder
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
                SetLookupStatus("Repair status added.", false);
                upnlRepairStatuses?.Update();
            }
            catch (Exception ex)
            {
                SetLookupStatus("Insert failed: " + ex.Message, true);
            }
        }

        protected void gvRepairStatuses_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow && e.Row.RowType != DataControlRowType.Footer)
                return;

            // TabContainer + nested UpdatePanel: force full postback for row actions so
            // edit/search/update reliably refresh (same approach as Packaging).
            var sm = ScriptManager.GetCurrent(Page);
            if (sm == null)
                return;

            foreach (string id in new[] { "btnUpdate", "btnCancel", "btnEdit", "btnDelete", "btnAdd" })
            {
                Control btn = e.Row.FindControl(id);
                if (btn != null)
                    sm.RegisterPostBackControl(btn);
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
                DropDownList ddlSortOrder = (DropDownList)this.gvItems.FooterRow.FindControl("ddlSortOrder");
                TextBox control10 = (TextBox)this.gvItems.FooterRow.FindControl("tbxUnitsPerQty");
                DropDownList control11 = (DropDownList)this.gvItems.FooterRow.FindControl("ddlUnits");
                
                // Use Repository Pattern instead of SqlDataSource
                int sortOrder = 1;
                if (ddlSortOrder != null)
                    int.TryParse(ddlSortOrder.SelectedValue, out sortOrder);
                var newItem = new Item
                {
                    ItemDesc = control1.Text,
                    SKU = control2.Text,
                    ItemEnabled = control3.Checked,
                    ItemsCharacteritics = control4.Text,
                    ItemDetail = control5.Text,
                    ItemServiceTypeID = OptionalFkId(control6?.SelectedValue),
                    ReplacementItemID = OptionalFkId(control7?.SelectedValue),
                    ItemShortName = control8.Text,
                    SortOrder = sortOrder,
                    UnitsPerQty = Convert.ToDouble(control10.Text),
                    ItemUnitID = OptionalFkId(control11?.SelectedValue)
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
            upnlItems?.Update();
        }

        /// <summary>App-standard pager (Previous / squares / Next) — see Classes/GridPager.cs.</summary>
        protected void gvItems_RowCreated(object sender, GridViewRowEventArgs e)
        {
            GridPager.BuildPager(gvItems, e.Row);
        }

        protected void gvPeople_RowCreated(object sender, GridViewRowEventArgs e)
        {
            GridPager.BuildPager(gvPeople, e.Row);
        }

        protected void gvEquipment_RowCreated(object sender, GridViewRowEventArgs e)
        {
            GridPager.BuildPager(gvEquipment, e.Row);
        }

        protected void gvAreas_RowCreated(object sender, GridViewRowEventArgs e)
        {
            GridPager.BuildPager(gvAreas, e.Row);
        }

        protected void gvPackaging_RowCreated(object sender, GridViewRowEventArgs e)
        {
            GridPager.BuildPager(gvPackaging, e.Row);
        }

        protected void gvRepairStatuses_RowCreated(object sender, GridViewRowEventArgs e)
        {
            GridPager.BuildPager(gvRepairStatuses, e.Row);
        }

        private void BindSortOrdersGrid()
        {
            try
            {
                var list = GetSortOrdersRepo().GetAll("SortValue") ?? new List<ItemSortOrder>();
                gvSortOrders.DataSource = list;
                gvSortOrders.DataBind();
            }
            catch (Exception ex)
            {
                SetLookupStatus("Error loading sort orders: " + ex.Message, true);
            }
        }

        protected void gvSortOrders_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gvSortOrders.EditIndex = e.NewEditIndex;
            BindSortOrdersGrid();
            upnlSortOrders?.Update();
        }

        protected void gvSortOrders_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvSortOrders.EditIndex = -1;
            BindSortOrdersGrid();
            upnlSortOrders?.Update();
        }

        protected void gvSortOrders_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            try
            {
                int id = Convert.ToInt32(gvSortOrders.DataKeys[e.RowIndex].Value);
                GridViewRow row = gvSortOrders.Rows[e.RowIndex];
                var tbxVal = (TextBox)row.FindControl("tbxSortValue");
                var tbxDesc = (TextBox)row.FindControl("tbxSortDesc");
                var cbx = (CheckBox)row.FindControl("cbxSortEnabled");
                int sortValue;
                int.TryParse(tbxVal != null ? tbxVal.Text : "0", out sortValue);
                GetSortOrdersRepo().Update(new ItemSortOrder
                {
                    SortOrderID = id,
                    SortValue = sortValue,
                    SortOrderDesc = tbxDesc != null ? tbxDesc.Text.Trim() : string.Empty,
                    IsEnabled = cbx != null && cbx.Checked
                });
                gvSortOrders.EditIndex = -1;
                BindSortOrdersGrid();
                upnlSortOrders?.Update();
                SetLookupStatus("Sort order saved.", false);
            }
            catch (Exception ex)
            {
                SetLookupStatus("Error updating sort order: " + ex.Message, true);
            }
        }

        protected void gvSortOrders_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            try
            {
                int id = Convert.ToInt32(gvSortOrders.DataKeys[e.RowIndex].Value);
                GetSortOrdersRepo().Delete(id);
                gvSortOrders.EditIndex = -1;
                BindSortOrdersGrid();
                upnlSortOrders?.Update();
                SetLookupStatus("Sort order deleted.", false);
            }
            catch (Exception ex)
            {
                SetLookupStatus("Error deleting sort order: " + ex.Message, true);
            }
        }

        protected void gvSortOrders_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow && e.Row.RowType != DataControlRowType.Footer)
                return;
            var sm = ScriptManager.GetCurrent(Page);
            if (sm == null)
                return;
            foreach (string id in new[] { "btnSoUpdate", "btnSoCancel", "btnSoEdit", "btnSoDelete", "btnSoAdd" })
            {
                Control btn = e.Row.FindControl(id);
                if (btn != null)
                    sm.RegisterPostBackControl(btn);
            }
        }

        protected void gvSortOrders_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (!string.Equals(e.CommandName, "AddItem", StringComparison.Ordinal))
                return;
            try
            {
                if (gvSortOrders.FooterRow == null)
                    return;
                var tbxVal = (TextBox)gvSortOrders.FooterRow.FindControl("tbxSortValueFooter");
                var tbxDesc = (TextBox)gvSortOrders.FooterRow.FindControl("tbxSortDescFooter");
                var cbx = (CheckBox)gvSortOrders.FooterRow.FindControl("cbxSortEnabledFooter");
                int sortValue;
                if (tbxVal == null || !int.TryParse(tbxVal.Text, out sortValue))
                {
                    SetLookupStatus("Enter a numeric sort value.", true);
                    return;
                }
                GetSortOrdersRepo().Insert(new ItemSortOrder
                {
                    SortValue = sortValue,
                    SortOrderDesc = tbxDesc != null ? tbxDesc.Text.Trim() : string.Empty,
                    IsEnabled = cbx == null || cbx.Checked
                });
                gvSortOrders.EditIndex = -1;
                BindSortOrdersGrid();
                upnlSortOrders?.Update();
                SetLookupStatus("Sort order added.", false);
            }
            catch (Exception ex)
            {
                SetLookupStatus("Error adding sort order: " + ex.Message, true);
            }
        }

        // Items Grid - Sorting Event Handler
        protected void gvItems_Sorting(object sender, GridViewSortEventArgs e)
        {
            // Store sort expression in ViewState
            string expr = e.SortExpression;
            if (string.Equals(expr, "SortOrder", StringComparison.OrdinalIgnoreCase))
                expr = DefaultItemsSort;
            ViewState["ItemsSortExpression"] = expr;
            gvItems.EditIndex = -1; // Exit edit mode when sorting
            BindItemsGrid(forceRefresh: true);
            upnlItems?.Update();
        }

        protected void gvItems_RowEditing(object sender, GridViewEditEventArgs e)
        {
            // Enter edit mode against the exact filtered list that rendered the
            // clicked row. Rebinding from the repository first changes row indexes.
            gvItems.EditIndex = e.NewEditIndex;
            BindItemsGrid();
            upnlItems?.Update();
        }

        protected void gvItems_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvItems.EditIndex = -1;
            // Force refresh when canceling to ensure clean state
            BindItemsGrid(forceRefresh: true);
            upnlItems?.Update();
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
                var ddlSortOrder = (DropDownList)row.FindControl("ddlSortOrder");
                var tbxUnitsPerQty = (TextBox)row.FindControl("tbxUnitsPerQtyr");
                var ddlUnits = (DropDownList)row.FindControl("ddlUnits");

                int sortOrder = 1;
                if (ddlSortOrder != null)
                    int.TryParse(ddlSortOrder.SelectedValue, out sortOrder);

                var item = new Item
                {
                    ItemID = itemId,
                    ItemDesc = tbxItem?.Text ?? "",
                    SKU = tbxSKU?.Text ?? "",
                    ItemEnabled = cbxItemEnabled != null && cbxItemEnabled.Checked,
                    ItemsCharacteritics = tbxItemCharacteristics?.Text ?? "",
                    ItemDetail = tbxItemDetail?.Text ?? "",
                    ItemServiceTypeID = OptionalFkId(ddlServiceType?.SelectedValue),
                    ReplacementItemID = OptionalFkId(ddlReplacement?.SelectedValue),
                    ItemShortName = tbxItemShortName?.Text ?? "",
                    SortOrder = sortOrder,
                    UnitsPerQty = tbxUnitsPerQty != null ? Convert.ToDouble(tbxUnitsPerQty.Text) : 1.0,
                    ItemUnitID = OptionalFkId(ddlUnits?.SelectedValue)
                };

                var repo = new ItemsRepository();
                repo.Update(item);

                gvItems.EditIndex = -1;
                // Force refresh after update to get latest data from database
                BindItemsGrid(forceRefresh: true);
                upnlItems?.Update();
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
                var repo = new ItemsRepository();
                string sortBy = ViewState["ItemsSortExpression"] as string ?? DefaultItemsSort;
                List<Item> items = repo.GetAll(null);
                items = ApplyItemsSort(items, sortBy);

                // The textbox is part of ViewState and is the authoritative filter for
                // this page instance. Session-cached lists can be missing or stale after
                // an app recycle, which previously rebound edit mode to the full list.
                string searchFilter = tbxItemSearch != null
                    ? tbxItemSearch.Text.Trim()
                    : string.Empty;

                if (!string.IsNullOrEmpty(searchFilter))
                {
                    string normalizedFilter = searchFilter.Replace("%", "");
                    items = items.Where(i => i.ItemDesc != null
                        && i.ItemDesc.IndexOf(normalizedFilter,
                            StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                }

                // Reset dropdown caches so each bind gets fresh lookup lists
                _itemUnitsCache = null;
                _itemServiceTypesCache = null;
                _replacementItemsCache = null;
                
                gvItems.DataSource = items;
                gvItems.DataBind();
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Error loading items: " + ex.Message;
            }
        }

        /// <summary>
        /// S/O is an int (1 Coffee … 15 Groups). Sorting in memory so 15 cannot
        /// appear before 1 the way it does with name order or a string sort.
        /// </summary>
        private static List<Item> ApplyItemsSort(List<Item> items, string sortBy)
        {
            if (items == null || items.Count == 0)
                return items ?? new List<Item>();

            string key = (sortBy ?? string.Empty).Trim();
            bool sortOrderFirst = string.IsNullOrEmpty(key)
                || key.StartsWith("SortOrder", StringComparison.OrdinalIgnoreCase);

            if (sortOrderFirst)
            {
                return items
                    .OrderBy(i => i.SortOrder ?? int.MaxValue)
                    .ThenBy(i => i.ItemDesc ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }

            if (key.StartsWith("ItemDesc", StringComparison.OrdinalIgnoreCase))
            {
                return items
                    .OrderBy(i => i.ItemDesc ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                    .ThenBy(i => i.SortOrder ?? int.MaxValue)
                    .ToList();
            }

            if (key.StartsWith("SKU", StringComparison.OrdinalIgnoreCase))
                return items.OrderBy(i => i.SKU ?? string.Empty, StringComparer.OrdinalIgnoreCase).ToList();
            if (key.StartsWith("ItemShortName", StringComparison.OrdinalIgnoreCase))
                return items.OrderBy(i => i.ItemShortName ?? string.Empty, StringComparer.OrdinalIgnoreCase).ToList();
            if (key.StartsWith("ItemEnabled", StringComparison.OrdinalIgnoreCase))
                return items.OrderByDescending(i => i.ItemEnabled ?? false).ThenBy(i => i.SortOrder ?? int.MaxValue).ToList();
            if (key.StartsWith("UnitsPerQty", StringComparison.OrdinalIgnoreCase))
                return items.OrderBy(i => i.UnitsPerQty ?? 0).ToList();
            if (key.StartsWith("ItemUnitID", StringComparison.OrdinalIgnoreCase))
                return items.OrderBy(i => i.ItemUnitID ?? 0).ToList();
            if (key.StartsWith("ItemServiceTypeID", StringComparison.OrdinalIgnoreCase))
                return items.OrderBy(i => i.ItemServiceTypeID ?? 0).ToList();
            if (key.StartsWith("ReplacementItemID", StringComparison.OrdinalIgnoreCase))
                return items.OrderBy(i => i.ReplacementItemID ?? 0).ToList();
            if (key.StartsWith("ItemsCharacteritics", StringComparison.OrdinalIgnoreCase))
                return items.OrderBy(i => i.ItemsCharacteritics ?? string.Empty, StringComparer.OrdinalIgnoreCase).ToList();
            if (key.StartsWith("ItemDetail", StringComparison.OrdinalIgnoreCase))
                return items.OrderBy(i => i.ItemDetail ?? string.Empty, StringComparer.OrdinalIgnoreCase).ToList();

            return items
                .OrderBy(i => i.SortOrder ?? int.MaxValue)
                .ThenBy(i => i.ItemDesc ?? string.Empty, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        protected void gvItems_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow &&
                e.Row.RowType != DataControlRowType.Footer)
                return;

            // TabContainer + nested UpdatePanel: full postback for row actions so
            // edit after search refreshes reliably (same approach as Repair Statuses).
            var sm = ScriptManager.GetCurrent(Page);
            if (sm != null)
            {
                foreach (string id in new[] { "btnUpdate", "btnCancel", "btnEdit", "btnAdd" })
                {
                    Control btn = e.Row.FindControl(id);
                    if (btn != null)
                        sm.RegisterPostBackControl(btn);
                }
            }

            var item = e.Row.DataItem as Item;
            BindItemsLookupDropdowns(e.Row, item);

            var sortDdl = e.Row.FindControl("ddlSortOrder") as DropDownList;
            if (sortDdl != null)
                GetSortOrdersRepo().FillDropDown(sortDdl, item != null ? item.SortOrder : 1);

            var sortLabel = e.Row.FindControl("lblSortOrder") as Label;
            if (sortLabel != null)
            {
                sortLabel.Text = item != null && item.SortOrder.HasValue
                    ? item.SortOrder.Value.ToString()
                    : string.Empty;
                sortLabel.ToolTip = GetSortOrdersRepo().Describe(item != null ? item.SortOrder : null);
            }

            var unitLabel = e.Row.FindControl("lblItemUnit") as Label;
            if (unitLabel != null)
            {
                ItemUnit unit = item?.ItemUnitID > 0
                    ? GetItemUnitsCache().FirstOrDefault(candidate =>
                        candidate.ItemUnitID == item.ItemUnitID.Value)
                    : null;

                unitLabel.Text = string.IsNullOrWhiteSpace(unit?.UnitOfMeasure)
                    ? "n/a"
                    : unit.UnitOfMeasure;
            }
        }

        private void BindItemsLookupDropdowns(GridViewRow row, Item item)
        {
            BindDropDown(
                row.FindControl("ddlUnits") as DropDownList,
                GetItemUnitsCache(),
                "UnitOfMeasure",
                "ItemUnitID",
                item?.ItemUnitID);

            BindDropDown(
                row.FindControl("ddlServiceType") as DropDownList,
                GetItemServiceTypesCache(),
                "ItemServiceTypeName",
                "ItemServiceTypeID",
                item?.ItemServiceTypeID);

            BindDropDown(
                row.FindControl("ddlReplacement") as DropDownList,
                GetReplacementItemsCache(),
                "ItemDesc",
                "ItemTypeID",
                item?.ReplacementItemID);
        }

        private static void BindDropDown<T>(
            DropDownList ddl,
            IList<T> data,
            string textField,
            string valueField,
            int? selectedId)
        {
            if (ddl == null)
                return;

            ddl.Items.Clear();
            ddl.Items.Add(new ListItem("n/a", "0"));
            ddl.DataTextField = textField;
            ddl.DataValueField = valueField;
            ddl.DataSource = data;
            ddl.DataBind();

            string value = (selectedId.HasValue && selectedId.Value > 0)
                ? selectedId.Value.ToString()
                : "0";
            var match = ddl.Items.FindByValue(value);
            ddl.ClearSelection();
            if (match != null)
                match.Selected = true;
            else
                ddl.Items.FindByValue("0").Selected = true;
        }

        private List<ItemUnit> GetItemUnitsCache()
        {
            if (_itemUnitsCache == null)
                _itemUnitsCache = new ItemUnitsRepository().GetAll("UnitOfMeasure") ?? new List<ItemUnit>();
            return _itemUnitsCache;
        }

        private List<ItemServiceType> GetItemServiceTypesCache()
        {
            if (_itemServiceTypesCache == null)
                _itemServiceTypesCache = new ItemServiceTypesRepository().GetAll("ItemServiceTypeName")
                    ?? new List<ItemServiceType>();
            return _itemServiceTypesCache;
        }

        private List<OrderItemLookup> GetReplacementItemsCache()
        {
            if (_replacementItemsCache == null)
            {
                // Canonical item lookup order: enabled first by SortOrder/description,
                // then disabled items prefixed with "_".
                _replacementItemsCache = new ItemsRepository().GetOrderItemLookups(null)
                    ?? new List<OrderItemLookup>();
            }
            return _replacementItemsCache;
        }

        /// <summary>Maps dropdown "n/a" (0) / empty to null for optional FK columns.</summary>
        private static int? OptionalFkId(string selectedValue)
        {
            if (string.IsNullOrWhiteSpace(selectedValue))
                return null;
            int id = Convert.ToInt32(selectedValue);
            return id > 0 ? (int?)id : null;
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
                upnlItems?.Update();
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
                ViewState["ItemsSortExpression"] = DefaultItemsSort;
                gvItems.PageIndex = 0; // Reset to first page
                gvItems.EditIndex = -1; // Exit edit mode if active
                BindItemsGrid(forceRefresh: true); // Force refresh for reset
                upnlItems?.Update();
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

                // Filter is always re-read from the textbox so edit row indexes
                // stay in sync with what is on screen (same approach as Items).
                string searchFilter = tbxAreaSearch != null
                    ? tbxAreaSearch.Text.Trim()
                    : string.Empty;

                if (!string.IsNullOrEmpty(searchFilter))
                {
                    cities = cities.Where(a => a.AreaName != null
                        && a.AreaName.IndexOf(searchFilter, StringComparison.OrdinalIgnoreCase) >= 0)
                        .ToList();
                }

                gvAreas.DataSource = cities;
                gvAreas.DataBind();
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Error loading areas: " + ex.Message;
            }
        }

        protected void btnAreaGo_Click(object sender, EventArgs e)
        {
            gvAreas.EditIndex = -1;
            gvAreas.PageIndex = 0;
            BindAreasGrid();
        }

        protected void btnAreaReset_Click(object sender, EventArgs e)
        {
            if (tbxAreaSearch != null)
                tbxAreaSearch.Text = string.Empty;
            gvAreas.EditIndex = -1;
            gvAreas.PageIndex = 0;
            BindAreasGrid();
        }

        protected void tbxAreaSearch_TextChanged(object sender, EventArgs e)
        {
            btnAreaGo_Click(sender, e);
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

        protected void gvPeople_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvPeople.EditIndex = -1;
            BindPeopleGrid();
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
            if (e.Row.RowType != DataControlRowType.DataRow)
                return;

            // ColorPicker + UpdatePanel often fail to refresh after Update/Cancel.
            // Force a full postback for those buttons so edit mode exits reliably.
            var sm = ScriptManager.GetCurrent(Page);
            if (sm != null)
            {
                foreach (string id in new[] { "btnUpdate", "btnCancel", "btnAdd", "btnEdit" })
                {
                    Control btn = e.Row.FindControl(id);
                    if (btn != null)
                        sm.RegisterPostBackControl(btn);
                }
            }

            ItemPackaging dataItem = e.Row.DataItem as ItemPackaging;
            if (dataItem == null)
                return;

            bool isEdit = (e.Row.RowState & DataControlRowState.Edit) == DataControlRowState.Edit;
            if (isEdit)
                return; // Edit values come from Bind(...) — do not overwrite posted colour text.

            // Display mode: paint swatches from stored hex (keep-bg exempts hover repaint).
            ApplyHexSwatch(e.Row.Cells[3], dataItem.BGColour, e.Row.FindControl("LabelBGColour") as Label);
            ApplyHexSwatch(e.Row.Cells[4], dataItem.Colour, e.Row.FindControl("LabelColour") as Label);
        }

        private void ApplyHexSwatch(TableCell cell, string hexColour, Label label)
        {
            if (cell == null || string.IsNullOrWhiteSpace(hexColour))
                return;

            try
            {
                Color color = ColorTranslator.FromHtml(NormalizeHexColour(hexColour));
                if (label != null)
                    label.Text = NormalizeHexColour(hexColour);
                cell.BackColor = color;
                cell.CssClass = "keep-bg";
                int brightness = (int)(color.R * 0.299 + color.G * 0.587 + color.B * 0.114);
                cell.ForeColor = brightness > 128 ? Color.Black : Color.White;
            }
            catch (Exception ex)
            {
                this.lblStatus.Text = ex.Message;
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

                if (!TryNormalizeHexColour(controlBGColour?.Text, out string bgColour)
                    || !TryNormalizeHexColour(controlColour?.Text, out string colour))
                {
                    SetLookupStatus("Colours must be hex values like #FF0000 (or leave blank).", true);
                    return;
                }

                var packaging = new ItemPackaging
                {
                    ItemPackagingDesc = controlDescription.Text,
                    AdditionalNotes = controlAdditionalNotes.Text,
                    BGColour = bgColour,
                    Colour = string.IsNullOrEmpty(colour) ? null : colour,
                    Symbol = controlSymbol.Text
                };

                var repo = new ItemPackagingsRepository();
                repo.Insert(packaging);
                BindPackagingGrid();
                SetLookupStatus("Packaging added.", false);
            }
            catch (Exception ex)
            {
                SetLookupStatus("Error adding record: " + ex.Message, true);
            }
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
            if (e.CommandName.Equals("Edit") || e.CommandName.Equals("Update") || e.CommandName.Equals("Cancel"))
                return;

            if (e.CommandName.Equals("AddItem") || e.CommandName.Equals("Add") || e.CommandName.Equals("Insert"))
            {
                try
                {
                    GridViewRow namingContainer = (GridViewRow)((Control)e.CommandSource).NamingContainer;
                    if (namingContainer == null)
                        return;

                    TextBox controlTypeDesc = (TextBox)namingContainer.FindControl("InvoiceTypeDescTextBox");
                    if (controlTypeDesc == null || string.IsNullOrEmpty(controlTypeDesc.Text))
                    {
                        SetLookupStatus("Enter an invoice type description before adding.", true);
                        gvInvoiceTypesUpdatePanel?.Update();
                        return;
                    }

                    CheckBox controlEnabled = (CheckBox)namingContainer.FindControl("EnabledCheckBox");
                    TextBox controlNotes = (TextBox)namingContainer.FindControl("NotesTextBox");

                    var invoiceType = new InvoiceType
                    {
                        InvoiceTypeDesc = controlTypeDesc.Text,
                        Enabled = controlEnabled != null && controlEnabled.Checked,
                        Notes = controlNotes != null ? controlNotes.Text : string.Empty
                    };

                    new InvoiceTypesRepository().Insert(invoiceType);
                    gvInvoiceTypes.EditIndex = -1;
                    BindInvoiceTypesGrid();
                    SetLookupStatus("Invoice type added.", false);
                    gvInvoiceTypesUpdatePanel?.Update();
                }
                catch (Exception ex)
                {
                    SetLookupStatus("Error adding invoice type: " + ex.Message, true);
                    gvInvoiceTypesUpdatePanel?.Update();
                }
            }
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
