using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FormziApi.Models
{
    public class ApiReturnData
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public object SuccessData { get; set; }
        public int StatusCode { get; set; }
    }
}