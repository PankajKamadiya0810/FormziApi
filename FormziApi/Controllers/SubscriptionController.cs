using FormziApi.Database;
using FormziApi.Helper;
using FormziApi.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FormziApi.Controllers
{
    [Authorize]
    public class SubscriptionController : Controller
    {
        #region Fields

        LogProvider lp = new LogProvider("Formzi");
        private FormziEntities db = new FormziEntities(); 
        
        #endregion

        #region Methods

        [Route("api/addsubscription")]
        [HttpPost]
        // PlanModel plan,SubscriberModel subscriber
        public int InsertSubscription(JObject subscriberplan)
        {
            try
            {
                var subscriber = subscriberplan["subscriber"].ToObject<Subscriber>();
                var plan = subscriberplan["plan"].ToObject<SubscriptionPlan>();
                subscriber.CreatedOn = subscriber.UpdatedOn = Common.GetDateTime(db);
                db.Subscribers.Add(subscriber);
                db.SubscriptionPlans.Add(plan);
                db.SaveChanges();
                return subscriber.Id;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return 0;
            }

        } 
        
        #endregion
    }
}