using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FormziApi.Models
{
    public class LocationModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ClientId { get; set; }
        public int OperationId { get; set; }
        public long AddressId { get; set; }

        public AddressModel Address { get; set; }

        public string FullAddress { get; set; }
    }
}