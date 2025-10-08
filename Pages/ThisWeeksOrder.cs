// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.Pages.ThisWeeksOrder
// Assembly: TrackerDotNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerDotNet.dll

using System;
using System.Web.UI;
using System.Web.UI.WebControls;

//- only form later versions #nullable disable
namespace TrackerDotNet.Pages
{
    public class ThisWeeksOrder : Page
    {
        protected System.Web.UI.ScriptManager smOrderSummary;
        protected DropDownList ddlOrdersPerPage;
        protected UpdateProgress uproOrderSummary;
        protected UpdatePanel upanOrderSummary;
        protected GridView gvOutstandingOrders;
        protected ObjectDataSource odsOpenOrders;

        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void ddlOrdersPerPage_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.gvOutstandingOrders.PageSize = (int)Convert.ToInt16(this.ddlOrdersPerPage.SelectedValue);
        }
    }
}