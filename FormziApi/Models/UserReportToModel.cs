using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FormziApi.Database;

namespace FormziApi.Models
{
    public class UserReportToModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public int ReportToId { get; set; }
        public int RoleId { get; set; }
    }
}