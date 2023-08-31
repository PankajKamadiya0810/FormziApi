using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FormziApi.Models
{
    //Created By Hiren 27-11-2017
    public class CommonFormQuestionsModel
    {
        public long Id { get; set; }
        public string Question { get; set; }
        public string JSONQuestion { get; set; }
        public int FormToolsId { get; set; }
        public Nullable<long> ParentQuestionId { get; set; }
        public bool IsReadOnly { get; set; }
    }
}