using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FormziApi.Models
{
    public class LanguageModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string LanguageCulture { get; set; }
        public string UniqueSeoCode { get; set; }
        public string FlagImageFileName { get; set; }
        public bool Rtl { get; set; }
        public bool Published { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsDeleted { get; set; }
        public bool BaseLanguage { get; set; }
        public int LanguageId { get; set; }
        public virtual List<FormModel> Forms { get; set; }
        public virtual List<LocaleStringResourceModel> LocaleStringResources { get; set; }
    }
}