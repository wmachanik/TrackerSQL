using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerSQL.Classes;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Tools
{
    public partial class SystemData : System.Web.UI.Page
    {
        private SysDataRepository _sysDataRepo = new SysDataRepository();
        private ItemServiceTypesRepository _itemServiceTypesRepo = new ItemServiceTypesRepository();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                dvSystemData.DataBind();
            }
        }

        public List<SysData> GetSystemDataForBinding()
        {
            try
            {
                SysData sysData = _sysDataRepo.GetSystemData();
                var list = new List<SysData>();
                if (sysData != null)
                    list.Add(sysData);
                return list;
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System, $"GetSystemDataForBinding failed: {ex.Message}");
                return new List<SysData>();
            }
        }

        public void UpdateSystemData(SysData updatedData)
        {
            try
            {
                if (updatedData == null)
                    updatedData = new SysData { ID = 1 };
                else
                    updatedData.ID = 1;

                _sysDataRepo.UpdateSystemData(updatedData);

                AppLogger.WriteLog(SystemConstants.LogTypes.System, "System data saved successfully");
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System, $"UpdateSystemData failed: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Helper method to display item service type name in read mode
        /// </summary>
        public string GetItemServiceTypeName(int? itemServiceTypeId)
        {
            if (!itemServiceTypeId.HasValue || itemServiceTypeId <= 0)
                return "(none)";

            try
            {
                ItemServiceType itemServiceType = _itemServiceTypesRepo.GetById(itemServiceTypeId.Value);
                if (itemServiceType != null && !string.IsNullOrEmpty(itemServiceType.ItemServiceTypeName))
                    return itemServiceType.ItemServiceTypeName;
                
                return $"(ID: {itemServiceTypeId})";
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.System, 
                    $"GetItemServiceTypeName({itemServiceTypeId}) failed: {ex.Message}");
                return $"(error loading service type {itemServiceTypeId})";
            }
        }

        /// <summary>
        /// Handles mode changes - CRITICAL: Must rebind after changing mode
        /// so DetailsView renders the correct controls for the new mode
        /// </summary>
        protected void dvSystemData_ModeChanging(object sender, DetailsViewModeEventArgs e)
        {
            try
            {
                // Change the mode
                dvSystemData.ChangeMode(e.NewMode);
                
                // CRITICAL: Rebind so DetailsView renders correct controls
                // Edit mode needs TextBox and DropDownList controls
                // Read mode needs Label controls
                dvSystemData.DataBind();
                
                AppLogger.WriteLog(SystemConstants.LogTypes.System, $"Mode changed to {e.NewMode}");
            }
            catch (Exception ex)
            {
                lblMessage.Text = $"Error changing mode: {ex.Message}";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                lblMessage.Visible = true;
                AppLogger.WriteLog(SystemConstants.LogTypes.System, $"dvSystemData_ModeChanging failed: {ex.Message}");
            }
        }

        protected void dvSystemData_ItemUpdated(object sender, DetailsViewUpdatedEventArgs e)
        {
            if (e.Exception == null)
            {
                try
                {
                    lblMessage.Text = "✓ System data updated successfully.";
                    lblMessage.ForeColor = System.Drawing.Color.Green;
                    lblMessage.Visible = true;

                    // Return to read mode and refresh
                    dvSystemData.ChangeMode(DetailsViewMode.ReadOnly);
                    dvSystemData.DataBind();
                }
                catch (Exception ex)
                {
                    lblMessage.Text = $"Error: {ex.Message}";
                    lblMessage.ForeColor = System.Drawing.Color.Red;
                    lblMessage.Visible = true;
                    AppLogger.WriteLog(SystemConstants.LogTypes.System, $"dvSystemData_ItemUpdated failed: {ex.Message}");
                    e.ExceptionHandled = true;
                }
            }
            else
            {
                lblMessage.Text = $"Save error: {e.Exception.Message}";
                lblMessage.ForeColor = System.Drawing.Color.Red;
                lblMessage.Visible = true;
                AppLogger.WriteLog(SystemConstants.LogTypes.System, $"ItemUpdated exception: {e.Exception.Message}");
                e.ExceptionHandled = true;
            }
        }

        protected void dvSystemData_DataBound(object sender, EventArgs e)
        {
            lblMessage.Visible = false;
        }
    }
}
