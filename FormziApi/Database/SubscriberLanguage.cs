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
    
    public partial class SubscriberLanguage
    {
        public int Id { get; set; }
        public int SubcriberId { get; set; }
        public int LanguageId { get; set; }
        public bool IsPublished { get; set; }
        public int DisplayOrder { get; set; }
    
        public virtual Language Language { get; set; }
        public virtual Subscriber Subscriber { get; set; }
    }
}