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
    
    public partial class ProjectForm
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public long FormId { get; set; }
        public bool IsDeleted { get; set; }
        public long CreatedBy { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public long UpdatedBy { get; set; }
        public System.DateTime UpdatedOn { get; set; }
    
        public virtual Project Project { get; set; }
        public virtual Form Form { get; set; }
    }
}
