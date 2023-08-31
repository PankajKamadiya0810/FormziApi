using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FormziApi.Models
{
    public class ProjectModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int ClientId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public long CreatedBy { get; set; }
        public System.DateTime UpdatedOn { get; set; }
        public long UpdatedBy { get; set; }
        public virtual List<FormModel> FormList { get; set; }
        public virtual List<RemovedForms> RemovedForms { get; set; }
        public virtual List<AddedForms> AddedForms { get; set; }
    }
    public class ProjectDataModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int ClientId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public long CreatedBy { get; set; }
        public System.DateTime UpdatedOn { get; set; }
        public long UpdatedBy { get; set; }

        public List<FormModel> FormList { get; set; }
    }
    public class RemovedForms
    {
        public long Id { get; set; }
    }
    public class AddedForms
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }

}