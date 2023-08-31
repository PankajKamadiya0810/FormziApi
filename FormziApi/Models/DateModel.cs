using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FormziApi.Models
{
    public class SubscriberFormsDateModel
    {
        public int SubscriberId { get; set; }
        public System.DateTime DateTime { get; set; }
        public string AuthKey { get; set; }
    }
    public class EmployeeFormsDateModel
    {
        public int EmployeeId { get; set; }
        public System.DateTime DateTime { get; set; }
    }
    public class UpdateEmpFormsModel
    {
        public long EmployeeId { get; set; }
        public List<EmployeeFormModel> EmployeeForms { get; set; }
    }

    public class FormSubmissionDateModel
    {
        public long FormSubmissionId { get; set; }
        public System.DateTime DateTime { get; set; }
        public string LastUpdatedDate { get; set; }
        public int FilterId { get; set; }
        public string UserDate { get; set; }
        public int SubscriberId { get; set; }
    }

    public class SyncClientsModel
    {
        public int SubscriberId { get; set; }
        public System.DateTime DateTime { get; set; }
    }

    public class SyncProjectsModel
    {
        public List<int> Clients { get; set; }
        public System.DateTime DateTime { get; set; }
    }

    public class SyncProjectFormModel
    {
        public List<int> Projects { get; set; }
        public System.DateTime DateTime { get; set; }
    }

    //Innfy specific
    //Added by jay on 18-2-2016
    public class FormSubmissionLatLngDateModel
    {
        public long FormSubmissionId { get; set; }
        public System.DateTime DateTime { get; set; }
        public string LastUpdatedDate { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public double Distance { get; set; }
        public string SelectedForms { get; set; }

        //Added by jay mistry 21-6-2016
        public int SubscriberId { get; set; }
        public string AuthKey { get; set; }
    }

    public class InnfyFormSubmissionByMe
    {
        public int SubscriberId { get; set; }
        public string DeviceId { get; set; }
        public int IsApproved { get; set; }
        public string AuthKey { get; set; }

    }

    public class InnfyFormSubmission
    {

        public long FormSubmissionId { get; set; }
        public Nullable<decimal> Latitude { get; set; }
        public Nullable<decimal> Longitude { get; set; }
        public long FormId { get; set; }
        public System.Guid FolderName { get; set; }
        public string LatLong { get; set; }
        public System.DateTime SubmittedOn { get; set; }
        public int SubscriberId { get; set; }
        public Nullable<int> IsApproved { get; set; }
        public string FormName { get; set; }
        public Nullable<double> Distance { get; set; }
        public string FormImage { get; set; }
        public List<InnfyFormAnswer> FormAnswers { get; set; }
        public string DeviceId { get; set; }
        public int FormCount { get; set; }
    }

    public class InnfyFormAnswer
    {
        public long FormAnswerId { get; set; }
        public string FormAnswerValue { get; set; }
        public int FormAnswerElementType { get; set; }
        public long FormQuestionId { get; set; }
        public long FormSubmissionId { get; set; }
    }

    //Added by Jay on 13-April-2016
    public class EmployeeProjectFormDateModel
    {
        public int EmployeeId { get; set; }
        public System.DateTime DateTime { get; set; }
    }
}
