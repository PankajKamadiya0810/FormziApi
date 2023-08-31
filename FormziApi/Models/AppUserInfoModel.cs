using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace FormziApi.Models
{
    public class AppUserInfoModel
    {
      
            public int AppInfoID { get; set; }
            public string Name { get; set; }
            public string DeviceId { get; set; }
            public string Email { get; set; }
            public string PhoneNo { get; set; }
            public string OTP { get; set; }
            public bool IsOTPVerified { get; set; }
            public int SubscriberId { get; set; }
            public string AuthKey { get; set; }
    }
}