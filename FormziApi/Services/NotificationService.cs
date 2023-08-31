using FormziApi.Database;
using FormziApi.Helper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Web;

namespace FormziApi.Services
{
    //This class file is added by Jay Mistry on 14-6-2016 Tuesday
    public class NotificationService
    {
        #region Fields

        LogProvider lp;
        private FormziEntities db; 
        
        #endregion

        #region Constructor
        
        public NotificationService()
        {
            lp = new LogProvider("Formzi");
            db = new FormziEntities();

        } 

        #endregion

        #region Methods

        public bool AppUserNotification(int isApproved, long formSubmissionId, string deviceId, int subscriberId, int? appInfoId)
        {
            try
            {
                //0 == Reject
                //1 == Approved
                //3 == Resolve
                //5 == New Post Added
                //10 == Assign to employee

                //NOTIFICATION_REJECT = 0;
                //NOTIFICATION_APPROVED = 1;
                //NOTIFICATION_RESOLVE = 3;
                //NOTIFICATION_NEW_POST_ADDED = 5;
                //NOTIFICATION_ASSIGN_TO_EMPLOYEE = 10;

                string message = string.Empty;
                subscriberId = subscriberId == 0 ? 1 : subscriberId;
                string subscriberLogo = "";
                var fileUrl = "";
                string smsRoute = "default";
                string emailSignature = "";
                int typeOfNotification = 0;
                string smsSenderId = "iWitns";
                appInfoId = appInfoId ?? 0;
                bool isSendNewPostSMS = false;

                AppUserInfo appUserInfo = new AppUserInfo();
                Subscriber subscriberdbModel = db.Subscribers.Where(i => i.Id == subscriberId).FirstOrDefault();
                if (appInfoId > 0)
                {
                    appUserInfo = db.AppUserInfoes.Where(i => i.AppInfoID == appInfoId).FirstOrDefault();
                }
                else
                {
                    appUserInfo = db.AppUserInfoes.Where(i => i.DeviceId == deviceId).OrderByDescending(o => o.AppInfoID).FirstOrDefault();
                }
                if (appUserInfo == null)
                {
                    return false;
                }
                List<AppSetting> appSettingList = subscriberdbModel.AppSettings.ToList();
                AppSetting appSettingModel = new AppSetting();

                subscriberId = subscriberId == 0 ? 1 : subscriberId;
                fileUrl = subscriberdbModel.AppSettings.Where(i => i.Key == Constants.FileUrl).FirstOrDefault().Value;
                subscriberLogo = !string.IsNullOrEmpty(subscriberdbModel.CompanyLogo) ? fileUrl + subscriberdbModel.Id + Constants.ImageFolder + Constants.SubscriberFolder + subscriberdbModel.CompanyLogo : "";
                isSendNewPostSMS  = subscriberdbModel.AppSettings.Where(i => i.Key == Constants.IS_SEND_NEW_POST_SMS).FirstOrDefault().Value == "true" ? true : false;

                emailSignature = subscriberdbModel.AppSettings.Where(i => i.Key == Constants.EmailSignature).FirstOrDefault().Value;

                switch (isApproved)
                {
                    case 0:
                        {
                            appSettingModel = appSettingList.Where(i => i.Key == Constants.POST_REJECTED).FirstOrDefault();
                            message = appSettingModel.Value;
                            message = message.Replace("@FormSubmissionId", formSubmissionId.ToString());
                            break;
                        }
                    case 1:
                        {
                            appSettingModel = appSettingList.Where(i => i.Key == Constants.POST_APPROVED).FirstOrDefault();
                            message = appSettingModel.Value;
                            message = message.Replace("@FormSubmissionId", formSubmissionId.ToString());
                            break;
                        }
                    case 3:
                        {
                            appSettingModel = appSettingList.Where(i => i.Key == Constants.POST_RESOLVED).FirstOrDefault();
                            message = appSettingModel.Value;
                            message = message.Replace("@FormSubmissionId", formSubmissionId.ToString());
                            break;
                        }
                    case 5:
                        {
                            appSettingModel = appSettingList.Where(i => i.Key == Constants.POST_RECEIVED).FirstOrDefault();
                            message = appSettingModel.Value;
                            message = message.Replace("@FormSubmissionId", formSubmissionId.ToString());
                            break;
                        }
                    case 10:
                        {
                            emailSignature = appSettingList.Where(i => i.Key == Constants.EmailSignature).FirstOrDefault() != null ? appSettingList.Where(i => i.Key == Constants.EmailSignature).FirstOrDefault().Value : string.Empty;
                            smsRoute = appSettingList.Where(i => i.Key == Constants.SMS_ROUTE).FirstOrDefault() != null ? appSettingList.Where(i => i.Key == Constants.SMS_ROUTE).FirstOrDefault().Value : string.Empty;
                            smsSenderId = appSettingList.Where(i => i.Key == Constants.SMS_SENDER_ID).FirstOrDefault() != null ? appSettingList.Where(i => i.Key == Constants.SMS_SENDER_ID).FirstOrDefault().Value : string.Empty;

                            if (!string.IsNullOrEmpty(emailSignature) && !string.IsNullOrEmpty(smsRoute) && !string.IsNullOrEmpty(smsSenderId))
                            {

                                message = "Post #" + formSubmissionId + " is assigned to designated person for further action! ";
                                bool isFromIndia = appUserInfo.PhoneNo.Substring(0, 2) == "91";
                                if (isFromIndia)
                                {
                                    AppUserInfoService appUserInfoService = new AppUserInfoService();
                                    if(isSendNewPostSMS)
                                    appUserInfoService.SendSMS(appUserInfo.PhoneNo, message + emailSignature, subscriberId, smsRoute, smsSenderId);
                                }
                            }
                            else
                            {

                            }
                            break;
                        }
                    default:
                        break;
                }

                //0 == Reject
                //1 == Approved
                //3 == Resolve
                //5 == New Post Added
                //10 == Assign to employee
                //Converted to
                //0 == New Post Added //1 == Accept //2 == Reject //3 == Resolve

                //SUBMISSION_NEW = 0;
                //SUBMISSION_APPROVED = 1;
                //SUBMISSION_REJECT = 2;
                //SUBMISSION_RESOLVE = 3;
                //SUBMISSION_ASSIGN_TO_EMPLOYEE = 10;

                switch (isApproved)
                {
                    case 0:
                        {
                            //typeOfNotification = Constants.SUBMISSION_NEW;
                            typeOfNotification = Constants.SUBMISSION_REJECT;
                            break;
                        }
                    case 1:
                        {
                            typeOfNotification = Constants.SUBMISSION_APPROVED;
                            break;
                        }
                    case 2:
                        {
                            typeOfNotification = Constants.SUBMISSION_REJECT;
                            break;
                        }
                    case 3:
                        {
                            typeOfNotification = Constants.SUBMISSION_RESOLVE;
                            break;
                        }
                    case 5:
                        {
                            typeOfNotification = Constants.SUBMISSION_NEW;
                            break;
                        }
                    case 10:
                        {
                            typeOfNotification = Constants.SUBMISSION_ASSIGN_TO_EMPLOYEE;
                            break;
                        }
                    default:
                        break;
                }

                if (appUserInfo != null)
                {
                    Notification model = new Notification();
                    model.AppInfoId = appUserInfo.AppInfoID;
                    model.CreatedOn = Common.GetDateTime(db);
                    model.IsUnread = true;
                    model.Message = message;
                    model.TypeOfNotification = typeOfNotification;
                    model.SubscriberId = subscriberId;

                    db.Notifications.Add(model);
                    db.SaveChanges();

                    string strTemplate = "";
                    int languageId = 1;

                    SubscriberLanguage subscriberModel = subscriberdbModel.SubscriberLanguages.OrderBy(o => o.DisplayOrder).FirstOrDefault();

                    if (subscriberModel != null)
                    {
                        languageId = subscriberModel.LanguageId;
                    }

                    StreamReader fp = new StreamReader(System.Web.HttpContext.Current.Server.MapPath("~/MailTemplates/Languages/" + languageId + "/NotificationMail.html"));
                    strTemplate = fp.ReadToEnd();
                    fp.Close();

                    strTemplate = strTemplate.Replace("@Name", appUserInfo.Name).Replace("@Message", message).Replace("@CompanyName", subscriberModel.Subscriber.CompanyName).Replace("@EmailSignature", emailSignature).Replace("@CompanyLogo", subscriberLogo);
                    if (isSendNewPostSMS)
                        return SendNoticationEmail(appUserInfo.Email, strTemplate, "Notification");
                    else
                        return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                throw e;
            }
        }

        public List<Notification> GetNotifications(int appInfoId, string date, int subscriberId= 1)
        {
            try
            {
                DateTime d = Convert.ToDateTime(date).ToUniversalTime();
                List<Notification> NotificationList = db.Notifications.Where(i => i.AppInfoId == appInfoId && i.CreatedOn >= d && i.SubscriberId == subscriberId).AsEnumerable()
                    .OrderByDescending(o => o.CreatedOn)
                    .Select(i => new Notification
                    {
                        AppInfoId = i.AppInfoId,
                        CreatedOn = i.CreatedOn,
                        Id = i.Id,
                        IsUnread = i.IsUnread,
                        Message = i.Message,
                        TypeOfNotification = i.TypeOfNotification,
                        SubscriberId = i.SubscriberId
                    }).ToList();
                return NotificationList;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                throw e;
            }
        }

        //i-Witness specific
        //Settings according to i-Witness (Ex. To email address, from email address)
        public bool SendNoticationEmail(string toEmailAddress, string mailBody, string subject)
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

                lobjMail.Body = mailBody;
                lobjMail.Subject = subject;
                lobjMail.From = new MailAddress(FromAddress, "");
                lobjMail.To.Add(new MailAddress(toEmailAddress));

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
                return true;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                throw e;
            }
        }

        public bool ReadNotification(int appInfoId, int notificationId)
        {
            try
            {
                Notification notificationModel = db.Notifications.Where(i => i.AppInfoId == appInfoId && i.Id == notificationId).FirstOrDefault();

                if (notificationModel != null)
                {
                    notificationModel.IsUnread = false;
                    db.Entry(notificationModel).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return false;
            }
        } 

        #endregion
    }
}