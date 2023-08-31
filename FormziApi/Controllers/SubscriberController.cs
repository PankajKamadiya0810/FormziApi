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
using FormziApi.Extention;
using FormziApi.Models;
using FormziApi.Services;
using System.Web.WebPages.Html;
using System.Text;
using Newtonsoft.Json;
using System.IO;
using System.Net.Http.Headers;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Web;
using System.Globalization;

namespace FormziApi.Controllers
{
    [Authorize]
    [EnableCors("*", "*", "*")]
    public class SubscriberController : ApiController
    {
        #region Fields

        private LogProvider lp = new LogProvider("Formzi");
        private FormziEntities db = new FormziEntities();
        private SubscriberService _subscriberService = new SubscriberService();

        #endregion

        #region Subscriber

        [Route("api/subscriber/{id}")]
        [HttpGet]
        public object GetSubscriberInfo(int id)
        {
            try
            {
                //SubscriberModel sModel = new SubscriberModel();
                var appLogin = db.AppLogins.Where(e => e.SubscriberId == id).FirstOrDefault();
                string password = General.Decrypt(appLogin.Password);
                var subscriber = db.Subscribers.Where(e => e.Id == id).FirstOrDefault();
                var model = subscriber.ToModel<Subscriber, SubscriberModel>();
                model.SubscriptionPlan = subscriber.SubscriptionPlan.ToModel<SubscriptionPlan, SubscriptionPlanModel>();
                return new { SubscriberInfo = model, Email = appLogin.Email, Password = password };
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }

        [Route("api/getsubscriberplan/{id}")]
        [HttpGet]
        public object GetSubscriberPlan(int id)
        {
            try
            {
                int roleValue = Convert.ToInt16(Constants.Roles.SubscriberAdmin);
                int roleId = db.Roles.Where(r => r.Value == roleValue && r.SubscriberId==id).FirstOrDefault().Id;//Changed By Hiren 29-11-2017
                EmployeeRole employeeRole = db.EmployeeRoles.Where(e => e.RoleId == roleId && e.AppLogin.SubscriberId == id && !e.AppLogin.IsDeleted).FirstOrDefault();
                if (employeeRole != null)
                {
                    AppLogin appLogin = db.AppLogins.Where(a => a.Id == employeeRole.AppLoginId).FirstOrDefault();
                    if (appLogin != null)
                    {
                        Subscriber subscriber = db.Subscribers.Where(s => s.Id == id).FirstOrDefault();
                        var model = subscriber.ToModel<Subscriber, SubscriberModel>();
                        model.SubscriptionPlan = db.SubscriptionPlans.Where(p => p.Id == subscriber.SubscriptionPlanId).FirstOrDefault().ToModel<SubscriptionPlan, SubscriptionPlanModel>();
                        var address = db.Addresses.Where(a => a.Id == subscriber.AddressId).FirstOrDefault();
                        model.Address = address.ToModel<Address, AddressModel>();
                        model.Address.CountryName = address.Country.Name;
                        model.Address.StateName = address.StateProvince.Name;
                        model.Email = appLogin.Email;
                        model.ProfilePic = employeeRole.AppLogin.Employees.FirstOrDefault().ProfilePicture;
                        model.EmployeeId = employeeRole.AppLogin.Employees.FirstOrDefault().Id;
                        return model;
                    }
                    else
                        return null;
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

        [AllowAnonymous]
        [Route("api/GetSubscribers")]
        [HttpGet]
        public object AllSubscriberList()
        {
            try
            {
                var model = db.Subscribers.AsEnumerable().Select(i => new
                {
                    Id = i.Id,
                    FirstName = i.FirstName,
                    LastName = i.LastName,
                    Website = i.Website,
                    CompanyName = i.CompanyName,
                    PreferredDomain = i.PreferredDomain,
                    SubDomain = i.SubDomain,
                    CompanyLogo = !string.IsNullOrEmpty(i.CompanyLogo) ? i.AppSettings.Where(x => x.Key == Constants.FileUrl).FirstOrDefault().Value + i.Id + Constants.ImageFolder + Constants.SubscriberFolder + i.CompanyLogo : "",
                    Email = i.Email,
                    EmailVerificationCode = i.EmailVerificationCode,
                    IsEmailVerified = i.IsEmailVerified,
                    CreatedOn = i.CreatedOn,
                    UpdatedOn = i.UpdatedOn,
                    AddressId = i.AddressId,
                    SubscriptionPlanId = i.SubscriptionPlanId
                }).ToList();
                return model;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }

        [AllowAnonymous]
        [Route("api/SubscriberLogo")]
        [HttpGet]
        public object GetSubscriberLogo(string url)
        {
            try
            {
                url = url.Replace("_", ".").ToLower();
                string SubscriberLogo = string.Empty;
                var subscriber = db.Subscribers.Where(s => s.PreferredDomain.ToLower() == url.ToLower() || s.SubDomain.ToLower() + "." + Constants.FORMZI_DOMAIN == url).FirstOrDefault();
                //var subscriber = db.Subscribers.FirstOrDefault();

                if (subscriber != null && subscriber.Id > 0)
                {
                    var fileUrl = subscriber.AppSettings.Where(i => i.Key == Constants.FileUrl).FirstOrDefault().Value;
                    SubscriberLogo = !string.IsNullOrEmpty(subscriber.CompanyLogo) ? fileUrl + subscriber.Id + Constants.ImageFolder + Constants.SubscriberFolder + subscriber.CompanyLogo : "";
                }
                return SubscriberLogo;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return string.Empty;
            }
        }

        [AllowAnonymous]
        [Route("api/GetSubscriber")]
        [HttpGet]
        public object GetSubscriber(string subdomain)
        {
            try
            {
                if (!string.IsNullOrEmpty(subdomain))
                {
                    var subscriber = db.Subscribers.Where(s => s.SubDomain.ToLower() + "." + Constants.FORMZI_DOMAIN == subdomain.ToLower() || s.SubDomain.ToLower() + "." + Constants.IWITNESS_DOMAIN == subdomain.ToLower()).FirstOrDefault();
                    //var subscriber = db.Subscribers.FirstOrDefault();

                    if (subscriber != null && subscriber.Id > 0)
                    {
                        var model = subscriber.ToModel<Subscriber, SubscriberModel>();
                        var fileUrl = subscriber.AppSettings.Where(i => i.Key == Constants.FileUrl).FirstOrDefault().Value;
                        model.CompanyLogo = !string.IsNullOrEmpty(subscriber.CompanyLogo) ? fileUrl + subscriber.Id + Constants.ImageFolder + Constants.SubscriberFolder + subscriber.CompanyLogo : "";
                        return model;
                    }
                }
                return null;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return string.Empty;
            }
        }

        [Route("api/SaveSubscriber")]
        [HttpPost]
        public async Task<HttpResponseMessage> AddSubscriber()
        {
            try
            {
                SubscriberService _SubscriberService = new SubscriberService();

                var httpRequest = HttpContext.Current.Request;

                if (!Request.Content.IsMimeMultipartContent())
                    throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);

                SubscriberModel model = new SubscriberModel();
                AddressModel amodel = new AddressModel();
                if (!String.IsNullOrEmpty(httpRequest.Params["FirstName"]))
                    model.FirstName = httpRequest.Params["FirstName"];
                if (!String.IsNullOrEmpty(httpRequest.Params["LastName"]))
                    model.LastName = httpRequest.Params["LastName"];
                if (!String.IsNullOrEmpty(httpRequest.Params["Website"]))
                    model.Website = httpRequest.Params["Website"];
                if (!String.IsNullOrEmpty(httpRequest.Params["CompanyName"]))
                    model.CompanyName = httpRequest.Params["CompanyName"];
                if (!String.IsNullOrEmpty(httpRequest.Params["PreferredDomain"]))
                    model.PreferredDomain = httpRequest.Params["PreferredDomain"];
                if (!String.IsNullOrEmpty(httpRequest.Params["EmailVerificationCode"]))
                    model.EmailVerificationCode = httpRequest.Params["EmailVerificationCode"];
                if (!String.IsNullOrEmpty(httpRequest.Params["SubDomain"]))
                    model.SubDomain = httpRequest.Params["SubDomain"];
                if (!String.IsNullOrEmpty(httpRequest.Params["Email"]))
                    model.Email = httpRequest.Params["Email"];
                if (!String.IsNullOrEmpty(httpRequest.Params["Address1"]))
                    amodel.Address1 = httpRequest.Params["Address1"];
                if (!String.IsNullOrEmpty(httpRequest.Params["Address2"]))
                    amodel.Address2 = httpRequest.Params["Address2"];
                if (!String.IsNullOrEmpty(httpRequest.Params["City"]))
                    amodel.City = httpRequest.Params["City"];
                if (!String.IsNullOrEmpty(httpRequest.Params["ZipPostalCode"]))
                    amodel.ZipPostalCode = httpRequest.Params["ZipPostalCode"];
                if (!String.IsNullOrEmpty(httpRequest.Params["PhoneNumber"]))
                    amodel.PhoneNumber = httpRequest.Params["PhoneNumber"];
                if (!String.IsNullOrEmpty(httpRequest.Params["FaxNumber"]))
                    amodel.FaxNumber = httpRequest.Params["FaxNumber"];
                if (!String.IsNullOrEmpty(httpRequest.Params["CountryId"]))
                    amodel.CountryId = Convert.ToInt32(httpRequest.Params["CountryId"]);
                if (!String.IsNullOrEmpty(httpRequest.Params["StateProvinceId"]))
                    amodel.StateProvinceId = Convert.ToInt32(httpRequest.Params["StateProvinceId"]);

                model.Address = amodel;
                if (httpRequest.Files.Count > 0)
                {
                    var postedFile = httpRequest.Files[0];
                    model.CompanyLogo = postedFile.FileName;
                }

                int SubscriberId = _SubscriberService.AddSubscriber(model);
                if (SubscriberId > 0)
                {
                    if (httpRequest.Files.Count > 0)
                    {
                        String fileName = string.Empty;
                        var postedFile = httpRequest.Files[0];
                        fileName = postedFile.FileName;
                        AppSettingsController objapp = new AppSettingsController();
                        string root = objapp.GetValueByKey(Constants.FileRoot);
                        string imagePath = root + SubscriberId + Constants.ImageFolder + Constants.SubscriberFolder;
                        if (!Directory.Exists(imagePath))
                        {
                            Directory.CreateDirectory(imagePath);
                        }
                        postedFile.SaveAs(imagePath + fileName);
                    }
                }

                return Request.CreateResponse(HttpStatusCode.OK, "Subscriber inserted successfully");
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return Request.CreateResponse(HttpStatusCode.OK, "There is some error.");
            }
        }

        #endregion

        #region Synchronization

        [Route("api/SyncClients")]
        [HttpPost]
        public List<ClientDataModel> SyncClients([FromBody]SyncClientsModel dataModel)
        {
            try
            {
                DateTime d = Convert.ToDateTime(dataModel.DateTime);
                var clients = db.Clients.AsEnumerable().Where(i => i.SubscriberId == dataModel.SubscriberId && (i.CreatedOn >= d || i.UpdatedOn >= d))
                    .Select(i => new ClientDataModel
                    {
                        Id = i.Id,
                        FirstName = i.FirstName,
                        LastName = i.LastName,
                        CompanyName = i.CompanyName,
                        Email = i.Email,
                        PhoneNumber = i.PhoneNumber,
                        CreatedOn = i.CreatedOn,
                        UpdatedOn = i.UpdatedOn,
                        IsDeleted = i.IsDeleted,
                        SubscriberId = i.SubscriberId
                    }).ToList();
                return clients;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }

        [Route("api/SyncProjects")]
        [HttpPost]
        public List<ProjectDataModel> SyncProjects([FromBody]SyncProjectsModel dataModel)
        {
            try
            {
                DateTime d = Convert.ToDateTime(dataModel.DateTime);
                var projects = db.Projects.AsEnumerable().Where(i => dataModel.Clients.Contains(i.ClientId) && (i.CreatedOn >= d || i.UpdatedOn >= d))
                    .Select(i => new ProjectDataModel
                    {
                        Id = i.Id,
                        Name = i.Name,
                        Description = i.Description,
                        ClientId = i.ClientId,
                        IsActive = i.IsActive,
                        IsDeleted = i.IsDeleted,
                        CreatedOn = i.CreatedOn,
                        CreatedBy = i.CreatedBy,
                        UpdatedOn = i.UpdatedOn,
                        UpdatedBy = i.UpdatedBy
                    }).ToList();
                return projects;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }

        [Route("api/SyncProjectForms")]
        [HttpPost]
        public List<ProjectFormModel> SyncProjectForms([FromBody]SyncProjectFormModel dataModel)
        {
            try
            {
                DateTime d = Convert.ToDateTime(dataModel.DateTime);
                var projectForms = db.ProjectForms.AsEnumerable().Where(i => dataModel.Projects.Contains(i.ProjectId) && (i.CreatedOn >= d || i.UpdatedOn >= d))
                    .Select(i => new ProjectFormModel
                    {
                        Id = i.Id,
                        FormId = i.FormId,
                        ProjectId = i.ProjectId,
                        IsDeleted = i.IsDeleted,
                        CreatedOn = i.CreatedOn,
                        CreatedBy = i.CreatedBy,
                        UpdatedOn = i.UpdatedOn,
                        UpdatedBy = i.UpdatedBy
                    }).ToList();
                return projectForms;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }

        #endregion

        #region Country

        //By Jay Mistry 19-7-2017
        [Route("api/countryBySubscriber")]
        [HttpGet]
        public object CountryList(int subscriberId = 0)
        {
            try
            {
                List<Country> countryList = db.Countries.Where(c => c.SubscriberId == subscriberId).OrderBy(i => i.Name).AsEnumerable().Select(i => new Country
                {
                    Id = i.Id,
                    Name = i.Name,
                    TwoLetterIsoCode = i.TwoLetterIsoCode,
                    ThreeLetterIsoCode = i.ThreeLetterIsoCode,
                    NumericIsoCode = i.NumericIsoCode,
                    Published = i.Published,
                    DisplayOrder = i.DisplayOrder,
                    CountryFlag = i.CountryFlag,
                    SubscriberId = i.SubscriberId
                }).ToList();

                return countryList;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }

        [Route("api/country")]
        [HttpPost]
        public object AddCountry(Country model)
        {
            try
            {
                db.Countries.Add(model);
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

        [Route("api/updateCountry")]
        [HttpPut]
        public object UpdateCountry(Country model)
        {
            try
            {
                Country dbModel = db.Countries.Where(i => i.Id == model.Id).FirstOrDefault();

                dbModel.Name = model.Name;
                dbModel.DisplayOrder = model.DisplayOrder;
                dbModel.NumericIsoCode = model.NumericIsoCode;
                dbModel.Published = model.Published;
                dbModel.SubscriberId = model.SubscriberId;
                dbModel.ThreeLetterIsoCode = model.ThreeLetterIsoCode;
                dbModel.TwoLetterIsoCode = model.TwoLetterIsoCode;

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

        [Route("api/country/{id}")]
        [HttpDelete]
        public object RemoveCountry(int id = 0)
        {
            try
            {
                Country dbModel = db.Countries.Where(c => c.Id == id).FirstOrDefault();
                dbModel.Published = false;
                db.Entry(dbModel).State = EntityState.Modified;
                //db.Countries.Remove(dbModel);
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

        [Route("api/countryById")]
        [HttpGet]
        public object CountryById(int id = 0)
        {
            try
            {
                Country model = db.Countries.Where(c => c.Id == id).AsEnumerable().Select(i => new Country
                {
                    Id = i.Id,
                    Name = i.Name,
                    TwoLetterIsoCode = i.TwoLetterIsoCode,
                    ThreeLetterIsoCode = i.ThreeLetterIsoCode,
                    NumericIsoCode = i.NumericIsoCode,
                    Published = i.Published,
                    DisplayOrder = i.DisplayOrder,
                    CountryFlag = i.CountryFlag,
                    SubscriberId = i.SubscriberId
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

        [Route("api/countryByName")]
        [HttpGet]
        public object CountryByName(string name,int subscriberId)
        {
            try
            {
                return db.Countries.Where(c => c.Name == name && c.SubscriberId == subscriberId).Any();
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return false;
            }
        }

        #endregion

        #region States

        //By Jay Mistry 19-7-2017
        [Route("api/states/{countryId}")]
        [HttpGet]
        public object StateList(int countryId = 0)
        {
            try
            {
                List<StateProvince> stateList = db.StateProvinces.Where(c => c.CountryId == countryId).OrderBy(i => i.Name).AsEnumerable()
                    .Select(i => new StateProvince
                    {
                        Id = i.Id,
                        Name = i.Name,
                        Abbreviation = i.Abbreviation,
                        CountryId = i.CountryId,
                        DisplayOrder = i.DisplayOrder
                    }).ToList();

                return stateList;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }

        [Route("api/state")]
        [HttpPost]
        public object AddStates(StateProvince model)
        {
            try
            {
                db.StateProvinces.Add(model);
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

        [Route("api/state")]
        [HttpPut]
        public object UpdateState(StateProvince model)
        {
            try
            {
                StateProvince dbModel = db.StateProvinces.Where(i => i.Id == model.Id).FirstOrDefault();

                dbModel.Name = model.Name;
                dbModel.DisplayOrder = model.DisplayOrder;
                dbModel.Abbreviation = model.Abbreviation;
                dbModel.CountryId = model.CountryId;
                dbModel.DisplayOrder = model.DisplayOrder;

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

        [Route("api/state/{id}")]
        [HttpDelete]
        public object RemoveState(int id = 0)
        {
            try
            {
                StateProvince dbModel = db.StateProvinces.Where(c => c.Id == id).FirstOrDefault();
                db.StateProvinces.Remove(dbModel);
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


        [Route("api/stateById")]
        [HttpGet]
        public object StateById(int id = 0)
        {
            try
            {
                StateProvince model = db.StateProvinces.Where(c => c.Id == id)
                    .OrderBy(i => i.Name)
                    .AsEnumerable()
                    .Select(i => new StateProvince
                    {
                        Id = i.Id,
                        Name = i.Name,
                        Abbreviation = i.Abbreviation,
                        CountryId = i.CountryId,
                        DisplayOrder = i.DisplayOrder
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

        [Route("api/stateByName")]
        [HttpGet]
        public object StateByName(string name, int countryId = 0)
        {
            try
            {
                return db.StateProvinces.Where(c => c.Name == name && c.CountryId == countryId)
                    .AsEnumerable()
                    .Select(i => new StateProvince
                    {
                        Id = i.Id,
                        Name = i.Name,
                        Abbreviation = i.Abbreviation,
                        CountryId = i.CountryId,
                        DisplayOrder = i.DisplayOrder
                    }).Any();
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return false;
            }
        }

        #endregion

        #region AMC Specific - No more in use

        [AllowAnonymous]
        [HttpGet]
        //[Route("api/amcGetSearchAreaDetail")]
        public object AMCGetSearchAreaDetail(string areaName, string wardId)
        {
            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    var builder = new UriBuilder(Constants.AMC_CCRS_URL + "GetSearchAreaDetail");
                    builder.Port = -1;
                    var query = System.Web.HttpUtility.ParseQueryString(builder.Query);
                    query["AreaName"] = areaName;
                    query["WardID"] = "0";
                    builder.Query = query.ToString();
                    string URL = builder.ToString();

                    //string URL = Constants.AMC_CCRS_URL + "GetSearchAreaDetail?AreaName=" + areaName + "&WardID=0";

                    httpClient.BaseAddress = new Uri(URL);
                    var result = httpClient.GetAsync(URL).Result;
                    var returnData = result.Content.ReadAsStringAsync();



                    if (!string.IsNullOrEmpty(returnData.Result))
                    {

                        AmcCheckCode checkData = JsonConvert.DeserializeObject<AmcCheckCode>(returnData.Result);

                        if (checkData.Code <= 0)
                        {
                            return null;
                        }

                        AmcReturnData data = JsonConvert.DeserializeObject<AmcReturnData>(returnData.Result);

                        List<WardList> wardList = data.Head.GroupBy(x => x.Ward_ID).Select(x => x.First()).Select(x => new WardList
                        {
                            Ward_ID = x.Zone_ID,
                            Ward_Name = x.Ward_Name
                        }).OrderBy(o => o.Ward_Name).ToList();

                        List<ZoneList> zoneList = data.Head.GroupBy(x => x.Zone_ID).Select(x => x.First()).Select(x => new ZoneList
                        {
                            Zone_ID = x.Zone_ID,
                            Zone_Name = x.Zone_Name
                        }).OrderBy(o => o.Zone_Name).ToList();

                        List<AreaList> areasList = data.Head.GroupBy(x => x.Area_ID).Select(x => x.First()).Select(x => new AreaList
                        {
                            Area_ID = x.Area_ID,
                            Area_Name = x.Area_Name
                        }).ToList();

                        List<Head> allDataList = data.Head;

                        //data.Head.GroupBy(x => x.Area_ID).Select(x => x.First()).Select(x => new Head
                        //{
                        //    Area_ID = x.Area_ID,
                        //    Area_Name = x.Area_Name,
                        //    Area_Name_Gujarati = x.Area_Name_Gujarati,
                        //    Ward_ID = x.Zone_ID,
                        //    Ward_Name = x.Ward_Name,
                        //    Zone_ID = x.Zone_ID,
                        //    Zone_Name = x.Zone_Name
                        //}).ToList();

                        return new { AllDataList = allDataList, WardList = wardList, ZoneList = zoneList, AreasList = areasList };

                    }
                }

                return null; //GetSearchAreaDetail("bodakdev", "0");
            }
            catch (Exception)
            {
                throw;
            }

        }

        [AllowAnonymous]
        [HttpPost]
        //[Route("api/amcRegisterComplaint")]
        public object AMCRegisterComplaint(Complaint model)
        {
            try
            {
                if (model.SubmissionId <= 0 || model.SubscriberId <= 0)
                {
                    return false;
                }

                AmcTokenData tokenData = new AmcTokenData();
                FormSubmission submissionModel = new FormSubmission();
                string fileUrl = string.Empty;
                string folderName = string.Empty;
                string tokenNo = string.Empty;

                using (var client = new HttpClient())
                {
                    string URL = Constants.AMC_CCRS_URL + "RegisterComplaint";
                    client.BaseAddress = new Uri(URL);
                    var content = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("ComplainantMobile", model.ComplainantMobile),
                        new KeyValuePair<string, string>("Remarks", model.Remarks),
                        new KeyValuePair<string, string>("ComplainantName", model.ComplainantName),
                        new KeyValuePair<string, string>("ComplainantAddress", model.ComplainantAddress),
                        new KeyValuePair<string, string>("ComplainantEmailID", model.ComplainantEmailID),
                        new KeyValuePair<string, string>("ProblemID", model.ProblemID.ToString()),
                        new KeyValuePair<string, string>("ComplainantContact", model.ComplainantContact),
                        new KeyValuePair<string, string>("WardID", model.WardID.ToString()),
                        new KeyValuePair<string, string>("AreaID", model.AreaID.ToString()),
                        new KeyValuePair<string, string>("LocationAddr", model.LocationAddr)
                    });

                    var result = client.PostAsync(URL, content).Result;

                    if (result.IsSuccessStatusCode)
                    {
                        string returnData = result.Content.ReadAsStringAsync().Result;
                        AmcCheckCode checkData = JsonConvert.DeserializeObject<AmcCheckCode>(returnData);

                        if (checkData.Code <= 0)
                        {
                            return true;
                        }

                        tokenData = JsonConvert.DeserializeObject<AmcTokenData>(returnData);
                        tokenNo = tokenData.Head;

                        if (string.IsNullOrEmpty(tokenNo))
                        {
                            return true;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }

                if (model.SubmissionId > 0)
                {
                    submissionModel = db.FormSubmissions.Where(i => i.Id == model.SubmissionId).FirstOrDefault();
                    if (submissionModel != null)
                    {
                        folderName = submissionModel.FolderName.ToString();
                        model.SubscriberId = submissionModel.SubscriberId;
                    }
                }
                else
                {
                    return false;
                }

                if (model.SubscriberId > 0)
                {
                    AppSetting appSettingModel = db.AppSettings.Where(i => i.SubscriberId == model.SubscriberId && i.Key == Constants.FileUrl).FirstOrDefault();
                    fileUrl = appSettingModel != null ? appSettingModel.Value : string.Empty;
                }
                else
                {
                    return false;
                }

                string[] files = !string.IsNullOrEmpty(model.SubmissionImages) ? model.SubmissionImages.Split(',') : null;

                foreach (var item in files)
                {
                    string imagePath = fileUrl + model.SubscriberId + "/" + Constants.FormSubmissionFolder + submissionModel.FolderName + Constants.ImageFolder + item;

                    var webClient = new WebClient();
                    byte[] imageBytes = webClient.DownloadData(imagePath);  //Example : imagePath = "www.images.com/1454841390332.jpg"
                    string base64String = Convert.ToBase64String(imageBytes);

                    Byte[] bitmapData = Convert.FromBase64String(FixBase64ForImage(base64String));
                    System.IO.MemoryStream streamBitmap = new System.IO.MemoryStream(bitmapData);
                    //Bitmap bitImage = new Bitmap((Bitmap)Image.FromStream(streamBitmap));

                    System.IO.MemoryStream ms = new System.IO.MemoryStream();
                    //bImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png); //Old code

                    #region Compress Image
                    Image imgOriginal = Image.FromStream(streamBitmap);
                    //pass in whatever value you want 
                    ImageUpload imgUploadObj = new ImageUpload();
                    imgUploadObj.Width = 100;
                    Bitmap bitImage = (Bitmap)imgUploadObj.Scale(imgOriginal);
                    #endregion

                    bitImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                    byte[] byteImage = ms.ToArray();

                    var SigBase64 = Convert.ToBase64String(byteImage); //Get Base64

                    ms.Dispose();

                    string URL = Constants.AMC_CCRS_URL + "InsertImageforToken";
                    //Example : http://210.212.122.114/Mobile/webService/Citizenapp.asmx/InsertImageforToken

                    using (var client = new HttpClient())
                    {
                        client.BaseAddress = new Uri(URL);
                        var content = new FormUrlEncodedContent(new[]
                        {
                                new KeyValuePair<string, string>("TokenNo", tokenNo), //Ex : "SWM-09161092565"
                                new KeyValuePair<string, string>("Image", SigBase64), //Ex : "1454841390332.jpg"
                                new KeyValuePair<string, string>("Latitude", submissionModel.Latitude.ToString()), //Ex : "19.0953024"
                                new KeyValuePair<string, string>("Longitude", submissionModel.Longitude.ToString()) //Ex : "72.8549376"
                        });
                        var result = client.PostAsync(URL, content).Result;
                    }

                }
                return true;
            }
            catch (Exception)
            {
                //throw;
                return false;
            }
        }

        public string FixBase64ForImage(string Image)
        {
            System.Text.StringBuilder sbText = new System.Text.StringBuilder(Image, Image.Length);
            sbText.Replace("\r\n", String.Empty); sbText.Replace(" ", String.Empty);
            return sbText.ToString();
        }

        #endregion
    }
}