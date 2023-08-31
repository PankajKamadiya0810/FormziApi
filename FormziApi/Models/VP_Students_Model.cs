using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FormziApi.Models
{
    public class VP_Students_Model
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public int SubscriberId { get; set; }
        public string IdentityNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool Gender { get; set; }
        public string DOB { get; set; }
        public Nullable<short> Age { get; set; }
        public short Grade { get; set; }
        public string GuardianName { get; set; }
        public string GuardianContactNumber { get; set; }
        public string Address { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<System.DateTime> UpdatedOn { get; set; }
        public Nullable<int> UpdatedBy { get; set; }

        public string GenderStr { get; set; }

        public int MobileId { get; set; }

        public long AddressId { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string ZipPostalCode { get; set; }
        public string PhoneNumber { get; set; }
        public string FaxNumber { get; set; }
        public bool IsDeleted { get; set; }
        public int CountryId { get; set; }
        public int StateProvinceId { get; set; }
        public string CountryName { get; set; }
        public string StateName { get; set; }

        public List<FormziApi.Database.FormAnswer> FormAnswers { get; set; }
    }
}