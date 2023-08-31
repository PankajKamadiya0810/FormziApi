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
using Newtonsoft.Json;
using FormziApi.Models;
using Newtonsoft.Json.Linq;
using FormziApi.Extention;
using System.Configuration;
using System.Net.Mail;
using System.IO;
using FormziApi.Services;
using System.Globalization;

namespace FormziApi.Controllers
{
    [Authorize]
    [EnableCors("*", "*", "*")]
    public class FormAnswersController : ApiController
    {
        #region Fields

        private LogProvider lp = new LogProvider("FormAnswersController");
        private FormziEntities db = new FormziEntities();

        #endregion

        #region Methods

        /// <summary>
        /// Add form answers when form fills.
        /// 2nd method call when form is submitted.
        /// It will read json file which is physically stored.
        /// </summary>
        /// <param name="JSONUrl"></param>
        [AllowAnonymous]
        [Route("api/AddFormAnswer")]
        [HttpGet]
        public void AddFormAnswer([FromUri]string[] JSONUrl)
        {
            try
            {
                string fileUrl = string.Join("\\", JSONUrl);
                //int languageId = 1;
                //string subscriberLogo = "";
                //var logoFileUrl = "";
                string emailSignature = "";
                bool isEmergencyResponse = false;
                var model = JsonConvert.DeserializeObject<FormSubmissionModel>(System.IO.File.ReadAllText(fileUrl));
                // Insert form submission data
                FormSubmission formSubmission = new FormSubmission();
                DateTime currentData = Common.GetDateTime(db);
                if (model.AppInfoID > 0)
                {
                    formSubmission.AppInfoId = model.AppInfoID; //added by jay on 29-6-2016
                }
                formSubmission.DeviceId = model.DeviceId;
                formSubmission.LatLong = model.LatLong;
                if (model.LatLong != "null" && !string.IsNullOrEmpty(model.LatLong))
                {
                    formSubmission.Latitude = decimal.Parse(model.LatLong.Split(',')[0]);
                    formSubmission.Longitude = decimal.Parse(model.LatLong.Split(',')[1]);
                }
                formSubmission.SubmittedOn = currentData;
                formSubmission.FormId = model.FormId;
                formSubmission.SubscriberId = model.SubscriberId;
                formSubmission.FolderName = new Guid(JSONUrl[JSONUrl.Length - 1].Split('.')[0].ToString());
                formSubmission.FormVersionId = model.VersionId;
                formSubmission.LanguageId = model.LanguageId;
                formSubmission.EmployeeId = model.EmployeeId;
                formSubmission.CreatedOn = currentData;
                formSubmission.UpdatedOn = currentData;
                formSubmission.IsApproved = 1;
                formSubmission.ApprovedBy = 1;
                formSubmission.ApprovedOn = currentData;
                db.FormSubmissions.Add(formSubmission);
                db.SaveChanges();

                Form form = db.Forms.Where(f => f.Id == formSubmission.FormId).FirstOrDefault();//Added By Hiren 9-11-2017
                List<FormAnswer> entitylist = new List<FormAnswer>();
                List<FormAnswer> requiredlist = new List<FormAnswer>();//Added By Hiren 13-11-2017
                foreach (var item in model.FormAnswers)
                {
                    FormAnswer entity = new FormAnswer();
                    entity.FormSubmissionId = formSubmission.Id;
                    FormQuestion formQuestion = db.FormQuestions.Find(item.FormQuestionId);
                    dynamic question = JsonConvert.DeserializeObject<object>(formQuestion.JSONQuestion);
                    entity.FormQuestionId = item.FormQuestionId;
                    entity.CreatedBy = item.CreatedBy;

                    if (Convert.ToString(question["component"].Value) == Constants.CaptureImage) entity.ElementType = Convert.ToInt16(Constants.ElementType.CaptureImage);
                    else if (Convert.ToString(question["component"].Value) == Constants.ELEMENT_TYPE_TEXT) entity.ElementType = Convert.ToInt16(Constants.ElementType.Text);
                    else if (Convert.ToString(question["component"].Value) == Constants.ELEMENT_TYPE_BARCODE_QRCODE) entity.ElementType = Convert.ToInt16(Constants.ElementType.Barcode);
                    //else if (Convert.ToString(question["component"].Value) == Constants.ELEMENT_TYPE_FILEUPLOAD) entity.ElementType = Convert.ToInt16(Constants.ElementType.CaptureImage);
                    //else if (Convert.ToString(question["component"].Value) == Constants.ELEMENT_TYPE_FILEUPLOAD) entity.ElementType = Convert.ToInt16(Constants.ElementType.CaptureVideo);
                    else if (Convert.ToString(question["component"].Value) == Constants.ELEMENT_TYPE_CHECKBOX) entity.ElementType = Convert.ToInt16(Constants.ElementType.Checkbox);
                    else if (Convert.ToString(question["component"].Value) == Constants.ELEMENT_TYPE_DATE) entity.ElementType = Convert.ToInt16(Constants.ElementType.Date);
                    else if (Convert.ToString(question["component"].Value) == Constants.ELEMENT_TYPE_EMAIL) entity.ElementType = Convert.ToInt16(Constants.ElementType.Email);
                    //else if (Convert.ToString(question["component"].Value) == Constants.ELEMENT_TYPE_FILEUPLOAD) entity.ElementType = Convert.ToInt16(Constants.ElementType.FileUpload);
                    else if (Convert.ToString(question["component"].Value) == Constants.ELEMENT_TYPE_FORMHEADER) entity.ElementType = Convert.ToInt16(Constants.ElementType.Formheader);
                    else if (Convert.ToString(question["component"].Value) == Constants.ELEMENT_TYPE_GEOLOCATION) entity.ElementType = Convert.ToInt16(Constants.ElementType.GeoLocation);
                    else if (Convert.ToString(question["component"].Value) == Constants.ELEMENT_TYPE_LABEL) entity.ElementType = Convert.ToInt16(Constants.ElementType.Label);
                    else if (Convert.ToString(question["component"].Value) == Constants.ELEMENT_TYPE_MEASUREMENT) entity.ElementType = Convert.ToInt16(Constants.ElementType.Measurment);
                    else if (Convert.ToString(question["component"].Value) == Constants.ELEMENT_TYPE_NAME) entity.ElementType = Convert.ToInt16(Constants.ElementType.Name);
                    else if (Convert.ToString(question["component"].Value) == Constants.ELEMENT_TYPE_ADDRESS) entity.ElementType = Convert.ToInt16(Constants.ElementType.Address);//Added By Hiren 20-11-2017
                    else if (Convert.ToString(question["component"].Value) == Constants.ELEMENT_TYPE_NUMBER) entity.ElementType = Convert.ToInt16(Constants.ElementType.Number);
                    else if (Convert.ToString(question["component"].Value) == Constants.ELEMENT_TYPE_PAGE) entity.ElementType = Convert.ToInt16(Constants.ElementType.Page);
                    else if (Convert.ToString(question["component"].Value) == Constants.ELEMENT_TYPE_PHONE) entity.ElementType = Convert.ToInt16(Constants.ElementType.Phone);
                    else if (Convert.ToString(question["component"].Value) == Constants.ELEMENT_TYPE_PRICE) entity.ElementType = Convert.ToInt16(Constants.ElementType.Price);
                    else if (Convert.ToString(question["component"].Value) == Constants.ELEMENT_TYPE_RADIO) entity.ElementType = Convert.ToInt16(Constants.ElementType.Radio);
                    else if (Convert.ToString(question["component"].Value) == Constants.ELEMENT_TYPE_SECTION) entity.ElementType = Convert.ToInt16(Constants.ElementType.Section);
                    else if (Convert.ToString(question["component"].Value) == Constants.ELEMENT_TYPE_SELECT) entity.ElementType = Convert.ToInt16(Constants.ElementType.Select);
                    else if (Convert.ToString(question["component"].Value) == Constants.ELEMENT_TYPE_SIGNATURE) entity.ElementType = Convert.ToInt16(Constants.ElementType.Signature);
                    else if (Convert.ToString(question["component"].Value) == Constants.ELEMENT_TYPE_SUMMARY) entity.ElementType = Convert.ToInt16(Constants.ElementType.Summary);
                    else if (Convert.ToString(question["component"].Value) == Constants.ELEMENT_TYPE_TEXTAREA) entity.ElementType = Convert.ToInt16(Constants.ElementType.TextArea);
                    else if (Convert.ToString(question["component"].Value) == Constants.ELEMENT_TYPE_TIME) entity.ElementType = Convert.ToInt16(Constants.ElementType.Time);
                    else if (Convert.ToString(question["component"].Value) == Constants.ELEMENT_TYPE_TOGGLE) entity.ElementType = Convert.ToInt16(Constants.ElementType.Toggle);
                    else if (Convert.ToString(question["component"].Value) == Constants.ELEMENT_TYPE_URL) entity.ElementType = Convert.ToInt16(Constants.ElementType.Url);
                    else if (Convert.ToString(question["component"].Value) == Constants.ELEMENT_TYPE_HAPPINESS) entity.ElementType = Convert.ToInt16(Constants.ElementType.Happiness);//Added By Hiren 27-10-2017
                    else entity.ElementType = 0;
                    if (!(entity.ElementType == Convert.ToInt16(Constants.ElementType.Formheader) || entity.ElementType == Convert.ToInt16(Constants.ElementType.Label)))
                    {
                        entity.CreatedOn = item.CreatedOn;
                        entity.UpdatedBy = item.CreatedBy;
                        entity.UpdatedOn = item.CreatedOn;
                        //Added By Hiren 16-11-2017
                        if (item.IsReadOnly && entity.ElementType == Convert.ToInt16(Constants.ElementType.Radio))
                            item.Value = "";
                        if (item.IsReadOnly && entity.ElementType == Convert.ToInt16(Constants.ElementType.Date))
                            item.Value = "";
                        if (item.IsReadOnly && entity.ElementType == Convert.ToInt16(Constants.ElementType.Time))
                            item.Value = "";
                        if (item.IsReadOnly && entity.ElementType == Convert.ToInt16(Constants.ElementType.Toggle))
                            item.Value = "";
                        if (item.IsReadOnly && entity.ElementType == Convert.ToInt16(Constants.ElementType.Select))
                            item.Value = "";
                        //End
                        entity.Value = item.Value;
                        entitylist.Add(entity);
                    }
                    //Added By Hiren 13-11-2017
                    if (Convert.ToBoolean(question["required"].Value) && String.IsNullOrEmpty(item.Value))
                    {
                        requiredlist.Add(entity);
                    }
                    //Workflow notifications Added by Hiren 10-11-2017
                    if (entity.ElementType == Convert.ToInt16(Constants.ElementType.Section))
                    {
                        if (form.WorkFlowEnabled && !item.IsReadOnly && model.EmployeeId != 0)
                        {
                            Employee employee = db.Employees.Where(f => f.Id == model.EmployeeId).FirstOrDefault();
                            if (employee != null)
                            {
                                //Changed By Hiren 22-11-2017
                                int emproleId = db.EmployeeRoles.Where(q => q.AppLoginId == employee.AppLoginId).FirstOrDefault().RoleId;
                                if (emproleId != 0)
                                {
                                    var userReportTo = db.UserReportToes.Where(x => x.EmpId == model.EmployeeId && x.EmpRoleId == emproleId).FirstOrDefault();
                                    if (userReportTo != null)
                                    {
                                        var userEmail = (from emp in db.Employees
                                                         join app in db.AppLogins on emp.AppLoginId equals app.Id
                                                         join ur in db.UserReportToes on emp.Id equals ur.ReportToEmpId
                                                         where ur.EmpId == userReportTo.EmpId
                                                         select new { app.Email, app.Id, emp.FirstName, emp.LastName }).FirstOrDefault();
                                        if (userEmail != null)
                                        {
                                            string toemailaddress = userEmail.Email;
                                            string employeefullname = employee.FirstName + ' ' + employee.LastName;
                                            string toempfullname = userEmail.FirstName + ' ' + userEmail.LastName;//Added By Hiren 06-12-2017
                                            //Added By Hiren 24-11-2017
                                            string submissionurl = "/#/Submission/edit/" + formSubmission.Id + "/" + userEmail.Id + '/';
                                            SubmissionService _submissionService = new SubmissionService();
                                            _submissionService.SendSubmissionSectionReportMail(model.SubscriberId, form.Name, employeefullname,toempfullname,toemailaddress, "New Submission Received!", submissionurl);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    //End
                }
                if (entitylist != null)
                {
                    db.FormAnswers.AddRange(entitylist);
                    db.SaveChanges();
                }
                //Added By Hiren 13-11-2017  //Insert form submission Log data
                SubmissionLog SubmissionLog = new SubmissionLog();
                if (formSubmission.Id != 0 && model.FormId != 0)
                {
                    SubmissionLog.FormId = model.FormId;
                    SubmissionLog.SubmissionId = formSubmission.Id;
                    SubmissionLog.EmployeeId = model.EmployeeId;
                    SubmissionLog.CreatedOn = currentData;
                    SubmissionLog.UpdatedOn = currentData;
                    db.SubmissionLogs.Add(SubmissionLog);
                    db.SaveChanges();
                }
                if (requiredlist != null && requiredlist.Count > 0)
                {
                    FormSubmission dbModel = db.FormSubmissions.Where(i => i.Id == formSubmission.Id).FirstOrDefault();
                    if (dbModel != null)
                    {
                        dbModel.IsCompleted = false;
                        dbModel.UpdatedOn = currentData;
                        db.Entry(dbModel).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }
                else
                {
                    FormSubmission dbModel = db.FormSubmissions.Where(i => i.Id == formSubmission.Id).FirstOrDefault();
                    if (dbModel != null)
                    {
                        dbModel.IsCompleted = true;
                        dbModel.UpdatedOn = currentData;
                        db.Entry(dbModel).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }
                //End
                //Note by jay mistry
                //Here static value '1' is used for I-Witness's subscriber id
                if (!string.IsNullOrEmpty(formSubmission.DeviceId) && formSubmission.AppInfoId > 0)
                {
                    FormAnswerService _formAnswerService = new FormAnswerService();
                    //Added by jay on 17-6-2016
                    //Notification Mail
                    NotificationService notificationService = new NotificationService();
                    notificationService.AppUserNotification(5, formSubmission.Id, formSubmission.DeviceId, formSubmission.SubscriberId, formSubmission.AppInfoId);
                    //Added by jay on 17-6-2016
                    //Sms to I-Witness Admin
                    //It Code will be execute based on settings.
                    string message = "";
                    string phoneNumber = "";
                    bool isSendNewPostSMS = false; // if true then send new post sms
                    string smsRoute = "default";
                    string smsSenderId = "iWitns";

                    AppSetting appSettingModel = new AppSetting();
                    //appSettingList = All App Settings of subscriber
                    List<AppSetting> appSettingList = db.AppSettings.Where(i => i.SubscriberId == model.SubscriberId).ToList();
                    appSettingModel = appSettingList.Where(i => i.Key == Constants.IS_SEND_NEW_POST_SMS).FirstOrDefault();
                    if (appSettingModel != null)
                    {
                        isSendNewPostSMS = appSettingModel.Value == "true" ? true : false;
                        appSettingModel = appSettingList.Where(i => i.Key == Constants.EmailSignature).FirstOrDefault();
                        AppUserInfo appUserInfo = new AppUserInfo();
                        if (formSubmission.AppInfoId > 0)
                        {
                            appUserInfo = db.AppUserInfoes.Where(i => i.AppInfoID == formSubmission.AppInfoId).FirstOrDefault();
                        }
                        else
                        {
                            appUserInfo = db.AppUserInfoes.Where(i => i.DeviceId == formSubmission.DeviceId).OrderByDescending(o => o.AppInfoID).FirstOrDefault();
                        }
                        phoneNumber = appSettingList.Where(i => i.Key == Constants.ADMIN_PHONE).FirstOrDefault() != null ? appSettingList.Where(i => i.Key == Constants.ADMIN_PHONE).FirstOrDefault().Value : string.Empty;
                        smsRoute = appSettingList.Where(i => i.Key == Constants.SMS_ROUTE).FirstOrDefault() != null ? appSettingList.Where(i => i.Key == Constants.SMS_ROUTE).FirstOrDefault().Value : string.Empty;
                        smsSenderId = appSettingList.Where(i => i.Key == Constants.SMS_SENDER_ID).FirstOrDefault() != null ? appSettingList.Where(i => i.Key == Constants.SMS_SENDER_ID).FirstOrDefault().Value : string.Empty;
                        if (isSendNewPostSMS)
                        {
                            _formAnswerService.SendNewSubmissionMail(model.SubscriberId);
                        }
                        if (isSendNewPostSMS && appSettingModel != null && appUserInfo != null && !string.IsNullOrEmpty(phoneNumber) && !string.IsNullOrEmpty(smsRoute) && !string.IsNullOrEmpty(smsSenderId))
                        {
                            emailSignature = appSettingModel.Value;
                            message = appSettingList.Where(i => i.Key == Constants.SMS_NEW_POST_ADMIN).FirstOrDefault() != null ? appSettingList.Where(i => i.Key == Constants.SMS_NEW_POST_ADMIN).FirstOrDefault().Value : string.Empty;
                            bool isFromIndia = appUserInfo.PhoneNo.Substring(0, 2) == "91";
                            if (isFromIndia)
                            {
                                AppUserInfoService appUserInfoService = new AppUserInfoService();
                                appUserInfoService.SendSMS(phoneNumber, message + emailSignature, formSubmission.SubscriberId, smsRoute, smsSenderId);
                            }
                        }
                        Form formDetails = db.Forms.Where(i => i.Id == formSubmission.FormId).FirstOrDefault();
                        if (formDetails != null)
                        {
                            isEmergencyResponse = appSettingList.Where(i => i.Key == Constants.EMERGENCY_RESPONSE_FORM).FirstOrDefault() != null ? appSettingList.Where(i => i.Key == Constants.EMERGENCY_RESPONSE_FORM).FirstOrDefault().Value == formDetails.Name : false;
                            if (isEmergencyResponse)
                            {
                                AcceptEmergencyResponseForm(formSubmission.Id);
                            }
                        }
                        if (model.LatLong != "null" && !string.IsNullOrEmpty(model.LatLong))
                        {
                            _formAnswerService.AddPostLocation(model.LatLong.Split(',')[0], model.LatLong.Split(',')[1], formSubmission.Id);
                        }
                    }
                }
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                var newException = new FormattedDbEntityValidationException(e);
                //throw newException;
                System.Diagnostics.Debug.WriteLine(newException);
                lp.Info(newException.ToString());
                lp.HandleError(e, newException.ToString());
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
            }
        }

        /// <summary>
        /// Add form answers
        /// </summary>
        /// <param name="formAnswers"></param>
        [Route("api/FormAnswers")]
        [HttpPost]
        public void PostFormAnswer([FromBody]List<FormAnswer> formAnswers)
        {
            try
            {
                foreach (FormAnswer formAnswer in formAnswers)
                {
                    formAnswer.CreatedOn = Common.GetDateTime(db);
                    formAnswer.UpdatedOn = Common.GetDateTime(db);
                    db.FormAnswers.Add(formAnswer);
                    db.SaveChanges();
                }
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
            }
        }

        #endregion

        #region I-Witness Specific

        /// <summary>
        /// Get list of submission (I-Witness specific)
        /// </summary>
        /// <param name="model">DateTime, UserDate</param>
        /// <returns></returns>
        [AllowAnonymous]
        [Route("api/SubmissionList")]
        [HttpPost]
        public object SubmissionList([FromBody] FormSubmissionDateModel model)
        {
            try
            {
                DateTime date = Convert.ToDateTime(model.DateTime);
                DateTime userDate = Convert.ToDateTime(model.UserDate).ToUniversalTime(); //  new DateTime(model.Year, model.Month, model.Day).ToUniversalTime();
                long id = model.FormSubmissionId;
                var formIds = db.Forms.Where(i => i.SubscriberId == model.SubscriberId && i.IsActive && !i.IsDeleted).Select(i => i.Id).ToList();
                IList<FormSubmission> data = db.FormSubmissions.Where(f => f.Id > id && f.SubscriberId == model.SubscriberId && f.SubmittedOn >= date && f.IsApproved == 1 && f.AppInfoId != null && !f.IsDeleted && formIds.Contains(f.FormId))
                  .OrderBy(o => o.Id).AsEnumerable().ToList();

                //Updated by jay on 8-6-2016 Wednesday
                //FilterId is use to filter data by days, months, year and show all data.
                //0 = nothing
                //1 = upto 1days older
                //2 = upto 15days older
                //3 = upto 1month older
                //4 = upto 3month older
                switch (model.FilterId)
                {
                    case 1:
                        {
                            DateTime SelectedDate = userDate;
                            data = data.Where(x => DateTime.Compare(x.SubmittedOn.ToUniversalTime(), userDate) >= 0).AsEnumerable().ToList();
                            break;
                        }
                    case 2:
                        {
                            DateTime SelectedDate = userDate.AddDays(-15);
                            data = data.Where(x => DateTime.Compare(x.SubmittedOn, SelectedDate) >= 0).AsEnumerable().ToList();
                            break;
                        }
                    case 3:
                        {
                            DateTime SelectedDate = userDate.AddMonths(-1);
                            data = data.Where(x => DateTime.Compare(x.SubmittedOn, SelectedDate) >= 0).AsEnumerable().ToList();
                            break;
                        }
                    case 4:
                        {
                            DateTime SelectedDate = userDate.AddMonths(-3);
                            data = data.Where(x => DateTime.Compare(x.SubmittedOn, SelectedDate) >= 0).AsEnumerable().ToList();
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }

                var inserted = data.AsEnumerable()
                .Select(f => new FormSubmissionList
                {
                    Id = f.Id,
                    FormId = f.FormId,
                    FolderName = f.FolderName,
                    LatLong = f.LatLong,
                    SubmittedOn = DateTime.SpecifyKind(f.SubmittedOn, DateTimeKind.Utc),
                    SubscriberId = f.SubscriberId,
                    FormName = f.Form.Name,
                    Image = f.Form.Image,
                    IsApproved = (int)f.IsApproved
                }).ToList();

                var insertedAnswer = inserted.Join(db.FormAnswers, fs => fs.Id, fa => fa.FormSubmissionId, (fs, fa) => new FormAnswerModel
                {
                    Id = fa.Id,
                    Value = fa.Value,
                    ElementType = fa.ElementType,
                    FormQuestionId = fa.FormQuestionId,
                    FormSubmissionId = fa.FormSubmissionId,
                    JSONQuestion = JsonConvert.DeserializeObject<FormComponent>(fa.FormQuestion.JSONQuestion).label
                }).ToList();

                var updated = Enumerable.Empty<FormSubmissionList>();
                var updatedAnswer = Enumerable.Empty<FormAnswerModel>();
                if (id > 0)
                {

                    updated = db.FormSubmissions.Where(f => f.SubscriberId == model.SubscriberId && f.ApprovedOn >= date && f.IsApproved != null && f.AppInfoId != null && !f.IsDeleted)
                         .OrderBy(o => o.Id)
                         .AsEnumerable()
                         .Select(f => new FormSubmissionList
                         {
                             Id = f.Id,
                             FormId = f.FormId,
                             FolderName = f.FolderName,
                             LatLong = f.LatLong,
                             SubmittedOn = f.SubmittedOn,
                             SubscriberId = f.SubscriberId,
                             FormName = f.Form.Name,
                             Image = f.Form.Image,
                             IsApproved = (int)f.IsApproved
                         }).ToList();

                    updatedAnswer = updated.Join(db.FormAnswers, fs => fs.Id, fa => fa.FormSubmissionId, (fs, fa) => new FormAnswerModel
                    {
                        Id = fa.Id,
                        Value = fa.Value,
                        ElementType = fa.ElementType,
                        FormQuestionId = fa.FormQuestionId,
                        FormSubmissionId = fa.FormSubmissionId,
                        JSONQuestion = JsonConvert.DeserializeObject<FormComponent>(fa.FormQuestion.JSONQuestion).label
                    }).ToList();
                }

                date = Common.GetDateTime(db);
                return new { Inserted = inserted, Updated = updated, CurrentDate = date, InsertedAnswer = insertedAnswer, UpdatedAnswer = updatedAnswer };

            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }

        }

        /// <summary>
        /// Added by jay on 18-2-2016
        /// i-witness specific, Mobile
        /// Get form submission by lat long and zoom        
        /// </summary>
        /// <param name="model">DateTime, UserDate</param>
        /// <returns></returns>        
        [AllowAnonymous]
        [Route("api/SubmissionListByLatLng")]
        [HttpPost]
        public object SubmissionList([FromBody] FormSubmissionLatLngDateModel model)
        {
            try
            {
                AppUserInfoService appUserInfoService = new AppUserInfoService();
                if (!appUserInfoService.CheckAppAuthentication(model.AuthKey, model.SubscriberId))
                {
                    return null;
                }

                double orig_lat = double.Parse(model.Latitude);
                double orig_long = double.Parse(model.Longitude);
                double bounding_distance = model.Distance;
                model.SubscriberId = model.SubscriberId > 0 ? model.SubscriberId : 1;
                var inserted = db.GetSubmissionByLatLngDis((decimal)orig_lat, (decimal)orig_long, (decimal)bounding_distance).ToList()
                    .Where(i => i.Latitude != null && i.Latitude > 0 && i.SubscriberId == model.SubscriberId)
                    .GroupBy(i => i.FormSubmissionId)
                    .AsEnumerable()
                    .Select(j => new InnfyFormSubmission
                    {
                        FormSubmissionId = j.Key,
                        FormId = j.FirstOrDefault().FormId,
                        FormName = j.FirstOrDefault().FormName,
                        Latitude = j.FirstOrDefault().Latitude,
                        Longitude = j.FirstOrDefault().Longitude,
                        FolderName = j.FirstOrDefault().FolderName,
                        FormImage = j.FirstOrDefault().FormImage,
                        IsApproved = j.FirstOrDefault().IsApproved,
                        SubmittedOn = j.FirstOrDefault().SubmittedOn,
                        SubscriberId = j.FirstOrDefault().SubscriberId,
                        FormAnswers = j.Where(k => k.FormSubmissionId == j.Key).Select(l => new InnfyFormAnswer
                        {
                            FormSubmissionId = l.FormSubmissionId,
                            FormAnswerElementType = l.FormAnswerElementType,
                            FormAnswerValue = l.FormAnswerValue,
                            FormAnswerId = l.FormAnswerId,
                            FormQuestionId = l.FormQuestionId
                        }).ToList()
                    });

                //var insertedAnswer = Enumerable.Empty<FormAnswerModel>();

                List<InnfyFormSubmission> insertedAnswer = null;

                if (!string.IsNullOrEmpty(model.LastUpdatedDate))
                {
                    List<string> FormList = model.LastUpdatedDate.Split(',').ToList();
                    insertedAnswer = inserted.Where(i => FormList.Contains(i.FormId.ToString())).ToList();
                }

                //SQRT(POWER(69.1 * (Latitude - @orig_lat), 2) + POWER(69.1 * (@orig_long - Longitude) * COS(Latitude / 57.3), 2)) AS [Distance] 

                DateTime date = Convert.ToDateTime(model.DateTime);
                long id = model.FormSubmissionId;

                var updated = Enumerable.Empty<FormSubmissionList>();
                var updatedAnswer = Enumerable.Empty<FormAnswerModel>();
                if (id > 0)
                {
                    //updated = db.FormSubmissions.Where(f => f.ApprovedOn >= date && f.IsApproved != null)
                    updated = db.FormSubmissions.Where(f => f.IsApproved != null && f.Latitude != null && f.Latitude > 0)
                         .Where(i => Math.Sqrt(Math.Pow(69.1 * (double.Parse(i.LatLong.Split(',')[0]) - orig_lat), 2) + Math.Pow(69.1 * (orig_long - double.Parse(i.LatLong.Split(',')[1])) * Math.Cos(double.Parse(i.LatLong.Split(',')[1]) / 57.3), 2)) < bounding_distance)
                         .OrderBy(o => o.Id)
                         .Select(f => new FormSubmissionList
                         {
                             Id = f.Id,
                             FormId = f.FormId,
                             FolderName = f.FolderName,
                             LatLong = f.LatLong,
                             SubmittedOn = f.SubmittedOn,
                             SubscriberId = f.SubscriberId,
                             FormName = f.Form.Name,
                             Image = f.Form.Image,
                             IsApproved = (int)f.IsApproved,

                         }).ToList();

                    updatedAnswer = updated.Join(db.FormAnswers, fs => fs.Id, fa => fa.FormSubmissionId, (fs, fa) => new FormAnswerModel
                    {
                        Id = fa.Id,
                        Value = fa.Value,
                        ElementType = fa.ElementType,
                        FormQuestionId = fa.FormQuestionId,
                        FormSubmissionId = fa.FormSubmissionId,
                    }).ToList();
                }



                var _lat = decimal.Parse(model.Latitude);
                var _lng = decimal.Parse(model.Longitude);

                //inserted = inserted.AsEnumerable().Where(i => decimal.Parse(i.LatLong.Split(',')[0]) >= _lat && decimal.Parse(i.LatLong.Split(',')[1]) <= _lng).ToList();

                date = Common.GetDateTime(db);
                return new { Inserted = inserted, Updated = updated, CurrentDate = date, InsertedAnswer = insertedAnswer, UpdatedAnswer = updatedAnswer };

            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }


        /// <summary>
        /// Added by jay on 18-2-2016
        /// I-Witness specific
        /// Get form submission by lat long and zoom
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [Route("api/SubmissionListByMe")]
        [HttpPost]
        public object SubmissionListByMe([FromBody] InnfyFormSubmissionByMe model)
        {
            try
            {
                //Check user Authentication
                AppUserInfoService appUserInfoService = new AppUserInfoService();
                if (!appUserInfoService.CheckAppAuthentication(model.AuthKey, model.SubscriberId))
                {
                    return null;
                }

                string DeviceId = model.DeviceId;
                int SubscriberId = model.SubscriberId;
                int IsApproved = model.IsApproved;

                var SubmissionList = db.GetFormSubmissionByMe(DeviceId, SubscriberId, IsApproved).ToList()
                    .GroupBy(i => i.FormSubmissionId)
                    .AsEnumerable()
                    .Select(j => new InnfyFormSubmission
                    {
                        FormSubmissionId = j.Key,
                        FormId = j.FirstOrDefault().FormId,
                        FormName = j.FirstOrDefault().FormName,
                        Latitude = j.FirstOrDefault().Latitude,
                        Longitude = j.FirstOrDefault().Longitude,
                        FolderName = j.FirstOrDefault().FolderName,
                        FormImage = j.FirstOrDefault().FormImage,
                        IsApproved = j.FirstOrDefault().IsApproved,
                        SubmittedOn = j.FirstOrDefault().SubmittedOn,
                        SubscriberId = j.FirstOrDefault().SubscriberId,
                        DeviceId = j.FirstOrDefault().DeviceId,
                        FormAnswers = j.Where(k => k.FormSubmissionId == j.Key).Select(l => new InnfyFormAnswer
                        {
                            FormSubmissionId = l.FormSubmissionId,
                            FormAnswerElementType = l.FormAnswerElementType,
                            FormAnswerValue = l.FormAnswerValue,
                            FormAnswerId = l.FormAnswerId,
                            FormQuestionId = l.FormQuestionId
                        }).ToList()
                    });

                return new { SubmissionList = SubmissionList };

            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }


        /// <summary>
        /// No more in use. I-Witness Specific AcceptEmergencyResponseForm
        /// </summary>
        /// <param name="id">Submission Id</param>
        /// <returns>True/False</returns>
        public bool AcceptEmergencyResponseForm(long id)
        {
            try
            {
                int _id = (int)id;
                FormSubmission formSubmission = db.FormSubmissions.Where(f => f.Id == _id).FirstOrDefault();
                if (formSubmission == null)
                {
                    return false;
                }
                formSubmission.ApprovedOn = Common.GetDateTime(db);
                formSubmission.IsApproved = Helper.Constants.FORM_APPROVED_ID;
                formSubmission.IsProcessing = false;
                db.Entry(formSubmission).State = EntityState.Modified;
                db.SaveChanges();

                NotificationService _notificationService = new NotificationService();
                _notificationService.AppUserNotification(Helper.Constants.FORM_APPROVED_ID, formSubmission.Id, formSubmission.DeviceId, formSubmission.SubscriberId, formSubmission.AppInfoId);

                return true;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return false;
            }
        }

        #endregion

        public class FormViewModel
        {
            public FormViewModel()
            {
                FormAnswers = new List<FormAnswerModel>();
                FormAnswerslist = new List<FormAnswer>();
            }
            public long Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string Image { get; set; }
            public bool IsActive { get; set; }
            public bool IsDeleted { get; set; }
            public long CreatedBy { get; set; }
            public System.DateTime CreatedOn { get; set; }
            public long UpdatedBy { get; set; }
            public System.DateTime UpdatedOn { get; set; }
            public int OperationId { get; set; }
            public int LanguageId { get; set; }
            public string LatLng { get; set; }
            public int SubscriberId { get; set; }
            public long FormId { get; set; }
            public string Severity { get; set; }
            public string ReportedType { get; set; }
            public List<FormAnswerModel> FormAnswers { get; set; }
            public List<FormAnswer> FormAnswerslist { get; set; }
        }

        public class FormSubmissionList
        {
            public long Id { get; set; }
            public string DeviceId { get; set; }
            public string LatLong { get; set; }
            public long SubmittedBy { get; set; }
            public System.DateTime SubmittedOn { get; set; }
            public long ApprovedBy { get; set; }
            public System.DateTime ApprovedOn { get; set; }
            public int IsApproved { get; set; }
            public long EmployeeId { get; set; }
            public long FormId { get; set; }
            public int SubscriberId { get; set; }
            public string FormName { get; set; }
            public string Image { get; set; }
            public System.Guid FolderName { get; set; }
        }
    }
}