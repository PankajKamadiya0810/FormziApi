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
    
    public partial class PostLocation
    {
        public long Id { get; set; }
        public long SubmissionId { get; set; }
        public string route { get; set; }
        public string administrative_area_level_1 { get; set; }
        public string administrative_area_level_2 { get; set; }
        public string administrative_area_level_3 { get; set; }
        public string colloquial_area { get; set; }
        public string locality { get; set; }
        public string sublocality { get; set; }
        public string neighborhood { get; set; }
        public string postal_code { get; set; }
        public string country { get; set; }
        public string shortName { get; set; }
    
        public virtual FormSubmission FormSubmission { get; set; }
    }
}
