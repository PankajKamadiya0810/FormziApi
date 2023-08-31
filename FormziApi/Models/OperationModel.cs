using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FormziApi.Models
{
    public class OperationModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public System.DateTime UpdatedOn { get; set; }
        public int SubscriberId { get; set; }
        public int NoOfLocations { get; set; }
        public string ManagerName { get; set; }

        public  List<LocationModel> Locations { get; set; }

        public List<OperationSettingModel> OperationSettings { get; set; }

        public List<AddressModel> LocationList { get; set; }
    }
}