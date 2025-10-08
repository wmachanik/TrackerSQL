// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.DataSets.TrackerDataSetTableAdapters.TableAdapterManager
// Assembly: TrackerDotNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerDotNet.dll

using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Diagnostics;

#nullable disable
namespace TrackerDotNet.DataSets.TrackerDataSetTableAdapters;

[DesignerCategory("code")]
[ToolboxItem(true)]
[Designer("Microsoft.VSDesigner.DataSource.Design.TableAdapterManagerDesigner, Microsoft.VSDesigner, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
[HelpKeyword("vs.data.TableAdapterManager")]
public class TableAdapterManager : Component
{
  private TableAdapterManager.UpdateOrderOption _updateOrder;
  private OrdersTblTableAdapter _ordersTblTableAdapter;
  private CustomersTblTableAdapter _customersTblTableAdapter;
  private ItemTypeTblTableAdapter _itemTypeTblTableAdapter;
  private CityTblTableAdapter _cityTblTableAdapter;
  private bool _backupDataSetBeforeUpdate;
  private IDbConnection _connection;

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  public TableAdapterManager.UpdateOrderOption UpdateOrder
  {
    get => this._updateOrder;
    set => this._updateOrder = value;
  }

  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  [Editor("Microsoft.VSDesigner.DataSource.Design.TableAdapterManagerPropertyEditor, Microsoft.VSDesigner, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor")]
  [DebuggerNonUserCode]
  public OrdersTblTableAdapter OrdersTblTableAdapter
  {
    get => this._ordersTblTableAdapter;
    set => this._ordersTblTableAdapter = value;
  }

  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  [Editor("Microsoft.VSDesigner.DataSource.Design.TableAdapterManagerPropertyEditor, Microsoft.VSDesigner, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor")]
  [DebuggerNonUserCode]
  public CustomersTblTableAdapter CustomersTblTableAdapter
  {
    get => this._customersTblTableAdapter;
    set => this._customersTblTableAdapter = value;
  }

  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  [DebuggerNonUserCode]
  [Editor("Microsoft.VSDesigner.DataSource.Design.TableAdapterManagerPropertyEditor, Microsoft.VSDesigner, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor")]
  public ItemTypeTblTableAdapter ItemTypeTblTableAdapter
  {
    get => this._itemTypeTblTableAdapter;
    set => this._itemTypeTblTableAdapter = value;
  }

  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  [DebuggerNonUserCode]
  [Editor("Microsoft.VSDesigner.DataSource.Design.TableAdapterManagerPropertyEditor, Microsoft.VSDesigner, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor")]
  public CityTblTableAdapter CityTblTableAdapter
  {
    get => this._cityTblTableAdapter;
    set => this._cityTblTableAdapter = value;
  }

  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  [DebuggerNonUserCode]
  public bool BackupDataSetBeforeUpdate
  {
    get => this._backupDataSetBeforeUpdate;
    set => this._backupDataSetBeforeUpdate = value;
  }

  [DebuggerNonUserCode]
  [Browsable(false)]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  public IDbConnection Connection
  {
    get
    {
      if (this._connection != null)
        return this._connection;
      if (this._ordersTblTableAdapter != null && this._ordersTblTableAdapter.Connection != null)
        return (IDbConnection) this._ordersTblTableAdapter.Connection;
      if (this._customersTblTableAdapter != null && this._customersTblTableAdapter.Connection != null)
        return (IDbConnection) this._customersTblTableAdapter.Connection;
      if (this._itemTypeTblTableAdapter != null && this._itemTypeTblTableAdapter.Connection != null)
        return (IDbConnection) this._itemTypeTblTableAdapter.Connection;
      return this._cityTblTableAdapter != null && this._cityTblTableAdapter.Connection != null ? (IDbConnection) this._cityTblTableAdapter.Connection : (IDbConnection) null;
    }
    set => this._connection = value;
  }

  [Browsable(false)]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  [DebuggerNonUserCode]
  public int TableAdapterInstanceCount
  {
    get
    {
      int adapterInstanceCount = 0;
      if (this._ordersTblTableAdapter != null)
        ++adapterInstanceCount;
      if (this._customersTblTableAdapter != null)
        ++adapterInstanceCount;
      if (this._itemTypeTblTableAdapter != null)
        ++adapterInstanceCount;
      if (this._cityTblTableAdapter != null)
        ++adapterInstanceCount;
      return adapterInstanceCount;
    }
  }

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  private int UpdateUpdatedRows(
    TrackerDataSet dataSet,
    List<DataRow> allChangedRows,
    List<DataRow> allAddedRows)
  {
    int num = 0;
    if (this._itemTypeTblTableAdapter != null)
    {
      DataRow[] realUpdatedRows = this.GetRealUpdatedRows(dataSet.ItemTypeTbl.Select((string) null, (string) null, DataViewRowState.ModifiedCurrent), allAddedRows);
      if (realUpdatedRows != null && 0 < realUpdatedRows.Length)
      {
        num += this._itemTypeTblTableAdapter.Update(realUpdatedRows);
        allChangedRows.AddRange((IEnumerable<DataRow>) realUpdatedRows);
      }
    }
    if (this._cityTblTableAdapter != null)
    {
      DataRow[] realUpdatedRows = this.GetRealUpdatedRows(dataSet.CityTbl.Select((string) null, (string) null, DataViewRowState.ModifiedCurrent), allAddedRows);
      if (realUpdatedRows != null && 0 < realUpdatedRows.Length)
      {
        num += this._cityTblTableAdapter.Update(realUpdatedRows);
        allChangedRows.AddRange((IEnumerable<DataRow>) realUpdatedRows);
      }
    }
    if (this._customersTblTableAdapter != null)
    {
      DataRow[] realUpdatedRows = this.GetRealUpdatedRows(dataSet.CustomersTbl.Select((string) null, (string) null, DataViewRowState.ModifiedCurrent), allAddedRows);
      if (realUpdatedRows != null && 0 < realUpdatedRows.Length)
      {
        num += this._customersTblTableAdapter.Update(realUpdatedRows);
        allChangedRows.AddRange((IEnumerable<DataRow>) realUpdatedRows);
      }
    }
    if (this._ordersTblTableAdapter != null)
    {
      DataRow[] realUpdatedRows = this.GetRealUpdatedRows(dataSet.OrdersTbl.Select((string) null, (string) null, DataViewRowState.ModifiedCurrent), allAddedRows);
      if (realUpdatedRows != null && 0 < realUpdatedRows.Length)
      {
        num += this._ordersTblTableAdapter.Update(realUpdatedRows);
        allChangedRows.AddRange((IEnumerable<DataRow>) realUpdatedRows);
      }
    }
    return num;
  }

  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  [DebuggerNonUserCode]
  private int UpdateInsertedRows(TrackerDataSet dataSet, List<DataRow> allAddedRows)
  {
    int num = 0;
    if (this._itemTypeTblTableAdapter != null)
    {
      DataRow[] dataRowArray = dataSet.ItemTypeTbl.Select((string) null, (string) null, DataViewRowState.Added);
      if (dataRowArray != null && 0 < dataRowArray.Length)
      {
        num += this._itemTypeTblTableAdapter.Update(dataRowArray);
        allAddedRows.AddRange((IEnumerable<DataRow>) dataRowArray);
      }
    }
    if (this._cityTblTableAdapter != null)
    {
      DataRow[] dataRowArray = dataSet.CityTbl.Select((string) null, (string) null, DataViewRowState.Added);
      if (dataRowArray != null && 0 < dataRowArray.Length)
      {
        num += this._cityTblTableAdapter.Update(dataRowArray);
        allAddedRows.AddRange((IEnumerable<DataRow>) dataRowArray);
      }
    }
    if (this._customersTblTableAdapter != null)
    {
      DataRow[] dataRowArray = dataSet.CustomersTbl.Select((string) null, (string) null, DataViewRowState.Added);
      if (dataRowArray != null && 0 < dataRowArray.Length)
      {
        num += this._customersTblTableAdapter.Update(dataRowArray);
        allAddedRows.AddRange((IEnumerable<DataRow>) dataRowArray);
      }
    }
    if (this._ordersTblTableAdapter != null)
    {
      DataRow[] dataRowArray = dataSet.OrdersTbl.Select((string) null, (string) null, DataViewRowState.Added);
      if (dataRowArray != null && 0 < dataRowArray.Length)
      {
        num += this._ordersTblTableAdapter.Update(dataRowArray);
        allAddedRows.AddRange((IEnumerable<DataRow>) dataRowArray);
      }
    }
    return num;
  }

  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  [DebuggerNonUserCode]
  private int UpdateDeletedRows(TrackerDataSet dataSet, List<DataRow> allChangedRows)
  {
    int num = 0;
    if (this._ordersTblTableAdapter != null)
    {
      DataRow[] dataRowArray = dataSet.OrdersTbl.Select((string) null, (string) null, DataViewRowState.Deleted);
      if (dataRowArray != null && 0 < dataRowArray.Length)
      {
        num += this._ordersTblTableAdapter.Update(dataRowArray);
        allChangedRows.AddRange((IEnumerable<DataRow>) dataRowArray);
      }
    }
    if (this._customersTblTableAdapter != null)
    {
      DataRow[] dataRowArray = dataSet.CustomersTbl.Select((string) null, (string) null, DataViewRowState.Deleted);
      if (dataRowArray != null && 0 < dataRowArray.Length)
      {
        num += this._customersTblTableAdapter.Update(dataRowArray);
        allChangedRows.AddRange((IEnumerable<DataRow>) dataRowArray);
      }
    }
    if (this._cityTblTableAdapter != null)
    {
      DataRow[] dataRowArray = dataSet.CityTbl.Select((string) null, (string) null, DataViewRowState.Deleted);
      if (dataRowArray != null && 0 < dataRowArray.Length)
      {
        num += this._cityTblTableAdapter.Update(dataRowArray);
        allChangedRows.AddRange((IEnumerable<DataRow>) dataRowArray);
      }
    }
    if (this._itemTypeTblTableAdapter != null)
    {
      DataRow[] dataRowArray = dataSet.ItemTypeTbl.Select((string) null, (string) null, DataViewRowState.Deleted);
      if (dataRowArray != null && 0 < dataRowArray.Length)
      {
        num += this._itemTypeTblTableAdapter.Update(dataRowArray);
        allChangedRows.AddRange((IEnumerable<DataRow>) dataRowArray);
      }
    }
    return num;
  }

  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  [DebuggerNonUserCode]
  private DataRow[] GetRealUpdatedRows(DataRow[] updatedRows, List<DataRow> allAddedRows)
  {
    if (updatedRows == null || updatedRows.Length < 1 || allAddedRows == null || allAddedRows.Count < 1)
      return updatedRows;
    List<DataRow> dataRowList = new List<DataRow>();
    for (int index = 0; index < updatedRows.Length; ++index)
    {
      DataRow updatedRow = updatedRows[index];
      if (!allAddedRows.Contains(updatedRow))
        dataRowList.Add(updatedRow);
    }
    return dataRowList.ToArray();
  }

  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  [DebuggerNonUserCode]
  public virtual int UpdateAll(TrackerDataSet dataSet)
  {
    if (dataSet == null)
      throw new ArgumentNullException(nameof (dataSet));
    if (!dataSet.HasChanges())
      return 0;
    if (this._ordersTblTableAdapter != null && !this.MatchTableAdapterConnection((IDbConnection) this._ordersTblTableAdapter.Connection))
      throw new ArgumentException("All TableAdapters managed by a TableAdapterManager must use the same connection string.");
    if (this._customersTblTableAdapter != null && !this.MatchTableAdapterConnection((IDbConnection) this._customersTblTableAdapter.Connection))
      throw new ArgumentException("All TableAdapters managed by a TableAdapterManager must use the same connection string.");
    if (this._itemTypeTblTableAdapter != null && !this.MatchTableAdapterConnection((IDbConnection) this._itemTypeTblTableAdapter.Connection))
      throw new ArgumentException("All TableAdapters managed by a TableAdapterManager must use the same connection string.");
    if (this._cityTblTableAdapter != null && !this.MatchTableAdapterConnection((IDbConnection) this._cityTblTableAdapter.Connection))
      throw new ArgumentException("All TableAdapters managed by a TableAdapterManager must use the same connection string.");
    IDbConnection connection = this.Connection;
    if (connection == null)
      throw new ApplicationException("TableAdapterManager contains no connection information. Set each TableAdapterManager TableAdapter property to a valid TableAdapter instance.");
    bool flag = false;
    if ((connection.State & ConnectionState.Broken) == ConnectionState.Broken)
      connection.Close();
    if (connection.State == ConnectionState.Closed)
    {
      connection.Open();
      flag = true;
    }
    IDbTransaction dbTransaction = connection.BeginTransaction();
    if (dbTransaction == null)
      throw new ApplicationException("The transaction cannot begin. The current data connection does not support transactions or the current state is not allowing the transaction to begin.");
    List<DataRow> allChangedRows = new List<DataRow>();
    List<DataRow> allAddedRows = new List<DataRow>();
    List<DataAdapter> dataAdapterList = new List<DataAdapter>();
    Dictionary<object, IDbConnection> dictionary = new Dictionary<object, IDbConnection>();
    int num = 0;
    DataSet dataSet1 = (DataSet) null;
    if (this.BackupDataSetBeforeUpdate)
    {
      dataSet1 = new DataSet();
      dataSet1.Merge((DataSet) dataSet);
    }
    try
    {
      if (this._ordersTblTableAdapter != null)
      {
        dictionary.Add((object) this._ordersTblTableAdapter, (IDbConnection) this._ordersTblTableAdapter.Connection);
        this._ordersTblTableAdapter.Connection = (OleDbConnection) connection;
        this._ordersTblTableAdapter.Transaction = (OleDbTransaction) dbTransaction;
        if (this._ordersTblTableAdapter.Adapter.AcceptChangesDuringUpdate)
        {
          this._ordersTblTableAdapter.Adapter.AcceptChangesDuringUpdate = false;
          dataAdapterList.Add((DataAdapter) this._ordersTblTableAdapter.Adapter);
        }
      }
      if (this._customersTblTableAdapter != null)
      {
        dictionary.Add((object) this._customersTblTableAdapter, (IDbConnection) this._customersTblTableAdapter.Connection);
        this._customersTblTableAdapter.Connection = (OleDbConnection) connection;
        this._customersTblTableAdapter.Transaction = (OleDbTransaction) dbTransaction;
        if (this._customersTblTableAdapter.Adapter.AcceptChangesDuringUpdate)
        {
          this._customersTblTableAdapter.Adapter.AcceptChangesDuringUpdate = false;
          dataAdapterList.Add((DataAdapter) this._customersTblTableAdapter.Adapter);
        }
      }
      if (this._itemTypeTblTableAdapter != null)
      {
        dictionary.Add((object) this._itemTypeTblTableAdapter, (IDbConnection) this._itemTypeTblTableAdapter.Connection);
        this._itemTypeTblTableAdapter.Connection = (OleDbConnection) connection;
        this._itemTypeTblTableAdapter.Transaction = (OleDbTransaction) dbTransaction;
        if (this._itemTypeTblTableAdapter.Adapter.AcceptChangesDuringUpdate)
        {
          this._itemTypeTblTableAdapter.Adapter.AcceptChangesDuringUpdate = false;
          dataAdapterList.Add((DataAdapter) this._itemTypeTblTableAdapter.Adapter);
        }
      }
      if (this._cityTblTableAdapter != null)
      {
        dictionary.Add((object) this._cityTblTableAdapter, (IDbConnection) this._cityTblTableAdapter.Connection);
        this._cityTblTableAdapter.Connection = (OleDbConnection) connection;
        this._cityTblTableAdapter.Transaction = (OleDbTransaction) dbTransaction;
        if (this._cityTblTableAdapter.Adapter.AcceptChangesDuringUpdate)
        {
          this._cityTblTableAdapter.Adapter.AcceptChangesDuringUpdate = false;
          dataAdapterList.Add((DataAdapter) this._cityTblTableAdapter.Adapter);
        }
      }
      if (this.UpdateOrder == TableAdapterManager.UpdateOrderOption.UpdateInsertDelete)
      {
        num += this.UpdateUpdatedRows(dataSet, allChangedRows, allAddedRows);
        num += this.UpdateInsertedRows(dataSet, allAddedRows);
      }
      else
      {
        num += this.UpdateInsertedRows(dataSet, allAddedRows);
        num += this.UpdateUpdatedRows(dataSet, allChangedRows, allAddedRows);
      }
      num += this.UpdateDeletedRows(dataSet, allChangedRows);
      dbTransaction.Commit();
      if (0 < allAddedRows.Count)
      {
        DataRow[] array = new DataRow[allAddedRows.Count];
        allAddedRows.CopyTo(array);
        for (int index = 0; index < array.Length; ++index)
          array[index].AcceptChanges();
      }
      if (0 < allChangedRows.Count)
      {
        DataRow[] array = new DataRow[allChangedRows.Count];
        allChangedRows.CopyTo(array);
        for (int index = 0; index < array.Length; ++index)
          array[index].AcceptChanges();
      }
    }
    catch (Exception ex)
    {
      dbTransaction.Rollback();
      if (this.BackupDataSetBeforeUpdate)
      {
        dataSet.Clear();
        dataSet.Merge(dataSet1);
      }
      else if (0 < allAddedRows.Count)
      {
        DataRow[] array = new DataRow[allAddedRows.Count];
        allAddedRows.CopyTo(array);
        for (int index = 0; index < array.Length; ++index)
        {
          DataRow dataRow = array[index];
          dataRow.AcceptChanges();
          dataRow.SetAdded();
        }
      }
      throw ex;
    }
    finally
    {
      if (flag)
        connection.Close();
      if (this._ordersTblTableAdapter != null)
      {
        this._ordersTblTableAdapter.Connection = (OleDbConnection) dictionary[(object) this._ordersTblTableAdapter];
        this._ordersTblTableAdapter.Transaction = (OleDbTransaction) null;
      }
      if (this._customersTblTableAdapter != null)
      {
        this._customersTblTableAdapter.Connection = (OleDbConnection) dictionary[(object) this._customersTblTableAdapter];
        this._customersTblTableAdapter.Transaction = (OleDbTransaction) null;
      }
      if (this._itemTypeTblTableAdapter != null)
      {
        this._itemTypeTblTableAdapter.Connection = (OleDbConnection) dictionary[(object) this._itemTypeTblTableAdapter];
        this._itemTypeTblTableAdapter.Transaction = (OleDbTransaction) null;
      }
      if (this._cityTblTableAdapter != null)
      {
        this._cityTblTableAdapter.Connection = (OleDbConnection) dictionary[(object) this._cityTblTableAdapter];
        this._cityTblTableAdapter.Transaction = (OleDbTransaction) null;
      }
      if (0 < dataAdapterList.Count)
      {
        DataAdapter[] array = new DataAdapter[dataAdapterList.Count];
        dataAdapterList.CopyTo(array);
        for (int index = 0; index < array.Length; ++index)
          array[index].AcceptChangesDuringUpdate = true;
      }
    }
    return num;
  }

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  protected virtual void SortSelfReferenceRows(
    DataRow[] rows,
    DataRelation relation,
    bool childFirst)
  {
    Array.Sort<DataRow>(rows, (IComparer<DataRow>) new TableAdapterManager.SelfReferenceComparer(relation, childFirst));
  }

  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  [DebuggerNonUserCode]
  protected virtual bool MatchTableAdapterConnection(IDbConnection inputConnection)
  {
    return this._connection != null || this.Connection == null || inputConnection == null || string.Equals(this.Connection.ConnectionString, inputConnection.ConnectionString, StringComparison.Ordinal);
  }

  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  public enum UpdateOrderOption
  {
    InsertUpdateDelete,
    UpdateInsertDelete,
  }

  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  private class SelfReferenceComparer : IComparer<DataRow>
  {
    private DataRelation _relation;
    private int _childFirst;

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
    internal SelfReferenceComparer(DataRelation relation, bool childFirst)
    {
      this._relation = relation;
      if (childFirst)
        this._childFirst = -1;
      else
        this._childFirst = 1;
    }

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
    private DataRow GetRoot(DataRow row, out int distance)
    {
      DataRow root = row;
      distance = 0;
      IDictionary<DataRow, DataRow> dictionary = (IDictionary<DataRow, DataRow>) new Dictionary<DataRow, DataRow>();
      dictionary[row] = row;
      for (DataRow parentRow = row.GetParentRow(this._relation, DataRowVersion.Default); parentRow != null && !dictionary.ContainsKey(parentRow); parentRow = parentRow.GetParentRow(this._relation, DataRowVersion.Default))
      {
        ++distance;
        root = parentRow;
        dictionary[parentRow] = parentRow;
      }
      if (distance == 0)
      {
        dictionary.Clear();
        dictionary[row] = row;
        for (DataRow parentRow = row.GetParentRow(this._relation, DataRowVersion.Original); parentRow != null && !dictionary.ContainsKey(parentRow); parentRow = parentRow.GetParentRow(this._relation, DataRowVersion.Original))
        {
          ++distance;
          root = parentRow;
          dictionary[parentRow] = parentRow;
        }
      }
      return root;
    }

    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
    [DebuggerNonUserCode]
    public int Compare(DataRow row1, DataRow row2)
    {
      if (object.ReferenceEquals((object) row1, (object) row2))
        return 0;
      if (row1 == null)
        return -1;
      if (row2 == null)
        return 1;
      int distance1 = 0;
      DataRow root1 = this.GetRoot(row1, out distance1);
      int distance2 = 0;
      DataRow root2 = this.GetRoot(row2, out distance2);
      if (object.ReferenceEquals((object) root1, (object) root2))
        return this._childFirst * distance1.CompareTo(distance2);
      return root1.Table.Rows.IndexOf(root1) < root2.Table.Rows.IndexOf(root2) ? -1 : 1;
    }
  }
}
