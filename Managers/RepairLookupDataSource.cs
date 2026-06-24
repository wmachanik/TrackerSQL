using System.Collections.Generic;
using System.ComponentModel;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Managers
{
    public class RepairLookupDataSource
    {
        private readonly RepairStatusesRepository _repairStatusesRepository = new RepairStatusesRepository();
        private readonly RepairFaultsRepository _repairFaultsRepository = new RepairFaultsRepository();
        private readonly EquipTypesRepository _equipTypesRepository = new EquipTypesRepository();
        private readonly EquipConditionsRepository _equipConditionsRepository = new EquipConditionsRepository();
        private readonly OrderLookupDataSource _orderLookupDataSource = new OrderLookupDataSource();

        [DataObjectMethod(DataObjectMethodType.Select)]
        public List<RepairStatus> GetRepairStatuses(string sortBy)
        {
            return _repairStatusesRepository.GetAll(string.IsNullOrWhiteSpace(sortBy) ? "RepairStatusID" : sortBy);
        }

        [DataObjectMethod(DataObjectMethodType.Select)]
        public List<RepairFault> GetRepairFaults(string sortBy)
        {
            return _repairFaultsRepository.GetAll(string.IsNullOrWhiteSpace(sortBy) ? "SortOrder" : sortBy);
        }

        [DataObjectMethod(DataObjectMethodType.Select)]
        public List<EquipType> GetEquipTypes(string sortBy)
        {
            return _equipTypesRepository.GetAll(string.IsNullOrWhiteSpace(sortBy) ? "EquipTypeDesc" : sortBy);
        }

        [DataObjectMethod(DataObjectMethodType.Select)]
        public List<EquipCondition> GetEquipConditions(string sortBy)
        {
            return _equipConditionsRepository.GetAll(string.IsNullOrWhiteSpace(sortBy) ? "ConditionDesc" : sortBy);
        }

        [DataObjectMethod(DataObjectMethodType.Select)]
        public List<OrderCompanyLookup> GetCompanyNames()
        {
            return _orderLookupDataSource.GetCompanyNames();
        }

        [DataObjectMethod(DataObjectMethodType.Select)]
        public List<OrderCompanyLookup> GetDemoCompanyNames()
        {
            return _orderLookupDataSource.GetCompanyNames();
        }
    }
}
