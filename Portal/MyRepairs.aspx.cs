using System;
using System.Web.UI.WebControls;
using TrackerSQL.Repositories;

namespace TrackerSQL.Portal
{
    public partial class MyRepairs : PortalPageBase
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
            gv.DataSource = Portal.GetOwnRepairs();
            gv.DataBind();
        }

        protected void gv_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gv.PageIndex = e.NewPageIndex;
            BindList();
        }

        protected static bool IsDoneStatus(int repairStatusId)
        {
            return repairStatusId == RepairsRepository.DoneStatusId;
        }
    }
}
