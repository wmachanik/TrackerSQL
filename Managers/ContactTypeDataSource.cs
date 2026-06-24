using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using TrackerSQL.Models;
using TrackerSQL.Repositories;

namespace TrackerSQL.Managers
{
    public class ContactTypeDataSource
    {
        private readonly ContactTypesRepository _contactTypesRepository = new ContactTypesRepository();

        [DataObjectMethod(DataObjectMethodType.Select)]
        public List<ContactType> GetAll(string sortBy)
        {
            return _contactTypesRepository.GetAll(string.IsNullOrWhiteSpace(sortBy) ? "ContactTypeDesc" : sortBy);
        }
    }
}
