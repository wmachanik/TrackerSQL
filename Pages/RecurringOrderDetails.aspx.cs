using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerSQL.Classes;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Pages
{
    public partial class RecurringOrderDetails : Page
    {
        private static string previousPage = string.Empty;
        private readonly RecurringOrdersRepository recurringOrdersRepository = new RecurringOrdersRepository();

        protected Label lblReoccuringOrderID;
        protected ScriptManager smReoccuringOrderDetails;
        protected UpdateProgress uprgReoccuringOrderDetails;
        protected UpdatePanel upnlReoccuringOrderDetails;
        protected DropDownList ddlCompanyName;
        protected Label ReoccuringOrderIDLabel;
        protected DropDownList ddlDeliveryBy;
        protected CheckBox EnabledCheckBox;
        protected TextBox NotesTextBox;
        protected GridView gvRecurringOrderItems;
        protected Button btnAddLine;
        protected Button btnUpdate;
        protected Button btnUpdateAndReturn;
        protected Button btnInsert;
        protected Button btnDelete;
        protected Button btnRevert;
        protected Button btnReturn;
        protected Literal ltrlStatus;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (IsPostBack)
            {
                return;
            }

            BindDropDownLists();

            previousPage = Request.UrlReferrer != null ? Request.UrlReferrer.ToString() : string.Empty;
            if (Request.QueryString["ID"] != null)
            {
                LoadRecurringOrder(Convert.ToInt32(Request.QueryString["ID"]));
            }
            else
            {
                btnUpdate.Enabled = false;
                btnUpdateAndReturn.Enabled = false;
                btnInsert.Enabled = true;
                btnDelete.Enabled = false;
                EnabledCheckBox.Checked = true;
                BindRecurringItemsGrid(new List<RecurringOrderItem> 
                { 
                    new RecurringOrderItem 
                    {
                        NextDateRequired = TimeZoneUtils.Now().Date
                    } 
                });
            }
        }

        private void BindDropDownLists()
        {
            BindCompanies();
            BindDeliveryBy();
        }

        private void BindCompanies()
        {
            ddlCompanyName.DataSource = new ContactsRepository().GetAllCompanyNames();
            ddlCompanyName.DataTextField = nameof(ContactLookup.CompanyName);
            ddlCompanyName.DataValueField = nameof(ContactLookup.ContactID);
            ddlCompanyName.DataBind();
        }

        private void BindDeliveryBy()
        {
            ddlDeliveryBy.DataSource = recurringOrdersRepository.GetDeliveryPeople();
            ddlDeliveryBy.DataTextField = nameof(DeliveryByLookup.DisplayName);
            ddlDeliveryBy.DataValueField = nameof(DeliveryByLookup.PersonID);
            ddlDeliveryBy.DataBind();
        }

        private void LoadRecurringOrder(int recurringOrderId)
        {
            var recurringOrder = recurringOrdersRepository.GetById(recurringOrderId);
            if (recurringOrder == null)
            {
                return;
            }

            StoreOriginalDataInViewState(recurringOrder);

            ReoccuringOrderIDLabel.Text = recurringOrder.RecurringOrderID.ToString();
            SetSelectedValueIfPresent(ddlCompanyName, recurringOrder.ContactID);
            SetSelectedValueIfPresent(ddlDeliveryBy, recurringOrder.DeliveryByID);
            EnabledCheckBox.Checked = recurringOrder.Enabled ?? false;
            NotesTextBox.Text = recurringOrder.Notes;
            BindRecurringItemsGrid(recurringOrder.Items);
        }

        private RecurringOrder GetDataFromForm()
        {
            var dataFromForm = new RecurringOrder();
            if (!string.IsNullOrEmpty(ReoccuringOrderIDLabel.Text))
            {
                dataFromForm.RecurringOrderID = Convert.ToInt32(ReoccuringOrderIDLabel.Text);
            }

            dataFromForm.ContactID = GetNullableInt(ddlCompanyName.SelectedValue);
            dataFromForm.DeliveryByID = GetNullableInt(ddlDeliveryBy.SelectedValue);
            dataFromForm.Enabled = EnabledCheckBox.Checked;
            dataFromForm.Notes = string.IsNullOrWhiteSpace(NotesTextBox.Text) ? null : NotesTextBox.Text.Trim();
            dataFromForm.Items = GetRecurringItemsFromGrid(false);
            return dataFromForm;
        }

        private void UpdateRecord()
        {
            var recurringOrder = GetDataFromForm();
            int calculatedCount = recurringOrdersRepository.AutoCalculateNextDatesForItems(recurringOrder);
            recurringOrdersRepository.Update(recurringOrder);
            LoadRecurringOrder(recurringOrder.RecurringOrderID);

            if (calculatedCount > 0)
            {
                ltrlStatus.Text = string.Format("Recurring order updated. {0} next date(s) auto-calculated.", calculatedCount);
            }
            else
            {
                ltrlStatus.Text = "Recurring order updated";
            }
            
            var showMessageBox = new showMessageBox(Page, "Recurring Order Update", ltrlStatus.Text);
        }

        private void StoreOriginalDataInViewState(RecurringOrder recurringOrder)
        {
            ViewState["OriginalRecurringOrder"] = new RecurringOrder
            {
                RecurringOrderID = recurringOrder.RecurringOrderID,
                ContactID = recurringOrder.ContactID,
                DeliveryByID = recurringOrder.DeliveryByID,
                Enabled = recurringOrder.Enabled,
                Notes = recurringOrder.Notes,
                Items = recurringOrder.Items == null ? new List<RecurringOrderItem>() : recurringOrder.Items.Select(item => new RecurringOrderItem
                {
                    RecurringOrderItemID = item.RecurringOrderItemID,
                    RecurringOrderID = item.RecurringOrderID,
                    RecurringTypeID = item.RecurringTypeID,
                    Value = item.Value,
                    ItemRequiredID = item.ItemRequiredID,
                    QtyRequired = item.QtyRequired,
                    DateLastDone = item.DateLastDone,
                    NextDateRequired = item.NextDateRequired,
                    RequireUntilDate = item.RequireUntilDate,
                    ItemPackagingID = item.ItemPackagingID
                }).ToList()
            };
        }

        private void RestoreOriginalDataFromViewState()
        {
            var originalOrder = ViewState["OriginalRecurringOrder"] as RecurringOrder;
            if (originalOrder == null)
            {
                return;
            }

            ReoccuringOrderIDLabel.Text = originalOrder.RecurringOrderID.ToString();
            SetSelectedValueIfPresent(ddlCompanyName, originalOrder.ContactID);
            SetSelectedValueIfPresent(ddlDeliveryBy, originalOrder.DeliveryByID);
            EnabledCheckBox.Checked = originalOrder.Enabled ?? false;
            NotesTextBox.Text = originalOrder.Notes;
            BindRecurringItemsGrid(originalOrder.Items);
        }

        private void ReturnToPrevPage()
        {
            ReturnToPrevPage(false);
        }

        private void ReturnToPrevPage(bool goToRecurringOrders)
        {
            if (goToRecurringOrders || string.IsNullOrWhiteSpace(previousPage))
            {
                Response.Redirect("~/Pages/RecurringOrders.aspx");
            }
            else
            {
                Response.Redirect(previousPage);
            }
        }

        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            UpdateRecord();
        }

        protected void btnUpdateAndReturn_Click(object sender, EventArgs e)
        {
            UpdateRecord();
            ReturnToPrevPage();
        }

        protected void btnRevert_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(ReoccuringOrderIDLabel.Text))
            {
                RestoreOriginalDataFromViewState();
                ltrlStatus.Text = "Changes reverted to last saved state";
            }
            else
            {
                BindRecurringItemsGrid(new List<RecurringOrderItem> 
                { 
                    new RecurringOrderItem 
                    {
                        NextDateRequired = TimeZoneUtils.Now().Date
                    } 
                });
                ddlCompanyName.SelectedIndex = 0;
                ddlDeliveryBy.SelectedIndex = 0;
                EnabledCheckBox.Checked = true;
                NotesTextBox.Text = string.Empty;
                ltrlStatus.Text = "Form cleared";
            }
        }

        protected void btnReturn_Click(object sender, EventArgs e)
        {
            ReturnToPrevPage();
        }

        protected void btnInsert_Click(object sender, EventArgs e)
        {
            var recurringOrder = GetDataFromForm();
            int calculatedCount = recurringOrdersRepository.AutoCalculateNextDatesForItems(recurringOrder);
            recurringOrder.RecurringOrderID = recurringOrdersRepository.Insert(recurringOrder);
            ltrlStatus.Text = calculatedCount > 0
                ? string.Format("Recurring order inserted. {0} next date(s) auto-calculated.", calculatedCount)
                : "Recurring order inserted";
            var showMessageBox = new showMessageBox(Page, "Recurring Order Insert", ltrlStatus.Text);
            ReturnToPrevPage(true);
        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            recurringOrdersRepository.Delete(Convert.ToInt32(ReoccuringOrderIDLabel.Text));
            ltrlStatus.Text = "Recurring order deleted";
            var showMessageBox = new showMessageBox(Page, "Recurring Order Deleted", ltrlStatus.Text);
            ReturnToPrevPage(true);
        }

        protected void btnAddLine_Click(object sender, EventArgs e)
        {
            var items = GetRecurringItemsFromGrid(true);
            items.Add(new RecurringOrderItem
            {
                NextDateRequired = TimeZoneUtils.Now().Date
            });
            BindRecurringItemsGrid(items);
        }

        protected void gvRecurringOrderItems_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (!string.Equals(e.CommandName, "DeleteLine", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            int rowIndex;
            if (!int.TryParse(Convert.ToString(e.CommandArgument), out rowIndex))
            {
                return;
            }

            var items = GetRecurringItemsFromGrid(true);
            if (rowIndex >= 0 && rowIndex < items.Count)
            {
                items.RemoveAt(rowIndex);
            }

            if (items.Count == 0)
            {
                items.Add(new RecurringOrderItem
                {
                    NextDateRequired = TimeZoneUtils.Now().Date
                });
            }

            BindRecurringItemsGrid(items);
        }

        protected void gvRecurringOrderItems_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow)
            {
                return;
            }

            var recurringOrderItem = e.Row.DataItem as RecurringOrderItem;
            if (recurringOrderItem == null)
            {
                return;
            }

            var ddlItemType = e.Row.FindControl("ddlItemType") as DropDownList;
            if (ddlItemType != null)
            {
                var itemsRepository = new ItemsRepository();
                ddlItemType.DataSource = itemsRepository.GetAll()
                    .OrderBy(item => item.ItemEnabled == false ? 1 : 0)
                    .ThenBy(item => item.SortOrder ?? int.MaxValue)
                    .ThenBy(item => item.ItemDesc ?? string.Empty)
                    .ToList();
                ddlItemType.DataTextField = nameof(Item.FormattedDisplayText);
                ddlItemType.DataValueField = nameof(Item.ItemID);
                ddlItemType.DataBind();
                EnsureSelectedItemPresent(ddlItemType, recurringOrderItem.ItemRequiredID, itemsRepository);
            }

            var ddlPackagingTypes = e.Row.FindControl("ddlPackagingTypes") as DropDownList;
            if (ddlPackagingTypes != null)
            {
                ddlPackagingTypes.DataSource = new ItemPackagingsRepository().GetAll("ItemPackagingDesc");
                ddlPackagingTypes.DataTextField = nameof(ItemPackaging.ItemPackagingDesc);
                ddlPackagingTypes.DataValueField = nameof(ItemPackaging.ItemPackagingID);
                ddlPackagingTypes.DataBind();
                SetSelectedValueIfPresent(ddlPackagingTypes, recurringOrderItem.ItemPackagingID);
            }

            var ddlReoccuranceType = e.Row.FindControl("ddlReoccuranceType") as DropDownList;
            if (ddlReoccuranceType != null)
            {
                ddlReoccuranceType.DataSource = recurringOrdersRepository.GetRecurringTypes();
                ddlReoccuranceType.DataTextField = nameof(RecurringTypeLookup.RecurringTypeDesc);
                ddlReoccuranceType.DataValueField = nameof(RecurringTypeLookup.RecurringTypeID);
                ddlReoccuranceType.DataBind();
                SetSelectedValueIfPresent(ddlReoccuranceType, recurringOrderItem.RecurringTypeID);
            }
        }

        private void SetSelectedValueIfPresent(ListControl control, int? value)
        {
            if (!value.HasValue)
            {
                return;
            }

            var listItem = control.Items.FindByValue(value.Value.ToString());
            if (listItem != null)
            {
                control.SelectedValue = listItem.Value;
            }
        }

        private void EnsureSelectedItemPresent(ListControl control, int? itemId, ItemsRepository itemsRepository)
        {
            if (!itemId.HasValue)
            {
                return;
            }

            var existingItem = control.Items.FindByValue(itemId.Value.ToString());
            if (existingItem == null)
            {
                var item = itemsRepository.GetById(itemId.Value);
                if (item != null)
                {
                    string itemText = string.IsNullOrWhiteSpace(item.ItemDesc)
                        ? "(item " + item.ItemID + ")"
                        : LookupFormatter.FormatLookupText(item.ItemDesc, item.ItemEnabled);

                    control.Items.Add(new ListItem(itemText, item.ItemID.ToString()));
                }
            }

            SetSelectedValueIfPresent(control, itemId);
        }

        private void BindRecurringItemsGrid(List<RecurringOrderItem> items)
        {
            gvRecurringOrderItems.DataSource = items != null && items.Count > 0
                ? items
                : new List<RecurringOrderItem> { new RecurringOrderItem() };
            gvRecurringOrderItems.DataBind();
        }

        private List<RecurringOrderItem> GetRecurringItemsFromGrid(bool includeEmptyRows)
        {
            var items = new List<RecurringOrderItem>();

            foreach (GridViewRow row in gvRecurringOrderItems.Rows)
            {
                var item = new RecurringOrderItem();

                var hfRecurringOrderItemID = row.FindControl("hfRecurringOrderItemID") as HiddenField;
                if (hfRecurringOrderItemID != null)
                {
                    item.RecurringOrderItemID = GetNullableInt(hfRecurringOrderItemID.Value) ?? 0;
                }

                var ddlItemType = row.FindControl("ddlItemType") as DropDownList;
                var tbxQuantity = row.FindControl("tbxQuantity") as TextBox;
                var ddlPackagingTypes = row.FindControl("ddlPackagingTypes") as DropDownList;
                var tbxValue = row.FindControl("tbxValue") as TextBox;
                var ddlReoccuranceType = row.FindControl("ddlReoccuranceType") as DropDownList;
                var tbxLastDate = row.FindControl("tbxLastDate") as TextBox;
                var tbxUntilDate = row.FindControl("tbxUntilDate") as TextBox;

                item.ItemRequiredID = ddlItemType == null ? null : GetNullableInt(ddlItemType.SelectedValue);
                item.QtyRequired = tbxQuantity == null ? null : GetNullableDouble(tbxQuantity.Text);
                item.ItemPackagingID = ddlPackagingTypes == null ? null : GetNullableInt(ddlPackagingTypes.SelectedValue);
                item.Value = tbxValue == null ? null : GetNullableInt(tbxValue.Text);
                item.RecurringTypeID = ddlReoccuranceType == null ? null : GetNullableInt(ddlReoccuranceType.SelectedValue);
                item.DateLastDone = tbxLastDate == null ? null : ParseOptionalUserDate(tbxLastDate.Text);
                // NextDateRequired will be auto-calculated by AutoCalculateNextDatesForItems - don't read from UI
                item.NextDateRequired = null;
                item.RequireUntilDate = tbxUntilDate == null ? null : ParseOptionalUserDate(tbxUntilDate.Text);

                if (includeEmptyRows || !IsEmptyRecurringItem(item))
                {
                    items.Add(item);
                }
            }

            return items;
        }

        private static bool IsEmptyRecurringItem(RecurringOrderItem recurringOrderItem)
        {
            return !recurringOrderItem.ItemRequiredID.HasValue
                && !recurringOrderItem.QtyRequired.HasValue
                && !recurringOrderItem.ItemPackagingID.HasValue
                && !recurringOrderItem.Value.HasValue
                && !recurringOrderItem.RecurringTypeID.HasValue
                && !recurringOrderItem.DateLastDone.HasValue
                && !recurringOrderItem.NextDateRequired.HasValue
                && !recurringOrderItem.RequireUntilDate.HasValue;
        }

        private int? GetNullableInt(string value)
        {
            if (int.TryParse(value, out var parsedValue) && parsedValue > 0)
            {
                return parsedValue;
            }

            return null;
        }

        private double? GetNullableDouble(string value)
        {
            if (double.TryParse(value, out var parsedValue))
            {
                return parsedValue;
            }

            return null;
        }

        private DateTime? ParseOptionalUserDate(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var parsedValue = TrackerTools.ParseUserDate(value);
            return parsedValue <= SystemConstants.DatabaseConstants.SystemMinDate ? (DateTime?)null : parsedValue;
        }
    }
}
