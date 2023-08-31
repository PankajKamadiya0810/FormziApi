using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FormziApi.Models
{
    public class WardModel
    {
        public int Id { get; set; }
        public int SubscriberId { get; set; }
        public int ZoneId { get; set; }
        public string Name { get; set; }
        public Nullable<System.DateTime> UpdatedOn { get; set; }
        public Nullable<System.DateTime> CreatedOn { get; set; }
        public bool IsDeleted { get; set; }

        public string ZoneName { get; set; }
    }
}