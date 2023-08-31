using FormziApi.Database;
using FormziApi.Helper;
using FormziApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FormziApi.Services
{
    public class SubscriberService
    {
        #region Fields

        LogProvider lp;
        private FormziEntities db;

        #endregion

        #region Constructor

        public SubscriberService()
        {
            lp = new LogProvider("Formzi");
            db = new FormziEntities();
        }

        public int AddSubscriber(SubscriberModel model)
        {
            try
            {
                Address amodel = new Address();
                amodel.Address1 = model.Address.Address1;
                amodel.Address2 = model.Address.Address2;
                amodel.City = model.Address.City;
                amodel.ZipPostalCode = model.Address.ZipPostalCode;
                amodel.PhoneNumber = model.Address.PhoneNumber;
                amodel.FaxNumber = model.Address.FaxNumber;
                amodel.CountryId = model.Address.CountryId;
                amodel.StateProvinceId = model.Address.StateProvinceId;
                amodel.CreatedOn = DateTime.Now;
                amodel.UpdatedOn = DateTime.Now;
                amodel.IsDeleted = false;
                db.Addresses.Add(amodel);
                db.SaveChanges();
                model.AddressId = amodel.Id;

                Subscriber smodel = new Subscriber();
                smodel.FirstName = model.FirstName;
                smodel.LastName = model.LastName;
                smodel.Website = model.Website;
                smodel.CompanyName = model.CompanyName;
                smodel.PreferredDomain = model.PreferredDomain;
                smodel.SubDomain = model.SubDomain;
                smodel.Email = model.Email;
                smodel.CompanyLogo = model.CompanyLogo;
                smodel.SubDomain = model.SubDomain;
                smodel.IsEmailVerified = true;
                smodel.EmailVerificationCode = "123";
                smodel.SubscriptionPlanId = 1;
                smodel.CreatedOn = DateTime.Now;
                smodel.UpdatedOn = DateTime.Now;
                smodel.AddressId = model.AddressId;
                db.Subscribers.Add(smodel);
                db.SaveChanges();
                return smodel.Id;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<LanguageModel> GetSubscriberLanguages(long subscriberId)
        {
            try
            {
                if (subscriberId == 0)
                    return null;
                int baseLanguageId = db.SubscriberLanguages.Where(i => (long)i.SubcriberId == subscriberId && i.IsPublished == true).OrderBy(o => o.DisplayOrder).FirstOrDefault().Id;
                return db.SubscriberLanguages.Where(i => (long)i.SubcriberId == subscriberId && i.IsPublished == true).OrderBy(o => o.DisplayOrder).Select(i => new LanguageModel()
                {
                    Id = i.Id,
                    Name = i.Language.Name,
                    LanguageCulture = i.Language.LanguageCulture,
                    DisplayOrder = i.DisplayOrder,
                    Published = i.IsPublished,
                    LanguageId = i.Language.Id,
                    UniqueSeoCode = i.Language.UniqueSeoCode,
                    BaseLanguage = i.Id == baseLanguageId
                }).ToList();
            }
            catch (Exception ex)
            {
                lp.Info(ex.Message);
                lp.HandleError(ex, ex.Message);
                return null;
            }
        }

        public Dictionary<string, object> GetAppResources(long subscriberId)
        {
            try
            {
                if (subscriberId == 0)
                    return null;
                Subscriber subscriber = db.Subscribers.Where((s => (long)s.Id == subscriberId)).FirstOrDefault();
                if (subscriber == null)
                    return null;

                Dictionary<string, object> PropertyName = new Dictionary<string, object>();

                List<Database.Language> languageList = db.SubscriberLanguages.Where(i => i.IsPublished && i.SubcriberId == subscriberId)
                        .AsEnumerable()
                        .Select(i => new Database.Language
                        {
                            Id = i.Language.Id,
                            Name = i.Language.UniqueSeoCode
                        }).ToList();

                foreach (var item in languageList)
                {
                    Dictionary<string, string> model = db.LocalStringResources.Where(i => i.LanguageId == item.Id && (long)i.SubscriberId == subscriberId && i.IsMobileEnabled).AsEnumerable().ToDictionary(p => p.Name, p => p.Value);
                    PropertyName.Add(item.Name, model);
                }
                return PropertyName;
            }
            catch (Exception ex)
            {
                lp.Info(ex.Message);
                lp.HandleError(ex, ex.Message);
                return null;
            }
        }

        public List<object> GetSubscribers(List<AppLogin> appLogin)
        {
            try
            {
                List<object> objectList = new List<object>();
                foreach (var appLoginItem in appLogin)
                {
                    string CompanyLogo;
                    if (string.IsNullOrEmpty(appLoginItem.Subscriber.CompanyLogo))
                        CompanyLogo = "";
                    else
                        CompanyLogo = appLoginItem.Subscriber.AppSettings.Where(x => x.Key == Constants.FileUrl).FirstOrDefault().Value + appLoginItem.Subscriber.Id + Constants.ImageFolder + Constants.SubscriberFolder + appLoginItem.Subscriber.CompanyLogo;

                    object obj = new
                    {
                        Id = appLoginItem.Subscriber.Id,
                        FirstName = appLoginItem.Subscriber.FirstName,
                        LastName = appLoginItem.Subscriber.LastName,
                        CompanyName = appLoginItem.Subscriber.CompanyName,
                        CompanyLogo = CompanyLogo,
                        AppLoginId = appLoginItem.Id
                    };
                    objectList.Add(obj);
                }
                return objectList;
            }
            catch (Exception ex)
            {
                lp.Info(ex.Message);
                lp.HandleError(ex, ex.Message);
                return null;
            }
        }
        #endregion
    }
}