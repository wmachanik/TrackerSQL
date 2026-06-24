using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using TrackerSQL.Classes;
using TrackerSQL.Controls;

namespace TrackerSQL.Tools
{
  public partial class MergeCustomersFromQB : System.Web.UI.Page
  {

    class PaymentTermTranslor
    {
      string _QBPaymentTermDesc;
      string _QonTPaymentTermDesc;
      int _QonTPaymentTermID;

      public PaymentTermTranslor()
      {
        _QBPaymentTermDesc = string.Empty;
        _QonTPaymentTermDesc = string.Empty;
        _QonTPaymentTermID = 0;
      }

      public string QBPaymentTermDesc { get { return _QBPaymentTermDesc; } set { _QBPaymentTermDesc = value; } }
      public string QonTPaymentTermDesc { get { return _QonTPaymentTermDesc; } set { _QonTPaymentTermDesc = value; } }
      public int QonTPaymentTermID { get { return _QonTPaymentTermID; } set { _QonTPaymentTermID = value; } }

      public int GetQonTPaymentTermIDByDesc(string pDesc)
      {
        PaymentTermsTbl _PaymentTerms = new PaymentTermsTbl();

        return _PaymentTerms.GetPaymentTermIDByDesc(pDesc);
      }

    }

    class PriceLevelTranslor
    {
      string _QBPriceLevelDesc;
      string _QonTPriceLevelDesc;
      int _QonTPriceLevelID;

      public PriceLevelTranslor()
      {
        _QBPriceLevelDesc = string.Empty;
        _QonTPriceLevelDesc = string.Empty;
        _QonTPriceLevelID = 0;
      }

      public string QBPriceLevelDesc { get { return _QBPriceLevelDesc; } set { _QBPriceLevelDesc = value; } }
      public string QonTPriceLevelDesc { get { return _QonTPriceLevelDesc; } set { _QonTPriceLevelDesc = value; } }
      public int QonTPriceLevelID { get { return _QonTPriceLevelID; } set { _QonTPriceLevelID = value; } }

      public int GetQonTPriceLevelIDByDesc(string pDesc)
      {
        PriceLevelsTbl _PriceLevels = new PriceLevelsTbl();

        return _PriceLevels.GetPriceLevelIDByDesc(pDesc);
      }

    }

    class AreaToAreaMap
    {
      string _Area;
      int _AreaID;

      public AreaToAreaMap()
      {
        _Area = string.Empty;
        _AreaID = AreaTblDAL.CONST_DEFAULT_AreaID;
      }

      public string Area { get { return _Area; } set { _Area = value; } }
      public int AreaID { get { return _AreaID; } set { _AreaID = value; } }

    }

    const string CONST_LOGFILENAME = "~/App_Data/MergeCustomers.log";

    private LogFile _LogFile;
    private List<PaymentTermTranslor> _PaymentTermTranslors;
    private List<PriceLevelTranslor> _PriceLevelTranslors;
    List<AreaToAreaMap> _AreaToAreaMap;

    protected void Page_Load(object sender, EventArgs e)
    {
      if (!IsPostBack)
      {
        char _c = '0';
        string _thisChar = _c.ToString();

        while (_c <= 'Z')
        {
          _thisChar = _c.ToString();
          StartDropDownList.Items.Add(new ListItem { Text = _thisChar, Value = _thisChar });
          FinishDropDownList.Items.Add(new ListItem { Text = _thisChar, Value = _thisChar });
          _c++;
        }

        StartDropDownList.SelectedIndex = 0;
        FinishDropDownList.SelectedIndex = FinishDropDownList.Items.Count-1;

        for (int i = 0; i < 500; i=i+25)
        {
          MaxRecsDropDownList.Items.Add(i.ToString());
        }
      }

    }

    private DataTable MergeFileWithData(string pFileName)
    {
      DataTable _table = new DataTable();
      string[] _allLines = File.ReadAllLines(pFileName);
      int _NumCols = 0;

      foreach (string _lines in _allLines)
      {
        //skip all lines t at do not contain Customer data
        if ((!_lines.StartsWith("!CUSTNAMEDICT")) && (_lines.StartsWith("!CUST")))
        {
          // create the columns
          string[] _cols = _lines.Split('\t');
          _NumCols = _cols.Length;
          foreach (string _col in _cols)
          {
            _table.Columns.Add(_col);
          }

        }

        if ((!_lines.StartsWith("CUSTNAMEDICT")) && (_lines.StartsWith("CUST")))
        {
          // create the columns
          string[] _rows = _lines.Split('\t');
          if (_rows.Length == _NumCols)
            _table.Rows.Add(_rows);
       }

      }

      return _table;

    }
    private DataTable ReadLogFileData(string pFileName)
    {
      DataTable _table = new DataTable();
      string[] _allLines = File.ReadAllLines(pFileName);

      int _MaxCols = 2;

      foreach (string _lines in _allLines)
      {
          string[] _thisline = _lines.Split(',');
          if (_thisline.Length > _MaxCols)
            _MaxCols = _thisline.Length;
      }
      _table.Columns.Add("Date");
      for (int i = 1; i < _MaxCols; i++)
			{
        _table.Columns.Add("Desc " + i.ToString());
			}

      foreach (string _lines in _allLines)
      {
        _table.Rows.Add(_lines.Split(','));
      }

      return _table;

    }
    private List<PaymentTermTranslor> GetPaymentTermList()
    {
      List<PaymentTermTranslor> _PaymentTermTranslors = new List<PaymentTermTranslor>();

      _PaymentTermTranslors.Add(  new  PaymentTermTranslor { QBPaymentTermDesc = "Due on receipt", QonTPaymentTermDesc  = "Due on receipt"});
      _PaymentTermTranslors.Add(  new  PaymentTermTranslor { QBPaymentTermDesc = "MidMonth", QonTPaymentTermDesc  = "MidMonth"});
      _PaymentTermTranslors.Add(  new  PaymentTermTranslor { QBPaymentTermDesc = "Net 15", QonTPaymentTermDesc  = "Net 15"});
      _PaymentTermTranslors.Add(  new  PaymentTermTranslor { QBPaymentTermDesc = "Net 30", QonTPaymentTermDesc  = "Net 30"});
      _PaymentTermTranslors.Add(  new  PaymentTermTranslor { QBPaymentTermDesc = "OnStatement", QonTPaymentTermDesc  = "OnStatement"});

      foreach (PaymentTermTranslor _PaymentTermTranslor in _PaymentTermTranslors)
      {
        _PaymentTermTranslor.QonTPaymentTermID = _PaymentTermTranslor.GetQonTPaymentTermIDByDesc(_PaymentTermTranslor.QonTPaymentTermDesc);
      }

      _PaymentTermTranslors.Sort((x, y) => x.QBPaymentTermDesc.CompareTo(y.QBPaymentTermDesc));
      return _PaymentTermTranslors;
    }

    private List<PriceLevelTranslor> GetPriceLevelList()
    {
      List<PriceLevelTranslor> _PriceLevelTranslors = new List<PriceLevelTranslor>();

      _PriceLevelTranslors.Add(new PriceLevelTranslor { QBPriceLevelDesc = "Standard", QonTPriceLevelDesc = "Standard" });
      _PriceLevelTranslors.Add(new PriceLevelTranslor { QBPriceLevelDesc = "Family/Charity/Agent", QonTPriceLevelDesc = "Family/Charity/Agent" });
      _PriceLevelTranslors.Add(new PriceLevelTranslor { QBPriceLevelDesc = "COD Discount", QonTPriceLevelDesc = "Discount 5%" });
      _PriceLevelTranslors.Add(new PriceLevelTranslor { QBPriceLevelDesc = "RRP", QonTPriceLevelDesc = "RRP" });
      _PriceLevelTranslors.Add(new PriceLevelTranslor { QBPriceLevelDesc = "CreditCard", QonTPriceLevelDesc = "RRP" });
      _PriceLevelTranslors.Add(new PriceLevelTranslor { QBPriceLevelDesc = "Dealer", QonTPriceLevelDesc = "Dealer" });
      _PriceLevelTranslors.Add(new PriceLevelTranslor { QBPriceLevelDesc = "Reseller", QonTPriceLevelDesc = "Dealer" });
      _PriceLevelTranslors.Add(new PriceLevelTranslor { QBPriceLevelDesc = "Farm", QonTPriceLevelDesc = "Host" });
      _PriceLevelTranslors.Add(new PriceLevelTranslor { QBPriceLevelDesc = "Pensioner", QonTPriceLevelDesc = "Pensioner" });
      _PriceLevelTranslors.Add(new PriceLevelTranslor { QBPriceLevelDesc = "Price With Dlvry", QonTPriceLevelDesc = "Standard" });
      _PriceLevelTranslors.Add(new PriceLevelTranslor { QBPriceLevelDesc = "Remote Agents", QonTPriceLevelDesc = "RemoteAgent" });

      foreach (PriceLevelTranslor _PriceLevelTranslor in _PriceLevelTranslors)
      {
        _PriceLevelTranslor.QonTPriceLevelID = _PriceLevelTranslor.GetQonTPriceLevelIDByDesc(_PriceLevelTranslor.QonTPriceLevelDesc);
      }

      _PriceLevelTranslors.Sort((x, y) => x.QBPriceLevelDesc.CompareTo(y.QBPriceLevelDesc));
      return _PriceLevelTranslors;
    }

    private List<AreaToAreaMap> MapAreasToAreaID()
    {
      List<AreaToAreaMap> _AreaToAreaIDs = new List<AreaToAreaMap>();

      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Atlantic Seaboard", AreaID = 9 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Benoni", AreaID = 2 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Bloem", AreaID = 5 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Bloemfontein", AreaID = 5 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Cape", AreaID = 1 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Cape Town", AreaID = 1 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Cape Town CBD", AreaID = 1 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Cape Town: CBD", AreaID = 1 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Cape Town: Near CBD", AreaID = 1 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Cape Town: Northern Suburbs", AreaID = 10 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Cape Town: NSuburbs", AreaID = 10 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Cape Town: Peninsula", AreaID = 6 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Cape Town: S.suburbs", AreaID = 8 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Cape Town: Southern", AreaID = 8 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Cape Town: Southern Peninsula", AreaID = 6 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Cape Town: Southern Penisula", AreaID = 6 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Cape Town: Southern Suburbs", AreaID = 8 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Cape Town: SSuburbs", AreaID = 8 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Cape town: SSurbs", AreaID = 8 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Cape Town: Stellenbosch", AreaID = 3 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Cape Town: Woodstock", AreaID = 1 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Cape Town:Atlantic Seaboard", AreaID = 9 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Cape Town:Belville", AreaID = 15 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Cape Town:CBD", AreaID = 1 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Cape Town:Constantia", AreaID = 11 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Cape Town:Hout Bay", AreaID = 20 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Cape Town:HoutBay", AreaID = 20 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Cape Town:Northern", AreaID = 10 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Cape Town:Northern Sunurbs", AreaID = 10 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Cape Town:Northern Surburbs", AreaID = 10 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Cape Town:NSubrubs", AreaID = 10 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Cape Town:NSuburbs", AreaID = 10 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Cape Town:Paardien Eiland", AreaID = 1 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Cape Town:Parow", AreaID = 15 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Cape Town:Somerset", AreaID = 4 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Cape Town:Southern Peninsula", AreaID = 6 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Cape Town:Southern Suburbs", AreaID = 8 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Cape Town:SSubrbs", AreaID = 8 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Cape Town:SSuburbs", AreaID = 8 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Cape Town:Town2Milnerton", AreaID = 1 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CBD: Milnerton", AreaID = 24 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CBD:Fhk", AreaID = 6 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Constantia", AreaID = 11 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT: / Tanzaina", AreaID = 2 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT: Atlantic Seaboard", AreaID = 9 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT: Beliville", AreaID = 15 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT: Belville", AreaID = 15 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT: Bishops court", AreaID = 8 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT: Brackenfell", AreaID = 15 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT: CBD", AreaID = 1 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT: CDB", AreaID = 1 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT: Central", AreaID = 1 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT: Century Cty", AreaID = 12 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT: Claremont", AreaID = 8 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT: Const", AreaID = 11 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT: Constantia", AreaID = 11 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Cpt: Constnaita", AreaID = 11 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT: Durbanville", AreaID = 16 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT: E[[ing", AreaID = 10 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT: Epping", AreaID = 10 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT: Fhk", AreaID = 6 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT: Fish hoek", AreaID = 6 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT: Fshk", AreaID = 6 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT: Gardens", AreaID = 1 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT: HBay", AreaID = 20 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT: Hourbay", AreaID = 20 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Cpt: Hout Bay", AreaID = 20 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT: HoutBay", AreaID = 20 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT: Kenilworth", AreaID = 8 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT: Kuilsriver", AreaID = 10 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT: Kuilsrivier", AreaID = 10 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT: Melkbos", AreaID = 26 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT: MID", AreaID = 1 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT: Milnerton", AreaID = 24 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT: Montague Gardens", AreaID = 12 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT: Mowbray", AreaID = 1 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT: Muizenberg", AreaID = 6 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT: Muizenbrg", AreaID = 6 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT: N. Subs", AreaID = 10 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT: N/Suburbs", AreaID = 10 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT: Newlands", AreaID = 8 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT: Noordhoek", AreaID = 6 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Cpt: North", AreaID = 10 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT: North Suburbs", AreaID = 10 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Cpt: Northern Suburbs", AreaID = 10 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT: NSubrbs", AreaID = 10 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT: NSurbs", AreaID = 10 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT: Paarden", AreaID = 1 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT: Peninsula", AreaID = 6 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT: Pinelands", AreaID = 8 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "cpt: plumstead", AreaID = 8 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT: Rndbsh", AreaID = 8 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT: Rondebosch", AreaID = 8 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT: Seapoint", AreaID = 1 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT: Simonstown", AreaID = 6 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT: SothSurbs", AreaID = 8 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT: South Suburbs", AreaID = 8 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT: Southern Pen", AreaID = 6 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT: Southern Subrubs", AreaID = 8 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT: Southern Suburbs", AreaID = 8 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT: SouthernS", AreaID = 8 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Cpt: SouthernSurbs", AreaID = 8 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT: SoutherS", AreaID = 8 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT: SoutherSubrbs", AreaID = 8 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT: SSburbs", AreaID = 8 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT: SSubrbs", AreaID = 8 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT: SSuburb", AreaID = 8 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT: SSuburbs", AreaID = 8 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT: SSurbs", AreaID = 8 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT: SSurubs", AreaID = 8 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT: Sthrn", AreaID = 8 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT: SthrnSbrs", AreaID = 8 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT: SthrnSubrubs", AreaID = 8 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT: Toaki", AreaID = 6 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT: Tokai", AreaID = 6 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT: Town", AreaID = 1 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT: Walkin", AreaID = 11 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Cpt: Westake2Muizenberg", AreaID = 6 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT: Westlake", AreaID = 6 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT: Westlk", AreaID = 6 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT: Woodstock", AreaID = 1 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT: Wynberg", AreaID = 6 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "cpt:: Belville", AreaID = 15 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT:CBD", AreaID = 1 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT:Cllct", AreaID = 11 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT:Const", AreaID = 11 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT:Constantia", AreaID = 11 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT:Noordhoek", AreaID = 6 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CPT:SSubrbs", AreaID = 8 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CT Ssuburbs", AreaID = 8 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CT: CBD", AreaID = 1 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "CTP: SSurbs", AreaID = 8 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Durban", AreaID = 14 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "East London", AreaID = 13 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "George", AreaID = 19 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "GordonsBay", AreaID = 4 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Grahamstown", AreaID = 5 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Hermanus", AreaID = 5 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Hout Bay", AreaID = 20 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Jhb", AreaID = 2 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Jhb: East", AreaID = 2 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Jhb: Honeydew", AreaID = 2 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Jhb: Midrand", AreaID = 2 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "JHB: Obs", AreaID = 2 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Jhb: Sandown", AreaID = 2 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Jhb: Sandton", AreaID = 2 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Jhb:Edenvale", AreaID = 2 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Jhb:Randburg", AreaID = 2 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Johannesberg", AreaID = 2 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Johannesburg", AreaID = 2 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Johannesburg: Alberton", AreaID = 2 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Johannesburg: Central West", AreaID = 2 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Johannesburg: Kempton", AreaID = 2 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Johannesburg: Midrand", AreaID = 2 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Johannesburg: North", AreaID = 2 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Johannesburg: Rosebank", AreaID = 2 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Johannesburg:Randburg", AreaID = 2 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Kakamas", AreaID = 5 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Kempton Park", AreaID = 2 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Krugersdorp", AreaID = 2 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "KZN:Vryheid", AreaID = 5 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Melkbos", AreaID = 26 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Midrand", AreaID = 2 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Mpumalanga", AreaID = 5 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Northern Suburbs", AreaID = 10 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "other", AreaID = 1 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "PE", AreaID = 28 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Phalaborwa", AreaID = 5 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "PMB", AreaID = 2 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Port Alfred", AreaID = 5 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Potch", AreaID = 2 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Pretoria", AreaID = 2 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Pretoria:Centrurion", AreaID = 2 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "PTA", AreaID = 2 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Regional", AreaID = 5 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Regional: Agulus", AreaID = 5 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "RegionalSA", AreaID = 5 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Rhodes", AreaID = 5 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Rustenberg", AreaID = 5 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Sandton", AreaID = 2 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Secunda", AreaID = 5 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Somerset", AreaID = 4 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Somerset West", AreaID = 4 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "South Suburtbs", AreaID = 8 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Souther Suburbs", AreaID = 8 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Southern Suburbs", AreaID = 8 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Stellenbosch", AreaID = 3 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Stellenbosch / Paarl", AreaID = 3 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Tulbach", AreaID = 5 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Vereeniging", AreaID = 5 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Vryheid", AreaID = 5 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "walkin", AreaID = 11 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Welkom", AreaID = 5 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Westlake", AreaID = 6 });
      _AreaToAreaIDs.Add(new AreaToAreaMap { Area = "Witbank", AreaID = 5 });


      return _AreaToAreaIDs;
    }

    private List<CustomersTbl> GetAllCustomersWithEmail(string pEmailAddress, out string pEmailFound)
    {
      CustomersTbl _Customer = new CustomersTbl();
      List<CustomersTbl> _Customers = new List<CustomersTbl>();
      pEmailFound = string.Empty;
      if (!string.IsNullOrEmpty(pEmailAddress))
      {
        string[] _Emails = pEmailAddress.Split(',');
        int i = 0;
        while ((i < _Emails.Length) && (_Customers.Count == 0))
        {
          _Customers = _Customer.GetAllCustomerWithEmailLIKE(_Emails[i]);
          if (_Customers.Count > 0)
            pEmailFound = _Emails[i];
          i++;
        }
      }
      return _Customers;
    }
    private bool CustomerExists(ref CustomersTbl pCustomer)
    {
      bool _found = false;
      long _CustID = 0;
      
      // search on co name,first and last nameand email (need to split on ";")
      /// need logic here
      /// 
      // search on Co Name remove anything after brackets
      string _CustomerName = (pCustomer.CompanyName.Contains("(") ? pCustomer.CompanyName.Substring(0, pCustomer.CompanyName.IndexOf("(")-1) : pCustomer.CompanyName);

      List <CustomersTbl> _Companys = pCustomer.GetAllCustomerWithNameLIKE(_CustomerName+"%");

      if (_Companys.Count > 0)
      {
        // we have some customers find the one we want and set the ID
        int i = 0;
        while ((i < _Companys.Count) && (! _found)) 
        {
          // check that details are valid there are any details that are similar if so assume they exist)
          if (_Companys[i].CustomerID > 0)
          {
            if ((_Companys[i].EmailAddress == pCustomer.EmailAddress) || (_Companys[i].AltEmailAddress == pCustomer.AltEmailAddress))
              _found = true;
            else if (_Companys[i].AltEmailAddress == pCustomer.AltEmailAddress)
            {
              _found = true;
              pCustomer.EmailAddress = _Companys[i].EmailAddress;
              pCustomer.AltEmailAddress = _Companys[i].AltEmailAddress;
            }
            else if ((_Companys[i].ContactFirstName == pCustomer.ContactFirstName) || (_Companys[i].ContactLastName == pCustomer.ContactLastName))
              _found = true;
            else if (_Companys[i].ContactAltFirstName == pCustomer.ContactFirstName)
            {
              _found = true;
              pCustomer.ContactFirstName = _Companys[i].ContactFirstName;
              pCustomer.ContactAltFirstName = _Companys[i].ContactAltFirstName;
              pCustomer.ContactLastName = _Companys[i].ContactLastName;
              pCustomer.ContactAltLastName = _Companys[i].ContactAltLastName;
            }
          }
          if (_found)
            _CustID = _Companys[i].CustomerID;
          else
            i++;
        }
      }
      if (!_found)
      {
        if (_Companys.Count > 0)
          _Companys.Clear();
        string _FoundEmailAddress = string.Empty;
        _Companys = GetAllCustomersWithEmail(pCustomer.EmailAddress, out _FoundEmailAddress);
        if (_Companys.Count > 0)
          pCustomer.EmailAddress = _FoundEmailAddress;
        else if (!string.IsNullOrEmpty(pCustomer.AltEmailAddress))
        {
          _Companys = pCustomer.GetAllCustomerWithEmailLIKE(pCustomer.AltEmailAddress);
          if (_Companys.Count > 0)
            pCustomer.AltEmailAddress = _FoundEmailAddress;
        }

        int i = 0;
        while ((i < _Companys.Count) && (!_found))
        {
          // check that there are any details that are similar if so assume they exist)
          if (!string.IsNullOrEmpty(_Companys[i].EmailAddress) && (_Companys[i].EmailAddress == pCustomer.EmailAddress))
            _found = true;
          else if (!string.IsNullOrEmpty(_Companys[i].AltEmailAddress ) &&  (_Companys[i].AltEmailAddress == pCustomer.AltEmailAddress))
            _found = true;
          else if (!string.IsNullOrEmpty(_Companys[i].ContactFirstName) && (_Companys[i].ContactFirstName == pCustomer.ContactFirstName))
            _found = true;
          else if (!string.IsNullOrEmpty(_Companys[i].ContactLastName) && (_Companys[i].ContactLastName == pCustomer.ContactLastName))
            _found = true;
          else if (!string.IsNullOrEmpty(_Companys[i].ContactFirstName) && (_Companys[i].ContactAltFirstName == pCustomer.ContactFirstName))
          {
            _found = true;
            pCustomer.ContactFirstName = _Companys[i].ContactFirstName;
            pCustomer.ContactAltFirstName = _Companys[i].ContactAltFirstName;
            pCustomer.ContactLastName = _Companys[i].ContactLastName;
            pCustomer.ContactAltLastName = _Companys[i].ContactAltLastName;
          }
          if (_found)
            _CustID = _Companys[i].CustomerID;
          else
            i++;
        }
      }

      if (_found)
        pCustomer.CustomerID = _CustID;
//      else if (!pCustomer.enabled)
//        _found = true; // we did not find the customer but they are disabled

      if (_Companys.Count > 0)
        _Companys.Clear();

      return _found;
    }

    private int GetPresonIDFromRep(string pAbbreviation)
    {
      PersonsTbl _Person = new PersonsTbl();

      return _Person.PersonsIDFromAbbreviation(pAbbreviation);
    }

/*
 * private int GetAreaID(string pAreaName)
    {
      int _ID = 0;
      if (!string.IsNullOrEmpty(pAreaName))
      {
        // try get the Area ID from the name
        AreaTblDAL _Area = new AreaTblDAL();
        _ID = _Area.GetAreaID(pAreaName);
      }
      if (_ID == 0)
        _ID = AreaTblDAL.CONST_DEFAULT_AreaID;

      return _ID;
    }
*/
    
    private int FindArea(string pArea, string pShipLines)
    {
      int _AreaID = AreaTblDAL.CONST_DEFAULT_AreaID;

      /// first see if the area is found, otherwise see if either line5/4 or 3 is part found;

      if ((pArea.Trim().Length > 0) && (_AreaToAreaMap.Exists(x => x.Area.Contains(pArea))))
        _AreaID = _AreaToAreaMap.Find(x => x.Area.Contains(pArea)).AreaID;
      else
      {
        char[] _whitespace = new char[] { ' ', '\t' };
        string[] _AreaFirstName = pShipLines.Split(_whitespace);

        int i = 0;
        bool _found = false;
        while ((!_found) && (i < _AreaFirstName.Length))
        {
          if (_AreaToAreaMap.Exists(x => x.Area.Contains(_AreaFirstName[i])))
          {
            _AreaID = _AreaToAreaMap.Find(x => x.Area.Contains(_AreaFirstName[i])).AreaID;
            _found = true;
          }
          i++;
        }
      }
      return _AreaID;
    }
    private Double GetDoubleFromString(string pStr)
    {
      Double _Double = 0.0;

      if (!string.IsNullOrEmpty(pStr))
      {
        pStr = pStr.Replace("\"", string.Empty);
        if (!Double.TryParse(pStr, NumberStyles.Any, CultureInfo.InvariantCulture, out _Double))
          _Double = 0.0;
      }
      return _Double;
    }

    private CustomersAccInfoTbl GetCustAccInfoFromDataRow(DataRow pDataRow)
    {
      CustomersAccInfoTbl _CustomerAccInfo = new CustomersAccInfoTbl();

      _CustomerAccInfo.BillAddr1 = (pDataRow["BADDR1"] == null) ? string.Empty : pDataRow["BADDR1"].ToString();
      _CustomerAccInfo.BillAddr2 = (pDataRow["BADDR2"] == null) ? string.Empty : pDataRow["BADDR2"].ToString();
      _CustomerAccInfo.BillAddr3 = (pDataRow["BADDR3"] == null) ? string.Empty : pDataRow["BADDR3"].ToString();
      _CustomerAccInfo.BillAddr4 = (pDataRow["BADDR4"] == null) ? string.Empty : pDataRow["BADDR4"].ToString();
      _CustomerAccInfo.BillAddr5 = (pDataRow["BADDR5"] == null) ? string.Empty : pDataRow["BADDR5"].ToString();
      _CustomerAccInfo.ShipAddr1 = (pDataRow["SADDR1"] == null) ? string.Empty : pDataRow["SADDR1"].ToString();
      _CustomerAccInfo.ShipAddr2 = (pDataRow["SADDR2"] == null) ? string.Empty : pDataRow["SADDR2"].ToString();
      _CustomerAccInfo.ShipAddr3 = (pDataRow["SADDR3"] == null) ? string.Empty : pDataRow["SADDR3"].ToString();
      _CustomerAccInfo.ShipAddr4 = (pDataRow["SADDR4"] == null) ? string.Empty : pDataRow["SADDR4"].ToString();
      _CustomerAccInfo.ShipAddr5 = (pDataRow["SADDR5"] == null) ? string.Empty : pDataRow["SADDR5"].ToString();
      _CustomerAccInfo.AccEmail = (pDataRow["EMAIL"] == null) ? string.Empty : pDataRow["EMAIL"].ToString();
      string _PayTerms = (pDataRow["TERMS"] == null) ? string.Empty : pDataRow["TERMS"].ToString();
      if (_PaymentTermTranslors.Exists(x => x.QBPaymentTermDesc.Equals(_PayTerms)))
      {
        // we have found a payment term
        PaymentTermTranslor _ThisTerm = _PaymentTermTranslors.Find(x => x.QBPaymentTermDesc.Equals(_PayTerms));

        _CustomerAccInfo.PaymentTermID = _ThisTerm.QonTPaymentTermID;
      }

      _CustomerAccInfo.Limit = ((pDataRow["LIMIT"] == null) ) ? 0.0 : GetDoubleFromString(pDataRow["LIMIT"].ToString());
      _CustomerAccInfo.CustomerVATNo = (pDataRow["RESALENUM"] == null) ? string.Empty : pDataRow["RESALENUM"].ToString();
      _CustomerAccInfo.Notes = (pDataRow["REP"] == null) ? string.Empty : string.Format("REP: {0}{1}", pDataRow["REP"].ToString(), Environment.NewLine);
      _CustomerAccInfo.FullCoName = (pDataRow["COMPANYNAME"] == null) ? string.Empty : pDataRow["COMPANYNAME"].ToString();
      _CustomerAccInfo.AccFirstName = (pDataRow["FIRSTNAME"] == null) ? string.Empty : pDataRow["FIRSTNAME"].ToString();
      _CustomerAccInfo.AccLastName = (pDataRow["LASTNAME"] == null) ? string.Empty : pDataRow["LASTNAME"].ToString();
      if (pDataRow["MIDINIT"] != null)
      {
        string _Prefix = pDataRow["MIDINIT"].ToString();
        if (_Prefix.Equals("le") || _Prefix.Equals("vd") || _Prefix.Equals("van"))
        {
          _CustomerAccInfo.AccLastName = _Prefix + " " + _CustomerAccInfo.AccLastName;
        }
      }
      if ((pDataRow["CUSTFLD1"] != null) && !string.IsNullOrEmpty(pDataRow["CUSTFLD1"].ToString()))
        _CustomerAccInfo.Notes += string.Format("Area: {0}{1}", pDataRow["CUSTFLD1"].ToString(), Environment.NewLine); ;
      _CustomerAccInfo.RegNo = (pDataRow["CUSTFLD2"] == null) ? string.Empty : pDataRow["CUSTFLD2"].ToString();
      _CustomerAccInfo.BankAccNo = (pDataRow["CUSTFLD3"] == null) ? string.Empty : pDataRow["CUSTFLD3"].ToString();
      _CustomerAccInfo.BankBranch = (pDataRow["CUSTFLD4"] == null) ? string.Empty : pDataRow["CUSTFLD4"].ToString();
      if ((pDataRow["CUSTFLD5"] != null) && !string.IsNullOrEmpty(pDataRow["CUSTFLD5"].ToString()))
        _CustomerAccInfo.Notes += string.Format("SN: {0}{1}", pDataRow["CUSTFLD5"].ToString(), Environment.NewLine);
      _CustomerAccInfo.Enabled = (pDataRow["HIDDEN"] == null) ? true : (pDataRow["HIDDEN"].ToString() == "Y" );

      string _PriceLevel = (pDataRow["PRICELEVEL"] == null) ? string.Empty : pDataRow["PRICELEVEL"].ToString();
      if (_PriceLevelTranslors.Exists(x => x.QBPriceLevelDesc.Equals(_PriceLevel)))
      {
        // we have found a payment term
        PriceLevelTranslor _ThisLevel = _PriceLevelTranslors.Find(x => x.QBPriceLevelDesc.Equals(_PriceLevel));

        _CustomerAccInfo.PaymentTermID = _ThisLevel.QonTPriceLevelID;
      }

      return _CustomerAccInfo;
    }
    
    private void AddAccountInfoToCust(CustomersTbl pCustomer, DataRow pDataRow)
    { 
      if (pCustomer.CustomerID > 0)
      {
        // add accont in pDataRow into CustomerAcc info if it does not exist, if it does?
        CustomersAccInfoTbl _NewCustomerAccInfo = GetCustAccInfoFromDataRow(pDataRow);
        _NewCustomerAccInfo.CustomerID = pCustomer.CustomerID;
 //keep not //finding the account info when it exists, look at forcing CustomerID to be unique in the table (use access)
        CustomersAccInfoTbl _ExistingCustomerAccInfo = _NewCustomerAccInfo.GetByCustomerID(pCustomer.CustomerID);
        if (_ExistingCustomerAccInfo.CustomersAccInfoID > 0)
        {
          _LogFile.AddFormatStringToLog(", Account Info for Customer: {0} exists, adding what is not there", pCustomer.CompanyName);
          // data exits only overwrite if empty
          _NewCustomerAccInfo.CustomersAccInfoID = _ExistingCustomerAccInfo.CustomersAccInfoID;
          _NewCustomerAccInfo.CustomerVATNo = (_ExistingCustomerAccInfo.CustomerVATNo == string.Empty) ? _NewCustomerAccInfo.CustomerVATNo : string.Empty;
          _NewCustomerAccInfo.BillAddr1 = (_ExistingCustomerAccInfo.BillAddr1 == string.Empty) ? _NewCustomerAccInfo.BillAddr1 : _ExistingCustomerAccInfo.BillAddr1;
          _NewCustomerAccInfo.BillAddr2 = (_ExistingCustomerAccInfo.BillAddr2 == string.Empty) ? _NewCustomerAccInfo.BillAddr2 : _ExistingCustomerAccInfo.BillAddr2;
          _NewCustomerAccInfo.BillAddr3 = (_ExistingCustomerAccInfo.BillAddr3 == string.Empty) ? _NewCustomerAccInfo.BillAddr3 : _ExistingCustomerAccInfo.BillAddr3;
          _NewCustomerAccInfo.BillAddr4 = (_ExistingCustomerAccInfo.BillAddr4 == string.Empty) ? _NewCustomerAccInfo.BillAddr4 : _ExistingCustomerAccInfo.BillAddr4;
          _NewCustomerAccInfo.BillAddr5 = (_ExistingCustomerAccInfo.BillAddr5 == string.Empty) ? _NewCustomerAccInfo.BillAddr5 : _ExistingCustomerAccInfo.BillAddr5;
          _NewCustomerAccInfo.ShipAddr1 = (_ExistingCustomerAccInfo.ShipAddr1 == string.Empty) ? _NewCustomerAccInfo.ShipAddr1 : _ExistingCustomerAccInfo.ShipAddr1;
          _NewCustomerAccInfo.ShipAddr2 = (_ExistingCustomerAccInfo.ShipAddr2 == string.Empty) ? _NewCustomerAccInfo.ShipAddr2 : _ExistingCustomerAccInfo.ShipAddr2;
          _NewCustomerAccInfo.ShipAddr3 = (_ExistingCustomerAccInfo.ShipAddr3 == string.Empty) ? _NewCustomerAccInfo.ShipAddr3 : _ExistingCustomerAccInfo.ShipAddr3;
          _NewCustomerAccInfo.ShipAddr4 = (_ExistingCustomerAccInfo.ShipAddr4 == string.Empty) ? _NewCustomerAccInfo.ShipAddr4 : _ExistingCustomerAccInfo.ShipAddr4;
          _NewCustomerAccInfo.ShipAddr5 = (_ExistingCustomerAccInfo.ShipAddr5 == string.Empty) ? _NewCustomerAccInfo.ShipAddr5 : _ExistingCustomerAccInfo.ShipAddr5;
          _NewCustomerAccInfo.AccEmail = (_ExistingCustomerAccInfo.AccEmail == string.Empty) ? _NewCustomerAccInfo.AccEmail : _ExistingCustomerAccInfo.AccEmail;
          _NewCustomerAccInfo.AltAccEmail = (_ExistingCustomerAccInfo.AltAccEmail == string.Empty) ? _NewCustomerAccInfo.AltAccEmail : _ExistingCustomerAccInfo.AltAccEmail;
          _NewCustomerAccInfo.PaymentTermID = (_ExistingCustomerAccInfo.PaymentTermID > 0) ? _NewCustomerAccInfo.PaymentTermID : _ExistingCustomerAccInfo.PaymentTermID;
          _NewCustomerAccInfo.PriceLevelID = (_ExistingCustomerAccInfo.PriceLevelID > 0) ? _NewCustomerAccInfo.PriceLevelID : _ExistingCustomerAccInfo.PriceLevelID;
          _NewCustomerAccInfo.Limit = (_ExistingCustomerAccInfo.Limit > 0.0) ? _NewCustomerAccInfo.Limit : _ExistingCustomerAccInfo.Limit;
          _NewCustomerAccInfo.FullCoName = (_ExistingCustomerAccInfo.FullCoName == string.Empty) ? _NewCustomerAccInfo.FullCoName : _ExistingCustomerAccInfo.FullCoName;
          _NewCustomerAccInfo.AccFirstName = (_ExistingCustomerAccInfo.AccFirstName == string.Empty) ? _NewCustomerAccInfo.AccFirstName : _ExistingCustomerAccInfo.AccFirstName;
          _NewCustomerAccInfo.AccLastName = (_ExistingCustomerAccInfo.AccLastName == string.Empty) ? _NewCustomerAccInfo.AccLastName : _ExistingCustomerAccInfo.AccLastName;
          _NewCustomerAccInfo.AltAccFirstName = (_ExistingCustomerAccInfo.AltAccFirstName == string.Empty) ? _NewCustomerAccInfo.AltAccFirstName : _ExistingCustomerAccInfo.AltAccFirstName;
          _NewCustomerAccInfo.AltAccLastName = (_ExistingCustomerAccInfo.AltAccLastName == string.Empty) ? _NewCustomerAccInfo.AltAccLastName : _ExistingCustomerAccInfo.AltAccLastName;
          _NewCustomerAccInfo.RegNo = (_ExistingCustomerAccInfo.RegNo == string.Empty) ? _NewCustomerAccInfo.RegNo : _ExistingCustomerAccInfo.RegNo;
          _NewCustomerAccInfo.BankAccNo = (_ExistingCustomerAccInfo.BankAccNo == string.Empty) ? _NewCustomerAccInfo.BankAccNo : _ExistingCustomerAccInfo.BankAccNo;
          _NewCustomerAccInfo.BankBranch = (_ExistingCustomerAccInfo.BankBranch == string.Empty) ? _NewCustomerAccInfo.BankBranch : _ExistingCustomerAccInfo.BankBranch;
          _NewCustomerAccInfo.Notes = (_ExistingCustomerAccInfo.Notes == string.Empty) ? _NewCustomerAccInfo.Notes : _ExistingCustomerAccInfo.Notes;

          if (!_NewCustomerAccInfo.Equals(_ExistingCustomerAccInfo))
          {
            _LogFile.AddToLog(", some items where found to be different and have be updated.");
            _NewCustomerAccInfo.Update(_NewCustomerAccInfo, _ExistingCustomerAccInfo.CustomersAccInfoID);
          }
          else
            _LogFile.AddToLog(", not items updated.");

        }
        else
        {
          _LogFile.AddFormatStringToLog(", Account Info for Customer: {0} does not exist adding it", pCustomer.CompanyName);
          // no data exists so add
          //           this is Not inserting
          _NewCustomerAccInfo.Insert(_NewCustomerAccInfo);
        }
      }
      else
        _LogFile.AddFormatStringToLog(", Account Info for Customer: {0} cannot be added customer does not exist", pCustomer.CompanyName);
      
    }
    private void AddCustomerToNotFound(CustomersTbl pCustomer, DataRow pDataRow)
    {
      //add to a table to add later

      // if customer not hidden then add
      if (pCustomer.enabled)
      {
        _LogFile.AddFormatStringToLog(", Customer {0} is enabled so will add to the tracker system.", pCustomer.CompanyName);
        CustomersAccInfoTbl _NewCustomerAccInfo = GetCustAccInfoFromDataRow(pDataRow);
       
        pCustomer.ContactAltFirstName = _NewCustomerAccInfo.AltAccFirstName;
        pCustomer.ContactAltLastName = _NewCustomerAccInfo.AltAccLastName;
        
        pCustomer.BillingAddress = String.Format("{0};{1};{2};{3};{4}", _NewCustomerAccInfo.ShipAddr1, _NewCustomerAccInfo.ShipAddr2, _NewCustomerAccInfo.ShipAddr3, _NewCustomerAccInfo.ShipAddr4, _NewCustomerAccInfo.ShipAddr5) ;
        
        /// need to do something else here 
        /// remove postal code or something, this is not working
        string _Area = pDataRow["CUSTFLD1"].ToString();
        pCustomer.AreaID = FindArea(_Area, _NewCustomerAccInfo.ShipAddr3 + " " + _NewCustomerAccInfo.ShipAddr4 + " " + _NewCustomerAccInfo.ShipAddr5);
        
        // need to extr postal code look for a group of 4 digits that is on its own or next to text, not numbers
        Match _match = Regex.Match(pCustomer.BillingAddress, @"\\(?<num>\d{4,5})\\");
        
        if (_match.Success)
        {
          pCustomer.PostalCode = _match.Groups["num"].Value;
        }
        pCustomer.AltEmailAddress = _NewCustomerAccInfo.AltAccEmail;
        pCustomer.MachineSN = (pDataRow["CUSTFLD5"] == null) ? string.Empty : pDataRow["CUSTFLD5"].ToString();
        if (string.IsNullOrEmpty(pCustomer.MachineSN))
          pCustomer.CustomerTypeID = CustomerTypeTbl.CONST_COFFEE_ONLY;
        else
          pCustomer.CustomerTypeID = CustomerTypeTbl.CONST_OUTRIGHT_PURCHASE;
        
        if (pDataRow["REP"] != null)
        {
          pCustomer.SalesAgentID = GetPresonIDFromRep(pDataRow["REP"].ToString());
          pCustomer.PreferedAgent = pCustomer.SalesAgentID;
        }
        // if we still get nothing then
        if (pCustomer.SalesAgentID == 0)
        {
                    if (pCustomer.PostalCode.StartsWith("8") || pCustomer.PostalCode.StartsWith("7"))
                        pCustomer.PreferedAgent = SystemConstants.DeliveryConstants.DefaultDeliveryPersonID;  // TrackerTools.CONST_DEFAULT_DELIVERYBYID;
                    else
                        pCustomer.PreferedAgent = SystemConstants.DeliveryConstants.CourierDeliveryID; //  TrackerTools.CONST_DEFAULT_DELIVERYIDOFCOURIER;

          pCustomer.PreferedAgent = pCustomer.SalesAgentID;
        }


        // ot set here as it should be false by default pCustomer.enabled = _NewCustomerAccInfo.enabled      
        
        pCustomer.Notes = string.Format("Client added automatically by Account Merge date: {0:D}", TimeZoneUtils.Now());
        
        if (pCustomer.InsertCustomer(pCustomer))
        {
          _LogFile.AddFormatStringToLog(", Customer: {0} added to tracker system.", pCustomer.CompanyName);
          // add cusotmer and if added then add account info
          if (CustomerExists(ref pCustomer))
          {
            _LogFile.AddFormatStringToLog(", adding Account Info for Customer: {0} to tracker system.", pCustomer.CompanyName);
            AddAccountInfoToCust(pCustomer, pDataRow);
          }
          else
            _LogFile.AddFormatStringToLog(", Customer: {0} was not found could not add account info", pCustomer.CompanyName);
        }
        else
          _LogFile.AddFormatStringToLog(", error adding Customer: {0}", pCustomer.CompanyName);

      }      
    }
    
    void MergeCustomersTableToQonTData(DataTable pTable)
    {
      CustomersTbl _Customer = new CustomersTbl();
      _LogFile = new LogFile(Server.MapPath(CONST_LOGFILENAME), false);
      _PaymentTermTranslors = GetPaymentTermList();
      _PriceLevelTranslors = GetPriceLevelList();
      _AreaToAreaMap = MapAreasToAreaID();

      TrackerTools _TT = new TrackerTools();
      string _err = _TT.GetTrackerSessionErrorString();

      char _StartAt = StartDropDownList.SelectedValue[0];
      char _FinishedAt = FinishDropDownList.SelectedValue[0];
      int _MaxRecs = Convert.ToInt32(MaxRecsDropDownList.SelectedValue);

      long _NumRecs = 0;
      foreach (DataRow _DataRow in pTable.Rows)
      {
        // get customer info - ? Do we need it ?
        _Customer.CompanyName = (_DataRow["NAME"] == null) ? string.Empty : _DataRow["NAME"].ToString();
        _Customer.CompanyName = _Customer.CompanyName.Replace("\"", string.Empty);  // delete and double quotes
        if ((!string.IsNullOrEmpty(_Customer.CompanyName)) && ((_Customer.CompanyName[0] >= _StartAt) && (_Customer.CompanyName[0] <= _FinishedAt)))
        {
          _Customer.EmailAddress = (_DataRow["EMAIL"] == null) ? string.Empty : _DataRow["EMAIL"].ToString();
          _Customer.ContactFirstName = (_DataRow["FIRSTNAME"] == null) ? string.Empty : _DataRow["FIRSTNAME"].ToString();
          _Customer.ContactLastName = (_DataRow["LASTNAME"] == null) ? string.Empty : _DataRow["LASTNAME"].ToString();
          if (_DataRow["MIDINIT"] != null)
          {
            string _Prefix = _DataRow["MIDINIT"].ToString();
            if (_Prefix.Equals("le") || _Prefix.Equals("vd") || _Prefix.Equals("van"))
            {
              _Customer.ContactLastName = _Prefix + " " + _Customer.ContactLastName;
            }
          }
          #region AddGeneralDetails
          _Customer.PhoneNumber = (_DataRow["PHONE1"] == null) ? string.Empty : _DataRow["PHONE1"].ToString();
          _Customer.CellNumber = (_DataRow["PHONE2"] == null) ? string.Empty : _DataRow["PHONE2"].ToString();
          _Customer.FaxNumber = (_DataRow["FAXNUM"] == null) ? string.Empty : _DataRow["FAXNUM"].ToString();
          _Customer.enabled = (_DataRow["HIDDEN"] == null) ? false : _DataRow["HIDDEN"].ToString().Equals("Y");
          #endregion

          _LogFile.AddFormatStringToLog("Record: {0} of {1}, ", _NumRecs, pTable.Rows.Count);
          if (CustomerExists(ref _Customer))
          {
            _LogFile.AddFormatStringToLog("Customer: {0} found, adding info to account", _Customer.CompanyName);
            AddAccountInfoToCust(_Customer, _DataRow);
          }
          else
          {
            if (_Customer.enabled)
            {
              _LogFile.AddFormatStringToLog("Customer: {0} NOT found, adding customer to tracker ", _Customer.CompanyName);
              AddCustomerToNotFound(_Customer, _DataRow);
            }
            else
              _LogFile.AddFormatStringToLog("Customer: {0} NOT found, but is disabled so ignoring", _Customer.CompanyName);
          }
          
          _err = _TT.GetTrackerSessionErrorString().Replace(",",";");

          if (!string.IsNullOrEmpty(_err)) _LogFile.AddLineFormatStringToLog(",err: {0}", _err);
          else _LogFile.AddLineToLog(",done");
          if (_NumRecs % 15 == 0)
          {
            //string _errString = 
            _LogFile.WriteLinesToLogFile();
          }
          _NumRecs++;
          if (_NumRecs == _MaxRecs)
            break;
        }
  
      }
      _LogFile.WriteLinesToLogFile();


    }


    void ConvertTableToObj(DataTable pTable)
    {
      MergeCustomersTableToQonTData(pTable);
      DataTable _LogData = ReadLogFileData(Server.MapPath(CONST_LOGFILENAME));
      gvCustomers.DataSource = _LogData;
      gvCustomers.DataBind();
    }


    protected void SelectImportFileButton_Click(object sender, EventArgs e)
    {
      if (MergeFileUpload.HasFile)
      {
        // do the merge
        try
        {
          string _status = string.Empty;
          string _filename = Server.MapPath("~/App_Data/") + Path.GetFileName(MergeFileUpload.FileName);
          if (System.IO.File.Exists(_filename))
          {
            System.IO.File.Delete(_filename);
            _status = "Old version of file existed, it was deleted. ";
          }

          MergeFileUpload.SaveAs(_filename);
          StatusLabel.Text = _status + "Upload status: File uploaded!";
          DataTable _table = MergeFileWithData(_filename);
          ConvertTableToObj(_table);
        }
        catch (Exception _ex)
        {
          StatusLabel.Text = "Error saving file to server: " + _ex.Message;
          throw;
        }

      }

    }
    protected void gvCustomers_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
      gvCustomers.PageIndex = e.NewPageIndex;
      bindGridView(); //bindgridview will get the data source and bind it again
    }

    private void bindGridView()
    {
      gvCustomers.DataSource = ReadLogFileData(Server.MapPath(CONST_LOGFILENAME)); ;
      gvCustomers.DataBind();
    }
  }
}
