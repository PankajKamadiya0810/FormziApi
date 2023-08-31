using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FormziApi.Models
{
    public class Head
    {
        public string Area_ID { get; set; }
        public string Area_Name { get; set; }
        public string Area_Name_Gujarati { get; set; }
        public string Ward_ID { get; set; }
        public string Ward_Name { get; set; }
        public string Zone_ID { get; set; }
        public string Zone_Name { get; set; }
    }

    public class AmcReturnData
    {
        public int Code { get; set; }
        public List<Head> Head { get; set; }
    }

    public class AmcCheckCode
    {
        public int Code { get; set; }
    }

    public class AmcTokenData
    {
        public int Code { get; set; } //code = '1' for true and '0' for false
        public string Head { get; set; } //SWM-09161092565   <--- its token
    }

    public class WardList
    {
        public string Ward_ID { get; set; }
        public string Ward_Name { get; set; }
    }

    public class ZoneList
    {
        public string Zone_ID { get; set; }
        public string Zone_Name { get; set; }
    }

    public class AreaList
    {
        public string Area_ID { get; set; }
        public string Area_Name { get; set; }
    }

    public class Complaint
    {
        public string ComplainantMobile { get; set; }
        public string Remarks { get; set; }
        public string ComplainantName { get; set; }
        public string ComplainantAddress { get; set; }
        public string ComplainantEmailID { get; set; }
        public int ProblemID { get; set; }
        public string ComplainantContact { get; set; }
        public int WardID { get; set; }
        public int AreaID { get; set; }
        public string LocationAddr { get; set; }

        public long SubmissionId { get; set; }
        public string SubmissionImages { get; set; }
        public int SubscriberId { get; set; }

    }

}