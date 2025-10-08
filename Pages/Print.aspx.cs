using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerDotNet.Classes;
using TrackerDotNet.Control;

namespace TrackerDotNet.Pages
{
  public partial class Print : System.Web.UI.Page
  {
    protected void Page_Load(object sender, EventArgs e)
    {
      PrintCtrl = Session["PrintCtrl"];
      QOnT.classes.PrintHelper.PrintWebControl(PrintCtrl);
    }
  }
}