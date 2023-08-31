using FormziApi.Database;
using FormziApi.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using FormziApi.Models;
using FormziApi.Services;
using System.Web.Http.Cors;

namespace FormziApi.Controllers
{
    //Innfy specific
    [Authorize]
    [EnableCors("*", "*", "*")]
    public class AppUserInfoController : ApiController
    {
        #region Fields

        private LogProvider lp;
        private FormziEntities db;
        private AppUserInfoService _appUserInfoService; 
        
        #endregion

        #region Constructors
        
        //Constructor added by Jay Mistry 14-6-2016
        public AppUserInfoController()
        {
            lp = new LogProvider("Formzi");
            db = new FormziEntities();
            _appUserInfoService = new AppUserInfoService();
        } 

        #endregion

        #region I-Witness Specific
        
        //i-witness specific , Mobile 
        [AllowAnonymous]
        [Route("api/AddAppUserInfo")]
        [HttpPost]
        public bool AddAppUserInfo([FromBody]AppUserInfoModel appUserInfo)
        {
            try
            {
                //Check user Authentication
                if (!_appUserInfoService.CheckAppAuthentication(appUserInfo.AuthKey, appUserInfo.SubscriberId))
                {
                    return false;
                }

                if (_appUserInfoService.AddAppUserInfo(appUserInfo))
                {
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

        //i-witness specific, Mobile
        [AllowAnonymous]
        [Route("api/OTP")]
        [HttpGet]
        public bool VerifyOTP(string email, string phoneNo, string oTP, string deviceId, int subscriberId, string authKey)
        {
            try
            {
                //Check user Authentication
                if (!_appUserInfoService.CheckAppAuthentication(authKey, subscriberId))
                {
                    return false;
                }

                if (!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(oTP) && _appUserInfoService.VerifyOTP(email, phoneNo, deviceId, oTP))
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

        //i-witness specific - Mobile
        [AllowAnonymous]
        [Route("api/Notification")]
        [HttpGet]
        public object Notification(int appInfoId, string date, int subscriberId, string authKey)
        {
            try
            {
                //Check user Authentication
                if (!_appUserInfoService.CheckAppAuthentication(authKey, subscriberId))
                {
                    return false;
                }

                NotificationService notificationService = new NotificationService();
                return notificationService.GetNotifications(appInfoId, date, subscriberId);
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return false;
            }
        }

        //i-witness specific, Mobile
        [AllowAnonymous]
        [Route("api/AppUserInfo")]
        [HttpGet]
        public AppUserInfo AddAppUserInfo(string email, int subscriberId, string authKey)
        {
            try
            {
                //Check user Authentication
                if (!_appUserInfoService.CheckAppAuthentication(authKey, subscriberId))
                {
                    return null;
                }

                AppUserInfo appUserInfo = db.AppUserInfoes.Where(i => i.Email == email).OrderByDescending(i => i.AppInfoID).FirstOrDefault();

                if (appUserInfo != null)
                {
                    AppUserInfo model = new AppUserInfo();
                    model.AppInfoID = appUserInfo.AppInfoID;
                    model.Name = appUserInfo.Name;
                    model.DeviceId = appUserInfo.DeviceId;
                    model.Email = appUserInfo.Email;
                    model.PhoneNo = appUserInfo.PhoneNo;

                    return model;
                }
                return null;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }

        //i-witness specific, Mobile
        [AllowAnonymous]
        [Route("api/ReadNotification")]
        [HttpGet]
        public bool ReadNotification(int appInfoId, int notificationId, int subscriberId, string authKey)
        {
            try
            {
                //Check user Authentication
                if (!_appUserInfoService.CheckAppAuthentication(authKey, subscriberId))
                {
                    return false;
                }

                NotificationService notificationService = new NotificationService();
                return notificationService.ReadNotification(appInfoId, notificationId);
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return false;
            }
        } 

        #endregion

        #region Methods
        
        [Route("api/verifyEmail/{id}")]
        [HttpGet]
        public ApiReturnData VerifyEmail(string id)
        {
            try
            {
                return _appUserInfoService.VerifyEmail(id);
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

        #endregion
    }
}