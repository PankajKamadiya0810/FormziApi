using FormziApi.Database;
using FormziApi.Helper;
using FormziApi.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Web;

namespace FormziApi.Services
{
    //This class file is added by Jay Mistry on 14-6-2016 Tuesday
    public class AppUserInfoService
    {
        #region Fields

        LogProvider lp;
        private FormziEntities db;

        #endregion

        #region Constructors

        public AppUserInfoService()
        {
            lp = new LogProvider("Formzi");
            db = new FormziEntities();
        }

        #endregion

        #region Methods

        /// <summary>
        /// It will add new mobile user
        /// Innfy - 'i-Witness' - Ticit Specific
        /// 
        /// </summary>
        /// <param name="appUserModel"></param>
        /// <returns></returns>
        public bool AddAppUserInfo(AppUserInfoModel appUserModel)
        {
            try
            {
                AppUserInfo dbAppUserModel = new AppUserInfo();

                dbAppUserModel = db.AppUserInfoes.Where(i => i.Email == appUserModel.Email && i.DeviceId == appUserModel.DeviceId && i.PhoneNo == appUserModel.PhoneNo).OrderByDescending(i => i.AppInfoID).FirstOrDefault();

                if (dbAppUserModel != null)
                {
                    if (appUserModel.PhoneNo != dbAppUserModel.PhoneNo)
                    {
                        dbAppUserModel = new AppUserInfo();
                        dbAppUserModel.Name = appUserModel.Name;
                        dbAppUserModel.DeviceId = appUserModel.DeviceId;
                        dbAppUserModel.Email = appUserModel.Email;
                        dbAppUserModel.PhoneNo = appUserModel.PhoneNo;

                        dbAppUserModel.IsOTPVerified = false;
                        dbAppUserModel.CreatedOn = Common.GetDateTime(db);

                        db.AppUserInfoes.Add(dbAppUserModel);
                        db.SaveChanges();

                        dbAppUserModel.EmailVerificationCode = Helper.General.GenerateAccessCode(Helper.Constants.NO_OF_CHARACTER);
                        dbAppUserModel.IsEmailVerified = false;

                        OTPMail(dbAppUserModel, appUserModel.SubscriberId);

                        return true;
                    }
                    else
                    {
                        dbAppUserModel.Name = appUserModel.Name;
                        dbAppUserModel.DeviceId = appUserModel.DeviceId;
                        dbAppUserModel.PhoneNo = appUserModel.PhoneNo;
                        if (!dbAppUserModel.IsEmailVerified)
                        {
                            dbAppUserModel.EmailVerificationCode = Helper.General.GenerateAccessCode(Helper.Constants.NO_OF_CHARACTER);
                            dbAppUserModel.IsEmailVerified = false;
                        }
                        dbAppUserModel.IsOTPVerified = false;
                        dbAppUserModel.UpdatedOn = Common.GetDateTime(db);

                        db.Entry(dbAppUserModel).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        return OTPMail(dbAppUserModel, appUserModel.SubscriberId);
                    }
                }
                else
                {
                    dbAppUserModel = new AppUserInfo();
                    dbAppUserModel.Name = appUserModel.Name;
                    dbAppUserModel.DeviceId = appUserModel.DeviceId;
                    dbAppUserModel.Email = appUserModel.Email;
                    dbAppUserModel.PhoneNo = appUserModel.PhoneNo;
                    dbAppUserModel.EmailVerificationCode = Helper.General.GenerateAccessCode(Helper.Constants.NO_OF_CHARACTER);
                    dbAppUserModel.IsEmailVerified = false;
                    dbAppUserModel.IsOTPVerified = false;
                    dbAppUserModel.CreatedOn = Common.GetDateTime(db);

                    db.AppUserInfoes.Add(dbAppUserModel);
                    db.SaveChanges();

                    dbAppUserModel.EmailVerificationCode = Helper.General.GenerateAccessCode(Helper.Constants.NO_OF_CHARACTER);
                    dbAppUserModel.IsEmailVerified = false;

                    return OTPMail(dbAppUserModel, appUserModel.SubscriberId);
                }
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return false;
            }
        }

        public bool OTPMail(AppUserInfo appUserInfo, int subscriberId)
        {
            try
            {
                string strTemplate = "";
                int languageId = 1;
                string emailSignature = "";
                string subscriberLogo = "";
                var fileUrl = "";
                string EmailVerificationLink = "";
                bool isFromIndia = appUserInfo.PhoneNo.Substring(0, 2) == "91";
                string smsRoute = "default";
                string smsSenderId = "iWitns";
                string innfyHost = ConfigurationManager.AppSettings["InnfyDomain"].ToString();

                Subscriber subscriberdbModel = db.Subscribers.Where(i => i.Id == subscriberId).FirstOrDefault();

                fileUrl = subscriberdbModel.AppSettings.Where(i => i.Key == Constants.FileUrl).FirstOrDefault().Value;
                subscriberLogo = !string.IsNullOrEmpty(subscriberdbModel.CompanyLogo) ? fileUrl + subscriberdbModel.Id + Constants.ImageFolder + Constants.SubscriberFolder + subscriberdbModel.CompanyLogo : "";
                EmailVerificationLink = innfyHost + "verify/" + appUserInfo.EmailVerificationCode;
                subscriberId = subscriberId == 0 ? 1 : subscriberId;

                SubscriberLanguage subscriberModel = subscriberdbModel.SubscriberLanguages
                    .OrderBy(o => o.DisplayOrder).FirstOrDefault();

                List<AppSetting> appSettingList = subscriberdbModel.AppSettings.ToList();

                AppSetting appSettingModel = appSettingList.Where(i => i.Key == Constants.EmailSignature).FirstOrDefault();

                if (appSettingModel == null)
                {
                    return false;
                }
                else
                {
                    emailSignature = appSettingModel.Value;
                }

                if (isFromIndia)
                {
                    appSettingModel = appSettingList.Where(i => i.Key == Constants.OTPTemplate).FirstOrDefault();
                    if (appSettingModel == null)
                    {
                        return false;
                    }
                }

                if (subscriberModel != null)
                {
                    languageId = subscriberModel.LanguageId;
                }

                StreamReader fp = new StreamReader(System.Web.HttpContext.Current.Server.MapPath("~/MailTemplates/Languages/" + languageId + "/OTPMail.html"));
                strTemplate = fp.ReadToEnd();
                fp.Close();

                string OTP = Helper.General.GenerateRandomNumeric(4);

                strTemplate = strTemplate.Replace("@Name", appUserInfo.Name).Replace("@OTP", OTP).Replace("@CompanyName", subscriberModel.Subscriber.CompanyName).Replace("@EmailSignature", emailSignature).Replace("@VerificationLink", EmailVerificationLink).Replace("@CompanyLogo", subscriberLogo);
                SendOTPEmail(appUserInfo.Email, strTemplate, "Verification OTP is: " + OTP);

                if (isFromIndia)
                {
                    smsRoute = appSettingList.Where(i => i.Key == Constants.SMS_ROUTE).FirstOrDefault() != null ? appSettingList.Where(i => i.Key == Constants.SMS_ROUTE).FirstOrDefault().Value : string.Empty;
                    smsSenderId = appSettingList.Where(i => i.Key == Constants.SMS_SENDER_ID).FirstOrDefault() != null ? appSettingList.Where(i => i.Key == Constants.SMS_SENDER_ID).FirstOrDefault().Value : string.Empty;                    
                    string smsMessage = appSettingModel.Value.Replace("@OTP_STRING@", OTP);
                    SendSMS(appUserInfo.PhoneNo, smsMessage, subscriberId, smsRoute,smsSenderId);
                }

                if (appUserInfo != null)
                {
                    appUserInfo.OTP = OTP;
                    appUserInfo.IsOTPVerified = false;
                    appUserInfo.UpdatedOn = Common.GetDateTime(db);
                    db.Entry(appUserInfo).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                }

                return true;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return false;
            }
        }

        public bool SendOTPEmail(string toEmailAddress, string mailBody, string subject)
        {
            try
            {
                string fromAddress = Convert.ToString(ConfigurationManager.AppSettings["InnfyRecipientFromEmail"]);

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
                lobjMail.From = new MailAddress(fromAddress, "");
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
                return false;
            }
        }

        public bool VerifyOTP(string email, string phoneNo, string deviceId, string oTP)
        {
            try
            {
                oTP = oTP.ToUpper();
                AppUserInfo model = db.AppUserInfoes.Where(i => i.Email == email && i.PhoneNo == phoneNo && i.DeviceId == deviceId && i.OTP == oTP && !i.IsOTPVerified)
                    .OrderByDescending(i => i.AppInfoID).FirstOrDefault();

                if (model != null)
                {
                    model.OTP = null;
                    model.IsOTPVerified = true;
                    model.UpdatedOn = Common.GetDateTime(db);
                    db.Entry(model).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                throw e;
            }
        }

        public bool SendSMS(string mobileNumber, string message, int subscriberId, string smsRoute, string smsSenderId)
        {
            try
            {   
                if (mobileNumber.Length > 10 && !mobileNumber.Contains('+'))
                {
                    mobileNumber = "+" + mobileNumber;
                }

                AppSetting dbModel = db.AppSettings.Where(i => i.Key == Constants.SMSAuthKey && i.SubscriberId == subscriberId).FirstOrDefault();
                if (dbModel == null)
                {
                    return false;
                }

                string authKey = dbModel.Value;
                string senderId = smsSenderId;
                StringBuilder sbPostData = new StringBuilder();
                sbPostData.AppendFormat("authkey={0}", authKey);
                sbPostData.AppendFormat("&mobiles={0}", mobileNumber);
                sbPostData.AppendFormat("&message={0}", message);
                sbPostData.AppendFormat("&sender={0}", senderId);
                sbPostData.AppendFormat("&route={0}", smsRoute);

                try
                {
                    string sendSMSUri = "https://control.msg91.com/api/sendhttp.php";
                    HttpWebRequest httpWReq = (HttpWebRequest)WebRequest.Create(sendSMSUri);
                    UTF8Encoding encoding = new UTF8Encoding();
                    byte[] data = encoding.GetBytes(sbPostData.ToString());
                    httpWReq.Method = "POST";
                    httpWReq.ContentType = "application/x-www-form-urlencoded";
                    httpWReq.ContentLength = data.Length;
                    using (Stream stream = httpWReq.GetRequestStream())
                    {
                        stream.Write(data, 0, data.Length);
                    }
                    HttpWebResponse response = (HttpWebResponse)httpWReq.GetResponse();
                    StreamReader reader = new StreamReader(response.GetResponseStream());
                    string responseString = reader.ReadToEnd();

                    reader.Close();
                    response.Close();

                    return true;
                }
                catch (Exception e)
                {
                    lp.Info(e.Message);
                    lp.HandleError(e, e.Message);
                    return false;
                }

            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return false;
            }
        }

        public ApiReturnData VerifyEmail(string id)
        {
            try
            {
                ApiReturnData model = new ApiReturnData();
                if (string.IsNullOrEmpty(id))
                {
                    model.Success = false;
                    model.Message = "Invalid url.";
                    model.SuccessData = null;
                }
                AppUserInfo dbModel = db.AppUserInfoes.Where(i => i.EmailVerificationCode == id).OrderByDescending(i => i.AppInfoID).FirstOrDefault();
                if (dbModel != null)
                {
                    dbModel.EmailVerificationCode = string.Empty;
                    dbModel.IsEmailVerified = false;
                    db.Entry(dbModel).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();

                    model.Success = true;
                    model.Message = "Your account has been successfully verified.";
                    model.SuccessData = null;
                    return model;
                }
                model.Success = false;
                model.Message = "Invalid url.";
                model.SuccessData = null;
                return model;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                ApiReturnData model = new ApiReturnData();
                model.Success = false;
                model.Message = "An error occurred. Please try agian later";
                model.SuccessData = null;
                return model;
            }
        }

        public bool CheckAppAuthentication(string authKey, int subscriberId)
        {
            try
            {
                List<AppSetting> appSettingList = db.AppSettings.Where(i => i.SubscriberId == subscriberId).ToList();

                AppSetting appSettingModel = appSettingList.Where(i => i.Key == Constants.APP_AUTH_KEY).FirstOrDefault();

                if (appSettingModel != null && authKey == appSettingModel.Value)
                {
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
                return false;
            }
        }

        #endregion
    }
}