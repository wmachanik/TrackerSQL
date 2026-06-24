using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackerSQL.Models
{
    public class ActiveDeliveryWithPerson
    {
        public DateTime RequiredByDate { get; set; } = DateTime.MinValue;
        public string Person { get; set; } = string.Empty;
        public int PersonID { get; set; } = 0;
    }
}
