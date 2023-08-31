using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace FormziApi.Models
{
    public class PlanModel
    {
        public string RescourceKey { get; set; }

        public string PlanTypeName { get; set; }

        public int PlanId { get; set; }

       
        public string SubscriberName { get; set; }

        public int PlanTypeId { get; set; }

       
        public string PlanName { get; set; }

        public string Description { get; set; }

       
        public decimal SetUpFee { get; set; }

       
        public decimal SubscriptionFee { get; set; }

       
        public int MaxStoresAllowed { get; set; }

       
        public int DurationInMonths { get; set; }

        public bool Termsandconditions { get; set; }

        
        public bool Active { get; set; }
    }

}