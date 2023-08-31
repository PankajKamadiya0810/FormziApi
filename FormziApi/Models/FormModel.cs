using FormziApi.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FormziApi.Models
{
    public class FormModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public long CreatedBy { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public long UpdatedBy { get; set; }
        public System.DateTime UpdatedOn { get; set; }
        public int LanguageId { get; set; }
        public int ProjectId { get; set; }
        public int ClientId { get; set; }
        public Nullable<int> OrderBy { get; set; }
        public long EmployeeId { get; set; }
        public int SubscriberId { get; set; }
        public int FormVersionId { get; set; }
        public string WebFormUID { get; set; }
        public bool WorkFlowEnabled { get; set; }
        public Nullable<bool> IsPrivateForm { get; set; } //Added By Hiren 28-10-2017
        public bool IsVisibleSection { get; set; } //Added By Hiren 27-11-2017

        public long FormId { get; set; }
        public bool IsSelected { get; set; }

        public List<FormQuestionsModel> FormQuestions { get; set; }
        public object formQuestion { get; set; }//Added By Hiren 21-11-2017

        //Added by Jay on 23-Sep-2016
        public Nullable<decimal> Latitude { get; set; }
        public Nullable<decimal> Longitude { get; set; }

        //Added by Jay on 2-Aug-2017
        public string ProjectName { get; set; }
        public string ClientName { get; set; }

        //Added by Hiren on 18-11-2017
        public bool IsDashboard { get; set; }
        public int TotalSubmission { get; set; }
        public string LanguageName { get; set;}
    }

    public class FormByName
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }

    public class FormModelLess
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public int ClientId { get; set; }
        public int ProjectId { get; set; }
    }

    public class FormVersionModel
    {
        public int Id { get; set; }
        public long FormId { get; set; } 
        public int Version { get; set; }
        public long CreatedBy { get; set; }
        public System.DateTime CreatedOn { get; set; }  
    }

    public class FormsList
    {
        public FormModel Form { get; set; }
        public List<FormQuestionsModel> FormQuestions { get; set; }
        //public List<Formzi.Controllers.FormQuestionsController.FormQuestionsGroupByLanguage> FormQuestionByLanguage { get; set; }
    }

    //Added By Hiren 21-11-2017
    public class FormQuestionsGroupByLanguage
    {
        public int languageId { get; set; }
        public bool published { get; set; }
        public List<FormQuestionWithId> formQuestions { get; set; }
    }

    public class FormQuestionWithId
    {
        public long Id { get; set; }
        public string JSONQuestion { get; set; }
        public Nullable<long> ParentQuestionId { get; set; }
        public bool IsReadOnly { get; set; }
    }
    //End
}
