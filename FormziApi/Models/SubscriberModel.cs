using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FormziApi.Models
{
    public class SubscriberModel
    {

        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ProfilePic { get; set; }
        public string Website { get; set; }
        public string CompanyName { get; set; }
        public string PreferredDomain { get; set; }
        public string CompanyLogo { get; set; }
        public string EmailVerificationCode { get; set; }
        public bool IsEmailVerified { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public System.DateTime UpdatedOn { get; set; }
        public bool IsDeleted { get; set; }
        public long AddressId { get; set; }
        public long EmployeeId { get; set; }
        public string Email { get; set; }
        public int SubscriptionPlanId { get; set; }
        public string SubDomain { get; set; }
        public  AddressModel Address { get; set; }
        public SubscriptionPlanModel SubscriptionPlan { get; set; }
    }
    
}