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
using FormziApi.Models;
using Newtonsoft.Json;
using FormziApi.Services;

namespace FormziApi.Controllers
{
    [Authorize]
    [EnableCors("*", "*", "*")]
    public class FormQuestionsController : ApiController
    {
        #region Fields
        LogProvider lp = new LogProvider("FormQuestionsController");
        private FormziEntities db = new FormziEntities();
        private FormQuestionService _formQuestionService = new FormQuestionService();
        #endregion

        #region Methods

        // GET: api/FormQuestionByFormId/5/1
        [Route("api/FormQuestionByFormId")]
        [HttpGet]
        public object GetFormQuestionByFormId(long FormId, long AppLoginId, int FormVersionId = 0, string IsEditMode = null)
        {
            try
            {
                long _formId = (long)FormId;
                long _appLoginId = (long)AppLoginId;//Added by Hiren

                if (FormVersionId == 0)
                {
                    if (IsEditMode == "a")
                    {
                        var formVersionIds = db.FormVersions.Where(i => i.FormId == _formId).AsEnumerable().Select(i => i.Id).ToList();

                        var formVersionIdsDashboard = db.Dashboards.Where(f => f.FormId == FormId).AsEnumerable().Select(i => i.FormVersionId).ToList();
                        var exceptVersionIds = formVersionIds.Except(formVersionIdsDashboard);

                        if (exceptVersionIds.Count() > 0)
                            FormVersionId = db.FormVersions.Where(x => exceptVersionIds.Contains(x.Id)).OrderByDescending(o => o.Id).FirstOrDefault().Id;
                        else
                            return null;
                    }
                    else if (IsEditMode == "e")
                    {
                        FormVersionId = db.Dashboards.Where(i => i.FormId == _formId).OrderByDescending(o => o.FormVersionId).FirstOrDefault().FormVersionId;
                        //db.FormSubmissions.Where(f => f.FormId == FormId && !f.IsDeleted)
                        //            .AsEnumerable()
                        //            .OrderByDescending(x => x.FormVersion.Id)
                        //            .Select(i => i.FormVersion.Id).Distinct().FirstOrDefault();
                    }
                    else
                    {
                        FormVersionId = db.FormVersions.Where(i => i.FormId == _formId).OrderByDescending(o => o.Id).FirstOrDefault().Id;
                    }
                }

                //added by jay
                //FormVersion formVersionModel = db.FormVersions.Where(i => i.FormId == FormId).OrderByDescending(o => o.Id).FirstOrDefault();
                //int FormVersionId = formVersionModel.Id;

                var form = db.Forms.Where(f => f.Id == FormId).Select(m => new FormModel
                {
                    Id = m.Id,
                    Name = m.Name,
                    Image = m.Image,
                    OrderBy = m.OrderBy,
                    IsActive = m.IsActive,//Added by jay on 1-Mar-2016
                    //OperationId = m.OperationId,
                    SubscriberId = m.SubscriberId,
                    LanguageId = m.LanguageId,
                    FormVersionId = FormVersionId,
                    WebFormUID = m.WebFormUID,
                    WorkFlowEnabled = m.WorkFlowEnabled,
                    IsPrivateForm = m.IsPrivateForm == null ? false : m.IsPrivateForm,//Added By Hiren 30-10-2017
                    IsVisibleSection=m.IsVisibleSection//Added By Hiren 27-11-2017
                }).FirstOrDefault();
                int SubscriberId = form.SubscriberId;
                List<FormQuestionsGroupByLanguage> formQue = new List<FormQuestionsGroupByLanguage>();
                List<LanguageModel> formLanguages = new List<LanguageModel>();
                List<LanguageModel> SubscriberLanguage = db.SubscriberLanguages.Where(i => i.SubcriberId == SubscriberId).OrderBy(o => o.DisplayOrder)
                   .Select(i => new LanguageModel
                   {
                       Id = i.Language.Id,
                       Name = i.Language.Name,
                       Rtl = i.Language.Rtl,
                       UniqueSeoCode = i.Language.UniqueSeoCode,
                       BaseLanguage = i.Language.Id == form.LanguageId
                   }).ToList();
                var fmQueList = db.FormQuestions.Where(f => f.FormId == FormId && !f.IsDeleted && f.FormVersionId == FormVersionId).AsEnumerable().ToList();
                foreach (var item in SubscriberLanguage)
                {
                    FormQuestionsGroupByLanguage model = fmQueList.Where(x => x.LanguageId == item.Id).GroupBy(g => g.LanguageId)
                        .AsEnumerable()
                        .Select(j => new FormQuestionsGroupByLanguage
                        {
                            formQuestions = j.Select(x => new CommonFormQuestionsModel { Id = x.Id,Question=x.Question,FormToolsId=x.FormToolsId,JSONQuestion=x.JSONQuestion,ParentQuestionId = x.ParentQuestionId}).ToList(),
                            languageId = item.Id,
                            published = j.Select(x => x.IsPublished).FirstOrDefault()
                        }).FirstOrDefault();

                    if (model != null)
                    {
                        //Added By Hiren
                        if (form.WorkFlowEnabled)
                        {
                            FormQuestionsGroupByLanguage newModel = new FormQuestionsGroupByLanguage();
                            newModel.languageId = model.languageId;
                            newModel.published = model.published;
                            //Changed By Hiren 27-11-2017 
                            List<CommonFormQuestionsModel> cfql = new List<CommonFormQuestionsModel>();
                            List<FormQuestion> fqList = model.formQuestions.ToList().AsEnumerable().Select(i => new FormQuestion {Id = i.Id,Question = i.Question,FormToolsId =i.FormToolsId,JSONQuestion =i.JSONQuestion,ParentQuestionId = i.ParentQuestionId }).ToList();
                            cfql = _formQuestionService.GetRoleBasedFormQuestions(FormId, FormVersionId, AppLoginId, fqList);
                            if (cfql != null && cfql.Count > 0)
                            {
                                model.formQuestions = cfql.ToList();
                            }
                        }
                        //End
                        formQue.Add(model);
                        item.Published = model.published;
                        formLanguages.Add(item);
                    }
                }
                return new { FormQuestions = formQue, Form = form, FormLanguages = formLanguages };
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }

        // POST: api/FormQuestions
        [Route("api/FormQuestions")]
        [HttpPost]
        public void PostFormQuestion([FromBody]FormQuestion formQuestion)
        {
            try
            {
                db.FormQuestions.Add(formQuestion);
                db.SaveChanges();
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
            }
        }

        // GET: api/FormQuestionByWebFormUID/6P1FI540Q0RP8CU6/1
        [AllowAnonymous]
        [Route("api/FormQuestionByWebFormUID/{FormId}/{AppLoginId}")]
        [HttpGet]
        public object FormQuestionByWebFormUID(string FormId, int AppLoginId)
        {
            try
            {
                var form = db.Forms.Where(f => f.WebFormUID == FormId).FirstOrDefault();
                if (form != null && !form.IsActive)
                {
                    return false;
                }

                var fileUrl = form.Subscriber.AppSettings.Where(i => i.Key == Constants.FileUrl).FirstOrDefault().Value;
                var companyLogo = "";
                if (!string.IsNullOrEmpty(form.Subscriber.CompanyLogo))
                {
                    companyLogo = form.Subscriber.CompanyLogo;
                }
                if (form != null)
                {
                    var model = new
                    {
                        FormQuestions = GetFormQuestionByFormId((long)form.Id, (long)AppLoginId),//Added AppLoginId By Hiren 23-10-2017
                        FormName = form.Name,
                        FormImage = form.Image != null ? form.Image : string.Empty,
                        FileUrl = fileUrl,
                        SubscriberCompanyLogoPath = companyLogo != "" ? fileUrl + form.SubscriberId + Constants.ImageFolder + Constants.SubscriberFolder + companyLogo : ""
                    };
                    return model;
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
        #endregion
        public class FormQuestionsGroupByLanguage
        {
            public int languageId { get; set; }
            public bool published { get; set; }
            public List<CommonFormQuestionsModel> formQuestions { get; set; }
        }
    }
}