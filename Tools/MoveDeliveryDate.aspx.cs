using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerDotNet.Classes;
using TrackerDotNet.Controls;

namespace TrackerDotNet.Tools
{
  public partial class MoveDeliveryDate : System.Web.UI.Page
  {
    protected void Page_Load(object sender, EventArgs e)
    {
      if (!IsPostBack)
      {
//        OldDeliveryDateTextBox.Text = String.Format("{0:d}", TimeZoneUtils.Now().Date);
        NewDeliveryDateTextBox.Text = String.Format("{0:d}", TimeZoneUtils.Now().AddDays(1).Date);
      }

    }

    protected void btnMove_Click(object sender, EventArgs e)
    {
      DateTime _OldDate = Convert.ToDateTime(OldDeliveryDateDDL.SelectedValue);
      DateTime _NewDate = Convert.ToDateTime(NewDeliveryDateTextBox.Text);

      if ((_OldDate == System.DateTime.MinValue) || (_NewDate == System.DateTime.MinValue))
      {
        StatusLiteral.Text = "new and old dates must be valid";
      }
      else
      {

        TrackerDotNet.Controls.NextRoastDateByCityTbl _NRD = new NextRoastDateByCityTbl();
        int _numRecs = 0;
        string _result = _NRD.MoveDeliveryDate(_OldDate, _NewDate, ref _numRecs);

        if (String.IsNullOrEmpty(_result))
        {
          List<int> _NRDIDs = _NRD.GetAllIDsByDate(_OldDate);
          foreach (int _NRDID in _NRDIDs)
          {
            _result += _NRD.UpdateDeliveryDateByID(_NRDID,_OldDate);
            _numRecs++;
          }
          if (!String.IsNullOrEmpty(_result))
            StatusLiteral.Text = "ERROR: " + _result;
          else
            StatusLiteral.Text = String.Format("Move done: {0}record(s) updated.", _numRecs);

          gvPrepData.DataBind();
          OldDeliveryDateDDL.DataBind();
        }
        else
        {
          StatusLiteral.Text = "ERROR: " + _result;
        }
      }

    }
  }
}