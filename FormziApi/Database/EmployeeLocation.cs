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
    
    public partial class EmployeeLocation
    {
        public int Id { get; set; }
        public long EmployeeId { get; set; }
        public int LocationId { get; set; }
    
        public virtual Employee Employee { get; set; }
        public virtual Location Location { get; set; }
    }
}
