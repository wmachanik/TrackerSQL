// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.Pages.SupportTables
// Assembly: TrackerDotNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerDotNet.dll

using AjaxControlToolkit;
using System;
using System.Web.UI;
using System.Web.UI.WebControls;

//- only form later versions #nullable disable
namespace TrackerDotNet.Pages
{
    public partial class SupportTables : Page
    {
        protected ScriptManager smSupporTables;
        protected UpdateProgress uprgSupporTables;
        protected DropDownList ddlTables;
        protected UpdatePanel upnlSupporTables;
        protected GridView gvSupporTable;
        protected ObjectDataSource odsItemTypeTbl;

        protected void Page_Load(object sender, EventArgs e)
        {
        }

        private void BindItemsTableToGrid()
        {
            this.gvSupporTable.AutoGenerateEditButton = true;
            this.gvSupporTable.DataSourceID = "odsItemTypeTbl";
            this.gvSupporTable.DataBind();
            this.upnlSupporTables.Update();
        }

        protected void ddlTables_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (this.ddlTables.SelectedValue)
            {
                case "Items":
                    this.BindItemsTableToGrid();
                    break;
            }
        }
    }
}