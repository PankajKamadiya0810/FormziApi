using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FormziApi.Models
{
    public class VP_PHCModel
    {
        public int Id { get; set; }
        public int SubscriberId { get; set; }
        public int OperationId { get; set; }
        public string LongName { get; set; }
        public string ShortName { get; set; }
        public string AddressPathText { get; set; }
        public bool IsDeleted { get; set; }
        public int UpdateBy { get; set; }
        public System.DateTime UpdatedOn { get; set; }
        public string SiteCode { get; set; }
        public string DOHClinicCode { get; set; }

        public List<VP_PHCServicesModel> VP_PHCServices { get; set; }
        
    }
}