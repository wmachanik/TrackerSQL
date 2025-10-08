// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.DataSets.CustomersDataSetTableAdapters.CustomersTableAdapter
// Assembly: TrackerDotNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerDotNet.dll

using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Diagnostics;

#nullable disable
namespace TrackerDotNet.DataSets.CustomersDataSetTableAdapters;

[DesignerCategory("code")]
[Designer("Microsoft.VSDesigner.DataSource.Design.TableAdapterDesigner, Microsoft.VSDesigner, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
[HelpKeyword("vs.data.TableAdapter")]
[ToolboxItem(true)]
[DataObject(true)]
public class CustomersTableAdapter : Component
{
  private OleDbDataAdapter _adapter;
  private OleDbConnection _connection;
  private OleDbTransaction _transaction;
  private OleDbCommand[] _commandCollection;
  private bool _clearBeforeFill;

  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  [DebuggerNonUserCode]
  public CustomersTableAdapter() => this.ClearBeforeFill = true;

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  protected internal OleDbDataAdapter Adapter
  {
    get
    {
      if (this._adapter == null)
        this.InitAdapter();
      return this._adapter;
    }
  }

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  internal OleDbConnection Connection
  {
    get
    {
      if (this._connection == null)
        this.InitConnection();
      return this._connection;
    }
    set
    {
      this._connection = value;
      if (this.Adapter.InsertCommand != null)
        this.Adapter.InsertCommand.Connection = value;
      if (this.Adapter.DeleteCommand != null)
        this.Adapter.DeleteCommand.Connection = value;
      if (this.Adapter.UpdateCommand != null)
        this.Adapter.UpdateCommand.Connection = value;
      for (int index = 0; index < this.CommandCollection.Length; ++index)
      {
        if (this.CommandCollection[index] != null)
          this.CommandCollection[index].Connection = value;
      }
    }
  }

  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  [DebuggerNonUserCode]
  internal OleDbTransaction Transaction
  {
    get => this._transaction;
    set
    {
      this._transaction = value;
      for (int index = 0; index < this.CommandCollection.Length; ++index)
        this.CommandCollection[index].Transaction = this._transaction;
      if (this.Adapter != null && this.Adapter.DeleteCommand != null)
        this.Adapter.DeleteCommand.Transaction = this._transaction;
      if (this.Adapter != null && this.Adapter.InsertCommand != null)
        this.Adapter.InsertCommand.Transaction = this._transaction;
      if (this.Adapter == null || this.Adapter.UpdateCommand == null)
        return;
      this.Adapter.UpdateCommand.Transaction = this._transaction;
    }
  }

  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  [DebuggerNonUserCode]
  protected OleDbCommand[] CommandCollection
  {
    get
    {
      if (this._commandCollection == null)
        this.InitCommandCollection();
      return this._commandCollection;
    }
  }

  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  [DebuggerNonUserCode]
  public bool ClearBeforeFill
  {
    get => this._clearBeforeFill;
    set => this._clearBeforeFill = value;
  }

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  private void InitAdapter()
  {
    this._adapter = new OleDbDataAdapter();
    this._adapter.TableMappings.Add((object) new DataTableMapping()
    {
      SourceTable = "Table",
      DataSetTable = "Customers",
      ColumnMappings = {
        {
          "CompanyName",
          "CompanyName"
        },
        {
          "CustomerID",
          "CustomerID"
        },
        {
          "ContactTitle",
          "ContactTitle"
        },
        {
          "ContactFirstName",
          "ContactFirstName"
        },
        {
          "ContactLastName",
          "ContactLastName"
        },
        {
          "ContactAltFirstName",
          "ContactAltFirstName"
        },
        {
          "ContactAltLastName",
          "ContactAltLastName"
        },
        {
          "Department",
          "Department"
        },
        {
          "BillingAddress",
          "BillingAddress"
        },
        {
          "StateOrProvince",
          "StateOrProvince"
        },
        {
          "PostalCode",
          "PostalCode"
        },
        {
          "PhoneNumber",
          "PhoneNumber"
        },
        {
          "Extension",
          "Extension"
        },
        {
          "FaxNumber",
          "FaxNumber"
        },
        {
          "CellNumber",
          "CellNumber"
        },
        {
          "EmailAddress",
          "EmailAddress"
        },
        {
          "AltEmailAddress",
          "AltEmailAddress"
        },
        {
          "CustomerType",
          "CustomerType"
        },
        {
          "EquipTypeName",
          "EquipTypeName"
        },
        {
          "CoffeePreference",
          "CoffeePreference"
        },
        {
          "City",
          "City"
        },
        {
          "ItemDesc",
          "ItemDesc"
        },
        {
          "PriPrefQty",
          "PriPrefQty"
        },
        {
          "Abreviation",
          "Abreviation"
        },
        {
          "MachineSN",
          "MachineSN"
        },
        {
          "UsesFilter",
          "UsesFilter"
        },
        {
          "autofulfill",
          "autofulfill"
        },
        {
          "enabled",
          "enabled"
        },
        {
          "PredictionDisabled",
          "PredictionDisabled"
        },
        {
          "AlwaysSendChkUp",
          "AlwaysSendChkUp"
        },
        {
          "NormallyResponds",
          "NormallyResponds"
        },
        {
          "Notes",
          "Notes"
        }
      }
    });
  }

  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  [DebuggerNonUserCode]
  private void InitConnection()
  {
    this._connection = new OleDbConnection();
    this._connection.ConnectionString = ConfigurationManager.ConnectionStrings["Tracker08ConnectionString"].ConnectionString;
  }

  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  [DebuggerNonUserCode]
  private void InitCommandCollection()
  {
    this._commandCollection = new OleDbCommand[4];
    this._commandCollection[0] = new OleDbCommand();
    this._commandCollection[0].Connection = this.Connection;
    this._commandCollection[0].CommandText = "SELECT        CustomersTbl.CompanyName, CustomersTbl.CustomerID, CustomersTbl.ContactTitle, CustomersTbl.ContactFirstName, CustomersTbl.ContactLastName, \r\n                         CustomersTbl.ContactAltFirstName, CustomersTbl.ContactAltLastName, CustomersTbl.Department, CustomersTbl.BillingAddress, CustomersTbl.StateOrProvince, \r\n                         CustomersTbl.PostalCode, CustomersTbl.PhoneNumber, CustomersTbl.Extension, CustomersTbl.FaxNumber, CustomersTbl.CellNumber, \r\n                         CustomersTbl.EmailAddress, CustomersTbl.AltEmailAddress, CustomersTbl.CustomerType, EquipTypeTbl.EquipTypeName, CustomersTbl.CoffeePreference, \r\n                         CityTbl.City, ItemTypeTbl.ItemDesc, CustomersTbl.PriPrefQty, PersonsTbl.Abreviation, CustomersTbl.MachineSN, CustomersTbl.UsesFilter, CustomersTbl.autofulfill, \r\n                         CustomersTbl.enabled, CustomersTbl.PredictionDisabled, CustomersTbl.AlwaysSendChkUp, CustomersTbl.NormallyResponds, CustomersTbl.Notes\r\nFROM            ((((CityTbl INNER JOIN\r\n                         CustomersTbl ON CityTbl.ID = CustomersTbl.City) INNER JOIN\r\n                         EquipTypeTbl ON CustomersTbl.EquipType = EquipTypeTbl.EquipTypeId) INNER JOIN\r\n                         ItemTypeTbl ON CustomersTbl.CoffeePreference = ItemTypeTbl.ItemTypeID) INNER JOIN\r\n                         PersonsTbl ON CustomersTbl.PreferedAgent = PersonsTbl.PersonID)\r\nORDER BY CustomersTbl.CompanyName";
    this._commandCollection[0].CommandType = CommandType.Text;
    this._commandCollection[1] = new OleDbCommand();
    this._commandCollection[1].Connection = this.Connection;
    this._commandCollection[1].CommandText = "SELECT        CustomersTbl.CompanyName, CustomersTbl.CustomerID, CustomersTbl.ContactTitle, CustomersTbl.ContactFirstName, CustomersTbl.ContactLastName, \r\n                         CustomersTbl.ContactAltFirstName, CustomersTbl.ContactAltLastName, CustomersTbl.Department, CustomersTbl.BillingAddress, CustomersTbl.StateOrProvince, \r\n                         CustomersTbl.PostalCode, CustomersTbl.PhoneNumber, CustomersTbl.Extension, CustomersTbl.FaxNumber, CustomersTbl.CellNumber, \r\n                         CustomersTbl.EmailAddress, CustomersTbl.AltEmailAddress, CustomersTbl.CustomerType, EquipTypeTbl.EquipTypeName, CustomersTbl.CoffeePreference, \r\n                         CityTbl.City, ItemTypeTbl.ItemDesc, CustomersTbl.PriPrefQty, PersonsTbl.Abreviation, CustomersTbl.MachineSN, CustomersTbl.UsesFilter, CustomersTbl.autofulfill, \r\n                         CustomersTbl.enabled, CustomersTbl.PredictionDisabled, CustomersTbl.AlwaysSendChkUp, CustomersTbl.NormallyResponds, CustomersTbl.Notes\r\nFROM            ((((CityTbl INNER JOIN\r\n                         CustomersTbl ON CityTbl.ID = CustomersTbl.City) INNER JOIN\r\n                         EquipTypeTbl ON CustomersTbl.EquipType = EquipTypeTbl.EquipTypeId) INNER JOIN\r\n                         ItemTypeTbl ON CustomersTbl.CoffeePreference = ItemTypeTbl.ItemTypeID) INNER JOIN\r\n                         PersonsTbl ON CustomersTbl.PreferedAgent = PersonsTbl.PersonID)\r\nWHERE        (CustomersTbl.enabled = ?)\r\nORDER BY CustomersTbl.CompanyName";
    this._commandCollection[1].CommandType = CommandType.Text;
    this._commandCollection[1].Parameters.Add(new OleDbParameter("enabled", OleDbType.Boolean, 2, ParameterDirection.Input, (byte) 0, (byte) 0, "enabled", DataRowVersion.Current, false, (object) null));
    this._commandCollection[2] = new OleDbCommand();
    this._commandCollection[2].Connection = this.Connection;
    this._commandCollection[2].CommandText = "SELECT        CustomersTbl.CompanyName, CustomersTbl.CustomerID, CustomersTbl.ContactTitle, CustomersTbl.ContactFirstName, CustomersTbl.ContactLastName, \r\n                         CustomersTbl.ContactAltFirstName, CustomersTbl.ContactAltLastName, CustomersTbl.Department, CustomersTbl.BillingAddress, CustomersTbl.StateOrProvince, \r\n                         CustomersTbl.PostalCode, CustomersTbl.PhoneNumber, CustomersTbl.Extension, CustomersTbl.FaxNumber, CustomersTbl.CellNumber, \r\n                         CustomersTbl.EmailAddress, CustomersTbl.AltEmailAddress, CustomersTbl.CustomerType, EquipTypeTbl.EquipTypeName, CustomersTbl.CoffeePreference, \r\n                         CityTbl.City, ItemTypeTbl.ItemDesc, CustomersTbl.PriPrefQty, PersonsTbl.Abreviation, CustomersTbl.MachineSN, CustomersTbl.UsesFilter, CustomersTbl.autofulfill, \r\n                         CustomersTbl.enabled, CustomersTbl.PredictionDisabled, CustomersTbl.AlwaysSendChkUp, CustomersTbl.NormallyResponds, CustomersTbl.Notes\r\nFROM            ((((CityTbl INNER JOIN\r\n                         CustomersTbl ON CityTbl.ID = CustomersTbl.City) INNER JOIN\r\n                         EquipTypeTbl ON CustomersTbl.EquipType = EquipTypeTbl.EquipTypeId) INNER JOIN\r\n                         ItemTypeTbl ON CustomersTbl.CoffeePreference = ItemTypeTbl.ItemTypeID) INNER JOIN\r\n                         PersonsTbl ON CustomersTbl.PreferedAgent = PersonsTbl.PersonID)\r\nWHERE        (CustomersTbl.CustomerID = ?)\r\nORDER BY CustomersTbl.CompanyName";
    this._commandCollection[2].CommandType = CommandType.Text;
    this._commandCollection[2].Parameters.Add(new OleDbParameter("CustomerID", OleDbType.Integer, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "CustomerID", DataRowVersion.Current, false, (object) null));
    this._commandCollection[3] = new OleDbCommand();
    this._commandCollection[3].Connection = this.Connection;
    this._commandCollection[3].CommandText = "UPDATE       CustomersTbl\r\nSET                CompanyName =, ContactTitle =, ContactFirstName =, ContactLastName =, ContactAltFirstName =, ContactAltLastName =, Department =, BillingAddress =, City =, \r\n                         StateOrProvince =, PostalCode =, [Country/Region] =, PhoneNumber =, Extension =, FaxNumber =, CellNumber =, EmailAddress =, AltEmailAddress =, ContractNo =, \r\n                         CustomerType =, EquipType =, CoffeePreference =, PriPrefQty =, SecondaryPreference =, SecPrefQty =, TypicallySecToo =, PreferedAgent =, MachineSN =, \r\n                         UsesFilter =, autofulfill =, enabled =, PredictionDisabled =, AlwaysSendChkUp =, NormallyResponds =, Notes =\r\nWHERE        (CustomerID = ?)";
    this._commandCollection[3].CommandType = CommandType.Text;
    this._commandCollection[3].Parameters.Add(new OleDbParameter("Original_CustomerID", OleDbType.Integer, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "CustomerID", DataRowVersion.Original, false, (object) null));
  }

  [HelpKeyword("vs.data.TableAdapter")]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  [DataObjectMethod(DataObjectMethodType.Fill, true)]
  [DebuggerNonUserCode]
  public virtual int Fill(CustomersDataSet.CustomersDataTable dataTable)
  {
    this.Adapter.SelectCommand = this.CommandCollection[0];
    if (this.ClearBeforeFill)
      dataTable.Clear();
    return this.Adapter.Fill((DataTable) dataTable);
  }

  [HelpKeyword("vs.data.TableAdapter")]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  [DebuggerNonUserCode]
  [DataObjectMethod(DataObjectMethodType.Select, true)]
  public virtual CustomersDataSet.CustomersDataTable GetData()
  {
    this.Adapter.SelectCommand = this.CommandCollection[0];
    CustomersDataSet.CustomersDataTable data = new CustomersDataSet.CustomersDataTable();
    this.Adapter.Fill((DataTable) data);
    return data;
  }

  [DebuggerNonUserCode]
  [HelpKeyword("vs.data.TableAdapter")]
  [DataObjectMethod(DataObjectMethodType.Select, false)]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  public virtual CustomersDataSet.CustomersDataTable GetCustomersByEnabled(bool? enabled)
  {
    this.Adapter.SelectCommand = this.CommandCollection[1];
    if (enabled.HasValue)
      this.Adapter.SelectCommand.Parameters[0].Value = (object) enabled.Value;
    else
      this.Adapter.SelectCommand.Parameters[0].Value = (object) DBNull.Value;
    CustomersDataSet.CustomersDataTable customersByEnabled = new CustomersDataSet.CustomersDataTable();
    this.Adapter.Fill((DataTable) customersByEnabled);
    return customersByEnabled;
  }

  [DataObjectMethod(DataObjectMethodType.Select, false)]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  [DebuggerNonUserCode]
  [HelpKeyword("vs.data.TableAdapter")]
  public virtual CustomersDataSet.CustomersDataTable GetDataByCustId(int CustomerID)
  {
    this.Adapter.SelectCommand = this.CommandCollection[2];
    this.Adapter.SelectCommand.Parameters[0].Value = (object) CustomerID;
    CustomersDataSet.CustomersDataTable dataByCustId = new CustomersDataSet.CustomersDataTable();
    this.Adapter.Fill((DataTable) dataByCustId);
    return dataByCustId;
  }

  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  [HelpKeyword("vs.data.TableAdapter")]
  [DataObjectMethod(DataObjectMethodType.Update, false)]
  [DebuggerNonUserCode]
  public virtual int UpdateCustomerByID(int Original_CustomerID)
  {
    OleDbCommand command = this.CommandCollection[3];
    command.Parameters[0].Value = (object) Original_CustomerID;
    ConnectionState state = command.Connection.State;
    if ((command.Connection.State & ConnectionState.Open) != ConnectionState.Open)
      command.Connection.Open();
    try
    {
      return command.ExecuteNonQuery();
    }
    finally
    {
      if (state == ConnectionState.Closed)
        command.Connection.Close();
    }
  }
}
