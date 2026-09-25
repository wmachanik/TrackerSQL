using System;
using System.Web.UI.WebControls;

namespace TrackerSQL.Portal
{
    public partial class MyRecurring : PortalPageBase
    {
        protected GridView gv;
        protected System.Web.UI.UpdatePanel upList;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
                BindList();
        }

        private void BindList()
        {
            gv.DataSource = Portal.GetOwnRecurring();
            gv.DataBind();
        }

        protected void gv_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gv.PageIndex = e.NewPageIndex;
            BindList();
        }
    }
}
