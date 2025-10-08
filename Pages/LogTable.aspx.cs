using System;
using System.Web.UI.WebControls;
using TrackerDotNet.Classes;
using TrackerDotNet.Controls;

namespace TrackerDotNet.Pages
{
  public partial class LogTable: System.Web.UI.Page
  {
    const string CONST_WHERECLAUSE_SESSIONVAR = "CustomerLogWhereFilter";
    const int CONST_GVCOL_CONTACTNAME = 4;
    const int CONST_GVCOL_JOBCARD = 5;
    const int CONST_GVCOL_EQUIPMENT = 6;
    const int CONST_GVCOL_MACHINESN = 7;
    const int CONST_GVCOL_FAULT = 8;
    const int CONST_GVCOL_FAULTDESC = 9;
    const int CONST_GVCOL_ROID = 10;

    protected void Page_PreInit(object sender, EventArgs e)
    {
      CheckBrowser _CheckBrowser = new CheckBrowser();
      bool _RunningOnMobile = _CheckBrowser.fBrowserIsMobile();
      Session[CheckBrowser.CONST_SESSION_RUNNINGONMOBILE] = _RunningOnMobile;

      //if (_RunningOnMobile)
      //{
      //  this.MasterPageFile = "~/MobileSite.master";
      //}
      //else
      //{
      //  this.MasterPageFile = "~/Site.master";
      //}
    }

    protected void Page_Load(object sender, EventArgs e)
    {
      if (!IsPostBack)
      {
        bool _RunningOnMobile = (bool)Session[CheckBrowser.CONST_SESSION_RUNNINGONMOBILE];
        if (_RunningOnMobile)
        {
          // TextBox _tbxFilterBy = (TextBox)this.Page.FindControl("tbxFilterBy");
          tbxFilterBy.Width = new Unit(8, UnitType.Em);
        }
      }
    }
    public string GetCompanyName(long pCompanyID)
    {
      if (pCompanyID > 0)
      {
        CompanyNames _Companys = new CompanyNames();
        return _Companys.GetCompanyNameByCompanyID(pCompanyID);
      }
      else
        return String.Empty;
    }
    public string GetMachineDesc(int pEquipID)
    {
      if (pEquipID > 0)
      {
        EquipTypeTbl _EquipType = new EquipTypeTbl();
        return _EquipType.GetEquipName(pEquipID);
      }
      else
        return String.Empty;
    }
    public string GetSectionDescription(int pSectionTypeID)
    {
      if (pSectionTypeID > 0)
      {
        SectionTypesTbl _SectionTypesTbl = new SectionTypesTbl();
        return _SectionTypesTbl.GetSectionTypeByID(pSectionTypeID);
      }
      else
        return String.Empty;
    }
    public string GetTransactionDescription(int pTransactionTypeID)
    {
      if (pTransactionTypeID > 0)
      {
        TransactionTypesTbl _TransactionTypesTbl = new TransactionTypesTbl();
        return _TransactionTypesTbl.GetTransactionTypeByID(pTransactionTypeID);
      }
      else
        return String.Empty;
    }
    protected void gvLog_RowDataBound(object sender, GridViewRowEventArgs e)
    {
      bool _RunningOnMobile = (bool)Session[CheckBrowser.CONST_SESSION_RUNNINGONMOBILE];
      if (_RunningOnMobile)
      {
        e.Row.Cells[CONST_GVCOL_CONTACTNAME].Visible = false;
        e.Row.Cells[CONST_GVCOL_EQUIPMENT].Visible = false;
        // e.Row.Cells[CONST_GVCOL_MACHINESN].Visible = false;
        e.Row.Cells[CONST_GVCOL_FAULT].Visible = false;
        e.Row.Cells[CONST_GVCOL_FAULTDESC].Visible = false;
        e.Row.Cells[CONST_GVCOL_ROID].Visible = false;
        //      e.Row.Cells[CONST_GVCOL_JOBCARD].Visible = false;
      }
    }
    protected void btnGo_Click(object sender, EventArgs e)
    {
      if ((ddlFilterBy.SelectedValue != "0") && (!String.IsNullOrWhiteSpace(tbxFilterBy.Text)))
      {
//        Session[CONST_WHERECLAUSE_SESSIONVAR] = (ddlFilterBy.SelectedValue + " LIKE '%" + tbxFilterBy.Text + "%'");

//      tbxFilterBy.Text = "";
//        odsLog.DataBind();
      }
    }

    protected void btnReset_Click(object sender, EventArgs e)
    {
      Session[CONST_WHERECLAUSE_SESSIONVAR] = "";

      ddlFilterBy.SelectedIndex = 0;
      tbxFilterBy.Text = "";
      odsLogTbl.DataBind();
    }

    protected void tbxFilterBy_TextChanged(object sender, EventArgs e)
    {
      if ((!String.IsNullOrWhiteSpace(tbxFilterBy.Text)) && (ddlFilterBy.SelectedIndex == 0))
      {
        ddlFilterBy.SelectedIndex = 1;   // should be company
        upnlSelection.Update();
      }
    }

    public string GetPersonsNameFromID(string pPersonsID)
    {
      string _Name = "anon";
      int _PersonsID = 0;

      if (!Int32.TryParse(pPersonsID, out _PersonsID)) 
        _PersonsID = 0;
      PersonsTbl _Person = new PersonsTbl();
      if (_PersonsID > 0)
        _Name = _Person.PersonsNameFromID(_PersonsID);

      return _Name;
    }

    public string GetSectionFromID(string pSectionID)
    {
      string _Section = "n/a";
      int _SectionID = 0;

      if (!Int32.TryParse(pSectionID, out _SectionID))
        _SectionID = 0;
      SectionTypesTbl _SectionTypes = new SectionTypesTbl();
      if (_SectionID > 0)
        _Section = _SectionTypes.GetSectionTypeByID(_SectionID);

      return _Section;
    }

    public string GetTransactionFromID(string pTransactionID)
    {
      string _Transaction = "n/a";
      int _TransactionID = 0;

      if (!Int32.TryParse(pTransactionID, out _TransactionID))
        _TransactionID = 0;
      TransactionTypesTbl _TransactionTypes = new TransactionTypesTbl();
      if (_TransactionID > 0)
        _Transaction = _TransactionTypes.GetTransactionTypeByID(_TransactionID);

      return _Transaction;
    }

    public string GetCustomerFromID(string pCustomerID)
    {
      string _CustomerName = "n/a";
      int _CustomerID = 0;

      if (!Int32.TryParse(pCustomerID, out _CustomerID))
        _CustomerID = 0;
      CustomersTbl _Customer = new CustomersTbl();
      if (_CustomerID > 0)
        _Customer = _Customer.GetCustomerByCustomerID(_CustomerID);

      return _CustomerName;
    }
  }
}