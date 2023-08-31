using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FormziApi.Models
{
    public class SubscriptionPlanModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int NumberOfUsers { get; set; }
        public int NumberOfForms { get; set; }
        public int NumberOfEntries { get; set; }
        public int NumberOfReports { get; set; }
        public int StorageLimit { get; set; }
        public decimal SubscriptionFee { get; set; }
        public int DurationInMonths { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsDeleted { get; set; }
    
    }
}