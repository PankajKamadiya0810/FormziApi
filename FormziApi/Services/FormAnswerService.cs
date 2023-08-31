using FormziApi.Database;
using FormziApi.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Http;

namespace FormziApi.Services
{
    //Added by jay mistry 
    public class FormAnswerService
    {
        #region Fields
        LogProvider lp;
        private FormziEntities db;
        #endregion

        #region Constructor
        public FormAnswerService()
        {
            lp = new LogProvider("Formzi");
            db = new FormziEntities();
        }
        #endregion

        #region Methods

        /// <summary>
        /// Email for new submission 
        /// Innfy specific 
        /// </summary>
        /// <param name="ToEmailAddress"></param>
        /// <param name="MailBody"></param>
        /// <param name="Subject"></param>
        private void SendEmail(string ToEmailAddress, string MailBody, string Subject)
        {
            try
            {
                string FromAddress = Convert.ToString(ConfigurationManager.AppSettings["InnfyRecipientFromEmail"]);

                int mintSmtpAuthenticate = 0;
                String mstrSendUserName = "";
                String mstrSendPassword = "";
                String mstrUseSSL = "";
                String mstrSMTPPort = "";
                String mstrSMTPServer = "";

                string strmintSmtpAuthenticate = Convert.ToString(ConfigurationManager.AppSettings["SmtpAuthenticate"]);
                mintSmtpAuthenticate = Convert.ToInt32(strmintSmtpAuthenticate);

                string strmstrSendUserName = Convert.ToString(ConfigurationManager.AppSettings["InnfySenderUserName"]);
                mstrSendUserName = strmstrSendUserName;

                string strmstrSendPassword = Convert.ToString(ConfigurationManager.AppSettings["InnfySenderPassword"]);
                mstrSendPassword = strmstrSendPassword;

                string strmstrUseSSL = Convert.ToString(ConfigurationManager.AppSettings["UseSSL"]);
                mstrUseSSL = strmstrUseSSL;

                string strmstrSMTPPort = Convert.ToString(ConfigurationManager.AppSettings["SMTPPort"]);
                mstrSMTPPort = strmstrSMTPPort;

                string strmstrSMTPServer = Convert.ToString(ConfigurationManager.AppSettings["SmtpServer"]);
                mstrSMTPServer = strmstrSMTPServer;

                System.Net.Mail.MailMessage lobjMail = new System.Net.Mail.MailMessage();
                SmtpClient sc = new System.Net.Mail.SmtpClient();
                System.Net.NetworkCredential auth = new System.Net.NetworkCredential(mstrSendUserName, mstrSendPassword);

                lobjMail.Body = MailBody;
                lobjMail.Subject = Subject;
                lobjMail.From = new MailAddress(FromAddress, "");
                lobjMail.To.Add(new MailAddress(ToEmailAddress));
                //lobjMail.CC.Add(new MailAddress(ConfigurationManager.AppSettings["InnfyAdminEmailcc"].ToString()));
                //lobjMail.Bcc.Add(new MailAddress(ConfigurationManager.AppSettings["InnfyAdminEmailbcc"].ToString()));

                if (!string.IsNullOrEmpty(strmstrSMTPServer))
                {
                    sc.Host = mstrSMTPServer;
                }
                sc.Port = Convert.ToInt32(mstrSMTPPort);
                lobjMail.IsBodyHtml = true;
                sc.UseDefaultCredentials = false;
                sc.Credentials = auth;
                sc.EnableSsl = ConfigurationManager.AppSettings["UseSSL"] == "0" ? false : true;
                sc.Send(lobjMail);
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                throw e;
            }
        }

        public bool SendNewSubmissionMail(int subscriberId)
        {
            try
            {
                int languageId = 1;
                string subscriberLogo = "";
                var logoFileUrl = "";
                string emailSignature = "";
                subscriberId = subscriberId == 0 ? 1 : subscriberId;
                string strTemplate = "";
                string strInnfyAdminEmail = "";

                Subscriber subscriberdbModel = db.Subscribers.Where(i => i.Id == subscriberId).FirstOrDefault();

                SubscriberLanguage subscriberModel = subscriberdbModel.SubscriberLanguages.OrderBy(o => o.DisplayOrder).FirstOrDefault();
                logoFileUrl = subscriberdbModel.AppSettings.Where(i => i.Key == Constants.FileUrl).FirstOrDefault().Value;
                strInnfyAdminEmail = subscriberdbModel.AppSettings.Where(i => i.Key == Constants.INNFY_ADMIN_EMAIL).FirstOrDefault().Value;
                subscriberLogo = !string.IsNullOrEmpty(subscriberdbModel.CompanyLogo) ? logoFileUrl + subscriberdbModel.Id + Constants.ImageFolder + Constants.SubscriberFolder + subscriberdbModel.CompanyLogo : "";

                if (subscriberModel != null)
                {
                    languageId = subscriberModel.LanguageId;
                }

                AppSetting appSettingModel = subscriberdbModel.AppSettings.Where(i => i.Key == Constants.EmailSignature).FirstOrDefault();

                if (appSettingModel != null)
                {
                    emailSignature = appSettingModel.Value;
                }

                string mailFilePath = "~/MailTemplates/Languages/" + languageId + "/NewSubmissionMail.html";
                StreamReader fp = new StreamReader(System.Web.HttpContext.Current.Server.MapPath(mailFilePath));
                strTemplate = fp.ReadToEnd();
                fp.Close();

                strTemplate = strTemplate.Replace("@EmailSignature", emailSignature).Replace("@CompanyLogo", subscriberLogo).Replace("@PreferredDomain", !string.IsNullOrEmpty(subscriberModel.Subscriber.PreferredDomain) ? subscriberModel.Subscriber.PreferredDomain : subscriberModel.Subscriber.SubDomain + "." + Constants.FORMZI_DOMAIN).Replace("@CompanyName", subscriberdbModel.CompanyName);

                //New submission mail to innfy admin
                //ConfigurationManager.AppSettings["InnfyAdminEmail"].ToString() Changed by Pankaj on 31-Aug-2017
                SendEmail(strInnfyAdminEmail, strTemplate, "New submission.");

                return true;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                throw e;
            }
        }

        public bool AddPostLocation(string latitude, string longitude, long submissionId)
        {
            try
            {
                string route = "";
                string shortName = "";
                string country = "";
                string administrative_area_level_1 = "";
                string administrative_area_level_2 = "";
                string administrative_area_level_3 = "";
                string colloquial_area = "";
                string locality = "";
                string sublocality = "";
                string neighborhood = "";
                string postal_code = "";

                ReverseGeoLoc.GetGeoLoction(latitude, longitude,
                out route,
                out shortName,
                out country,
                out administrative_area_level_1,
                out administrative_area_level_2,
                out administrative_area_level_3,
                out colloquial_area,
                out locality,
                out sublocality,
                out neighborhood,
                out postal_code);

                PostLocation model = new PostLocation();
                model.administrative_area_level_1 = administrative_area_level_1;
                model.administrative_area_level_2 = administrative_area_level_2;
                model.administrative_area_level_3 = administrative_area_level_3;
                model.colloquial_area = colloquial_area;
                model.country = country;
                model.locality = locality;
                model.neighborhood = neighborhood;
                model.postal_code = postal_code;
                model.route = route;
                model.shortName = shortName;
                model.sublocality = sublocality;
                model.SubmissionId = submissionId;

                if (!string.IsNullOrEmpty(model.country))
                {
                    db.PostLocations.Add(model);
                    db.SaveChanges();
                }
                return true;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                //throw e;
                return false;
            }
        }

        #endregion
    }
}