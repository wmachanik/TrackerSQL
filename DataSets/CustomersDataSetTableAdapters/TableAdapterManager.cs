// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.DataSets.CustomersDataSetTableAdapters.TableAdapterManager
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
using System.Diagnostics;

#nullable disable
namespace TrackerDotNet.DataSets.CustomersDataSetTableAdapters;

[DesignerCategory("code")]
[Designer("Microsoft.VSDesigner.DataSource.Design.TableAdapterManagerDesigner, Microsoft.VSDesigner, Version=10.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
[ToolboxItem(true)]
[HelpKeyword("vs.data.TableAdapterManager")]
public class TableAdapterManager : Component
{
  private TableAdapterManager.UpdateOrderOption _updateOrder;
  private bool _backupDataSetBeforeUpdate;
  private IDbConnection _connection;

  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  [DebuggerNonUserCode]
  public TableAdapterManager.UpdateOrderOption UpdateOrder
  {
    get => this._updateOrder;
    set => this._updateOrder = value;
  }

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  public bool BackupDataSetBeforeUpdate
  {
    get => this._backupDataSetBeforeUpdate;
    set => this._backupDataSetBeforeUpdate = value;
  }

  [Browsable(false)]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  [DebuggerNonUserCode]
  public IDbConnection Connection
  {
    get => this._connection != null ? this._connection : (IDbConnection) null;
    set => this._connection = value;
  }

  [Browsable(false)]
  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  public int TableAdapterInstanceCount => 0;

  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  [DebuggerNonUserCode]
  private int UpdateUpdatedRows(
    CustomersDataSet dataSet,
    List<DataRow> allChangedRows,
    List<DataRow> allAddedRows)
  {
    return 0;
  }

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  private int UpdateInsertedRows(CustomersDataSet dataSet, List<DataRow> allAddedRows) => 0;

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  private int UpdateDeletedRows(CustomersDataSet dataSet, List<DataRow> allChangedRows) => 0;

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
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

  [DebuggerNonUserCode]
  [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
  public virtual int UpdateAll(CustomersDataSet dataSet)
  {
    if (dataSet == null)
      throw new ArgumentNullException(nameof (dataSet));
    if (!dataSet.HasChanges())
      return 0;
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

    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
    [DebuggerNonUserCode]
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

    [DebuggerNonUserCode]
    [GeneratedCode("System.Data.Design.TypedDataSetGenerator", "4.0.0.0")]
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
