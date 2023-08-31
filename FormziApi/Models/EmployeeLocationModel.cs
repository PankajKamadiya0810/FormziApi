using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FormziApi.Models
{
    public class EmployeeLocationModel
    {
        public int Id { get; set; }
        public long EmployeeId { get; set; }
        public int LocationId { get; set; }
        public int OperationId { get; set; }
    }
}