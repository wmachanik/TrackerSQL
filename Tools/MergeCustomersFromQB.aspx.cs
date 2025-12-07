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

    class AreaToCityMap
    {
      string _Area;
      int _CityID;

      public AreaToCityMap()
      {
        _Area = string.Empty;
        _CityID = CityTblDAL.CONST_DEFAULT_CITYID;
      }

      public string Area { get { return _Area; } set { _Area = value; } }
      public int CityID { get { return _CityID; } set { _CityID = value; } }

    }

    const string CONST_LOGFILENAME = "~/App_Data/MergeCustomers.log";

    private LogFile _LogFile;
    private List<PaymentTermTranslor> _PaymentTermTranslors;
    private List<PriceLevelTranslor> _PriceLevelTranslors;
    List<AreaToCityMap> _AreaToCityMap;

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

    private List<AreaToCityMap> MapAreasToCityID()
    {
      List<AreaToCityMap> _AreaToCityIDs = new List<AreaToCityMap>();

      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Atlantic Seaboard", CityID = 9 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Benoni", CityID = 2 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Bloem", CityID = 5 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Bloemfontein", CityID = 5 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Cape", CityID = 1 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Cape Town", CityID = 1 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Cape Town CBD", CityID = 1 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Cape Town: CBD", CityID = 1 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Cape Town: Near CBD", CityID = 1 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Cape Town: Northern Suburbs", CityID = 10 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Cape Town: NSuburbs", CityID = 10 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Cape Town: Peninsula", CityID = 6 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Cape Town: S.suburbs", CityID = 8 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Cape Town: Southern", CityID = 8 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Cape Town: Southern Peninsula", CityID = 6 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Cape Town: Southern Penisula", CityID = 6 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Cape Town: Southern Suburbs", CityID = 8 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Cape Town: SSuburbs", CityID = 8 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Cape town: SSurbs", CityID = 8 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Cape Town: Stellenbosch", CityID = 3 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Cape Town: Woodstock", CityID = 1 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Cape Town:Atlantic Seaboard", CityID = 9 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Cape Town:Belville", CityID = 15 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Cape Town:CBD", CityID = 1 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Cape Town:Constantia", CityID = 11 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Cape Town:Hout Bay", CityID = 20 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Cape Town:HoutBay", CityID = 20 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Cape Town:Northern", CityID = 10 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Cape Town:Northern Sunurbs", CityID = 10 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Cape Town:Northern Surburbs", CityID = 10 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Cape Town:NSubrubs", CityID = 10 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Cape Town:NSuburbs", CityID = 10 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Cape Town:Paardien Eiland", CityID = 1 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Cape Town:Parow", CityID = 15 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Cape Town:Somerset", CityID = 4 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Cape Town:Southern Peninsula", CityID = 6 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Cape Town:Southern Suburbs", CityID = 8 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Cape Town:SSubrbs", CityID = 8 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Cape Town:SSuburbs", CityID = 8 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Cape Town:Town2Milnerton", CityID = 1 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CBD: Milnerton", CityID = 24 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CBD:Fhk", CityID = 6 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Constantia", CityID = 11 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT: / Tanzaina", CityID = 2 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT: Atlantic Seaboard", CityID = 9 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT: Beliville", CityID = 15 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT: Belville", CityID = 15 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT: Bishops court", CityID = 8 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT: Brackenfell", CityID = 15 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT: CBD", CityID = 1 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT: CDB", CityID = 1 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT: Central", CityID = 1 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT: Century Cty", CityID = 12 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT: Claremont", CityID = 8 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT: Const", CityID = 11 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT: Constantia", CityID = 11 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Cpt: Constnaita", CityID = 11 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT: Durbanville", CityID = 16 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT: E[[ing", CityID = 10 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT: Epping", CityID = 10 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT: Fhk", CityID = 6 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT: Fish hoek", CityID = 6 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT: Fshk", CityID = 6 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT: Gardens", CityID = 1 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT: HBay", CityID = 20 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT: Hourbay", CityID = 20 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Cpt: Hout Bay", CityID = 20 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT: HoutBay", CityID = 20 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT: Kenilworth", CityID = 8 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT: Kuilsriver", CityID = 10 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT: Kuilsrivier", CityID = 10 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT: Melkbos", CityID = 26 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT: MID", CityID = 1 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT: Milnerton", CityID = 24 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT: Montague Gardens", CityID = 12 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT: Mowbray", CityID = 1 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT: Muizenberg", CityID = 6 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT: Muizenbrg", CityID = 6 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT: N. Subs", CityID = 10 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT: N/Suburbs", CityID = 10 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT: Newlands", CityID = 8 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT: Noordhoek", CityID = 6 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Cpt: North", CityID = 10 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT: North Suburbs", CityID = 10 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Cpt: Northern Suburbs", CityID = 10 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT: NSubrbs", CityID = 10 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT: NSurbs", CityID = 10 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT: Paarden", CityID = 1 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT: Peninsula", CityID = 6 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT: Pinelands", CityID = 8 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "cpt: plumstead", CityID = 8 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT: Rndbsh", CityID = 8 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT: Rondebosch", CityID = 8 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT: Seapoint", CityID = 1 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT: Simonstown", CityID = 6 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT: SothSurbs", CityID = 8 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT: South Suburbs", CityID = 8 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT: Southern Pen", CityID = 6 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT: Southern Subrubs", CityID = 8 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT: Southern Suburbs", CityID = 8 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT: SouthernS", CityID = 8 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Cpt: SouthernSurbs", CityID = 8 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT: SoutherS", CityID = 8 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT: SoutherSubrbs", CityID = 8 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT: SSburbs", CityID = 8 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT: SSubrbs", CityID = 8 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT: SSuburb", CityID = 8 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT: SSuburbs", CityID = 8 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT: SSurbs", CityID = 8 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT: SSurubs", CityID = 8 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT: Sthrn", CityID = 8 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT: SthrnSbrs", CityID = 8 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT: SthrnSubrubs", CityID = 8 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT: Toaki", CityID = 6 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT: Tokai", CityID = 6 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT: Town", CityID = 1 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT: Walkin", CityID = 11 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Cpt: Westake2Muizenberg", CityID = 6 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT: Westlake", CityID = 6 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT: Westlk", CityID = 6 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT: Woodstock", CityID = 1 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT: Wynberg", CityID = 6 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "cpt:: Belville", CityID = 15 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT:CBD", CityID = 1 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT:Cllct", CityID = 11 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT:Const", CityID = 11 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT:Constantia", CityID = 11 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT:Noordhoek", CityID = 6 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CPT:SSubrbs", CityID = 8 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CT Ssuburbs", CityID = 8 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CT: CBD", CityID = 1 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "CTP: SSurbs", CityID = 8 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Durban", CityID = 14 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "East London", CityID = 13 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "George", CityID = 19 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "GordonsBay", CityID = 4 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Grahamstown", CityID = 5 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Hermanus", CityID = 5 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Hout Bay", CityID = 20 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Jhb", CityID = 2 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Jhb: East", CityID = 2 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Jhb: Honeydew", CityID = 2 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Jhb: Midrand", CityID = 2 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "JHB: Obs", CityID = 2 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Jhb: Sandown", CityID = 2 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Jhb: Sandton", CityID = 2 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Jhb:Edenvale", CityID = 2 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Jhb:Randburg", CityID = 2 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Johannesberg", CityID = 2 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Johannesburg", CityID = 2 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Johannesburg: Alberton", CityID = 2 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Johannesburg: Central West", CityID = 2 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Johannesburg: Kempton", CityID = 2 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Johannesburg: Midrand", CityID = 2 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Johannesburg: North", CityID = 2 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Johannesburg: Rosebank", CityID = 2 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Johannesburg:Randburg", CityID = 2 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Kakamas", CityID = 5 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Kempton Park", CityID = 2 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Krugersdorp", CityID = 2 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "KZN:Vryheid", CityID = 5 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Melkbos", CityID = 26 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Midrand", CityID = 2 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Mpumalanga", CityID = 5 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Northern Suburbs", CityID = 10 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "other", CityID = 1 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "PE", CityID = 28 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Phalaborwa", CityID = 5 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "PMB", CityID = 2 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Port Alfred", CityID = 5 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Potch", CityID = 2 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Pretoria", CityID = 2 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Pretoria:Centrurion", CityID = 2 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "PTA", CityID = 2 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Regional", CityID = 5 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Regional: Agulus", CityID = 5 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "RegionalSA", CityID = 5 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Rhodes", CityID = 5 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Rustenberg", CityID = 5 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Sandton", CityID = 2 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Secunda", CityID = 5 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Somerset", CityID = 4 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Somerset West", CityID = 4 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "South Suburtbs", CityID = 8 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Souther Suburbs", CityID = 8 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Southern Suburbs", CityID = 8 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Stellenbosch", CityID = 3 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Stellenbosch / Paarl", CityID = 3 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Tulbach", CityID = 5 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Vereeniging", CityID = 5 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Vryheid", CityID = 5 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "walkin", CityID = 11 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Welkom", CityID = 5 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Westlake", CityID = 6 });
      _AreaToCityIDs.Add(new AreaToCityMap { Area = "Witbank", CityID = 5 });


      return _AreaToCityIDs;
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
 * private int GetCityID(string pCityName)
    {
      int _ID = 0;
      if (!string.IsNullOrEmpty(pCityName))
      {
        // try get the City ID from the name
        CityTblDAL _City = new CityTblDAL();
        _ID = _City.GetCityID(pCityName);
      }
      if (_ID == 0)
        _ID = CityTblDAL.CONST_DEFAULT_CITYID;

      return _ID;
    }
*/
    
    private int FindCity(string pArea, string pShipLines)
    {
      int _CityID = CityTblDAL.CONST_DEFAULT_CITYID;

      /// first see if the area is found, otherwise see if either line5/4 or 3 is part found;

      if ((pArea.Trim().Length > 0) && (_AreaToCityMap.Exists(x => x.Area.Contains(pArea))))
        _CityID = _AreaToCityMap.Find(x => x.Area.Contains(pArea)).CityID;
      else
      {
        char[] _whitespace = new char[] { ' ', '\t' };
        string[] _AreaFirstName = pShipLines.Split(_whitespace);

        int i = 0;
        bool _found = false;
        while ((!_found) && (i < _AreaFirstName.Length))
        {
          if (_AreaToCityMap.Exists(x => x.Area.Contains(_AreaFirstName[i])))
          {
            _CityID = _AreaToCityMap.Find(x => x.Area.Contains(_AreaFirstName[i])).CityID;
            _found = true;
          }
          i++;
        }
      }
      return _CityID;
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
        string _city = pDataRow["CUSTFLD1"].ToString();
        pCustomer.City = FindCity(_city, _NewCustomerAccInfo.ShipAddr3 + " " + _NewCustomerAccInfo.ShipAddr4 + " " + _NewCustomerAccInfo.ShipAddr5);
        
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
      _AreaToCityMap = MapAreasToCityID();

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