// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.DataSets.TrackerDataSetTableAdapters.ItemTypeTblTableAdapter
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

[HelpKeyword("vs.data.TableAdapter")]
[Designer("Microsoft.VSDesigner.DataSource.Design.TableAdapterDesigner, Microsoft.VSDesigner, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
[DesignerCategory("code")]
[ToolboxItem(true)]
[DataObject(true)]
public class ItemTypeTblTableAdapter : Component
{
  private OleDbDataAdapter _adapter;
  private OleDbConnection _connection;
  private OleDbTransaction _transaction;
  private OleDbCommand[] _commandCollection;
  private bool _clearBeforeFill;

  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  [DebuggerNonUserCode]
  public ItemTypeTblTableAdapter() => this.ClearBeforeFill = true;

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

  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  [DebuggerNonUserCode]
  public bool ClearBeforeFill
  {
    get => this._clearBeforeFill;
    set => this._clearBeforeFill = value;
  }

  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  [DebuggerNonUserCode]
  private void InitAdapter()
  {
    this._adapter = new OleDbDataAdapter();
    this._adapter.TableMappings.Add((object) new DataTableMapping()
    {
      SourceTable = "Table",
      DataSetTable = "ItemTypeTbl",
      ColumnMappings = {
        {
          "ItemTypeID",
          "ItemTypeID"
        },
        {
          "ItemDesc",
          "ItemDesc"
        },
        {
          "ItemEnabled",
          "ItemEnabled"
        },
        {
          "ItemsCharacteritics",
          "ItemsCharacteritics"
        },
        {
          "ItemDetail",
          "ItemDetail"
        },
        {
          "ServiceTypeID",
          "ServiceTypeID"
        },
        {
          "ReplacementID",
          "ReplacementID"
        },
        {
          "SortOrder",
          "SortOrder"
        }
      }
    });
    this._adapter.DeleteCommand = new OleDbCommand();
    this._adapter.DeleteCommand.Connection = this.Connection;
    this._adapter.DeleteCommand.CommandText = "DELETE FROM `ItemTypeTbl` WHERE ((`ItemTypeID` = ?))";
    this._adapter.DeleteCommand.CommandType = CommandType.Text;
    this._adapter.DeleteCommand.Parameters.Add(new OleDbParameter("Original_ItemTypeID", OleDbType.Integer, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "ItemTypeID", DataRowVersion.Original, false, (object) null));
    this._adapter.InsertCommand = new OleDbCommand();
    this._adapter.InsertCommand.Connection = this.Connection;
    this._adapter.InsertCommand.CommandText = "INSERT INTO `ItemTypeTbl` (`ItemDesc`, `ItemEnabled`, `ItemsCharacteritics`, `ItemDetail`, `ServiceTypeId`, `ReplacementID`, `SortOrder`) VALUES (?, ?, ?, ?, ?, ?, ?)";
    this._adapter.InsertCommand.CommandType = CommandType.Text;
    this._adapter.InsertCommand.Parameters.Add(new OleDbParameter("ItemDesc", OleDbType.VarWChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "ItemDesc", DataRowVersion.Current, false, (object) null));
    this._adapter.InsertCommand.Parameters.Add(new OleDbParameter("ItemEnabled", OleDbType.Boolean, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "ItemEnabled", DataRowVersion.Current, false, (object) null));
    this._adapter.InsertCommand.Parameters.Add(new OleDbParameter("ItemsCharacteritics", OleDbType.VarWChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "ItemsCharacteritics", DataRowVersion.Current, false, (object) null));
    this._adapter.InsertCommand.Parameters.Add(new OleDbParameter("ItemDetail", OleDbType.LongVarWChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "ItemDetail", DataRowVersion.Current, false, (object) null));
    this._adapter.InsertCommand.Parameters.Add(new OleDbParameter("ServiceTypeID", OleDbType.Integer, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "ServiceTypeID", DataRowVersion.Current, false, (object) null));
    this._adapter.InsertCommand.Parameters.Add(new OleDbParameter("ReplacementID", OleDbType.Integer, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "ReplacementID", DataRowVersion.Current, false, (object) null));
    this._adapter.InsertCommand.Parameters.Add(new OleDbParameter("SortOrder", OleDbType.Integer, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "SortOrder", DataRowVersion.Current, false, (object) null));
    this._adapter.UpdateCommand = new OleDbCommand();
    this._adapter.UpdateCommand.Connection = this.Connection;
    this._adapter.UpdateCommand.CommandText = "UPDATE `ItemTypeTbl` SET `ItemDesc` = ?, `ItemEnabled` = ?, `ItemsCharacteritics` = ?, `ItemDetail` = ?, `ServiceTypeID` = ?, `ReplacementID` = ?, `SortOrder` = ? WHERE ((`ItemTypeID` = ?))";
    this._adapter.UpdateCommand.CommandType = CommandType.Text;
    this._adapter.UpdateCommand.Parameters.Add(new OleDbParameter("ItemDesc", OleDbType.VarWChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "ItemDesc", DataRowVersion.Current, false, (object) null));
    this._adapter.UpdateCommand.Parameters.Add(new OleDbParameter("ItemEnabled", OleDbType.Boolean, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "ItemEnabled", DataRowVersion.Current, false, (object) null));
    this._adapter.UpdateCommand.Parameters.Add(new OleDbParameter("ItemsCharacteritics", OleDbType.VarWChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "ItemsCharacteritics", DataRowVersion.Current, false, (object) null));
    this._adapter.UpdateCommand.Parameters.Add(new OleDbParameter("ItemDetail", OleDbType.LongVarWChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "ItemDetail", DataRowVersion.Current, false, (object) null));
    this._adapter.UpdateCommand.Parameters.Add(new OleDbParameter("ServiceTypeID", OleDbType.Integer, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "ServiceTypeID", DataRowVersion.Current, false, (object) null));
    this._adapter.UpdateCommand.Parameters.Add(new OleDbParameter("ReplacementID", OleDbType.Integer, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "ReplacementID", DataRowVersion.Current, false, (object) null));
    this._adapter.UpdateCommand.Parameters.Add(new OleDbParameter("SortOrder", OleDbType.Integer, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "SortOrder", DataRowVersion.Current, false, (object) null));
    this._adapter.UpdateCommand.Parameters.Add(new OleDbParameter("Original_ItemTypeID", OleDbType.Integer, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "ItemTypeID", DataRowVersion.Original, false, (object) null));
  }

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
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
    this._commandCollection[0].CommandText = "SELECT ItemTypeID, ItemDesc, ItemEnabled, ItemsCharacteritics, ItemDetail, ServiceTypeID, ReplacementID, SortOrder FROM ItemTypeTbl";
    this._commandCollection[0].CommandType = CommandType.Text;
  }

  [DebuggerNonUserCode]
  [HelpKeyword("vs.data.TableAdapter")]
  [DataObjectMethod(DataObjectMethodType.Fill, true)]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  public virtual int Fill(TrackerDataSet.ItemTypeTblDataTable dataTable)
  {
    this.Adapter.SelectCommand = this.CommandCollection[0];
    if (this.ClearBeforeFill)
      dataTable.Clear();
    return this.Adapter.Fill((DataTable) dataTable);
  }

  [DataObjectMethod(DataObjectMethodType.Select, true)]
  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  [HelpKeyword("vs.data.TableAdapter")]
  public virtual TrackerDataSet.ItemTypeTblDataTable GetData()
  {
    this.Adapter.SelectCommand = this.CommandCollection[0];
    TrackerDataSet.ItemTypeTblDataTable data = new TrackerDataSet.ItemTypeTblDataTable();
    this.Adapter.Fill((DataTable) data);
    return data;
  }

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  [HelpKeyword("vs.data.TableAdapter")]
  public virtual int Update(TrackerDataSet.ItemTypeTblDataTable dataTable)
  {
    return this.Adapter.Update((DataTable) dataTable);
  }

  [HelpKeyword("vs.data.TableAdapter")]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  [DebuggerNonUserCode]
  public virtual int Update(TrackerDataSet dataSet)
  {
    return this.Adapter.Update((DataSet) dataSet, "ItemTypeTbl");
  }

  [HelpKeyword("vs.data.TableAdapter")]
  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  public virtual int Update(DataRow dataRow)
  {
    return this.Adapter.Update(new DataRow[1]{ dataRow });
  }

  [HelpKeyword("vs.data.TableAdapter")]
  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  public virtual int Update(DataRow[] dataRows) => this.Adapter.Update(dataRows);

  [DebuggerNonUserCode]
  [DataObjectMethod(DataObjectMethodType.Delete, true)]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  [HelpKeyword("vs.data.TableAdapter")]
  public virtual int Delete(int Original_ItemTypeID)
  {
    this.Adapter.DeleteCommand.Parameters[0].Value = (object) Original_ItemTypeID;
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

  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  [DebuggerNonUserCode]
  [DataObjectMethod(DataObjectMethodType.Insert, true)]
  [HelpKeyword("vs.data.TableAdapter")]
  public virtual int Insert(
    string ItemDesc,
    bool ItemEnabled,
    string ItemsCharacteritics,
    string ItemDetail,
    int? ServiceTypeID,
    int? ReplacementID,
    int? SortOrder)
  {
    this.Adapter.InsertCommand.Parameters[0].Value = ItemDesc != null ? (object) ItemDesc : throw new ArgumentNullException(nameof (ItemDesc));
    this.Adapter.InsertCommand.Parameters[1].Value = (object) ItemEnabled;
    if (ItemsCharacteritics == null)
      this.Adapter.InsertCommand.Parameters[2].Value = (object) DBNull.Value;
    else
      this.Adapter.InsertCommand.Parameters[2].Value = (object) ItemsCharacteritics;
    if (ItemDetail == null)
      this.Adapter.InsertCommand.Parameters[3].Value = (object) DBNull.Value;
    else
      this.Adapter.InsertCommand.Parameters[3].Value = (object) ItemDetail;
    if (ServiceTypeID.HasValue)
      this.Adapter.InsertCommand.Parameters[4].Value = (object) ServiceTypeID.Value;
    else
      this.Adapter.InsertCommand.Parameters[4].Value = (object) DBNull.Value;
    if (ReplacementID.HasValue)
      this.Adapter.InsertCommand.Parameters[5].Value = (object) ReplacementID.Value;
    else
      this.Adapter.InsertCommand.Parameters[5].Value = (object) DBNull.Value;
    if (SortOrder.HasValue)
      this.Adapter.InsertCommand.Parameters[6].Value = (object) SortOrder.Value;
    else
      this.Adapter.InsertCommand.Parameters[6].Value = (object) DBNull.Value;
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

  [DebuggerNonUserCode]
  [DataObjectMethod(DataObjectMethodType.Update, true)]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  [HelpKeyword("vs.data.TableAdapter")]
  public virtual int Update(
    string ItemDesc,
    bool ItemEnabled,
    string ItemsCharacteritics,
    string ItemDetail,
    int? ServiceTypeID,
    int? ReplacementID,
    int? SortOrder,
    int Original_ItemTypeID)
  {
    this.Adapter.UpdateCommand.Parameters[0].Value = ItemDesc != null ? (object) ItemDesc : throw new ArgumentNullException(nameof (ItemDesc));
    this.Adapter.UpdateCommand.Parameters[1].Value = (object) ItemEnabled;
    if (ItemsCharacteritics == null)
      this.Adapter.UpdateCommand.Parameters[2].Value = (object) DBNull.Value;
    else
      this.Adapter.UpdateCommand.Parameters[2].Value = (object) ItemsCharacteritics;
    if (ItemDetail == null)
      this.Adapter.UpdateCommand.Parameters[3].Value = (object) DBNull.Value;
    else
      this.Adapter.UpdateCommand.Parameters[3].Value = (object) ItemDetail;
    if (ServiceTypeID.HasValue)
      this.Adapter.UpdateCommand.Parameters[4].Value = (object) ServiceTypeID.Value;
    else
      this.Adapter.UpdateCommand.Parameters[4].Value = (object) DBNull.Value;
    if (ReplacementID.HasValue)
      this.Adapter.UpdateCommand.Parameters[5].Value = (object) ReplacementID.Value;
    else
      this.Adapter.UpdateCommand.Parameters[5].Value = (object) DBNull.Value;
    if (SortOrder.HasValue)
      this.Adapter.UpdateCommand.Parameters[6].Value = (object) SortOrder.Value;
    else
      this.Adapter.UpdateCommand.Parameters[6].Value = (object) DBNull.Value;
    this.Adapter.UpdateCommand.Parameters[7].Value = (object) Original_ItemTypeID;
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
