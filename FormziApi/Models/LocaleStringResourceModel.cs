using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FormziApi.Models
{
    public class LocaleStringResourceModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public bool IsHtml { get; set; }
        public bool IsDeleted { get; set; }
        public int OperationId { get; set; }
        public int LanguageId { get; set; }
        public int SubscriberId { get; set; }

        public string LanguageName { get; set; }
        public string SubscriberName { get; set; }

        public bool IsMobileEnabled { get; set; }
        public bool IsWebEnabled { get; set; }

        public int ResourceType { get; set; }

        public  LanguageModel Language { get; set; }
        public  OperationModel Operation { get; set; }
        public  SubscriberModel Subscriber { get; set; }
    }
}