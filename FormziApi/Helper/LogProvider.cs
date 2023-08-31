using FormziApi.Database;
using log4net;
using log4net.Config;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Web;

namespace FormziApi.Helper
{
    public class LogProvider
    {
        #region Fields

        private ILog _Log;
        private FormziEntities db = new FormziEntities();
        #endregion

        #region Methods

        public LogProvider(string className)
        {
            XmlConfigurator.Configure();
            _Log = LogManager.GetLogger(className);
        }

        public void Info(object message)
        {
            _Log.Info(message);
        }

        public void HandleError(Exception ObjExection, string Message)
        {
            try
            {
                if (ObjExection.GetType() == typeof(System.Threading.ThreadAbortException))
                {
                    return;
                }
                //Object of LogManager class, that will use in writing exception in error log file
                ILog objLog = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType.FullName);
                string strErrorHandlerToEmail = "";
                int subscriberId = int.Parse(ConfigurationManager.AppSettings["InnfySubscriberId"].ToString());

                Subscriber subscriberdbModel = db.Subscribers.Where(i => i.Id == subscriberId).FirstOrDefault();
                strErrorHandlerToEmail = subscriberdbModel.AppSettings.Where(i => i.Key == Constants.ERROR_HANDLER_TOEMAIL).FirstOrDefault().Value;
                //Error logger function called for inserting error details in text file
                objLog.Error(ObjExection.Message, ObjExection);

                string Filepath = "ErrorFile.htm";

                Filepath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                Filepath = System.IO.Path.GetDirectoryName(Filepath);

                // Email template used as a Format of this email about error notification
                StringBuilder strMailBody = new StringBuilder();
                strMailBody.Append("<!DOCTYPE html PUBLIC '-//W3C//DTD XHTML 1.0 Transitional//EN' 'http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd'>");
                strMailBody.Append("<html xmlns='http://www.w3.org/1999/xhtml'>");
                strMailBody.Append("<head>");
                strMailBody.Append("<title>Error Message Alert</title>");
                strMailBody.Append("</head>");
                strMailBody.Append("<body>");
                strMailBody.Append("<table cellpadding='0' cellspacing='3'>");
                strMailBody.Append("<tr>");
                strMailBody.Append("<td colspan='2'>");
                strMailBody.Append("Dear Administrator,");
                strMailBody.Append("</td>");
                strMailBody.Append("</tr>");
                strMailBody.Append("<tr>");
                strMailBody.Append("<td colspan='2'>");
                strMailBody.Append("An error ocurred in Formzi website. Following are the details of the" + System.Environment.NewLine);
                strMailBody.Append("error:");
                strMailBody.Append("</td>");
                strMailBody.Append("</tr>");
                strMailBody.Append("<tr>");
                strMailBody.Append("<td width='120px'>");
                strMailBody.Append("<b>Error Message:</b>");
                strMailBody.Append("</td>");
                strMailBody.Append("<td width='450px'>");
                strMailBody.Append("{ERROR_MESSAGE}");
                strMailBody.Append("</td>");
                strMailBody.Append("</tr>");
                strMailBody.Append("<tr>");
                strMailBody.Append("<td>");
                strMailBody.Append("<b>Date:</b>");
                strMailBody.Append("</td>");
                strMailBody.Append("<td>");
                strMailBody.Append("{DATETIME}");
                strMailBody.Append("</td>");
                strMailBody.Append("</tr>");
                strMailBody.Append("<tr>");
                strMailBody.Append("<td valign='top'>");
                strMailBody.Append("<b>Stack Trace:</b>");
                strMailBody.Append("</td>");
                strMailBody.Append("<td valign='top'>");
                strMailBody.Append("{STACK_TRACE}");
                strMailBody.Append(Message);
                strMailBody.Append("</td>");
                strMailBody.Append("</tr>");
                strMailBody.Append("<tr>");
                strMailBody.Append("<td colspan='2'>");
                strMailBody.Append("<br/>");
                strMailBody.Append("<br/>");
                strMailBody.Append("<b>");
                strMailBody.Append("From,");
                strMailBody.Append("</br>");
                strMailBody.Append("Formzi Website Error Notification.");
                strMailBody.Append("</b>");
                strMailBody.Append("</td>");
                strMailBody.Append("</tr>");
                strMailBody.Append("<tr>");
                strMailBody.Append("<td>");
                strMailBody.Append("<br/>");
                strMailBody.Append("</td>");
                strMailBody.Append("</tr>");
                strMailBody.Append("</table>");
                strMailBody.Append("</body>");
                strMailBody.Append("</html>");
                string MailBody = strMailBody.ToString();

                //Body of email that contains template string with exception message
                MailBody = MailBody.Replace("{ERROR_MESSAGE}", ObjExection.Message);
                MailBody = MailBody.Replace("{STACK_TRACE}", ObjExection.StackTrace + ".");
                MailBody = MailBody.Replace("{DATETIME}", DateTime.Now.ToString());

                //Subject of error notification email
                string Subject = "Error handler service - Error caught in Formzi Website";

                int mintSmtpAuthenticate = 0;
                String mstrSendUserName = "";
                String mstrSendPassword = "";
                String mstrUseSSL = "";
                String mstrSMTPPort = "";
                String mstrSMTPServer = "";

                string strmintSmtpAuthenticate = Convert.ToString(ConfigurationManager.AppSettings["SmtpAuthenticate"]);
                mintSmtpAuthenticate = Convert.ToInt32(strmintSmtpAuthenticate);
                string strmstrSendUserName = Convert.ToString(ConfigurationManager.AppSettings["SenderUserName"]);
                mstrSendUserName = strmstrSendUserName;
                string strmstrSendPassword = Convert.ToString(ConfigurationManager.AppSettings["SenderPassword"]);
                mstrSendPassword = strmstrSendPassword;
                string strmstrUseSSL = Convert.ToString(ConfigurationManager.AppSettings["UseSSL"]);
                mstrUseSSL = strmstrUseSSL;
                string strmstrSMTPPort = Convert.ToString(ConfigurationManager.AppSettings["SMTPPort"]);
                mstrSMTPPort = strmstrSMTPPort;
                string strmstrSMTPServer = Convert.ToString(ConfigurationManager.AppSettings["SMTPServer"]);
                mstrSMTPServer = strmstrSMTPServer;

                System.Net.Mail.MailMessage lobjMail = new System.Net.Mail.MailMessage();
                SmtpClient sc = new System.Net.Mail.SmtpClient();
                System.Net.NetworkCredential auth = new System.Net.NetworkCredential(mstrSendUserName, mstrSendPassword);

                string strAdminEmail = Convert.ToString(ConfigurationManager.AppSettings["ErrorHandlerFromEmail"]);
                lobjMail.From = new MailAddress(strAdminEmail);

                string strEmailTo = strErrorHandlerToEmail;//Convert.ToString(ConfigurationManager.AppSettings["ErrorHandlerToEmail"]);

                lobjMail.To.Add(new MailAddress(strEmailTo));

                lobjMail.Subject = Subject;
                lobjMail.Body = MailBody.ToString();

                if (!string.IsNullOrEmpty(strmstrSMTPServer))
                {
                    sc.Host = mstrSMTPServer;
                }

                sc.Port = Convert.ToInt32(mstrSMTPPort);
                lobjMail.IsBodyHtml = true;
                sc.UseDefaultCredentials = false;
                sc.Credentials = auth;
                sc.EnableSsl = strmstrUseSSL == "1" ? true : false;

                //Send Email
                sc.Send(lobjMail);
            }
            catch (Exception)
            { }
        }

        #endregion
    }
}