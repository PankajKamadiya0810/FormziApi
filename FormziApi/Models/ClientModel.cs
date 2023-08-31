using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FormziApi.Models
{
    public class ClientModel
    {
        public int Id { get; set; }
        public string UniqueNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CompanyName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Notes { get; set; }
        public bool ReceiveReminders { get; set; }
        public int SubscriberId { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public System.DateTime UpdatedOn { get; set; }
        public bool IsDeleted { get; set; }
        public List<LocationModel> Locations { get; set; }
    }

    public class ClientDataModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CompanyName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public System.DateTime UpdatedOn { get; set; }
        public bool IsDeleted { get; set; }
        public int SubscriberId { get; set; }

        public List<ProjectDataModel> ProjectList { get; set; }
    }
}