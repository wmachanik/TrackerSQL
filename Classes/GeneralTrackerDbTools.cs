// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.classes.GeneralTrackerDbTools
// Assembly: TrackerDotNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerDotNet.dll

using System;
using System.Collections.Generic;
using System.Data;
using TrackerDotNet.Controls;

//- only form later versions #nullable disable
namespace TrackerDotNet.Classes
{
    public class GeneralTrackerDbTools
    {
        private const int CONST_MAXROLLINGAVEVALUES = 6;

        public GeneralTrackerDbTools.LineUsageData GetLatestUsageData(
          long pCustomerID,
          int pServiceTypeID)
        {
            GeneralTrackerDbTools.LineUsageData latestUsageData1 = new GeneralTrackerDbTools.LineUsageData();
            ClientUsageLinesTbl latestUsageData2 = new ClientUsageLinesTbl().GetLatestUsageData(pCustomerID, pServiceTypeID);
            if (latestUsageData2 != null)
            {
                latestUsageData1.LastCount = latestUsageData2.CupCount;
                latestUsageData1.LastQty = latestUsageData2.Qty;
                latestUsageData1.UsageDate = latestUsageData2.LineDate;
            }
            return latestUsageData1;
        }

        public DateTime GetInstallDate(long pCustomerID)
        {
            return new ClientUsageLinesTbl().GetCustomerInstallDate(pCustomerID);
        }

        public double GetAveConsumption(long pCustomerID)
        {
            return new ClientUsageTbl().GetAverageConsumption(pCustomerID);
        }

        public GeneralTrackerDbTools.LineUsageData GetLastCupCount(long pCustomerID)
        {
            return this.GetLastCupCount(pCustomerID, 2);
        }

        public GeneralTrackerDbTools.LineUsageData GetLastCupCount(long pCustomerID, int pServiceTypeID)
        {
            return this.GetLatestUsageData(pCustomerID, pServiceTypeID);
        }

        public int DaysToAddToDate(int pServiceTypeID)
        {
            int addToDate = 0;
            switch (pServiceTypeID)
            {
                case 11:
                    addToDate = -7;
                    break;
                case 12:
                    addToDate = -14;
                    break;
                case 13:
                    addToDate = -21;
                    break;
                case 14:
                    addToDate = -31;
                    break;
                case 15:
                    addToDate = -42;
                    break;
                case 16 /*0x10*/:
                    addToDate = -61;
                    break;
            }
            return addToDate;
        }

        public DateTime AddHolidayExtension(long pCustomerID, DateTime pDT, int pServiceTypeID)
        {
            List<ClientUsageLinesTbl> last10UsageLines = new ClientUsageLinesTbl().GetLast10UsageLines(pCustomerID);
            for (int index = 0; last10UsageLines.Count > index && index < 6; ++index)
            {
                if (last10UsageLines[index].ServiceTypeID != 0)
                {
                    if (last10UsageLines[index].ServiceTypeID >= 11 || last10UsageLines[index].ServiceTypeID <= 16 /*0x10*/)
                        pDT = pDT.AddMonths(this.DaysToAddToDate(last10UsageLines[index].ServiceTypeID));
                    else if (last10UsageLines[index].ServiceTypeID == pServiceTypeID)
                        index = 6;
                }
            }
            return pDT;
        }

        public int DaysToRemoveFromDate(int pServiceTypeID)
        {
            int removeFromDate = 0;
            switch (pServiceTypeID)
            {
                case 11:
                    removeFromDate = -7;
                    break;
                case 12:
                    removeFromDate = -14;
                    break;
                case 13:
                    removeFromDate = -21;
                    break;
                case 14:
                    removeFromDate = -31;
                    break;
                case 15:
                    removeFromDate = -42;
                    break;
                case 16 /*0x10*/:
                    removeFromDate = -61;
                    break;
            }
            return removeFromDate;
        }

        public DateTime RemoveHolidayPeriodFromDate(long pCustomerID, DateTime pDT, int pServiceTypeID)
        {
            List<ClientUsageLinesTbl> last10UsageLines = new ClientUsageLinesTbl().GetLast10UsageLines(pCustomerID);
            for (int index = 0; last10UsageLines.Count > index && index < 6; ++index)
            {
                if (last10UsageLines[index].ServiceTypeID != 0)
                {
                    if (last10UsageLines[index].ServiceTypeID >= 11 || last10UsageLines[index].ServiceTypeID <= 16 /*0x10*/)
                        pDT = pDT.AddMonths(this.DaysToRemoveFromDate(last10UsageLines[index].ServiceTypeID));
                    else if (last10UsageLines[index].ServiceTypeID == pServiceTypeID)
                        index = 6;
                }
            }
            return pDT;
        }

        public int CalcEstCupCount(
          long pCustomerID,
          GeneralTrackerDbTools.LineUsageData pClientUsageData,
          bool pIsCoffee)
        {
            double a = 1.0;
            if (pClientUsageData.UsageDate > DateTime.MinValue)
            {
                double aveConsumption = this.GetAveConsumption(pCustomerID);
                int int32 = Convert.ToInt32((TimeZoneUtils.Now().Date - this.RemoveHolidayPeriodFromDate(pCustomerID, pClientUsageData.UsageDate, 2).Date).TotalDays);
                a = !pIsCoffee || pClientUsageData.LastQty == 0.0 ? (double)pClientUsageData.LastCount + (double)int32 * aveConsumption : (double)pClientUsageData.LastCount + pClientUsageData.LastQty * 100.0;
            }
            return Convert.ToInt32(Math.Round(a));
        }

        public int CoffeeCupsLeft(
          long pCustomerID,
          GeneralTrackerDbTools.LineUsageData pCoffeeUsageData)
        {
            return this.CoffeeCupsLeft(pCustomerID, pCoffeeUsageData, 2, 100);
        }

        public int CoffeeCupsLeft(
          long pCustomerID,
          GeneralTrackerDbTools.LineUsageData pCoffeeUsageData,
          int pServiceTypeID,
          int pTypicalPerKg)
        {
            GeneralTrackerDbTools.LineUsageData latestUsageData = this.GetLatestUsageData(pCustomerID, pServiceTypeID);
            int num = (int)Math.Round(latestUsageData.LastQty * (double)pTypicalPerKg);
            if (latestUsageData.LastCount > pCoffeeUsageData.LastCount)
                num -= latestUsageData.LastCount - pCoffeeUsageData.LastCount;
            return num;
        }

        public double CalcAveConsumption(long pCustomerID)
        {
            return this.CalcAveConsumption(pCustomerID, 2, 5.0, true);
        }

        public double CalcAveConsumption(
          long pCustomerID,
          int pServiceTypeID,
          double pTypicalAverageConsumption,
          bool pPerDayCalc)
        {
            double num1 = pTypicalAverageConsumption;
            List<double> doubleList = new List<double>();
            int num2 = 0;
            int index1 = 1;
            DateTime minValue = DateTime.MinValue;
            List<ClientUsageLinesTbl> last10UsageLines = new ClientUsageLinesTbl().GetLast10UsageLines(pCustomerID, pServiceTypeID);
            last10UsageLines.Sort((Comparison<ClientUsageLinesTbl>)((x, y) => x.LineDate.CompareTo(y.LineDate)));
            if (last10UsageLines.Count > 1)
            {
                DateTime dateTime = last10UsageLines[0].LineDate;
                long cupCount = last10UsageLines[0].CupCount;
                for (; last10UsageLines.Count > index1 && num2 < 6; ++index1)
                {
                    int serviceTypeId = last10UsageLines[index1].ServiceTypeID;
                    if (cupCount < last10UsageLines[index1].CupCount)
                    {
                        if (serviceTypeId == pServiceTypeID && last10UsageLines[index1].LineDate > dateTime)
                        {
                            long num3 = last10UsageLines[index1].CupCount - cupCount;
                            if (pPerDayCalc)
                            {
                                int days = (last10UsageLines[index1].LineDate - dateTime).Days;
                                double num4 = Math.Round((double)num3 / (double)days, 5);
                                doubleList.Add(num4);
                            }
                            else
                                doubleList.Add((double)num3);
                            ++num2;
                            dateTime = last10UsageLines[index1].LineDate;
                            cupCount = last10UsageLines[index1].CupCount;
                        }
                        else if (serviceTypeId >= 11 && serviceTypeId <= 16 /*0x10*/)
                            dateTime = dateTime.AddDays((double)this.DaysToAddToDate(serviceTypeId));
                    }
                }
                if (doubleList.Count >= 6)
                {
                    double num5 = doubleList[0];
                    for (int index2 = 1; index2 < doubleList.Count; ++index2)
                    {
                        if (doubleList[index2] > num5)
                            num5 = doubleList[index2];
                    }
                    doubleList.Remove(num5);
                }
                double num6 = 0.0;
                for (int index3 = 0; index3 < doubleList.Count; ++index3)
                    num6 += doubleList[index3];
                if (num6 > 0.0)
                    num1 = Math.Round(num6 / (double)doubleList.Count, SystemConstants.DatabaseConstants.NumDecimalPoints);
            }
            else
                num1 = !pPerDayCalc ? pTypicalAverageConsumption : 5.0;
            if (num1 <= 0.0)
                num1 = pTypicalAverageConsumption;
            return num1;
        }

        private GeneralTrackerDbTools.ClientServiceItem SetClientServiceData(
          long pCustomerID,
          TrackedServiceItemTbl pTrackedServiceItemData,
          double pDailyAverage)
        {
            bool pPerDayCalc = pDailyAverage == 0.0;
            GeneralTrackerDbTools.ClientServiceItem clientServiceItem = new GeneralTrackerDbTools.ClientServiceItem();
            clientServiceItem.UsageDateFieldName = pTrackedServiceItemData.UsageDateFieldName;
            clientServiceItem.UsageAveFieldName = pTrackedServiceItemData.UsageAveFieldName;
            clientServiceItem.ThisItemsAverage = this.CalcAveConsumption(pCustomerID, pTrackedServiceItemData.ServiceTypeID, pTrackedServiceItemData.TypicalAvePerItem, pPerDayCalc);
            GeneralTrackerDbTools.LineUsageData latestUsageData = this.GetLatestUsageData(pCustomerID, pTrackedServiceItemData.ServiceTypeID);
            if (latestUsageData.UsageDate == DateTime.MinValue)
                latestUsageData.UsageDate = this.GetInstallDate(pCustomerID);
            int num = !pPerDayCalc ? (int)Math.Round(latestUsageData.LastQty * clientServiceItem.ThisItemsAverage / pDailyAverage, 0) : (int)Math.Round(latestUsageData.LastQty * 100.0 / clientServiceItem.ThisItemsAverage, 0);
            clientServiceItem.NextUsageDate = this.AddHolidayExtension(pCustomerID, latestUsageData.UsageDate.AddDays((double)num), pTrackedServiceItemData.ServiceTypeID);
            return clientServiceItem;
        }

        public bool InsertClientUsageTable(
          long pCustomerID,
          List<GeneralTrackerDbTools.ClientServiceItem> pClientServiceItems)
        {
            TrackerDb trackerDb = new TrackerDb();
            string str1 = "INSERT INTO ClientUsageTbl (CustomerID,";
            string str2 = ") VALUES (?, ";
            for (int index = 0; index < pClientServiceItems.Count; ++index)
            {
                str1 = $"{$"{str1}{pClientServiceItems[index].UsageDateFieldName}, "}{pClientServiceItems[index].UsageAveFieldName}, ";
                str2 += "?, ?, ";
            }
            string strSQL = $"{str1.Remove(str1.Length - 2, 2)}{str2.Remove(str2.Length - 2, 2)})";
            trackerDb.AddParams((object)pCustomerID, DbType.Int64);
            for (int index = 0; index < pClientServiceItems.Count; ++index)
            {
                trackerDb.AddParams((object)pClientServiceItems[index].NextUsageDate, DbType.Date);
                trackerDb.AddParams((object)pClientServiceItems[index].ThisItemsAverage, DbType.Double);
            }
            return string.IsNullOrEmpty(trackerDb.ExecuteNonQuerySQL(strSQL));
        }

        public bool UpdateClientUsageTable(
          long pCustomerID,
          List<GeneralTrackerDbTools.ClientServiceItem> pClientServiceItems)
        {
            TrackerDb trackerDb = new TrackerDb();
            string str = "UPDATE ClientUsageTbl SET ";
            for (int index = 0; index < pClientServiceItems.Count; ++index)
                str = $"{$"{str}{pClientServiceItems[index].UsageDateFieldName} = ?, "}{pClientServiceItems[index].UsageAveFieldName} = ?, ";
            string strSQL = str.Remove(str.Length - 2, 2) + " WHERE CustomerID = ?";
            for (int index = 0; index < pClientServiceItems.Count; ++index)
            {
                trackerDb.AddParams((object)pClientServiceItems[index].NextUsageDate, DbType.Date, "@" + pClientServiceItems[index].UsageDateFieldName);
                trackerDb.AddParams((object)pClientServiceItems[index].ThisItemsAverage, DbType.Double, "@" + pClientServiceItems[index].UsageAveFieldName);
            }
            trackerDb.AddWhereParams((object)pCustomerID, DbType.Int64, "@CustomerID");
            bool flag = string.IsNullOrEmpty(trackerDb.ExecuteNonQuerySQL(strSQL));
            trackerDb.Close();
            return flag;
        }

        public bool CalcAndSaveNextRequiredDates(long pCustomerID)
        {
            List<TrackedServiceItemTbl> all = new TrackedServiceItemTbl().GetAll("ThisItemSetsDailyAverage, ServiceTypeID");
            double pDailyAverage = 5.0;
            int index = 0;
            List<GeneralTrackerDbTools.ClientServiceItem> pClientServiceItems = new List<GeneralTrackerDbTools.ClientServiceItem>();
            for (; all[index].ThisItemSetsDailyAverage; ++index)
            {
                GeneralTrackerDbTools.ClientServiceItem clientServiceItem = this.SetClientServiceData(pCustomerID, all[index], 0.0);
                pDailyAverage = clientServiceItem.ThisItemsAverage;
                pClientServiceItems.Add(clientServiceItem);
            }
            for (; index < all.Count; ++index)
            {
                GeneralTrackerDbTools.ClientServiceItem clientServiceItem = this.SetClientServiceData(pCustomerID, all[index], pDailyAverage);
                pClientServiceItems.Add(clientServiceItem);
            }
            return !new ClientUsageTbl().UsageDataExists(pCustomerID) ? this.InsertClientUsageTable(pCustomerID, pClientServiceItems) : this.UpdateClientUsageTable(pCustomerID, pClientServiceItems);
        }

        public bool UpdatePredictions(int pCustomerID, int pLastCount)
        {
            bool flag = false;
            DateTime installDate = this.GetInstallDate(pCustomerID);
            if (pLastCount > 0L && installDate != DateTime.MinValue)
            {
                this.CalcAndSaveNextRequiredDates(pCustomerID);
            }
            else
            {
                double num = this.CalcAveConsumption(pCustomerID);
                if (num <= 0.0)
                    num = 5.0;
                DateTime date = TimeZoneUtils.Now().AddDays(20.0).Date;
                new ClientUsageTbl().Update(new ClientUsageTbl()
                {
                    CustomerID = pCustomerID,
                    LastCupCount = pLastCount,
                    NextCoffeeBy = date,
                    NextCleanOn = date.AddDays(20.0),
                    NextFilterEst = date.AddDays(30.0),
                    NextDescaleEst = date.AddDays(30.0),
                    NextServiceEst = date.AddYears(1),
                    DailyConsumption = num,
                    CleanAveCount = 200.0,
                    FilterAveCount = 300.0,
                    DescaleAveCount = 500.0,
                    ServiceAveCount = 10000.0
                });
            }
            return flag;
        }

        public bool ResetCustomerReminderCount(long pCustomerID, bool pForceEnable)
        {
            return new CustomersTbl().ResetReminderCount(pCustomerID, pForceEnable);
        }

        public bool SetClientCoffeeOnlyIfInfo(long pCustomerID)
        {
            return string.IsNullOrEmpty(new ContactType().UpdateContactTypeIfInfoOnly(pCustomerID, 3));
        }

        public class LineUsageData
        {
            private int _LastCount;
            private double _LastQty;
            private DateTime _UsageDate;

            public LineUsageData()
            {
                this._LastCount = 0;
                this._LastQty = 0.0;
                this._UsageDate = DateTime.MinValue;
            }

            public int LastCount
            {
                get => this._LastCount;
                set => this._LastCount = value;
            }

            public double LastQty
            {
                get => this._LastQty;
                set => this._LastQty = value;
            }

            public DateTime UsageDate
            {
                get => this._UsageDate;
                set => this._UsageDate = value;
            }
        }

        public class ClientServiceItem
        {
            private string _UsageDateFieldName;
            private string _UsageAveFieldName;
            private DateTime _NextUsageDate;
            private double _ThisItemsAverage;

            public ClientServiceItem()
            {
                this._UsageDateFieldName = "";
                this._UsageAveFieldName = "";
                this._NextUsageDate = DateTime.MaxValue;
                this._ThisItemsAverage = 5.0;
            }

            public string UsageDateFieldName
            {
                get => this._UsageDateFieldName;
                set => this._UsageDateFieldName = value;
            }

            public string UsageAveFieldName
            {
                get => this._UsageAveFieldName;
                set => this._UsageAveFieldName = value;
            }

            public DateTime NextUsageDate
            {
                get => this._NextUsageDate;
                set => this._NextUsageDate = value;
            }

            public double ThisItemsAverage
            {
                get => this._ThisItemsAverage;
                set => this._ThisItemsAverage = value;
            }
        }
    }
}