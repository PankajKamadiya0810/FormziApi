using FormziApi.Database;
using FormziApi.Helper;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;

namespace FormziApi.Services
{
    //Added by Jay Mistry 31st August 2016, Wednesday 11am 
    public class SubmissionService
    {
        #region Fields

        LogProvider lp;
        private FormziEntities db;

        #endregion

        #region Constructor

        public SubmissionService()
        {
            lp = new LogProvider("Formzi");
            db = new FormziEntities();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Method call when i-witness mobile app user will Resolved, Assign, Reject issue
        /// </summary>
        /// <param name="submissionId"></param>
        /// <param name="currentEmpId">UserId</param>
        /// <param name="assignToEmpId"></param>
        /// <param name="actionId">Resolved, Reject, Assign Id</param>
        /// <returns></returns>
        public bool SubmissionAction(long submissionId, long currentEmpId, long assignToEmpId, int actionId)
        {
            try
            {
                if (submissionId > 0 && currentEmpId > 0 && actionId > 0)
                {
                    NotificationService notificationService = new NotificationService();

                    if (!UpdateEmpSubmissionMap(submissionId, currentEmpId, false)) //Update previous data
                        return false;

                    switch (actionId)
                    {
                        case 2: //Reject / Close
                            UpdateSubmissionAction(submissionId, Constants.SUBMISSION_REJECT);
                            break;
                        case 3: //Resolve employee-submission mapping
                            UpdateSubmissionAction(submissionId, Constants.SUBMISSION_RESOLVE);
                            break;
                        case 10: // Transfer //Assign to employee
                            if (assignToEmpId <= 0)
                                return false;

                            AddEmpSubmissionMap(submissionId, assignToEmpId, true);

                            //Update form submission data
                            FormSubmission formSubModel = db.FormSubmissions.Where(i => i.Id == submissionId).FirstOrDefault();
                            formSubModel.Action = Constants.SUBMISSION_APPROVED;
                            formSubModel.IsApproved = Constants.SUBMISSION_APPROVED;
                            formSubModel.ApprovedOn = Common.GetDateTime(db);
                            formSubModel.UpdatedOn = Common.GetDateTime(db);
                            db.Entry(formSubModel).State = EntityState.Modified;
                            db.SaveChanges();

                            //Notification Mail
                            notificationService.AppUserNotification(Constants.NOTIFICATION_ASSIGN_TO_EMPLOYEE, formSubModel.Id, formSubModel.DeviceId, formSubModel.SubscriberId, formSubModel.AppInfoId);

                            break;
                        default:
                            break;
                    }
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                throw e;
            }
        }

        /// <summary>
        /// Submission entry updated here
        /// Possible updates, Resolved, Reject, Assign issue
        /// </summary>
        /// <param name="submissionId"></param>
        /// <param name="actionId"></param>
        /// <returns></returns>
        public bool UpdateSubmissionAction(long submissionId, int actionId)
        {
            try
            {
                if (submissionId > 0 && actionId > 0)
                {
                    NotificationService notificationService = new NotificationService();

                    FormSubmission submissionModel = db.FormSubmissions.Where(i => i.Id == submissionId).FirstOrDefault();

                    submissionModel.IsApproved = actionId;
                    submissionModel.Action = actionId;
                    submissionModel.UpdatedOn = Common.GetDateTime(db);

                    if (actionId == Constants.SUBMISSION_APPROVED)
                    {
                        submissionModel.IsApproved = Constants.SUBMISSION_APPROVED;
                        submissionModel.ApprovedOn = Common.GetDateTime(db);
                    }
                    db.Entry(submissionModel).State = EntityState.Modified;
                    db.SaveChanges();

                    //Notification Mail
                    notificationService.AppUserNotification(actionId, submissionModel.Id, submissionModel.DeviceId, submissionModel.SubscriberId, submissionModel.AppInfoId);

                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                throw e;
            }
        }

        /// <summary>
        /// Add Employee-Submission Mapping
        /// </summary>
        /// <param name="submissionId"></param>
        /// <param name="employeeId"></param>
        /// <param name="assigned"></param>
        /// <returns></returns>
        public bool AddEmpSubmissionMap(long submissionId, long employeeId, bool assigned)
        {
            try
            {
                if (submissionId > 0 && employeeId > 0)
                {
                    SubmissionEmployeeMap submissionEmpModel = new SubmissionEmployeeMap();
                    submissionEmpModel.CreatedOn = Common.GetDateTime(db);
                    submissionEmpModel.Assigned = assigned;
                    submissionEmpModel.EmployeeId = employeeId;
                    submissionEmpModel.SubmissionId = submissionId;

                    db.SubmissionEmployeeMaps.Add(submissionEmpModel);
                    db.SaveChanges();

                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                throw e;
            }
        }

        /// <summary>
        /// Update Employee-Submission Mapping
        /// </summary>
        /// <param name="submissionId"></param>
        /// <param name="employeeId"></param>
        /// <param name="assigned"></param>
        /// <returns></returns>
        public bool UpdateEmpSubmissionMap(long submissionId, long employeeId, bool assigned)
        {
            try
            {
                if (submissionId > 0 && employeeId > 0)
                {
                    SubmissionEmployeeMap model = db.SubmissionEmployeeMaps.Where(i => i.SubmissionId == submissionId && i.EmployeeId == employeeId)
                        .OrderByDescending(o => o.Id)
                        .FirstOrDefault();

                    if (model == null)
                        return false;

                    model.Assigned = assigned;
                    model.UpdatedOn = Common.GetDateTime(db);
                    db.Entry(model).State = EntityState.Modified;
                    db.SaveChanges();

                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                throw e;
            }
        }

        /// <summary>
        /// Remove Employee-Submission Mapping
        /// </summary>
        /// <param name="submissionId"></param>
        /// <param name="employeeId"></param>
        /// <param name="assigned"></param>
        /// <returns></returns>
        public bool RemoveEmpSubmissionMap(long submissionId)
        {
            try
            {
                if (submissionId > 0)
                {
                    SubmissionEmployeeMap model = db.SubmissionEmployeeMaps.Where(i => i.SubmissionId == submissionId)
                        .OrderByDescending(o => o.Id)
                        .FirstOrDefault();

                    if (model == null)
                        return false;

                    model.Assigned = false;
                    model.UpdatedOn = Common.GetDateTime(db);
                    db.Entry(model).State = EntityState.Modified;
                    db.SaveChanges();

                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                throw e;
            }
        }

        /// <summary>
        /// AMC Post to ccrs system
        /// </summary>
        /// <param name="formId"></param>
        /// <param name="formSubmissionId"></param>
        /// <returns></returns>
        public bool AMC_PostToCCRS(int formId, long formSubmissionId)
        {
            try
            {
                return false;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                //throw e;
                return false;
            }

        }

        public bool SendSubmissionMail(long subscriberId, string ToEmail)
        {
            try
            {
                int languageId = 1;
                string subscriberLogo = "";
                var logoFileUrl = "";
                string emailSignature = "";
                subscriberId = subscriberId == 0 ? 1 : subscriberId;
                string strTemplate = "";
                string email = "";

                Subscriber subscriberdbModel = db.Subscribers.Where(i => i.Id == subscriberId).FirstOrDefault();

                SubscriberLanguage subscriberModel = subscriberdbModel.SubscriberLanguages.OrderBy(o => o.DisplayOrder).FirstOrDefault();
                logoFileUrl = subscriberdbModel.AppSettings.Where(i => i.Key == Constants.FileUrl).FirstOrDefault().Value;
                email = ToEmail;
                subscriberLogo = !string.IsNullOrEmpty(subscriberdbModel.CompanyLogo) ? logoFileUrl + subscriberdbModel.Id + Constants.ImageFolder + Constants.SubscriberFolder + subscriberdbModel.CompanyLogo : "";

                if (subscriberModel != null)
                {
                    languageId = subscriberModel.LanguageId;
                }

                AppSetting appSettingModel = subscriberdbModel.AppSettings.Where(i => i.Key == Constants.EmailSignature).FirstOrDefault();

                if (appSettingModel != null)
                {
                    emailSignature = appSettingModel.Value;
                }

                string mailFilePath = "~/MailTemplates/Languages/" + languageId + "/SubmissionMail.html";
                StreamReader fp = new StreamReader(System.Web.HttpContext.Current.Server.MapPath(mailFilePath));
                strTemplate = fp.ReadToEnd();
                fp.Close();

                strTemplate = strTemplate.Replace("@EmailSignature", emailSignature).Replace("@CompanyLogo", subscriberLogo).Replace("@PreferredDomain", !string.IsNullOrEmpty(subscriberModel.Subscriber.PreferredDomain) ? subscriberModel.Subscriber.PreferredDomain : subscriberModel.Subscriber.SubDomain + "." + Constants.FORMZI_DOMAIN).Replace("@CompanyName", subscriberdbModel.CompanyName);


                Helper.General.SendEmail(email, strTemplate, "New submission.");
                return true;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                throw e;
            }
        }

        /// <summary>
        /// Send Submission Section Report Mail (Added By Hiren 10-11-2017)
        /// </summary>
        /// <param name="subscriberId">subscriberId</param>
        /// <param name="FormName">FormName</param>
        /// <param name="EmployeeName">EmployeeName</param>
        /// <param name="ToEmail">ToEmail</param>
        /// <returns>True/False</returns>
        public bool SendSubmissionSectionReportMail(long subscriberId,string FormName,string EmployeeName, string ToEmployeeName,string ToEmail,string Subject,string SubmissionUrl)
        {
            try
            {
                int languageId = 1;
                string subscriberLogo = "";
                var logoFileUrl = "";
                string emailSignature = "";
                subscriberId = subscriberId == 0 ? 1 : subscriberId;
                string strTemplate = "";
                string email = "";
                string submissionUrl = "";//Added By Hiren 24-11-2017

                Subscriber subscriberdbModel = db.Subscribers.Where(i => i.Id == subscriberId).FirstOrDefault();

                SubscriberLanguage subscriberModel = subscriberdbModel.SubscriberLanguages.OrderBy(o => o.DisplayOrder).FirstOrDefault();
                logoFileUrl = subscriberdbModel.AppSettings.Where(i => i.Key == Constants.FileUrl).FirstOrDefault().Value;
                email = ToEmail;
                subscriberLogo = !string.IsNullOrEmpty(subscriberdbModel.CompanyLogo) ? logoFileUrl + subscriberdbModel.Id + Constants.ImageFolder + Constants.SubscriberFolder + subscriberdbModel.CompanyLogo : "";

                if (subscriberModel != null)
                {
                    languageId = subscriberModel.LanguageId;
                    submissionUrl = subscriberdbModel.SubDomain + "." + Constants.FORMZI_DOMAIN + SubmissionUrl;
                }

                AppSetting appSettingModel = subscriberdbModel.AppSettings.Where(i => i.Key == Constants.EmailSignature).FirstOrDefault();

                if (appSettingModel != null)
                {
                    emailSignature = appSettingModel.Value;
                }

                string mailFilePath = "~/MailTemplates/Languages/" + languageId + "/SubmissionReportMail.html";
                StreamReader fp = new StreamReader(System.Web.HttpContext.Current.Server.MapPath(mailFilePath));
                strTemplate = fp.ReadToEnd();
                fp.Close();
                strTemplate = strTemplate.Replace("@EmailSignature",emailSignature).Replace("@FormName",FormName).Replace("@UrlLink",submissionUrl).Replace("@ToEmployeeName",ToEmployeeName).Replace("@EmployeeName",EmployeeName).Replace("@CompanyLogo",subscriberLogo).Replace("@PreferredDomain", !string.IsNullOrEmpty(subscriberModel.Subscriber.PreferredDomain) ? subscriberModel.Subscriber.PreferredDomain : subscriberModel.Subscriber.SubDomain + "." + Constants.FORMZI_DOMAIN).Replace("@CompanyName",subscriberdbModel.CompanyName);
                Helper.General.SendEmail(email,strTemplate,Subject);
                return true;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                throw e;
            }
        }
        #endregion
    }
}