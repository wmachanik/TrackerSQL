using System;
using System.Collections.Generic;
using System.Data;
using TrackerSQL.Classes;
using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    public class DeliveryRepository
    {
        public List<ActiveDeliveryDate> GetActiveDeliveryDates()
        {
            var result = new List<ActiveDeliveryDate>();

            string sql = @"
                SELECT DISTINCT RequiredByDate
                FROM OrdersTbl
                WHERE Done = 0
                ORDER BY RequiredByDate";

            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql, null))
            {
                if (rdr != null)
                {
                    while (rdr.Read())
                    {
                        result.Add(new ActiveDeliveryDate
                        {
                            RequiredByDate = rdr["RequiredByDate"] == DBNull.Value
                                ? TimeZoneUtils.Now().Date
                                : Convert.ToDateTime(rdr["RequiredByDate"]).Date
                        });
                    }
                }
            }

            return result;
        }

        // OPTIONAL: if you need the second method later
        public List<ActiveDeliveryWithPerson> GetActiveDeliveryWithPerson(string sortBy = null)
        {
            var result = new List<ActiveDeliveryWithPerson>();

            string sql = @"
                SELECT DISTINCT 
                    o.RequiredByDate,
                    p.Person,
                    p.PersonID
                FROM OrdersTbl o
                LEFT JOIN PersonsTbl p ON o.ToBeDeliveredBy = p.PersonID
                WHERE o.Done = 0";

            if (!string.IsNullOrEmpty(sortBy))
            {
                sql += $" ORDER BY {sortBy}";
            }

            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql, null))
            {
                if (rdr != null)
                {
                    while (rdr.Read())
                    {
                        result.Add(new ActiveDeliveryWithPerson
                        {
                            RequiredByDate = rdr["RequiredByDate"] == DBNull.Value
                                ? TimeZoneUtils.Now().Date
                                : Convert.ToDateTime(rdr["RequiredByDate"]).Date,

                            Person = rdr["Person"] == DBNull.Value
                                ? string.Empty
                                : rdr["Person"].ToString(),

                            PersonID = rdr["PersonID"] == DBNull.Value
                                ? 0
                                : Convert.ToInt32(rdr["PersonID"])
                        });
                    }
                }
            }

            return result;
        }
    }
}