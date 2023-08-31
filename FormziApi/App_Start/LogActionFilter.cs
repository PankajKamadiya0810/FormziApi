using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
//using System.Web.Http.Filters; //for filter
using System.Web.Mvc; //for ActionFilterAttribute
using System.Web.Routing; //for RouteData

namespace FormziApi.App_Start
{
    public class LogActionFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            Log("OnActionExecuting", filterContext.RouteData);
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            Log("OnActionExecuted", filterContext.RouteData);
        }

        public override void OnResultExecuting(ResultExecutingContext filterContext)
        {
            Log("OnResultExecuting", filterContext.RouteData);

            #region Commented code of Web MVC
            //var actionName = filterContext.RouteData.Values["action"];
            //var controllerName = filterContext.RouteData.Values["controller"];

            //if (filterContext != null)
            //{
            //    HttpSessionStateBase objHttpSessionStateBase = filterContext.HttpContext.Session;
            //    var userSession = objHttpSessionStateBase["userInfo"];
            //    if (((userSession == null) && (!objHttpSessionStateBase.IsNewSession)) || (objHttpSessionStateBase.IsNewSession))
            //    {
            //        objHttpSessionStateBase.RemoveAll();
            //        objHttpSessionStateBase.Clear();
            //        objHttpSessionStateBase.Abandon();
            //        if (filterContext.HttpContext.Request.IsAjaxRequest())
            //        {
            //            filterContext.HttpContext.Response.StatusCode = 403;
            //            filterContext.Result = new JsonResult { Data = "LogOut" };
            //        }
            //        else
            //        {
            //            filterContext.Result = new RedirectResult("~/Home/Index");
            //        }
            //    }
            //    else
            //    {
            //        //if (!CheckAccessRight(actionName, controllerName))
            //        //{
            //        //string redirectUrl = string.Format("?returnUrl={0}", filterContext.HttpContext.Request.Url.PathAndQuery);

            //        //filterContext.HttpContext.Response.Redirect(FormsAuthentication.LoginUrl + redirectUrl, true);
            //        //}
            //        //else
            //        //{
            //        //    base.OnActionExecuting(filterContext);
            //        //}
            //    }
            //} 
            #endregion
        }

        public override void OnResultExecuted(ResultExecutedContext filterContext)
        {
            Log("OnResultExecuted", filterContext.RouteData);
        }


        private void Log(string methodName, RouteData routeData)
        {
            var controllerName = routeData.Values["controller"];
            var actionName = routeData.Values["action"];
            var message = String.Format("{0} controller:{1} action:{2}", methodName, controllerName, actionName);
            System.Diagnostics.Debug.WriteLine(message, "Action Filter Log");
        }

    }
}