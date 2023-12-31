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
    
    public partial class AppUserInfo
    {
        public AppUserInfo()
        {
            this.Notifications = new HashSet<Notification>();
        }
    
        public int AppInfoID { get; set; }
        public string Name { get; set; }
        public string DeviceId { get; set; }
        public string Email { get; set; }
        public string PhoneNo { get; set; }
        public string OTP { get; set; }
        public bool IsOTPVerified { get; set; }
        public string EmailVerificationCode { get; set; }
        public bool IsEmailVerified { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public Nullable<System.DateTime> UpdatedOn { get; set; }
    
        public virtual ICollection<Notification> Notifications { get; set; }
    }
}
