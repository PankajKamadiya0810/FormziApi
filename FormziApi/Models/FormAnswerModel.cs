using FormziApi.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FormziApi.Models
{
    public class FormAnswerModel
    {
        private FormziEntities db = new FormziEntities();

        private DateTime _date;
        public FormAnswerModel()
        {
            _date = DateTime.Now;
        }

        public long Id { get; set; }
        public string Value { get; set; }
        public System.DateTime CreatedOn
        {
            get { return _date; }
            set { _date = Common.GetDateTime(db); }
        }
        public int CreatedBy { get; set; }
        public System.DateTime UpdatedOn
        {
            get { return _date; }
            set { _date = Common.GetDateTime(db); }
        }
        public int UpdatedBy { get; set; }
        public long FormQuestionId { get; set; }
        public long FormSubmissionId { get; set; }
        public string Component { get; set; }

        public int ElementType { get; set; }
        public bool IsReadOnly { get; set; }
        public string JSONQuestion { get; set; }
        public string Question { get; set; }
    }
}