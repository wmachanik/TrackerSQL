using System;
using System.Collections.Generic;
using System.Data;
using TrackerDotNet.Classes;

namespace TrackerDotNet.Controls
{
    /// <summary>
    /// Service Types table class with centralized enum for better code quality
    /// </summary>
    public class ServiceTypeTbl
    {
        // Keep existing constants for backward compatibility
        public const int CONST_CLEANING_SERVICETYPEID = 1;
        public const int CONST_COFFEE_SERVICETYPEID = 2;
        public const int CONST_DESCALING_SERVICETYPEID = 4;
        public const int CONST_FILTER_SERVICETYPEID = 5;
        public const int CONST_SERVICE_SERVICETYPEID = 10;
        
        /// <summary>
        /// Enum for service types used in coffee checkup operations
        /// Maps to the existing ServiceType enum but focuses on coffee-related services
        /// </summary>
        public enum CoffeeServiceType
        {
            Cleaning = CONST_CLEANING_SERVICETYPEID,      // 1
            Coffee = CONST_COFFEE_SERVICETYPEID,          // 2
            Descaling = CONST_DESCALING_SERVICETYPEID,    // 4
            Filter = CONST_FILTER_SERVICETYPEID,          // 5
            Service = CONST_SERVICE_SERVICETYPEID         // 10
        }
        
        private const string CONST_SQL_SELECT = "SELECT ServiceTypeId, ServiceType FROM ServiceTypesTbl";
        private const string CONST_SQL_SELECTBYID = "SELECT ServiceType FROM ServiceTypesTbl WHERE ServiceTypeId = ?";
        
        private int _ServiceTypeId;
        private string _ServiceType;

        public ServiceTypeTbl()
        {
            this._ServiceTypeId = 0;
            this._ServiceType = string.Empty;
        }

        public int ServiceTypeId
        {
            get => this._ServiceTypeId;
            set => this._ServiceTypeId = value;
        }

        public string ServiceType
        {
            get => this._ServiceType;
            set => this._ServiceType = value;
        }

        /// <summary>
        /// Helper method to get CoffeeServiceType from ID
        /// </summary>
        public static CoffeeServiceType GetCoffeeServiceType(int id)
        {
            if (Enum.IsDefined(typeof(CoffeeServiceType), id))
                return (CoffeeServiceType)id;
            
            throw new ArgumentException($"Unknown coffee service type ID: {id}");
        }
        
        /// <summary>
        /// Helper method to check if a service type ID is valid for coffee operations
        /// </summary>
        public static bool IsValidCoffeeServiceType(int id)
        {
            return Enum.IsDefined(typeof(CoffeeServiceType), id);
        }
        
        /// <summary>
        /// Helper method to get display name for coffee service type
        /// </summary>
        public static string GetCoffeeServiceTypeName(CoffeeServiceType serviceType)
        {
            switch (serviceType)
            {
                case CoffeeServiceType.Cleaning: return "Cleaning";
                case CoffeeServiceType.Coffee: return "Coffee";
                case CoffeeServiceType.Descaling: return "Descaling";
                case CoffeeServiceType.Filter: return "Filter";
                case CoffeeServiceType.Service: return "Service";
                default: return "Unknown";
            }
        }

        /// <summary>
        /// Get all service types from database
        /// </summary>
        public List<ServiceTypeTbl> GetAll()
        {
            List<ServiceTypeTbl> all = new List<ServiceTypeTbl>();
            TrackerDb trackerDb = new TrackerDb();
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(CONST_SQL_SELECT);
            if (dataReader != null)
            {
                while (dataReader.Read())
                    all.Add(new ServiceTypeTbl()
                    {
                        ServiceTypeId = dataReader["ServiceTypeId"] == DBNull.Value ? 0 : Convert.ToInt32(dataReader["ServiceTypeId"]),
                        ServiceType = dataReader["ServiceType"] == DBNull.Value ? string.Empty : dataReader["ServiceType"].ToString()
                    });
                dataReader.Close();
            }
            trackerDb.Close();
            return all;
        }

        /// <summary>
        /// Get service type description by ID
        /// </summary>
        public string GetServiceTypeDesc(int pServiceTypeId)
        {
            string serviceTypeDesc = string.Empty;
            TrackerDb trackerDb = new TrackerDb();
            trackerDb.AddWhereParams(pServiceTypeId, DbType.Int32);
            IDataReader dataReader = trackerDb.ExecuteSQLGetDataReader(CONST_SQL_SELECTBYID);
            if (dataReader != null)
            {
                if (dataReader.Read())
                    serviceTypeDesc = dataReader["ServiceType"] == DBNull.Value ? string.Empty : dataReader["ServiceType"].ToString();
                dataReader.Close();
            }
            trackerDb.Close();
            return serviceTypeDesc;
        }
    }
}