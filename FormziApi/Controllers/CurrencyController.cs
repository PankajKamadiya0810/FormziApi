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
using System.Web.Http.OData;
using FormziApi.Helper;
using System.Web.Http.Cors;
using FormziApi.Models;
using Newtonsoft.Json;
using FormziApi.Services;

namespace FormziApi.Controllers
{
    [Authorize]
    [EnableCors("*", "*", "*")]
    public class CurrencyController : ApiController
    {
        #region Fields

        private LogProvider lp;
        private FormziEntities db; 
        
        #endregion

        #region Constructors
        
        public CurrencyController()
        {
            lp = new LogProvider("Formzi");
            db = new FormziEntities();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get currency list
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [Route("api/currencies")]
        [HttpGet]
        public object GetCurrency()
        {
            try
            {
                return db.Currencies.Where(m => m.Published == true).Select(m => new
                {
                    m.Id,
                    m.Name,
                }).ToList();
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }

        #endregion
    }
}