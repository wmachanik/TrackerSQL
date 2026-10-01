using System;
using System.Collections.Generic;
using System.Data;
using TrackerSQL.Classes;
using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    /// <summary>System → Driver App settings (MobileApiSettingsTbl, one row with SettingsID = 1).</summary>
    public class MobileApiSettingsRepository
    {
        private static bool _landlineSeen;

        /// <summary>MobileApiSettingsTbl.LandlinePrefixes (SQLCommands-DriverGps-02.xml); only a positive answer is cached.</summary>
        private static bool LandlineColumnExists()
        {
            if (_landlineSeen)
                return true;
            try
            {
                using (var db = new TrackerSQLDb())
                    _landlineSeen = db.ExecuteScalar<int>(
                        "SELECT CASE WHEN COL_LENGTH(N'dbo.MobileApiSettingsTbl', N'LandlinePrefixes') IS NULL THEN 0 ELSE 1 END") == 1;
            }
            catch
            {
                _landlineSeen = false;
            }
            return _landlineSeen;
        }

        /// <summary>The saved row, or null when the settings have never been saved.</summary>
        public MobileApiSettings Get()
        {
            bool landline = LandlineColumnExists();
            string sql = @"
SELECT RunDoneOnDelivery, SendDeliveryConfirmation, AllowNotesToOffice, OfficeEmail, UpdatedAt, UpdatedBy"
                + (landline ? ", LandlinePrefixes" : string.Empty) + @"
FROM MobileApiSettingsTbl WHERE SettingsID = 1";

            using (var db = new TrackerSQLDb())
            using (var rdr = db.ExecuteReader(sql))
            {
                if (rdr == null || !rdr.Read())
                    return null;

                return new MobileApiSettings
                {
                    RunDoneOnDelivery = Convert.ToBoolean(rdr["RunDoneOnDelivery"]),
                    SendDeliveryConfirmation = Convert.ToBoolean(rdr["SendDeliveryConfirmation"]),
                    AllowNotesToOffice = Convert.ToBoolean(rdr["AllowNotesToOffice"]),
                    OfficeEmail = rdr["OfficeEmail"] == DBNull.Value ? null : rdr["OfficeEmail"].ToString(),
                    LandlinePrefixes = landline && rdr["LandlinePrefixes"] != DBNull.Value ? rdr["LandlinePrefixes"].ToString() : null,
                    UpdatedAt = Convert.ToDateTime(rdr["UpdatedAt"]),
                    UpdatedBy = rdr["UpdatedBy"] == DBNull.Value ? null : rdr["UpdatedBy"].ToString()
                };
            }
        }

        public bool Save(MobileApiSettings settings, string updatedBy)
        {
            bool landline = LandlineColumnExists();
            string sql = @"
IF EXISTS (SELECT 1 FROM MobileApiSettingsTbl WHERE SettingsID = 1)
    UPDATE MobileApiSettingsTbl
    SET RunDoneOnDelivery = @RunDone, SendDeliveryConfirmation = @SendConfirm, AllowNotesToOffice = @NotesToOffice,
        OfficeEmail = @OfficeEmail, UpdatedAt = GETDATE(), UpdatedBy = @UpdatedBy" + (landline ? ", LandlinePrefixes = @Landline" : string.Empty) + @"
    WHERE SettingsID = 1
ELSE
    INSERT INTO MobileApiSettingsTbl (SettingsID, RunDoneOnDelivery, SendDeliveryConfirmation, AllowNotesToOffice, OfficeEmail, UpdatedBy"
                + (landline ? ", LandlinePrefixes" : string.Empty) + @")
    VALUES (1, @RunDone, @SendConfirm, @NotesToOffice, @OfficeEmail, @UpdatedBy" + (landline ? ", @Landline" : string.Empty) + ")";

            string office = string.IsNullOrWhiteSpace(settings.OfficeEmail) ? null : settings.OfficeEmail.Trim();
            var ps = new List<DBParameter>
            {
                new DBParameter { ParamName = "@RunDone", DataValue = settings.RunDoneOnDelivery, DataDbType = DbType.Boolean },
                new DBParameter { ParamName = "@SendConfirm", DataValue = settings.SendDeliveryConfirmation, DataDbType = DbType.Boolean },
                new DBParameter { ParamName = "@NotesToOffice", DataValue = settings.AllowNotesToOffice, DataDbType = DbType.Boolean },
                new DBParameter { ParamName = "@OfficeEmail", DataValue = (object)office ?? DBNull.Value, DataDbType = DbType.String },
                new DBParameter { ParamName = "@UpdatedBy", DataValue = (object)updatedBy ?? DBNull.Value, DataDbType = DbType.String }
            };
            if (landline)
                ps.Add(new DBParameter { ParamName = "@Landline", DataValue = (object)settings.LandlinePrefixes ?? DBNull.Value, DataDbType = DbType.String });
            using (var db = new TrackerSQLDb())
                return db.ExecuteNonQuery(sql, ps) > 0;
        }
    }
}
