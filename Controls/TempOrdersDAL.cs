// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.control.TempOrdersDAL
// Assembly: TrackerDotNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerDotNet.dll

using System.Data;
using TrackerDotNet.Classes;

//- only form later versions #nullable disable
namespace TrackerDotNet.Controls
{
    public class TempOrdersDAL
    {
        private const string CONST_SQL_UPDATEORDERSASDONE = "UPDATE OrdersTbl SET Done = True WHERE EXISTS (SELECT OriginalOrderID FROM TempOrdersLinesTbl WHERE (OriginalOrderID = OrdersTbl.OrderID))";

        public bool Insert(TempOrdersData pTempOrder)
        {
            int index = 0;
            TempOrdersHeaderTbl tempOrdersHeaderTbl = new TempOrdersHeaderTbl();
            TempOrdersLinesTbl tempOrdersLinesTbl = new TempOrdersLinesTbl();
            bool flag = tempOrdersHeaderTbl.Insert(pTempOrder.HeaderData);
            int currentToHeaderId = tempOrdersHeaderTbl.GetCurrentTOHeaderID();
            for (; flag && pTempOrder.OrdersLines.Count > index; ++index)
            {
                pTempOrder.OrdersLines[index].TOHeaderID = currentToHeaderId;
                flag = tempOrdersLinesTbl.Insert(pTempOrder.OrdersLines[index]);
            }
            return flag;
        }

        public bool HasCoffeeInTempOrder()
        {
            string strSQL = "SELECT ServiceTypeID  FROM TempOrdersLinesTbl WHERE ServiceTypeID = 2";
            TrackerDb trackerDb = new TrackerDb();
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(strSQL);
            bool flag = dataReader != null && dataReader.Read();
            dataReader.Close();
            trackerDb.Close();
            return flag;
        }

        public bool MarkTempOrdersItemsAsDone()
        {
            TrackerDb trackerDb = new TrackerDb();
            bool flag = string.IsNullOrEmpty(trackerDb.ExecuteNonQuerySQL("UPDATE OrdersTbl SET Done = True WHERE EXISTS (SELECT OriginalOrderID FROM TempOrdersLinesTbl WHERE (OriginalOrderID = OrdersTbl.OrderID))"));
            trackerDb.Close();
            return flag;
        }

        public bool KillTempOrdersData()
        {
            TempOrdersHeaderTbl tempOrdersHeaderTbl = new TempOrdersHeaderTbl();
            TempOrdersLinesTbl tempOrdersLinesTbl = new TempOrdersLinesTbl();
            bool flag = tempOrdersHeaderTbl.DeleteAllRecords();
            return tempOrdersLinesTbl.DeleteAllRecords() && flag;
        }
    }
}