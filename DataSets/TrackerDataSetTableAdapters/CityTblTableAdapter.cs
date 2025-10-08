// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.DataSets.TrackerDataSetTableAdapters.CityTblTableAdapter
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

[ToolboxItem(true)]
[Designer("Microsoft.VSDesigner.DataSource.Design.TableAdapterDesigner, Microsoft.VSDesigner, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
[HelpKeyword("vs.data.TableAdapter")]
[DesignerCategory("code")]
[DataObject(true)]
public class CityTblTableAdapter : Component
{
  private OleDbDataAdapter _adapter;
  private OleDbConnection _connection;
  private OleDbTransaction _transaction;
  private OleDbCommand[] _commandCollection;
  private bool _clearBeforeFill;

  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  [DebuggerNonUserCode]
  public CityTblTableAdapter() => this.ClearBeforeFill = true;

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
      DataSetTable = "CityTbl",
      ColumnMappings = {
        {
          "ID",
          "ID"
        },
        {
          "City",
          "City"
        },
        {
          "RoastingDay",
          "RoastingDay"
        },
        {
          "DeliveryDelay",
          "DeliveryDelay"
        }
      }
    });
    this._adapter.DeleteCommand = new OleDbCommand();
    this._adapter.DeleteCommand.Connection = this.Connection;
    this._adapter.DeleteCommand.CommandText = "DELETE FROM `CityTbl` WHERE ((`ID` = ?))";
    this._adapter.DeleteCommand.CommandType = CommandType.Text;
    this._adapter.DeleteCommand.Parameters.Add(new OleDbParameter("Original_ID", OleDbType.Integer, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "ID", DataRowVersion.Original, false, (object) null));
    this._adapter.InsertCommand = new OleDbCommand();
    this._adapter.InsertCommand.Connection = this.Connection;
    this._adapter.InsertCommand.CommandText = "INSERT INTO `CityTbl` (`City`, `RoastingDay`, `DeliveryDelay`) VALUES (?, ?, ?)";
    this._adapter.InsertCommand.CommandType = CommandType.Text;
    this._adapter.InsertCommand.Parameters.Add(new OleDbParameter("City", OleDbType.VarWChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "City", DataRowVersion.Current, false, (object) null));
    this._adapter.InsertCommand.Parameters.Add(new OleDbParameter("RoastingDay", OleDbType.Integer, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "RoastingDay", DataRowVersion.Current, false, (object) null));
    this._adapter.InsertCommand.Parameters.Add(new OleDbParameter("DeliveryDelay", OleDbType.Integer, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "DeliveryDelay", DataRowVersion.Current, false, (object) null));
    this._adapter.UpdateCommand = new OleDbCommand();
    this._adapter.UpdateCommand.Connection = this.Connection;
    this._adapter.UpdateCommand.CommandText = "UPDATE `CityTbl` SET `City` = ?, `RoastingDay` = ?, `DeliveryDelay` = ? WHERE ((`ID` = ?))";
    this._adapter.UpdateCommand.CommandType = CommandType.Text;
    this._adapter.UpdateCommand.Parameters.Add(new OleDbParameter("City", OleDbType.VarWChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "City", DataRowVersion.Current, false, (object) null));
    this._adapter.UpdateCommand.Parameters.Add(new OleDbParameter("RoastingDay", OleDbType.Integer, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "RoastingDay", DataRowVersion.Current, false, (object) null));
    this._adapter.UpdateCommand.Parameters.Add(new OleDbParameter("DeliveryDelay", OleDbType.Integer, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "DeliveryDelay", DataRowVersion.Current, false, (object) null));
    this._adapter.UpdateCommand.Parameters.Add(new OleDbParameter("Original_ID", OleDbType.Integer, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "ID", DataRowVersion.Original, false, (object) null));
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
    this._commandCollection = new OleDbCommand[1];
    this._commandCollection[0] = new OleDbCommand();
    this._commandCollection[0].Connection = this.Connection;
    this._commandCollection[0].CommandText = "SELECT ID, City, RoastingDay, DeliveryDelay FROM CityTbl";
    this._commandCollection[0].CommandType = CommandType.Text;
  }

  [HelpKeyword("vs.data.TableAdapter")]
  [DataObjectMethod(DataObjectMethodType.Fill, true)]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  [DebuggerNonUserCode]
  public virtual int Fill(TrackerDataSet.CityTblDataTable dataTable)
  {
    this.Adapter.SelectCommand = this.CommandCollection[0];
    if (this.ClearBeforeFill)
      dataTable.Clear();
    return this.Adapter.Fill((DataTable) dataTable);
  }

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  [HelpKeyword("vs.data.TableAdapter")]
  [DataObjectMethod(DataObjectMethodType.Select, true)]
  public virtual TrackerDataSet.CityTblDataTable GetData()
  {
    this.Adapter.SelectCommand = this.CommandCollection[0];
    TrackerDataSet.CityTblDataTable data = new TrackerDataSet.CityTblDataTable();
    this.Adapter.Fill((DataTable) data);
    return data;
  }

  [HelpKeyword("vs.data.TableAdapter")]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  [DebuggerNonUserCode]
  public virtual int Update(TrackerDataSet.CityTblDataTable dataTable)
  {
    return this.Adapter.Update((DataTable) dataTable);
  }

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  [HelpKeyword("vs.data.TableAdapter")]
  public virtual int Update(TrackerDataSet dataSet)
  {
    return this.Adapter.Update((DataSet) dataSet, "CityTbl");
  }

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  [HelpKeyword("vs.data.TableAdapter")]
  public virtual int Update(DataRow dataRow)
  {
    return this.Adapter.Update(new DataRow[1]{ dataRow });
  }

  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  [HelpKeyword("vs.data.TableAdapter")]
  [DebuggerNonUserCode]
  public virtual int Update(DataRow[] dataRows) => this.Adapter.Update(dataRows);

  [HelpKeyword("vs.data.TableAdapter")]
  [DebuggerNonUserCode]
  [DataObjectMethod(DataObjectMethodType.Delete, true)]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  public virtual int Delete(int Original_ID)
  {
    this.Adapter.DeleteCommand.Parameters[0].Value = (object) Original_ID;
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

  [HelpKeyword("vs.data.TableAdapter")]
  [DataObjectMethod(DataObjectMethodType.Insert, true)]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  [DebuggerNonUserCode]
  public virtual int Insert(string City, int? RoastingDay, int? DeliveryDelay)
  {
    if (City == null)
      this.Adapter.InsertCommand.Parameters[0].Value = (object) DBNull.Value;
    else
      this.Adapter.InsertCommand.Parameters[0].Value = (object) City;
    if (RoastingDay.HasValue)
      this.Adapter.InsertCommand.Parameters[1].Value = (object) RoastingDay.Value;
    else
      this.Adapter.InsertCommand.Parameters[1].Value = (object) DBNull.Value;
    if (DeliveryDelay.HasValue)
      this.Adapter.InsertCommand.Parameters[2].Value = (object) DeliveryDelay.Value;
    else
      this.Adapter.InsertCommand.Parameters[2].Value = (object) DBNull.Value;
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

  [DataObjectMethod(DataObjectMethodType.Update, true)]
  [HelpKeyword("vs.data.TableAdapter")]
  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  public virtual int Update(string City, int? RoastingDay, int? DeliveryDelay, int Original_ID)
  {
    if (City == null)
      this.Adapter.UpdateCommand.Parameters[0].Value = (object) DBNull.Value;
    else
      this.Adapter.UpdateCommand.Parameters[0].Value = (object) City;
    if (RoastingDay.HasValue)
      this.Adapter.UpdateCommand.Parameters[1].Value = (object) RoastingDay.Value;
    else
      this.Adapter.UpdateCommand.Parameters[1].Value = (object) DBNull.Value;
    if (DeliveryDelay.HasValue)
      this.Adapter.UpdateCommand.Parameters[2].Value = (object) DeliveryDelay.Value;
    else
      this.Adapter.UpdateCommand.Parameters[2].Value = (object) DBNull.Value;
    this.Adapter.UpdateCommand.Parameters[3].Value = (object) Original_ID;
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
