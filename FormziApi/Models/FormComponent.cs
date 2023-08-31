using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FormziApi.Models
{
    public class FormComponent
    {
        public int id { get; set; }
        public string component { get; set; }
        public bool editable { get; set; }
        public int index { get; set; }
        public string label { get; set; }
        public string description { get; set; }
        public string placeholder { get; set; }
        public List<string> options { get; set; }
        public bool required { get; set; }
        public string validation { get; set; }
        public bool draggable { get; set; }
        public Config config { get; set; }
        public Language language { get; set; }
        public bool isPrivate { get; set; }//Added By Hiren 16-11-2017
    }

    public class Config
    {
        public string max { get; set; }
        public string min { get; set; }
        public List<RoleModel> roles { get; set; }
        public bool sendEmail { get; set; }
        public bool isPrivate { get; set; }
    }

    public class Language
    {
        public string locale { get; set; }
        public string dir { get; set; }
    }
}