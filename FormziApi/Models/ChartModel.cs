using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FormziApi.Models
{
    public class ConditionObject
    {
        public string Question_id { get; set; }
        public string Question { get; set; }
        public string Rule { get; set; }
        public string Answer { get; set; }
    }

    public class Query
    {
        public string @operator { get; set; }
        public List<ConditionObject> condition_objects { get; set; }
    }

    public class Xaxis
    {
        public int id { get; set; }
        public string component { get; set; }
        public string label { get; set; }
        public bool required { get; set; }
        public List<string> options { get; set; }
    }

    public class Yaxis
    {
        public int id { get; set; }
        public string component { get; set; }
        public string label { get; set; }
        public bool required { get; set; }
        public List<string> options { get; set; }
    }

    public class Columns
    {
        public int id { get; set; }
        public string component { get; set; }
        public string label { get; set; }
        public bool required { get; set; }
        public List<string> options { get; set; }
    }

    public class Chart
    {
        public string type { get; set; }
        public string title { get; set; }
        public string xAxisTitle { get; set; }
        public string yAxisTitle { get; set; }
        public bool fullsize { get; set; }
        public bool showMinPoints { get; set; }
        public bool showMaxPoints { get; set; }
        public bool showAvgLines { get; set; }
        public string priority { get; set; }
        public string method { get; set; }
        public Xaxis xaxis { get; set; }
        public Yaxis yaxis { get; set; }
        public bool? showasPyramid { get; set; }
        public bool? showasDoughnut { get; set; }
        //public Query query { get; set; }
        public FilterRule query { get; set; }
        public string sqlQuery { get; set; }
        public List<Columns> columns { get; set; }
    }

    public class DashboardObjs
    {
        public string component { get; set; }
        public int index { get; set; }
        public Chart chart { get; set; }
        public object chartData { get; set; }
    }

    public class ChartModel
    {
        public int ID { get; set; }
        public long FormID { get; set; }
        public int FormVersionId { get; set; }
        public string FormName { get; set; }
        public int SubscriberId { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public List<DashboardObjs> DashboardObjs { get; set; }
        public object SubmissionCount { get; set; }

    }

    //public class XaxisAnswer
    //{
    //    public string xaxis { get; set; }
    //    public List<string> YaxisAnswer { get; set; }
    //}

    public class XAnswer
    {
        public string type { get; set; }
        public string title { get; set; }
    }

    public class XaxisAnswer
    {
        public XAnswer chart { get; set; }
    }
}