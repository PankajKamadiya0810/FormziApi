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
using FormziApi.Helper;
using System.Web.Http.Cors;

namespace FormziApi.Controllers
{
    [Authorize]
    [EnableCors("*", "*", "*")]
    public class SettingsController : ApiController
    {
        #region Fields
        
        LogProvider lp = new LogProvider("Formzi");
        private FormziEntities db = new FormziEntities(); 
        
        #endregion

        #region Methods
        
        [Route("api/settings")]
        [HttpPost]
        public int PostOperation([FromBody]List<AppSetting> setting)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return 0;
                }
                for (int i = 0; i < setting.Count; i++)
                {
                    AppSetting newSetting = setting[i];
                    db.AppSettings.Add(newSetting);
                    db.SaveChanges();
                }

                return 0;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return 0;
            }
        }

        [Route("api/setting/language/{OperationId}")]
        [HttpGet]
        public object GetLanguage(int OperationId)
        {
            try
            {
                var data = db.OperationSettings.Where(m => m.OperationId == OperationId && m.Key == Constants.Language).Select(l => new
                {
                    Id = l.Value,
                    LanguageName = db.Languages.Where(j => j.Id.ToString() == l.Value).FirstOrDefault().Name,
                    Rtl = db.Languages.Where(j => j.Id.ToString() == l.Value).FirstOrDefault().Rtl,
                    UniqueSeoCode = db.Languages.Where(j => j.Id.ToString() == l.Value).FirstOrDefault().UniqueSeoCode,

                }).ToList();

                return data;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }

        [Route("api/getRecentLoginHistory")]
        [HttpGet]
        public object RecentLoginHistory()
        {
            var model = db.LoginLogs.OrderByDescending(o => o.LoggedOn).Take(5).AsEnumerable()
                .Select(i => new
                {
                    Date = i.LoggedOn.ToShortDateString() + " " + i.LoggedOn.ToShortTimeString(),
                    Email = i.AppLogin.Email,
                    Name = i.AppLogin.Employees.Where(j => j.AppLoginId == i.AppLoginId).FirstOrDefault().FirstName + " " + i.AppLogin.Employees.Where(j => j.AppLoginId == i.AppLoginId).FirstOrDefault().LastName
                }).ToList();
            return model;
        } 
        
        #endregion
    }
}