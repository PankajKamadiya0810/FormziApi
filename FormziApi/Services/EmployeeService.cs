using FormziApi.Database;
using FormziApi.Helper;
using FormziApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FormziApi.Services
{
    public class EmployeeService
    {
        #region Fields
        
        LogProvider lp;
        private FormziEntities db;
        
        #endregion

        #region Constructor

        public EmployeeService()
        {
            lp = new LogProvider("CreosformAPI");
            db = new FormziEntities();
        } 

        #endregion

        #region Methods

        public List<ClientDataModel> EmployeeForms(EmployeeProjectFormDateModel dateModel)
        {

            try
            {
                DateTime d = Convert.ToDateTime(dateModel.DateTime);
                IEnumerable<EmployeeForm> EmployeeFormList = db.EmployeeForms.Where(i => i.EmployeeId == dateModel.EmployeeId && !i.IsDeleted).AsEnumerable();

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
                            SubscriberId = i.FirstOrDefault().Client.SubscriberId,
                            ProjectList = EmployeeFormList.Select(j => j.Project).Where(k => k.ClientId == i.FirstOrDefault().ClientId)
                            .GroupBy(gp => gp.Id)
                            .Select(f => new ProjectDataModel
                            {
                                Id = f.FirstOrDefault().Id,
                                Name = f.FirstOrDefault().Name,
                                ClientId = f.FirstOrDefault().ClientId,
                                IsActive = f.FirstOrDefault().IsActive,
                                IsDeleted = f.FirstOrDefault().IsDeleted,
                                FormList = EmployeeFormList.Where(x => x.ProjectId == f.FirstOrDefault().Id)
                                .GroupBy(gp => gp.Id)
                                .Select(h => new FormModel
                                {
                                    Id = h.FirstOrDefault().Form.Id,
                                    Name = h.FirstOrDefault().Form.Name,
                                    IsSelected = false
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

        #endregion
    }
}