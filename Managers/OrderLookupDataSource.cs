using System.Collections.Generic;
using System.ComponentModel;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Managers
{
    /// <summary>
    /// Shared ObjectDataSource lookups for OrderEntry, OrderDetail, and OrderDone.
    /// </summary>
    public class OrderLookupDataSource
    {
        private readonly ContactsRepository _contactsRepository = new ContactsRepository();
        private readonly PersonsRepository _personsRepository = new PersonsRepository();
        private readonly ItemsRepository _itemsRepository = new ItemsRepository();
        private readonly ItemPackagingsRepository _itemPackagingsRepository = new ItemPackagingsRepository();

        [DataObjectMethod(DataObjectMethodType.Select)]
        public List<OrderCompanyLookup> GetCompanyNames()
        {
            var list = new List<OrderCompanyLookup>();
            foreach (var contact in _contactsRepository.GetAllCompanyNames())
            {
                list.Add(new OrderCompanyLookup
                {
                    CustomerID = contact.ContactID,
                    CompanyName = contact.CompanyName
                });
            }

            return list;
        }

        [DataObjectMethod(DataObjectMethodType.Select)]
        public List<Person> GetPersons(string sortBy)
        {
            string orderBy = string.IsNullOrWhiteSpace(sortBy) ? "Abbreviation" : sortBy;
            return _personsRepository.GetAllEnabled(orderBy);
        }

        [DataObjectMethod(DataObjectMethodType.Select)]
        public List<OrderItemLookup> GetItems(string sortBy)
        {
            return _itemsRepository.GetOrderItemLookups(sortBy);
        }

        [DataObjectMethod(DataObjectMethodType.Select)]
        public List<OrderPackagingLookup> GetPackagingTypes()
        {
            var list = new List<OrderPackagingLookup>();
            foreach (var packaging in _itemPackagingsRepository.GetAll("ItemPrepDescription"))
            {
                list.Add(new OrderPackagingLookup
                {
                    PackagingID = packaging.ItemPackagingID,
                    Description = packaging.ItemPackagingDesc ?? string.Empty
                });
            }

            return list;
        }
    }
}
