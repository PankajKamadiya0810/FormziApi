using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FormziApi.Models
{
    public class FormQuestionsModel
    {
        public long Id { get; set; }
        public string JSONQuestion { get; set; }
        public bool IsDeleted { get; set; }
        public int FormToolsId { get; set; }
        public long FormId { get; set; }
        public int FormVersionId {get; set;}
        public int LanguageId {get; set;}
        public bool IsPublished { get; set; }
        public virtual List<FormAnswerModel> FormAnswers { get; set; }
    }

    public class JSONQuestion
    {
        public string id { get; set; }
        public string component { get; set; }
        public bool label { get; set; }
        
    }
}