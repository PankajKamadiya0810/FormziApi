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
using Newtonsoft.Json.Linq;
using FormziApi.Models;
using System.Web.WebPages.Html;
using System.Configuration;

namespace FormziApi.Controllers
{
    [Authorize]
    [EnableCors("*", "*", "*")]
    public class LocalStringResourcesController : ApiController
    {
        #region Fields

        LogProvider lp = new LogProvider("Formzi");
        private FormziEntities db = new FormziEntities();

        #endregion

        #region Methods

        [Route("api/localestringresourcebykey")]
        [HttpGet]
        public string GetLocaleStringResourcebyKey(int LanguageId, string ResourceName)
        {
            try
            {
                return db.LocalStringResources.Where(l => l.LanguageId == LanguageId && l.Name == ResourceName &&
                    !l.IsDeleted).FirstOrDefault().Value;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }

        [AllowAnonymous]
        [Route("api/localestringresources")]
        [HttpGet]
        public object GetLocaleStringResources(string url)
        {
            try
            {
                url = url.Replace("_", ".").ToLower();
                var subscriber = db.Subscribers.Where(s => s.PreferredDomain.ToLower() == url.ToLower() || s.SubDomain.ToLower() + "." + Constants.FORMZI_DOMAIN == url).FirstOrDefault();
                if (subscriber != null)
                {
                    int subscriberId = subscriber.Id;
                    int _labelType = (int)Constants.ResourceType.Label;
                    int baseLanguageId = Convert.ToInt16(db.AppSettings.Where(a => a.SubscriberId == subscriberId && a.Key.ToLower() == Constants.SubscriberBaseLanguage.ToLower()).FirstOrDefault().Value);

                    var data = db.LocalStringResources.Where(l => l.IsDeleted == false && l.SubscriberId == subscriberId && l.ResourceType == _labelType).ToList()
                        .Select(l => { return new { l.Name, l.Value, l.LanguageId }; }).Where(t => t.LanguageId == baseLanguageId);
                    Dictionary<string, string> labels = new Dictionary<string, string>();

                    foreach (var item in data)
                    {
                        labels.Add(item.Name, item.Value);
                    }
                    return new { LocalStringResources = labels, BaseLanguage = baseLanguageId };
                }
                else
                    return null;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }

        #endregion

        //Created by jay mistry on 1-8-2016
        #region Local_String_Resource CRUD

        [Route("api/localStringResources")]
        [HttpGet]
        public object AllLocalStringResources()
        {
            try
            {
                return db.LocalStringResources.AsEnumerable().Select(i => new LocaleStringResourceModel
                {
                    Id = i.Id,
                    IsDeleted = i.IsDeleted,
                    IsHtml = i.IsHtml,
                    IsMobileEnabled = i.IsMobileEnabled,
                    IsWebEnabled = i.IsWebEnabled,
                    LanguageId = i.LanguageId,
                    LanguageName = i.Language.Name,
                    Name = i.Name,
                    OperationId = i.OperationId,
                    SubscriberId = i.SubscriberId,
                    SubscriberName = i.Subscriber.FirstName + " " + i.Subscriber.LastName,
                    Value = i.Value
                }).ToList();
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }

        [Route("api/localStringResources")]
        [HttpGet]
        public object LocalStringResourcesList(int subscriberId, int languageId = 0, int resourceTypeId = 0)
        {
            try
            {
                if (subscriberId > 0)
                {
                    List<LocaleStringResourceModel> model = db.LocalStringResources
                        .Where(i => i.SubscriberId == subscriberId)
                        .AsEnumerable()
                        .Select(i => new LocaleStringResourceModel
                        {
                            Id = i.Id,
                            IsDeleted = i.IsDeleted,
                            IsHtml = i.IsHtml,
                            IsMobileEnabled = i.IsMobileEnabled,
                            IsWebEnabled = i.IsWebEnabled,
                            LanguageId = i.LanguageId,
                            LanguageName = i.Language.Name,
                            Name = i.Name,
                            OperationId = i.OperationId,
                            SubscriberId = i.SubscriberId,
                            SubscriberName = i.Subscriber.FirstName + " " + i.Subscriber.LastName,
                            Value = i.Value,
                            ResourceType = i.ResourceType
                        }).ToList();
                    //Added By Hiren 21-12-2017
                    if (languageId != 0 && resourceTypeId != 0)
                    {
                        model = model.Where(l => l.LanguageId == languageId && l.ResourceType == resourceTypeId).ToList();
                    }
                    else if (languageId != 0)
                    {
                        model = model.Where(l => l.LanguageId == languageId).ToList();
                    }
                    else if (resourceTypeId != 0)
                    {
                        model = model.Where(l => l.ResourceType == resourceTypeId).ToList();
                    }
                    else
                    {
                        model = model.ToList();
                    }
                    //End
                    return model;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }

        [Route("api/localStringResource")]
        [HttpPost]
        public object AddLocalStringResources(LocalStringResource model)
        {
            try
            {
                DateTime dateTime = Common.GetDateTime(db);
                if (model == null || model.Name.Length <= 0 || model.Value.Length <= 0 || model.LanguageId <= 0 || model.SubscriberId <= 0)
                {
                    return false;
                }
                if (db.LocalStringResources.Where(i => i.SubscriberId == model.SubscriberId && i.Name == model.Name && i.LanguageId == model.LanguageId).Any())
                {
                    return false;
                }
                List<int> subscriberIds = db.Subscribers.Select(i => i.Id).ToList();
                List<int> languageList = db.Languages.Select(i => i.Id).ToList();
                foreach (var item in subscriberIds)
                {
                    foreach (var langId in languageList)
                    {
                        if (db.LocalStringResources.Where(i => i.SubscriberId == item && i.Name == model.Name && i.LanguageId == langId).Any())
                            continue;

                        model.LanguageId = langId;
                        model.SubscriberId = item;
                        model.IsDeleted = false;
                        model.IsHtml = true;
                        model.CreatedOn = dateTime;
                        model.UpdatedOn = dateTime;
                        db.LocalStringResources.Add(model);
                        db.SaveChanges();
                    }
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

        [Route("api/localStringResource")]
        [HttpPut]
        public object UpdateLocalStringResources(LocalStringResource model)
        {
            try
            {
                DateTime dateTime = Common.GetDateTime(db);
                if (model == null)
                {
                    return false;
                }
                model.UpdatedOn = dateTime;
                db.Entry(model).State = EntityState.Modified;
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

        [Route("api/localStringResource/{id}")]
        [HttpDelete]
        public object DeleteLocalStringResources(int id)
        {
            try
            {
                DateTime dateTime = Common.GetDateTime(db);
                if (id <= 0)
                {
                    return false;
                }
                LocalStringResource model = db.LocalStringResources.Where(i => i.Id == id).FirstOrDefault();
                model.IsDeleted = true;
                model.UpdatedOn = dateTime;
                db.Entry(model).State = EntityState.Modified;
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

        [Route("api/localStringResourceByName")]
        [HttpGet]
        public object LocalStringResourcesList(int subscriberId, int languageId, string name)
        {
            try
            {
                if (subscriberId > 0 && languageId > 0)
                {
                    List<LocaleStringResourceModel> model = db.LocalStringResources
                        .Where(i => i.SubscriberId == subscriberId && i.LanguageId == languageId && i.Name == name)
                        .AsEnumerable()
                        .Select(i => new LocaleStringResourceModel
                        {
                            Id = i.Id,
                            IsDeleted = i.IsDeleted,
                            IsHtml = i.IsHtml,
                            IsMobileEnabled = i.IsMobileEnabled,
                            IsWebEnabled = i.IsWebEnabled,
                            LanguageId = i.LanguageId,
                            LanguageName = i.Language.Name,
                            Name = i.Name,
                            OperationId = i.OperationId,
                            SubscriberId = i.SubscriberId,
                            SubscriberName = i.Subscriber.FirstName + " " + i.Subscriber.LastName,
                            Value = i.Value
                        }).ToList();
                    return model;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }

        [Route("api/localStringResource/{id}")]
        [HttpGet]
        public object LocalStringResource(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return null;
                }
                LocaleStringResourceModel model = db.LocalStringResources
                    .Where(i => i.Id == id)
                    .AsEnumerable()
                    .Select(i => new LocaleStringResourceModel
                    {
                        Id = i.Id,
                        IsDeleted = i.IsDeleted,
                        IsHtml = i.IsHtml,
                        IsMobileEnabled = i.IsMobileEnabled,
                        IsWebEnabled = i.IsWebEnabled,
                        LanguageId = i.LanguageId,
                        LanguageName = i.Language.Name,
                        Name = i.Name,
                        OperationId = i.OperationId,
                        SubscriberId = i.SubscriberId,
                        SubscriberName = i.Subscriber.FirstName + " " + i.Subscriber.LastName,
                        Value = i.Value,
                        ResourceType = i.ResourceType
                    }).FirstOrDefault();
                return model;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }

        //Added by jay mistry 25-7-2017
        [Route("api/resourceTypes")]
        [HttpGet]
        public IHttpActionResult ResourceTypeList()
        {
            try
            {
                List<SelectListItem> actionList = Enum.GetValues(typeof(Constants.ResourceType)).Cast<Constants.ResourceType>()
               .Select(v => new SelectListItem
               {
                   Text = v.ToString(),
                   Value = ((int)v).ToString()
               }).OrderBy(r=>r.Text).ToList();//Changed By Hiren 06-01-2018

                return Ok(actionList);
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return Ok();
            }
        }

        #endregion

        [HttpGet]
        [Route("api/formResources")]
        [AllowAnonymous]
        public object FormResources(string url)
        {
            try
            {
                url = url.Replace("_", ".").ToLower();
                var subscriber = db.Subscribers.Where(s => s.PreferredDomain.ToLower() == url.ToLower() || s.SubDomain.ToLower() + "." + Constants.FORMZI_DOMAIN == url).FirstOrDefault();
                if (subscriber != null)
                {
                    int subscriberId = subscriber.Id;
                    Dictionary<string, object> PropertyName = new Dictionary<string, object>();
                    int _formType = (int)Constants.ResourceType.Form;

                    List<Database.Language> languageList = db.SubscriberLanguages.Where(i => i.IsPublished && i.SubcriberId == subscriberId)
                        .AsEnumerable()
                        .Select(i => new Database.Language
                        {
                            Id = i.Language.Id,
                            Name = i.Language.UniqueSeoCode
                        }).ToList();

                    foreach (var item in languageList)
                    {
                        Dictionary<string, string> model = db.LocalStringResources.Where(i => i.LanguageId == item.Id && i.SubscriberId == subscriberId && i.ResourceType == _formType)
                            .AsEnumerable().ToDictionary(p => p.Name, p => p.Value);

                        PropertyName.Add(item.Name, model);
                    }

                    return PropertyName;
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

        [HttpGet]
        [Route("api/MobileResources")]
        [AllowAnonymous]
        public object MobileResources(DateTime DateTime, int subscriberId = 0, int languageId = 0)
        {
            try
            {
                DateTime d = Convert.ToDateTime(DateTime);
                Dictionary<string, object> PropertyName = new Dictionary<string, object>();
                if (languageId == 0)
                    languageId = int.Parse(ConfigurationManager.AppSettings["AppBaseLanguageId"].ToString());

                int _appType = (int)Constants.ResourceType.App;

                var language = db.SubscriberLanguages.Where(i => i.IsPublished && i.SubcriberId == subscriberId)
                        .AsEnumerable()
                        .Select(i => new Database.Language
                        {
                            Id = i.Language.Id,
                            Name = i.Language.UniqueSeoCode
                        }).FirstOrDefault();

                Dictionary<string, string> resources = db.LocalStringResources.Where(i => i.SubscriberId == subscriberId && i.LanguageId == languageId && i.ResourceType == _appType && (i.CreatedOn >= d || i.UpdatedOn >= d)).AsEnumerable().ToDictionary(p => p.Name, p => p.Value);
                PropertyName.Add("Resources", resources);

                return (object)new
                {
                    AppResources = PropertyName,
                    LanguageSEOCode = language.UniqueSeoCode,
                    BaseLanguageId = languageId,
                    Datetime = Common.GetDateTime(db)
                };
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return false;
            }
        }
    }
}