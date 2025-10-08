using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerDotNet.Controls;

namespace TrackerDotNet.Tools
{
    public partial class SystemData : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {
                // Bind the data on first load
                dvSystemData.DataBind();
            }
        }

        protected void dvSystemData_ItemUpdated(object sender, DetailsViewUpdatedEventArgs e)
        {
            if (e.Exception == null)
            {
                lblMessage.Text = "System data updated successfully.";
                lblMessage.Visible = true;
                dvSystemData.DataBind(); // Refresh the data
            }
            else
            {
                lblMessage.Text = "Error updating system data: " + e.Exception.Message;
                lblMessage.ForeColor = System.Drawing.Color.Red;
                lblMessage.Visible = true;
                e.ExceptionHandled = true;
            }
        }

        protected void dvSystemData_DataBound(object sender, EventArgs e)
        {
            // Clear any previous messages when data is bound
            lblMessage.Visible = false;
        }

        protected string GetItemTypeName(object groupItemTypeId)
        {
            if (groupItemTypeId == null || groupItemTypeId == DBNull.Value)
                return "None Selected";

            int itemTypeId;
            if (!int.TryParse(groupItemTypeId.ToString(), out itemTypeId) || itemTypeId <= 0)
                return "None Selected";

            try
            {
                ItemTypeTbl itemType = new ItemTypeTbl();
                var itemTypes = itemType.GetAll();
                
                foreach (var item in itemTypes)
                {
                    if (item.ItemTypeID == itemTypeId)
                    {
                        return $"{item.ItemDesc} (ID: {item.ItemTypeID})";
                    }
                }
                
                return $"Unknown Item Type (ID: {itemTypeId})";
            }
            catch (Exception ex)
            {
                return $"Error loading item type: {ex.Message}";
            }
        }
    }
}