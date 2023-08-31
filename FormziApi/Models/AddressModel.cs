using FormziApi.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FormziApi.Models
{
   public class AddressModel
    {
        public long Id { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string ZipPostalCode { get; set; }
        public string PhoneNumber { get; set; }
        public string FaxNumber { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public System.DateTime UpdatedOn { get; set; }
        public bool IsDeleted { get; set; }
        public int CountryId { get; set; }
        public int StateProvinceId { get; set; }
        public string CountryName { get; set; }
        public string StateName { get; set; }
       
    }
}
