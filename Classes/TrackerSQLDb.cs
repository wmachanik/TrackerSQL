using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using TrackerSQL.Classes;

namespace TrackerSQL.Classes
{
    /// <summary>
    /// Lightweight SQL Server data access helper intended to sit alongside the existing Access-based
    /// TrackerDb class. This is not a full replacement — it provides a small set of safe helper
    /// methods to execute queries and map DBParameter objects to named SqlParameters.
    /// </summary>
    public class TrackerSQLDb : IDisposable
    {
        private readonly SqlConnection _conn;

        public TrackerSQLDb()
        {
            // Prefer an explicit SQL connection string name if present; fall back to the Access name so
            // developers can reuse their existing config key while they update Web.config.
            string connName = "TrackerDataSQL"; // new preferred connection string name
            var cs = ConfigurationManager.ConnectionStrings[connName]?.ConnectionString;
            if (string.IsNullOrWhiteSpace(cs))
            {
                // fallback to old name used by SystemConstants
                cs = ConfigurationManager.ConnectionStrings[SystemConstants.DatabaseConstants.ConnectionStringName]?.ConnectionString;
            }

            if (string.IsNullOrWhiteSpace(cs))
                throw new ConfigurationErrorsException("No SQL connection string found for TrackerData or the legacy Tracker08ConnectionString.");

            _conn = new SqlConnection(cs);
        }

        private SqlDbType ConvertToSqlDbType(DbType dbType)
        {
            switch (dbType)
            {
                case DbType.AnsiString: return SqlDbType.VarChar;
                case DbType.AnsiStringFixedLength: return SqlDbType.Char;
                case DbType.Binary: return SqlDbType.VarBinary;
                case DbType.Byte: return SqlDbType.TinyInt;
                case DbType.Boolean: return SqlDbType.Bit;
                case DbType.Currency: return SqlDbType.Money;
                case DbType.Date: return SqlDbType.Date;
                case DbType.DateTime: return SqlDbType.DateTime2;
                case DbType.Decimal: return SqlDbType.Decimal;
                case DbType.Double: return SqlDbType.Float;
                case DbType.Guid: return SqlDbType.UniqueIdentifier;
                case DbType.Int16: return SqlDbType.SmallInt;
                case DbType.Int32: return SqlDbType.Int;
                case DbType.Int64: return SqlDbType.BigInt;
                case DbType.Single: return SqlDbType.Real;
                case DbType.String: return SqlDbType.NVarChar;
                case DbType.StringFixedLength: return SqlDbType.NChar;
                case DbType.Time: return SqlDbType.Time;
                case DbType.Xml: return SqlDbType.Xml;
                default: return SqlDbType.Variant;
            }
        }

        private object PrepareValueForSql(DbType dbType, object value)
        {
            if (value == null || value == DBNull.Value)
                return DBNull.Value;

            try
            {
                switch (dbType)
                {
                    case DbType.Date:
                        var dateVal = Convert.ToDateTime(value).Date;
                        if (dateVal < new DateTime(1900, 1, 1) || dateVal > new DateTime(2079, 12, 31))
                        {
                            AppLogger.WriteLog(SystemConstants.LogTypes.Database, $"Date value {dateVal} is outside Access supported range, using minimum date");
                            return new DateTime(1900, 1, 1);
                        }
                        return dateVal;

                    case DbType.DateTime:
                        DateTime dtVal = value is DateTime dt ? dt : Convert.ToDateTime(value);
                        if (dtVal < new DateTime(1900, 1, 1) || dtVal > new DateTime(2079, 12, 31))
                        {
                            AppLogger.WriteLog(SystemConstants.LogTypes.Database, $"DateTime value {dtVal} is outside Access supported range, using minimum date");
                            dtVal = new DateTime(1900, 1, 1);
                        }
                        // FIX: Return DateTime object instead of string to match DbType.Date behavior
                        // For SQL Server we can return the full DateTime
                        return dtVal;

                    case DbType.Int16:
                        return Convert.ToInt16(value);
                    case DbType.Int32:
                        return Convert.ToInt32(value);
                    case DbType.Int64:
                        return Convert.ToInt64(value);
                    case DbType.Decimal:
                    case DbType.Currency:
                        return Convert.ToDecimal(value);
                    case DbType.Double:
                        return Convert.ToDouble(value);
                    case DbType.Single:
                        return Convert.ToSingle(value);
                    case DbType.Boolean:
                        return Convert.ToBoolean(value);
                    case DbType.Guid:
                        if (value is Guid g) return g;
                        if (Guid.TryParse(value.ToString(), out Guid parsed)) return parsed;
                        return DBNull.Value;
                    case DbType.Binary:
                        if (value is byte[] bytes)
                            return bytes;
                        else
                            return DBNull.Value;
                    default:
                        string s = Convert.ToString(value);
                        if (string.IsNullOrWhiteSpace(s)) return DBNull.Value;
                        return s;
                }
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Database, $"PrepareValueForSql failed for DbType {dbType}: {ex.Message}");
                return DBNull.Value;
            }
        }

        private SqlParameter BuildSqlParameter(DBParameter p, int index)
        {
            string paramName = p.ParamName;
            if (string.IsNullOrWhiteSpace(paramName) || paramName == DBParameter.CONST_PARAMNAMEDEFAULT)
            {
                paramName = "@p" + index.ToString();
            }
            else if (!paramName.StartsWith("@"))
            {
                paramName = "@" + paramName.TrimStart('?');
            }

            var sqlParam = new SqlParameter(paramName, ConvertToSqlDbType(p.DataDbType));
            sqlParam.Value = PrepareValueForSql(p.DataDbType, p.DataValue);
            return sqlParam;
        }

        public DataTable ReturnDataTable(string sql, List<DBParameter> parameters = null)
        {
            var dt = new DataTable();
            try
            {
                using (var cmd = new SqlCommand(sql, _conn))
                {
                    if (parameters != null)
                    {
                        for (int i = 0; i < parameters.Count; i++)
                        {
                            cmd.Parameters.Add(BuildSqlParameter(parameters[i], i));
                        }
                    }

                    using (var da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Database, $"ReturnDataTable failed: {ex.Message} SQL: {sql}");
                throw;
            }

            return dt;
        }

        public object ExecuteScalar(string sql, List<DBParameter> parameters = null)
        {
            try
            {
                using (var cmd = new SqlCommand(sql, _conn))
                {
                    if (parameters != null)
                    {
                        for (int i = 0; i < parameters.Count; i++)
                            cmd.Parameters.Add(BuildSqlParameter(parameters[i], i));
                    }

                    if (_conn.State != ConnectionState.Open) _conn.Open();
                    var res = cmd.ExecuteScalar();
                    return res == DBNull.Value ? null : res;
                }
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Database, $"ExecuteScalar failed: {ex.Message} SQL: {sql}");
                throw;
            }
            finally
            {
                if (_conn.State == ConnectionState.Open) _conn.Close();
            }
        }

        public int ExecuteNonQuery(string sql, List<DBParameter> parameters = null)
        {
            try
            {
                using (var cmd = new SqlCommand(sql, _conn))
                {
                    if (parameters != null)
                    {
                        for (int i = 0; i < parameters.Count; i++)
                            cmd.Parameters.Add(BuildSqlParameter(parameters[i], i));
                    }

                    if (_conn.State != ConnectionState.Open) _conn.Open();
                    return cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Database, $"ExecuteNonQuery failed: {ex.Message} SQL: {sql}");
                throw;
            }
            finally
            {
                if (_conn.State == ConnectionState.Open) _conn.Close();
            }
        }

        public SqlDataReader ExecuteReader(string sql, List<DBParameter> parameters = null)
        {
            try
            {
                var cmd = new SqlCommand(sql, _conn);
                if (parameters != null)
                {
                    for (int i = 0; i < parameters.Count; i++)
                        cmd.Parameters.Add(BuildSqlParameter(parameters[i], i));
                }

                if (_conn.State != ConnectionState.Open) _conn.Open();
                // Caller must close the reader which will also close the connection
                return cmd.ExecuteReader(CommandBehavior.CloseConnection);
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Database, $"ExecuteReader failed: {ex.Message} SQL: {sql}");
                // Ensure connection closed on error
                if (_conn.State == ConnectionState.Open) _conn.Close();
                throw;
            }
        }

        public void Dispose()
        {
            try
            {
                if (_conn != null)
                {
                    if (_conn.State == ConnectionState.Open) _conn.Close();
                    _conn.Dispose();
                }
            }
            catch { }
        }
    }
}
