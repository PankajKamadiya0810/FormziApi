using FormziApi.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FormziApi.Models
{
    public class FormSubmissionModel
    {
        public FormSubmissionModel()
        {
            FormAnswers = new List<FormAnswerModel>();
        }
        public long Id { get; set; }
        public string DeviceId { get; set; }
        public string LatLong { get; set; }
        public long SubmittedBy { get; set; }
        public System.DateTime SubmittedOn { get; set; }
        public Nullable<long> ApprovedBy { get; set; }
        public Nullable<System.DateTime> ApprovedOn { get; set; }
        public Nullable<int> IsApproved { get; set; }
        public long EmployeeId { get; set; }
        public long FormId { get; set; }
        public int SubscriberId { get; set; }
        public string FormName { get; set; }
        public string FormImage { get; set; }
        public bool IsProcessing { get; set; }
        public System.Guid FolderName { get; set; }
        public List<FormAnswerModel> FormAnswers { get; set; }
        public List<FormQuestionsModel> FormQuestions { get; set; }
        public bool IsCompleted { get; set; }

        public bool IsDeleted { get; set; } //Added by Pankaj on 28-Aug-2017

        //Added by Juned Lanja on 23-SEP-2015
        public int VersionId { get; set; }
        public int LanguageId { get; set; }
        //End

        //Added by Jay on 22-Feb-2016
        public Nullable<decimal> Latitude { get; set; }
        public Nullable<decimal> Longitude { get; set; }
        public Nullable<long> AssignTo { get; set; }
        public string AssignToName { get; set; }
        //End

        //Added by Jay on 6-June-2016
        public AppUserInfo AppUserInfo { get; set; }
        //End

        public Nullable<int> AppInfoID { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNo { get; set; }

        //Post location
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

        public string Type { get; set; }
        public string Category { get; set; }
        public string Severity { get; set; }
        public string Description { get; set; }
        public string Images { get; set; }
        public object ImageList { get; set; }
    }
}