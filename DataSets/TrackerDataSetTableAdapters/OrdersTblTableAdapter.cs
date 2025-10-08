// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.DataSets.TrackerDataSetTableAdapters.OrdersTblTableAdapter
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
[HelpKeyword("vs.data.TableAdapter")]
[DesignerCategory("code")]
[DataObject(true)]
[Designer("Microsoft.VSDesigner.DataSource.Design.TableAdapterDesigner, Microsoft.VSDesigner, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
public class OrdersTblTableAdapter : Component
{
  private OleDbDataAdapter _adapter;
  private OleDbConnection _connection;
  private OleDbTransaction _transaction;
  private OleDbCommand[] _commandCollection;
  private bool _clearBeforeFill;

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  public OrdersTblTableAdapter() => this.ClearBeforeFill = true;

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

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
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
      DataSetTable = "OrdersTbl",
      ColumnMappings = {
        {
          "OrderID",
          "OrderID"
        },
        {
          "CustomerId",
          "CustomerId"
        },
        {
          "OrderDate",
          "OrderDate"
        },
        {
          "RoastDate",
          "RoastDate"
        },
        {
          "ItemTypeID",
          "ItemTypeID"
        },
        {
          "QuantityOrdered",
          "QuantityOrdered"
        },
        {
          "RequiredByDate",
          "RequiredByDate"
        },
        {
          "ToBeDeliveredBy",
          "ToBeDeliveredBy"
        },
        {
          "Confirmed",
          "Confirmed"
        },
        {
          "Done",
          "Done"
        },
        {
          "Notes",
          "Notes"
        }
      }
    });
    this._adapter.DeleteCommand = new OleDbCommand();
    this._adapter.DeleteCommand.Connection = this.Connection;
    this._adapter.DeleteCommand.CommandText = "DELETE FROM `OrdersTbl` WHERE ((`OrderID` = ?))";
    this._adapter.DeleteCommand.CommandType = CommandType.Text;
    this._adapter.DeleteCommand.Parameters.Add(new OleDbParameter("Original_OrderID", OleDbType.Integer, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "OrderID", DataRowVersion.Original, false, (object) null));
    this._adapter.InsertCommand = new OleDbCommand();
    this._adapter.InsertCommand.Connection = this.Connection;
    this._adapter.InsertCommand.CommandText = "INSERT INTO `OrdersTbl` (`CustomerId`, `OrderDate`, `RoastDate`, `ItemTypeID`, `QuantityOrdered`, `RequiredByDate`, `ToBeDeliveredBy`, `Confirmed`, `Done`, `Notes`) VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)";
    this._adapter.InsertCommand.CommandType = CommandType.Text;
    this._adapter.InsertCommand.Parameters.Add(new OleDbParameter("CustomerId", OleDbType.Integer, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "CustomerId", DataRowVersion.Current, false, (object) null));
    this._adapter.InsertCommand.Parameters.Add(new OleDbParameter("OrderDate", OleDbType.Date, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "OrderDate", DataRowVersion.Current, false, (object) null));
    this._adapter.InsertCommand.Parameters.Add(new OleDbParameter("RoastDate", OleDbType.Date, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "RoastDate", DataRowVersion.Current, false, (object) null));
    this._adapter.InsertCommand.Parameters.Add(new OleDbParameter("ItemTypeID", OleDbType.Integer, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "ItemTypeID", DataRowVersion.Current, false, (object) null));
    this._adapter.InsertCommand.Parameters.Add(new OleDbParameter("QuantityOrdered", OleDbType.Single, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "QuantityOrdered", DataRowVersion.Current, false, (object) null));
    this._adapter.InsertCommand.Parameters.Add(new OleDbParameter("RequiredByDate", OleDbType.Date, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "RequiredByDate", DataRowVersion.Current, false, (object) null));
    this._adapter.InsertCommand.Parameters.Add(new OleDbParameter("ToBeDeliveredBy", OleDbType.Integer, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "ToBeDeliveredBy", DataRowVersion.Current, false, (object) null));
    this._adapter.InsertCommand.Parameters.Add(new OleDbParameter("Confirmed", OleDbType.Boolean, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "Confirmed", DataRowVersion.Current, false, (object) null));
    this._adapter.InsertCommand.Parameters.Add(new OleDbParameter("Done", OleDbType.Boolean, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "Done", DataRowVersion.Current, false, (object) null));
    this._adapter.InsertCommand.Parameters.Add(new OleDbParameter("Notes", OleDbType.VarWChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "Notes", DataRowVersion.Current, false, (object) null));
    this._adapter.UpdateCommand = new OleDbCommand();
    this._adapter.UpdateCommand.Connection = this.Connection;
    this._adapter.UpdateCommand.CommandText = "UPDATE `OrdersTbl` SET `CustomerId` = ?, `OrderDate` = ?, `RoastDate` = ?, `ItemTypeID` = ?, `QuantityOrdered` = ?, `RequiredByDate` = ?, `ToBeDeliveredBy` = ?, `Confirmed` = ?, `Done` = ?, `Notes` = ? WHERE ((`OrderID` = ?))";
    this._adapter.UpdateCommand.CommandType = CommandType.Text;
    this._adapter.UpdateCommand.Parameters.Add(new OleDbParameter("CustomerId", OleDbType.Integer, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "CustomerId", DataRowVersion.Current, false, (object) null));
    this._adapter.UpdateCommand.Parameters.Add(new OleDbParameter("OrderDate", OleDbType.Date, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "OrderDate", DataRowVersion.Current, false, (object) null));
    this._adapter.UpdateCommand.Parameters.Add(new OleDbParameter("RoastDate", OleDbType.Date, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "RoastDate", DataRowVersion.Current, false, (object) null));
    this._adapter.UpdateCommand.Parameters.Add(new OleDbParameter("ItemTypeID", OleDbType.Integer, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "ItemTypeID", DataRowVersion.Current, false, (object) null));
    this._adapter.UpdateCommand.Parameters.Add(new OleDbParameter("QuantityOrdered", OleDbType.Single, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "QuantityOrdered", DataRowVersion.Current, false, (object) null));
    this._adapter.UpdateCommand.Parameters.Add(new OleDbParameter("RequiredByDate", OleDbType.Date, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "RequiredByDate", DataRowVersion.Current, false, (object) null));
    this._adapter.UpdateCommand.Parameters.Add(new OleDbParameter("ToBeDeliveredBy", OleDbType.Integer, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "ToBeDeliveredBy", DataRowVersion.Current, false, (object) null));
    this._adapter.UpdateCommand.Parameters.Add(new OleDbParameter("Confirmed", OleDbType.Boolean, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "Confirmed", DataRowVersion.Current, false, (object) null));
    this._adapter.UpdateCommand.Parameters.Add(new OleDbParameter("Done", OleDbType.Boolean, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "Done", DataRowVersion.Current, false, (object) null));
    this._adapter.UpdateCommand.Parameters.Add(new OleDbParameter("Notes", OleDbType.VarWChar, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "Notes", DataRowVersion.Current, false, (object) null));
    this._adapter.UpdateCommand.Parameters.Add(new OleDbParameter("Original_OrderID", OleDbType.Integer, 0, ParameterDirection.Input, (byte) 0, (byte) 0, "OrderID", DataRowVersion.Original, false, (object) null));
  }

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
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
    this._commandCollection[0].CommandText = "SELECT OrderID, CustomerId, OrderDate, RoastDate, ItemTypeID, QuantityOrdered, RequiredByDate, ToBeDeliveredBy, Confirmed, Done, Notes FROM OrdersTbl";
    this._commandCollection[0].CommandType = CommandType.Text;
  }

  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  [HelpKeyword("vs.data.TableAdapter")]
  [DataObjectMethod(DataObjectMethodType.Fill, true)]
  [DebuggerNonUserCode]
  public virtual int Fill(TrackerDataSet.OrderTblDataTable dataTable)
  {
    this.Adapter.SelectCommand = this.CommandCollection[0];
    if (this.ClearBeforeFill)
      dataTable.Clear();
    return this.Adapter.Fill((DataTable) dataTable);
  }

  [DataObjectMethod(DataObjectMethodType.Select, true)]
  [DebuggerNonUserCode]
  [HelpKeyword("vs.data.TableAdapter")]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  public virtual TrackerDataSet.OrderTblDataTable GetData()
  {
    this.Adapter.SelectCommand = this.CommandCollection[0];
    TrackerDataSet.OrderTblDataTable data = new TrackerDataSet.OrderTblDataTable();
    this.Adapter.Fill((DataTable) data);
    return data;
  }

  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  [DebuggerNonUserCode]
  [HelpKeyword("vs.data.TableAdapter")]
  public virtual int Update(TrackerDataSet.OrderTblDataTable dataTable)
  {
    return this.Adapter.Update((DataTable) dataTable);
  }

  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  [HelpKeyword("vs.data.TableAdapter")]
  [DebuggerNonUserCode]
  public virtual int Update(TrackerDataSet dataSet)
  {
    return this.Adapter.Update((DataSet) dataSet, "OrdersTbl");
  }

  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  [HelpKeyword("vs.data.TableAdapter")]
  [DebuggerNonUserCode]
  public virtual int Update(DataRow dataRow)
  {
    return this.Adapter.Update(new DataRow[1]{ dataRow });
  }

  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  [HelpKeyword("vs.data.TableAdapter")]
  [DebuggerNonUserCode]
  public virtual int Update(DataRow[] dataRows) => this.Adapter.Update(dataRows);

  [DebuggerNonUserCode]
  [HelpKeyword("vs.data.TableAdapter")]
  [DataObjectMethod(DataObjectMethodType.Delete, true)]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  public virtual int Delete(int Original_OrderID)
  {
    this.Adapter.DeleteCommand.Parameters[0].Value = (object) Original_OrderID;
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
  [DataObjectMethod(DataObjectMethodType.Insert, true)]
  [DebuggerNonUserCode]
  [HelpKeyword("vs.data.TableAdapter")]
  public virtual int Insert(
    int? CustomerId,
    DateTime? OrderDate,
    DateTime? RoastDate,
    int? ItemTypeID,
    float? QuantityOrdered,
    DateTime? RequiredByDate,
    int? ToBeDeliveredBy,
    bool Confirmed,
    bool Done,
    string Notes)
  {
    if (CustomerId.HasValue)
      this.Adapter.InsertCommand.Parameters[0].Value = (object) CustomerId.Value;
    else
      this.Adapter.InsertCommand.Parameters[0].Value = (object) DBNull.Value;
    if (OrderDate.HasValue)
      this.Adapter.InsertCommand.Parameters[1].Value = (object) OrderDate.Value;
    else
      this.Adapter.InsertCommand.Parameters[1].Value = (object) DBNull.Value;
    if (RoastDate.HasValue)
      this.Adapter.InsertCommand.Parameters[2].Value = (object) RoastDate.Value;
    else
      this.Adapter.InsertCommand.Parameters[2].Value = (object) DBNull.Value;
    if (ItemTypeID.HasValue)
      this.Adapter.InsertCommand.Parameters[3].Value = (object) ItemTypeID.Value;
    else
      this.Adapter.InsertCommand.Parameters[3].Value = (object) DBNull.Value;
    if (QuantityOrdered.HasValue)
      this.Adapter.InsertCommand.Parameters[4].Value = (object) QuantityOrdered.Value;
    else
      this.Adapter.InsertCommand.Parameters[4].Value = (object) DBNull.Value;
    if (RequiredByDate.HasValue)
      this.Adapter.InsertCommand.Parameters[5].Value = (object) RequiredByDate.Value;
    else
      this.Adapter.InsertCommand.Parameters[5].Value = (object) DBNull.Value;
    if (ToBeDeliveredBy.HasValue)
      this.Adapter.InsertCommand.Parameters[6].Value = (object) ToBeDeliveredBy.Value;
    else
      this.Adapter.InsertCommand.Parameters[6].Value = (object) DBNull.Value;
    this.Adapter.InsertCommand.Parameters[7].Value = (object) Confirmed;
    this.Adapter.InsertCommand.Parameters[8].Value = (object) Done;
    if (Notes == null)
      this.Adapter.InsertCommand.Parameters[9].Value = (object) DBNull.Value;
    else
      this.Adapter.InsertCommand.Parameters[9].Value = (object) Notes;
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
  [HelpKeyword("vs.data.TableAdapter")]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  public virtual int Update(
    int? CustomerId,
    DateTime? OrderDate,
    DateTime? RoastDate,
    int? ItemTypeID,
    float? QuantityOrdered,
    DateTime? RequiredByDate,
    int? ToBeDeliveredBy,
    bool Confirmed,
    bool Done,
    string Notes,
    int Original_OrderID)
  {
    if (CustomerId.HasValue)
      this.Adapter.UpdateCommand.Parameters[0].Value = (object) CustomerId.Value;
    else
      this.Adapter.UpdateCommand.Parameters[0].Value = (object) DBNull.Value;
    if (OrderDate.HasValue)
      this.Adapter.UpdateCommand.Parameters[1].Value = (object) OrderDate.Value;
    else
      this.Adapter.UpdateCommand.Parameters[1].Value = (object) DBNull.Value;
    if (RoastDate.HasValue)
      this.Adapter.UpdateCommand.Parameters[2].Value = (object) RoastDate.Value;
    else
      this.Adapter.UpdateCommand.Parameters[2].Value = (object) DBNull.Value;
    if (ItemTypeID.HasValue)
      this.Adapter.UpdateCommand.Parameters[3].Value = (object) ItemTypeID.Value;
    else
      this.Adapter.UpdateCommand.Parameters[3].Value = (object) DBNull.Value;
    if (QuantityOrdered.HasValue)
      this.Adapter.UpdateCommand.Parameters[4].Value = (object) QuantityOrdered.Value;
    else
      this.Adapter.UpdateCommand.Parameters[4].Value = (object) DBNull.Value;
    if (RequiredByDate.HasValue)
      this.Adapter.UpdateCommand.Parameters[5].Value = (object) RequiredByDate.Value;
    else
      this.Adapter.UpdateCommand.Parameters[5].Value = (object) DBNull.Value;
    if (ToBeDeliveredBy.HasValue)
      this.Adapter.UpdateCommand.Parameters[6].Value = (object) ToBeDeliveredBy.Value;
    else
      this.Adapter.UpdateCommand.Parameters[6].Value = (object) DBNull.Value;
    this.Adapter.UpdateCommand.Parameters[7].Value = (object) Confirmed;
    this.Adapter.UpdateCommand.Parameters[8].Value = (object) Done;
    if (Notes == null)
      this.Adapter.UpdateCommand.Parameters[9].Value = (object) DBNull.Value;
    else
      this.Adapter.UpdateCommand.Parameters[9].Value = (object) Notes;
    this.Adapter.UpdateCommand.Parameters[10].Value = (object) Original_OrderID;
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
