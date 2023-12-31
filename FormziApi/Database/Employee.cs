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
    
    public partial class Employee
    {
        public Employee()
        {
            this.Dashboards = new HashSet<Dashboard>();
            this.Dashboards1 = new HashSet<Dashboard>();
            this.Documents = new HashSet<Document>();
            this.EmployeeForms = new HashSet<EmployeeForm>();
            this.EmployeeLocations = new HashSet<EmployeeLocation>();
            this.FormSubmissions = new HashSet<FormSubmission>();
            this.Jobs = new HashSet<Job>();
            this.SubmissionEmployeeMaps = new HashSet<SubmissionEmployeeMap>();
            this.Forms = new HashSet<Form>();
        }
    
        public long Id { get; set; }
        public string PayrollId { get; set; }
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool Gender { get; set; }
        public string ProfilePicture { get; set; }
        public Nullable<System.DateTime> BirthDate { get; set; }
        public string PhoneNumber { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public System.DateTime UpdatedOn { get; set; }
        public long AppLoginId { get; set; }
        public int SubscriberId { get; set; }
        public int SystemRoleId { get; set; }
    
        public virtual AppLogin AppLogin { get; set; }
        public virtual ICollection<Dashboard> Dashboards { get; set; }
        public virtual ICollection<Dashboard> Dashboards1 { get; set; }
        public virtual ICollection<Document> Documents { get; set; }
        public virtual ICollection<EmployeeForm> EmployeeForms { get; set; }
        public virtual ICollection<EmployeeLocation> EmployeeLocations { get; set; }
        public virtual Subscriber Subscriber { get; set; }
        public virtual ICollection<FormSubmission> FormSubmissions { get; set; }
        public virtual ICollection<Job> Jobs { get; set; }
        public virtual ICollection<SubmissionEmployeeMap> SubmissionEmployeeMaps { get; set; }
        public virtual ICollection<Form> Forms { get; set; }
    }
}
