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
using FormziApi.Extention;
using FormziApi.Models;

namespace FormziApi.Controllers
{
    [Authorize]
    [EnableCors("*", "*", "*")]
    public class AddressesController : ApiController
    {
        #region Fields

        private LogProvider lp = new LogProvider("Formzi");
        private FormziEntities db = new FormziEntities();

        #endregion

        #region Methods

        [Route("api/countries")]
        [HttpGet]
        public object GetCountry()
        {
            try
            {
                var country = db.Countries.Where(c => c.Published).OrderBy(y => y.DisplayOrder).Select(m => new
                {
                    Value = m.Id,
                    Text = m.Name,
                    Selected = false,
                });
                return country;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }

        }

        [Route("api/addresses/{id}")]
        [HttpDelete]
        public bool DeleteAddress(int id)
        {
            try
            {
                Address address = db.Addresses.Find(id);
                if (address == null)
                {
                    return false;
                }
                address.UpdatedOn = Common.GetDateTime(db);
                address.IsDeleted = true;
                db.Entry(address).State = EntityState.Modified;
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

        [ResponseType(typeof(StateProvince))]
        [Route("api/statebycountryid/{CountryId}")]
        [HttpGet]
        public object GetStatebyCountryId(int CountryId)
        {
            try
            {
                var state = db.StateProvinces.Where(c => c.CountryId == CountryId).OrderBy(y => y.DisplayOrder)
                    .OrderBy(o => o.Name)
                    .Select(m => new
                {
                    Value = m.Id,
                    Text = m.Name,
                    Selected = false,
                }).ToList();
                return state;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }

        [HttpPost]
        public Int64 PostAddress([FromBody]Address address)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return 0;
                }

                address.CreatedOn = Common.GetDateTime(db);
                db.Addresses.Add(address);
                db.SaveChanges();
                return address.Id;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return 0;
            }
        }

        [Route("api/addresses/{id}")]
        [HttpGet]
        public object GetAddressById(int id)
        {
            try
            {
                var address = db.Addresses.Where(e => e.Id == id && e.IsDeleted == false).FirstOrDefault();
                var model = address.ToModel<Address, AddressModel>();
                var states = db.StateProvinces.Where(s => s.CountryId == model.CountryId).Select(r => new System.Web.Mvc.SelectListItem()
                {
                    Text = r.Name,
                    Value = r.Id.ToString(),
                    Selected = r.Id == model.StateProvinceId
                });
                return new { Address = model, States = states };
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