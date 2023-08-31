using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FormziApi.Models
{
    public class EmployeeRoleModel
    {
        public int Id { get; set; }
        public long AppLoginId { get; set; }
        public int RoleId { get; set; }
        public int OperationId { get; set; }
        public string RoleName { get; set; }
    }
}