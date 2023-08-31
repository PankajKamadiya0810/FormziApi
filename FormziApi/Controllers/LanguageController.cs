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
using System.IO;
using FormziApi.Models;
using Newtonsoft.Json;

namespace FormziApi.Controllers
{
    [Authorize]
    [EnableCors("*", "*", "*")]
    public class LanguageController : ApiController
    {
        #region Fields
        
        LogProvider lp = new LogProvider("Formzi");
        private FormziEntities db = new FormziEntities(); 
        
        #endregion

        #region Methods
        
        // Get languages
        [AllowAnonymous]
        [Route("api/languages")]
        [HttpGet]
        public object GetLanguages()
        {
            try
            {
                return db.Languages.Where(m => m.IsDeleted == false && m.Published == true).Select(m => new
                {
                    m.Id,
                    m.Name,
                    m.LanguageCulture,
                    m.FlagImageFileName,
                    m.Rtl,
                    m.Published,
                    m.IsDeleted,
                    m.DisplayOrder,
                    m.UniqueSeoCode
                }).ToList();
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }

        }

        // Get languages
        [Route("api/languageFlags")]
        [HttpGet]
        public object getFlags()
        {
            try
            {
                using (var client = new WebClient())
                {
                    return client.DownloadString(new Uri("http://admin.formzi.com/images/flags/flags.xml"));
                }
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }

        [Route("api/SubscriberPublishlanguages/{subscriberId}")]
        [HttpGet]
        public object GetSubscriberPublishLanguages(int subscriberId)
        {
            try
            {
                if (subscriberId == 0)
                {
                    return null;
                }

                var languageList = db.SubscriberLanguages.Where(i => i.SubcriberId == subscriberId && i.IsPublished == true).OrderBy(o => o.DisplayOrder);
                var baseLanguageId = languageList.FirstOrDefault().Id;

                var subscribersLanguageList = db.SubscriberLanguages.Where(i => i.SubcriberId == subscriberId && i.IsPublished == true)
                    .OrderBy(o => o.DisplayOrder)
                    .Select(i => new LanguageModel
                    {
                        Id = i.Id,
                        Name = i.Language.Name,
                        LanguageCulture = i.Language.LanguageCulture,
                        DisplayOrder = i.DisplayOrder,
                        Published = i.IsPublished,
                        LanguageId = i.Language.Id,
                        UniqueSeoCode = i.Language.UniqueSeoCode,
                        BaseLanguage = i.Id == baseLanguageId,
                    }).ToList();

                return subscribersLanguageList;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }

        [Route("api/Subscriberslanguages/{subscriberId}")]
        [HttpGet]
        public object GetSubscriberLanguages(int subscriberId=0)
        {
            try
            {
                if (subscriberId <= 0)
                {
                    return null;
                }
                //var subscribersLanguageList = db.SubscriberLanguages.Where(i => i.SubcriberId == subscriberId)
                //    .OrderBy(o => o.DisplayOrder)
                //    .Select(i => new LanguageModel
                //    {
                //        Id = i.Id,
                //        Name = i.Language.Name,
                //        LanguageCulture = i.Language.LanguageCulture,
                //        DisplayOrder = i.DisplayOrder,
                //        Published = i.IsPublished,
                //        LanguageId = i.Language.Id
                //    }).ToList();

                AppSetting appSettingModel = db.AppSettings.Where(i => i.Key == Constants.SubscriberBaseLanguage && i.SubscriberId == subscriberId).FirstOrDefault();
                int baseLanguageId = 0;

                if(appSettingModel != null)
                {
                    baseLanguageId = int.Parse(appSettingModel.Value);
                }

                List<LanguageModel> languageModelList = new List<LanguageModel>();

                var subscribersLanguageList = db.SubscriberLanguages.Where(i => i.SubcriberId == subscriberId)
                    .OrderBy(o => o.DisplayOrder)
                    .Select(i => new LanguageModel
                    {
                        Id = i.Id,
                        Name = i.Language.Name,
                        LanguageCulture = i.Language.LanguageCulture,
                        DisplayOrder = i.DisplayOrder,
                        Published = i.IsPublished,
                        LanguageId = i.Language.Id
                    }).ToList();

                foreach (var subsLangModel in subscribersLanguageList)
                {
                    LanguageModel model = new LanguageModel();
                    model.Id = subsLangModel.Id;
                    model.Name = subsLangModel.Name;
                    model.LanguageCulture = subsLangModel.LanguageCulture;
                    model.DisplayOrder = subsLangModel.DisplayOrder;
                    model.Published = subsLangModel.Published;
                    model.LanguageId = subsLangModel.LanguageId;
                    model.BaseLanguage = baseLanguageId == subsLangModel.LanguageId;

                    languageModelList.Add(model);
                }

                var LanguageList = db.Languages
                    .OrderBy(o => o.DisplayOrder)
                    .Select(i => new LanguageModel
                    {
                        Id = i.Id,
                        Name = i.Name,
                        LanguageCulture = i.LanguageCulture,
                        DisplayOrder = i.DisplayOrder,
                        Published = i.Published,
                        LanguageId = i.Id
                    }).ToList();

                int _displayOrder = 0;
                if (subscribersLanguageList.LastOrDefault() != null)
                {
                    _displayOrder = subscribersLanguageList.OrderByDescending(o => o.DisplayOrder).FirstOrDefault().DisplayOrder;
                }

                foreach (var subsLangModel in LanguageList)
                {
                    if (!(languageModelList.Any(i => i.LanguageId == subsLangModel.LanguageId)))
                    {
                        LanguageModel model = new LanguageModel();
                        model.Id = subsLangModel.Id;
                        model.Name = subsLangModel.Name;
                        model.LanguageCulture = subsLangModel.LanguageCulture;
                        model.DisplayOrder = _displayOrder;
                        model.Published = false;
                        model.LanguageId = subsLangModel.LanguageId;

                        languageModelList.Add(model);
                    }
                }

                return languageModelList;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }

        }

        [Route("api/SubscriberslanguageInfo/{subscriberId}/{id}")]
        [HttpGet]
        public object GetSubscribersLanguageInfo(int subscriberId, int id = 0)
        {
            try
            {
                if (id == 0)
                {
                    return null;
                }
                //int subscriberId = 0;

                List<SubscriberLanguage> subscribersLanguageList = db.SubscriberLanguages.Where(i => i.SubcriberId == subscriberId).ToList();
                SubscriberLanguage subModel = subscribersLanguageList.Where(i => i.SubcriberId == subscriberId && i.LanguageId == id).FirstOrDefault();

                int _displayOrder = 0;
                if (subscribersLanguageList.LastOrDefault() != null)
                {
                    _displayOrder = subscribersLanguageList.OrderByDescending(o => o.DisplayOrder).FirstOrDefault().DisplayOrder;
                }

                bool IsLanguagePublished = false;

                var subscribersLanguageDetails = subscribersLanguageList.Where(i => i.LanguageId == id).FirstOrDefault();
                if (subscribersLanguageDetails != null)
                {
                    IsLanguagePublished = subscribersLanguageDetails.IsPublished;
                }

                if (subModel != null)
                {
                    LanguageModel model = new LanguageModel();

                    //var subsLangModel = db.SubscriberLanguages.Where(i => i.Id == id).FirstOrDefault();

                    model.Id = subModel.Id;
                    model.Name = subModel.Language.Name;
                    model.LanguageCulture = subModel.Language.LanguageCulture;
                    model.DisplayOrder = IsLanguagePublished ? subModel.DisplayOrder : _displayOrder;
                    model.Published = subModel.IsPublished;
                    model.LanguageId = subModel.Language.Id;

                    return model;
                }
                else
                {
                    LanguageModel model = new LanguageModel();

                    var subsLangModel = db.Languages.Where(i => i.Id == id).FirstOrDefault();

                    model.Id = subsLangModel.Id;
                    model.Name = subsLangModel.Name;
                    model.LanguageCulture = subsLangModel.LanguageCulture;
                    model.DisplayOrder = _displayOrder;
                    model.Published = false;
                    model.LanguageId = subsLangModel.Id;

                    return model;
                }
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }

        }

        [Route("api/UpdateSubscriberslanguageInfo")]
        [HttpPost]
        public object UpdateSubscribersLanguageInfo(int subscriberId, string modelJson)
        {
            try
            {
                LanguageModel model = JsonConvert.DeserializeObject<LanguageModel>(modelJson);
                if (model == null || model.DisplayOrder == 0)
                {
                    return false;
                }

                List<SubscriberLanguage> subscriberLanguageList = db.SubscriberLanguages.Where(i => i.SubcriberId == subscriberId).ToList();
                SubscriberLanguage subModel = subscriberLanguageList.Where(i => i.SubcriberId == subscriberId && i.LanguageId == model.LanguageId).FirstOrDefault();

                if (subModel != null)
                {
                    subModel.IsPublished = model.Published;
                    subModel.DisplayOrder = model.DisplayOrder;

                    db.Entry(subModel).State = EntityState.Modified;
                    db.SaveChanges();

                    return true;
                }
                else
                {
                    SubscriberLanguage subscriberModel = new SubscriberLanguage();
                    subscriberModel.IsPublished = true;
                    subscriberModel.DisplayOrder = model.DisplayOrder;
                    subscriberModel.LanguageId = model.LanguageId;
                    subscriberModel.SubcriberId = subscriberId;

                    db.SubscriberLanguages.Add(subscriberModel);
                    db.SaveChanges();

                    return true;
                }
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