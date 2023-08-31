using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.WebPages.Html;

namespace FormziApi.Models
{
    public class DashboardModel
    {
        public int Id { get; set; }
        public long FormId { get; set; }
        public int SubscriberId { get; set; }
        public string Page { get; set; }
        public object DashboardObjs { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public long CreatedBy { get; set; }
        public Nullable<System.DateTime> UpdatedOn { get; set; }
        public Nullable<long> UpdatedBy { get; set; }
    }

    public class DashboardAnswersModel
    {
        public string Id { get; set; }
        public List<SelectListItem> Data { get; set; }
    }
}