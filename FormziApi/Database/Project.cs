//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace FormziApi.Database
{
    using System;
    using System.Collections.Generic;
    
    public partial class Project
    {
        public Project()
        {
            this.EmployeeForms = new HashSet<EmployeeForm>();
            this.ProjectForms = new HashSet<ProjectForm>();
        }
    
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
    
        public virtual Client Client { get; set; }
        public virtual ICollection<EmployeeForm> EmployeeForms { get; set; }
        public virtual ICollection<ProjectForm> ProjectForms { get; set; }
    }
}