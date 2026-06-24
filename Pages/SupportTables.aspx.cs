using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerSQL.Classes;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Pages
{
    public partial class SupportTables : Page
    {
        private const string CONST_SORTEXPRESSION_VIEWSTATE = "SupportTableSortExpression";
        private const string CONST_SELECTEDTABLE_VIEWSTATE = "SupportTableSelected";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                ViewState[CONST_SORTEXPRESSION_VIEWSTATE] = null;
                ViewState[CONST_SELECTEDTABLE_VIEWSTATE] = "";
            }
        }

        protected void ddlTables_SelectedIndexChanged(object sender, EventArgs e)
        {
            string newTable = ddlTables.SelectedValue;
            string previousTable = ViewState[CONST_SELECTEDTABLE_VIEWSTATE] as string ?? "";

            // Reset sort, page, and edit index when table changes
            if (newTable != previousTable)
            {
                ViewState[CONST_SORTEXPRESSION_VIEWSTATE] = null;
                gvSupporTable.PageIndex = 0;
                gvSupporTable.EditIndex = -1;
            }

            ViewState[CONST_SELECTEDTABLE_VIEWSTATE] = newTable;
            lblStatus.Text = "";
            SetupColumnsForTable(newTable);
            BindSelectedTable();
        }

        protected void gvSupporTable_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvSupporTable.EditIndex = -1;
            gvSupporTable.PageIndex = e.NewPageIndex;
            BindSelectedTable();
        }

        protected void gvSupporTable_Sorting(object sender, GridViewSortEventArgs e)
        {
            gvSupporTable.EditIndex = -1;
            ViewState[CONST_SORTEXPRESSION_VIEWSTATE] = e.SortExpression;
            BindSelectedTable();
        }

        protected void gvSupporTable_RowEditing(object sender, GridViewEditEventArgs e)
        {
            gvSupporTable.EditIndex = e.NewEditIndex;
            BindSelectedTable();
        }

        protected void gvSupporTable_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
        {
            gvSupporTable.EditIndex = -1;
            BindSelectedTable();
        }

        protected void gvSupporTable_RowUpdating(object sender, GridViewUpdateEventArgs e)
        {
            string selectedTable = ViewState[CONST_SELECTEDTABLE_VIEWSTATE] as string ?? "";

            try
            {
                bool updated = false;
                switch (selectedTable)
                {
                    case "Areas": updated = UpdateArea(e); break;
                    case "AreaPrepDays": updated = UpdateAreaPrepDays(e); break;
                    case "EquipmentTypes": updated = UpdateEquipmentType(e); break;
                    case "InvoiceTypes": updated = UpdateInvoiceType(e); break;
                    case "ItemPackaging": updated = UpdateItemPackaging(e); break;
                    case "Items": updated = UpdateItem(e); break;
                    case "PaymentTerms": updated = UpdatePaymentTerm(e); break;
                    case "People": updated = UpdatePerson(e); break;
                    case "PriceLevels": updated = UpdatePriceLevel(e); break;
                    case "RepairStatuses": updated = UpdateRepairStatus(e); break;
                }

                if (updated)
                    lblStatus.Text = "<span style='color:green'>Record updated successfully.</span>";
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"<span style='color:red'>Error updating: {ex.Message}</span>";
                AppLogger.WriteLog(SystemConstants.LogTypes.System, $"SupportTables Update error: {ex.Message}");
            }

            gvSupporTable.EditIndex = -1;
            BindSelectedTable();
        }

        private void SetupColumnsForTable(string tableName)
        {
            gvSupporTable.Columns.Clear();
            gvSupporTable.DataKeyNames = null;

            // Add command column with image buttons first
            AddCommandColumn();

            switch (tableName)
            {
                case "Areas":
                    gvSupporTable.DataKeyNames = new[] { "ID" };
                    AddBoundField("ID", "ID", true);
                    AddBoundField("AreaName", "Area Name", false);
                    break;

                case "AreaPrepDays":
                    gvSupporTable.DataKeyNames = new[] { "AreaPrepDaysID" };
                    AddBoundField("AreaPrepDaysID", "ID", true);
                    AddBoundField("AreaID", "Area ID", false);
                    AddBoundField("PrepDayOfWeekID", "Prep Day", false);
                    AddBoundField("DeliveryDelayDays", "Delay Days", false);
                    AddBoundField("DeliveryOrder", "Order", false);
                    break;

                case "EquipmentTypes":
                    gvSupporTable.DataKeyNames = new[] { "EquipTypeID" };
                    AddBoundField("EquipTypeID", "ID", true);
                    AddBoundField("EquipTypeName", "Name", false);
                    AddBoundField("EquipTypeDescription", "Description", false);
                    break;

                case "InvoiceTypes":
                    gvSupporTable.DataKeyNames = new[] { "InvoiceTypeID" };
                    AddBoundField("InvoiceTypeID", "ID", true);
                    AddBoundField("InvoiceTypeDesc", "Type", false);
                    AddCheckBoxField("Enabled", "Enabled");
                    AddBoundField("Notes", "Notes", false);
                    break;

                case "ItemPackaging":
                    gvSupporTable.DataKeyNames = new[] { "ItemPackagingID" };
                    AddBoundField("ItemPackagingID", "ID", true);
                    AddBoundField("ItemPackagingDesc", "Description", false);
                    AddBoundField("Symbol", "Symbol", false);
                    break;

                case "Items":
                    gvSupporTable.DataKeyNames = new[] { "ItemID" };
                    AddBoundField("ItemID", "ID", true);
                    AddBoundField("SKU", "SKU", false);
                    AddBoundField("ItemDesc", "Description", false);
                    AddBoundField("ItemShortName", "Short Name", false);
                    AddCheckBoxField("ItemEnabled", "Enabled");
                    break;

                case "PaymentTerms":
                    gvSupporTable.DataKeyNames = new[] { "PaymentTermID" };
                    AddBoundField("PaymentTermID", "ID", true);
                    AddBoundField("PaymentTermDesc", "Term", false);
                    AddBoundField("PaymentDays", "Days", false);
                    AddCheckBoxField("Enabled", "Enabled");
                    break;

                case "People":
                    gvSupporTable.DataKeyNames = new[] { "PersonID" };
                    AddBoundField("PersonID", "ID", true);
                    AddBoundField("PersonName", "Name", false);
                    AddBoundField("Abbreviation", "Abbreviation", false);
                    AddCheckBoxField("Enabled", "Enabled");
                    break;

                case "PriceLevels":
                    gvSupporTable.DataKeyNames = new[] { "PriceLevelID" };
                    AddBoundField("PriceLevelID", "ID", true);
                    AddBoundField("PriceLevelDesc", "Level", false);
                    AddBoundField("PricingFactor", "Factor", false);
                    AddCheckBoxField("Enabled", "Enabled");
                    break;

                case "RepairStatuses":
                    gvSupporTable.DataKeyNames = new[] { "RepairStatusID" };
                    AddBoundField("RepairStatusID", "ID", true);
                    AddBoundField("RepairStatusDesc", "Status", false);
                    AddBoundField("SortOrder", "Sort Order", false);
                    break;
            }
        }

        private void AddCommandColumn()
        {
            var cmdField = new CommandField
            {
                ShowEditButton = true,
                ButtonType = ButtonType.Image,
                EditImageUrl = "~/images/imgButtons/EditItem.gif",
                UpdateImageUrl = "~/images/imgButtons/UpdateItem.gif",
                CancelImageUrl = "~/images/imgButtons/CancelItem.gif",
                EditText = "Edit",
                UpdateText = "Update",
                CancelText = "Cancel"
            };
            gvSupporTable.Columns.Add(cmdField);
        }

        private void AddBoundField(string dataField, string headerText, bool readOnly)
        {
            var field = new BoundField
            {
                DataField = dataField,
                HeaderText = headerText,
                SortExpression = dataField,
                ReadOnly = readOnly
            };
            gvSupporTable.Columns.Add(field);
        }

        private void AddCheckBoxField(string dataField, string headerText)
        {
            var field = new CheckBoxField
            {
                DataField = dataField,
                HeaderText = headerText,
                SortExpression = dataField
            };
            gvSupporTable.Columns.Add(field);
        }

        private void BindSelectedTable()
        {
            string selectedTable = ViewState[CONST_SELECTEDTABLE_VIEWSTATE] as string ?? "";
            string sortBy = ViewState[CONST_SORTEXPRESSION_VIEWSTATE] as string;

            // Ensure columns are set up (for postbacks)
            if (gvSupporTable.Columns.Count == 0 && !string.IsNullOrEmpty(selectedTable))
            {
                SetupColumnsForTable(selectedTable);
            }

            try
            {
                switch (selectedTable)
                {
                    case "Areas": BindTable(new AreasRepository(), sortBy, "AreaName"); break;
                    case "AreaPrepDays": BindTable(new AreaPrepDaysRepository(), sortBy, "AreaID"); break;
                    case "EquipmentTypes": BindTable(new EquipTypesRepository(), sortBy, "EquipTypeName"); break;
                    case "InvoiceTypes": BindTable(new InvoiceTypesRepository(), sortBy, "InvoiceTypeDesc"); break;
                    case "ItemPackaging": BindTable(new ItemPackagingsRepository(), sortBy, "ItemPackagingDesc"); break;
                    case "Items": BindItemsTable(sortBy); break;
                    case "PaymentTerms": BindTable(new PaymentTermsRepository(), sortBy, "PaymentTermDesc"); break;
                    case "People": BindPeopleTable(sortBy); break;
                    case "PriceLevels": BindTable(new PriceLevelsRepository(), sortBy, "PriceLevelDesc"); break;
                    case "RepairStatuses": BindTable(new RepairStatusesRepository(), sortBy, "RepairStatusDesc"); break;
                    default:
                        gvSupporTable.DataSource = null;
                        gvSupporTable.DataBind();
                        break;
                }
                upnlSupporTables.Update();
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"<span style='color:red'>Error loading data: {ex.Message}</span>";
                AppLogger.WriteLog(SystemConstants.LogTypes.System, "SupportTables BindSelectedTable error: " + ex.Message);
            }
        }

        private void BindTable<T>(RepositoryBase<T> repo, string sortBy, string defaultSort) where T : new()
        {
            string effectiveSort = string.IsNullOrEmpty(sortBy) ? defaultSort : sortBy;
            var data = repo.GetAll(effectiveSort);
            gvSupporTable.DataSource = data;
            gvSupporTable.DataBind();
        }

        private void BindItemsTable(string sortBy)
        {
            var repo = new ItemsRepository();
            string effectiveSort = string.IsNullOrEmpty(sortBy) ? "ItemDesc" : sortBy;
            var items = repo.GetAll(effectiveSort);
            gvSupporTable.DataSource = items;
            gvSupporTable.DataBind();
        }

        private void BindPeopleTable(string sortBy)
        {
            var repo = new PersonsRepository();
            string effectiveSort = string.IsNullOrEmpty(sortBy) ? "Abbreviation" : sortBy;
            var people = repo.GetAll(effectiveSort);
            gvSupporTable.DataSource = people;
            gvSupporTable.DataBind();
        }

        #region Update Methods

        private bool UpdateArea(GridViewUpdateEventArgs e)
        {
            int id = Convert.ToInt32(gvSupporTable.DataKeys[e.RowIndex].Value);
            var repo = new AreasRepository();
            var entity = repo.GetById(id);
            if (entity != null)
            {
                entity.AreaName = e.NewValues["AreaName"]?.ToString() ?? "";
                repo.Update(entity);
                return true;
            }
            return false;
        }

        private bool UpdateAreaPrepDays(GridViewUpdateEventArgs e)
        {
            int id = Convert.ToInt32(gvSupporTable.DataKeys[e.RowIndex].Value);
            var repo = new AreaPrepDaysRepository();
            var entity = repo.GetById(id);
            if (entity != null)
            {
                entity.AreaID = e.NewValues["AreaID"] != null ? Convert.ToInt32(e.NewValues["AreaID"]) : 0;
                entity.PrepDayOfWeekID = e.NewValues["PrepDayOfWeekID"] != null && !string.IsNullOrEmpty(e.NewValues["PrepDayOfWeekID"].ToString()) 
                    ? Convert.ToByte(e.NewValues["PrepDayOfWeekID"]) : (byte?)null;
                entity.DeliveryDelayDays = e.NewValues["DeliveryDelayDays"] != null && !string.IsNullOrEmpty(e.NewValues["DeliveryDelayDays"].ToString()) 
                    ? Convert.ToInt16(e.NewValues["DeliveryDelayDays"]) : (short?)null;
                entity.DeliveryOrder = e.NewValues["DeliveryOrder"] != null && !string.IsNullOrEmpty(e.NewValues["DeliveryOrder"].ToString()) 
                    ? Convert.ToInt16(e.NewValues["DeliveryOrder"]) : (short?)null;
                repo.Update(entity);
                return true;
            }
            return false;
        }

        private bool UpdateEquipmentType(GridViewUpdateEventArgs e)
        {
            int id = Convert.ToInt32(gvSupporTable.DataKeys[e.RowIndex].Value);
            var repo = new EquipTypesRepository();
            var entity = repo.GetById(id);
            if (entity != null)
            {
                entity.EquipTypeName = e.NewValues["EquipTypeName"]?.ToString() ?? "";
                entity.EquipTypeDescription = e.NewValues["EquipTypeDescription"]?.ToString() ?? "";
                repo.Update(entity);
                return true;
            }
            return false;
        }

        private bool UpdateInvoiceType(GridViewUpdateEventArgs e)
        {
            int id = Convert.ToInt32(gvSupporTable.DataKeys[e.RowIndex].Value);
            var repo = new InvoiceTypesRepository();
            var entity = repo.GetById(id);
            if (entity != null)
            {
                entity.InvoiceTypeDesc = e.NewValues["InvoiceTypeDesc"]?.ToString() ?? "";
                entity.Enabled = e.NewValues["Enabled"] != null && Convert.ToBoolean(e.NewValues["Enabled"]);
                entity.Notes = e.NewValues["Notes"]?.ToString() ?? "";
                repo.Update(entity);
                return true;
            }
            return false;
        }

        private bool UpdateItemPackaging(GridViewUpdateEventArgs e)
        {
            int id = Convert.ToInt32(gvSupporTable.DataKeys[e.RowIndex].Value);
            var repo = new ItemPackagingsRepository();
            var entity = repo.GetById(id);
            if (entity != null)
            {
                entity.ItemPackagingDesc = e.NewValues["ItemPackagingDesc"]?.ToString() ?? "";
                entity.Symbol = e.NewValues["Symbol"]?.ToString() ?? "";
                repo.Update(entity);
                return true;
            }
            return false;
        }

        private bool UpdateItem(GridViewUpdateEventArgs e)
        {
            int id = Convert.ToInt32(gvSupporTable.DataKeys[e.RowIndex].Value);
            var repo = new ItemsRepository();
            var entity = repo.GetById(id);
            if (entity != null)
            {
                entity.SKU = e.NewValues["SKU"]?.ToString() ?? "";
                entity.ItemDesc = e.NewValues["ItemDesc"]?.ToString() ?? "";
                entity.ItemShortName = e.NewValues["ItemShortName"]?.ToString() ?? "";
                entity.ItemEnabled = e.NewValues["ItemEnabled"] != null && Convert.ToBoolean(e.NewValues["ItemEnabled"]);
                repo.Update(entity);
                return true;
            }
            return false;
        }

        private bool UpdatePaymentTerm(GridViewUpdateEventArgs e)
        {
            int id = Convert.ToInt32(gvSupporTable.DataKeys[e.RowIndex].Value);
            var repo = new PaymentTermsRepository();
            var entity = repo.GetById(id);
            if (entity != null)
            {
                entity.PaymentTermDesc = e.NewValues["PaymentTermDesc"]?.ToString() ?? "";
                entity.PaymentDays = e.NewValues["PaymentDays"] != null && !string.IsNullOrEmpty(e.NewValues["PaymentDays"].ToString()) 
                    ? Convert.ToInt32(e.NewValues["PaymentDays"]) : (int?)null;
                entity.Enabled = e.NewValues["Enabled"] != null && Convert.ToBoolean(e.NewValues["Enabled"]);
                repo.Update(entity);
                return true;
            }
            return false;
        }

        private bool UpdatePerson(GridViewUpdateEventArgs e)
        {
            int id = Convert.ToInt32(gvSupporTable.DataKeys[e.RowIndex].Value);
            var repo = new PersonsRepository();
            var entity = repo.GetById(id);
            if (entity != null)
            {
                entity.PersonName = e.NewValues["PersonName"]?.ToString() ?? "";
                entity.Abbreviation = e.NewValues["Abbreviation"]?.ToString() ?? "";
                entity.Enabled = e.NewValues["Enabled"] != null && Convert.ToBoolean(e.NewValues["Enabled"]);
                repo.Update(entity);
                return true;
            }
            return false;
        }

        private bool UpdatePriceLevel(GridViewUpdateEventArgs e)
        {
            int id = Convert.ToInt32(gvSupporTable.DataKeys[e.RowIndex].Value);
            var repo = new PriceLevelsRepository();
            var entity = repo.GetById(id);
            if (entity != null)
            {
                entity.PriceLevelDesc = e.NewValues["PriceLevelDesc"]?.ToString() ?? "";
                entity.PricingFactor = e.NewValues["PricingFactor"] != null && !string.IsNullOrEmpty(e.NewValues["PricingFactor"].ToString()) 
                    ? Convert.ToDouble(e.NewValues["PricingFactor"]) : (double?)null;
                entity.Enabled = e.NewValues["Enabled"] != null && Convert.ToBoolean(e.NewValues["Enabled"]);
                repo.Update(entity);
                return true;
            }
            return false;
        }

        private bool UpdateRepairStatus(GridViewUpdateEventArgs e)
        {
            int id = Convert.ToInt32(gvSupporTable.DataKeys[e.RowIndex].Value);
            var repo = new RepairStatusesRepository();
            var entity = repo.GetById(id);
            if (entity != null)
            {
                entity.RepairStatusDesc = e.NewValues["RepairStatusDesc"]?.ToString() ?? "";
                entity.SortOrder = e.NewValues["SortOrder"] != null && !string.IsNullOrEmpty(e.NewValues["SortOrder"].ToString()) 
                    ? Convert.ToInt32(e.NewValues["SortOrder"]) : (int?)null;
                repo.Update(entity);
                return true;
            }
            return false;
        }

        #endregion
    }
}
