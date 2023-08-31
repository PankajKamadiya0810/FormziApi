using FormziApi.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FormziApi.Models
{
    public class FormAnswerExport
    {
        public long Id { get; set; }
        public string LatLong { get; set; }
        
        public long FormId { get; set; }
        public long SubmissionId { get; set; }

        public int SubscriberId { get; set; }
        public string FormName { get; set; }
        public string FormImage { get; set; }
        public System.Guid FolderName { get; set; }

        public string route { get; set; }
        public string administrative_area_level_1 { get; set; }
        public string administrative_area_level_2 { get; set; }
        public string administrative_area_level_3 { get; set; }
        public string colloquial_area { get; set; }
        public string locality { get; set; }
        public string sublocality { get; set; }
        public string neighborhood { get; set; }
        public string postal_code { get; set; }
        public string country { get; set; }
        public string shortName { get; set; }

        public Nullable<int> AppInfoID { get; set; }
        public string Name { get; set; }
        public string DeviceId { get; set; }
        public string Email { get; set; }
        public string PhoneNo { get; set; }

        public IEnumerable<FormAnswerModel> FormAnswers { get; set; }

        public string Value { get; set; }

        

    }
}