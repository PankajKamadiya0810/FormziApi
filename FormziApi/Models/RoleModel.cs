using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FormziApi.Models
{
    public class RoleModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Value { get; set; }
        public int SubscriberId { get; set; }
        public bool IsActive { get; set; }
        public bool IsAssigned { get; set; }

        //Added By Hiren 6-11-2017
        public long? CreatedBy { get; set; }
        public System.DateTime? CreatedOn { get; set; }
        public long? UpdatedBy { get; set; }
        public System.DateTime? UpdatedOn { get; set; }
        public int ReportToId { get; set; }//Added By Hiren 21-11-2017
        public string ReportToName { get; set; }//Added By Hiren 24-11-2017
    }
}