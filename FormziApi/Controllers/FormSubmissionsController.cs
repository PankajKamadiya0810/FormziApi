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
using FormziApi.Extention;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Web.UI.WebControls;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO.Compression;
using System.Web;
using System.Configuration;
using System.Globalization;
using Newtonsoft.Json;
using FormziApi.Services;
using System.Text;
using System.Net.Http.Headers;
using OfficeOpenXml;
using System.Collections;
using System.Text.RegularExpressions;
using System.Web.WebPages.Html;
using System.Diagnostics;
using System.Data.SqlClient;

namespace FormziApi.Controllers
{
    [Authorize]
    [EnableCors("*", "*", "*")]
    public class FormSubmissionsController : ApiController
    {
        #region Fields
        LogProvider lp = new LogProvider("FormSubmissionsController");
        private FormziEntities db = new FormziEntities();
        public readonly FormQuestionsController fc;
        private FormQuestionService _formQuestionService = new FormQuestionService();

        public FormSubmissionsController()
        {
            this.fc = new FormQuestionsController();
        }
        #endregion

        #region Methods

        /// <summary>
        /// Submit Form - Creates folder based on GUID and Save JSON file in it.
        /// MAIN METHOD FOR SUBMITTING FORM ----- 1st method call when form is submitted
        /// Formzi, i-witness specific  
        /// </summary>
        /// <returns>Return path of form submitted.</returns>
        [AllowAnonymous]
        [Route("api/addformsubmission")]
        [HttpPost]
        public async Task<HttpResponseMessage> AddFormSubmission()
        {
            try
            {
                if (!Request.Content.IsMimeMultipartContent())
                {
                    throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                }

                AppSettingsController objapp = new AppSettingsController();
                var root = objapp.GetValueByKey(Constants.FileRoot);
                MultipartFormDataStreamProvider provider = new MultipartFormDataStreamProvider(root);
                var result = await Request.Content.ReadAsMultipartAsync(provider);

                if (result.FormData["AnswerJson"] == null || result.FormData["SubscriberId"] == null)
                {
                    throw new HttpResponseException(HttpStatusCode.BadRequest);
                }

                // Get Guid for name of image and json  
                string fileName = General.UniqueFileName();
                // Create and save json file
                var jsonPath = root + Convert.ToInt64(result.FormData["SubscriberId"]) + Constants.JSONFolder;
                Directory.CreateDirectory(jsonPath);

                System.IO.File.WriteAllText(jsonPath + fileName + Constants.JSON, result.FormData["AnswerJson"]);
                // Added by Rinkle on 6-june-2015 (start)
                var formSubmissionPath = root + Convert.ToInt64(result.FormData["SubscriberId"]) + Constants.FormSubmissionDataFolder + fileName;
                Directory.CreateDirectory(formSubmissionPath + Constants.ImageFolder);
                Directory.CreateDirectory(formSubmissionPath + Constants.VideoFolder);
                Directory.CreateDirectory(formSubmissionPath + Constants.AudioFolder);
                Directory.CreateDirectory(formSubmissionPath + Constants.FilesFolder);//Added By Hiren 23-01-2018
                // end
                //added by jay mistry
                //uploaded images needs to move form temp folder to dynamically created image directory at form submition time.
                var answerJSON = JsonConvert.DeserializeObject<RootObjectForAnswers>(result.FormData["AnswerJson"]);
                foreach (var item in answerJSON.FormAnswers)
                {
                    //this for file upload 
                    if (item.Component == "FILEUPLOAD")
                    {
                        if (!string.IsNullOrEmpty(item.Value))
                        {
                            string path2 = root + Convert.ToInt64(result.FormData["SubscriberId"]) + Constants.ImageFolder + "TempImages/";
                            var itemList = item.Value.Split(',');
                            foreach (var objItem in itemList)
                            {
                                string folder = Constants.FilesFolder;//Added By Hiren 24-01-2018
                                string path = formSubmissionPath + folder;
                                if (File.Exists(path2 + objItem))
                                {
                                    // Move the file.
                                    File.Move(path2 + objItem, path + objItem);
                                }
                            }
                        }
                    }
                    //Added By Hiren 23-01-2018 (Capture Image)
                    if (item.Component == "CAPTUREIMAGE")
                    {
                        if (!string.IsNullOrEmpty(item.Value))
                        {
                            string path2 = root + Convert.ToInt64(result.FormData["SubscriberId"]) + Constants.ImageFolder + "TempImages/";
                            var imageList = item.Value.Split(',');
                            foreach (var objItem in imageList)
                            {
                                string folder = Constants.ImageFolder;//Added By Hiren 24-01-2018
                                string path = formSubmissionPath + folder;
                                if (File.Exists(path2 + objItem))
                                {
                                    // Move the file.
                                    File.Move(path2 + objItem, path + objItem);
                                }
                            }
                        }
                    }

                    //Capture Video
                    if (item.Component == "CAPTUREVIDEO")
                    {
                        if (!string.IsNullOrEmpty(item.Value))
                        {
                            string path2 = root + Convert.ToInt64(result.FormData["SubscriberId"]) + Constants.ImageFolder + "TempImages/";
                            var videoList = item.Value.Split(',');
                            foreach (var objItem in videoList)
                            {
                                string folder = Constants.VideoFolder;//Added By Hiren 24-01-2018
                                string path = formSubmissionPath + folder;
                                if (File.Exists(path2 + objItem))
                                {
                                    // Move the file.
                                    File.Move(path2 + objItem, path + objItem);
                                }
                            }
                        }
                    }

                    //Audio
                    if (item.Component == "AUDIO")
                    {
                        if (!string.IsNullOrEmpty(item.Value))
                        {
                            string path2 = root + Convert.ToInt64(result.FormData["SubscriberId"]) + Constants.ImageFolder + "TempImages/";
                            var audioList = item.Value.Split(',');
                            foreach (var objItem in audioList)
                            {
                                string folder = Constants.AudioFolder;//Added By Hiren 24-01-2018
                                string path = formSubmissionPath + folder;
                                if (File.Exists(path2 + objItem))
                                {
                                    // Move the file.
                                    File.Move(path2 + objItem, path + objItem);
                                }
                            }
                        }
                    }
                    //End
                    if (item.Component == "EMAIL")
                    {
                        if (!string.IsNullOrEmpty(item.Value))
                        {
                            var frmQuestion = db.FormQuestions.Where(x => x.Id == item.FormQuestionId).AsEnumerable().FirstOrDefault();
                            FormComponent question = JsonConvert.DeserializeObject<FormComponent>(frmQuestion.JSONQuestion);
                            if (question.config.sendEmail)
                            {
                                SubmissionService _submissionService = new SubmissionService();
                                _submissionService.SendSubmissionMail(Convert.ToInt64(result.FormData["SubscriberId"]), item.Value);
                            }
                        }
                    }
                }
                //end
                return Request.CreateResponse(HttpStatusCode.OK, formSubmissionPath);
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }

        /// <summary>
        /// Submit Form (new method and basic POST code for jaydeep's mobile app) - Creates folder based on GUID and Save JSON file in it.
        /// MAIN METHOD FOR SUBMITTING FORM ----- 1st method call when form is submitted
        /// Formzi, i-witness specific  
        /// </summary>
        /// <returns>Return path of form submitted.</returns>
        [AllowAnonymous]
        [Route("api/addformsubmissionNew")]
        [HttpPost]
        public async Task<HttpResponseMessage> AddFormSubmissionNew()
        {
            try
            {
                var httpRequest = HttpContext.Current.Request;
                AppSettingsController objapp = new AppSettingsController();
                var root = objapp.GetValueByKey(Constants.FileRoot);
                string formSubmissionPath = string.Empty;
                HttpResponseMessage processCount = await Task.Run<HttpResponseMessage>(() =>
                {
                    if (httpRequest.Params["AnswerJson"] == null || httpRequest.Params["SubscriberId"] == null)
                    {
                        throw new HttpResponseException(HttpStatusCode.BadRequest);
                    }

                    // Get Guid for name of image and json  
                    string fileName = General.UniqueFileName();

                    // Create and save json file
                    var jsonPath = root + Convert.ToInt64(httpRequest.Params["SubscriberId"]) + Constants.JSONFolder;
                    Directory.CreateDirectory(jsonPath);

                    System.IO.File.WriteAllText(jsonPath + fileName + Constants.JSON, httpRequest.Params["AnswerJson"]);
                    // Added by Rinkle on 6-june-2015 (start)
                    formSubmissionPath = root + Convert.ToInt64(httpRequest.Params["SubscriberId"]) + Constants.FormSubmissionDataFolder + fileName;
                    Directory.CreateDirectory(formSubmissionPath + Constants.ImageFolder);
                    Directory.CreateDirectory(formSubmissionPath + Constants.VideoFolder);
                    Directory.CreateDirectory(formSubmissionPath + Constants.AudioFolder);
                    Directory.CreateDirectory(formSubmissionPath + Constants.FilesFolder);//Added By Hiren 23-01-2018
                    // end


                    //added by jay mistry
                    //uploaded images needs to move form temp folder to dynamically created image directory at form submition time.
                    var answerJSON = JsonConvert.DeserializeObject<RootObjectForAnswers>(httpRequest.Params["AnswerJson"]);
                    foreach (var item in answerJSON.FormAnswers)
                    {
                        if (item.Component == "FILEUPLOAD")
                        {
                            if (!string.IsNullOrEmpty(item.Value))
                            {
                                string path = formSubmissionPath + Constants.FilesFolder;//Changed By Hiren 24-01-2018
                                string path2 = root + Convert.ToInt64(httpRequest.Params["SubscriberId"]) + Constants.ImageFolder + "TempImages/";
                                var fileList = item.Value.Split(',');
                                foreach (var f in fileList)
                                {
                                    if (File.Exists(path2 + f))
                                    {
                                        // Move the file.
                                        File.Move(path2 + f, path + f);
                                    }
                                }
                            }
                        }
                        //Added By Hiren 23-01-2018 
                        //Capture Image
                        if (item.Component == "CAPTUREIMAGE")
                        {
                            if (!string.IsNullOrEmpty(item.Value))
                            {
                                string path = formSubmissionPath + Constants.ImageFolder;
                                string path2 = root + Convert.ToInt64(httpRequest.Params["SubscriberId"]) + Constants.ImageFolder + "TempImages/";
                                var imageList = item.Value.Split(',');
                                foreach (var image in imageList)
                                {
                                    if (File.Exists(path2 + image))
                                    {
                                        // Move the file.
                                        File.Move(path2 + image, path + image);
                                    }
                                }
                            }
                        }
                        //Capture Video
                        if (item.Component == "CAPTUREVIDEO")
                        {
                            if (!string.IsNullOrEmpty(item.Value))
                            {
                                string path = formSubmissionPath + Constants.VideoFolder;
                                string path2 = root + Convert.ToInt64(httpRequest.Params["SubscriberId"]) + Constants.ImageFolder + "TempImages/";
                                var videoList = item.Value.Split(',');
                                foreach (var video in videoList)
                                {
                                    if (File.Exists(path2 + video))
                                    {
                                        // Move the file.
                                        File.Move(path2 + video, path + video);
                                    }
                                }
                            }
                        }
                        //Audio
                        if (item.Component == "AUDIO")
                        {
                            if (!string.IsNullOrEmpty(item.Value))
                            {
                                string path = formSubmissionPath + Constants.AudioFolder;
                                string path2 = root + Convert.ToInt64(httpRequest.Params["SubscriberId"]) + Constants.ImageFolder + "TempImages/";
                                var audioList = item.Value.Split(',');
                                foreach (var audio in audioList)
                                {
                                    if (File.Exists(path2 + audio))
                                    {
                                        // Move the file.
                                        File.Move(path2 + audio, path + audio);
                                    }
                                }
                            }
                        }
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, formSubmissionPath);
                });
                return Request.CreateResponse(HttpStatusCode.OK, formSubmissionPath);
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                //return null;
                return Request.CreateResponse(HttpStatusCode.OK, "There is some error.");
            }
        }

        [AllowAnonymous]
        [Route("api/editformsubmission")]
        [HttpPut]
        public async Task<HttpResponseMessage> UpdateFormSubmission()
        {
            try
            {
                if (!Request.Content.IsMimeMultipartContent())
                {
                    throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                }
                AppSettingsController objapp = new AppSettingsController();
                var root = objapp.GetValueByKey(Constants.FileRoot);
                MultipartFormDataStreamProvider provider = new MultipartFormDataStreamProvider(root);
                var result = await Request.Content.ReadAsMultipartAsync(provider);
                if (result.FormData["AnswerJson"] == null || result.FormData["SubscriberId"] == null)
                {
                    throw new HttpResponseException(HttpStatusCode.BadRequest);
                }
                var answerJSON = JsonConvert.DeserializeObject<RootObjectForAnswers>(result.FormData["AnswerJson"]);
                FormSubmission formSubmission = new FormSubmission();
                DateTime currentData = Common.GetDateTime(db);
                Form form = db.Forms.Where(f => f.Id == answerJSON.FormId).FirstOrDefault();//Added By Hiren 06-12-2017
                foreach (var item in answerJSON.FormAnswers)
                {
                    FormAnswer entity = new FormAnswer();
                    entity = db.FormAnswers.Where(i => i.Id == item.Id).FirstOrDefault();
                    formSubmission.Id = item.FormSubmissionId;//Added By Hiren 05-12-2017
                    FormQuestion formQuestion = db.FormQuestions.Find(item.FormQuestionId);//Added By Hiren 13-11-2017
                    dynamic question = JsonConvert.DeserializeObject<object>(formQuestion.JSONQuestion);//Added By Hiren 13-11-2017
                    //Added By Hiren 28-10-2017
                    if (entity == null)
                    {
                        FormAnswer newentity = new FormAnswer();
                        newentity.Value = item.ElementType == 16 && item.ElementType == 17 && item.ElementType == 29 && item.ElementType == 35 ? RemoveFileUrl(item.Value) : item.Value;//Change by Hiren 25-01-2018
                        newentity.CreatedOn = Common.GetDateTime(db);
                        newentity.UpdatedOn = Common.GetDateTime(db);
                        if (Convert.ToString(item.Component) == Constants.ELEMENT_TYPE_CAPTUREIMAGE) newentity.ElementType = Convert.ToInt16(Constants.ElementType.CaptureImage);//Change by Hiren 23-01-2018
                        else if (Convert.ToString(item.Component) == Constants.ELEMENT_TYPE_TEXT) newentity.ElementType = Convert.ToInt16(Constants.ElementType.Text);
                        else if (Convert.ToString(item.Component) == Constants.ELEMENT_TYPE_BARCODE_QRCODE) newentity.ElementType = Convert.ToInt16(Constants.ElementType.Barcode);
                        else if (Convert.ToString(item.Component) == Constants.ELEMENT_TYPE_CHECKBOX) newentity.ElementType = Convert.ToInt16(Constants.ElementType.Checkbox);
                        else if (Convert.ToString(item.Component) == Constants.ELEMENT_TYPE_DATE) newentity.ElementType = Convert.ToInt16(Constants.ElementType.Date);
                        else if (Convert.ToString(item.Component) == Constants.ELEMENT_TYPE_EMAIL) newentity.ElementType = Convert.ToInt16(Constants.ElementType.Email);
                        else if (Convert.ToString(item.Component) == Constants.ELEMENT_TYPE_FORMHEADER) newentity.ElementType = Convert.ToInt16(Constants.ElementType.Formheader);
                        else if (Convert.ToString(item.Component) == Constants.ELEMENT_TYPE_GEOLOCATION) newentity.ElementType = Convert.ToInt16(Constants.ElementType.GeoLocation);
                        else if (Convert.ToString(item.Component) == Constants.ELEMENT_TYPE_LABEL) newentity.ElementType = Convert.ToInt16(Constants.ElementType.Label);
                        else if (Convert.ToString(item.Component) == Constants.ELEMENT_TYPE_MEASUREMENT) newentity.ElementType = Convert.ToInt16(Constants.ElementType.Measurment);
                        else if (Convert.ToString(item.Component) == Constants.ELEMENT_TYPE_NAME) newentity.ElementType = Convert.ToInt16(Constants.ElementType.Name);
                        else if (Convert.ToString(item.Component) == Constants.ELEMENT_TYPE_ADDRESS) newentity.ElementType = Convert.ToInt16(Constants.ElementType.Name);//Added By Hiren 20-11-2017
                        else if (Convert.ToString(item.Component) == Constants.ELEMENT_TYPE_NUMBER) newentity.ElementType = Convert.ToInt16(Constants.ElementType.Number);
                        else if (Convert.ToString(item.Component) == Constants.ELEMENT_TYPE_PAGE) newentity.ElementType = Convert.ToInt16(Constants.ElementType.Page);
                        else if (Convert.ToString(item.Component) == Constants.ELEMENT_TYPE_PHONE) newentity.ElementType = Convert.ToInt16(Constants.ElementType.Phone);
                        else if (Convert.ToString(item.Component) == Constants.ELEMENT_TYPE_PRICE) newentity.ElementType = Convert.ToInt16(Constants.ElementType.Price);
                        else if (Convert.ToString(item.Component) == Constants.ELEMENT_TYPE_RADIO) newentity.ElementType = Convert.ToInt16(Constants.ElementType.Radio);
                        else if (Convert.ToString(item.Component) == Constants.ELEMENT_TYPE_SECTION) newentity.ElementType = Convert.ToInt16(Constants.ElementType.Section);
                        else if (Convert.ToString(item.Component) == Constants.ELEMENT_TYPE_SELECT) newentity.ElementType = Convert.ToInt16(Constants.ElementType.Select);
                        else if (Convert.ToString(item.Component) == Constants.ELEMENT_TYPE_SIGNATURE) newentity.ElementType = Convert.ToInt16(Constants.ElementType.Signature);
                        else if (Convert.ToString(item.Component) == Constants.ELEMENT_TYPE_SUMMARY) newentity.ElementType = Convert.ToInt16(Constants.ElementType.Summary);
                        else if (Convert.ToString(item.Component) == Constants.ELEMENT_TYPE_TEXTAREA) newentity.ElementType = Convert.ToInt16(Constants.ElementType.TextArea);
                        else if (Convert.ToString(item.Component) == Constants.ELEMENT_TYPE_TIME) newentity.ElementType = Convert.ToInt16(Constants.ElementType.Time);
                        else if (Convert.ToString(item.Component) == Constants.ELEMENT_TYPE_TOGGLE) newentity.ElementType = Convert.ToInt16(Constants.ElementType.Toggle);
                        else if (Convert.ToString(item.Component) == Constants.ELEMENT_TYPE_URL) newentity.ElementType = Convert.ToInt16(Constants.ElementType.Url);
                        else if (Convert.ToString(item.Component) == Constants.ELEMENT_TYPE_HAPPINESS) newentity.ElementType = Convert.ToInt16(Constants.ElementType.Happiness);
                        else if (Convert.ToString(item.Component) == Constants.ELEMENT_TYPE_CAPTCHA) newentity.ElementType = Convert.ToInt16(Constants.ElementType.Captcha);//Added By Hiren 29-12-2017
                        else if (Convert.ToString(item.Component) == Constants.ELEMENT_TYPE_CAPTUREVIDEO) newentity.ElementType = Convert.ToInt16(Constants.ElementType.CaptureVideo);//Added By Hiren 23-01-2017
                        else if (Convert.ToString(item.Component) == Constants.ELEMENT_TYPE_AUDIO) newentity.ElementType = Convert.ToInt16(Constants.ElementType.Audio);//Added By Hiren 23-01-2017
                        else if (Convert.ToString(item.Component) == Constants.ELEMENT_TYPE_FILEUPLOAD) newentity.ElementType = Convert.ToInt16(Constants.ElementType.FileUpload);//Added By Hiren 23-01-2017

                        else newentity.ElementType = 0;//Added By Hiren 20-11-2017
                        newentity.FormQuestionId = item.FormQuestionId;
                        newentity.FormSubmissionId = item.FormSubmissionId;
                        formSubmission.Id = item.FormSubmissionId;
                        db.FormAnswers.Add(newentity);
                        db.SaveChanges();
                        //Workflow notifications Added by Hiren 06-12-2017 (Added Answer)
                        if (newentity.ElementType == Convert.ToInt16(Constants.ElementType.Section))
                        {
                            var formSectionList = form.FormSections.Where(l => l.FormId == answerJSON.FormId && l.VersionId == answerJSON.VersionId).ToList();
                            if (formSectionList != null && formSectionList.Count > 0)
                            {
                                if (form.WorkFlowEnabled && !item.IsReadOnly && answerJSON.EmployeeId != 0)
                                {
                                    SubmissionService _submissionService = new SubmissionService();
                                    string emailSubject = form.Name + " Submission Updated";
                                    //_submissionService.CommonSubmissionEmail(answerJSON.SubscriberId, form.Name, formSubmission.Id, answerJSON.EmployeeId, emailSubject, "E");
                                }
                            }
                        }
                        //End
                    }
                    else
                    {
                        if (!item.IsReadOnly)
                        {
                            formSubmission.Id = item.FormSubmissionId;
                            entity.Value = entity.ElementType == 16 && item.ElementType == 17 && item.ElementType == 29 && item.ElementType == 35 ? RemoveFileUrl(item.Value) : item.Value;//Changed By Hiren 25-01-2018
                            db.Entry(entity).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        //Workflow notifications Added by Hiren 10-11-2017 (Update Answer)
                        if (entity.ElementType == Convert.ToInt16(Constants.ElementType.Section))
                        {
                            //Added by Hiren 22-11-2017
                            var formSectionList = form.FormSections.Where(l => l.FormId == answerJSON.FormId && l.VersionId == answerJSON.VersionId).ToList();//Added By Hiren 22-11-2017
                            if (formSectionList != null && formSectionList.Count > 0)
                            {
                                if (form.WorkFlowEnabled && !item.IsReadOnly && answerJSON.EmployeeId != 0)
                                {
                                    SubmissionService _submissionService = new SubmissionService();
                                    string emailSubject = form.Name + " Submission Updated";
                                    //_submissionService.CommonSubmissionEmail(answerJSON.SubscriberId, form.Name, formSubmission.Id, answerJSON.EmployeeId, emailSubject, "E");
                                }
                            }
                        }
                        //End
                    }
                    //End
                    if (item.Component == "FILEUPLOAD")
                    {
                        if (!string.IsNullOrEmpty(item.Value))
                        {
                            Guid folderName = db.FormSubmissions.Where(f => f.Id == formSubmission.Id).FirstOrDefault().FolderName;//Changed By Hiren 25-01-2018
                            var formSubmissionPath = root + Convert.ToInt64(result.FormData["SubscriberId"]) + Constants.FormSubmissionDataFolder + folderName;
                            string path2 = root + Convert.ToInt64(result.FormData["SubscriberId"]) + Constants.ImageFolder + "TempImages/";
                            var itemList = item.Value.Split(',');
                            foreach (var objItem in itemList)
                            {
                                string folder = Constants.FilesFolder;//Added By Hiren 24-01-2018
                                string path = formSubmissionPath + folder;
                                if (File.Exists(path2 + objItem))
                                {
                                    File.Move(path2 + objItem, path + objItem);
                                }
                            }
                        }
                    }

                    //Added By Hiren 23-01-2018
                    if (item.Component == "CAPTUREIMAGE")
                    {
                        if (!string.IsNullOrEmpty(item.Value))
                        {
                            Guid folderName = db.FormSubmissions.Where(f => f.Id == formSubmission.Id).FirstOrDefault().FolderName;//Changed By Hiren 25-01-2018
                            var formSubmissionPath = root + Convert.ToInt64(result.FormData["SubscriberId"]) + Constants.FormSubmissionDataFolder + folderName;
                            string path2 = root + Convert.ToInt64(result.FormData["SubscriberId"]) + Constants.ImageFolder + "TempImages/";
                            var imageList = item.Value.Split(',');
                            foreach (var objItem in imageList)
                            {
                                string folder = Constants.ImageFolder;//Added By Hiren 24-01-2018
                                string path = formSubmissionPath + folder;
                                if (File.Exists(path2 + objItem))
                                {
                                    File.Move(path2 + objItem, path + objItem);
                                }
                            }
                        }
                    }

                    if (item.Component == "CAPTUREVIDEO")
                    {
                        if (!string.IsNullOrEmpty(item.Value))
                        {
                            Guid folderName = db.FormSubmissions.Where(f => f.Id == formSubmission.Id).FirstOrDefault().FolderName;//Changed By Hiren 25-01-2018
                            var formSubmissionPath = root + Convert.ToInt64(result.FormData["SubscriberId"]) + Constants.FormSubmissionDataFolder + folderName;
                            string path2 = root + Convert.ToInt64(result.FormData["SubscriberId"]) + Constants.ImageFolder + "TempImages/";
                            var videoList = item.Value.Split(',');
                            foreach (var objItem in videoList)
                            {
                                string folder = Constants.VideoFolder;//Added By Hiren 24-01-2018
                                string path = formSubmissionPath + folder;
                                if (File.Exists(path2 + objItem))
                                {
                                    File.Move(path2 + objItem, path + objItem);
                                }
                            }
                        }
                    }

                    if (item.Component == "AUDIO")
                    {
                        if (!string.IsNullOrEmpty(item.Value))
                        {
                            Guid folderName = db.FormSubmissions.Where(f => f.Id == formSubmission.Id).FirstOrDefault().FolderName;//Changed By Hiren 25-01-2018
                            var formSubmissionPath = root + Convert.ToInt64(result.FormData["SubscriberId"]) + Constants.FormSubmissionDataFolder + folderName;
                            string path2 = root + Convert.ToInt64(result.FormData["SubscriberId"]) + Constants.ImageFolder + "TempImages/";
                            var audioList = item.Value.Split(',');
                            foreach (var objItem in audioList)
                            {
                                string folder = Constants.AudioFolder;//Added By Hiren 24-01-2018
                                string path = formSubmissionPath + folder;
                                if (File.Exists(path2 + objItem))
                                {
                                    File.Move(path2 + objItem, path + objItem);
                                }
                            }
                        }
                    }
                }
                //Added By Hiren 07-12-2017 (Get Required Question List Does't Have Answer Value)
                List<CommonFormQuestionsModel> rqlist = new List<CommonFormQuestionsModel>();
                List<FormQuestion> fqList = db.FormQuestions.Where(i => i.FormId == answerJSON.FormId && i.FormVersionId == answerJSON.VersionId && i.FormToolsId > 1).AsEnumerable().Select(i => new FormQuestion { Id = i.Id, Question = i.Question, FormToolsId = i.FormToolsId, ParentQuestionId = i.ParentQuestionId, JSONQuestion = i.JSONQuestion }).ToList();
                //rqlist = _formQuestionService.GetRequiredFormQuestions(fqList, formSubmission.Id);
                //End
                //Added By Hiren 13-11-2017
                SubmissionLog logModel = db.SubmissionLogs.Where(i => i.FormId == answerJSON.FormId && i.SubmissionId == formSubmission.Id && i.EmployeeId == answerJSON.EmployeeId).FirstOrDefault();
                if (logModel != null)
                {
                    logModel.UpdatedOn = currentData;
                    db.Entry(logModel).State = EntityState.Modified;
                    db.SaveChanges();
                }
                else
                {
                    SubmissionLog SubmissionLog = new SubmissionLog();
                    SubmissionLog.FormId = answerJSON.FormId;
                    SubmissionLog.SubmissionId = formSubmission.Id;
                    SubmissionLog.EmployeeId = answerJSON.EmployeeId;
                    SubmissionLog.CreatedOn = currentData;
                    SubmissionLog.UpdatedOn = currentData;
                    db.SubmissionLogs.Add(SubmissionLog);
                    db.SaveChanges();
                }
                if (rqlist != null && rqlist.Count > 0)
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
                return Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }

        public string RemoveFileUrl(string value)
        {
            string strValue = string.Empty;
            if (!string.IsNullOrEmpty(value))
            {
                int counter = 0;
                var itemList = value.Split(',');
                foreach (var objItem in itemList)
                {
                    var n = objItem.Split('/');
                    string fileName = objItem.Split('/')[n.Length - 1];

                    if (counter > 0)
                        strValue = strValue + "," + fileName;
                    else
                        strValue = fileName;

                    counter++;
                }
                return strValue;
            }
            return null;
        }

        /// <summary>
        /// List of submission. Call SP to fetch data. (Changed by Hiren 24-10-2017 Added the appLoginId)
        /// </summary>
        /// <param name="subscriberId"></param>
        /// <param name="status">Approved, Processing. Ex. 1,2,3</param>
        /// <param name="id">SubmissionId</param>
        /// <param name="formId">formId</param>
        /// <param name="formVersionId">formVersionId</param>
        /// <param name="appLoginId">appLoginId</param>
        /// <param name="Page">Page</param>
        /// <param name="RecsPerPage">RecsPerPage</param>
        /// <param name="action">Approved, Accepted, Rejected. Ex. 1,2,3</param>
        /// <param name="formVersionId">1,2,3...</param>
        /// <returns>Return List of submission</returns>
        [Route("api/formsubmissions")]
        [HttpGet]
        public object GetList(int subscriberId, int? status, long id, int formId, int formVersionId, int appLoginId, int Page = 0, int RecsPerPage = 0, int action = 0)
        {
            try
            {
                if (formVersionId == 0)
                {
                    formVersionId = db.FormSubmissions.Where(f => f.FormId == formId && !f.IsDeleted)
                                    .AsEnumerable()
                                    .OrderByDescending(x => x.FormVersion.Id)
                                    .Select(i => i.FormVersion.Id).Distinct().FirstOrDefault();
                }
                Uri myuri = new Uri(HttpContext.Current.Request.Url.AbsoluteUri);
                string pathQuery = myuri.PathAndQuery;
                string path = myuri.ToString().Replace(pathQuery, "");
                string strcon = ConfigurationManager.ConnectionStrings["FormziEntitiesSP"].ConnectionString;
                SqlConnection DbConnection = new SqlConnection(strcon);
                DbConnection.Open();
                SqlCommand command = new SqlCommand("[dbo].[usp_GetFormSubmissionDetailByFormID]", DbConnection);
                command.CommandType = CommandType.StoredProcedure;
                //create type table
                DataTable table = new DataTable();
                SqlParameter parameter = command.Parameters.AddWithValue("@subscriberId", subscriberId);//Added By Hiren 23-11-2017
                parameter = command.Parameters.AddWithValue("@fromId", formId);
                parameter = command.Parameters.AddWithValue("@versionId", formVersionId);
                parameter = command.Parameters.AddWithValue("@IsHtml", 1);
                parameter = command.Parameters.AddWithValue("@Page", Page);
                parameter = command.Parameters.AddWithValue("@RecsPerPage", RecsPerPage);
                parameter = command.Parameters.AddWithValue("@Path", path);
                parameter = command.Parameters.AddWithValue("@AppLoginId", appLoginId);
                command.ExecuteNonQuery();
                SqlDataReader reader = command.ExecuteReader();
                table.Load(reader);
                DbConnection.Close();
                List<FormQuestion> fqList = db.FormQuestions.Where(i => i.FormId == formId && i.FormVersionId == formVersionId && i.FormToolsId > 1).AsEnumerable().Select(i => new FormQuestion { Id = i.Id, Question = i.Question, FormToolsId = i.FormToolsId, ParentQuestionId = i.ParentQuestionId, JSONQuestion = i.JSONQuestion }).ToList();
                table.Columns["Actions"].ColumnName = "101#00#Actions";
                table.Columns["Id"].ColumnName = "102#00#ID";
                Int16 counter = 104;
                //Added By Hiren 27-11-2017
                Employee empDetails = db.Employees.Where(e => e.AppLoginId == appLoginId).FirstOrDefault();
                Form formDetails = db.Forms.Where(f => f.Id == formId).FirstOrDefault();
                List<CommonFormQuestionsModel> cfql = new List<CommonFormQuestionsModel>();
                if (empDetails != null && empDetails.SystemRoleId != 1 && empDetails.SystemRoleId != 2 && empDetails.SystemRoleId != 3)//Changed By Hiren 25-01-2018
                {
                    cfql = _formQuestionService.GetRoleBasedFormQuestions(formId, formVersionId, appLoginId, fqList);
                }
                if (fqList.Count > 0)
                {
                    foreach (var item in fqList)
                    {
                        if (table.Columns[item.Id.ToString()] != null)
                        {
                            //Added By Hiren 27-11-2017
                            if (empDetails != null && empDetails.SystemRoleId != 1 && empDetails.SystemRoleId != 2 && empDetails.SystemRoleId != 3)//Changed By Hiren 25-01-2018
                            {
                                bool isExist = cfql.Where(e => e.Id == item.Id).Any();
                                if (isExist)
                                {
                                    table.Columns[item.Id.ToString()].ColumnName = counter + "#" + item.FormToolsId + "#" + item.Question;
                                }
                                else
                                {
                                    table.Columns.Remove(item.Id.ToString());//If Don't have permission than remove it.
                                }
                            }
                            else
                            {
                                table.Columns[item.Id.ToString()].ColumnName = counter + "#" + item.FormToolsId + "#" + item.Question;
                            }
                            //End
                            counter++;
                        }
                    }
                }
                //End
                counter++;
                table.Columns["SubmittedOn"].ColumnName = counter + "#00#Submitted On"; counter++;
                table.Columns["Reported By"].ColumnName = counter + "#00#Reported By"; counter++;
                table.Columns["Email"].ColumnName = counter + "#00#Email"; counter++;
                table.Columns["Phone No."].ColumnName = counter + "#00#Phone No."; counter++;
                table.Columns["IsCompleted"].ColumnName = counter + "#00#Status"; counter++;
                table.Columns["RowNum"].ColumnName = "98#00#RowNum";
                table.Columns["TotalCount"].ColumnName = "99#00#TotalCount";
                return table;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }

        /// <summary>
        /// Added AppLoginId By Hiren 24-10-2017
        /// </summary>
        /// <param name="Id">Id</param>
        /// <param name="AppLoginId">AppLoginId</param>
        /// <returns></returns>
        [AllowAnonymous]
        [Route("api/FormAnswerByFormSubmissionId/{Id}/{AppLoginId}")]
        [HttpGet]
        public object FormAnswerByFormSubmissionId(int Id, int AppLoginId)
        {
            try
            {
                long formId = db.FormSubmissions.Where(f => f.Id == Id).FirstOrDefault().FormId;
                Guid folderName = db.FormSubmissions.Where(f => f.Id == Id).FirstOrDefault().FolderName;
                var form = db.Forms.Where(f => f.Id == formId).FirstOrDefault();
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
                        FormQuestions = GetFormQuestionsByFormId((int)form.Id, Id, AppLoginId),//Added AppLoginId By Hiren 24-10-2017
                        //FormAnswers = GetFormAnswerbySubmissionId(Id),
                        FormName = form.Name,
                        FormImage = form.Image != null ? form.Image : string.Empty,
                        FileUrl = fileUrl,
                        SubscriberCompanyLogoPath = companyLogo != "" ? fileUrl + form.SubscriberId + Constants.ImageFolder + Constants.SubscriberFolder + companyLogo : "",
                        SubmissionFolderPath = fileUrl + form.SubscriberId + Constants.FormSubmissionDataFolder + folderName
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

        /// <summary>
        /// AppLoginId Added By Hiren 24-10-2017
        /// </summary>
        /// <param name="FormId">FormId</param>
        /// <param name="SubmissionId">SubmissionId</param>
        /// <param name="AppLoginId">AppLoginId</param>
        /// <returns></returns>
        public object GetFormQuestionsByFormId(int FormId, int SubmissionId, int AppLoginId)
        {
            try
            {
                long _formId = (long)FormId;
                int? _formversionId = db.FormSubmissions.Where(f => f.Id == SubmissionId && !f.IsDeleted).FirstOrDefault().FormVersionId;
                long _appLoginId = (long)AppLoginId; //Added by Hiren 24-10-2017
                FormVersion formVersionModel = db.FormVersions.Where(i => i.Id == _formversionId).FirstOrDefault();
                int FormVersionId = formVersionModel.Id;
                var form = db.Forms.Where(f => f.Id == FormId).Select(m => new FormModel
                {
                    Id = m.Id,
                    Name = m.Name,
                    Image = m.Image,
                    OrderBy = m.OrderBy,
                    IsActive = m.IsActive,
                    SubscriberId = m.SubscriberId,
                    LanguageId = m.LanguageId,
                    FormVersionId = FormVersionId,
                    WebFormUID = m.WebFormUID,
                    WorkFlowEnabled = m.WorkFlowEnabled,
                    IsVisibleSection = m.IsVisibleSection//Added By Hiren 27-11-2017
                }).FirstOrDefault();

                int SubscriberId = form.SubscriberId;
                List<FormQuestionsGroupByLanguage> formQue = new List<FormQuestionsGroupByLanguage>();
                List<LanguageModel> SubscriberLanguage = db.SubscriberLanguages.Where(i => i.SubcriberId == SubscriberId)
                   .OrderBy(o => o.DisplayOrder)
                   .Select(i => new LanguageModel
                   {
                       Id = i.Language.Id,
                       Name = i.Language.Name,
                       Rtl = i.Language.Rtl,
                       UniqueSeoCode = i.Language.UniqueSeoCode,
                       BaseLanguage = i.Language.Id == form.LanguageId
                   }).ToList();
                var fmQueList = db.FormQuestions.Where(f => f.FormId == FormId && !f.IsDeleted && f.FormVersionId == formVersionModel.Id).AsEnumerable().ToList();
                FormQuestionsGroupByLanguage model = fmQueList
                    .GroupBy(g => g.LanguageId)
                    .AsEnumerable()
                    .Select(j => new FormQuestionsGroupByLanguage
                    {
                        formQuestions = j.Select(x => new CommonFormQuestionsModel { Id = x.Id, Question = x.Question, FormToolsId = x.FormToolsId, JSONQuestion = x.JSONQuestion, ParentQuestionId = x.ParentQuestionId }).ToList(),
                        formAnswers = GetFormAnswerbySubmissionId(SubmissionId),
                        languageId = j.Select(x => x.LanguageId).FirstOrDefault(),
                        published = j.Select(x => x.IsPublished).FirstOrDefault()
                    }).FirstOrDefault();
                if (model != null)
                {
                    //Added By Hiren
                    if (form.WorkFlowEnabled)
                    {
                        //Render Question with answers based on role if section control there (Added By Hiren)
                        FormQuestionsGroupByLanguage newModel = new FormQuestionsGroupByLanguage();
                        newModel.languageId = model.languageId;
                        newModel.published = model.published;
                        //Changed By Hiren 27-11-2017 
                        List<CommonFormQuestionsModel> cfql = new List<CommonFormQuestionsModel>();
                        List<FormQuestion> fqList = model.formQuestions.ToList().AsEnumerable().Select(i => new FormQuestion { Id = i.Id, Question = i.Question, FormToolsId = i.FormToolsId, JSONQuestion = i.JSONQuestion, ParentQuestionId = i.ParentQuestionId }).ToList();
                        cfql = _formQuestionService.GetRoleBasedFormQuestions(FormId, formVersionModel.Id, AppLoginId, fqList);
                        if (cfql != null && cfql.Count > 0)
                        {
                            model.formQuestions = cfql.ToList();
                        }
                    }
                    //End
                    formQue.Add(model);
                }
                return new { FormQuestions = formQue, FormAnswers = GetFormAnswerbySubmissionId(SubmissionId), Form = form, FormLanguages = SubscriberLanguage };
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }

        /// <summary>
        /// Get Form Answer by Submission Id
        /// </summary>
        /// <param name="id">id</param>
        /// <returns>Form Answer List</returns>
        public List<FormAnswerWithIdModel> GetFormAnswerbySubmissionId(int id)
        {
            try
            {
                List<FormAnswerWithIdModel> lstmodel = new List<FormAnswerWithIdModel>();
                var frmAns = db.FormAnswers.Where(i => i.FormSubmissionId == id).AsEnumerable().ToList();
                foreach (var item in frmAns)
                {
                    FormAnswerWithIdModel model = new FormAnswerWithIdModel();
                    model.Id = item.Id;
                    model.Answer = item.ElementType == 16 && item.ElementType == 17 && item.ElementType == 29 && item.ElementType == 35 ? FileUrl(item.Value) : item.Value;//Changed By Hiren 25-01-2018
                    model.QuestionId = item.FormQuestionId;
                    model.ElementType = item.ElementType;
                    model.FormSubmissionId = item.FormSubmissionId;//Added By Hiren 24-10-2017
                    lstmodel.Add(model);
                }
                return lstmodel;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }

        /// <summary>
        /// No more used now. Change status of submission. Ex. Processing is true/false.
        /// </summary>
        /// <param name="id">submission id</param>
        /// <param name="processStatus">True/False</param>
        /// <returns>Return True/False</returns>
        [Route("api/editprocessingstatus")]
        [HttpGet]
        public bool EditProcessingStatus(int id, bool processStatus)
        {
            try
            {
                FormSubmission formSubmission = db.FormSubmissions.Where(f => f.Id == id).FirstOrDefault();
                if (formSubmission == null)
                {
                    return false;
                }
                formSubmission.IsProcessing = processStatus;
                db.Entry(formSubmission).State = EntityState.Modified;
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

        /// <summary>
        /// List of submission by form id. Form submission table details will be return.
        /// </summary>
        /// <param name="id">form id</param>
        /// <returns>List of submission.</returns>
        [Route("api/formsubmissionsListbyformid")]
        [HttpGet]
        public object GetFormSubmissionsListByFormId(long id)
        {
            try
            {
                if (id == 0)
                {
                    return null;
                }
                List<FormSubmissionModel> formSubmissionList = db.FormSubmissions.Where(f => f.FormId == id)
                    .OrderByDescending(o => o.SubmittedOn)
                    .AsEnumerable()
                    .Select(i => new FormSubmissionModel
                    {
                        Id = i.Id,
                        FormId = i.FormId,
                        FormName = i.Form.Name,
                        FormImage = i.Form.Image,
                        SubmittedOn = i.SubmittedOn,
                        IsCompleted = i.IsCompleted
                    }).ToList();
                return formSubmissionList;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }

        /// <summary>
        /// No more used now. Change state of submission. Ex. Approved, Rejected etc.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="isApproved"></param>
        /// <param name="employeeId"></param>
        /// <param name="status"></param>
        //[Route("api/editFormSubmission")]
        [HttpPost]
        public object EditFormSubmission(int id, int isApproved, long employeeId, int? status)
        {
            try
            {
                FormSubmission formSubmission = db.FormSubmissions.Where(f => f.Id == id).FirstOrDefault();
                if (formSubmission == null)
                {
                    return null;
                }
                formSubmission.ApprovedOn = Common.GetDateTime(db);
                formSubmission.ApprovedBy = employeeId;
                formSubmission.IsApproved = isApproved;
                formSubmission.IsProcessing = false;
                db.Entry(formSubmission).State = EntityState.Modified;
                db.SaveChanges();
                NotificationService _notificationService = new NotificationService();
                _notificationService.AppUserNotification(isApproved, formSubmission.Id, formSubmission.DeviceId, formSubmission.SubscriberId, formSubmission.AppInfoId);
                return GetList(formSubmission.SubscriberId, status, 0, 0, 0, 0);
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }

        /// <summary>
        /// File Upload. Common file upload method (EX. Form image, employee profile image)
        /// </summary>
        /// <param name="createUniqueName"></param>
        /// <param name="folderPath"></param>
        /// <param name="employeeId"></param>
        /// 
        /// <returns>Return Http response message</returns>
        [AllowAnonymous]
        [Route("api/uploadfile")]
        [HttpPost]
        public async Task<HttpResponseMessage> UploadFile()
        {
            try
            {
                HttpResponseMessage result = null;
                var httpRequest = HttpContext.Current.Request;
                HttpResponseMessage processCount = await Task.Run<HttpResponseMessage>(() =>
                {
                    if (httpRequest.Files.Count > 0)
                    {
                        String fileName = string.Empty;
                        List<object> fileNames = new List<object>();
                        foreach (string file in httpRequest.Files)
                        {
                            var postedFile = httpRequest.Files[file];
                            if (Convert.ToBoolean(httpRequest.Params["createUniqueName"]))
                            {
                                fileName = Guid.NewGuid() + System.IO.Path.GetExtension(postedFile.FileName);
                            }
                            else
                            {
                                fileName = postedFile.FileName;
                            }
                            var folderPath = httpRequest.Params["folderPath"];
                            Directory.CreateDirectory(folderPath);
                            postedFile.SaveAs(folderPath + "/" + fileName);

                            fileNames.Add(new
                            {
                                originalName = postedFile.FileName,
                                changedName = fileName
                            });
                        }
                        result = Request.CreateResponse(HttpStatusCode.Created, fileNames);
                    }
                    else
                    {
                        result = Request.CreateResponse(HttpStatusCode.BadRequest);
                    }
                    return result;
                });
                return processCount;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }

        /// <summary>
        /// No more use. Previously it was used in BI module of formzi , Reports and chart made by divyesh
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        //[Route("api/formsubmissionsanalytics/{id}")]
        [HttpGet]
        public object GetFormSubmissionsAnalytics(long id)
        {
            try
            {
                if (id == 0)
                {
                    return null;
                }
                var formsubmissionlist = db.FormSubmissions.Where(f => f.FormId == id).AsEnumerable();

                int Counter = 1;
                var formSubmissionListDay = formsubmissionlist
                    .OrderBy(o => o.SubmittedOn)
                    .AsEnumerable()
                    .GroupBy(g => g.SubmittedOn.Date)
                    .Select(i => new
                    {
                        //Key = i.Key.Date.Day,
                        Key = Counter++,
                        Count = i.Count(),
                        Day = i.Key.Date.Day + "-" + CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(i.Key.Date.Month),
                    }).ToList();

                Counter = 1;

                var formSubmissionListWeek = formsubmissionlist
                    .OrderBy(o => o.SubmittedOn)
                    .AsEnumerable()
                    .Select(p => new
                    {
                        Project = p,
                        Year = p.SubmittedOn.Year,
                        Week = CultureInfo.InvariantCulture.Calendar.GetWeekOfYear
                                      (p.SubmittedOn, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday),

                    })
                    .GroupBy(x => new { x.Year, x.Week })
                    .Select((g, i) => new
                    {
                        //WeekGroup = g,
                        Key = g.Key.Week,
                        WeekNum = i + 1,
                        Year = g.Key.Year,
                        CalendarWeek = g.Key.Week,
                        Count = g.Count(),
                        WeekDate = FirstDateOfWeek(g.Key.Year, g.Key.Week).ToShortDateString() + " - " + FirstDateOfWeek(g.Key.Year, g.Key.Week + 1).ToShortDateString(),
                    }).ToList();

                var formSubmissionListMonth = formsubmissionlist
                    .OrderBy(o => o.SubmittedOn)
                    .AsEnumerable()
                    .GroupBy(g => g.SubmittedOn.Month)
                    .Select(i => new
                    {
                        Count = i.Count(),
                        Month = i.Key,
                        MonthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(i.Key)
                    }).ToList();

                var formSubmissionListYear = formsubmissionlist
                      .OrderBy(o => o.SubmittedOn)
                      .AsEnumerable()
                      .GroupBy(g => g.SubmittedOn.Year)
                      .Select(i => new
                      {
                          Count = i.Count(),
                          Year = i.Key,
                      }).ToList();

                var model = new
                {
                    formSubmissionListMonth = formSubmissionListMonth,
                    formSubmissionListWeek = formSubmissionListWeek,
                    formSubmissionListYear = formSubmissionListYear,
                    formSubmissionListDay = formSubmissionListDay
                };
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
        /// Deprecated Method. Resolve submission. 
        /// I-Witness specific
        /// SUBMISSION_RESOLVE value is 3 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="appLoginId"></param>
        /// <returns></returns>
        //[Route("api/IWitnessResolveFormSubmission")]
        [HttpGet]
        public object IWitnessResolveFormSubmission(long id, long appLoginId)
        {
            try
            {
                if (id == 0 || appLoginId == 0)
                {
                    return null;
                }
                if (db.AppLogins.Where(i => i.Id == appLoginId).Any())
                {
                    FormSubmission formSubmissionModel = db.FormSubmissions.Where(f => f.Id == id).FirstOrDefault();
                    formSubmissionModel.IsApproved = Constants.SUBMISSION_RESOLVE; // here SUBMISSION_RESOLVE is 3 which is used for Resolved
                    formSubmissionModel.ApprovedOn = Common.GetDateTime(db);
                    db.Entry(formSubmissionModel).State = EntityState.Modified;
                    db.SaveChanges();
                    return true;
                }
                else
                {
                    return false;
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
        /// Used in BI module of formzi , Reports and chart made by divyesh
        /// added by Jay
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("api/FormAnalysis")]
        [HttpGet]
        public object FormAnalysis()
        {
            var formAnalysis = db.FormAnswers
                .Where(i => (i.FormQuestionId == 338 && i.Value == "True"))
                .Where(i => (i.FormQuestionId == 114 && i.Value == "Male"))
                .Where(i => (i.FormQuestionId == 111 && i.Value == "Pepsico"))
                .AsEnumerable()
                .Select(i => new
                {
                    value = i.Value
                }).ToList();

            var model = new
            {
                formAnalysis,
                formAnalysis.Count
            };
            return model;
        }

        /// <summary>
        /// Remove employee assignment with submission
        /// </summary>
        /// <param name="submissionId"></param>
        /// <param name="employeeId"></param>
        /// <returns></returns>
        [Route("api/removeEmployeeAssignment")]
        [HttpDelete]
        public IHttpActionResult RemoveEmployeeSubmissionMap(long submissionId)
        {
            try
            {
                if (submissionId > 0 && submissionId > 0)
                {
                    SubmissionService _submissionService = new SubmissionService();
                    bool isCompleted = _submissionService.RemoveEmpSubmissionMap(submissionId);
                    return Ok(isCompleted);
                }
                else
                {
                    return Ok(false);
                }
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                //return false;
                return Ok(false);
            }
        }

        //Added by jay mistry 26-8-2016
        [Route("api/EditSubmissionAction")]
        [HttpPost]
        public IHttpActionResult EditSubmissionAction(int submissionId, int actionId)
        {
            try
            {
                if (submissionId > 0 && actionId > 0)
                {
                    SubmissionService _submissionService = new SubmissionService();
                    bool isCompleted = _submissionService.UpdateSubmissionAction(submissionId, actionId);
                    return Ok(isCompleted);
                }
                return Ok(false);
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return Ok(false);
            }
        }

        //[Route("api/SubmissionActionList")]
        [HttpGet]
        public IHttpActionResult SubmissionActionList()
        {
            try
            {
                List<SelectListItem> actionList = Enum.GetValues(typeof(Constants.SubmissionAction)).Cast<Constants.SubmissionAction>()
               .Select(v => new SelectListItem
               {
                   Text = v.ToString(),
                   Value = ((int)v).ToString()
               }).ToList();

                return Ok(actionList);
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return Ok();
            }
        }

        //Added by jay mistry 16-9-2016
        /// <summary>
        /// Get details of submission by submission id. It also includes AppInfo and location details.
        /// </summary>
        /// <param name="submissionId"></param>
        /// <returns></returns>
        [Route("api/SubmissionInfo")]
        [HttpGet]
        public IHttpActionResult SubmissionInfo(long submissionId)
        {
            try
            {
                if (submissionId > 0)
                {
                    FormSubmission fsModel = db.FormSubmissions.Where(i => i.Id == submissionId).FirstOrDefault();

                    FormSubmissionModel model = new FormSubmissionModel();

                    model.AppInfoID = fsModel.AppInfoId != null ? (int)fsModel.AppInfoId : 0;
                    model.ApprovedBy = fsModel.ApprovedBy;
                    model.ApprovedOn = fsModel.ApprovedOn;
                    model.DeviceId = fsModel.DeviceId;
                    model.EmployeeId = fsModel.EmployeeId;
                    model.FormId = fsModel.FormId;
                    model.FormImage = fsModel.Form.Image;
                    model.FormName = fsModel.Form.Name;
                    model.Id = fsModel.Id;
                    model.IsApproved = fsModel.IsApproved;
                    model.IsProcessing = fsModel.IsProcessing;
                    model.SubmittedOn = fsModel.SubmittedOn;
                    model.SubscriberId = fsModel.SubscriberId;
                    model.LanguageId = (int)fsModel.LanguageId;
                    model.LatLong = fsModel.LatLong;
                    model.Latitude = fsModel.Latitude;
                    model.Longitude = fsModel.Longitude;

                    if (model.AppInfoID > 0 && !string.IsNullOrEmpty(model.DeviceId))
                    {
                        if (model.AppInfoID > 0)
                        {
                            AppUserInfo appUserInfoModel = db.AppUserInfoes.Where(i => i.AppInfoID == model.AppInfoID).FirstOrDefault();

                            model.Email = appUserInfoModel.Email;
                            model.Name = appUserInfoModel.Name;
                            model.PhoneNo = appUserInfoModel.PhoneNo;
                        }
                        else if (!string.IsNullOrEmpty(model.DeviceId))
                        {
                            AppUserInfo appUserInfoModel = db.AppUserInfoes.Where(i => i.DeviceId == model.DeviceId).FirstOrDefault();

                            model.Email = appUserInfoModel.Email;
                            model.Name = appUserInfoModel.Name;
                            model.PhoneNo = appUserInfoModel.PhoneNo;
                        }

                        PostLocation postModel = db.PostLocations.Where(i => i.SubmissionId == model.Id).FirstOrDefault();

                        if (postModel != null)
                        {
                            model.administrative_area_level_1 = postModel.administrative_area_level_1;
                            model.administrative_area_level_2 = postModel.administrative_area_level_2;
                            model.administrative_area_level_3 = postModel.administrative_area_level_3;
                            model.colloquial_area = postModel.colloquial_area;
                            model.country = postModel.country;
                            model.locality = postModel.locality;
                            model.neighborhood = postModel.neighborhood;
                            model.postal_code = postModel.postal_code;
                            model.route = postModel.route;
                            model.shortName = postModel.shortName;
                            model.sublocality = postModel.sublocality;
                        }
                    }
                    return Ok(model);
                }
                return Ok();
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return Ok();
            }
        }

        [AllowAnonymous]
        [Route("api/GetVideoFile")]
        [HttpGet]
        public System.Web.Mvc.ActionResult GetVideoFile(string path, string name)
        {
            return new FormziApi.App_Start.VideoDataResult();
        }

        /// <summary>
        /// List of submission by user id. (Only forms assigned to user)
        /// </summary>
        /// <param name="id">Employee id</param>
        /// <returns>List of submission.</returns>
        [Route("api/formSubmissionsByUserId")]
        [HttpGet]
        public object GetFormSubmissionsListByFormId(int id)
        {
            try
            {
                if (id == 0)
                {
                    return null;
                }
                List<FormSubmissionModel> formSubmissionList = db.FormSubmissions.Where(f => f.SubmittedBy == id)
                    .OrderByDescending(o => o.SubmittedOn)
                    .AsEnumerable()
                    .Select(i => new FormSubmissionModel
                    {
                        Id = i.Id,
                        FormId = i.FormId,
                        FormName = i.Form.Name,
                        FormImage = i.Form.Image,
                        SubmittedOn = i.SubmittedOn,
                        IsCompleted = i.IsCompleted
                    }).ToList();
                return formSubmissionList;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }

        [Route("api/RemoveSubmission")]
        [HttpDelete]
        public bool RemoveFormSubmission(long id)
        {
            try
            {
                FormSubmission formSubmission = db.FormSubmissions.Find(id);
                if (formSubmission == null)
                {
                    return false;
                }
                List<Notification> notificationList = db.Notifications
                            .Where(i => i.AppInfoId == formSubmission.AppInfoId)
                            .AsEnumerable()
                            .Where(i => i.Message.Contains("#" + id)).ToList();
                if (notificationList.Count > 0)
                {
                    foreach (var item in notificationList)
                    {
                        db.Notifications.Remove(item);
                    }
                    db.SaveChanges();
                }
                formSubmission.IsDeleted = true;
                formSubmission.UpdatedOn = Common.GetDateTime(db);
                db.Entry(formSubmission).State = EntityState.Modified;
                db.SaveChanges();
                return true;
                //if (id > 0)
                //{
                //    string folderName = string.Empty;
                //    string path = string.Empty;

                //    var formAnswerModel = db.FormAnswers.Where(i => i.FormSubmissionId == id).AsEnumerable();
                //    var formSubmissionModel = db.FormSubmissions.Where(i => i.Id == id).FirstOrDefault();

                //    var subEmpMappings = db.SubmissionEmployeeMaps.Where(i => i.SubmissionId == id).ToList();

                //    if (formAnswerModel != null && formSubmissionModel != null && subEmpMappings != null)
                //    {
                //        var postLocationModel = db.PostLocations.Where(i => i.SubmissionId == formSubmissionModel.Id).FirstOrDefault();

                //        db.FormAnswers.RemoveRange(formAnswerModel);
                //        db.SaveChanges();

                //        folderName = formSubmissionModel.FolderName.ToString();

                //        List<AppSetting> appSettingList = db.AppSettings.Where(i => i.SubscriberId == formSubmissionModel.SubscriberId).ToList();

                //        string fileUrl = appSettingList.Where(i => i.Key == Constants.FileUrl).FirstOrDefault() != null ? appSettingList.Where(i => i.Key == Constants.FileUrl).FirstOrDefault().Value : string.Empty;
                //        string fileRoot = appSettingList.Where(i => i.Key == Constants.FileRoot).FirstOrDefault() != null ? appSettingList.Where(i => i.Key == Constants.FileRoot).FirstOrDefault().Value : string.Empty;

                //        string directoryPath = fileRoot + formSubmissionModel.SubscriberId + "/" + Constants.FormSubmissionFolder + folderName;

                //        var jsonPath = fileRoot + formSubmissionModel.SubscriberId + Constants.JSONFolder;

                //        if (!string.IsNullOrEmpty(fileUrl) && Directory.Exists(directoryPath))
                //        {
                //            //Directory.Delete(fileRoot + formSubmissionModel.SubscriberId + "/" + Constants.FormSubmissionFolder + folderName);

                //            DeleteDirectory(directoryPath);

                //            if (!string.IsNullOrEmpty(fileRoot) && File.Exists(jsonPath + folderName + ".json"))
                //            {
                //                File.Delete(jsonPath + folderName + ".json");
                //            }
                //        }

                //        List<Notification> notificationList = db.Notifications
                //            .Where(i => i.AppInfoId == formSubmissionModel.AppInfoId)
                //            .AsEnumerable()
                //            .Where(i => i.Message.Contains("#" + id)).ToList();

                //        if (notificationList.Count > 0)
                //        {
                //            foreach (var item in notificationList)
                //            {
                //                db.Notifications.Remove(item);
                //            }
                //            db.SaveChanges();
                //        }

                //        if (postLocationModel != null)
                //        {
                //            db.PostLocations.Remove(postLocationModel);
                //            db.SaveChanges();
                //        }

                //        if (subEmpMappings != null)
                //        {
                //            db.SubmissionEmployeeMaps.RemoveRange(subEmpMappings);
                //            db.SaveChanges();
                //        }

                //        db.FormSubmissions.Remove(formSubmissionModel);
                //        db.SaveChanges();
                //        return true;
                //    }
                //    else
                //    {
                //        return false;
                //    }
                //}
                //return false;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return false;
            }
        }

        #endregion

        #region Static And Void Methods

        static DateTime FirstDateOfWeek(int year, int weekOfYear)
        {
            DateTime jan1 = new DateTime(year, 1, 1);

            int daysOffset = (int)CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek - (int)jan1.DayOfWeek;

            DateTime firstMonday = jan1.AddDays(daysOffset);

            int firstWeek = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(jan1, CultureInfo.CurrentCulture.DateTimeFormat.CalendarWeekRule, CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek);

            if (firstWeek <= 1)
            {
                weekOfYear -= 1;
            }

            return firstMonday.AddDays(weekOfYear * 7);
        }

        public static DateTime FirstDateOfWeekISO8601(int year, int weekOfYear)
        {
            DateTime jan1 = new DateTime(year, 1, 1);
            int daysOffset = DayOfWeek.Thursday - jan1.DayOfWeek;

            DateTime firstThursday = jan1.AddDays(daysOffset);
            var cal = CultureInfo.CurrentCulture.Calendar;
            int firstWeek = cal.GetWeekOfYear(firstThursday, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

            var weekNum = weekOfYear;
            if (firstWeek <= 1)
            {
                weekNum -= 1;
            }
            var result = firstThursday.AddDays(weekNum * 7);
            return result.AddDays(-3);
        }

        public static void DeleteDirectory(string target_dir)
        {
            string[] files = Directory.GetFiles(target_dir);
            string[] dirs = Directory.GetDirectories(target_dir);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }

            Directory.Delete(target_dir, false);
        }

        #endregion

        #region I-Witness Specific

        /// <summary>
        /// When form submitted, after submitting form and its images, it it changes flag of IsSync to true.
        /// For i-witness Mobile  app
        /// </summary>
        /// <param name="folderName"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [Route("api/editsyncstatus/{folderName}")]
        [HttpPost]
        public bool EditSyncStatus(Guid folderName)
        {
            try
            {
                FormSubmission formSubmission = db.FormSubmissions.Where(f => f.FolderName == folderName).FirstOrDefault();
                if (formSubmission == null)
                {
                    return false;
                }
                formSubmission.IsSync = true;
                db.Entry(formSubmission).State = EntityState.Modified;
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

        /// <summary>
        /// No more in use. Change status of submission ex. Resolved, Reject etc
        /// For i-witness mobile app 
        /// </summary>
        /// <param name="submissionId"></param>
        /// <param name="currentEmpId"></param>
        /// <param name="assignToEmpId"></param>
        /// <param name="actionId"></param>
        /// <returns></returns>
        //[Route("api/SubmissionAction")]
        [HttpPost]
        public IHttpActionResult SubmissionAction(long submissionId, long currentEmpId, long assignToEmpId, int actionId)
        {
            try
            {
                SubmissionService _submissionService = new SubmissionService();
                bool isCompleted = _submissionService.SubmissionAction(submissionId, currentEmpId, assignToEmpId, actionId);
                return Ok(isCompleted);
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return Ok(false);
            }
        }

        /// <summary>
        /// I-Witness dashboard contact form inquiry mail.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="email"></param>
        /// <param name="phone"></param>
        /// <param name="message"></param>
        /// <param name="languageId"></param>
        /// <param name="subscriberId"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [Route("api/sendInquiryMail")]
        [HttpGet]
        public bool sendInquiryMail(string name, string email, string phone, string message, int languageId, int subscriberId = 1)
        {
            try
            {
                string strTemplate = "";
                string subscriberLogo = "";
                var fileUrl = "";
                string emailSignature = "";
                string strInnfyAdminEmail = "";
                subscriberId = subscriberId > 0 ? subscriberId : 1;

                Subscriber subscriberdbModel = db.Subscribers.Where(i => i.Id == subscriberId).FirstOrDefault();
                fileUrl = subscriberdbModel.AppSettings.Where(i => i.Key == Constants.FileUrl).FirstOrDefault().Value;
                subscriberLogo = !string.IsNullOrEmpty(subscriberdbModel.CompanyLogo) ? fileUrl + subscriberdbModel.Id + Constants.ImageFolder + Constants.SubscriberFolder + subscriberdbModel.CompanyLogo : "";
                emailSignature = subscriberdbModel.AppSettings.Where(i => i.Key == Constants.EmailSignature).FirstOrDefault().Value;
                strInnfyAdminEmail = subscriberdbModel.AppSettings.Where(i => i.Key == Constants.INNFY_ADMIN_EMAIL).FirstOrDefault().Value;

                string InquiryMailPath = "~/MailTemplates/Languages/" + languageId + "/SendInquiryMail.html";
                StreamReader fp = new StreamReader(System.Web.HttpContext.Current.Server.MapPath(InquiryMailPath));
                strTemplate = fp.ReadToEnd();
                fp.Close();
                string password = General.CreatePassword();
                strTemplate = strTemplate.Replace("@Name", "Admin").Replace("@Email", email).Replace("@CustName", name).Replace("@PhoneNo", phone).Replace("@Message", message).Replace("@Signature", emailSignature).Replace("@CompanyLogo", subscriberLogo);
                if (phone == "" || phone == null)
                {
                    strTemplate = strTemplate.Replace("@Phone", "display:none;");
                }
                if (message == "" || message == null)
                {
                    strTemplate = strTemplate.Replace("@Msg", "display:none;");
                }
                //ConfigurationManager.AppSettings["InnfyAdminEmail"].ToString()
                Helper.General.InnfySendInquiryEmail(strInnfyAdminEmail, strTemplate, "Message from www.i-Witness.org");
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
        /// Get Form Submission Info By Id (Display Info) Added By Hiren 28-11-2017
        /// </summary>
        /// <param name="id">id</param>
        /// <param name="AppLoginId">AppLoginId</param>
        /// <returns>Single Submission Details</returns>
        [AllowAnonymous]
        [Route("api/formsubmissioninfobyid")]
        [HttpGet]
        public object GetFormSubmissionInfoById(long Id, long AppLoginId)
        {
            try
            {
                var formSubmission = db.FormSubmissions.Where(f => f.Id == Id).FirstOrDefault();
                if (formSubmission == null)
                {
                    return null;
                }
                FormSubmissionModel model = new FormSubmissionModel();
                FormSubmission dbModel = db.FormSubmissions.Where(f => f.Id == Id).FirstOrDefault();
                if (dbModel != null)
                {
                    model.Id = dbModel.Id;
                    model.FolderName = dbModel.FolderName;
                    model.FormImage = dbModel.Form.Image;
                    model.FormId = dbModel.FormId;
                    model.FormName = dbModel.Form.Name;
                    model.Latitude = dbModel.Latitude;
                    model.Longitude = dbModel.Longitude;
                    long VersionId = (long)dbModel.FormVersionId;
                    Employee empDetails = db.Employees.Where(e => e.AppLoginId == AppLoginId).FirstOrDefault();
                    if (empDetails != null)
                    {
                        List<FormQuestion> fqList = db.FormQuestions.Where(i => i.FormId == dbModel.FormId && i.FormVersionId == dbModel.FormVersionId && i.FormToolsId > 1).AsEnumerable().Select(i => new FormQuestion { Id = i.Id, Question = i.Question, JSONQuestion = i.JSONQuestion, FormToolsId = i.FormToolsId, ParentQuestionId = i.ParentQuestionId }).ToList();
                        List<FormAnswerModel> faList = new List<FormAnswerModel>();
                        if (empDetails.SystemRoleId == 1 || empDetails.SystemRoleId == 2 || empDetails.SystemRoleId == 3)//Subscriber Admin,Operation Head,Operation Admin
                        {
                            foreach (var que in fqList)
                            {
                                FormAnswer ans = new FormAnswer();
                                ans = db.FormAnswers.Where(a => a.FormQuestionId == que.Id && a.FormSubmissionId == model.Id).FirstOrDefault();
                                if (ans != null)
                                {
                                    FormAnswerModel faModel = new FormAnswerModel();
                                    faModel.Id = ans.Id;
                                    faModel.ElementType = ans.ElementType;
                                    faModel.Value = ans.ElementType == 16 && ans.ElementType == 17 && ans.ElementType == 29 && ans.ElementType == 35 ? FileUrl(ans.Value) : ans.Value;//Changed By Hiren 25-01-2018
                                    faModel.FormQuestionId = ans.FormQuestionId;
                                    faModel.FormSubmissionId = ans.FormSubmissionId;
                                    faModel.JSONQuestion = JsonConvert.DeserializeObject<FormComponent>(que.JSONQuestion).label;
                                    faList.Add(faModel);
                                }
                                else
                                {
                                    FormAnswerModel faModel = new FormAnswerModel();
                                    faModel.Id = 0;
                                    faModel.ElementType = que.FormToolsId;
                                    faModel.Value = "";
                                    faModel.FormQuestionId = que.Id;
                                    faModel.FormSubmissionId = Id;
                                    faModel.JSONQuestion = JsonConvert.DeserializeObject<FormComponent>(que.JSONQuestion).label;
                                    faList.Add(faModel);
                                }
                                model.FormAnswers = faList.OrderBy(f => f.FormQuestionId).ToList();
                            }
                        }
                        else
                        {
                            //Other System Role Id
                            List<CommonFormQuestionsModel> cfql = new List<CommonFormQuestionsModel>();
                            cfql = _formQuestionService.GetRoleBasedFormQuestions(model.FormId, VersionId, AppLoginId, fqList);
                            if (cfql != null && cfql.Count > 0)
                            {
                                foreach (var que in cfql)
                                {
                                    FormAnswer ans = new FormAnswer();
                                    ans = db.FormAnswers.Where(a => a.FormQuestionId == que.Id && a.FormSubmissionId == model.Id).FirstOrDefault();
                                    if (ans != null)
                                    {
                                        FormAnswerModel faModel = new FormAnswerModel();
                                        faModel.Id = ans.Id;
                                        faModel.ElementType = ans.ElementType;
                                        faModel.Value = ans.ElementType == 16 && ans.ElementType == 17 && ans.ElementType == 29 && ans.ElementType == 35 ? FileUrl(ans.Value) : ans.Value;//Changed By Hiren 25-01-2018
                                        faModel.FormQuestionId = ans.FormQuestionId;
                                        faModel.FormSubmissionId = ans.FormSubmissionId;
                                        faModel.JSONQuestion = JsonConvert.DeserializeObject<FormComponent>(que.JSONQuestion).label;
                                        faList.Add(faModel);
                                    }
                                    else
                                    {
                                        FormAnswerModel faModel = new FormAnswerModel();
                                        faModel.Id = 0;
                                        faModel.ElementType = que.FormToolsId;
                                        faModel.Value = "";
                                        faModel.FormQuestionId = que.Id;
                                        faModel.FormSubmissionId = Id;
                                        faModel.JSONQuestion = JsonConvert.DeserializeObject<FormComponent>(que.JSONQuestion).label;
                                        faList.Add(faModel);
                                    }
                                }
                                model.FormAnswers = faList.OrderBy(f => f.FormQuestionId).ToList();
                            }
                        }
                    }
                }
                return model;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }

        //I-witness specific, Mobile
        [AllowAnonymous]
        [Route("api/formsubmissionbyid")]
        [HttpGet]
        public object GetFormSubmissionById(long id, string status)
        {
            try
            {
                var formSubmission = db.FormSubmissions.Where(f => f.Id == id).FirstOrDefault();
                if (formSubmission == null)
                {
                    return null;
                }
                FormSubmissionModel model = new FormSubmissionModel();
                FormSubmission dbModel = db.FormSubmissions.Where(f => f.Id == id).FirstOrDefault();
                if (dbModel != null)
                {
                    model.Id = dbModel.Id;
                    model.FolderName = dbModel.FolderName;
                    model.FormImage = dbModel.Form.Image;
                    model.FormId = dbModel.FormId;
                    model.FormName = dbModel.Form.Name;
                    model.Latitude = dbModel.Latitude;
                    model.Longitude = dbModel.Longitude;
                    model.FormAnswers = dbModel.FormAnswers.Select(j => new FormAnswerModel
                    {
                        Id = j.Id,
                        ElementType = j.ElementType,
                        JSONQuestion = JsonConvert.DeserializeObject<FormComponent>(j.FormQuestion.JSONQuestion).label,
                        Value = j.ElementType == 16 && j.ElementType == 17 && j.ElementType == 29 && j.ElementType == 35 ? FileUrl(j.Value) : j.Value //Changed By Hiren 25-01-2018
                    }).ToList();
                }

                return model;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }

        public string FileUrl(string value)
        {
            string strValue = string.Empty;
            if (!string.IsNullOrEmpty(value))
            {
                int counter = 0;
                var itemList = value.Split(',');
                foreach (var objItem in itemList)
                {
                    string fileExtension = Path.GetExtension(objItem);
                    string folder = General.GetMimeType(fileExtension);
                    string path = folder + objItem;

                    if (counter > 0)
                        strValue = strValue + "," + path;
                    else
                        strValue = path;

                    counter++;
                }
                return strValue;
            }
            return null;
        }

        //I-Witness Specific
        ///[Route("api/InnfyAssignEmployeeToSubmission")]
        [HttpGet]
        public bool AssignEmployeeToSubmission(int subscriberId, long submissionId, long employeeId)
        {
            try
            {
                if (submissionId > 0 && submissionId > 0 && employeeId > 0)
                {
                    var formSubmission = db.FormSubmissions.Where(i => i.SubscriberId == subscriberId && i.Id == submissionId).FirstOrDefault();
                    if (formSubmission != null)
                    {
                        //start
                        //unassign from submission employee mapping table
                        List<SubmissionEmployeeMap> unAssignList = db.SubmissionEmployeeMaps
                            .Where(i => i.SubmissionId == submissionId && i.Assigned).ToList();

                        foreach (SubmissionEmployeeMap submissionModel in unAssignList)
                        {
                            submissionModel.Assigned = false;
                            submissionModel.UpdatedOn = Common.GetDateTime(db);
                            db.Entry(submissionModel).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        //End

                        //start
                        //unassign from submission employee mapping table
                        List<SubmissionEmployeeMap> list = db.SubmissionEmployeeMaps.Where(i => i.SubmissionId == submissionId && i.Assigned).ToList();

                        foreach (SubmissionEmployeeMap submissionModel in list)
                        {
                            submissionModel.Assigned = false;
                            submissionModel.UpdatedOn = Common.GetDateTime(db);
                            db.Entry(submissionModel).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        //End

                        SubmissionEmployeeMap assignModel = db.SubmissionEmployeeMaps
                           .Where(i => i.SubmissionId == submissionId && i.EmployeeId == employeeId && i.Assigned)
                           .OrderByDescending(o => o.Id)
                           .FirstOrDefault();

                        if (assignModel != null)
                        {
                            assignModel.Assigned = true;
                            assignModel.UpdatedOn = Common.GetDateTime(db);
                            db.Entry(assignModel).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                        else
                        {
                            //code to email employee for multiple submission assign

                            SubmissionEmployeeMap model = new SubmissionEmployeeMap();
                            model.SubmissionId = submissionId;
                            model.EmployeeId = employeeId;
                            model.Assigned = true;
                            model.CreatedOn = Common.GetDateTime(db);

                            db.SubmissionEmployeeMaps.Add(model);
                            db.SaveChanges();

                            //Added by jay on 17-6-2016
                            //Notification Mail
                            NotificationService notificationService = new NotificationService();
                            notificationService.AppUserNotification(Constants.NOTIFICATION_ASSIGN_TO_EMPLOYEE, formSubmission.Id, formSubmission.DeviceId, formSubmission.SubscriberId, formSubmission.AppInfoId);
                        }
                        return true;
                    }
                    return false;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return false;
            }
        }

        //i-Witness Specific
        ///[Route("api/InnfyBulkAssignEmployeeToSubmission")]
        [HttpPost]
        public bool BulkAssignEmployeeToSubmission(int subscriberId, string formSubmissionListString, long employeeId)
        {
            try
            {
                List<BultFormSubmissionId> formSubmissionList = JsonConvert.DeserializeObject<List<BultFormSubmissionId>>(formSubmissionListString);
                if (subscriberId > 0 && formSubmissionList != null && employeeId > 0)
                {
                    foreach (var item in formSubmissionList)
                    {
                        long _id = long.Parse(item.Id);
                        var formSubmission = db.FormSubmissions.Where(i => i.SubscriberId == subscriberId && i.Id == _id).FirstOrDefault();
                        if (formSubmission != null)
                        {
                            //Added by jay on 22-8-2016

                            if (db.SubmissionEmployeeMaps.Where(i => i.SubmissionId == _id && i.EmployeeId == employeeId && i.Assigned).Any())
                            {

                                SubmissionEmployeeMap subEmpMap = db.SubmissionEmployeeMaps.Where(i => i.SubmissionId == _id && i.EmployeeId == employeeId && i.Assigned)
                                    .OrderByDescending(o => o.Id)
                                    .FirstOrDefault();

                                if (subEmpMap != null)
                                {
                                    subEmpMap.UpdatedOn = Common.GetDateTime(db);
                                    subEmpMap.Assigned = false;
                                    db.Entry(subEmpMap).State = EntityState.Modified;
                                    db.SaveChanges();
                                }
                            }

                            if (!db.SubmissionEmployeeMaps.Where(i => i.SubmissionId == _id && i.EmployeeId == employeeId && !i.Assigned).Any())
                            {
                                SubmissionEmployeeMap subEmpMap = new SubmissionEmployeeMap();
                                subEmpMap.CreatedOn = Common.GetDateTime(db);
                                subEmpMap.Assigned = true;
                                subEmpMap.EmployeeId = employeeId;
                                subEmpMap.SubmissionId = long.Parse(item.Id);

                                db.SubmissionEmployeeMaps.Add(subEmpMap);
                                db.SaveChanges();
                            }

                            //code to email employee for multiple submission assign

                            //Added by jay on 22-8-2016
                            //Notification Mail
                            NotificationService notificationService = new NotificationService();
                            notificationService.AppUserNotification(Constants.NOTIFICATION_ASSIGN_TO_EMPLOYEE, formSubmission.Id, formSubmission.DeviceId, formSubmission.SubscriberId, formSubmission.AppInfoId);

                        }
                    }
                    return true;
                }
                else if (subscriberId > 0 && formSubmissionList != null && employeeId == 0)
                {
                    foreach (var item in formSubmissionList)
                    {
                        long _id = long.Parse(item.Id);
                        var fsModel = db.FormSubmissions.Where(i => i.SubscriberId == subscriberId && i.Id == _id).FirstOrDefault();
                        if (fsModel != null)
                        {
                            //remove from submission employee mapping table
                            List<SubmissionEmployeeMap> list = db.SubmissionEmployeeMaps.Where(i => i.SubmissionId == _id).ToList();

                            foreach (SubmissionEmployeeMap model in list)
                            {
                                db.SubmissionEmployeeMaps.Remove(model);
                                db.SaveChanges();
                            }
                        }
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return false;
            }
        }

        /// <summary>
        /// No more in use. I-Witness Specific
        /// </summary>
        /// <param name="employeeId"></param>
        /// <param name="subscriberId"></param>
        /// <returns></returns>
        //[Route("api/GetResolvedIssueByEmployee")]
        [HttpGet]
        public object GetResolvedIssueByEmployee(long employeeId, int subscriberId)
        {
            try
            {
                //Here 3 is used for resolved submission
                int IsApproved = 3;
                IsApproved = 1;

                List<FormSubmissionModel> formSubmissionList = null;
                if (employeeId >= 0 && subscriberId > 0)
                {
                    formSubmissionList = db.SubmissionEmployeeMaps
                        .Where(i => i.FormSubmission.SubscriberId == subscriberId && i.FormSubmission.IsApproved == IsApproved && i.EmployeeId == employeeId && i.Assigned)
                        .Select(i => new FormSubmissionModel
                        {
                            ApprovedBy = i.FormSubmission.ApprovedBy,
                            ApprovedOn = i.FormSubmission.ApprovedOn,
                            //AssignToName = i.AssignTo != null ? i.Employee.FirstName + " " + i.Employee.LastName : string.Empty,
                            AssignToName = i.Employee.FirstName + " " + i.Employee.LastName,
                            DeviceId = i.FormSubmission.DeviceId,
                            EmployeeId = i.EmployeeId,
                            FormId = i.FormSubmission.FormId,
                            FormImage = i.FormSubmission.Form.Image,
                            FormName = i.FormSubmission.Form.Name,
                            Id = i.FormSubmission.Id,
                            IsApproved = i.FormSubmission.IsApproved,
                            IsProcessing = i.FormSubmission.IsProcessing,
                            SubmittedOn = i.FormSubmission.SubmittedOn,
                            SubscriberId = i.FormSubmission.SubscriberId,
                            LanguageId = (int)i.FormSubmission.LanguageId,
                            FormAnswers = i.FormSubmission.FormAnswers.Select(j => new FormAnswerModel
                            {
                                Id = j.Id,
                                Value = j.Value,
                                ElementType = j.ElementType,
                                FormQuestionId = j.FormQuestionId,
                                FormSubmissionId = j.FormSubmissionId
                            }).ToList()
                        }).ToList();
                    return formSubmissionList;
                }
                return formSubmissionList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        //[Route("api/ComplaintsAssignedToMe")]
        [HttpGet]
        public object GetComplaintsAssignedToMe(long employeeId, int subscriberId)
        {
            try
            {
                //Here SUBMISSION_APPROVED = 1 is used for approved submission
                int IsApproved = Constants.SUBMISSION_APPROVED;
                List<FormSubmissionModel> formSubmissionList = null;
                if (employeeId >= 0 && subscriberId > 0)
                {
                    formSubmissionList = db.SubmissionEmployeeMaps
                        .Where(i => i.FormSubmission.SubscriberId == subscriberId && i.FormSubmission.IsApproved == IsApproved && i.EmployeeId == employeeId && i.Assigned)
                        .Select(i => new FormSubmissionModel
                        {
                            ApprovedBy = i.FormSubmission.ApprovedBy,
                            ApprovedOn = i.FormSubmission.ApprovedOn,
                            //AssignToName = i.AssignTo != null ? i.Employee.FirstName + " " + i.Employee.LastName : string.Empty,
                            AssignToName = i.Employee.FirstName + " " + i.Employee.LastName,
                            DeviceId = i.FormSubmission.DeviceId,
                            EmployeeId = i.EmployeeId,
                            FormId = i.FormSubmission.FormId,
                            FormImage = i.FormSubmission.Form.Image,
                            FormName = i.FormSubmission.Form.Name,
                            Id = i.FormSubmission.Id,
                            IsApproved = i.FormSubmission.IsApproved,
                            IsProcessing = i.FormSubmission.IsProcessing,
                            SubmittedOn = i.FormSubmission.SubmittedOn,
                            SubscriberId = i.FormSubmission.SubscriberId,
                            LanguageId = (int)i.FormSubmission.LanguageId,
                            Latitude = i.FormSubmission.Latitude,
                            LatLong = i.FormSubmission.LatLong,
                            Longitude = i.FormSubmission.Longitude,
                            FormQuestions = i.FormSubmission.Form.FormQuestions
                            .Where(m => m.FormVersionId == i.FormSubmission.FormVersionId)
                            .OrderByDescending(o => o.FormVersionId)
                            .Select(k => new FormQuestionsModel
                            {
                                Id = k.Id,
                                FormId = k.FormId,
                                JSONQuestion = k.JSONQuestion,
                                FormVersionId = k.FormVersionId,
                                LanguageId = k.LanguageId
                            }).ToList(),
                        }).ToList();

                    //foreach (var item in formSubmissionList)
                    //{
                    //    db.formver
                    //    item.FormQuestions = 
                    //}

                    return formSubmissionList;
                }
                return formSubmissionList;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }

        //i-Witness Specific
        //Added by jay mistry 30-9-2016
        [Route("api/downloadSubmissions/{subscriberId}")]
        [HttpGet]
        public object ExportDSToExcel(int subscriberId)
        {
            var formAnswerList = db.FormSubmissions.Where(i => i.SubscriberId == subscriberId)
                .Select(i => new FormAnswerExport
                {
                    FormId = i.FormId,
                    FormName = i.Form.Name,
                    SubmissionId = i.Id,
                    SubscriberId = i.SubscriberId,
                    FolderName = i.FolderName,
                    FormAnswers = i.FormAnswers.Select(j => new FormAnswerModel
                    {
                        Id = j.Id,
                        Value = j.Value
                    }).AsEnumerable(),
                    administrative_area_level_1 = i.PostLocations.FirstOrDefault().administrative_area_level_1,
                    administrative_area_level_2 = i.PostLocations.FirstOrDefault().administrative_area_level_2,
                    administrative_area_level_3 = i.PostLocations.FirstOrDefault().administrative_area_level_3,
                    colloquial_area = i.PostLocations.FirstOrDefault().colloquial_area,
                    country = i.PostLocations.FirstOrDefault().country,
                    locality = i.PostLocations.FirstOrDefault().locality,
                    neighborhood = i.PostLocations.FirstOrDefault().neighborhood,
                    postal_code = i.PostLocations.FirstOrDefault().postal_code,
                    route = i.PostLocations.FirstOrDefault().route,
                    shortName = i.PostLocations.FirstOrDefault().shortName,
                    sublocality = i.PostLocations.FirstOrDefault().sublocality,
                    AppInfoID = i.AppInfoId
                }).ToList();

            DataTable table = new DataTable("Submissions");

            table.Columns.Add("#Id", typeof(int));
            table.Columns.Add("Category", typeof(string));
            table.Columns.Add("Issue/Appreciation", typeof(string));
            table.Columns.Add("Issue Type", typeof(string));
            table.Columns.Add("Description", typeof(string));
            table.Columns.Add("Images", typeof(string));

            table.Columns.Add("Street/Route", typeof(string));
            table.Columns.Add("Area", typeof(string));
            table.Columns.Add("country", typeof(string));
            table.Columns.Add("Postal code", typeof(string));

            table.Columns.Add("Sender's Name", typeof(string));
            table.Columns.Add("Email", typeof(string));
            table.Columns.Add("Phone Number", typeof(string));

            IEnumerable<AppUserInfo> appUserInfoList = db.AppUserInfoes.AsEnumerable();

            var appSettingModel = db.AppSettings.Where(i => i.SubscriberId == subscriberId && i.Key == Constants.FileUrl).FirstOrDefault();

            foreach (var item in formAnswerList)
            {
                AppUserInfo appUserModel = appUserInfoList.Where(i => i.AppInfoID == item.AppInfoID).FirstOrDefault();
                List<string> imageWithPath = new List<string>();
                string imagesStringPath = string.Empty;
                if (item.FormAnswers.Count() == 5 && item.FormAnswers.ToList()[4] != null && !string.IsNullOrEmpty(item.FormAnswers.ToList()[4].Value) && appSettingModel != null)
                {

                    foreach (string image in item.FormAnswers.ToList()[4].Value.Split(','))
                    {
                        if (!string.IsNullOrEmpty(image))
                        {
                            imageWithPath.Add(appSettingModel.Value + subscriberId + "/" + Constants.FormSubmissionFolder + item.FolderName + Constants.ImageFolder + image);
                        }
                    }
                    imagesStringPath = string.Join(", ", imageWithPath);
                }
                if (appUserModel != null && item.FormAnswers.Count() == 5)
                {

                    table.Rows.Add(item.SubmissionId,
                        item.FormName,
                        item.FormAnswers.ToList()[0].Value,
                        item.FormAnswers.ToList()[1].Value,
                        item.FormAnswers.ToList()[2].Value,
                        imagesStringPath,
                        item.route,
                        item.colloquial_area,
                        item.country,
                        item.postal_code,
                        appUserModel.Name,
                        appUserModel.Email,
                        appUserModel.PhoneNo);
                }
            }

            return CreateExcelFile(table, subscriberId);
        }

        public string CreateExcelFile(DataTable dt, int subscriberId)
        {
            try
            {
                using (ExcelPackage xp = new ExcelPackage())
                {
                    ExcelWorksheet ws = xp.Workbook.Worksheets.Add(dt.TableName);

                    int rowstart = 2;
                    int colstart = 2;
                    int rowend = rowstart;
                    int colend = colstart + dt.Columns.Count;

                    ws.Cells[rowstart, colstart, rowend, colend].Merge = true;
                    ws.Cells[rowstart, colstart, rowend, colend].Value = dt.TableName;
                    ws.Cells[rowstart, colstart, rowend, colend].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    ws.Cells[rowstart, colstart, rowend, colend].Style.Font.Bold = true;
                    ws.Cells[rowstart, colstart, rowend, colend].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    ws.Cells[rowstart, colstart, rowend, colend].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);

                    rowstart += 2;
                    rowend = rowstart + dt.Rows.Count;
                    ws.Cells[rowstart, colstart].LoadFromDataTable(dt, true);
                    int i = 1;
                    foreach (DataColumn dc in dt.Columns)
                    {
                        i++;
                        if (dc.DataType == typeof(decimal))
                            ws.Column(i).Style.Numberformat.Format = "#0.00";
                    }
                    ws.Cells[ws.Dimension.Address].AutoFitColumns();

                    ws.Cells[rowstart, colstart, rowend, colend].Style.Border.Top.Style =
                    ws.Cells[rowstart, colstart, rowend, colend].Style.Border.Bottom.Style =
                    ws.Cells[rowstart, colstart, rowend, colend].Style.Border.Left.Style =
                    ws.Cells[rowstart, colstart, rowend, colend].Style.Border.Right.Style = OfficeOpenXml.Style.ExcelBorderStyle.Thin;

                    string strFileName = dt.TableName; //+ "_" + DateTime.Now.ToShortDateString() + "_" + DateTime.Now.ToShortTimeString();

                    byte[] data = xp.GetAsByteArray();

                    var root = db.AppSettings.Where(s => s.SubscriberId == subscriberId && s.Key.ToLower() == Constants.FileRoot.ToLower()).FirstOrDefault().Value;
                    root = root + subscriberId + Constants.DOCUMENTS_FOLDER;

                    if (!Directory.Exists(root))
                    {
                        Directory.CreateDirectory(root);
                    }
                    string path = root + strFileName + ".xlsx";
                    File.WriteAllBytes(path, data);

                    var fileUrl = db.AppSettings.Where(s => s.SubscriberId == subscriberId && s.Key.ToLower() == Constants.FileUrl.ToLower()).FirstOrDefault().Value;
                    fileUrl = fileUrl + subscriberId + Constants.DOCUMENTS_FOLDER + strFileName + ".xlsx";

                    return fileUrl;

                    //HttpContext.Current.Response.AddHeader("content-disposition", "attachment;filename=" + Regex.Replace(strFileName, @"[^\wa-zA-Z0-9+]", "_") + ".xlsx");
                    //HttpContext.Current.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    //HttpContext.Current.Response.BinaryWrite(xp.GetAsByteArray());
                    //HttpContext.Current.Response.End();
                }
            }
            catch
            {
                throw;
            }
        }

        #endregion
    }

    public class RootObjectForAnswers
    {
        public int SubscriberId { get; set; }
        public int FormId { get; set; }
        public int LanguageId { get; set; }
        public int VersionId { get; set; }
        public string LatLong { get; set; }
        public string DeviceId { get; set; }
        public long EmployeeId { get; set; }
        public List<FormAnswerModel> FormAnswers { get; set; }
    }

    public class BultFormSubmissionId
    {
        public string Id { get; set; }
    }

    public class FormAnswerWithIdModel
    {
        public long Id { get; set; }
        public string Answer { get; set; }
        public long QuestionId { get; set; }
        public int ElementType { get; set; }
        public long FormSubmissionId { get; set; }//Added By Hiren
    }

    public class FormQuestionsGroupByLanguage
    {
        public int languageId { get; set; }
        public bool published { get; set; }
        public List<CommonFormQuestionsModel> formQuestions { get; set; }
        public List<FormAnswerWithIdModel> formAnswers { get; set; }
    }
}