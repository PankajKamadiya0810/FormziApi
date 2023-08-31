using FormziApi.Controllers;
using FormziApi.Database;
using FormziApi.Helper;
using FormziApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FormziApi.Services
{
    public class FormQuestionService
    {

        #region Fields
        LogProvider lp;
        private FormziEntities db;
        #endregion

        #region Constructor

        public FormQuestionService()
        {
            lp = new LogProvider("CreosformAPI");
            db = new FormziEntities();
        }

        #endregion

        #region Methods
        /// <summary>
        /// Common Method For Render Role Base Question
        /// </summary>
        /// <param name="formId">formId</param>
        /// <param name="formVersionId">formVersionId</param>
        /// <param name="appLoginId">appLoginId</param>
        /// <param name="fqList">fqList</param>
        /// <returns>List Of Question Based on Role</returns>
        public List<CommonFormQuestionsModel> GetRoleBasedFormQuestions(long formId,long formVersionId,long appLoginId, List<FormQuestion> fqList)
        {
            try
            {
                Form formDetails = db.Forms.Where(f => f.Id == formId).FirstOrDefault();
                List<CommonFormQuestionsModel> formquestionList = new List<CommonFormQuestionsModel>();
                var roleIds = db.EmployeeRoles.Where(q => q.AppLoginId == appLoginId).AsEnumerable().Select(i => i.RoleId).Distinct().ToList();
                long? ParentQuestionId = null;
                bool IsReadOnly = false;
                if (fqList != null && fqList.Count > 0 && formDetails != null)
                {
                    foreach (var obj in fqList)
                    {
                        CommonFormQuestionsModel cfqModel = new CommonFormQuestionsModel();
                        cfqModel.Id = obj.Id;
                        cfqModel.Question = obj.Question;
                        cfqModel.FormToolsId = obj.FormToolsId;
                        cfqModel.ParentQuestionId = obj.ParentQuestionId;
                        cfqModel.JSONQuestion = obj.JSONQuestion;

                        FormQuestion que = new FormQuestion();
                        que = db.FormQuestions.Where(q => q.FormId == formId && q.FormVersionId == formVersionId && q.Id == obj.Id).FirstOrDefault();
                        if (que != null)
                        {
                            if (que.FormToolsId == Convert.ToInt16(Constants.ElementType.Section))
                            {
                                FormSection formSections = new FormSection();
                                formSections = que.FormSections.FirstOrDefault();
                                if (formSections != null && que.FormSections.Count > 0)
                                {
                                    if (formSections.FormSectionRoles.Count > 0)
                                    {
                                        if (roleIds.Count > 0)
                                        {
                                            var rolelist = formSections.FormSectionRoles.Where(x => roleIds.Contains(x.RoleId) && x.FormSectionId == formSections.Id).OrderByDescending(o => o.Id).ToList();
                                            if (rolelist != null && rolelist.Count > 0)
                                            {
                                                ParentQuestionId = que.Id;
                                                cfqModel.IsReadOnly = false;
                                                IsReadOnly = cfqModel.IsReadOnly;
                                                formquestionList.Add(cfqModel);
                                            }
                                            else
                                            {
                                                if (formDetails.IsVisibleSection)
                                                {
                                                    ParentQuestionId = que.Id;
                                                    cfqModel.IsReadOnly = true;
                                                    IsReadOnly = cfqModel.IsReadOnly;
                                                    formquestionList.Add(cfqModel);
                                                }
                                                else
                                                {
                                                    ParentQuestionId = null;
                                                    IsReadOnly = true;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            ParentQuestionId = que.Id;
                                            cfqModel.IsReadOnly = true;
                                            IsReadOnly = cfqModel.IsReadOnly;
                                            formquestionList.Add(cfqModel);
                                        }
                                    }
                                    else
                                    {
                                        ParentQuestionId = que.Id;
                                        cfqModel.IsReadOnly = false;
                                        IsReadOnly = cfqModel.IsReadOnly;
                                        formquestionList.Add(cfqModel);
                                    }
                                }
                            }
                            else
                            {
                                if (que.ParentQuestionId == ParentQuestionId)
                                {
                                    if (IsReadOnly)
                                    {
                                        cfqModel.IsReadOnly = true;
                                        formquestionList.Add(cfqModel);
                                    }
                                    else
                                    {
                                        cfqModel.IsReadOnly = false;
                                        formquestionList.Add(cfqModel);
                                    }
                                }
                            }
                        }
                    }
                }
                return formquestionList;
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