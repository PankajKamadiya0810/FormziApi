using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FormziApi.Models
{
    public class DepartmentDesignationModel
    {
        public int Id { get; set; }
        public int SubscriberId { get; set; }
        public int DepartmentId { get; set; }
        public int DesignationId { get; set; }
        public Nullable<System.DateTime> UpdatedOn { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public bool IsDeleted { get; set; }

        public string DesignationName { get; set; }
        public string DepartmentName { get; set; }
        public string SubscriberName { get; set; }
    }
}