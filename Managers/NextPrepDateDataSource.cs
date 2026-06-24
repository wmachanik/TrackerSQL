using System.Collections.Generic;
using System.ComponentModel;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Managers
{
    public class NextPrepDateDataSource
    {
        private readonly NextPrepDateByAreaRepository _repository = new NextPrepDateByAreaRepository();

        [DataObjectMethod(DataObjectMethodType.Select)]
        public List<DeliveryDateLookup> GetAllDeliveryDates()
        {
            var list = new List<DeliveryDateLookup>();
            foreach (var date in _repository.GetAllDeliveryDates())
            {
                list.Add(new DeliveryDateLookup { Date = date });
            }

            return list;
        }

        public List<AreaPrepDateRow> GetAreaPrepDateGrid()
        {
            return _repository.GetAreaPrepDateGrid();
        }
    }
}
