using System.Collections.Generic;
using System.Data;
using TrackerSQL.Classes;
using TrackerSQL.Models;

namespace TrackerSQL.Repositories
{
    public class ItemUnitsRepository : RepositoryBase<ItemUnit>
    {
        protected override string TableName => "ItemUnitsTbl";
        protected override string KeyColumn => "ItemUnitID";

        protected override string CoreColumns =>
            "ItemUnitID, UnitOfMeasure, UnitDescription";

        protected override string LookupColumns =>
            "ItemUnitID, UnitDescription";

        public string GetUnitOfMeasure(int itemUnitId)
        {
            var unit = GetById(itemUnitId);
            return unit?.UnitOfMeasure ?? string.Empty;
        }

        public string GetUnitDescription(int itemUnitId)
        {
            var unit = GetById(itemUnitId);
            return unit?.UnitDescription ?? string.Empty;
        }
    }
}