using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using FormziApi.Database;
using System.Web.Http.Cors;
using FormziApi.Helper;


namespace FormziApi.Controllers
{
    [Authorize]
    [EnableCors("*", "*", "*")]
    public class AppSettingsController : ApiController
    {
        #region Fields

        private LogProvider lp = new LogProvider("Formzi");
        private FormziEntities db = new FormziEntities();

        #endregion

        #region Methods

        /// <summary>
        /// App Setting details by key
        /// </summary>
        /// <param name="key">Key Name Ex. OTPTemplate</param>
        /// <returns>App Setting model contains Id, Key and value</returns>
        [Route("api/getappsettings")]
        [HttpGet]
        public object GetSettingsByKey(string key)
        {
            try
            {
                return db.AppSettings.Where(c => c.Key == key).Select(m => new
                {
                    m.Id,
                    m.Key,
                    m.Value
                }).ToList();
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }

        /// <summary>
        /// App Setting details by value
        /// </summary>
        /// <param name="key">Key Name Ex. OTPTemplate</param>
        /// <returns>Value field only</returns>
        [Route("api/getsettingvalue")]
        [HttpGet]
        public string GetValueByKey(string key)
        {
            try
            {
                return db.AppSettings.Where(c => c.Key.ToLower() == key.ToLower()).FirstOrDefault().Value;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }

        /// <summary>
        /// App Setting list by subscriber id.
        /// </summary>
        /// <param name="subscriberId">Subscriber id Ex. 1</param>
        /// <returns>List of settings containing Id, Key, Value, Active and Filed.</returns>
        //By Jay Mistry 19-7-2017
        [Route("api/appSettings")]
        [HttpGet]
        public object SettingList(int subscriberId = 0)
        {
            try
            {
                return db.AppSettings.Where(c => c.SubscriberId == subscriberId).AsEnumerable().Select(m => new AppSetting
                {
                    Id = m.Id,
                    Key = m.Key,
                    Value = m.Value,
                    Active = m.Active
                }).ToList();
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }

        /// <summary>
        /// Add App Setting details.
        /// </summary>
        /// <param name="model">Key, Value, SubscriberId, Active and IsReadonly field.</param>
        /// <returns>Returns True/False</returns>
        [Route("api/appSettings")]
        [HttpPost]
        public object AddSetting(AppSetting model)
        {
            try
            {
                model.CreatedOn = Common.GetDateTime(db);
                model.UpdatedOn = Common.GetDateTime(db);

                db.AppSettings.Add(model);
                db.SaveChanges();
                return true;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }

        /// <summary>
        /// Update Setting details.
        /// </summary>
        /// <param name="model">Key, Value, SubscriberId, Active and IsReadonly field.</param>
        /// <returns>Returns True/False</returns>
        [Route("api/appSettings")]
        [HttpPut]
        public object UpdateSetting(AppSetting model)
        {
            try
            {
                AppSetting dbModel = db.AppSettings.Where(i => i.Id == model.Id).FirstOrDefault();
                dbModel.Active = model.Active;
                dbModel.IsReadonly = model.IsReadonly;
                dbModel.Key = model.Key;
                dbModel.Value = model.Value;
                db.Entry(dbModel).State = EntityState.Modified;
                db.SaveChanges();
                return true;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return false;
            }
        }

        /// <summary>
        /// Deactive record
        /// </summary>
        /// <param name="id">Id of record.</param>
        /// <returns>Return True/False</returns>
        [Route("api/appSettings/{id}")]
        [HttpDelete]
        public object RemoveSetting(int id = 0)
        {
            try
            {
                AppSetting dbModel = db.AppSettings.Where(c => c.Id == id).FirstOrDefault();
                dbModel.Active = false;
                db.Entry(dbModel).State = EntityState.Modified;
                db.SaveChanges();

                return true;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }

        /// <summary>
        /// Single App Setting record by id and subscriberId wise.
        /// </summary>
        /// <param name="id">record id</param>
        /// <param name="subscriberId">subscriber id</param>
        /// <returns>Model or null</returns>
        [Route("api/appSettingsById")]
        [HttpGet]
        public object SettingById(int id = 0,int subscriberId=0)
        {
            try
            {
                return db.AppSettings.Where(c => c.Id == id && c.SubscriberId == subscriberId).AsEnumerable().Select(m => new AppSetting
                {
                    Id = m.Id,
                    Key = m.Key,
                    Value = m.Value,
                    Active = m.Active,
                    IsReadonly = m.IsReadonly
                }).FirstOrDefault();
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }

        /// <summary>
        /// Single App Setting record by Key and subscriberId wise.
        /// </summary>
        /// <param name="key">Key Name Ex. OTPTemplate</param>
        /// <param name="subscriberId">Subscriber id Ex. 1</param>
        /// <returns>Model or null</returns>
        [Route("api/subscriberAppsettingByKey")]
        [HttpGet]
        public object SubscriberSettingsByKey(string key,int subscriberId = 0)
        {
            try
            {
                return db.AppSettings.Where(c => c.Key == key && c.SubscriberId == subscriberId).Select(m => new
                {
                    m.Id,
                    m.Key,
                    m.Value,
                    m.Active,
                    m.IsReadonly
                }).FirstOrDefault();
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }
        
        #endregion

        #region I-Witness Specific

        /// <summary>
        /// i-Witness - Generate Auth Key by subscriber id.
        /// </summary>
        /// <param name="subscriberId">subscriber id. Ex. 1</param>
        /// <returns></returns>
        //i-Witness specific
        [Route("api/authKey")]
        [HttpGet]
        public string GenerateAuthKey(int subscriberId)
        {
            try
            {
                AppSetting model = db.AppSettings.Where(i => i.SubscriberId == subscriberId && i.Key == Helper.Constants.APP_AUTH_KEY).FirstOrDefault();

                if (model != null)
                {
                    model.Value = Helper.General.GenerateAccessCode(20);

                    db.Entry(model).State = EntityState.Modified;
                    db.SaveChanges();
                    return model.Value;
                }
                return string.Empty;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return string.Empty;
            }
        } 

        #endregion
    }
}