using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;

namespace FormziApi.App_Start
{
    public class VideoDataResult : ActionResult
    {
        /// <summary>
        /// The below method will respond with the Video file
        /// </summary>
        /// <param name="context"></param>
        public override void ExecuteResult(ControllerContext context)
        {
            //Uri theRealURL = new Uri(HttpContext.Current.Request.Url.Scheme + "://" + HttpContext.Current.Request.Url.Authority + HttpContext.Current.Request.RawUrl);
            //string videoFileName = HttpUtility.ParseQueryString(theRealURL.Query).Get("name");
            //videoFileName = "~/VideoFiles/" + videoFileName;

            string videoFileName = context.HttpContext.Request.QueryString["name"];

            string path = context.HttpContext.Request.QueryString["path"];

            var strVideoFilePath = HostingEnvironment.MapPath("~/VideoFiles/" + videoFileName);

            var attachmentName = "attachment; filename=" + videoFileName;

            context.HttpContext.Response.AddHeader("Content-Disposition", attachmentName);

            var objFile = new FileInfo(strVideoFilePath);

            var stream = objFile.OpenRead();
            var objBytes = new byte[stream.Length];
            stream.Read(objBytes, 0, (int)objFile.Length);
            context.HttpContext.Response.BinaryWrite(objBytes);

        }
    }
}