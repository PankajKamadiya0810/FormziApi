using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FormziApi.Models
{
    public class AppLoginModel
    {
        public AppLoginModel()
        {
            IsWebEnabled = true;
        }
        public long Id { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool IsWebEnabled { get; set; }
        public bool IsMobileEnabled { get; set; }
        public int SubscriberId { get; set; }
        public bool IsReset { get; set; }
        public bool IsDeleted { get; set; }
        public List<EmployeeRoleModel> EmployeeRoles { get; set; }
    }
}