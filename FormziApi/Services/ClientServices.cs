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
    //Added by jay on 6-6-2016
    public class ClientServices
    {
        #region Fields

        LogProvider lp;
        private FormziEntities db;
        FormQuestionsController FormQuestionCtr;

        #endregion

        #region Constructor

        public ClientServices()
        {
            lp = new LogProvider("CreosformAPI");
            db = new FormziEntities();
            FormQuestionCtr = new FormQuestionsController();
        }

        #endregion

        #region Methods

        //Main method of Client, Project, Form
        public List<ClientDataModel> GetCPF(EmployeeProjectFormDateModel dateModel)
        {

            try
            {
                DateTime d = Convert.ToDateTime(dateModel.DateTime);
                IEnumerable<EmployeeForm> EmployeeFormList = db.EmployeeForms.Where(i => i.EmployeeId == dateModel.EmployeeId && !i.IsDeleted).AsEnumerable();
                long AppLoginId = db.Employees.Where(e=>e.Id== dateModel.EmployeeId).FirstOrDefault().AppLoginId;//Added By Hiren 21-11-2017
                List<long> formIds = EmployeeFormList.Select(i => i.FormId).ToList();
                List<ClientDataModel> ClientList = new List<ClientDataModel>();
                if (EmployeeFormList.Any())
                {
                    ClientList = EmployeeFormList
                        .Where(i => !i.Client.IsDeleted)
                        .GroupBy(g => g.ClientId)
                        .Select(i => new ClientDataModel
                        {
                            Id = i.Key,
                            FirstName = i.FirstOrDefault().Client.FirstName,
                            LastName = i.FirstOrDefault().Client.LastName,
                            CompanyName = i.FirstOrDefault().Client.CompanyName,
                            Email = i.FirstOrDefault().Client.Email,
                            PhoneNumber = i.FirstOrDefault().Client.PhoneNumber,
                            CreatedOn = i.FirstOrDefault().Client.CreatedOn,
                            UpdatedOn = i.FirstOrDefault().Client.UpdatedOn,
                            IsDeleted = i.FirstOrDefault().Client.IsDeleted,
                            SubscriberId = i.FirstOrDefault().Client.SubscriberId,
                            ProjectList = EmployeeFormList.Select(j => j.Project).Where(k => k.ClientId == i.FirstOrDefault().ClientId)
                            .GroupBy(gp => gp.Id)
                            .Select(f => new ProjectDataModel
                            {
                                Id = f.FirstOrDefault().Id,
                                Name = f.FirstOrDefault().Name,
                                ClientId = f.FirstOrDefault().ClientId,
                                Description = f.FirstOrDefault().Description,
                                IsActive = f.FirstOrDefault().IsActive,
                                IsDeleted = f.FirstOrDefault().IsDeleted,
                                CreatedBy = f.FirstOrDefault().CreatedBy,
                                CreatedOn = f.FirstOrDefault().CreatedOn,
                                UpdatedBy = f.FirstOrDefault().UpdatedBy,
                                UpdatedOn = f.FirstOrDefault().UpdatedOn,
                                FormList = EmployeeFormList.Where(x => x.ProjectId == f.FirstOrDefault().Id)
                                .GroupBy(gp => gp.Id)
                                .Select(h => new FormModel
                                {
                                    Id = h.FirstOrDefault().Form.Id,
                                    Name = h.FirstOrDefault().Form.Name,
                                    Description = h.FirstOrDefault().Form.Description,
                                    ProjectId = h.FirstOrDefault().ProjectId,
                                    Image = h.FirstOrDefault().Form.Image,
                                    OrderBy = h.FirstOrDefault().Form.OrderBy,
                                    IsActive = h.FirstOrDefault().Form.IsActive,
                                    SubscriberId = h.FirstOrDefault().Form.SubscriberId,
                                    LanguageId = h.FirstOrDefault().Form.LanguageId,
                                    WebFormUID = h.FirstOrDefault().Form.WebFormUID,
                                    FormVersionId = h.FirstOrDefault().Form.FormVersions.LastOrDefault().Id,
                                    formQuestion = FormQuestionCtr.GetFormQuestionByFormId(h.FirstOrDefault().Form.Id,AppLoginId)//Added By Hiren 21-11-2017
                                    //FormQuestions = h.FirstOrDefault().Form.FormQuestions
                                    //.Where(v => v.FormVersionId == h.FirstOrDefault().Form.FormVersions.LastOrDefault().Id).OrderByDescending(o => o.FormVersionId).Select(q => new FormQuestionsModel
                                    //{
                                    //    Id = q.Id,
                                    //    JSONQuestion = q.JSONQuestion,
                                    //    IsDeleted = q.IsDeleted,
                                    //    FormToolsId = q.FormToolsId,
                                    //    FormId = q.FormId,
                                    //    FormVersionId = q.FormVersionId,
                                    //    LanguageId = q.LanguageId,
                                    //    IsPublished = q.IsPublished
                                    //}).ToList()

                                }).ToList()
                            }).ToList()
                        }).ToList();

                    return ClientList;
                }
                else
                {
                    return ClientList;
                }
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);

                return null;
            }
        }

        public List<ClientDataModel> GetCPFForEmp(int subscriberId, int employeeId = 0)
        {
            try
            {
                List<EmployeeForm> empFormList =
                    db.EmployeeForms.Where(i => i.EmployeeId == employeeId)
                    .AsEnumerable()
                    .Select(i => new EmployeeForm
                    {
                        Id = i.Id,
                        ClientId = i.ClientId,
                        ProjectId = i.ProjectId,
                        FormId = i.FormId
                    }).ToList();



                List<ClientDataModel> ClientList = db.Clients
                        .Where(i => !i.IsDeleted && i.SubscriberId == subscriberId)
                        .AsEnumerable()
                        .Select(i => new ClientDataModel
                        {
                            Id = i.Id,
                            FirstName = i.FirstName,
                            LastName = i.LastName,
                            CompanyName = i.CompanyName,
                            ProjectList = i.Projects.Where(k => k.IsActive && !k.IsDeleted)
                            .Select(f => new ProjectDataModel
                            {
                                Id = f.Id,
                                Name = f.Name,
                                FormList = f.ProjectForms.Where(x => x.ProjectId == f.Id)
                                .Select(h => new FormModel
                                {
                                    Id = h.Form.Id,
                                    Name = h.Form.Name,
                                    Description = h.Form.Description,
                                    ProjectId = h.ProjectId,
                                    IsActive = h.Form.IsActive,
                                    ClientId = i.Id,
                                    ClientName = i.CompanyName,
                                    ProjectName = f.Name,
                                    IsSelected = empFormList != null ? empFormList.Where(x => x.ClientId == i.Id && x.ProjectId == h.ProjectId && x.FormId == h.Form.Id).Any() : false
                                }).ToList()
                            }).ToList()
                        }).ToList();
                return ClientList;

                //List<FormModel> model = new List<FormModel>();

                //foreach (ClientDataModel clients in ClientList)
                //{
                //    foreach (ProjectDataModel projects in clients.ProjectList)
                //    {
                //        foreach (FormModel form in projects.FormList)
                //        {
                //            model.Add(new FormModel
                //            {
                //                Id = form.Id,
                //                ClientId = form.ClientId,
                //                ProjectId = form.ProjectId,
                //                ClientName = form.ClientName,
                //                ProjectName = form.ProjectName
                //            });
                //        }
                //    }
                //}

                //return model;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);

                return null;
            }
        }

        public bool UpdateCPFForEmp(List<EmployeeForm> model)
        {
            try
            {
                if (model == null)
                {
                    return false;
                }

                long _employeeId = model.FirstOrDefault().EmployeeId;
                DateTime currentDate = Common.GetDateTime(db);

                List<EmployeeForm> empFormList = db.EmployeeForms.Where(i => i.EmployeeId == _employeeId).ToList();
                db.EmployeeForms.RemoveRange(empFormList);

                foreach (EmployeeForm item in model)
                {
                    item.CreatedOn = currentDate;
                    item.CreatedBy = _employeeId;
                    item.UpdatedBy = _employeeId;
                    item.UpdatedOn = currentDate;
                    item.IsDeleted = false;

                    db.EmployeeForms.Add(item);
                }
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

        public bool RemoveCPFForEmp(int employeeId)
        {
            try
            {
                if (employeeId <= 0)
                {
                    return false;
                }

                List<EmployeeForm> empFormList = db.EmployeeForms.Where(i => i.EmployeeId == employeeId).ToList();
                db.EmployeeForms.RemoveRange(empFormList);
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

        #endregion
    }
}