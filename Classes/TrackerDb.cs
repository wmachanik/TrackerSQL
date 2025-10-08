// Decompiled with JetBrains decompiler
// Type: TrackerDotNet.classes.TrackerDb
// Assembly: TrackerDotNet, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 2B5ACBFB-45EE-46B9-81D2-DBD1194F39CE
// Assembly location: C:\SRC\Apps\qtracker\bin\TrackerDotNet.dll

using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Configuration;

//- only form later versions #nullable disable
namespace TrackerDotNet.Classes
{
    public class TrackerDb : IDisposable
    {
        /*/ Moved to SystemConstants:
        public const string CONST_CONSTRING = "Tracker08ConnectionString"; → SystemConstants.DatabaseConstants.ConnectionStringName
        public const int CONST_INVALIDID = -1;                             → SystemConstants.DatabaseConstants.InvalidID
        public const string CONST_INVALIDIDSTR = "-1";                     → SystemConstants.DatabaseConstants.InvalidIDStr
        */
        public const string SQLTABLENAME_LOGTBL = "LogTbl";
        public const string SQLTABLENAME_SECTIONTYPESTBL = "SectionTypesTbl";
        public const string SQLTABLENAME_TRANSACTIONTYPESTBL = "TransactionTypesTbl";
        private const string CONST_SQL_CREATE_LOGTBL = "CREATE TABLE LogTbl ( [LogID] AUTOINCREMENT, [DateAdded] DateTime, [UserID] INT, [SectionID] INT, [TranactionTypeID] INT,  [CustomerID] INT, Details VARCHAR(255), [Notes] MEMO,  CONSTRAINT [pk_LogID] PRIMARY KEY (LogID) )";
        private const string CONST_SQL_CREATE_SECTIONTYPETBL = "CREATE TABLE SectionTypesTbl ( [SectionID] INT, [SectionType] VARCHAR(50), [Notes] MEMO,  CONSTRAINT [pk_SectionID] PRIMARY KEY (SectionID) )";
        private const string CONST_SQL_CREATE_TRANSACTIONTYPETBL = "CREATE TABLE TransactionTypesTbl ( [TransactionID] INT, [TransactionType] VARCHAR(50), [Notes] MEMO,  CONSTRAINT [pk_TransactionID] PRIMARY KEY (TransactionID) )";
        private List<DBParameter> _Params;
        private List<DBParameter> _WhereParams;
        private OleDbConnection _TrackerDbConn;
        private int _numRecs;
        private OleDbCommand _command;



        public TrackerDb()
        {
            this._TrackerDbConn = (OleDbConnection)null;
            this._command = (OleDbCommand)null;
            this.Initialize();
            this._Params = new List<DBParameter>();
            this._WhereParams = new List<DBParameter>();
            this._numRecs = 0;
        }

        public List<DBParameter> Params
        {
            get => this._Params;
            set => this._Params = value;
        }

        public List<DBParameter> WhereParams
        {
            get => this._WhereParams;
            set => this._WhereParams = value;
        }

        public OleDbConnection TrackerDbConn
        {
            get => this._TrackerDbConn;
            set => this._TrackerDbConn = value;
        }

        public int numRecs
        {
            get => this._numRecs;
            set => this._numRecs = value;
        }

        public string ErrorResult
        {
            get => new TrackerTools().GetTrackerSessionErrorString();
            set => new TrackerTools().SetTrackerSessionErrorString(value);
        }

        private object PrepareValueForOleDb(DbType dbType, object value)
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
                        return dtVal;

                    case DbType.Int16:
                        short shortVal = Convert.ToInt16(value);
                        return shortVal;

                    case DbType.Int32:
                        int intVal = Convert.ToInt32(value);
                        return intVal;

                    case DbType.Int64:
                        long longVal = Convert.ToInt64(value);
                        if (longVal <= int.MaxValue && longVal >= int.MinValue)
                        {
                            return (int)longVal;
                        }
                        // Only log if this is actually problematic (which it shouldn't be for normal customer IDs)
                        if (EnableDetailedLogging)
                        {
                            AppLogger.WriteLog(SystemConstants.LogTypes.Database, $"Long value {longVal} exceeds int range, keeping as long");
                        }
                        return longVal;

                    case DbType.String:
                    case DbType.AnsiString:
                    case DbType.AnsiStringFixedLength:
                    case DbType.StringFixedLength:
                    case DbType.Xml:
                        string strVal = Convert.ToString(value);
                        if (string.IsNullOrWhiteSpace(strVal))
                            return DBNull.Value;

                        // Access has a 255 character limit for many string fields
                        if (strVal.Length > 255)
                        {
                            AppLogger.WriteLog(SystemConstants.LogTypes.Database, $"String value truncated from {strVal.Length} to 255 characters");
                            strVal = strVal.Substring(0, 255);
                        }
                        return strVal;

                    case DbType.Guid:
                        Guid guidResult;
                        if (Guid.TryParse(value.ToString(), out guidResult))
                            return guidResult;
                        return DBNull.Value;

                    case DbType.Byte:
                        return Convert.ToByte(value);

                    case DbType.Binary:
                        if (value is byte[] bytes)
                            return bytes;
                        else
                            return DBNull.Value;

                    default:
                        return value;
                }
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Database, $"Parameter conversion failed for DbType {dbType}, value '{value}' (Type: {value?.GetType().Name}): {ex.Message}");
                return DBNull.Value;
            }
        }
        /// <summary>
        /// Normalizes DateTime parameters for Access database compatibility
        /// </summary>
        private DbType NormalizeDateTypeForAccess(DbType originalType)
        {
            // For Access compatibility, always use DbType.Date for date comparisons
            if (originalType == DbType.Date || originalType == DbType.DateTime2 || originalType == DbType.DateTimeOffset)
            {
                return DbType.DateTime;
            }
            return originalType;
        }
        private OleDbParameter BuildOleDbParameter(DBParameter param)
        {
            OleDbParameter oleParam = new OleDbParameter();

            // Normalize date types for Access compatibility
            var normalizedType = NormalizeDateTypeForAccess(param.DataDbType);

            oleParam.Value = PrepareValueForOleDb(normalizedType, param.DataValue);

            if (param.ParamName != null && !param.ParamName.Equals("?"))
            {
                oleParam.ParameterName = param.ParamName;
                oleParam.DbType = normalizedType;
            }
            else
            {
                oleParam.OleDbType = ConvertToOleDbType(normalizedType);
            }

            return oleParam;
        }
        //private object ConvertToOleDbValue(DbType dbType, object value)
        //{
        //    if (value == null || value == DBNull.Value)
        //        return DBNull.Value;

        //    try
        //    {
        //        switch (dbType)
        //        {
        //            case DbType.Date:
        //            case DbType.DateTime:
        //                if (value is DateTime dt)
        //                    return dt.Date;
        //                return Convert.ToDateTime(value).Date;

        //            case DbType.Int16:
        //                return Convert.ToInt16(value);

        //            case DbType.Int32:
        //                return Convert.ToInt32(value);

        //            case DbType.Int64:
        //                long myValue = Convert.ToInt64(value); // your logic
        //                if (myValue <= int.MaxValue && myValue >= int.MinValue)
        //                {
        //                    return (int)myValue;
        //                }
        //                else
        //                {
        //                    // Log to file, event log, or debugging trace
        //                    System.Diagnostics.Trace.WriteLine($"Conversion failed: long to int for Access database");
        //                    return -1; // DBNull.Value;
        //                }

        //            case DbType.Decimal:
        //            case DbType.Currency:
        //                return Convert.ToDecimal(value);

        //            case DbType.Double:
        //                return Convert.ToDouble(value);

        //            case DbType.Single:
        //                return Convert.ToSingle(value);

        //            case DbType.Boolean:
        //                return Convert.ToBoolean(value);

        //            case DbType.String:
        //            case DbType.AnsiString:
        //            case DbType.AnsiStringFixedLength:
        //            case DbType.StringFixedLength:
        //                string strVal = Convert.ToString(value);
        //                if (string.IsNullOrWhiteSpace(strVal))
        //                    return DBNull.Value;
        //                else
        //                    return strVal;


        //            case DbType.Guid:
        //                if (Guid.TryParse(value.ToString(), out Guid result))
        //                    return result;
        //                else
        //                    return DBNull.Value;

        //            case DbType.Byte:
        //                return Convert.ToByte(value);

        //            case DbType.Binary:
        //                if (value is byte[] bytes)
        //                    return bytes;
        //                else
        //                    return DBNull.Value;


        //            default:
        //                return value;
        //        }
        //    }
        //    catch
        //    {
        //        return DBNull.Value; // fallback to avoid crashing
        //    }
        //}

        private OleDbType ConvertToOleDbType(DbType pDbType)
        {
            switch (pDbType)
            {
                case DbType.AnsiString:
                case DbType.AnsiStringFixedLength:
                    return OleDbType.Char;
                case DbType.Binary:
                    return OleDbType.Binary;
                case DbType.Byte:
                    return OleDbType.UnsignedTinyInt;
                case DbType.Boolean:
                    return OleDbType.Boolean;
                case DbType.Currency:
                    return OleDbType.Currency;
                case DbType.Date:
                    return OleDbType.Date;
                case DbType.DateTime:
                case DbType.DateTime2:
                case DbType.DateTimeOffset:
                    return OleDbType.Date;
                case DbType.Decimal:
                    return OleDbType.Decimal;
                case DbType.Double:
                    return OleDbType.Double;
                case DbType.Guid:
                    return OleDbType.Guid;
                case DbType.Int16:
                    return OleDbType.SmallInt;
                case DbType.Int32:
                    return OleDbType.Integer;
                case DbType.Int64:
                    // Use Integer for Access compatibility, but let PrepareValueForOleDb handle the conversion
                    return OleDbType.Integer;
                case DbType.Object:
                    return OleDbType.IDispatch;
                case DbType.SByte:
                    return OleDbType.TinyInt;
                case DbType.Single:
                    return OleDbType.Single;
                case DbType.String:
                    return OleDbType.VarChar;
                case DbType.Time:
                    return OleDbType.DBTime;
                case DbType.UInt16:
                    return OleDbType.UnsignedSmallInt;
                case DbType.UInt32:
                    return OleDbType.UnsignedInt;
                case DbType.UInt64:
                    return OleDbType.UnsignedBigInt;
                case DbType.VarNumeric:
                    return OleDbType.Decimal;
                case DbType.StringFixedLength:
                    return OleDbType.Char;
                case DbType.Xml:
                    return OleDbType.Char;
                default:
                    return OleDbType.IUnknown;
            }
        }

        private void Initialize() => this.Open();

        private bool ValidateConnection()
        {
            try
            {
                if (this._TrackerDbConn == null)
                {
                    // Only log if detailed logging is enabled
                    if (EnableDetailedLogging)
                    {
                        AppLogger.WriteLog(SystemConstants.LogTypes.Database, "Connection is null, reinitializing...");
                    }
                    this.Initialize();
                    return this._TrackerDbConn != null;
                }

                if (this._TrackerDbConn.State == ConnectionState.Broken)
                {
                    // Always log broken connections as these are problems
                    AppLogger.WriteLog(SystemConstants.LogTypes.Database, "Connection is broken, recreating...");
                    this._TrackerDbConn.Close();
                    this._TrackerDbConn.Dispose();
                    this.Initialize();
                    return this._TrackerDbConn?.State == ConnectionState.Closed;
                }

                return true;
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Database, $"Connection validation failed: {ex.Message}");
                return false;
            }
        }

        public void Open()
        {
            try
            {
                if (this._TrackerDbConn != null)
                {
                    if (this._TrackerDbConn.State == ConnectionState.Open)
                        this._TrackerDbConn.Close();
                    this._TrackerDbConn.Dispose();
                    this._TrackerDbConn = null;
                }

                string connectionString = ConfigurationManager.ConnectionStrings[SystemConstants.DatabaseConstants.ConnectionStringName]?.ConnectionString;
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    throw new ConfigurationErrorsException("Connection string 'Tracker08ConnectionString' is missing or empty in configuration.");
                }

                this._TrackerDbConn = new OleDbConnection(connectionString);

                // Test the connection
                this._TrackerDbConn.Open();
                this._TrackerDbConn.Close();

                // Remove this line - only log on errors
                // AppLogger.WriteLog(SystemConstants.LogTypes.Database, "Database connection initialized successfully");
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Database, $"Failed to initialize database connection: {ex.Message}");
                throw new Exception($"Database connection failed: {ex.Message}", ex);
            }
        }

        // Helper method to combine parameter lists for error logging
        private List<DBParameter> CombineParameters(List<DBParameter> pParams, List<DBParameter> pWhereParams)
        {
            var combined = new List<DBParameter>();

            if (pParams != null)
                combined.AddRange(pParams);

            if (pWhereParams != null)
                combined.AddRange(pWhereParams);

            return combined.Count > 0 ? combined : null;
        }
        public bool TableExists(string pTableName)
        {
            bool flag = false;
            this._TrackerDbConn.Open();
            try
            {
                flag = this._TrackerDbConn.GetSchema("Tables", new string[3]
                {
        null,
        null,
        pTableName
                }).Rows.Count != 0;
            }
            catch (OleDbException ex)
            {
                this.ErrorResult = ex.Message;
            }
            finally
            {
                this._TrackerDbConn.Close();
            }
            return flag;
        }

        public string ExecuteNonQuerySQL(string strSQL)
        {
            return this.ExecuteNonQuerySQLWithParams(strSQL, this.Params.Count == 0 ? (List<DBParameter>)null : this.Params, this.WhereParams.Count == 0 ? (List<DBParameter>)null : this.WhereParams);
        }

        public string ExecuteNonQuerySQLWithParams(string strSQL, List<DBParameter> pParams)
        {
            return this.ExecuteNonQuerySQLWithParams(strSQL, pParams, (List<DBParameter>)null);
        }

        public string ExecuteNonQuerySQLWithParams(
          string strSQL,
          List<DBParameter> pParams,
          List<DBParameter> pWhereParams)
        {
            this.ErrorResult = string.Empty;

            try
            {
                this._TrackerDbConn.Open();
                OleDbTransaction transaction = this._TrackerDbConn.BeginTransaction();
                this._command = new OleDbCommand(strSQL, this._TrackerDbConn, transaction);

                if (pParams != null)
                {
                    foreach (DBParameter pParam in pParams)
                    {
                        this._command.Parameters.Add(BuildOleDbParameter(pParam));
                    }
                }

                if (pWhereParams != null)
                {
                    foreach (DBParameter pWhereParam in pWhereParams)
                    {
                        this._command.Parameters.Add(BuildOleDbParameter(pWhereParam));
                    }
                }

                //for (int i = 0; i < _command.Parameters.Count; i++)
                //{
                //    OleDbParameter param = _command.Parameters[i];
                //    System.Diagnostics.Debug.WriteLine(
                //        $"Param[{i}]: Value={param.Value}, DbType={param.DbType}, OleDbType={param.OleDbType}");
                //}

                this.numRecs = this._command.ExecuteNonQuery();
                transaction.Commit();
            }
            catch (OleDbException ex)
            {
                // Enhanced error logging with detailed parameter information
                this.ErrorResult = BuildDetailedErrorMessage(ex, strSQL, CombineParameters(pParams, pWhereParams));

                // Also log to AppLogger like the retry method
                AppLogger.WriteLog(SystemConstants.LogTypes.Database, $"ExecuteNonQuerySQL failed: {this.ErrorResult}");
            }
            catch (Exception ex)
            {
                // Handle non-OleDb exceptions
                this.ErrorResult = $"Unexpected Error in ExecuteNonQuery: {ex.Message}\nQuery: {strSQL}";
                AppLogger.WriteLog(SystemConstants.LogTypes.Database, this.ErrorResult);
            }
            finally
            {
                if (this._command != null)
                    this._command.Dispose();

                this._TrackerDbConn.Close();
            }

            return this.ErrorResult;

        }

        public DataSet ReturnDataSet(string strSQL)
        {
            return this.ReturnDataSet(strSQL, this.WhereParams.Count == 0 ? (List<DBParameter>)null : this.WhereParams);
        }

        public DataSet ReturnDataSet(string strSQL, List<DBParameter> pWhereParams)
        {
            DataSet dataSet = null;

            try
            {
                this._TrackerDbConn.Open();
                this._command = new OleDbCommand(strSQL, this._TrackerDbConn);

                if (pWhereParams != null)
                {
                    foreach (DBParameter param in pWhereParams)
                    {
                        this._command.Parameters.Add(BuildOleDbParameter(param));
                    }
                }

                dataSet = new DataSet();
                OleDbDataAdapter adapter = new OleDbDataAdapter(this._command);
                adapter.Fill(dataSet, "objDataSet");
            }
            catch (OleDbException ex)
            {
                // Enhanced error logging with detailed parameter information
                this.ErrorResult = BuildDetailedErrorMessage(ex, strSQL, pWhereParams);
                // Also log to AppLogger like the retry method
                AppLogger.WriteLog(SystemConstants.LogTypes.Database, $"ReturnDataSet failed: {this.ErrorResult}");
            }
            catch (Exception ex)
            {
                // Handle non-OleDb exceptions
                this.ErrorResult = $"Unexpected Error in ReturnDataSet: {ex.Message}\nQuery: {strSQL}";
                AppLogger.WriteLog(SystemConstants.LogTypes.Database, this.ErrorResult);
            }
            finally
            {
                if (this._command != null)
                    this._command.Dispose();

                this._TrackerDbConn.Close();
            }

            return dataSet;
        }

        private static readonly bool EnableDetailedLogging = ConfigHelper.GetBool("EnableDatabaseDetailedLogging",false);

        public IDataReader ExecuteSQLGetDataReader(string strSQL)
        {
            IDataReader dataReader = this.ExecuteSQLGetDataReader(strSQL, this.WhereParams.Count == 0 ? (List<DBParameter>)null : this.WhereParams);
            return dataReader;
        }
        public IDataReader ExecuteSQLGetDataReader(string strSQL, List<DBParameter> pWhereParams)
        {
            IDataReader dataReader = null;

            try
            {
                this._TrackerDbConn.Open();
                this._command = new OleDbCommand(strSQL, this._TrackerDbConn);

                if (pWhereParams != null)
                {
                    foreach (DBParameter pWhereParam in pWhereParams)
                    {
                        this._command.Parameters.Add(BuildOleDbParameter(pWhereParam));
                    }
                }

                // ✅ This automatically closes connection when reader is closed
                dataReader = this._command.ExecuteReader(); // CommandBehavior.CloseConnection);
            }
            catch (OleDbException ex)
            {
                this.ErrorResult = BuildDetailedErrorMessage(ex, strSQL, pWhereParams);
                AppLogger.WriteLog(SystemConstants.LogTypes.Database, $"ExecuteSQLGetDataReader failed: {this.ErrorResult}");

                if (this._TrackerDbConn?.State == ConnectionState.Open)
                {
                    this._TrackerDbConn.Close();
                }

                return null;
            }
            catch (Exception ex)
            {
                this.ErrorResult = $"Unexpected Error in ExecuteSQLGetDataReader: {ex.Message}\nQuery: {strSQL}";
                AppLogger.WriteLog(SystemConstants.LogTypes.Database, this.ErrorResult);

                if (this._TrackerDbConn?.State == ConnectionState.Open)
                {
                    this._TrackerDbConn.Close();
                }

                return null;
            }

            return dataReader;
        }

        public IDataReader ExecuteSQLGetDataReaderWithRetry(string strSQL, List<DBParameter> pWhereParams, int maxRetries = 3)
        {
            int retryCount = 0;
            TimeSpan delay = TimeSpan.FromMilliseconds(100);

            while (retryCount < maxRetries)
            {
                try
                {
                    if (!ValidateConnection())
                    {
                        throw new InvalidOperationException("Cannot establish valid database connection");
                    }

                    return ExecuteSQLGetDataReader(strSQL, pWhereParams);
                }
                catch (OleDbException ex) when (IsTransientError(ex) && retryCount < maxRetries - 1)
                {
                    retryCount++;
                    AppLogger.WriteLog(SystemConstants.LogTypes.Database, $"Transient error on attempt {retryCount}, retrying in {delay.TotalMilliseconds}ms: {ex.Message}");

                    System.Threading.Thread.Sleep(delay);
                    delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 2); // Exponential backoff

                    // Reset connection for retry
                    try
                    {
                        this.Initialize();
                    }
                    catch (Exception initEx)
                    {
                        AppLogger.WriteLog(SystemConstants.LogTypes.Database, $"Failed to reinitialize connection for retry: {initEx.Message}");
                    }
                }
                catch (Exception ex)
                {
                    AppLogger.WriteLog(SystemConstants.LogTypes.Database, $"Non-retryable error: {ex.Message}");
                    throw;
                }
            }

            throw new Exception($"Failed to execute query after {maxRetries} attempts");
        }

        private bool IsTransientError(OleDbException ex)
        {
            // Common transient error patterns for Access/OleDb
            var transientMessages = new[]
            {
                "timeout",
                "connection",
                "network",
                "unavailable",
                "busy",
                "locked"
            };

            return transientMessages.Any(msg => ex.Message.ToLower().Contains(msg));
        }

        private string BuildDetailedErrorMessage(OleDbException ex, string strSQL, List<DBParameter> pWhereParams)
        {
            var errorDetails = new System.Text.StringBuilder();
            errorDetails.AppendLine($"SQL Error: {ex.Message}");
            errorDetails.AppendLine($"Query: {strSQL}");

            // Only include parameter details if command was created and has parameters
            if (this._command?.Parameters.Count > 0 && pWhereParams?.Count > 0)
            {
                errorDetails.AppendLine("Parameters:");
                int paramCount = Math.Min(this._command.Parameters.Count, pWhereParams.Count);
                for (int i = 0; i < paramCount; i++)
                {
                    var param = this._command.Parameters[i];
                    var originalParam = pWhereParams[i];
                    errorDetails.AppendLine($"  [{i}] {originalParam.DataValue} ({originalParam.DataDbType}) -> {param.Value}");
                }
            }

            return errorDetails.ToString();
        }

        // Add these helper methods to your TrackerDb class
        //private OleDbType GetOleDbType(DbType dbType)
        //{
        //    switch (dbType)
        //    {
        //        case DbType.DateTime: return OleDbType.DBTimeStamp;
        //        case DbType.Int32: return OleDbType.Integer;
        //        case DbType.String: return OleDbType.VarChar;
        //        default: return OleDbType.VarChar;
        //    }
        //}

        //private object ConvertParameterForAccess(object value, DbType dbType)
        //{
        //    if (value == null) return DBNull.Value;

        //    if (dbType == DbType.DateTime)
        //    {
        //        // Force proper Access date format
        //        return ((DateTime)value).ToString("MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
        //    }
        //    return value;
        //}
        /* old one
          public IDataReader ExecuteSQLGetDataReader(string strSQL, List<DBParameter> pWhereParams)
                {
                    IDataReader dataReader = (IDataReader)null;
                    try
                    {
                        this._TrackerDbConn.Open();
                        this._command = new OleDbCommand(strSQL, this._TrackerDbConn);
                        if (pWhereParams != null)
                        {
                            foreach (DBParameter pWhereParam in pWhereParams)
                            {
                                OleDbParameterCollection parameters = this._command.Parameters;
                                OleDbParameter oleDbParameter1 = new OleDbParameter();
                                oleDbParameter1.Value = pWhereParam.DataValue;
                                oleDbParameter1.DbType = pWhereParam.DataDbType;
                                OleDbParameter oleDbParameter2 = oleDbParameter1;
                                parameters.Add(oleDbParameter2);
                            }
                        }
                        dataReader = (IDataReader)this._command.ExecuteReader();
                    }
                    catch (OleDbException ex)
                    {
                        this.ErrorResult = ex.Message;
                    }
                    return dataReader;
                }
        */

        public Hashtable ReturnHashTable(string strSQL)
        {
            return this.ReturnHashTable(strSQL, (List<DBParameter>)null);
        }

        public Hashtable ReturnHashTable(string strSQL, List<DBParameter> pWhereParams)
        {
            OleDbDataReader oleDbDataReader = null;
            Hashtable hashtable = new Hashtable();

            try
            {
                this._TrackerDbConn.Open();
                this._command = new OleDbCommand(strSQL, this._TrackerDbConn);

                if (pWhereParams != null)
                {
                    foreach (DBParameter pWhereParam in pWhereParams)
                    {
                        this._command.Parameters.Add(BuildOleDbParameter(pWhereParam));
                    }
                }

                oleDbDataReader = this._command.ExecuteReader();
                while (oleDbDataReader.Read())
                {
                    object key = oleDbDataReader.GetValue(0);
                    object value = oleDbDataReader.GetValue(1);
                    hashtable.Add(key, value);
                }
            }
            catch (OleDbException ex)
            {
                this.ErrorResult = ex.Message;
                HttpContext.Current.Session["DataAccessError"] = ex.Message;
                AppLogger.WriteLog(SystemConstants.LogTypes.Database, $"ReturnHashTable failed: {this.ErrorResult}");
            }
            catch (Exception ex)
            {
                // Handle non-OleDb exceptions
                this.ErrorResult = $"Unexpected Error in ReturnHashTable: {ex.Message}\nQuery: {strSQL}";
                AppLogger.WriteLog(SystemConstants.LogTypes.Database, this.ErrorResult);
            }
            finally
            {
                if (oleDbDataReader != null && !oleDbDataReader.IsClosed)
                    oleDbDataReader.Close();

                this._command.Dispose();
                this._TrackerDbConn.Close();
            }

            return hashtable;
        }


        public void Close()
        {
            if (this.Params.Count > 0)
                this.Params.Clear();
            if (this.WhereParams.Count > 0)
                this.WhereParams.Clear();
            if (this._command != null)
                this._command.Dispose();
            this.TrackerDbConn.Close();
            this.TrackerDbConn.Dispose();
        }

        public void AddParams(object pDataValue)
        {
            this.Params.Add(new DBParameter()
            {
                DataValue = pDataValue,
                DataDbType = DbType.String
            });
        }

        public void AddParams(object pDataValue, DbType pDataDbType)
        {
            this.Params.Add(new DBParameter()
            {
                DataValue = pDataValue,
                DataDbType = pDataDbType
            });
        }

        public void AddParams(object pDataValue, string pParamName)
        {
            this.Params.Add(new DBParameter()
            {
                DataValue = pDataValue,
                DataDbType = DbType.String,
                ParamName = pParamName
            });
        }

        public void AddParams(object pDataValue, DbType pDataDbType, string pParamName)
        {
            if (pDataValue == null)
            {
                pDataValue = (object)string.Empty;
                pDataDbType = DbType.String;
            }
            this.Params.Add(new DBParameter()
            {
                DataValue = pDataValue,
                DataDbType = pDataDbType
            });
        }

        public void AddWhereParams(object pDataValue)
        {
            this.WhereParams.Add(new DBParameter()
            {
                DataValue = pDataValue,
                DataDbType = DbType.String
            });
        }

        public void AddWhereParams(object pDataValue, DbType pDataDbType)
        {
            this.WhereParams.Add(new DBParameter()
            {
                DataValue = pDataValue,
                DataDbType = pDataDbType
            });
        }

        public void AddWhereParams(object pDataValue, string pParamName)
        {
            this.WhereParams.Add(new DBParameter()
            {
                DataValue = pDataValue,
                DataDbType = DbType.String,
                ParamName = pParamName
            });
        }

        public void AddWhereParams(object pDataValue, DbType pDataDbType, string pParamName)
        {
            if (pDataValue == null)
            {
                pDataValue = (object)string.Empty;
                pDataDbType = DbType.String;
            }
            this.WhereParams.Add(new DBParameter()
            {
                DataValue = pDataValue,
                DataDbType = pDataDbType
            });
        }

        public bool CreateIfDoesNotExists(string pTableName)
        {
            bool ifDoesNotExists = false;
            TrackerDb trackerDb = new TrackerDb();
            if (!trackerDb.TableExists(pTableName))
            {
                string strSQL = string.Empty;
                switch (pTableName)
                {
                    case "LogTbl":
                        strSQL = "CREATE TABLE LogTbl ( [LogID] AUTOINCREMENT, [DateAdded] DateTime, [UserID] INT, [SectionID] INT, [TranactionTypeID] INT,  [CustomerID] INT, Details VARCHAR(255), [Notes] MEMO,  CONSTRAINT [pk_LogID] PRIMARY KEY (LogID) )";
                        break;
                    case "SectionTypesTbl":
                        strSQL = "CREATE TABLE SectionTypesTbl ( [SectionID] INT, [SectionType] VARCHAR(50), [Notes] MEMO,  CONSTRAINT [pk_SectionID] PRIMARY KEY (SectionID) )";
                        break;
                    case "TransactionTypesTbl":
                        strSQL = "CREATE TABLE TransactionTypesTbl ( [TransactionID] INT, [TransactionType] VARCHAR(50), [Notes] MEMO,  CONSTRAINT [pk_TransactionID] PRIMARY KEY (TransactionID) )";
                        break;
                }
                if (!string.IsNullOrWhiteSpace(strSQL))
                    ifDoesNotExists = string.IsNullOrWhiteSpace(trackerDb.ExecuteNonQuerySQL(strSQL));
            }
            trackerDb.Close();
            return ifDoesNotExists;
        }
        // Add this to TrackerDb class as now an IDisposable clas
        public void Dispose()
        {
            this.Close();
        }

        public bool TestConnection()
        {
            try
            {
                using (var testConn = new OleDbConnection(
                    ConfigurationManager.ConnectionStrings[SystemConstants.DatabaseConstants.ConnectionStringName].ConnectionString))
                {
                    testConn.Open();
                    using (var cmd = new OleDbCommand("SELECT COUNT(*) FROM MSysObjects WHERE Type=1", testConn))
                    {
                        cmd.ExecuteScalar();
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                AppLogger.WriteLog(SystemConstants.LogTypes.Database, $"Connection test failed: {ex.Message}");
                return false;
            }
        }

        public string GetDatabaseStatus()
        {
            try
            {
                if (!TestConnection())
                    return "Database connection failed";

                // Test basic query performance
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                using (var testConn = new OleDbConnection(
                    ConfigurationManager.ConnectionStrings[SystemConstants.DatabaseConstants.ConnectionStringName].ConnectionString))
                {
                    testConn.Open();
                    using (var cmd = new OleDbCommand("SELECT TOP 1 * FROM CustomersTbl", testConn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            reader.Read();
                        }
                    }
                }
                stopwatch.Stop();

                return $"Database healthy - Query time: {stopwatch.ElapsedMilliseconds}ms";
            }
            catch (Exception ex)
            {
                return $"Database status check failed: {ex.Message}";
            }
        }
    }
}