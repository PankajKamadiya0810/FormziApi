using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FormziApi.Models
{
    public class ProjectFormModel
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public long FormId { get; set; }
        public bool IsDeleted { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public long CreatedBy { get; set; }
        public System.DateTime UpdatedOn { get; set; }
        public long UpdatedBy { get; set; }
    }
}