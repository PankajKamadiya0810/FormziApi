using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FormziApi.Models
{
    public class VP_PHCServicesModel
    {
        public int Id { get; set; }
        public int SubscriberId { get; set; }
        public int PHCId { get; set; }
        public int HCServiceId { get; set; }
        public bool IsDeleted { get; set; }
        public Nullable<long> UpdatedBy { get; set; }
        public Nullable<System.DateTime> UpdatedOn { get; set; }

        public string ServiceType { get; set; }
        public bool IsNewHC { get; set; }
    }
}