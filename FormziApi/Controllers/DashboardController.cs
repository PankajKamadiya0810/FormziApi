using FormziApi.Database;
using FormziApi.Helper;
using FormziApi.Models;
using FormziApi.Services;
using Newtonsoft.Json;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.WebPages.Html;

namespace FormziApi.Controllers
{
    //[Authorize]
    [EnableCors("*", "*", "*")]
    public class DashboardController : ApiController
    {
        #region Fields

        private FormziEntities db = new FormziEntities();
        private LogProvider lp = new LogProvider("DashboardController");
        private FormQuestionService _formQuestionService = new FormQuestionService();

        #endregion

        #region Methods

        [Route("api/dashboardInfo/{subscriberId}")]
        [HttpGet]
        public object DashboardInfo(int subscriberId)
        {
            try
            {
                DateTime today = DateTime.Today;
                DateTime week = DateTime.UtcNow.AddDays(-7);
                DateTime month = DateTime.UtcNow.AddMonths(-1);
                DateTime year = DateTime.UtcNow.AddYears(-1);

                IEnumerable<AppUserInfo> appUserInfoList = db.AppUserInfoes.AsEnumerable();

                var data = new
                {
                    Forms = db.Forms.Where(i => i.SubscriberId == subscriberId && i.IsActive && !i.IsDeleted).Select(i => new
                    {
                        FormId = i.Id,
                        Name = i.Name,
                        Data = new
                        {
                            Today = i.FormSubmissions.Where(j => DateTime.Compare(j.SubmittedOn, today) >= 0).Count(),
                            Week = i.FormSubmissions.Where(j => DateTime.Compare(j.SubmittedOn, week) >= 0).Count(),
                            Month = i.FormSubmissions.Where(j => DateTime.Compare(j.SubmittedOn, month) >= 0).Count(),
                            Year = i.FormSubmissions.Where(j => DateTime.Compare(j.SubmittedOn, year) >= 0).Count(),
                            Total = i.FormSubmissions.Count()
                        }
                    }).ToList(),
                    Users = db.FormSubmissions.Where(j => j.SubscriberId == subscriberId).GroupBy(g => g.AppInfoId)
                            .Select(k => new
                            {
                                Name = appUserInfoList.Where(l => l.AppInfoID == k.Key).FirstOrDefault().Name,
                                Email = appUserInfoList.Where(l => l.AppInfoID == k.Key).FirstOrDefault().Email,
                                PhoneNumber = appUserInfoList.Where(l => l.AppInfoID == k.Key).FirstOrDefault().PhoneNo,
                                Count = k.Count()
                            }).OrderByDescending(o => o.Count).Take(5),
                    City = db.PostLocations.GroupBy(g => g.locality)
                        .Select(k => new
                        {
                            administrative_area_level_1 = k.FirstOrDefault().administrative_area_level_1,
                            administrative_area_level_2 = k.FirstOrDefault().administrative_area_level_2,
                            administrative_area_level_3 = k.FirstOrDefault().administrative_area_level_3,
                            colloquial_area = k.FirstOrDefault().colloquial_area,
                            country = k.FirstOrDefault().country,
                            locality = k.FirstOrDefault().locality,
                            neighborhood = k.FirstOrDefault().neighborhood,
                            postal_code = k.FirstOrDefault().postal_code,
                            route = k.FirstOrDefault().route,
                            shortName = k.FirstOrDefault().shortName,
                            sublocality = k.FirstOrDefault().sublocality,
                            Count = k.Count()
                        }).OrderByDescending(o => o.Count).Take(5),
                    TotalUsers = db.AppUserInfoes.Count()
                };
                return data;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return false;
            }
        }

        //[FromBody]
        [Route("api/addDashboard")]
        [HttpPost]
        public object AddDashboard([FromBody]string data)
        {
            try
            {
                //string result = await Request.Content.ReadAsStringAsync();

                if (string.IsNullOrEmpty(data))
                {
                    return false;
                }

                //var test = JsonConvert.DeserializeObject<ChartModel>(data);
                ChartModel model = JsonConvert.DeserializeObject<ChartModel>(data);

                if (model != null)
                {
                    Dashboard dashboardDBModel = db.Dashboards.Where(i => i.FormId == model.FormID && i.FormVersionId == model.FormVersionId).FirstOrDefault();
                    DateTime currentDateTime = Common.GetDateTime(db);
                    if (dashboardDBModel != null)
                    {
                        dashboardDBModel.FormVersionId = model.FormVersionId;
                        dashboardDBModel.DashboardObjs = JsonConvert.SerializeObject(model.DashboardObjs);
                        dashboardDBModel.SubscriberId = model.SubscriberId;
                        dashboardDBModel.UpdatedBy = 1; // item.CreatedBy;
                        dashboardDBModel.UpdatedOn = currentDateTime;

                        db.Entry(dashboardDBModel).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        return true;
                    }
                    else
                    {
                        Dashboard dbModel = new Dashboard();
                        dbModel.CreatedBy = 1; // item.CreatedBy;
                        dbModel.CreatedOn = currentDateTime;
                        dbModel.DashboardObjs = JsonConvert.SerializeObject(model.DashboardObjs);
                        dbModel.FormId = model.FormID;
                        dbModel.FormVersionId = model.FormVersionId;
                        dbModel.IsActive = true;
                        dbModel.IsDeleted = false;
                        dbModel.SubscriberId = model.SubscriberId;
                        dbModel.UpdatedBy = 1; // item.CreatedBy;
                        dbModel.UpdatedOn = currentDateTime;

                        db.Dashboards.Add(dbModel);
                        db.SaveChanges();

                        return true;
                    }
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

        [Route("api/getDashboard")]
        [HttpGet]
        public object GetDashboard(int formId, int subscriberId, int VersionId = 0)
        {
            try
            {
                DateTime lastYear = DateTime.Now.AddYears(-1);
                DateTime currentYear = DateTime.Now;

                if (formId > 0 && subscriberId > 0)
                {
                    if (VersionId == 0)
                    {
                        VersionId = db.Dashboards.Where(f => f.FormId == formId).OrderByDescending(o => o.FormVersionId).FirstOrDefault().FormVersionId;
                        //db.FormSubmissions.Where(f => f.FormId == FormId && !f.IsDeleted)
                        //            .AsEnumerable()
                        //            .OrderByDescending(x => x.FormVersion.Id)
                        //            .Select(i => i.FormVersion.Id).Distinct().FirstOrDefault();
                    }

                    Dashboard model = db.Dashboards.Where(i => i.SubscriberId == subscriberId && i.FormId == formId && i.FormVersionId == VersionId).AsEnumerable()
                        .Select(i => new Dashboard
                        {

                            CreatedBy = i.CreatedBy,
                            CreatedOn = i.CreatedOn,
                            DashboardObjs = i.DashboardObjs,
                            FormId = i.FormId,
                            FormVersionId = i.FormVersionId,
                            Id = i.Id,
                            IsActive = i.IsActive,
                            IsDeleted = i.IsDeleted,
                            Page = i.Page,
                            SubscriberId = i.SubscriberId,
                            UpdatedBy = i.CreatedBy,
                            UpdatedOn = i.UpdatedOn
                        }).FirstOrDefault();

                    return model;
                }
                return null;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }

        [Route("api/formversionsForDashboard/{FormId}/{IsEditMode}")]
        [HttpGet]
        public object GetDashboardByFormVersions(long FormId, string IsEditMode)
        {
            try
            {
                if (FormId == 0)
                {
                    return null;
                }
                List<FormVersionModel> formVersionList = new List<FormVersionModel>();

                var formVersionIds = db.FormVersions.Where(f => f.FormId == FormId)
                    .AsEnumerable()
                    .Select(i => i.Id).ToList();

                var formVersionIdsDashboard = db.Dashboards.Where(f => f.FormId == FormId).AsEnumerable().Select(i => i.FormVersionId).ToList();

                if (IsEditMode == "e")
                {
                    formVersionList = db.FormVersions.Where(x => formVersionIdsDashboard.Contains(x.Id))
                    .Select(i => new FormVersionModel
                    {
                        Id = i.Id,
                        FormId = i.FormId,
                        Version = i.Version,
                        CreatedOn = i.CreatedOn,
                        CreatedBy = i.CreatedBy
                    }).OrderByDescending(o => o.Id).ToList();
                }
                else if (IsEditMode == "a")
                {
                    var exceptVersionIds = formVersionIds.Except(formVersionIdsDashboard);

                    formVersionList = db.FormVersions.Where(x => exceptVersionIds.Contains(x.Id))
                        .Select(i => new FormVersionModel
                        {
                            Id = i.Id,
                            FormId = i.FormId,
                            Version = i.Version,
                            CreatedOn = i.CreatedOn,
                            CreatedBy = i.CreatedBy
                        }).OrderByDescending(o => o.Id).ToList();
                }
                else
                {
                    formVersionList = db.FormVersions.Where(x => formVersionIds.Contains(x.Id))
                    .Select(i => new FormVersionModel
                    {
                        Id = i.Id,
                        FormId = i.FormId,
                        Version = i.Version,
                        CreatedOn = i.CreatedOn,
                        CreatedBy = i.CreatedBy
                    }).OrderByDescending(o => o.Id).ToList();
                }
                return formVersionList;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }

        [Route("api/getChartAnswers")]
        [HttpGet]
        public object GetChartAnswers(int formId, int subscriberId, string currentDate, int formVersionId)
        {
            try
            {
                if (formId > 0)
                {
                    if (formVersionId == 0)
                    {
                        formVersionId = db.FormVersions.Where(i => i.FormId == formId).OrderByDescending(o => o.Id).FirstOrDefault().Id;
                        //db.FormSubmissions.Where(f => f.FormId == formId && !f.IsDeleted)
                        //            .AsEnumerable()
                        //            .OrderByDescending(x => x.FormVersion.Id)
                        //            .Select(i => i.FormVersion.Id).Distinct().FirstOrDefault();
                    }

                    Dashboard model = db.Dashboards.Where(i => i.SubscriberId == subscriberId && i.FormId == formId && i.FormVersionId == formVersionId).FirstOrDefault();

                    if (model != null && !string.IsNullOrEmpty(model.DashboardObjs))
                    {
                        var test = model.DashboardObjs.Replace(" ", String.Empty);

                        test = System.Text.RegularExpressions.Regex.Replace(test, @"\s+", "");

                        List<DashboardObjs> dashboardObj = JsonConvert.DeserializeObject<List<DashboardObjs>>(model.DashboardObjs);

                        foreach (var item in dashboardObj)
                        {
                            if (item.component == "PIE")
                            {
                                Dictionary<string, int> pieAnswer = new Dictionary<string, int>();

                                var testdata = db.FormAnswers
                                           .Where(i => i.FormSubmission.FormId == model.FormId && !i.FormSubmission.IsDeleted && i.FormSubmission.FormVersionId == model.FormVersionId)
                                           .GroupBy(g => new { g.FormSubmissionId })
                                           .Select(i => new
                                           {
                                               x = i.Where(j => j.FormQuestionId == item.chart.xaxis.id).FirstOrDefault().Value,
                                               y = i.Where(j => j.FormQuestionId == item.chart.xaxis.id).Count()
                                           }).ToList();

                                var finalResult = testdata.Where(i => i.x != null && i.y != null).GroupBy(g => g.x)
                                            .Select(i => new
                                            {
                                                x = i.Key,
                                                y = i.Count()
                                            }).ToList();

                                foreach (var data in finalResult)
                                {
                                    pieAnswer.Add(data.x, data.y);
                                }

                                item.chartData = pieAnswer;

                            }
                            else if (item.component == "FUNNEL")
                            {
                                Dictionary<string, int> funnelAnswer = new Dictionary<string, int>();
                                foreach (var x in item.chart.xaxis.options)
                                {
                                    int cnt = db.FormAnswers.Where(i => i.FormSubmission.FormId == model.FormId && !i.FormSubmission.IsDeleted && i.FormQuestionId == item.chart.xaxis.id && i.Value == x).Count();
                                    if (cnt > 0)
                                    {
                                        funnelAnswer.Add(x, cnt);
                                    }
                                }
                                item.chartData = funnelAnswer;

                            }
                            else //For AREA, BAR and COLUMN chart
                            {

                                #region For Data table - Not in use

                                //DataTable pieDataTable = new DataTable("Test"); // Data table for excel download
                                //DataRow row;

                                //List<long> ids = db.FormAnswers
                                //           .Where(i => i.FormSubmission.FormId == model.FormId && !i.FormSubmission.IsDeleted && i.FormSubmission.FormVersionId == model.FormVersionId)
                                //           .Select(i => i.FormQuestionId).Distinct().ToList();

                                //foreach (var id in ids)
                                //{
                                //    pieDataTable.Columns.Add(id.ToString(), typeof(string));
                                //}

                                //List<DashboardAnswersModel> data = db.FormAnswers
                                //           .Where(i => i.FormSubmission.FormId == model.FormId && i.FormSubmission.FormVersionId == model.FormVersionId)
                                //           .AsEnumerable()
                                //           .GroupBy(g => g.FormSubmissionId)
                                //           .Select(i => new DashboardAnswersModel
                                //           {
                                //               Id = i.Key.ToString(),
                                //               Data = i.Select(j => new SelectListItem
                                //               {
                                //                   Text = j.Value,
                                //                   Value = j.FormQuestionId.ToString()
                                //               }).ToList()
                                //           }).ToList();



                                //foreach (var x in data)
                                //{
                                //    row = pieDataTable.NewRow();
                                //    foreach (var y in x.Data)
                                //    {
                                //        row[y.Value] = y.Text;
                                //    }
                                //    pieDataTable.Rows.Add(row);
                                //}

                                //columnAnswer = new List<object>(); 

                                #endregion
                                List<object> columnAnswer = new List<object>();
                                SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["FormziEntitiesSP"].ConnectionString);
                                connection.Open();
                                SqlCommand sqlCommand = new SqlCommand("[dbo].[GetChartDataByFormID]", connection);
                                sqlCommand.CommandType = CommandType.StoredProcedure;
                                DataTable dt = new DataTable();
                                sqlCommand.Parameters.AddWithValue("@fromId", model.FormId);
                                sqlCommand.Parameters.AddWithValue("@versionId", model.FormVersionId);
                                sqlCommand.Parameters.AddWithValue("@xAxis", item.component == "TABLE" ? 0 : item.chart.xaxis.id);
                                sqlCommand.Parameters.AddWithValue("@yAxis", item.component == "TABLE" ? 0 : item.chart.yaxis.id);
                                var str = string.Empty;
                                if (item.component == "TABLE")
                                {
                                    int counter = 0;
                                    foreach (var itemstr in item.chart.columns)
                                    {
                                        if (counter > 0)
                                            str = str + "," + itemstr.id;
                                        else
                                            str = itemstr.id.ToString();

                                        counter++;
                                    }
                                    sqlCommand.Parameters.AddWithValue("@Columns", str);
                                }
                                sqlCommand.ExecuteNonQuery();
                                SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
                                dt.Load(sqlDataReader);
                                connection.Close();

                                if (item.component == "TABLE")
                                {
                                    List<long> ids = new List<long>();
                                    if (!string.IsNullOrEmpty(str))
                                    {
                                        foreach (var Iids in str.Split(','))
                                        {
                                            ids.Add(Convert.ToInt64(Iids));
                                        }
                                    }
                                    List<FormQuestion> fqList = db.FormQuestions.Where(i => i.FormId == model.FormId && i.FormVersionId == model.FormVersionId && i.FormToolsId > 1 && ids.Contains(i.Id))
                    .AsEnumerable().Select(i => new FormQuestion { Id = i.Id, Question = i.Question, FormToolsId = i.FormToolsId }).ToList();

                                    if (fqList.Count > 0)
                                    {
                                        foreach (var itemQ in fqList)
                                        {
                                            dt.Columns[itemQ.Id.ToString()].ColumnName = itemQ.Question;
                                        }
                                    }

                                    var dynamicDt = new List<dynamic>();
                                    foreach (DataRow row in dt.Rows)
                                    {
                                        dynamic dyn = new ExpandoObject();
                                        dyn = row.ItemArray;
                                        dynamicDt.Add(dyn);
                                        //foreach (DataColumn col in dt.Columns)
                                        //{
                                        //    var dic = (IDictionary<string, object>)dyn;
                                        //    dic[col.ColumnName] = row[col];
                                        //}
                                    }
                                    item.chartData = dt;
                                }
                                else
                                {
                                    foreach (var data in dt.AsEnumerable())//.Select(i => new
                                    {
                                        columnAnswer.Add(new
                                        {
                                            x = data.ItemArray[0].ToString(),
                                            y = Convert.ToDecimal(data.ItemArray[1].ToString())
                                        });
                                    }
                                    item.chartData = columnAnswer;
                                }


                                #region commented
                                //var testdata = db.FormAnswers
                                //           .Where(i => i.FormSubmission.FormId == model.FormId && !i.FormSubmission.IsDeleted && i.FormSubmission.FormVersionId == model.FormVersionId)
                                //           .GroupBy(g => new { g.FormSubmissionId })
                                //           .Select(i => new
                                //           {
                                //               x = i.Where(j => j.FormQuestionId == item.chart.xaxis.id).FirstOrDefault().Value,
                                //               y = i.Where(j => j.FormQuestionId == item.chart.yaxis.id).FirstOrDefault().Value
                                //           }).ToList();

                                //foreach (var x in item.chart.xaxis.options)
                                //{
                                //    List<int> strXAxisAns = new List<int>();

                                //    foreach (var y in item.chart.yaxis.options)
                                //    {
                                //        if (testdata == null)
                                //        {
                                //            strXAxisAns.Add(0);
                                //            continue;
                                //        }
                                //        var finalResult = testdata.Where(i => i.x != null && i.y != null).GroupBy(g => g.x)
                                //            .Select(i => new
                                //            {
                                //                x = i.Key,
                                //                y = i.Where(j => j.x == x && j.y == y).Count()

                                //            }).ToList();

                                //        if (finalResult.Where(i => i.x == x).FirstOrDefault() != null)
                                //        {
                                //            strXAxisAns.Add(finalResult.Where(i => i.x == x).FirstOrDefault().y);
                                //        }
                                //        else
                                //        {
                                //            strXAxisAns.Add(0);
                                //        }
                                //    }

                                //columnAnswer.Add(new
                                //{
                                //    x = x,
                                //    y = strXAxisAns
                                //});
                                //item.chartData = columnAnswer;
                                // }
                                #endregion
                            }
                        }

                        ChartModel returnData = new ChartModel();
                        returnData.DashboardObjs = dashboardObj;
                        returnData.FormID = model.FormId;
                        returnData.FormVersionId = model.FormVersionId;
                        returnData.ID = model.Id;
                        returnData.IsActive = model.IsActive;
                        returnData.IsDeleted = model.IsDeleted;
                        returnData.FormName = model.Form.Name;

                        int year = DateTime.Today.Year;
                        int month = DateTime.Today.Month + 1;
                        int day = DateTime.Now.Day;

                        if (!string.IsNullOrEmpty(currentDate))
                        {
                            year = Convert.ToDateTime(currentDate).ToUniversalTime().Year;
                            month = Convert.ToDateTime(currentDate).ToUniversalTime().Month;
                            day = Convert.ToDateTime(currentDate).ToUniversalTime().Day;
                        }

                        DateTime today = !string.IsNullOrEmpty(currentDate) ? new DateTime(year, month, day).AddHours(-5) : DateTime.Today;

                        DateTime _week = today.AddDays(-7);
                        DateTime _month = today.AddMonths(-1);
                        DateTime _year = today.AddYears(-1);

                        //FormVersion formVersionDbModel = db.FormVersions.Where(i => i.FormId == formId).OrderByDescending(o => o.Id).FirstOrDefault();

                        //int formVersionId = 1;
                        //if (formVersionDbModel != null)
                        //{
                        //    formVersionId = formVersionDbModel.Id;
                        //}

                        returnData.SubmissionCount = db.Forms.Where(i => i.Id == formId && i.SubscriberId == subscriberId && i.IsActive && !i.IsDeleted).Select(i => new
                        {
                            FormId = i.Id,
                            Name = i.Name,
                            Data = new
                            {
                                Today = i.FormSubmissions.Where(j => j.FormVersionId == formVersionId && DateTime.Compare(j.SubmittedOn, today) >= 0).Count(),
                                Week = i.FormSubmissions.Where(j => j.FormVersionId == formVersionId && DateTime.Compare(j.SubmittedOn, _week) >= 0).Count(),
                                Month = i.FormSubmissions.Where(j => j.FormVersionId == formVersionId && DateTime.Compare(j.SubmittedOn, _month) >= 0).Count(),
                                Year = i.FormSubmissions.Where(j => j.FormVersionId == formVersionId && DateTime.Compare(j.SubmittedOn, _year) >= 0).Count(),
                                Total = i.FormSubmissions.Where(j => j.FormVersionId == formVersionId).Count()
                            }
                        }).ToList();

                        return returnData;
                    }
                    return null;
                }
                return null;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }

        [Route("api/ActiveDashboard")]
        [HttpPost]
        public object ActiveDashboard(int formId, int subscriberId, bool isActive)
        {
            try
            {
                if (formId > 0 && subscriberId > 0)
                {
                    Dashboard model = db.Dashboards.Where(i => i.FormId == formId && i.SubscriberId == subscriberId).FirstOrDefault();

                    model.IsActive = isActive;
                    db.Entry(model).State = System.Data.Entity.EntityState.Modified;
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

        [Route("api/updateDashboard")]
        [HttpPost]
        public object UpdateDashboard([FromBody]string data)
        {
            try
            {
                //string result = await Request.Content.ReadAsStringAsync();

                if (string.IsNullOrEmpty(data))
                {
                    return false;
                }

                ChartModel model = JsonConvert.DeserializeObject<ChartModel>(data);
                if (model == null) return false;

                //List<DashboardObjs> model = JsonConvert.DeserializeObject<List<DashboardObjs>>(data);

                FormVersion formVersionDBModel = db.FormVersions.Where(i => i.FormId == model.FormID).OrderByDescending(o => o.Id).FirstOrDefault();
                if (formVersionDBModel == null) return false;

                Dashboard dashboardDBModel = db.Dashboards.Where(i => i.FormId == model.FormID).FirstOrDefault();

                if (dashboardDBModel != null)
                {
                    dashboardDBModel.DashboardObjs = JsonConvert.SerializeObject(model.DashboardObjs);
                    dashboardDBModel.UpdatedBy = 1; // item.CreatedBy;
                    dashboardDBModel.UpdatedOn = Common.GetDateTime(db);

                    db.Entry(dashboardDBModel).State = System.Data.Entity.EntityState.Modified;
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
                    int colend = colstart + dt.Columns.Count - 1;

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

        [Route("api/deleteDashboard")]
        [HttpGet]
        public object DeleteDashboard(int formId, int subscriberId, int formVersionId)
        {
            try
            {
                if (formId > 0 && subscriberId > 0 && formVersionId > 0)
                {
                    Dashboard model = db.Dashboards.Where(i => i.FormId == formId && i.SubscriberId == subscriberId && i.FormVersionId == formVersionId).FirstOrDefault();
                    if (model != null)
                    {
                        db.Dashboards.Remove(model);
                        db.SaveChanges();
                        return true;
                    }
                    else return false;
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
        /// i-Witness Specific
        /// </summary>
        /// <param name="formId"></param>
        /// <param name="index"></param>
        /// <param name="subscriberId"></param>
        /// <returns></returns>
        [Route("api/downloadChartData")]
        [HttpGet]
        public object DownloadChartData(int formId, int index, int subscriberId)
        {
            try
            {
                if (formId > 0)
                {
                    Dashboard model = db.Dashboards.Where(i => i.SubscriberId == subscriberId && i.FormId == formId).FirstOrDefault();

                    if (model != null && !string.IsNullOrEmpty(model.DashboardObjs))
                    {
                        var test = model.DashboardObjs.Replace(" ", String.Empty);

                        test = System.Text.RegularExpressions.Regex.Replace(test, @"\s+", "");

                        List<DashboardObjs> dashboardObj = JsonConvert.DeserializeObject<List<DashboardObjs>>(model.DashboardObjs);

                        List<object> columnAnswer = new List<object>();
                        Dictionary<string, int> pieAnswer = new Dictionary<string, int>();
                        Dictionary<string, int> funnelAnswer = new Dictionary<string, int>();

                        foreach (var item in dashboardObj)
                        {
                            if (item.component == "PIE")
                            {
                                DataTable pieDataTable = new DataTable(item.component + "_Chart_Data_Index_" + item.index); // Data table for excel download

                                pieDataTable.Columns.Add(item.chart.xaxis.label, typeof(string));
                                pieDataTable.Columns.Add("Total", typeof(string));
                                foreach (var x in item.chart.xaxis.options)
                                {
                                    int cnt = db.FormAnswers.Where(i => i.FormSubmission.FormId == model.FormId && i.FormQuestionId == item.chart.xaxis.id && i.Value == x).Count();
                                    pieAnswer.Add(x, cnt);
                                    pieDataTable.Rows.Add(x, cnt.ToString());
                                }
                                item.chartData = pieAnswer;
                                if (index == item.index)
                                    return CreateExcelFile(pieDataTable, subscriberId);
                            }
                            else if (item.component == "FUNNEL")
                            {
                                DataTable funnelDataTable = new DataTable(item.component + "_Chart_Data_Index_" + item.index); // Data table for excel download

                                funnelDataTable.Columns.Add(item.chart.xaxis.label, typeof(string));
                                funnelDataTable.Columns.Add("Total", typeof(string));

                                foreach (var x in item.chart.xaxis.options)
                                {
                                    int cnt = db.FormAnswers.Where(i => i.FormSubmission.FormId == model.FormId && i.FormQuestionId == item.chart.xaxis.id && i.Value == x).Count();
                                    funnelAnswer.Add(x, cnt);
                                    funnelDataTable.Rows.Add(x, cnt.ToString());
                                }
                                item.chartData = funnelAnswer;
                                if (index == item.index)
                                    return CreateExcelFile(funnelDataTable, subscriberId);

                            }
                            else
                            {
                                DataTable areaDataTable = new DataTable(item.component + "_Chart_Data_Index_" + item.index); // Data table for excel download
                                DataRow row;

                                columnAnswer = new List<object>();

                                List<FormQuestion> fqList = db.FormQuestions.Where(i => i.FormId == formId && i.FormVersionId == model.FormVersionId && i.FormToolsId > 1 && i.FormToolsId != 11 && i.FormToolsId != 22 && i.FormToolsId != 4 && i.FormToolsId != 12)
                    .AsEnumerable().Select(i => new FormQuestion { Id = i.Id, Question = i.Question, FormToolsId = i.FormToolsId }).ToList();

                                SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["FormziEntitiesSP"].ConnectionString);
                                connection.Open();
                                SqlCommand sqlCommand = new SqlCommand("[dbo].[GetChartDataByFormID]", connection);
                                sqlCommand.CommandType = CommandType.StoredProcedure;
                                DataTable dt = new DataTable();
                                sqlCommand.Parameters.AddWithValue("@fromId", model.FormId);
                                sqlCommand.Parameters.AddWithValue("@versionId", model.FormVersionId);
                                sqlCommand.Parameters.AddWithValue("@xAxis", item.chart.xaxis.id);
                                sqlCommand.Parameters.AddWithValue("@yAxis", item.chart.yaxis.id);
                                sqlCommand.ExecuteNonQuery();
                                SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
                                dt.Load(sqlDataReader);
                                connection.Close();

                                foreach (var data in dt.AsEnumerable())//.Select(i => new
                                {
                                    columnAnswer.Add(new
                                    {
                                        x = data.ItemArray[0].ToString(),
                                        y = Convert.ToDecimal(data.ItemArray[1].ToString())
                                    });
                                }

                                item.chartData = columnAnswer;

                                dt.Columns.RemoveAt(2);

                                foreach (DataColumn column in dt.Columns)
                                {
                                    FormQuestion formQuestion = fqList.Where(i => i.Id.ToString() == column.ColumnName).FirstOrDefault();
                                    bool flag = false;
                                    if (formQuestion != null)
                                        flag = formQuestion.FormToolsId == 3 || formQuestion.FormToolsId == 20 || formQuestion.FormToolsId == 21;
                                    if (flag)
                                        areaDataTable.Columns.Add(formQuestion.Question, typeof(Decimal));
                                    else
                                        areaDataTable.Columns.Add(formQuestion == null ? column.ColumnName : formQuestion.Question, typeof(string));
                                }

                                foreach (DataRow rows in dt.Rows)
                                    areaDataTable.LoadDataRow(rows.ItemArray, true);

                                //areaDataTable.Columns.Add(item.chart.xaxis.label, typeof(string));
                                //for (int i = 0; i < item.chart.yaxis.options.Count; i++)
                                //{
                                //    areaDataTable.Columns.Add(item.chart.yaxis.options[i], typeof(string));
                                //}

                                //foreach (var x in item.chart.xaxis.options)
                                //{
                                //    List<int> strXAxisAns = new List<int>();
                                //    row = areaDataTable.NewRow();
                                //    row[item.chart.xaxis.label] = x;
                                //    foreach (var y in item.chart.yaxis.options)
                                //    {
                                //        if (testdata == null)
                                //        {
                                //            strXAxisAns.Add(0);
                                //            continue;
                                //        }
                                //        var finalResult = testdata.Where(i => i.x != null && i.y != null).GroupBy(g => g.x)
                                //            .Select(i => new
                                //            {
                                //                x = i.Key,
                                //                y = i.Where(j => j.x == x && j.y == y).Count()

                                //            }).ToList();

                                //        if (finalResult.Where(i => i.x == x).FirstOrDefault() != null)
                                //        {
                                //            strXAxisAns.Add(finalResult.Where(i => i.x == x).FirstOrDefault().y);
                                //            row[y] = finalResult.Where(i => i.x == x).FirstOrDefault().y.ToString();

                                //            //table.Rows.Add(x, y, finalResult.Where(i => i.x == x).FirstOrDefault().y.ToString());
                                //        }
                                //        else
                                //        {
                                //            strXAxisAns.Add(0);
                                //            row[y] = 0;
                                //        }
                                //    }

                                //    columnAnswer.Add(new
                                //    {
                                //        x = x,
                                //        y = strXAxisAns
                                //    });
                                //    item.chartData = columnAnswer;

                                //    areaDataTable.Rows.Add(row);
                                //}
                                if (index == item.index)
                                    return CreateExcelFile(areaDataTable, subscriberId);
                            }
                        }

                        ChartModel returnData = new ChartModel();
                        returnData.DashboardObjs = dashboardObj;
                        returnData.FormID = model.FormId;
                        returnData.FormVersionId = model.FormVersionId;
                        returnData.ID = model.Id;
                        returnData.IsActive = model.IsActive;
                        returnData.IsDeleted = model.IsDeleted;


                        DateTime today = DateTime.UtcNow;
                        DateTime week = DateTime.UtcNow.AddDays(-7);
                        DateTime month = DateTime.UtcNow.AddMonths(-1);
                        DateTime year = DateTime.UtcNow.AddYears(-1);

                        returnData.SubmissionCount = db.Forms.Where(i => i.Id == formId && i.SubscriberId == subscriberId && i.IsActive && !i.IsDeleted).Select(i => new
                        {
                            FormId = i.Id,
                            Name = i.Name,
                            Data = new
                            {
                                Today = i.FormSubmissions.Where(j => DateTime.Compare(j.SubmittedOn, today) >= 0).Count(),
                                Week = i.FormSubmissions.Where(j => DateTime.Compare(j.SubmittedOn, week) >= 0).Count(),
                                Month = i.FormSubmissions.Where(j => DateTime.Compare(j.SubmittedOn, month) >= 0).Count(),
                                Year = i.FormSubmissions.Where(j => DateTime.Compare(j.SubmittedOn, year) >= 0).Count(),
                                Total = i.FormSubmissions.Count()
                            }
                        }).ToList();

                        return returnData;
                    }
                    return null;
                }
                return null;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return false;
            }
        }

        #endregion

        /// <summary>
        /// Download excel. Stored Procedure is used to fetch data.
        /// </summary>
        /// <param name="formId">1</param>
        /// <param name="formVersionId">1</param>
        /// <param name="appLoginId">1</param>
        /// <returns>download the excel file based on permission</returns>
        [Route("api/downloadFormSubmissionExcel")]
        [HttpGet]
        public object downloadFormSubmissionExcel(int formId, int formVersionId, int appLoginId)
        {
            try
            {
                Form formDBModel = db.Forms.Where(i => i.Id == formId).FirstOrDefault();
                if (formDBModel == null)
                {
                    return null;
                }
                if (formVersionId == 0)
                {
                    formVersionId = db.FormVersions.Where(i => i.FormId == formId).OrderByDescending(o => o.Id).FirstOrDefault().Id;
                }
                List<FormQuestion> fqList = db.FormQuestions.Where(i => i.FormId == formId && i.FormVersionId == formVersionId && i.FormToolsId > 1 && i.FormToolsId != 22 && i.FormToolsId != 4 && i.FormToolsId != 12).AsEnumerable().Select(i => new FormQuestion { Id = i.Id, Question = i.Question, FormToolsId = i.FormToolsId, JSONQuestion = i.JSONQuestion, ParentQuestionId = i.ParentQuestionId }).ToList();
                string strcon = ConfigurationManager.ConnectionStrings["FormziEntitiesSP"].ConnectionString;
                SqlConnection DbConnection = new SqlConnection(strcon);
                DbConnection.Open();
                SqlCommand command = new SqlCommand("[dbo].[usp_GetFormSubmissionDetailByFormID]", DbConnection);
                command.CommandType = CommandType.StoredProcedure;

                //create type table
                DataTable tempTable = new DataTable(formDBModel.Name.Replace(" ", "-"));
                DataTable dt = new DataTable(formDBModel.Name.Replace(" ", "-"));
                SqlParameter parameter = command.Parameters.AddWithValue("@subscriberId", formDBModel.SubscriberId);//Added By Hiren 23-11-2017
                parameter = command.Parameters.AddWithValue("@fromId", formId);
                parameter = command.Parameters.AddWithValue("@versionId", formVersionId);
                parameter = command.Parameters.AddWithValue("@IsHtml", 0);
                parameter = command.Parameters.AddWithValue("@Page", 0);
                parameter = command.Parameters.AddWithValue("@RecsPerPage", 0);
                parameter = command.Parameters.AddWithValue("@Path", null);
                parameter = command.Parameters.AddWithValue("@AppLoginId", appLoginId);
                command.ExecuteNonQuery();
                SqlDataReader reader = command.ExecuteReader();
                tempTable.Columns.Add("Report No.", typeof(int));
                dt.Columns.Add("Report No.", typeof(int));

                //Added By Hiren 27-11-2017
                Employee empDetails = db.Employees.Where(e => e.AppLoginId == appLoginId).FirstOrDefault();
                Form formDetails = db.Forms.Where(f => f.Id == formId).FirstOrDefault();
                List<CommonFormQuestionsModel> cfql = new List<CommonFormQuestionsModel>();
                if (empDetails != null && empDetails.SystemRoleId == 0)
                {
                    cfql = _formQuestionService.GetRoleBasedFormQuestions(formId, formVersionId, appLoginId, fqList);
                }
                foreach (var item in fqList)
                {
                    FormQuestion fqModel = fqList.Where(i => i.Id == item.Id).FirstOrDefault();
                    dynamic question = JsonConvert.DeserializeObject<object>(fqModel.JSONQuestion);
                    string indexValue = Convert.ToString(question["index"]);
                    if (fqModel.FormToolsId != Convert.ToInt16(Constants.ElementType.Section))
                    {
                        bool isNum = false;
                        if (fqModel != null)
                        {
                            isNum = fqModel.FormToolsId == 3 || fqModel.FormToolsId == 20 || fqModel.FormToolsId == 21;
                        }
                        if (isNum)
                        {
                            tempTable.Columns.Add(item.Id.ToString(), typeof(decimal));
                            dt.Columns.Add(item.Question + '_' + indexValue, typeof(decimal));//Added By Hiren 22-11-2017
                        }
                        else
                        {
                            tempTable.Columns.Add(item.Id.ToString(), typeof(string));
                            dt.Columns.Add(item.Question + '_' + indexValue, typeof(string));//Added By Hiren 22-11-2017
                        }
                    }
                }
                tempTable.Columns.Add("Submitted On", typeof(string));
                tempTable.Columns.Add("Email", typeof(string));
                tempTable.Columns.Add("Reported By", typeof(string));
                tempTable.Columns.Add("Phone No.", typeof(string));
                tempTable.Columns.Add("Latitude", typeof(string));
                tempTable.Columns.Add("Longitude", typeof(string));

                if (dt.Columns.Contains("Submitted On"))
                    dt.Columns.Add("SubmittedOn", typeof(string));
                else
                    dt.Columns.Add("Submitted On", typeof(string));

                if (dt.Columns.Contains("Email"))
                    dt.Columns.Add("Submitted Email", typeof(string));
                else
                    dt.Columns.Add("Email", typeof(string));

                if (dt.Columns.Contains("Reported By"))
                    dt.Columns.Add("ReportedBy", typeof(string));
                else
                    dt.Columns.Add("Reported By", typeof(string));

                if (dt.Columns.Contains("Phone No."))
                    dt.Columns.Add("PhoneNo.", typeof(string));
                else
                    dt.Columns.Add("Phone No.", typeof(string));

                dt.Columns.Add("Latitude", typeof(string));
                dt.Columns.Add("Longitude", typeof(string));
                tempTable.Load(reader);
                DbConnection.Close();

                //Changed By Hiren 27-11-2017
                if (fqList != null && fqList.Count > 0)
                {
                    foreach (var item in fqList)
                    {
                        FormQuestion fqModel = fqList.Where(i => i.Id == item.Id).FirstOrDefault();
                        dynamic question = JsonConvert.DeserializeObject<object>(fqModel.JSONQuestion);
                        string indexValue = Convert.ToString(question["index"]);
                        if (tempTable.Columns[item.Id.ToString()] != null)
                        {
                            if (empDetails != null && empDetails.SystemRoleId == 0)
                            {
                                if (cfql != null)
                                {
                                    bool isExist = cfql.Where(e => e.Id == item.Id).Any();
                                    if (!isExist)
                                    {
                                        tempTable.Columns.Remove(item.Id.ToString());
                                        dt.Columns.Remove(item.Question + '_' + indexValue);
                                    }
                                }
                            }
                        }
                    }
                }
                //End
                foreach (DataColumn col in tempTable.Columns)
                {
                    System.Diagnostics.Debug.WriteLine(col.ColumnName);
                    FormQuestion fqModel = fqList.Where(i => i.Id.ToString() == col.ColumnName).OrderByDescending(o => o.Id).FirstOrDefault();
                    bool isNum = false;
                    bool isTime = false;
                    bool isDate = false;
                    if (fqModel != null)
                    {
                        isNum = fqModel.FormToolsId == 3 || fqModel.FormToolsId == 20 || fqModel.FormToolsId == 21;
                        isTime = fqModel.FormToolsId == 10;
                        isDate = fqModel.FormToolsId == 9;
                    }
                    foreach (DataRow row in tempTable.Rows)
                    {
                        if (isNum)
                        {
                            col.DataType = typeof(Decimal);
                            if (!string.IsNullOrEmpty(row[col.ColumnName].ToString()))
                            {
                                row[col.ColumnName] = Convert.ToDecimal(row[col.ColumnName].ToString());
                            }
                            else
                            {
                                row[col.ColumnName] = 0.0;
                            }
                        }
                        if (isTime)
                        {
                            if (!string.IsNullOrEmpty(row[col.ColumnName].ToString()))
                            {
                                //Added By Hiren 20-11-2017
                                DateTime dtDate = DateTime.ParseExact(row[col.ColumnName].ToString(), "MM-dd-yyyy HH:mm", CultureInfo.InvariantCulture);
                                row[col.ColumnName] = dtDate.ToString("hh:mm tt");
                            }
                        }
                        if (isDate)
                        {
                            if (!string.IsNullOrEmpty(row[col.ColumnName].ToString()))
                            {
                                //Added By Hiren 17-11-2017
                                DateTime dtDate = DateTime.ParseExact(row[col.ColumnName].ToString(), "MM-dd-yyyy", CultureInfo.InvariantCulture);
                                row[col.ColumnName] = dtDate.ToString("dd-MMM-yyyy");
                            }
                        }
                        if (string.IsNullOrEmpty(row[col.ColumnName].ToString()))
                        {
                            row[col.ColumnName] = "";
                        }
                    }
                }
                foreach (DataRow row in tempTable.Rows)
                {
                    //string str = Convert.ToDateTime(row.ItemArray[5]).ToString("hh:mm tt");
                    //row[5] = str;
                    dt.LoadDataRow(row.ItemArray, true);
                }
                using (ExcelPackage xp = new ExcelPackage())
                {
                    ExcelWorksheet ws = xp.Workbook.Worksheets.Add(dt.TableName);

                    int rowstart = 2;
                    int colstart = 2;
                    int rowend = rowstart;
                    int colend = dt.Columns.Count > 0 ? colstart + dt.Columns.Count - 1 : colstart + dt.Columns.Count; //modified  "- 1" is added on 22-6-2017

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
                    byte[] arryData = xp.GetAsByteArray();
                    var root = db.AppSettings.Where(s => s.SubscriberId == formDBModel.SubscriberId && s.Key.ToLower() == Constants.FileRoot.ToLower()).FirstOrDefault().Value;
                    root = root + formDBModel.SubscriberId + Constants.DOCUMENTS_FOLDER;
                    if (!Directory.Exists(root))
                    {
                        Directory.CreateDirectory(root);
                    }
                    string path = root + strFileName + ".xlsx";
                    
                    File.WriteAllBytes(path, arryData);
                    var fileUrl = db.AppSettings.Where(s => s.SubscriberId == formDBModel.SubscriberId && s.Key.ToLower() == Constants.FileUrl.ToLower()).FirstOrDefault().Value;
                    fileUrl = fileUrl + formDBModel.SubscriberId + Constants.DOCUMENTS_FOLDER + strFileName + ".xlsx";
                    return fileUrl;
                }
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


