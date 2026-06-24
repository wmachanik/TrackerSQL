// Decompiled with JetBrains decompiler
// Type: TrackerSQL.classes.TrackerTools
// Assembly: TrackerSQL, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerSQL.dll

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Web;
using TrackerSQL.Managers;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

//- only form later versions #nullable disable
namespace TrackerSQL.Classes
{
    public class TrackerTools
    {
        public const string CONST_STR_NULLDATE = "1980/01/01";
        public const string CONST_SESSION_DATAACCESSERROR = "DataAccessError";
        //public const string CONST_POREQUIRED = "!!!PO required!!!"; ? SystemConstants.UIConstants.PORequiredText
        // all moved in to SystemConstants
        //public const int CONST_SERVTYPECLEAN = 1;
        //public const int CONST_SERVTYPECOFFEE = 2;
        //public const int CONST_SERVTYPECOUNT = 3;
        //public const int CONST_SERVTYPEDESCALE = 4;
        //public const int CONST_SERVTYPEFILTER = 5;
        //public const int CONST_SERVTYPESWOPCOLLECT = 6;
        //public const int CONST_SERVTYPESWOPSTART = 7;
        //public const int CONST_SERVTYPESWOPSTOP = 8;
        //public const int CONST_SERVTYPESWOPRETRUN = 9;
        //public const int CONST_SERVTYPESERVICE = 10;
        //public const int CONST_SERVTYPE1WKHOLI = 11;
        //public const int CONST_SERVTYPE2WKHOLI = 12;
        //public const int CONST_SERVTYPE3WKHOLI = 13;
        //public const int CONST_SERVTYPE1MTHHOLI = 14;
        //public const int CONST_SERVTYPE6WKHOLI = 15;
        //public const int CONST_SERVTYPE2MTHHOLI = 16 /*0x10*/;
        //public const int CONST_SERVTYPENOTAPPLICABLE = 17;
        //public const int CONST_SERVTYPEMAINTENANCE = 18;
        //public const int CONST_SERVTYPEGREENBEAN = 19;
        //public const int CONST_SERVTYPEGROUPITEM = 21;
        //public const string CONST_STRING_SERVTYPECLEAN = "1";
        //public const string CONST_STRING_SERVTYPECOFFEE = "2";
        //public const string CONST_STRING_SERVTYPECOUNT = "3";
        //public const string CONST_STRING_SERVTYPEDESCALE = "4";
        //public const string CONST_STRING_SERVTYPEFILTER = "5";
        //public const string CONST_STRING_SERVTYPESWOPCOLLECT = "6";
        //public const string CONST_STRING_SERVTYPESWOPSTART = "7";
        //public const string CONST_STRING_SERVTYPESWOPSTOP = "8";
        //public const string CONST_STRING_SERVTYPESWOPRETRUN = "9";
        //public const string CONST_STRING_SERVTYPESERVICE = "10";
        //public const string CONST_STRING_SERVTYPE1WKHOLI = "11";
        //public const string CONST_STRING_SERVTYPE2WKHOLI = "12";
        //public const string CONST_STRING_SERVTYPE3WKHOLI = "13";
        //public const string CONST_STRING_SERVTYPE1MTHHOLI = "14";
        //public const string CONST_STRING_SERVTYPE6WKHOLI = "15";
        //public const string CONST_STRING_SERVTYPE2MTHHOLI = "16";
        //public const string CONST_STRING_SERVTYPENOTAPPLICABLE = "17";
        //public const string CONST_STRING_SERVTYPEMAINTENANCE = "18";
        //public const string CONST_STRING_SERVTYPEGREENBEAN = "19";
        public const string CONST_DESC_SERVTYPECLEANSTR = "Clean";
        public const string CONST_DESC_SERVTYPECOFFEESTR = "Coffee";
        public const string CONST_DESC_SERVTYPECOUNTSTR = "Count";
        public const string CONST_DESC_SERVTYPEDESCALESTR = "Descale";
        public const string CONST_DESC_SERVTYPEFILTERSTR = "Filter";
        public const string CONST_DESC_SERVTYPESWOPCOLLECTSTR = "SwopCollect";
        public const string CONST_DESC_SERVTYPESWOPSTARTSTR = "SwopStart";
        public const string CONST_DESC_SERVTYPESWOPSTOPSTR = "SwopStop";
        public const string CONST_DESC_SERVTYPESWOPRETURNSTR = "SwopReturn";
        public const string CONST_DESC_SERVTYPESERVICESTR = "Service";
        public const string CONST_DESC_SERVTYPENOTAPPLICABLE = "N/A";
        public const int CONST_TYPICALNUMCUPSPERKG = 100;
        public const double CONST_TYPICALAVECONSUMPTION = 5.0;
        public const double CONST_TYPICALCLEAN_CONSUMPTION = 200.0;
        public const double CONST_TYPICALDECAL_CONSUMPTION = 500.0;
        public const double CONST_TYPICALFILTER_CONSUMPTION = 300.0;
        //
        //public const string CONST_DEFAULT_DELIVERYBYABBREVIATION = "SQ";  - now in SystemConstants   
        //public const int CONST_DEFAULT_DELIVERYIDOFCOURIER = 7;
        //public const string CONST_DEFAULT_DELIVERYBYCOURIERABBREVIATION = "Cour";
        // moved to sytemConstants
        //public static DateTime CONST_NULLDATE = DateTime.MinValue;
        //public static DateTime STATIC_TrackerMinDate = DateTime.Parse("1980/01/01").Date;

        public int GetDaysToPrepDate(DateTime pThisDate)
        {
            DayOfWeek pRoastDayOfWeek = DayOfWeek.Tuesday;
            if (pThisDate.DayOfWeek == DayOfWeek.Tuesday && pThisDate.Hour >= 10 || ((pThisDate.DayOfWeek == DayOfWeek.Wednesday ? 1 : 0) | (pThisDate.DayOfWeek != DayOfWeek.Thursday ? 0 : (pThisDate.Hour < 10 ? 1 : 0))) != 0)
                pRoastDayOfWeek = DayOfWeek.Thursday;
            return this.GetDaysToPrepDate(pThisDate, pRoastDayOfWeek);
        }

        public int GetDaysToPrepDate(DateTime pThisDate, DayOfWeek pRoastDayOfWeek)
        {
            DayOfWeek dayOfWeek = pThisDate.DayOfWeek;
            if (pRoastDayOfWeek < DayOfWeek.Sunday || pRoastDayOfWeek > DayOfWeek.Saturday)
                pRoastDayOfWeek = DayOfWeek.Tuesday;
            int num = pRoastDayOfWeek - dayOfWeek;
            return dayOfWeek <= pRoastDayOfWeek ? 7 + num : 14 + num;
        }
        public static DateTime ParseUserDate(string dateString)
        {
            if (string.IsNullOrWhiteSpace(dateString))
                return SystemConstants.DatabaseConstants.SystemMinDate;

            dateString = dateString.Trim();

            // Try exact match first
            if (DateTime.TryParseExact(dateString, SystemConstants.FormatConstants.DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
                return result;

            // Try parsing with time portion
            if (DateTime.TryParseExact(dateString, SystemConstants.FormatConstants.DateFormat + " HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
                return result;

            // Try general parse as fallback (not recommended for user input, but useful for debugging)
            if (DateTime.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
                return result;

            // Log and return min date if all parsing fails
            AppLogger.WriteLog(SystemConstants.LogTypes.System, $"ParseUserDate: Could not parse date string '{dateString}' with format '{SystemConstants.FormatConstants.DateFormat}'. Returning SystemMinDate.");
            return SystemConstants.DatabaseConstants.SystemMinDate;
        }
        public int NumDaysTillNextRoast() => this.GetDaysToPrepDate(TimeZoneUtils.Now().Date);

        public int NumDaysTillNextRoast(DayOfWeek pRoastDayOfWeek)
        {
            return this.GetDaysToPrepDate(TimeZoneUtils.Now().Date, pRoastDayOfWeek);
        }

        public DateTime RemoveTimePortion(DateTime pDate) => pDate.Date;

        public DateTime GetClosestNextPreperationDate(DateTime pThisDate)
        {
            return this.RemoveTimePortion(pThisDate.AddDays((double)(this.GetDaysToPrepDate(pThisDate) - 7)));
        }

        public DateTime GetClosestNextPreperationDate(DateTime pThisDate, DayOfWeek pRoastDayOfWeek)
        {
            return this.RemoveTimePortion(pThisDate.AddDays((double)(this.GetDaysToPrepDate(pThisDate, pRoastDayOfWeek) - 7)));
        }

        public bool PrepDateIsBtw(DateTime pPrepDate) => this.PrepDateIsBtw(pPrepDate, 1L);

        public bool PrepDateIsBtw(DateTime pPrepDate, long pOrderId)
        {
            DateTime closestNextPreperationDate1 = this.GetClosestNextPreperationDate(TimeZoneUtils.Now().AddDays(-7.0), DayOfWeek.Monday);
            DateTime closestNextPreperationDate2 = this.GetClosestNextPreperationDate(TimeZoneUtils.Now().Date, DayOfWeek.Monday);
            return closestNextPreperationDate1 <= pPrepDate && pPrepDate < closestNextPreperationDate2;
        }

        public bool IsNextPreperationDateByAreaTodays()
        {
            var sysData = new SysDataRepository().GetById(1);
            if (sysData?.DateLastPrepDateCalcd == null)
            {
                return false;
            }

            DateTime dateTime = TimeZoneUtils.Now().Date;
            if (dateTime.Hour >= 14)
            {
                dateTime = dateTime.AddDays(1.0);
            }

            return dateTime.Date == sysData.DateLastPrepDateCalcd.Value.Date;
        }

        private string UpdateOrInsertAreaNextPreperationDate(
          int pAreaID,
          TrackerTools.PrepAndDeliveryData pThisPrepAndDeliveryData,
          TrackerTools.PrepAndDeliveryData pNextPrepAndDeliveryData)
        {
            try
            {
                using (var db = new TrackerSQLDb())
                {
                    // First check if record exists
                    string checkSql = "SELECT AreaID FROM NextPreperationDateByAreasTbl WHERE AreaID = @AreaID";
                    var checkParams = new List<DBParameter>
                    {
                        new DBParameter 
                        { 
                            DataValue = pAreaID, 
                            DataDbType = System.Data.DbType.Int32, 
                            ParamName = "@AreaID" 
                        }
                    };
                    
                    var existingId = db.ExecuteScalar(checkSql, checkParams);
                    
                    if (existingId != null)
                    {
                        // UPDATE existing record
                        string updateSql = @"
                            UPDATE NextPreperationDateByAreasTbl 
                            SET PreperationDate = @PrepDate,
                                DeliveryDate = @DeliveryDate,
                                DeliveryOrder = @DeliveryOrder,
                                NextPreperationDate = @NextPreperationDate,
                                NextDeliveryDate = @NextDeliveryDate
                            WHERE AreaID = @AreaID";
                        
                        var updateParams = new List<DBParameter>
                        {
                            new DBParameter { DataValue = pThisPrepAndDeliveryData.PrepDate, DataDbType = System.Data.DbType.Date, ParamName = "@PrepDate" },
                            new DBParameter { DataValue = pThisPrepAndDeliveryData.DeliveryDate, DataDbType = System.Data.DbType.Date, ParamName = "@DeliveryDate" },
                            new DBParameter { DataValue = pThisPrepAndDeliveryData.SortOrder, DataDbType = System.Data.DbType.Int32, ParamName = "@DeliveryOrder" },
                            new DBParameter { DataValue = pNextPrepAndDeliveryData.PrepDate, DataDbType = System.Data.DbType.Date, ParamName = "@NextPreperationDate" },
                            new DBParameter { DataValue = pNextPrepAndDeliveryData.DeliveryDate, DataDbType = System.Data.DbType.Date, ParamName = "@NextDeliveryDate" },
                            new DBParameter { DataValue = pAreaID, DataDbType = System.Data.DbType.Int32, ParamName = "@AreaID" }
                        };
                        
                        db.ExecuteNonQuery(updateSql, updateParams);
                        return string.Empty; // Success
                    }
                    else
                    {
                        // INSERT new record
                        string insertSql = @"
                            INSERT INTO NextPreperationDateByAreasTbl 
                            (AreaID, PreperationDate, DeliveryDate, DeliveryOrder, NextPreperationDate, NextDeliveryDate)
                            VALUES 
                            (@AreaID, @PrepDate, @DeliveryDate, @DeliveryOrder, @NextPreperationDate, @NextDeliveryDate)";
                        
                        var insertParams = new List<DBParameter>
                        {
                            new DBParameter { DataValue = pAreaID, DataDbType = System.Data.DbType.Int32, ParamName = "@AreaID" },
                            new DBParameter { DataValue = pThisPrepAndDeliveryData.PrepDate, DataDbType = System.Data.DbType.Date, ParamName = "@PrepDate" },
                            new DBParameter { DataValue = pThisPrepAndDeliveryData.DeliveryDate, DataDbType = System.Data.DbType.Date, ParamName = "@DeliveryDate" },
                            new DBParameter { DataValue = pThisPrepAndDeliveryData.SortOrder, DataDbType = System.Data.DbType.Int32, ParamName = "@DeliveryOrder" },
                            new DBParameter { DataValue = pNextPrepAndDeliveryData.PrepDate, DataDbType = System.Data.DbType.Date, ParamName = "@NextPreperationDate" },
                            new DBParameter { DataValue = pNextPrepAndDeliveryData.DeliveryDate, DataDbType = System.Data.DbType.Date, ParamName = "@NextDeliveryDate" }
                        };
                        
                        db.ExecuteNonQuery(insertSql, insertParams);
                        return string.Empty; // Success
                    }
                }
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Database, 
                    $"UpdateOrInsertAreaNextPreperationDate error for AreaID {pAreaID}: {ex.Message}");
                return ex.Message;
            }
        }

        private byte GetCorrectedDOW(byte pDOW) => pDOW == (byte)0 ? (byte)1 : (byte)((int)pDOW - 1);

        private TrackerTools.PrepAndDeliveryData GetPreAndDeliveryDate(
          int pIdx,
          int pAreaID,
          List<AreaPrepDays> pAreaPrepDays,
          DateTime pForThisDate)
        {
            TrackerTools.PrepAndDeliveryData preAndDeliveryDate = new TrackerTools.PrepAndDeliveryData();
            byte _ThisDatesDOW = (byte)pForThisDate.DayOfWeek;
            int index1 = pAreaPrepDays.FindIndex(pIdx, x => x.AreaID == pAreaID);
            if (index1 > -1)
            {
                byte correctedDow = this.GetCorrectedDOW((byte)(pAreaPrepDays[index1].PrepDayOfWeekID ?? 0));
                int deliveryDelayDays = pAreaPrepDays[index1].DeliveryDelayDays ?? 0;
                int deliveryOrder = pAreaPrepDays[index1].DeliveryOrder ?? 0;
                int index2 = pAreaPrepDays.FindIndex(index1, x => x.AreaID != pAreaID);
                if (index2 > -1)
                {
                    int index3 = pAreaPrepDays.FindIndex(index1, index2 - index1, x => (int)this.GetCorrectedDOW((byte)(x.PrepDayOfWeekID ?? 0)) >= (int)_ThisDatesDOW);
                    if (index3 > -1)
                    {
                        correctedDow = this.GetCorrectedDOW((byte)(pAreaPrepDays[index3].PrepDayOfWeekID ?? 0));
                        deliveryDelayDays = pAreaPrepDays[index3].DeliveryDelayDays ?? 0;
                        deliveryOrder = pAreaPrepDays[index3].DeliveryOrder ?? 0;
                    }
                }
                preAndDeliveryDate.PrepDate = (int)correctedDow < (int)_ThisDatesDOW ? pForThisDate.AddDays((double)(7 - (int)_ThisDatesDOW + (int)correctedDow)) : pForThisDate.AddDays((double)((int)correctedDow - (int)_ThisDatesDOW));
                preAndDeliveryDate.DeliveryDate = preAndDeliveryDate.PrepDate.AddDays((double)deliveryDelayDays);
                preAndDeliveryDate.SortOrder = deliveryOrder;
            }
            return preAndDeliveryDate;
        }
        public int SetNextPreperationDateByArea()
        {
            List<AreaPrepDays> all = new AreaPrepDaysRepository().GetAll("AreaID, PrepDayOfWeekID");
            if (all.Count == 0)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Orders,
                    "SetNextPreperationDateByArea: no rows in AreaPrepDaysTbl — cannot calculate dates.");
                return 0;
            }

            var now = TimeZoneUtils.Now();
            DateTime anchorDate = now.Hour >= 14 ? now.Date.AddDays(1) : now.Date;

            var AreaStartIndex = new Dictionary<int, int>();
            for (int i = 0; i < all.Count; i++)
            {
                int AreaId = all[i].AreaID;
                if (!AreaStartIndex.ContainsKey(AreaId))
                    AreaStartIndex[AreaId] = i;
            }

            int windowDays = ConfigHelper.GetInt("CoffeeCheckupReminderWindowDays", 9);
            if (windowDays <= 0) windowDays = 9;

            var closureProvider = new HolidayClosureProvider();
            bool holidayInWindow = false;
            try
            {
                holidayInWindow = closureProvider.IsThereAHolodayComing(TimeZoneUtils.Now().Date, windowDays);
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Orders,
                    "SetNextPreperationDateByArea: holiday check skipped: " + ex.Message);
            }

            int areasUpdated = 0;
            foreach (var kvp in AreaStartIndex.OrderBy(k => k.Key))
            {
                int AreaId = kvp.Key;
                if (AreaId <= 0)
                    continue;

                int startIdx = kvp.Value;
                var thisPair = this.GetPreAndDeliveryDate(startIdx, AreaId, all, anchorDate);
                DateTime nextAnchor = (thisPair.PrepDate == thisPair.DeliveryDate)
                    ? thisPair.PrepDate.AddDays(1).Date
                    : thisPair.DeliveryDate.Date;
                var nextPair = this.GetPreAndDeliveryDate(startIdx, AreaId, all, nextAnchor);

                if (holidayInWindow)
                    closureProvider = HandleHolidayClosures(closureProvider, thisPair, nextPair);

                if (!HasValidPrepDates(thisPair) || !HasValidPrepDates(nextPair))
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.Orders,
                        $"SetNextPreperationDateByArea: skipping AreaID {AreaId} — could not calculate prep/delivery dates.");
                    continue;
                }

                string updateResult = this.UpdateOrInsertAreaNextPreperationDate(AreaId, thisPair, nextPair);
                if (!string.IsNullOrEmpty(updateResult))
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.Orders,
                        $"SetNextPreperationDateByArea: AreaID {AreaId} update failed: {updateResult}");
                    continue;
                }

                areasUpdated++;
            }

            using (var trackerSQLDb = new TrackerSQLDb())
            {
                trackerSQLDb.ExecuteNonQuery(
                    "UPDATE SysDataTbl SET DateLastPrepDateCalcd = @DateLastPrepDateCalcd WHERE ID = 1",
                    new List<DBParameter>
                    {
                        new DBParameter
                        {
                            ParamName = "@DateLastPrepDateCalcd",
                            DataValue = TimeZoneUtils.Now().Date,
                            DataDbType = DbType.Date
                        }
                    });
            }

            AppLogger.WriteLog(SystemConstants.LogTypes.Orders,
                $"SetNextPreperationDateByArea: updated {areasUpdated} of {AreaStartIndex.Count} areas from {all.Count} prep-day rows.");

            return areasUpdated;
        }

        private static bool HasValidPrepDates(TrackerTools.PrepAndDeliveryData pair)
        {
            return pair != null
                && pair.PrepDate > DateTime.MinValue
                && pair.DeliveryDate > DateTime.MinValue;
        }

        private static HolidayClosureProvider HandleHolidayClosures(HolidayClosureProvider closureProvider, PrepAndDeliveryData thisPair, PrepAndDeliveryData nextPair)
        {
            // Adjust current pair only if dates land on closures
            if (closureProvider.IsClosed(thisPair.PrepDate, true) || closureProvider.IsClosed(thisPair.DeliveryDate, false))
            {
                var adj = closureProvider.AdjustPair(thisPair.PrepDate, thisPair.DeliveryDate);
                if (adj.WasAdjusted)
                {
                    thisPair.PrepDate = adj.Prep;
                    thisPair.DeliveryDate = adj.Delivery;
                }
            }

            // Adjust next pair only if dates land on closures
            if (closureProvider.IsClosed(nextPair.PrepDate, true) || closureProvider.IsClosed(nextPair.DeliveryDate, false))
            {
                var adj2 = closureProvider.AdjustPair(nextPair.PrepDate, nextPair.DeliveryDate);
                if (adj2.WasAdjusted)
                {
                    nextPair.PrepDate = adj2.Prep;
                    nextPair.DeliveryDate = adj2.Delivery;
                }
            }
            return closureProvider;
        }

        /*
public void SetNextPreperationDateByArea()
{
   List<AreaPrepDays> all = new AreaPrepDaysRepository().GetAll("AreaID, PrepDayOfWeekID");
   DateTime minValue = DateTime.MinValue;
   TrackerTools.PrepAndDeliveryData prepAndDeliveryData1 = new TrackerTools.PrepAndDeliveryData();
   TrackerTools.PrepAndDeliveryData prepAndDeliveryData2 = new TrackerTools.PrepAndDeliveryData();
   DateTime pForThisDate1 = TimeZoneUtils.Now().Date;
   if (pForThisDate1.Hour >= 14)
       pForThisDate1 = pForThisDate1.AddDays(1.0);
   int num = 0;
label_6:
   while (num < all.Count)
   {
       int AreaId = all[num].AreaID;
       TrackerTools.PrepAndDeliveryData preAndDeliveryDate1 = this.GetPreAndDeliveryDate(num, AreaId, all, pForThisDate1);
       DateTime pForThisDate2 = preAndDeliveryDate1.PrepDate == preAndDeliveryDate1.DeliveryDate ? preAndDeliveryDate1.PrepDate.AddDays(1.0).Date : preAndDeliveryDate1.DeliveryDate.Date;
       TrackerTools.PrepAndDeliveryData preAndDeliveryDate2 = this.GetPreAndDeliveryDate(num, AreaId, all, pForThisDate2);
       this.UpdateOrInsertAreaNextPreperationDate(AreaId, preAndDeliveryDate1, preAndDeliveryDate2);
       ++num;
       while (true)
       {
           if (num < all.Count && AreaId == all[num].AreaID)
               ++num;
           else
               goto label_6;
       }
   }
   TrackerDb trackerDb = new TrackerDb();
   trackerDb.ExecuteNonQuerySQLWithParams("UPDATE SysDataTbl SET DateLastPrepDateCalcd = ? WHERE ID=1", new List<DBParameter>()
   {
       new DBParameter()
       {
           DataValue = (object) TimeZoneUtils.Now().Date,
           DataDbType = DbType.Date
       }
   });
   trackerDb.Close();
}
*/
        public DateTime GetNextPreperationDateByCustomerID(long pCustID, ref DateTime pDelivery)
        {
            if (!this.IsNextPreperationDateByAreaTodays())
                this.SetNextPreperationDateByArea();

            var prepDataForCustomer = LoadPrepDataForContact(pCustID);
            if (!HasValidPrepDates(prepDataForCustomer))
            {
                this.SetNextPreperationDateByArea();
                prepDataForCustomer = LoadPrepDataForContact(pCustID);
            }

            pDelivery = prepDataForCustomer.DeliveryDate ?? DateTime.MinValue;
            return prepDataForCustomer.PreperationDate ?? DateTime.MinValue;
        }

        private static NextPreperationDateByArea LoadPrepDataForContact(long contactId)
        {
            return new NextPrepDateByAreaRepository().GetPrepDataForContact((int)contactId);
        }

        private static bool HasValidPrepDates(NextPreperationDateByArea prepData)
        {
            return prepData != null
                && prepData.PreperationDate.HasValue
                && prepData.PreperationDate.Value > DateTime.MinValue
                && prepData.DeliveryDate.HasValue
                && prepData.DeliveryDate.Value > DateTime.MinValue;
        }

        public TrackerTools.ContactPreferedItems RetrieveCustomerPrefs(long custId)
        {
            var contactPreferedItems = new TrackerTools.ContactPreferedItems(custId);
            const string sql = @"
                SELECT c.PreferredAgentID, c.ItemPrefID, c.PriPrefQty, c.PrefItemPackagingID, a.RequiresPurchOrder
                FROM ContactsTbl c
                LEFT OUTER JOIN ContactsAccInfoTbl a ON c.ContactID = a.ContactID
                WHERE c.ContactID = @ContactID";

            var parameters = new List<DBParameter>
            {
                new DBParameter { ParamName = "@ContactID", DataValue = custId, DataDbType = DbType.Int64 }
            };

            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql, parameters))
            {
                if (rdr != null && rdr.Read())
                {
                    int preferredAgentId = rdr["PreferredAgentID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["PreferredAgentID"]);
                    contactPreferedItems.PreferredDeliveryByID = preferredAgentId > 0
                        ? preferredAgentId
                        : SystemConstants.DeliveryConstants.DefaultDeliveryPersonID;
                    contactPreferedItems.PreferedItem = rdr["ItemPrefID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["ItemPrefID"]);
                    contactPreferedItems.PreferedQty = rdr["PriPrefQty"] == DBNull.Value ? 1.0 : Convert.ToDouble(rdr["PriPrefQty"]);
                    contactPreferedItems.PrefPackagingID = rdr["PrefItemPackagingID"] == DBNull.Value ? 0 : Convert.ToInt32(rdr["PrefItemPackagingID"]);
                    contactPreferedItems.RequiresPurchOrder = rdr["RequiresPurchOrder"] != DBNull.Value && Convert.ToBoolean(rdr["RequiresPurchOrder"]);
                }
            }

            return contactPreferedItems;
        }

        public void SetTrackerSessionErrorString(string pErrorString)
        {
            HttpContext current = HttpContext.Current;
            if (current == null || current.Session == null)
                return;
            current.Session["DataAccessError"] = (object)pErrorString;
        }

        public void ClearTrackerSessionErrorString()
        {
            HttpContext current = HttpContext.Current;
            if (current == null || current.Session == null)
                return;
            current.Session["DataAccessError"] = (object)string.Empty;
        }

        public string GetTrackerSessionErrorString()
        {
            HttpContext current = HttpContext.Current;
            string sessionErrorString = string.Empty;
            if (current != null && current.Session != null)
                sessionErrorString = current.Session["DataAccessError"] != null ? (string)current.Session["DataAccessError"] : string.Empty;
            return sessionErrorString;
        }

        public bool IsTrackerSessionErrorString()
        {
            HttpContext current = HttpContext.Current;
            return !string.IsNullOrWhiteSpace(current.Session["DataAccessError"] != null ? (string)current.Session["DataAccessError"] : string.Empty);
        }
        public static string SafeString(string value, string defaultValue = "n/a")
        {
            return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
        }
        public int ChangeItemIfGroupToNextItemInGroup(
          long pContactID,
          int pItemTypeID,
          DateTime pDeliveryDate)
        {
            if (pContactID == SystemConstants.DatabaseConstants.InvalidID || pItemTypeID == SystemConstants.DatabaseConstants.InvalidID)
            {
                return pItemTypeID;
            }

            var groupServiceTypeId = new SysDataRepository().GetGroupItemServiceTypeId();
            var itemServiceTypeId = new ItemsRepository().GetItemServiceTypeId(pItemTypeID);
            if (itemServiceTypeId.HasValue && groupServiceTypeId.HasValue && itemServiceTypeId.Value == groupServiceTypeId.Value)
            {
                return new UsedItemGroupRepository().GetNextGroupItemId(pContactID, pItemTypeID, pDeliveryDate);
            }

            return pItemTypeID;
        }
        public static DateTime? ConvertToNullableDateTime(object dateObj)
        {
            if (dateObj == null || dateObj == DBNull.Value)
                return null;

            if (dateObj is DateTime dt)
                return dt;

            if (DateTime.TryParse(dateObj.ToString(), out DateTime parsed))
                return parsed;

            return null;
        }
        public enum ServiceType
        {
            stNone,
            STypeClean,
            STypeCoffee,
            STypeCount,
            STypeDescale,
            STypeFilter,
            STypeSwopCollect,
            STypeSwopStart,
            STypeSwopStop,
            STypeSwopRetrun,
            STypeService,
            SType1WkHoli,
            SType2WkHoli,
            SType3WkHoli,
            SType1MthHoli,
            SType6WkHoli,
            SType2MthHoli,
        }

        public class ContactPreferedItems
        {
            private long _CustID;
            private int _PreferredDeliveryByID;
            private int _PreferedItem;
            private double _PreferedQty;
            private int _PrefPackagingID;
            private bool _RequiresPurchOrder;

            public ContactPreferedItems(long pCustID)
            {
                this._CustID = pCustID;
                this._PreferredDeliveryByID = 3;
                this._PreferedItem = 0;
                this._RequiresPurchOrder = false;
                this._PreferedQty = 1.0;
                this._PrefPackagingID = SystemConstants.DatabaseConstants.InvalidID;
            }

            public long CustID
            {
                get => this._CustID;
                set => this._CustID = value;
            }

            public int PreferredDeliveryByID
            {
                get => this._PreferredDeliveryByID;
                set => this._PreferredDeliveryByID = value;
            }

            public int PreferedItem
            {
                get => this._PreferedItem;
                set => this._PreferedItem = value;
            }

            public double PreferedQty
            {
                get => this._PreferedQty;
                set => this._PreferedQty = value;
            }

            public int PrefPackagingID
            {
                get => this._PrefPackagingID;
                set => this._PrefPackagingID = value;
            }

            public bool RequiresPurchOrder
            {
                get => this._RequiresPurchOrder;
                set => this._RequiresPurchOrder = value;
            }
        }

        public class PrepAndDeliveryData
        {
            private DateTime _PrepDate;
            private DateTime _DeliveryDate;
            private int _SortOrder;

            public PrepAndDeliveryData()
            {
                this._PrepDate = this._DeliveryDate = DateTime.MinValue;
                this._SortOrder = 0;
            }

            public DateTime PrepDate
            {
                get => this._PrepDate;
                set => this._PrepDate = value;
            }

            public DateTime DeliveryDate
            {
                get => this._DeliveryDate;
                set => this._DeliveryDate = value;
            }

            public int SortOrder
            {
                get => this._SortOrder;
                set => this._SortOrder = value;
            }
        }
        ///// <summary>
        ///// Modern SQL Server version of SetNextPreperationDateByArea
        ///// Returns count of areas processed
        ///// </summary>
        //public int SetNextPreperationDateByAreaModern()
        //{
        //    try
        //    {
        //        // Load all rows using modern SQL instead of legacy AreaPrepDaysTbl
        //        List<AreaPrepDaysTbl> all = GetAreaPrepDaysFromSQL();
                
        //        if (all == null || all.Count == 0)
        //        {
        //            AppLogger.WriteLog(SystemConstants.LogTypes.System, 
        //                "TrackerTools.SetNextPreperationDateByAreaModern: No area prep days found");
        //            return 0;
        //        }

        //        // Anchor date: if now >= 14:00, use tomorrow; else today
        //        var now = TimeZoneUtils.Now();
        //        DateTime anchorDate = now.Hour >= 14 ? now.Date.AddDays(1) : now.Date;

        //        // Determine the first index of each AreaID in the 'all' list
        //        var AreaStartIndex = new Dictionary<int, int>();
        //        for (int i = 0; i < all.Count; i++)
        //        {
        //            int AreaId = all[i].AreaID;
        //            if (!AreaStartIndex.ContainsKey(AreaId))
        //                AreaStartIndex[AreaId] = i;
        //        }

        //        // Read holiday window
        //        int windowDays = ConfigHelper.GetInt("CoffeeCheckupReminderWindowDays", 9);
        //        if (windowDays <= 0) windowDays = 9;

        //        var closureProvider = new HolidayClosureProvider();
        //        bool holidayInWindow = closureProvider.IsThereAHolodayComing(TimeZoneUtils.Now().Date, windowDays);

        //        // Process each Area once
        //        int areasProcessed = 0;
        //        foreach (var kvp in AreaStartIndex.OrderBy(k => k.Key))
        //        {
        //            int AreaId = kvp.Key;
        //            int startIdx = kvp.Value;

        //            // Current window (based on anchorDate)
        //            var thisPair = this.GetPreAndDeliveryDate(startIdx, AreaId, all, anchorDate);

        //            // Next window starts the day after the current delivery
        //            DateTime nextAnchor = (thisPair.PrepDate == thisPair.DeliveryDate)
        //                ? thisPair.PrepDate.AddDays(1).Date
        //                : thisPair.DeliveryDate.Date;

        //            var nextPair = this.GetPreAndDeliveryDate(startIdx, AreaId, all, nextAnchor);

        //            if (holidayInWindow)
        //            {
        //                closureProvider = HandleHolidayClosures(closureProvider, thisPair, nextPair);
        //            }

        //            // Persist for this Area
        //            this.UpdateOrInsertAreaNextPreperationDate(AreaId, thisPair, nextPair);
        //            areasProcessed++;
        //        }

        //        // Mark last calculated date using modern SQL
        //        UpdateSysDataLastPrepDateCalculated();

        //        AppLogger.WriteLog(SystemConstants.LogTypes.System, 
        //            $"TrackerTools.SetNextPreperationDateByAreaModern: Processed {areasProcessed} areas");
                
        //        return areasProcessed;
        //    }
        //    catch (Exception ex)
        //    {
        //        AppLogger.WriteLog(SystemConstants.LogTypes.System, 
        //            $"TrackerTools.SetNextPreperationDateByAreaModern error: {ex.Message}");
        //        throw;
        //    }
        //}

        /// <summary>
        /// Get all area prep days from SQL Server instead of legacy Controls class
        /// </summary>
        private List<AreaPrepDays> GetAreaPrepDaysFromSQL()
        {
            return new AreaPrepDaysRepository().GetAll("AreaID, PrepDayOfWeekID");
        }

        /// <summary>
        /// Update SysData last prep date calculated using modern SQL
        /// </summary>
        private void UpdateSysDataLastPrepDateCalculated()
        {
            try
            {
                using (var db = new TrackerSQLDb())
                {
                    string sql = "UPDATE SysDataTbl SET DateLastPrepDateCalcd = @Date WHERE ID = 1";
                    var parameters = new List<DBParameter>
                    {
                        new DBParameter 
                        { 
                            DataValue = TimeZoneUtils.Now().Date, 
                            DataDbType = System.Data.DbType.Date, 
                            ParamName = "@Date" 
                        }
                    };
                    
                    db.ExecuteNonQuery(sql, parameters);
                }
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Database, 
                    $"UpdateSysDataLastPrepDateCalculated error: {ex.Message}");
                throw;
            }
        }
    }
}
