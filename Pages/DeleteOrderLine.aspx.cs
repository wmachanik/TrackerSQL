// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.Pages.DeleteOrderLine
// Assembly: TrackerDotNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerDotNet.dll

using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerDotNet.Classes;

//- only form later versions #nullable disable
namespace TrackerDotNet.Pages
{
    public partial class DeleteOrderLine : Page
    {
        protected DetailsView dvDeleteOrderItem;
        protected SqlDataSource sdsOrderLine;
        protected Button btnDelete;
        protected Button btnReturn;
        protected Literal ltrlStatus;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (this.IsPostBack)
                return;
            if (this.Request.UrlReferrer == (Uri)null)
            {
                this.Session["ReturnOrderURL"] = (object)"";
                this.btnReturn.Enabled = false;
            }
            else
                this.Session["ReturnOrderURL"] = (object)this.Request.UrlReferrer.OriginalString.ToString();
        }

        protected void ReturnToDetailPage()
        {
            string url = this.Session["ReturnOrderURL"].ToString();
            if (url.Length <= 0)
                return;
            this.Response.Redirect(url);
        }

        protected void btnDelete_Click(object sender, EventArgs e)
        {
            if (this.Request.QueryString["OrderId"] == null)
                return;
            string strSQL = "DELETE FROM OrdersTbl WHERE OrderID = " + this.Request.QueryString["OrderId"].ToString();
            TrackerDb trackerDb = new TrackerDb();
            string str = trackerDb.ExecuteNonQuerySQL(strSQL);
            trackerDb.Close();
            this.ltrlStatus.Text = string.IsNullOrEmpty(str) ? "Item Deleted" : "Error deleting item: " + str;
            this.ReturnToDetailPage();
        }

        protected void btnReturn_Click(object sender, EventArgs e) => this.ReturnToDetailPage();
    }
}