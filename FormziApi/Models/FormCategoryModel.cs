using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FormziApi.Models
{
    public class FormCategoryModel
    {
        public int Id { get; set; }
        public long FormId { get; set; }
        public int CategoryId { get; set; }

        public  CategoryModel Category { get; set; }
        public  FormModel Form { get; set; }
    }
}