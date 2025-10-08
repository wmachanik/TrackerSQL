// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.DataSets.TrackerDataSetTableAdapters.CustomersTblTableAdapter
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
namespace TrackerDotNet.DataSets.TrackerDataSetTableAdapters;

[DesignerCategory("code")]
[DataObject(true)]
[Designer("Microsoft.VSDesigner.DataSource.Design.TableAdapterDesigner, Microsoft.VSDesigner, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
[HelpKeyword("vs.data.TableAdapter")]
[ToolboxItem(true)]
public class CustomersTblTableAdapter : Component
{
  private OleDbDataAdapter _adapter;
  private OleDbConnection _connection;
  private OleDbTransaction _transaction;
  private OleDbCommand[] _commandCollection;
  private bool _clearBeforeFill;

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  public CustomersTblTableAdapter() => this.ClearBeforeFill = true;

  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  [DebuggerNonUserCode]
  protected internal OleDbDataAdapter Adapter
  {
    get
    {
      if (this._adapter == null)
        this.InitAdapter();
      return this._adapter;
    }
  }

  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  [DebuggerNonUserCode]
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

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
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

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  protected OleDbCommand[] CommandCollection
  {
    get
    {
      if (this._commandCollection == null)
        this.InitCommandCollection();
      return this._commandCollection;
    }
  }

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
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
      DataSetTable = "CustomersTbl",
      ColumnMappings = {
        {
          "CustomerID",
          "CustomerID"
        },
        {
          "CompanyName",
          "CompanyName"
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
          "City",
          "City"
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
          "Country/Region",
          "Country/Region"
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
          "ContractNo",
          "ContractNo"
        },
        {
          "CustomerType",
          "CustomerType"
        },
        {
          "EquipType",
          "EquipType"
        },
        {
          "CoffeePreference",
          "CoffeePreference"
        },
        {
          "PriPrefQty",
          "PriPrefQty"
        },
        {
          "SecondaryPreference",
          "SecondaryPreference"
        },
        {
          "SecPrefQty",
          "SecPrefQty"
        },
        {
          "TypicallySecToo",
          "TypicallySecToo"
        },
        {
          "PreferedAgent",
          "PreferedAgent"
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
    this._adapter.DeleteCommand = new OleDbCommand();
    this._adapter.DeleteCommand.Connection = this.Connection;
    this._adapter.DeleteCommand.CommandText = "DELETE FROM `CustomersTbl` WHERE ((`CustomerID` = ?))";
    this._adapter.DeleteCommand.CommandType = CommandType.Text;
    this._adapter.DeleteCommand.Parameters.Add(new OleDbParameter("Original_CustomerID", OleDbType.Integer, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "CustomerID", DataRowVersion.Original, false, (object) null));
    this._adapter.InsertCommand = new OleDbCommand();
    this._adapter.InsertCommand.Connection = this.Connection;
    this._adapter.InsertCommand.CommandText = "INSERT INTO `CustomersTbl` (`CompanyName`, `ContactTitle`, `ContactFirstName`, `ContactLastName`, `ContactAltFirstName`, `ContactAltLastName`, `Department`, `BillingAddress`, `City`, `StateOrProvince`, `PostalCode`, `Country/Region`, `PhoneNumber`, `Extension`, `FaxNumber`, `CellNumber`, `EmailAddress`, `AltEmailAddress`, `ContractNo`, `CustomerType`, `EquipType`, `CoffeePreference`, `PriPrefQty`, `SecondaryPreference`, `SecPrefQty`, `TypicallySecToo`, `PreferedAgent`, `MachineSN`, `UsesFilter`, `autofulfill`, `enabled`, `PredictionDisabled`, `AlwaysSendChkUp`, `NormallyResponds`, `Notes`) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";
    this._adapter.InsertCommand.CommandType = CommandType.Text;
    this._adapter.InsertCommand.Parameters.Add(new OleDbParameter("CompanyName", OleDbType.VarWChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "CompanyName", DataRowVersion.Current, false, (object) null));
    this._adapter.InsertCommand.Parameters.Add(new OleDbParameter("ContactTitle", OleDbType.VarWChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "ContactTitle", DataRowVersion.Current, false, (object) null));
    this._adapter.InsertCommand.Parameters.Add(new OleDbParameter("ContactFirstName", OleDbType.VarWChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "ContactFirstName", DataRowVersion.Current, false, (object) null));
    this._adapter.InsertCommand.Parameters.Add(new OleDbParameter("ContactLastName", OleDbType.VarWChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "ContactLastName", DataRowVersion.Current, false, (object) null));
    this._adapter.InsertCommand.Parameters.Add(new OleDbParameter("ContactAltFirstName", OleDbType.VarWChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "ContactAltFirstName", DataRowVersion.Current, false, (object) null));
    this._adapter.InsertCommand.Parameters.Add(new OleDbParameter("ContactAltLastName", OleDbType.VarWChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "ContactAltLastName", DataRowVersion.Current, false, (object) null));
    this._adapter.InsertCommand.Parameters.Add(new OleDbParameter("Department", OleDbType.VarWChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "Department", DataRowVersion.Current, false, (object) null));
    this._adapter.InsertCommand.Parameters.Add(new OleDbParameter("BillingAddress", OleDbType.VarWChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "BillingAddress", DataRowVersion.Current, false, (object) null));
    this._adapter.InsertCommand.Parameters.Add(new OleDbParameter("City", OleDbType.Integer, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "City", DataRowVersion.Current, false, (object) null));
    this._adapter.InsertCommand.Parameters.Add(new OleDbParameter("StateOrProvince", OleDbType.VarWChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "StateOrProvince", DataRowVersion.Current, false, (object) null));
    this._adapter.InsertCommand.Parameters.Add(new OleDbParameter("PostalCode", OleDbType.VarWChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "PostalCode", DataRowVersion.Current, false, (object) null));
    this._adapter.InsertCommand.Parameters.Add(new OleDbParameter("Country/Region", OleDbType.VarWChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "Country/Region", DataRowVersion.Current, false, (object) null));
    this._adapter.InsertCommand.Parameters.Add(new OleDbParameter("PhoneNumber", OleDbType.VarWChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "PhoneNumber", DataRowVersion.Current, false, (object) null));
    this._adapter.InsertCommand.Parameters.Add(new OleDbParameter("Extension", OleDbType.VarWChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "Extension", DataRowVersion.Current, false, (object) null));
    this._adapter.InsertCommand.Parameters.Add(new OleDbParameter("FaxNumber", OleDbType.VarWChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "FaxNumber", DataRowVersion.Current, false, (object) null));
    this._adapter.InsertCommand.Parameters.Add(new OleDbParameter("CellNumber", OleDbType.VarWChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "CellNumber", DataRowVersion.Current, false, (object) null));
    this._adapter.InsertCommand.Parameters.Add(new OleDbParameter("EmailAddress", OleDbType.VarWChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "EmailAddress", DataRowVersion.Current, false, (object) null));
    this._adapter.InsertCommand.Parameters.Add(new OleDbParameter("AltEmailAddress", OleDbType.VarWChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "AltEmailAddress", DataRowVersion.Current, false, (object) null));
    this._adapter.InsertCommand.Parameters.Add(new OleDbParameter("ContractNo", OleDbType.VarWChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "ContractNo", DataRowVersion.Current, false, (object) null));
    this._adapter.InsertCommand.Parameters.Add(new OleDbParameter("CustomerType", OleDbType.VarWChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "CustomerType", DataRowVersion.Current, false, (object) null));
    this._adapter.InsertCommand.Parameters.Add(new OleDbParameter("EquipType", OleDbType.Integer, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "EquipType", DataRowVersion.Current, false, (object) null));
    this._adapter.InsertCommand.Parameters.Add(new OleDbParameter("CoffeePreference", OleDbType.Integer, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "CoffeePreference", DataRowVersion.Current, false, (object) null));
    this._adapter.InsertCommand.Parameters.Add(new OleDbParameter("PriPrefQty", OleDbType.Single, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "PriPrefQty", DataRowVersion.Current, false, (object) null));
    this._adapter.InsertCommand.Parameters.Add(new OleDbParameter("SecondaryPreference", OleDbType.Integer, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "SecondaryPreference", DataRowVersion.Current, false, (object) null));
    this._adapter.InsertCommand.Parameters.Add(new OleDbParameter("SecPrefQty", OleDbType.Single, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "SecPrefQty", DataRowVersion.Current, false, (object) null));
    this._adapter.InsertCommand.Parameters.Add(new OleDbParameter("TypicallySecToo", OleDbType.Boolean, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "TypicallySecToo", DataRowVersion.Current, false, (object) null));
    this._adapter.InsertCommand.Parameters.Add(new OleDbParameter("PreferedAgent", OleDbType.Integer, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "PreferedAgent", DataRowVersion.Current, false, (object) null));
    this._adapter.InsertCommand.Parameters.Add(new OleDbParameter("MachineSN", OleDbType.VarWChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "MachineSN", DataRowVersion.Current, false, (object) null));
    this._adapter.InsertCommand.Parameters.Add(new OleDbParameter("UsesFilter", OleDbType.Boolean, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "UsesFilter", DataRowVersion.Current, false, (object) null));
    this._adapter.InsertCommand.Parameters.Add(new OleDbParameter("autofulfill", OleDbType.Boolean, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "autofulfill", DataRowVersion.Current, false, (object) null));
    this._adapter.InsertCommand.Parameters.Add(new OleDbParameter("enabled", OleDbType.Boolean, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "enabled", DataRowVersion.Current, false, (object) null));
    this._adapter.InsertCommand.Parameters.Add(new OleDbParameter("PredictionDisabled", OleDbType.Boolean, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "PredictionDisabled", DataRowVersion.Current, false, (object) null));
    this._adapter.InsertCommand.Parameters.Add(new OleDbParameter("AlwaysSendChkUp", OleDbType.Boolean, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "AlwaysSendChkUp", DataRowVersion.Current, false, (object) null));
    this._adapter.InsertCommand.Parameters.Add(new OleDbParameter("NormallyResponds", OleDbType.Boolean, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "NormallyResponds", DataRowVersion.Current, false, (object) null));
    this._adapter.InsertCommand.Parameters.Add(new OleDbParameter("Notes", OleDbType.LongVarWChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "Notes", DataRowVersion.Current, false, (object) null));
    this._adapter.UpdateCommand = new OleDbCommand();
    this._adapter.UpdateCommand.Connection = this.Connection;
    this._adapter.UpdateCommand.CommandText = "UPDATE `CustomersTbl` SET `CompanyName` = ?, `ContactTitle` = ?, `ContactFirstName` = ?, `ContactLastName` = ?, `ContactAltFirstName` = ?, `ContactAltLastName` = ?, `Department` = ?, `BillingAddress` = ?, `City` = ?, `StateOrProvince` = ?, `PostalCode` = ?, `Country/Region` = ?, `PhoneNumber` = ?, `Extension` = ?, `FaxNumber` = ?, `CellNumber` = ?, `EmailAddress` = ?, `AltEmailAddress` = ?, `ContractNo` = ?, `CustomerType` = ?, `EquipType` = ?, `CoffeePreference` = ?, `PriPrefQty` = ?, `SecondaryPreference` = ?, `SecPrefQty` = ?, `TypicallySecToo` = ?, `PreferedAgent` = ?, `MachineSN` = ?, `UsesFilter` = ?, `autofulfill` = ?, `enabled` = ?, `PredictionDisabled` = ?, `AlwaysSendChkUp` = ?, `NormallyResponds` = ?, `Notes` = ? WHERE ((`CustomerID` = ?))";
    this._adapter.UpdateCommand.CommandType = CommandType.Text;
    this._adapter.UpdateCommand.Parameters.Add(new OleDbParameter("CompanyName", OleDbType.VarWChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "CompanyName", DataRowVersion.Current, false, (object) null));
    this._adapter.UpdateCommand.Parameters.Add(new OleDbParameter("ContactTitle", OleDbType.VarWChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "ContactTitle", DataRowVersion.Current, false, (object) null));
    this._adapter.UpdateCommand.Parameters.Add(new OleDbParameter("ContactFirstName", OleDbType.VarWChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "ContactFirstName", DataRowVersion.Current, false, (object) null));
    this._adapter.UpdateCommand.Parameters.Add(new OleDbParameter("ContactLastName", OleDbType.VarWChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "ContactLastName", DataRowVersion.Current, false, (object) null));
    this._adapter.UpdateCommand.Parameters.Add(new OleDbParameter("ContactAltFirstName", OleDbType.VarWChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "ContactAltFirstName", DataRowVersion.Current, false, (object) null));
    this._adapter.UpdateCommand.Parameters.Add(new OleDbParameter("ContactAltLastName", OleDbType.VarWChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "ContactAltLastName", DataRowVersion.Current, false, (object) null));
    this._adapter.UpdateCommand.Parameters.Add(new OleDbParameter("Department", OleDbType.VarWChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "Department", DataRowVersion.Current, false, (object) null));
    this._adapter.UpdateCommand.Parameters.Add(new OleDbParameter("BillingAddress", OleDbType.VarWChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "BillingAddress", DataRowVersion.Current, false, (object) null));
    this._adapter.UpdateCommand.Parameters.Add(new OleDbParameter("City", OleDbType.Integer, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "City", DataRowVersion.Current, false, (object) null));
    this._adapter.UpdateCommand.Parameters.Add(new OleDbParameter("StateOrProvince", OleDbType.VarWChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "StateOrProvince", DataRowVersion.Current, false, (object) null));
    this._adapter.UpdateCommand.Parameters.Add(new OleDbParameter("PostalCode", OleDbType.VarWChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "PostalCode", DataRowVersion.Current, false, (object) null));
    this._adapter.UpdateCommand.Parameters.Add(new OleDbParameter("Country/Region", OleDbType.VarWChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "Country/Region", DataRowVersion.Current, false, (object) null));
    this._adapter.UpdateCommand.Parameters.Add(new OleDbParameter("PhoneNumber", OleDbType.VarWChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "PhoneNumber", DataRowVersion.Current, false, (object) null));
    this._adapter.UpdateCommand.Parameters.Add(new OleDbParameter("Extension", OleDbType.VarWChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "Extension", DataRowVersion.Current, false, (object) null));
    this._adapter.UpdateCommand.Parameters.Add(new OleDbParameter("FaxNumber", OleDbType.VarWChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "FaxNumber", DataRowVersion.Current, false, (object) null));
    this._adapter.UpdateCommand.Parameters.Add(new OleDbParameter("CellNumber", OleDbType.VarWChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "CellNumber", DataRowVersion.Current, false, (object) null));
    this._adapter.UpdateCommand.Parameters.Add(new OleDbParameter("EmailAddress", OleDbType.VarWChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "EmailAddress", DataRowVersion.Current, false, (object) null));
    this._adapter.UpdateCommand.Parameters.Add(new OleDbParameter("AltEmailAddress", OleDbType.VarWChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "AltEmailAddress", DataRowVersion.Current, false, (object) null));
    this._adapter.UpdateCommand.Parameters.Add(new OleDbParameter("ContractNo", OleDbType.VarWChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "ContractNo", DataRowVersion.Current, false, (object) null));
    this._adapter.UpdateCommand.Parameters.Add(new OleDbParameter("CustomerType", OleDbType.VarWChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "CustomerType", DataRowVersion.Current, false, (object) null));
    this._adapter.UpdateCommand.Parameters.Add(new OleDbParameter("EquipType", OleDbType.Integer, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "EquipType", DataRowVersion.Current, false, (object) null));
    this._adapter.UpdateCommand.Parameters.Add(new OleDbParameter("CoffeePreference", OleDbType.Integer, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "CoffeePreference", DataRowVersion.Current, false, (object) null));
    this._adapter.UpdateCommand.Parameters.Add(new OleDbParameter("PriPrefQty", OleDbType.Single, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "PriPrefQty", DataRowVersion.Current, false, (object) null));
    this._adapter.UpdateCommand.Parameters.Add(new OleDbParameter("SecondaryPreference", OleDbType.Integer, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "SecondaryPreference", DataRowVersion.Current, false, (object) null));
    this._adapter.UpdateCommand.Parameters.Add(new OleDbParameter("SecPrefQty", OleDbType.Single, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "SecPrefQty", DataRowVersion.Current, false, (object) null));
    this._adapter.UpdateCommand.Parameters.Add(new OleDbParameter("TypicallySecToo", OleDbType.Boolean, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "TypicallySecToo", DataRowVersion.Current, false, (object) null));
    this._adapter.UpdateCommand.Parameters.Add(new OleDbParameter("PreferedAgent", OleDbType.Integer, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "PreferedAgent", DataRowVersion.Current, false, (object) null));
    this._adapter.UpdateCommand.Parameters.Add(new OleDbParameter("MachineSN", OleDbType.VarWChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "MachineSN", DataRowVersion.Current, false, (object) null));
    this._adapter.UpdateCommand.Parameters.Add(new OleDbParameter("UsesFilter", OleDbType.Boolean, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "UsesFilter", DataRowVersion.Current, false, (object) null));
    this._adapter.UpdateCommand.Parameters.Add(new OleDbParameter("autofulfill", OleDbType.Boolean, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "autofulfill", DataRowVersion.Current, false, (object) null));
    this._adapter.UpdateCommand.Parameters.Add(new OleDbParameter("enabled", OleDbType.Boolean, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "enabled", DataRowVersion.Current, false, (object) null));
    this._adapter.UpdateCommand.Parameters.Add(new OleDbParameter("PredictionDisabled", OleDbType.Boolean, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "PredictionDisabled", DataRowVersion.Current, false, (object) null));
    this._adapter.UpdateCommand.Parameters.Add(new OleDbParameter("AlwaysSendChkUp", OleDbType.Boolean, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "AlwaysSendChkUp", DataRowVersion.Current, false, (object) null));
    this._adapter.UpdateCommand.Parameters.Add(new OleDbParameter("NormallyResponds", OleDbType.Boolean, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "NormallyResponds", DataRowVersion.Current, false, (object) null));
    this._adapter.UpdateCommand.Parameters.Add(new OleDbParameter("Notes", OleDbType.LongVarWChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "Notes", DataRowVersion.Current, false, (object) null));
    this._adapter.UpdateCommand.Parameters.Add(new OleDbParameter("Original_CustomerID", OleDbType.Integer, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "CustomerID", DataRowVersion.Original, false, (object) null));
  }

  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  [DebuggerNonUserCode]
  private void InitConnection()
  {
    this._connection = new OleDbConnection();
    this._connection.ConnectionString = ConfigurationManager.ConnectionStrings["Tracker08ConnectionString"].ConnectionString;
  }

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  private void InitCommandCollection()
  {
    this._commandCollection = new OleDbCommand[1];
    this._commandCollection[0] = new OleDbCommand();
    this._commandCollection[0].Connection = this.Connection;
    this._commandCollection[0].CommandText = "SELECT CustomerID, CompanyName, ContactTitle, ContactFirstName, ContactLastName, ContactAltFirstName, ContactAltLastName, Department, BillingAddress, City, StateOrProvince, PostalCode, [Country/Region], PhoneNumber, Extension, FaxNumber, CellNumber, EmailAddress, AltEmailAddress, ContractNo, CustomerType, EquipType, CoffeePreference, PriPrefQty, SecondaryPreference, SecPrefQty, TypicallySecToo, PreferedAgent, MachineSN, UsesFilter, autofulfill, enabled, PredictionDisabled, AlwaysSendChkUp, NormallyResponds, Notes FROM CustomersTbl";
    this._commandCollection[0].CommandType = CommandType.Text;
  }

  [DataObjectMethod(DataObjectMethodType.Fill, true)]
  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  [HelpKeyword("vs.data.TableAdapter")]
  public virtual int Fill(TrackerDataSet.CustomersTblDataTable dataTable)
  {
    this.Adapter.SelectCommand = this.CommandCollection[0];
    if (this.ClearBeforeFill)
      dataTable.Clear();
    return this.Adapter.Fill((DataTable) dataTable);
  }

  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  [DebuggerNonUserCode]
  [HelpKeyword("vs.data.TableAdapter")]
  [DataObjectMethod(DataObjectMethodType.Select, true)]
  public virtual TrackerDataSet.CustomersTblDataTable GetData()
  {
    this.Adapter.SelectCommand = this.CommandCollection[0];
    TrackerDataSet.CustomersTblDataTable data = new TrackerDataSet.CustomersTblDataTable();
    this.Adapter.Fill((DataTable) data);
    return data;
  }

  [DebuggerNonUserCode]
  [HelpKeyword("vs.data.TableAdapter")]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  public virtual int Update(TrackerDataSet.CustomersTblDataTable dataTable)
  {
    return this.Adapter.Update((DataTable) dataTable);
  }

  [HelpKeyword("vs.data.TableAdapter")]
  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  public virtual int Update(TrackerDataSet dataSet)
  {
    return this.Adapter.Update((DataSet) dataSet, "CustomersTbl");
  }

  [HelpKeyword("vs.data.TableAdapter")]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  [DebuggerNonUserCode]
  public virtual int Update(DataRow dataRow)
  {
    return this.Adapter.Update(new DataRow[1]{ dataRow });
  }

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  [HelpKeyword("vs.data.TableAdapter")]
  public virtual int Update(DataRow[] dataRows) => this.Adapter.Update(dataRows);

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  [HelpKeyword("vs.data.TableAdapter")]
  [DataObjectMethod(DataObjectMethodType.Delete, true)]
  public virtual int Delete(int Original_CustomerID)
  {
    this.Adapter.DeleteCommand.Parameters[0].Value = (object) Original_CustomerID;
    ConnectionState state = this.Adapter.DeleteCommand.Connection.State;
    if ((this.Adapter.DeleteCommand.Connection.State & ConnectionState.Open) != ConnectionState.Open)
      this.Adapter.DeleteCommand.Connection.Open();
    try
    {
      return this.Adapter.DeleteCommand.ExecuteNonQuery();
    }
    finally
    {
      if (state == ConnectionState.Closed)
        this.Adapter.DeleteCommand.Connection.Close();
    }
  }

  [DataObjectMethod(DataObjectMethodType.Insert, true)]
  [HelpKeyword("vs.data.TableAdapter")]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  [DebuggerNonUserCode]
  public virtual int Insert(
    string CompanyName,
    string ContactTitle,
    string ContactFirstName,
    string ContactLastName,
    string ContactAltFirstName,
    string ContactAltLastName,
    string Department,
    string BillingAddress,
    int? City,
    string StateOrProvince,
    string PostalCode,
    string _Country_Region,
    string PhoneNumber,
    string Extension,
    string FaxNumber,
    string CellNumber,
    string EmailAddress,
    string AltEmailAddress,
    string ContractNo,
    string CustomerType,
    int? EquipType,
    int? CoffeePreference,
    float? PriPrefQty,
    int? SecondaryPreference,
    float? SecPrefQty,
    bool TypicallySecToo,
    int? PreferedAgent,
    string MachineSN,
    bool UsesFilter,
    bool autofulfill,
    bool enabled,
    bool PredictionDisabled,
    bool AlwaysSendChkUp,
    bool NormallyResponds,
    string Notes)
  {
    this.Adapter.InsertCommand.Parameters[0].Value = CompanyName != null ? (object) CompanyName : throw new ArgumentNullException(nameof (CompanyName));
    if (ContactTitle == null)
      this.Adapter.InsertCommand.Parameters[1].Value = (object) DBNull.Value;
    else
      this.Adapter.InsertCommand.Parameters[1].Value = (object) ContactTitle;
    if (ContactFirstName == null)
      this.Adapter.InsertCommand.Parameters[2].Value = (object) DBNull.Value;
    else
      this.Adapter.InsertCommand.Parameters[2].Value = (object) ContactFirstName;
    if (ContactLastName == null)
      this.Adapter.InsertCommand.Parameters[3].Value = (object) DBNull.Value;
    else
      this.Adapter.InsertCommand.Parameters[3].Value = (object) ContactLastName;
    if (ContactAltFirstName == null)
      this.Adapter.InsertCommand.Parameters[4].Value = (object) DBNull.Value;
    else
      this.Adapter.InsertCommand.Parameters[4].Value = (object) ContactAltFirstName;
    if (ContactAltLastName == null)
      this.Adapter.InsertCommand.Parameters[5].Value = (object) DBNull.Value;
    else
      this.Adapter.InsertCommand.Parameters[5].Value = (object) ContactAltLastName;
    if (Department == null)
      this.Adapter.InsertCommand.Parameters[6].Value = (object) DBNull.Value;
    else
      this.Adapter.InsertCommand.Parameters[6].Value = (object) Department;
    if (BillingAddress == null)
      this.Adapter.InsertCommand.Parameters[7].Value = (object) DBNull.Value;
    else
      this.Adapter.InsertCommand.Parameters[7].Value = (object) BillingAddress;
    if (City.HasValue)
      this.Adapter.InsertCommand.Parameters[8].Value = (object) City.Value;
    else
      this.Adapter.InsertCommand.Parameters[8].Value = (object) DBNull.Value;
    if (StateOrProvince == null)
      this.Adapter.InsertCommand.Parameters[9].Value = (object) DBNull.Value;
    else
      this.Adapter.InsertCommand.Parameters[9].Value = (object) StateOrProvince;
    if (PostalCode == null)
      this.Adapter.InsertCommand.Parameters[10].Value = (object) DBNull.Value;
    else
      this.Adapter.InsertCommand.Parameters[10].Value = (object) PostalCode;
    if (_Country_Region == null)
      this.Adapter.InsertCommand.Parameters[11].Value = (object) DBNull.Value;
    else
      this.Adapter.InsertCommand.Parameters[11].Value = (object) _Country_Region;
    if (PhoneNumber == null)
      this.Adapter.InsertCommand.Parameters[12].Value = (object) DBNull.Value;
    else
      this.Adapter.InsertCommand.Parameters[12].Value = (object) PhoneNumber;
    if (Extension == null)
      this.Adapter.InsertCommand.Parameters[13].Value = (object) DBNull.Value;
    else
      this.Adapter.InsertCommand.Parameters[13].Value = (object) Extension;
    if (FaxNumber == null)
      this.Adapter.InsertCommand.Parameters[14].Value = (object) DBNull.Value;
    else
      this.Adapter.InsertCommand.Parameters[14].Value = (object) FaxNumber;
    if (CellNumber == null)
      this.Adapter.InsertCommand.Parameters[15].Value = (object) DBNull.Value;
    else
      this.Adapter.InsertCommand.Parameters[15].Value = (object) CellNumber;
    if (EmailAddress == null)
      this.Adapter.InsertCommand.Parameters[16 /*0x10*/].Value = (object) DBNull.Value;
    else
      this.Adapter.InsertCommand.Parameters[16 /*0x10*/].Value = (object) EmailAddress;
    if (AltEmailAddress == null)
      this.Adapter.InsertCommand.Parameters[17].Value = (object) DBNull.Value;
    else
      this.Adapter.InsertCommand.Parameters[17].Value = (object) AltEmailAddress;
    if (ContractNo == null)
      this.Adapter.InsertCommand.Parameters[18].Value = (object) DBNull.Value;
    else
      this.Adapter.InsertCommand.Parameters[18].Value = (object) ContractNo;
    this.Adapter.InsertCommand.Parameters[19].Value = CustomerType != null ? (object) CustomerType : throw new ArgumentNullException(nameof (CustomerType));
    if (EquipType.HasValue)
      this.Adapter.InsertCommand.Parameters[20].Value = (object) EquipType.Value;
    else
      this.Adapter.InsertCommand.Parameters[20].Value = (object) DBNull.Value;
    if (CoffeePreference.HasValue)
      this.Adapter.InsertCommand.Parameters[21].Value = (object) CoffeePreference.Value;
    else
      this.Adapter.InsertCommand.Parameters[21].Value = (object) DBNull.Value;
    if (PriPrefQty.HasValue)
      this.Adapter.InsertCommand.Parameters[22].Value = (object) PriPrefQty.Value;
    else
      this.Adapter.InsertCommand.Parameters[22].Value = (object) DBNull.Value;
    if (SecondaryPreference.HasValue)
      this.Adapter.InsertCommand.Parameters[23].Value = (object) SecondaryPreference.Value;
    else
      this.Adapter.InsertCommand.Parameters[23].Value = (object) DBNull.Value;
    if (SecPrefQty.HasValue)
      this.Adapter.InsertCommand.Parameters[24].Value = (object) SecPrefQty.Value;
    else
      this.Adapter.InsertCommand.Parameters[24].Value = (object) DBNull.Value;
    this.Adapter.InsertCommand.Parameters[25].Value = (object) TypicallySecToo;
    if (PreferedAgent.HasValue)
      this.Adapter.InsertCommand.Parameters[26].Value = (object) PreferedAgent.Value;
    else
      this.Adapter.InsertCommand.Parameters[26].Value = (object) DBNull.Value;
    if (MachineSN == null)
      this.Adapter.InsertCommand.Parameters[27].Value = (object) DBNull.Value;
    else
      this.Adapter.InsertCommand.Parameters[27].Value = (object) MachineSN;
    this.Adapter.InsertCommand.Parameters[28].Value = (object) UsesFilter;
    this.Adapter.InsertCommand.Parameters[29].Value = (object) autofulfill;
    this.Adapter.InsertCommand.Parameters[30].Value = (object) enabled;
    this.Adapter.InsertCommand.Parameters[31 /*0x1F*/].Value = (object) PredictionDisabled;
    this.Adapter.InsertCommand.Parameters[32 /*0x20*/].Value = (object) AlwaysSendChkUp;
    this.Adapter.InsertCommand.Parameters[33].Value = (object) NormallyResponds;
    if (Notes == null)
      this.Adapter.InsertCommand.Parameters[34].Value = (object) DBNull.Value;
    else
      this.Adapter.InsertCommand.Parameters[34].Value = (object) Notes;
    ConnectionState state = this.Adapter.InsertCommand.Connection.State;
    if ((this.Adapter.InsertCommand.Connection.State & ConnectionState.Open) != ConnectionState.Open)
      this.Adapter.InsertCommand.Connection.Open();
    try
    {
      return this.Adapter.InsertCommand.ExecuteNonQuery();
    }
    finally
    {
      if (state == ConnectionState.Closed)
        this.Adapter.InsertCommand.Connection.Close();
    }
  }

  [HelpKeyword("vs.data.TableAdapter")]
  [DataObjectMethod(DataObjectMethodType.Update, true)]
  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  public virtual int Update(
    string CompanyName,
    string ContactTitle,
    string ContactFirstName,
    string ContactLastName,
    string ContactAltFirstName,
    string ContactAltLastName,
    string Department,
    string BillingAddress,
    int? City,
    string StateOrProvince,
    string PostalCode,
    string _Country_Region,
    string PhoneNumber,
    string Extension,
    string FaxNumber,
    string CellNumber,
    string EmailAddress,
    string AltEmailAddress,
    string ContractNo,
    string CustomerType,
    int? EquipType,
    int? CoffeePreference,
    float? PriPrefQty,
    int? SecondaryPreference,
    float? SecPrefQty,
    bool TypicallySecToo,
    int? PreferedAgent,
    string MachineSN,
    bool UsesFilter,
    bool autofulfill,
    bool enabled,
    bool PredictionDisabled,
    bool AlwaysSendChkUp,
    bool NormallyResponds,
    string Notes,
    int Original_CustomerID)
  {
    this.Adapter.UpdateCommand.Parameters[0].Value = CompanyName != null ? (object) CompanyName : throw new ArgumentNullException(nameof (CompanyName));
    if (ContactTitle == null)
      this.Adapter.UpdateCommand.Parameters[1].Value = (object) DBNull.Value;
    else
      this.Adapter.UpdateCommand.Parameters[1].Value = (object) ContactTitle;
    if (ContactFirstName == null)
      this.Adapter.UpdateCommand.Parameters[2].Value = (object) DBNull.Value;
    else
      this.Adapter.UpdateCommand.Parameters[2].Value = (object) ContactFirstName;
    if (ContactLastName == null)
      this.Adapter.UpdateCommand.Parameters[3].Value = (object) DBNull.Value;
    else
      this.Adapter.UpdateCommand.Parameters[3].Value = (object) ContactLastName;
    if (ContactAltFirstName == null)
      this.Adapter.UpdateCommand.Parameters[4].Value = (object) DBNull.Value;
    else
      this.Adapter.UpdateCommand.Parameters[4].Value = (object) ContactAltFirstName;
    if (ContactAltLastName == null)
      this.Adapter.UpdateCommand.Parameters[5].Value = (object) DBNull.Value;
    else
      this.Adapter.UpdateCommand.Parameters[5].Value = (object) ContactAltLastName;
    if (Department == null)
      this.Adapter.UpdateCommand.Parameters[6].Value = (object) DBNull.Value;
    else
      this.Adapter.UpdateCommand.Parameters[6].Value = (object) Department;
    if (BillingAddress == null)
      this.Adapter.UpdateCommand.Parameters[7].Value = (object) DBNull.Value;
    else
      this.Adapter.UpdateCommand.Parameters[7].Value = (object) BillingAddress;
    if (City.HasValue)
      this.Adapter.UpdateCommand.Parameters[8].Value = (object) City.Value;
    else
      this.Adapter.UpdateCommand.Parameters[8].Value = (object) DBNull.Value;
    if (StateOrProvince == null)
      this.Adapter.UpdateCommand.Parameters[9].Value = (object) DBNull.Value;
    else
      this.Adapter.UpdateCommand.Parameters[9].Value = (object) StateOrProvince;
    if (PostalCode == null)
      this.Adapter.UpdateCommand.Parameters[10].Value = (object) DBNull.Value;
    else
      this.Adapter.UpdateCommand.Parameters[10].Value = (object) PostalCode;
    if (_Country_Region == null)
      this.Adapter.UpdateCommand.Parameters[11].Value = (object) DBNull.Value;
    else
      this.Adapter.UpdateCommand.Parameters[11].Value = (object) _Country_Region;
    if (PhoneNumber == null)
      this.Adapter.UpdateCommand.Parameters[12].Value = (object) DBNull.Value;
    else
      this.Adapter.UpdateCommand.Parameters[12].Value = (object) PhoneNumber;
    if (Extension == null)
      this.Adapter.UpdateCommand.Parameters[13].Value = (object) DBNull.Value;
    else
      this.Adapter.UpdateCommand.Parameters[13].Value = (object) Extension;
    if (FaxNumber == null)
      this.Adapter.UpdateCommand.Parameters[14].Value = (object) DBNull.Value;
    else
      this.Adapter.UpdateCommand.Parameters[14].Value = (object) FaxNumber;
    if (CellNumber == null)
      this.Adapter.UpdateCommand.Parameters[15].Value = (object) DBNull.Value;
    else
      this.Adapter.UpdateCommand.Parameters[15].Value = (object) CellNumber;
    if (EmailAddress == null)
      this.Adapter.UpdateCommand.Parameters[16 /*0x10*/].Value = (object) DBNull.Value;
    else
      this.Adapter.UpdateCommand.Parameters[16 /*0x10*/].Value = (object) EmailAddress;
    if (AltEmailAddress == null)
      this.Adapter.UpdateCommand.Parameters[17].Value = (object) DBNull.Value;
    else
      this.Adapter.UpdateCommand.Parameters[17].Value = (object) AltEmailAddress;
    if (ContractNo == null)
      this.Adapter.UpdateCommand.Parameters[18].Value = (object) DBNull.Value;
    else
      this.Adapter.UpdateCommand.Parameters[18].Value = (object) ContractNo;
    this.Adapter.UpdateCommand.Parameters[19].Value = CustomerType != null ? (object) CustomerType : throw new ArgumentNullException(nameof (CustomerType));
    if (EquipType.HasValue)
      this.Adapter.UpdateCommand.Parameters[20].Value = (object) EquipType.Value;
    else
      this.Adapter.UpdateCommand.Parameters[20].Value = (object) DBNull.Value;
    if (CoffeePreference.HasValue)
      this.Adapter.UpdateCommand.Parameters[21].Value = (object) CoffeePreference.Value;
    else
      this.Adapter.UpdateCommand.Parameters[21].Value = (object) DBNull.Value;
    if (PriPrefQty.HasValue)
      this.Adapter.UpdateCommand.Parameters[22].Value = (object) PriPrefQty.Value;
    else
      this.Adapter.UpdateCommand.Parameters[22].Value = (object) DBNull.Value;
    if (SecondaryPreference.HasValue)
      this.Adapter.UpdateCommand.Parameters[23].Value = (object) SecondaryPreference.Value;
    else
      this.Adapter.UpdateCommand.Parameters[23].Value = (object) DBNull.Value;
    if (SecPrefQty.HasValue)
      this.Adapter.UpdateCommand.Parameters[24].Value = (object) SecPrefQty.Value;
    else
      this.Adapter.UpdateCommand.Parameters[24].Value = (object) DBNull.Value;
    this.Adapter.UpdateCommand.Parameters[25].Value = (object) TypicallySecToo;
    if (PreferedAgent.HasValue)
      this.Adapter.UpdateCommand.Parameters[26].Value = (object) PreferedAgent.Value;
    else
      this.Adapter.UpdateCommand.Parameters[26].Value = (object) DBNull.Value;
    if (MachineSN == null)
      this.Adapter.UpdateCommand.Parameters[27].Value = (object) DBNull.Value;
    else
      this.Adapter.UpdateCommand.Parameters[27].Value = (object) MachineSN;
    this.Adapter.UpdateCommand.Parameters[28].Value = (object) UsesFilter;
    this.Adapter.UpdateCommand.Parameters[29].Value = (object) autofulfill;
    this.Adapter.UpdateCommand.Parameters[30].Value = (object) enabled;
    this.Adapter.UpdateCommand.Parameters[31 /*0x1F*/].Value = (object) PredictionDisabled;
    this.Adapter.UpdateCommand.Parameters[32 /*0x20*/].Value = (object) AlwaysSendChkUp;
    this.Adapter.UpdateCommand.Parameters[33].Value = (object) NormallyResponds;
    if (Notes == null)
      this.Adapter.UpdateCommand.Parameters[34].Value = (object) DBNull.Value;
    else
      this.Adapter.UpdateCommand.Parameters[34].Value = (object) Notes;
    this.Adapter.UpdateCommand.Parameters[35].Value = (object) Original_CustomerID;
    ConnectionState state = this.Adapter.UpdateCommand.Connection.State;
    if ((this.Adapter.UpdateCommand.Connection.State & ConnectionState.Open) != ConnectionState.Open)
      this.Adapter.UpdateCommand.Connection.Open();
    try
    {
      return this.Adapter.UpdateCommand.ExecuteNonQuery();
    }
    finally
    {
      if (state == ConnectionState.Closed)
        this.Adapter.UpdateCommand.Connection.Close();
    }
  }
}
