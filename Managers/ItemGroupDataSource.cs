using System.Collections.Generic;
using System.ComponentModel;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Managers
{
    public class ItemGroupDataSource
    {
        private readonly ItemsRepository _itemsRepository = new ItemsRepository();
        private readonly ItemGroupsRepository _itemGroupsRepository = new ItemGroupsRepository();

        [DataObjectMethod(DataObjectMethodType.Select)]
        public List<OrderItemLookup> GetAllGroupTypeItems()
        {
            return _itemsRepository.GetAllGroupTypeItems();
        }

        [DataObjectMethod(DataObjectMethodType.Select)]
        public List<OrderItemLookup> GetAllItemsNotInItemGroup(int groupItemTypeId)
        {
            return _itemsRepository.GetItemsNotInGroup(groupItemTypeId);
        }

        [DataObjectMethod(DataObjectMethodType.Select)]
        public List<ItemGroupGridRow> GetAllByGroupItemTypeId(int groupItemId, string sortBy)
        {
            return _itemGroupsRepository.GetGridRowsByGroupReferenceItemId(groupItemId, sortBy);
        }

        [DataObjectMethod(DataObjectMethodType.Insert)]
        public bool InsertItemGroup(ItemGroupGridRow row)
        {
            return _itemGroupsRepository.InsertItemToGroup(row.GroupItemTypeID, row.ItemTypeID, row.Notes);
        }

        [DataObjectMethod(DataObjectMethodType.Delete)]
        public bool DeleteItemGroup(int itemGroupId)
        {
            return _itemGroupsRepository.Delete(itemGroupId);
        }

        [DataObjectMethod(DataObjectMethodType.Select)]
        public List<OrderItemLookup> GetAllItemDesc()
        {
            return _itemsRepository.GetOrderItemLookups(null);
        }
    }
}
