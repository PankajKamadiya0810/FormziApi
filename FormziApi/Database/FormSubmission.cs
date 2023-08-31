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
    
    public partial class FormSubmission
    {
        public FormSubmission()
        {
            this.FormAnswers = new HashSet<FormAnswer>();
            this.PostLocations = new HashSet<PostLocation>();
            this.SubmissionEmployeeMaps = new HashSet<SubmissionEmployeeMap>();
            this.SubmissionLogs = new HashSet<SubmissionLog>();
        }
    
        public long Id { get; set; }
        public Nullable<int> AppInfoId { get; set; }
        public string DeviceId { get; set; }
        public string LatLong { get; set; }
        public Nullable<decimal> Latitude { get; set; }
        public Nullable<decimal> Longitude { get; set; }
        public long SubmittedBy { get; set; }
        public System.DateTime SubmittedOn { get; set; }
        public Nullable<long> ApprovedBy { get; set; }
        public Nullable<System.DateTime> ApprovedOn { get; set; }
        public Nullable<int> IsApproved { get; set; }
        public bool IsProcessing { get; set; }
        public long EmployeeId { get; set; }
        public long FormId { get; set; }
        public int SubscriberId { get; set; }
        public System.Guid FolderName { get; set; }
        public bool IsSync { get; set; }
        public Nullable<int> FormVersionId { get; set; }
        public Nullable<int> LanguageId { get; set; }
        public Nullable<int> Action { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public Nullable<System.DateTime> UpdatedOn { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsDeleted { get; set; }
    
        public virtual Employee Employee { get; set; }
        public virtual ICollection<FormAnswer> FormAnswers { get; set; }
        public virtual FormVersion FormVersion { get; set; }
        public virtual Language Language { get; set; }
        public virtual Subscriber Subscriber { get; set; }
        public virtual ICollection<PostLocation> PostLocations { get; set; }
        public virtual ICollection<SubmissionEmployeeMap> SubmissionEmployeeMaps { get; set; }
        public virtual ICollection<SubmissionLog> SubmissionLogs { get; set; }
        public virtual Form Form { get; set; }
    }
}
