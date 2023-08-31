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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using FormziApi.Helper;
using FormziApi.Extention;
using FormziApi.Models;
using System.IO;
using System.Configuration;
using FormziApi.Services;

namespace FormziApi.Controllers
{
    [Authorize]
    [EnableCors("*", "*", "*")]
    public class FormsController : ApiController
    {
        #region Fields

        LogProvider lp = new LogProvider("FormsController");
        private FormziEntities db = new FormziEntities();

        #endregion

        #region Methods

        /// <summary>
        /// List of forms subscriberId and employee wise
        /// </summary>
        /// <param name="subscriberId">subscriber Id. Ex. 1</param>
        /// <param name="id">employee Id. Ex. 1</param>
        /// <returns>List containing Id, Name, Description, LanguageName, CreatedOn, UpdatedOn, OrderBy, TotalSubmission, WebFormUID, IsActive, IsDashboard</returns>
        // GET: api/Forms
        [Route("api/Forms/{subscriberId}")]
        [HttpGet]
        public object GetForms(int subscriberId)
        {
            try
            {
                //Changed by Hiren 19-11-2017
                List<FormModel> forms = db.Forms.Where(i => i.SubscriberId == subscriberId && !i.IsDeleted).AsEnumerable()
                    .Select(m => new FormModel
                    {
                        Id = m.Id,
                        Name = m.Name,
                        Description = m.Description,
                        LanguageName = m.Language.Name,
                        CreatedOn = m.CreatedOn,
                        UpdatedOn = m.UpdatedOn,
                        OrderBy = m.OrderBy == null ? 0 : m.OrderBy, // Added by Jugnu Pathak on 25-06-2015
                        TotalSubmission = m.FormSubmissions.Where(a => !a.IsDeleted).Count(), // Changed by Pankaj on 28-Aug-2017 (!i=>i.IsDeleted), //Added by Jay
                        WebFormUID = m.WebFormUID,
                        IsActive = m.IsActive,
                        IsDashboard = m.Dashboards.Any(),
                        WorkFlowEnabled = m.WorkFlowEnabled,
                        IsPrivateForm = m.IsPrivateForm == null ? false : m.IsPrivateForm,
                    }).OrderByDescending(a => a.CreatedOn).ToList();
                return forms;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }

        /// <summary>
        /// Get Database date
        /// </summary>
        /// <returns>Date</returns>
        [Route("api/getdate/")]
        [HttpGet]
        public DateTime? getDate()
        {
            try
            {
                return Common.GetDateTime(db);
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }

        /// <summary>
        /// i-witness - List of forms (For i-witness Mobile  app)
        /// It includes latest server date which need to pass on next api call
        /// </summary>
        /// <param name="SubscriberId"></param>
        /// <param name="DateTime"></param>
        /// <param name="AuthKey"></param>
        /// <returns>List containing Id, Name, Description, LanguageName, CreatedOn, UpdatedOn, OrderBy, TotalSubmission, WebFormUID, IsActive, IsDashboard, Date</returns>
        [AllowAnonymous]
        [Route("api/formlist")]
        [HttpPost]
        public List<object> getFormList([FromBody]SubscriberFormsDateModel dateModel)
        {
            try
            {
                //Check user Authentication
                AppUserInfoService appUserInfoService = new AppUserInfoService();
                if (!appUserInfoService.CheckAppAuthentication(dateModel.AuthKey, dateModel.SubscriberId))
                {
                    return null;
                }

                var obj = new List<object>();
                DateTime d = Convert.ToDateTime(dateModel.DateTime);

                //var form = db.Forms.Where(m => m.Operation.SubscriberId == dateModel.SubscriberId && 
                //    m.IsDeleted == false && m.CreatedOn >= d || m.UpdatedOn >= d)
                //    .OrderBy(a => a.CreatedOn).FirstOrDefault();

                var form = db.Forms.Where(m => m.SubscriberId == dateModel.SubscriberId &&
                    m.IsDeleted == false && m.IsActive == false && m.CreatedOn >= d || m.UpdatedOn >= d)
                    .OrderBy(a => a.CreatedOn).FirstOrDefault();

                if (form != null)
                {
                    obj.Add(GetFormsBySubscriberId(dateModel.SubscriberId)); // GetFormsBySubscriberId --> currently implemented according to INNFY
                    obj.Add(getDate());
                }
                else
                {
                    obj.Add(getDate());
                }
                return obj;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }

        /// <summary>
        /// List of forms by subscriber id which includes form questions list
        /// Currently implemented according to i-witness
        /// </summary>
        /// <param name="SubscriberId">subscriber id. Ex. 1</param>
        /// <returns>Form Model or null</returns>
        [Route("api/FormBySubscriberId/{SubscriberId}")]
        [HttpGet]
        public object GetFormsBySubscriberId(int SubscriberId)
        {
            try
            {
                var form = db.Forms.Where(m => m.IsDeleted == false && m.IsActive && m.SubscriberId == SubscriberId)
                    .AsEnumerable()
                    .Select(i => new
                    {
                        Id = i.Id,
                        Name = i.Name,
                        Description = i.Description,
                        CreatedBy = i.CreatedBy,
                        CreatedOn = i.CreatedOn,
                        Image = i.Image,
                        IsActive = i.IsActive,
                        IsDeleted = i.IsDeleted,
                        LanguageId = i.LanguageId,
                        //OperationId = i.OperationId,
                        UpdatedBy = i.UpdatedBy,
                        UpdatedOn = i.UpdatedOn,
                        OrderBy = i.OrderBy == 0 ? int.MaxValue : i.OrderBy,
                        IsPrivateForm = i.IsPrivateForm == null ? false : i.IsPrivateForm,//Added By Hiren 30-10-2017
                        VersionId = (int)i.FormVersions.OrderByDescending(o => o.Id).FirstOrDefault().Id,
                        FormQuestions = i.FormQuestions.Where(q => q.FormVersionId == (int)i.FormVersions.OrderByDescending(o => o.Id).FirstOrDefault().Id)
                        .Select(j => new FormQuestion
                        {
                            Id = j.Id,
                            JSONQuestion = j.JSONQuestion,
                            IsDeleted = j.IsDeleted,
                            FormToolsId = j.FormToolsId,
                            FormId = j.FormId
                        })
                        .ToList()
                    }).OrderBy(m => m.OrderBy).ToList();

                return form;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }

        /// <summary>
        /// Delete form by id and employee id.
        /// </summary>
        /// <param name="id">form id</param>
        /// <param name="employeeId">employee id</param>
        /// <returns>List of forms</returns>
        [Route("api/deleteForm/")]
        [HttpDelete]
        public object DeleteForm(long id, long employeeId)
        {
            try
            {
                Form form = db.Forms.Find(id);
                if (form == null)
                {
                    return NotFound();
                }
                form.IsDeleted = true;
                form.IsActive = false;
                form.UpdatedBy = employeeId;
                form.UpdatedOn = Common.GetDateTime(db);
                db.Entry(form).State = EntityState.Modified;
                db.SaveChanges();
                //Added by jay mistry 28-7-2016
                List<EmployeeForm> empformList = db.EmployeeForms.Where(i => i.FormId == id).ToList();
                foreach (var item in empformList)
                {
                    item.IsDeleted = true;
                    db.Entry(item).State = EntityState.Modified;
                    db.SaveChanges();
                }
                return GetForms(form.Employee.SubscriberId);
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }

        /// <summary>
        /// Save form
        /// </summary>
        /// <param name="subscriberId">subscriber id.</param>
        /// <param name="dataModel">Form and FormQuestions model</param>
        /// <returns>Return True/False</returns>
        [Route("api/saveform/{subscriberId}")]
        [HttpPost]
        public bool SaveForm(int subscriberId, object dataModel)
        {
            try
            {
                int lastFormVersion = 0, formVersionId = 0, submissions = 0;
                DateTime dt = Common.GetDateTime(db);
                List<FormQuestion> questions = new List<FormQuestion>();

                //Added By Hiren 16-10-2017 (this for Remove From Section Role & Form Section if Submission not exist than remove it.)
                List<FormSection> formSections = new List<FormSection>();
                List<FormSectionRole> formSectionRoles = new List<FormSectionRole>();

                FormVersion formVersion = new FormVersion();
                SaveFormModel formModel = JsonConvert.DeserializeObject<SaveFormModel>(dataModel.ToString());
                bool isNewForm = (formModel.Form.Id == 0) ? true : false;
                if (!isNewForm)
                {
                    formVersion = db.FormVersions.Where(i => i.FormId == formModel.Form.Id).OrderByDescending(o => o.Id).FirstOrDefault();
                    formVersionId = formVersion.Id;
                    lastFormVersion = formVersion.Version;
                    submissions = db.FormSubmissions.Where(s => s.FormVersionId == formVersionId).Count();
                }
                if (isNewForm)
                {
                    var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                    var random = new Random();
                    var UniqueKey = new string(Enumerable.Repeat(chars, 16).Select(s => s[random.Next(s.Length)]).ToArray());
                    formModel.Form.OrderBy = 0;
                    formModel.Form.WebFormUID = UniqueKey;
                    formModel.Form.CreatedOn = dt;
                    formModel.Form.UpdatedOn = dt;
                    db.Forms.Add(formModel.Form);
                    db.SaveChanges();
                }
                else
                {
                    Form formObject = db.Forms.Where(o => o.Id == formModel.Form.Id).FirstOrDefault();
                    formObject.Image = formModel.Form.Image;
                    formObject.Name = formModel.Form.Name;
                    formObject.Description = formModel.Form.Description;
                    formObject.UpdatedOn = dt;
                    formObject.UpdatedBy = formModel.Form.UpdatedBy;
                    formObject.WorkFlowEnabled = formModel.Form.WorkFlowEnabled;//Added By Hiren 18-10-2017
                    formObject.IsPrivateForm = formModel.Form.IsPrivateForm == null ? false : formModel.Form.IsPrivateForm;//Added By Hiren 30-10-2017
                    formObject.IsVisibleSection = formModel.Form.IsVisibleSection;//Added By Hiren 27-11-2017

                    db.Entry(formObject).State = EntityState.Modified;
                    db.SaveChanges();

                    //Dashboard dashboardModel = db.Dashboards.Where(i => i.FormId == formModel.Form.Id).FirstOrDefault();
                    //if (dashboardModel != null)
                    //{
                    //    db.Dashboards.Remove(dashboardModel);
                    //    db.SaveChanges();
                    //}
                }
                if (submissions > 0 || isNewForm)
                {
                    FormVersion formVersionModel = new FormVersion();
                    formVersionModel.FormId = formModel.Form.Id;
                    formVersionModel.Version = lastFormVersion + 1;
                    formVersionModel.CreatedBy = formModel.Form.CreatedBy;
                    formVersionModel.CreatedOn = dt;
                    db.FormVersions.Add(formVersionModel);
                    db.SaveChanges();
                    formVersionId = formVersionModel.Id;
                }
                else
                {
                    //Form Sections
                    formSections = db.FormSections.Where(q => q.VersionId == formVersionId).OrderBy(i => i.Id).ToList();
                    if (formSections != null && formSections.Count > 0)
                    {
                        foreach (FormSection section in formSections)
                        {
                            //Remove FromSectionRoles by FormSectionId(Added By Hiren 16-10-2017)
                            formSectionRoles = db.FormSectionRoles.Where(q => q.FormSectionId == section.Id).OrderBy(i => i.Id).ToList();
                            if (formSectionRoles.Count > 0)
                            {
                                db.FormSectionRoles.RemoveRange(formSectionRoles);
                                db.SaveChanges();
                            }
                        }
                        //Remove FormSections by VersionId(Added By Hiren 16-10-2017)
                        db.FormSections.RemoveRange(formSections);
                        db.SaveChanges();
                    }
                    //Remove FormQuestions by FormVersionId 
                    questions = db.FormQuestions.Where(q => q.FormVersionId == formVersionId).OrderBy(i => i.Id).ToList();
                    if (questions.Count > 0)
                    {
                        db.FormQuestions.RemoveRange(questions);
                        db.SaveChanges();
                    }
                }
                foreach (FormQuestionsByLanguage item in formModel.FormQuestions)
                {
                    var que = JsonConvert.DeserializeObject<List<object>>(item.formQuestions);
                    long? ParentQuestionId = null;//Added by hiren 16-10-2017 (this is for section control)

                    foreach (object obj in que)
                    {
                        FormQuestion objFormQuestion = new FormQuestion();
                        objFormQuestion.FormId = formModel.Form.Id;
                        objFormQuestion.FormToolsId = 1;
                        if (ParentQuestionId == null)
                            ParentQuestionId = null;

                        if (obj != null && !string.IsNullOrEmpty(obj.ToString()))
                        {
                            objFormQuestion.Question = JsonConvert.DeserializeObject<FormComponent>(obj.ToString()) != null ? JsonConvert.DeserializeObject<FormComponent>(obj.ToString()).label : string.Empty;

                            dynamic question = JsonConvert.DeserializeObject<object>(obj.ToString());

                            if (Convert.ToString(question["component"].Value) == Constants.CaptureImage) objFormQuestion.FormToolsId = Convert.ToInt16(Constants.ElementType.CaptureImage);
                            else if (Convert.ToString(question["component"].Value) == Constants.ELEMENT_TYPE_TEXT) objFormQuestion.FormToolsId = Convert.ToInt16(Constants.ElementType.Text);
                            else if (Convert.ToString(question["component"].Value) == Constants.ELEMENT_TYPE_BARCODE_QRCODE) objFormQuestion.FormToolsId = Convert.ToInt16(Constants.ElementType.Barcode);
                            //else if (Convert.ToString(question["component"].Value) == Constants.ELEMENT_TYPE_FILEUPLOAD) objFormQuestion.FormToolsId = Convert.ToInt16(Constants.ElementType.CaptureImage);
                            //else if (Convert.ToString(question["component"].Value) == Constants.ELEMENT_TYPE_FILEUPLOAD) objFormQuestion.FormToolsId = Convert.ToInt16(Constants.ElementType.CaptureVideo);
                            else if (Convert.ToString(question["component"].Value) == Constants.ELEMENT_TYPE_CHECKBOX) objFormQuestion.FormToolsId = Convert.ToInt16(Constants.ElementType.Checkbox);
                            else if (Convert.ToString(question["component"].Value) == Constants.ELEMENT_TYPE_DATE) objFormQuestion.FormToolsId = Convert.ToInt16(Constants.ElementType.Date);
                            else if (Convert.ToString(question["component"].Value) == Constants.ELEMENT_TYPE_EMAIL) objFormQuestion.FormToolsId = Convert.ToInt16(Constants.ElementType.Email);
                            //else if (Convert.ToString(question["component"].Value) == Constants.ELEMENT_TYPE_FILEUPLOAD) objFormQuestion.FormToolsId = Convert.ToInt16(Constants.ElementType.FileUpload);
                            else if (Convert.ToString(question["component"].Value) == Constants.ELEMENT_TYPE_FORMHEADER) objFormQuestion.FormToolsId = Convert.ToInt16(Constants.ElementType.Formheader);
                            else if (Convert.ToString(question["component"].Value) == Constants.ELEMENT_TYPE_GEOLOCATION) objFormQuestion.FormToolsId = Convert.ToInt16(Constants.ElementType.GeoLocation);
                            else if (Convert.ToString(question["component"].Value) == Constants.ELEMENT_TYPE_LABEL) objFormQuestion.FormToolsId = Convert.ToInt16(Constants.ElementType.Label);
                            else if (Convert.ToString(question["component"].Value) == Constants.ELEMENT_TYPE_MEASUREMENT) objFormQuestion.FormToolsId = Convert.ToInt16(Constants.ElementType.Measurment);
                            else if (Convert.ToString(question["component"].Value) == Constants.ELEMENT_TYPE_NAME) objFormQuestion.FormToolsId = Convert.ToInt16(Constants.ElementType.Name);
                            else if (Convert.ToString(question["component"].Value) == Constants.ELEMENT_TYPE_NUMBER) objFormQuestion.FormToolsId = Convert.ToInt16(Constants.ElementType.Number);
                            else if (Convert.ToString(question["component"].Value) == Constants.ELEMENT_TYPE_PAGE) objFormQuestion.FormToolsId = Convert.ToInt16(Constants.ElementType.Page);
                            else if (Convert.ToString(question["component"].Value) == Constants.ELEMENT_TYPE_PHONE) objFormQuestion.FormToolsId = Convert.ToInt16(Constants.ElementType.Phone);
                            else if (Convert.ToString(question["component"].Value) == Constants.ELEMENT_TYPE_PRICE) objFormQuestion.FormToolsId = Convert.ToInt16(Constants.ElementType.Price);
                            else if (Convert.ToString(question["component"].Value) == Constants.ELEMENT_TYPE_RADIO) objFormQuestion.FormToolsId = Convert.ToInt16(Constants.ElementType.Radio);
                            else if (Convert.ToString(question["component"].Value) == Constants.ELEMENT_TYPE_SECTION) objFormQuestion.FormToolsId = Convert.ToInt16(Constants.ElementType.Section);
                            else if (Convert.ToString(question["component"].Value) == Constants.ELEMENT_TYPE_SELECT) objFormQuestion.FormToolsId = Convert.ToInt16(Constants.ElementType.Select);
                            else if (Convert.ToString(question["component"].Value) == Constants.ELEMENT_TYPE_SIGNATURE) objFormQuestion.FormToolsId = Convert.ToInt16(Constants.ElementType.Signature);
                            else if (Convert.ToString(question["component"].Value) == Constants.ELEMENT_TYPE_SUMMARY) objFormQuestion.FormToolsId = Convert.ToInt16(Constants.ElementType.Summary);
                            else if (Convert.ToString(question["component"].Value) == Constants.ELEMENT_TYPE_TEXTAREA) objFormQuestion.FormToolsId = Convert.ToInt16(Constants.ElementType.TextArea);
                            else if (Convert.ToString(question["component"].Value) == Constants.ELEMENT_TYPE_TIME) objFormQuestion.FormToolsId = Convert.ToInt16(Constants.ElementType.Time);
                            else if (Convert.ToString(question["component"].Value) == Constants.ELEMENT_TYPE_TOGGLE) objFormQuestion.FormToolsId = Convert.ToInt16(Constants.ElementType.Toggle);
                            else if (Convert.ToString(question["component"].Value) == Constants.ELEMENT_TYPE_URL) objFormQuestion.FormToolsId = Convert.ToInt16(Constants.ElementType.Url);
                            else if (Convert.ToString(question["component"].Value) == Constants.ELEMENT_TYPE_HAPPINESS) objFormQuestion.FormToolsId = Convert.ToInt16(Constants.ElementType.Happiness);
                            else if (Convert.ToString(question["component"].Value) == Constants.ELEMENT_TYPE_ADDRESS) objFormQuestion.FormToolsId = Convert.ToInt16(Constants.ElementType.Address);
                            else objFormQuestion.FormToolsId = 0;
                            if (Convert.ToString(question["component"].Value) == Constants.ELEMENT_TYPE_SECTION)
                                ParentQuestionId = null;
                        }
                        else
                        {
                            objFormQuestion.Question = string.Empty;
                        }

                        objFormQuestion.JSONQuestion = obj.ToString();
                        objFormQuestion.FormVersionId = formVersionId;
                        objFormQuestion.LanguageId = int.Parse(item.languageId);
                        objFormQuestion.IsPublished = item.published;
                        objFormQuestion.IsDeleted = false;
                        objFormQuestion.ParentQuestionId = ParentQuestionId;

                        db.FormQuestions.Add(objFormQuestion);
                        db.SaveChanges();

                        if (formModel.Form.WorkFlowEnabled)
                        {
                            FormComponent question = JsonConvert.DeserializeObject<FormComponent>(objFormQuestion.JSONQuestion);
                            if (Convert.ToString(question.component) == Constants.ELEMENT_TYPE_SECTION)
                            {
                                ParentQuestionId = objFormQuestion.Id; //This is Use For Section Control Question (Hiren 16-10-2017)

                                FormSection frmSection = new FormSection();
                                frmSection.FormId = formModel.Form.Id;
                                frmSection.VersionId = formVersionId;
                                frmSection.FormQueId = objFormQuestion.Id;
                                frmSection.IsDeleted = false;
                                db.FormSections.Add(frmSection);
                                db.SaveChanges();

                                if (question.config.roles.Count > 0)
                                {
                                    foreach (var role in question.config.roles)
                                    {
                                        if (role.IsAssigned && question.config.isPrivate)
                                        {
                                            FormSectionRole frmSecRoleModel = new FormSectionRole();
                                            frmSecRoleModel.FormSectionId = frmSection.Id;
                                            frmSecRoleModel.RoleId = role.Id;
                                            frmSecRoleModel.IsDeleted = false;
                                            db.FormSectionRoles.Add(frmSecRoleModel);
                                            db.SaveChanges();
                                        }
                                    }
                                }
                            }
                        }
                    }
                    db.SaveChanges();
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

        /// <summary>
        /// Save Form and its questions
        /// </summary>
        /// <param name="form">Form and FormQuestion model</param>
        /// <returns>returns form id</returns>
        [Route("api/Forms")]
        [HttpPost]
        public Int64 PostForm([FromBody]TempFormModel form)
        {
            try
            {
                if (form.objForm.Id > 0)
                {
                    DeleteForm(form.objForm.Id, form.objForm.CreatedBy);
                }
                form.objForm.CreatedOn = Common.GetDateTime(db);
                form.objForm.UpdatedOn = Common.GetDateTime(db);
                db.Forms.Add(form.objForm);
                db.SaveChanges();
                var model = JsonConvert.DeserializeObject<List<object>>(form.strFormQuestion);
                foreach (object obj in model)
                {
                    FormQuestion objFormQuestion = new FormQuestion();
                    objFormQuestion.FormId = form.objForm.Id;
                    objFormQuestion.FormToolsId = 1;
                    objFormQuestion.JSONQuestion = obj.ToString();
                    db.FormQuestions.Add(objFormQuestion);
                    db.SaveChanges();
                }
                return form.objForm.Id;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return 0;
            }
        }

        /// <summary>
        /// Update form order
        /// </summary>
        /// <param name="FormId">Form Id</param>
        /// <param name="Orderby">Orderby</param>
        /// <returns>returns form id</returns>
        // Add by Jugnu Pathak on 25-06-2015
        [Route("api/UpdateFromOrderBy/{FormId}/{Orderby}")]
        [HttpPost]
        public int UpdateFromOrderBy(int FormId, int Orderby)
        {
            try
            {
                var form = db.Forms.Where(f => f.Id == FormId).FirstOrDefault();
                if (form != null)
                {
                    form.OrderBy = Orderby;
                    form.UpdatedOn = Common.GetDateTime(db);
                    db.Entry(form).State = EntityState.Modified;
                    return db.SaveChanges();
                }
                else
                    return 0;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return 0;
            }
        }

        /// <summary>
        /// Update form active
        /// </summary>
        /// <param name="FormId">Form Id</param>
        /// <param name="Active">Active</param>
        /// <returns>returns form id</returns>
        // Add by Pankaj Kamadiya on 02-09-2017
        [Route("api/UpdateFromActive/{FormId}/{Active}")]
        [HttpPost]
        public int UpdateFromActive(int FormId, bool Active)
        {
            try
            {
                var form = db.Forms.Where(f => f.Id == FormId).FirstOrDefault();
                if (form != null)
                {
                    form.IsActive = Active;
                    form.UpdatedOn = Common.GetDateTime(db);
                    db.Entry(form).State = EntityState.Modified;
                    return db.SaveChanges();
                    //return GetForms(form.Employee.SubscriberId);
                }
                else
                    return 0;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return 0;
            }
        }

        /// <summary>
        /// Form details by form id
        /// </summary>
        /// <param name="id">form id</param>
        /// <returns>Form details</returns>
        //Added by Jay
        [Route("api/FormByFormId/{id}")]
        [HttpGet]
        public object GetFormByFormId(long id)
        {
            try
            {
                var form = db.Forms.Where(m => m.Id == id && m.IsDeleted == false).FirstOrDefault();
                Form model = new Form();
                model.Id = form.Id;
                model.Name = form.Name;
                model.Description = form.Description;
                model.Image = form.Image;
                model.IsActive = form.IsActive;
                model.IsDeleted = form.IsDeleted;
                model.CreatedBy = form.CreatedBy;
                model.UpdatedBy = form.UpdatedBy;
                model.LanguageId = form.LanguageId;
                model.WebFormUID = form.WebFormUID;
                model.IsPrivateForm = form.IsPrivateForm == null ? false : form.IsPrivateForm;//Added By Hiren 30-10-2017
                return model;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }

        /// <summary>
        /// Form details by Unique Id
        /// </summary>
        /// <param name="id">Unique Id</param>
        /// <returns>Form details</returns>
        //Added by Jay
        [Route("api/FormByFormUID/{id}")]
        [HttpGet]
        public object GetFormByFormUID(string id)
        {
            try
            {
                long _id = long.Parse(id);
                var form = db.Forms.Where(m => m.Id == _id && m.IsDeleted == false).FirstOrDefault();
                Form model = new Form();
                model.Id = form.Id;
                model.Name = form.Name;
                model.Description = form.Description;
                model.Image = form.Image;
                model.IsActive = form.IsActive;
                model.IsDeleted = form.IsDeleted;
                model.CreatedBy = form.CreatedBy;
                model.UpdatedBy = form.UpdatedBy;
                model.LanguageId = form.LanguageId;
                model.WebFormUID = form.WebFormUID;//Added By Hiren 28-11-2017
                model.IsPrivateForm = form.IsPrivateForm == null ? false : form.IsPrivateForm;//Added By Hiren 30-10-2017
                return model;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }

        /// <summary>
        /// Sync forms by datetime
        /// </summary>
        /// <param name="dateModel">EmployeeId and datetime</param>
        /// <returns></returns>
        [Route("api/SyncEmployeeForms")]
        [HttpPost]
        public List<object> SyncEmployeeForms([FromBody]EmployeeFormsDateModel dateModel)
        {
            try
            {
                var obj = new List<object>();
                DateTime d = Convert.ToDateTime(dateModel.DateTime);
                //only employee's assign all forms
                var employeeForms = db.EmployeeForms.AsEnumerable().Where(m => m.EmployeeId == dateModel.EmployeeId)
                        .Select(i => new EmployeeFormModel
                        {
                            Id = i.Id,
                            EmployeeId = i.EmployeeId,
                            ClientId = i.ClientId,
                            ProjectId = i.ProjectId,
                            FormId = i.FormId,
                            IsDeleted = i.IsDeleted,
                            CreatedBy = i.CreatedBy,
                            CreatedOn = i.CreatedOn,
                            UpdatedBy = i.UpdatedBy,
                            UpdatedOn = i.UpdatedOn
                        });
                var formIdList = employeeForms.Select(i => i.FormId).ToList();
                // Check for form updated 
                var updatedForms = db.Forms.AsEnumerable().Where(i => formIdList.Contains(i.Id) && (i.CreatedOn >= d || i.UpdatedOn >= d));
                // if updated found then return forms
                if (updatedForms.Any())
                {
                    List<FormModel> forms = updatedForms.OrderBy(o => o.CreatedOn)
                        .Select(i => new FormModel
                        {
                            Id = i.Id,
                            Name = i.Name,
                            Description = i.Description,
                            Image = i.Image,
                            IsActive = i.IsActive,
                            IsDeleted = i.IsDeleted,
                            OrderBy = i.OrderBy,
                            LanguageId = i.LanguageId,
                            WebFormUID = i.WebFormUID,
                            SubscriberId = i.SubscriberId,
                            CreatedOn = i.CreatedOn,
                            CreatedBy = i.CreatedBy,
                            UpdatedOn = i.UpdatedOn,
                            UpdatedBy = i.UpdatedBy
                        }).ToList();

                    //only updated form's id
                    var UpdatedFormIds = forms.Select(i => i.Id).ToList();
                    // latest version of updated forms
                    var formVersions = db.FormVersions.AsEnumerable()
                        .Where(i => UpdatedFormIds.Contains(i.FormId))
                        .Select(i => new FormVersionModel
                        {
                            Id = i.Id,
                            FormId = i.FormId,
                            Version = i.Version,
                            CreatedOn = i.CreatedOn,
                            CreatedBy = i.CreatedBy
                        });

                    var versionList = formVersions.OrderByDescending(o => o.Id)
                        .Select(i => new
                        {
                            FormId = forms.Where(j => j.Id == i.FormId).FirstOrDefault(),
                            Id = i.Id
                        }).ToList()
                        .GroupBy(g => g.FormId)
                        .Select(i => i.FirstOrDefault().Id).ToList();

                    // form question of latest versions
                    var formQuestionsList = db.FormQuestions.AsEnumerable()
                        .Where(i => versionList.Contains(i.FormVersionId))
                        .Select(j => new FormQuestionsModel
                        {
                            Id = j.Id,
                            JSONQuestion = j.JSONQuestion,
                            FormVersionId = j.FormVersionId,
                            IsDeleted = j.IsDeleted,
                            FormToolsId = j.FormToolsId,
                            FormId = j.FormId,
                            LanguageId = j.LanguageId,
                            IsPublished = j.IsPublished
                        }).ToList();
                    List<FormsList> FormList = new List<FormsList>();
                    foreach (var item in forms)
                    {
                        FormList.Add(new FormsList
                        {
                            Form = item,
                            FormQuestions = formQuestionsList.Where(i => i.FormId == item.Id).OrderBy(i => i.Id).ToList()
                        });
                    }
                    EmployeeFormList employeeFormList = new EmployeeFormList
                    {
                        EmployeeForms = employeeForms.Where(i => (i.CreatedOn >= d || i.UpdatedOn >= d)).ToList(),
                        FormVersions = formVersions.ToList(),
                        Forms = FormList
                    };
                    obj.Add(employeeFormList);
                    obj.Add(getDate());
                }
                else
                {
                    obj.Add(getDate());
                }
                return obj;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }

        /// <summary>
        /// Active/Deactive form
        /// </summary>
        /// <param name="subscriberId"></param>
        /// <param name="formId"></param>
        /// <param name="loginUserId"></param>
        /// <param name="isActive"></param>
        /// <returns>Return True/False</returns>
        //Added by jay on 1-Mar-2016
        [Route("api/FormActiveUpdate")]
        [HttpPost]
        public bool FormActiveUpdate(int subscriberId, int formId, int loginUserId, bool isActive)
        {
            try
            {
                var formModel = db.Forms.Where(i => i.Id == formId && i.SubscriberId == subscriberId).FirstOrDefault();
                if (formModel != null)
                {
                    formModel.IsActive = isActive;
                    formModel.UpdatedBy = loginUserId;
                    formModel.UpdatedOn = Common.GetDateTime(db);
                    db.Entry(formModel).State = EntityState.Modified;
                    db.SaveChanges();
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

        /// <summary>
        /// Form languages publish
        /// </summary>
        /// <param name="formId">Form Id</param>
        /// <param name="modelJson">Language Model</param>
        /// <returns>Return True/False</returns>
        [Route("api/formLanguages")]
        [HttpPost]
        public object FormLanguages(int formId, string modelJson)
        {
            try
            {
                List<LanguageModel> languages = JsonConvert.DeserializeObject<List<LanguageModel>>(modelJson);
                if (languages == null)
                {
                    return false;
                }

                List<FormQuestion> formQueList = db.FormQuestions.Where(i => i.FormId == formId).ToList();
                if (formQueList == null)
                {
                    return false;
                }

                foreach (var langItem in languages)
                {
                    List<FormQuestion> formLangQueList = formQueList.Where(i => i.LanguageId == langItem.Id).ToList();

                    foreach (var item in formLangQueList)
                    {
                        item.IsPublished = langItem.Published;
                        db.Entry(item).State = EntityState.Modified;
                    }
                    db.SaveChanges();
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

        /// <summary>
        /// Active Form
        /// </summary>
        /// <param name="id">Id</param>
        /// <param name="isActive">True/False</param>
        /// <param name="employeeId">Employee Id</param>
        /// <returns>Return True/False</returns>
        //Added by jay on 26-May-2016
        [Route("api/ActiveForm/")]
        [HttpPost]
        public object ActiveForm(long id, bool isActive, int employeeId)
        {
            try
            {
                Form form = db.Forms.Find(id);
                if (form == null)
                {
                    return false;
                }
                form.IsActive = isActive;
                form.UpdatedBy = employeeId;
                form.UpdatedOn = Common.GetDateTime(db);
                db.Entry(form).State = EntityState.Modified;
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
        //End

        //Added by jay on 29-July-2017

        /// <summary>
        /// Active form language
        /// </summary>
        /// <param name="formId"></param>
        /// <param name="versionId"></param>
        /// <param name="languageId"></param>
        /// <param name="isActive"></param>
        /// <returns>Return True/False</returns>
        [Route("api/activeFormLanguage")]
        [HttpGet]
        public object ActiveFormLanguage(long formId, int versionId, int languageId, bool isActive)
        {
            try
            {
                if (formId > 0 && versionId > 0 && languageId > 0)
                {
                    List<FormQuestion> formQueList = db.FormQuestions.Where(i => i.FormId == formId && i.FormVersionId == versionId && i.LanguageId == languageId).ToList();
                    formQueList.ForEach(i => i.IsPublished = isActive); db.SaveChanges();
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

        /// <summary>
        /// Get Form VersionsList By FormId    
        /// </summary>
        /// <param name="FormId">FormId</param>
        /// <returns>Form Version List</returns>
        // added by Pankaj
        [Route("api/formversionsListbyformid/{FormId}")]
        [HttpGet]
        public object GetFormVersionsListByFormId(long FormId)
        {
            try
            {
                if (FormId == 0)
                {
                    return null;
                }
                var formVersionIds = db.FormSubmissions.Where(f => f.FormId == FormId && !f.IsDeleted).AsEnumerable().Select(i => i.FormVersion.Id).Distinct().ToList();
                if (formVersionIds.Count > 0)
                {
                    var formVersionList = db.FormVersions.Where(x => formVersionIds.Contains(x.Id))
                    .Select(i => new FormVersionModel
                    {
                        Id = i.Id,
                        FormId = i.FormId,
                        Version = i.Version,
                        CreatedOn = i.CreatedOn,
                        CreatedBy = i.CreatedBy
                    }).OrderByDescending(o => o.Id).ToList();
                    return formVersionList;
                }
                else
                {//Added By Hiren 29-11-2017
                    var formVersionList = db.FormVersions.Where(x => x.FormId == FormId)
                    .Select(i => new FormVersionModel
                    {
                        Id = i.Id,
                        FormId = i.FormId,
                        Version = i.Version,
                        CreatedOn = i.CreatedOn,
                        CreatedBy = i.CreatedBy
                    }).OrderByDescending(o => o.Id).ToList();
                    return formVersionList;//End
                }
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }

        /// <summary>
        /// Update Private Form
        /// </summary>
        /// <param name="FormId">Form Id</param>
        /// <param name="IsPrivateForm">IsPrivateForm</param>
        /// <returns>returns form id</returns>
        // Added by Hiren on 28-10-2017
        [Route("api/UpdatePrivateForm/{FormId}/{IsPrivateForm}")]
        [HttpPost]
        public int UpdatePrivateForm(int FormId, bool IsPrivateForm)
        {
            try
            {
                var form = db.Forms.Where(f => f.Id == FormId).FirstOrDefault();
                if (form != null)
                {
                    form.IsPrivateForm = IsPrivateForm;
                    form.UpdatedOn = Common.GetDateTime(db);
                    db.Entry(form).State = EntityState.Modified;
                    return db.SaveChanges();
                }
                else
                    return 0;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return 0;
            }
        }

        /// <summary>
        /// Check Form Is Private Form Or Not (Added By Hiren 1-11-2017)
        /// </summary>
        /// <param name="id">WebFormUID</param>
        /// <returns>True/False</returns>
        [AllowAnonymous]
        [Route("api/formIsPrivateForm")]
        [HttpGet]
        public object CheckFormIsPrivate(string id)
        {
            try
            {
                var form = db.Forms.Where(f => f.WebFormUID == id).FirstOrDefault();
                Form model = new Form();
                model.Id = form.Id;
                model.Name = form.Name;
                model.Description = form.Description;
                model.Image = form.Image;
                model.IsActive = form.IsActive;
                model.IsDeleted = form.IsDeleted;
                model.CreatedBy = form.CreatedBy;
                model.UpdatedBy = form.UpdatedBy;
                model.LanguageId = form.LanguageId;
                model.IsPrivateForm = form.IsPrivateForm == null ? false : form.IsPrivateForm;
                return model;
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

    public class TempFormModel
    {
        public Form objForm { get; set; }
        public string strFormQuestion { get; set; }
    }

    public class FormQuestionsByLanguage
    {
        public string languageId { get; set; }
        public string formQuestions { get; set; }
        public bool published { get; set; }
        public string FormId { get; set; }
    }

    public class SaveFormModel
    {
        public Form Form { get; set; }
        public List<FormQuestionsByLanguage> FormQuestions { get; set; }
    }
}