using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FormziApi.Models
{
    public class FormIssueTypeModel
    {
        public int Id { get; set; }
        public int SubscriberId { get; set; }
        public long FormId { get; set; }
        public string Name { get; set; }
        public int WorkDuration { get; set; }
        public int ForwardDuration { get; set; }
        public Nullable<System.DateTime> UpdatedOn { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }

        public bool IsDeleted { get; set; }
    }
}