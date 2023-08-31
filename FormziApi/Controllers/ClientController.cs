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
using System.Xml.Linq;
using System.Web.UI;
using System.Xml;
using FormziApi.Models;
using FormziApi.Extention;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using FormziApi.Services;
using System.Data.Entity.Validation;
namespace FormziApi.Controllers
{
    [Authorize]
    [EnableCors("*", "*", "*")]
    public class ClientController : ApiController
    {
        #region Fields

        private LogProvider lp = new LogProvider("Formzi");
        private FormziEntities db = new FormziEntities();
        private ClientServices _clientService = new ClientServices();

        #endregion

        #region Client

        /// <summary>
        /// List of client by subscriber id.
        /// </summary>
        /// <param name="subscriberId">Subscriber id. Ex. 1</param>
        /// <returns>List of client containing Id, Unique Number, FullName, Email, Company Name, Phone number, IsDeleted and Sites list.</returns>
        [Route("api/clients/{subscriberId}")]
        [HttpGet]
        public object GetClientList(int subscriberId)
        {
            try
            {
                var client = db.Clients.Where(c => c.SubscriberId == subscriberId).OrderByDescending(a => a.CreatedOn).Select(c => new
                {
                    c.Id,
                    c.UniqueNumber,
                    FullName = c.FirstName + " " + c.LastName,
                    c.Email,
                    c.CompanyName,
                    c.PhoneNumber,
                    c.IsDeleted,
                    Sites = db.Locations.Where(s => s.ClientId == c.Id && !s.Address.IsDeleted).Count(),
                }).ToList();
                return client;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }

        /// <summary>
        /// Single client details by client id.
        /// </summary>
        /// <param name="id">client id. Ex. 1</param>
        /// <returns>Client containing Id, Unique Number, FullName, Email, Company Name, Phone number, IsDeleted and Location list.</returns>
        [Route("api/client/{id}")]
        [HttpGet]
        public object GetClient(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return null;
                }
                ClientModel client = db.Clients.Where(e => e.Id == id).FirstOrDefault().ToModel<Client, ClientModel>();
                client.Locations = GetLocationByClientId(client.Id);
                return client;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }

        }

        [Route("api/client")]
        [HttpPost]
        public int PostClient([FromBody]Client client)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return 0;
                }
                client.CreatedOn = Common.GetDateTime(db);
                client.UpdatedOn = Common.GetDateTime(db);
                db.Clients.Add(client);
                db.SaveChanges();
                return client.Id;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return 0;
            }
        }

        [Route("api/client/{id}")]
        [HttpPut]
        public int PutClient(int id, Client clientInfo)
        {
            try
            {
                if (!ModelState.IsValid || id != clientInfo.Id || clientInfo.PhoneNumber.Length > 20)
                {
                    return 0;
                }
                Client client = db.Clients.Find(clientInfo.Id);
                client.UniqueNumber = clientInfo.UniqueNumber;
                client.FirstName = clientInfo.FirstName;
                client.LastName = clientInfo.LastName;
                client.CompanyName = clientInfo.CompanyName;
                client.PhoneNumber = clientInfo.PhoneNumber;
                client.Email = clientInfo.Email;
                client.ReceiveReminders = clientInfo.ReceiveReminders;
                client.UpdatedOn = Common.GetDateTime(db);
                client.IsDeleted = clientInfo.IsDeleted;
                db.Entry(client).State = EntityState.Modified;
                db.SaveChanges();
                return client.Id;
            }
            catch (System.Data.Entity.Validation.DbEntityValidationException e)
            {
                var newException = new FormattedDbEntityValidationException(e);
                //throw newException;
                lp.Info(newException.ToString());
                lp.HandleError(e, newException.ToString());
                return 0;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return 0;
            }

        }

        [Route("api/client/{id}")]
        [HttpDelete]
        public object DeleteClient(int id)
        {
            try
            {
                Client client = db.Clients.Find(id);
                if (client == null)
                {
                    return NotFound();
                }
                client.UpdatedOn = Common.GetDateTime(db);
                client.IsDeleted = true;
                db.Entry(client).State = EntityState.Modified;
                db.SaveChanges();

                //Added by jay mistry 28-7-2016
                List<EmployeeForm> empformList = db.EmployeeForms.Where(i => i.ClientId == id).ToList();

                foreach (var item in empformList)
                {
                    item.IsDeleted = true;
                    db.Entry(item).State = EntityState.Modified;
                    db.SaveChanges();
                }

                return GetClientList(client.SubscriberId);
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }

        [Route("api/clientEmailExist")]
        [HttpGet]
        public bool ClientEmailExist(int id, string email)
        {
            try
            {
                var client = db.Clients.Where(c => c.Id != id && c.Email == email).FirstOrDefault();
                if (client == null)
                    return true;
                else
                    return false;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return false;
            }
        }

        [Route("api/clientemailexist")]
        [HttpGet]
        public bool CheckEmailExist(int id, string email, int subscriberId)
        {
            try
            {
                var client = db.Clients.Where(e => e.Id != id && e.SubscriberId == subscriberId && e.Email == email && e.IsDeleted == false).FirstOrDefault();
                if (client == null)
                    return true;
                else
                    return false;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return false;
            }
        }

        [Route("api/removeSubscriberClient/{id}")]
        [HttpDelete]
        public object DeleteSubscriberClient(int id)
        {
            try
            {
                List<Client> clients = db.Clients.Where(i => i.SubscriberId == id).ToList();
                if (clients == null)
                {
                    return NotFound();
                }

                foreach (var client in clients)
                {
                    List<Location> locationList = db.Locations.Where(i => i.ClientId == client.Id).ToList();

                    foreach (var item in locationList)
                    {
                        Address addModel = db.Addresses.Where(i => i.Id == item.AddressId).FirstOrDefault();

                        db.Addresses.Remove(addModel);
                        db.Locations.Remove(item);
                        db.SaveChanges();
                    }

                    List<Project> projectList = db.Projects.Where(i => i.ClientId == client.Id).ToList();

                    foreach (var proj in projectList)
                    {
                        List<ProjectForm> projForms =  db.ProjectForms.Where(i => i.ProjectId == proj.Id).ToList();
                        db.ProjectForms.RemoveRange(projForms);
                        db.SaveChanges();

                        List<EmployeeForm> empProjFormList = db.EmployeeForms.Where(i => i.ProjectId == proj.Id).ToList();

                        db.EmployeeForms.RemoveRange(empProjFormList);
                        db.SaveChanges();

                        db.Projects.Remove(proj);
                        db.SaveChanges();
                    }

                    db.Clients.Remove(client);
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

        #endregion

        #region Client and its Sites
        [Route("api/getClientSites/{clientId}")]
        [HttpGet]
        public List<LocationModel> GetLocationByClientId(int clientId)
        {
            try
            {
                var locations = db.Locations.Where(e => e.ClientId == clientId && !e.Address.IsDeleted).ToList().ToListModel<Location, LocationModel>();
                if (locations.Count > 0)
                {
                    locations.ToList().Select(l =>
                    {
                        var address = db.Addresses.Where(a => a.Id == l.AddressId).FirstOrDefault();
                        if (address != null)
                        {
                            l.Address = address.ToModel<Address, AddressModel>();
                            l.Address.CountryName = address.Country.Name;
                            l.Address.StateName = address.StateProvince.Name;
                            l.FullAddress = address.Address1 + "," + address.City + ",<br/>" + address.StateProvince.Name + ",";
                            l.FullAddress += address.Country.Name + ",<br/>" + address.ZipPostalCode;

                        }
                        return l;
                    }).ToList();
                }
                return locations;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }

        //[Route("api/getClientSiteLocation/{clientId}")]

        [Route("api/clientSiteLocation/{clientId}")]
        [HttpGet]
        public List<LocationModel> GetSiteAddressByClientId(int clientId)
        {
            try
            {
                var locations = db.Locations.Where(e => e.ClientId == clientId && !e.Address.IsDeleted).ToList().ToListModel<Location, LocationModel>();
                if (locations.Count > 0)
                {
                    locations.ToList().Select(l =>
                    {
                        var address = db.Addresses.Where(a => a.Id == l.AddressId).FirstOrDefault();
                        if (address != null)
                        {
                            l.Address = address.ToModel<Address, AddressModel>();
                            l.Address.CountryName = address.Country.Name;
                            l.Address.StateName = address.StateProvince.Name;
                            l.FullAddress = address.Address1 + "," + address.City + ",<br/>" + address.StateProvince.Name + ",";
                            l.FullAddress += address.Country.Name + ",<br/>" + address.ZipPostalCode;

                        }
                        return l;
                    }).ToList();
                }
                return locations;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }

        [Route("api/clientSitelocation")]
        [HttpPost]
        public object PostClientSiteLocation(JObject jObject)
        {
            try
            {
                if (jObject == null)
                    return null;

                AddressModel addressModel = jObject["addressModel"].ToObject<AddressModel>();
                LocationModel locationModel = jObject["locationModel"].ToObject<LocationModel>();

                addressModel.CreatedOn = addressModel.UpdatedOn = Common.GetDateTime(db);
                Address address = addressModel.ToEntity<AddressModel, Address>();
                db.Addresses.Add(address);
                db.SaveChanges();

                locationModel.AddressId = address.Id;
                Location location = locationModel.ToEntity<LocationModel, Location>();
                db.Locations.Add(location);
                db.SaveChanges();
                db = new FormziEntities();
                var locations = GetSiteAddressByClientId(location.ClientId);
                return locations;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }

        //Updated by jay -> httpput to httppost and
        [Route("api/clientSiteLocation")]
        [HttpPut]
        public object PutClientSiteLocation(string data)
        {
            try
            {
                if (string.IsNullOrEmpty(data))
                    return null;

                Location location = JsonConvert.DeserializeObject<Location>(data);

                if (location == null)
                    return null;

                location.Address.UpdatedOn = Common.GetDateTime(db);
                db.Entry(location).State = EntityState.Modified;
                db.Entry(location.Address).State = EntityState.Modified;
                db.SaveChanges();
                db = new FormziEntities();
                var locations = GetSiteAddressByClientId(location.ClientId);
                return locations;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return 0;
            }
        }

        [Route("api/clientSitelocation/{addressId}")]
        [HttpDelete]
        public object DeleteClientLocation(int addressId)
        {
            try
            {
                Address address = db.Addresses.Find(addressId);
                int clientId = address.Locations.FirstOrDefault().ClientId;
                if (address == null)
                {
                    return null;
                }
                address.UpdatedOn = Common.GetDateTime(db);
                address.IsDeleted = true;
                db.Entry(address).State = EntityState.Modified;
                db.SaveChanges();
                db = new FormziEntities();
                var locations = GetLocationByClientId(clientId);
                return locations;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }

        #endregion

        #region Client and its Project

        //Updated by jay -> httppost to httpput and JObject in parameter removed.
        //This was commented on old project        
        [Route("api/addClientProject")]
        [HttpPost]
        public object PutClientProject(string data)
        {
            try
            {
                if (string.IsNullOrEmpty(data))
                    return null;

                ProjectModel model = JsonConvert.DeserializeObject<ProjectModel>(data);

                if (model == null)
                    return null;

                DateTime currentDate = Common.GetDateTime(db);

                Project project = new Project();
                project.Name = model.Name;
                project.Description = model.Description;
                project.ClientId = model.ClientId;
                project.CreatedBy = model.CreatedBy;
                project.CreatedOn = currentDate;
                project.UpdatedBy = model.CreatedBy;
                project.UpdatedOn = currentDate;
                project.IsActive = true;
                project.IsDeleted = false;

                db.Projects.Add(project);
                db.SaveChanges();
                foreach (var item in model.FormList)
                {
                    ProjectForm pf = new ProjectForm();
                    pf.ProjectId = project.Id;
                    pf.FormId = item.Id;
                    pf.IsDeleted = false;
                    pf.CreatedBy = model.CreatedBy;
                    pf.CreatedOn = currentDate;
                    pf.UpdatedBy = model.CreatedBy;
                    pf.UpdatedOn = currentDate;
                    db.ProjectForms.Add(pf);
                    db.SaveChanges();
                }
                return GetClientProjectByClientId(project.ClientId);
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return 0;
            }
        }

        [Route("api/getClientProject")]
        [HttpGet]
        public List<ProjectModel> GetClientProjectByClientId(int id)
        {
            try
            {
                //var ProjectForms = db.ProjectForms.AsEnumerable();
                //List<AddedForms> forms = db.Forms.Select(j => new AddedForms { Id = j.Id, Name = j.Name }).ToList();
                List<ProjectModel> model = db.Projects.Where(i => i.ClientId == id && i.IsDeleted == false).AsEnumerable().Select(i => new ProjectModel
                {
                    Id = i.Id,
                    Name = i.Name,
                    Description = i.Description,
                    ClientId = i.ClientId,
                    CreatedBy = i.CreatedBy,
                    CreatedOn = i.CreatedOn,
                    IsActive = i.IsActive,
                    IsDeleted = i.IsDeleted,
                    UpdatedBy = i.UpdatedBy,
                    UpdatedOn = i.UpdatedOn,
                    FormList = i.ProjectForms.Where(x => !x.IsDeleted).AsEnumerable().Select(j => new FormModel { Id = j.FormId, Name = j.Form.Name }).ToList(),
                }).ToList();
                return model;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                throw;
            }
        }

        //Updated by jay -> Httpput to httppost
        [Route("api/editClientProject")]
        [HttpPost]
        public object UpdateClientProject(JObject jObject)
        {
            try
            {
                if (jObject == null)
                    return null;

                var model = jObject.ToObject<ProjectModel>();
                //model.IsActive = true;
                //model.IsDeleted = false;
                model.UpdatedOn = Common.GetDateTime(db);

                Project project = db.Projects.Where(i => i.Id == model.Id && i.IsDeleted == false).FirstOrDefault();
                project.Name = model.Name;
                project.Description = model.Description;
                project.ClientId = model.ClientId;
                project.UpdatedBy = model.CreatedBy;
                project.UpdatedOn = model.CreatedOn;
                project.IsDeleted = model.IsDeleted;
                project.IsActive = model.IsActive;

                db.Entry(project).State = EntityState.Modified;
                db.SaveChanges();

                var projectForms = db.ProjectForms.Where(i => i.ProjectId == project.Id);
                foreach (var item in model.AddedForms)
                {
                    var projectForm = projectForms.Where(i => i.ProjectId == project.Id && i.FormId == item.Id).FirstOrDefault();
                    if (projectForm != null)
                    {
                        projectForm.IsDeleted = false;
                        projectForm.UpdatedBy = model.CreatedBy;
                        projectForm.UpdatedOn = model.CreatedOn;
                        db.Entry(project).State = EntityState.Modified;
                    }
                    else
                    {
                        ProjectForm pf = new ProjectForm();
                        pf.ProjectId = project.Id;
                        pf.FormId = item.Id;
                        pf.IsDeleted = false;
                        pf.CreatedBy = model.CreatedBy;
                        pf.CreatedOn = model.CreatedOn;
                        pf.UpdatedBy = model.CreatedBy;
                        pf.UpdatedOn = model.CreatedOn;
                        db.ProjectForms.Add(pf);
                    }
                }
                foreach (var item in model.RemovedForms)
                {
                    var projectForm = db.ProjectForms.Where(i => i.ProjectId == project.Id && i.FormId == item.Id).FirstOrDefault();
                    if (projectForm != null)
                    {
                        projectForm.IsDeleted = true;
                        projectForm.UpdatedBy = model.CreatedBy;
                        projectForm.UpdatedOn = Common.GetDateTime(db);
                        db.Entry(projectForm).State = EntityState.Modified;
                    }
                }
                db.SaveChanges();

                return GetClientProjectByClientId(project.ClientId);
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return 0;
            }
        }

        [Route("api/removeClientProject/{projectId}")]
        [HttpDelete]
        public object RemoveClientProject(int projectId)
        {
            try
            {


                Project project = db.Projects.Find(projectId);
                if (project == null)
                {
                    return NotFound();
                }
                project.UpdatedOn = Common.GetDateTime(db);
                project.IsDeleted = true;
                db.Entry(project).State = EntityState.Modified;
                db.SaveChanges();

                //Added by jay mistry 28-7-2016
                List<EmployeeForm> empformList = db.EmployeeForms.Where(i => i.ProjectId == projectId).ToList();

                foreach (var item in empformList)
                {
                    item.IsDeleted = true;
                    db.Entry(item).State = EntityState.Modified;
                    db.SaveChanges();
                }

                List<ProjectForm> projectForms = db.ProjectForms.Where(i => i.ProjectId == projectId).ToList();
                foreach (var item in projectForms)
                {
                    item.IsDeleted = true;
                    item.UpdatedOn = Common.GetDateTime(db);
                    db.Entry(item).State = EntityState.Modified;
                    db.SaveChanges();
                }
                //db.ProjectForms.RemoveRange(projectForms);
                //db.SaveChanges();

                return GetClientProjectByClientId(project.ClientId);
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }

        [Route("api/getSingleClientProject")]
        [HttpGet]
        public ProjectModel GetClientProjectByProjectId(int id)
        {
            try
            {
                if (id == 0)
                {
                    return null;
                }
                Project projectModel = db.Projects.Where(i => i.Id == id && i.IsDeleted == false).FirstOrDefault();

                ProjectModel model = new ProjectModel();
                model.Id = projectModel.Id;
                model.Name = projectModel.Name;
                model.Description = projectModel.Description;
                model.ClientId = projectModel.ClientId;
                model.CreatedBy = projectModel.CreatedBy;
                model.CreatedOn = projectModel.CreatedOn;
                model.IsActive = projectModel.IsActive;
                model.IsDeleted = projectModel.IsDeleted;
                model.UpdatedBy = projectModel.UpdatedBy;
                model.UpdatedOn = projectModel.UpdatedOn;
                model.FormList = db.ProjectForms.Where(i => i.ProjectId == projectModel.Id && !i.IsDeleted).AsEnumerable().Select(i => new FormModel
                {
                    Id = i.FormId,
                    Name = i.Form.Name
                }).ToList();

                return model;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                throw;
            }
        }

        #endregion

        #region Client and its Forms

        [Route("api/getClientForms")]
        [HttpGet]
        public List<FormModel> GetClientForms(int id)
        {
            try
            {
                List<FormModel> model = db.EmployeeForms.Where(i => i.ClientId == id).Select(i => new FormModel
                {
                    Id = i.FormId,
                    Name = i.Form.Name
                }).ToList();

                return model;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                throw;
            }
        }

        [Route("api/SearchForms")]
        [HttpGet]
        public List<FormModel> SearchForms(string searchKeyword, int clientId, int projectId, int subscriberId)
        {
            try
            {
                List<FormModel> formModel = null;
                if (clientId != 0 && projectId != 0 && !string.IsNullOrEmpty(searchKeyword) && subscriberId != 0)
                {

                    var keyword = General.CleanInput(searchKeyword.ToLower());
                    formModel = db.ProjectForms.Where(i => i.ProjectId == projectId && !i.IsDeleted && !i.Form.IsDeleted) // && i.Form.SubscriberId == subscriberId)
                        .AsEnumerable()
                        .Where(i => General.CleanInput(i.Form.Name.ToLower()).Contains(keyword))
                        .Select(i => new FormModel
                        {
                            Id = i.FormId,
                            Name = i.Form.Name
                        }).ToList();

                    return formModel;
                }
                else if (clientId != 0 && projectId == 0 && !string.IsNullOrEmpty(searchKeyword) && subscriberId != 0)
                {
                    var keyword = General.CleanInput(searchKeyword.ToLower());
                    var projectList = db.Projects.Where(i => i.ClientId == clientId && i.Client.SubscriberId == subscriberId).Select(i => i.Id).ToList();
                    formModel = db.ProjectForms.Where(i => projectList.Contains(i.ProjectId) && !i.IsDeleted && !i.Form.IsDeleted)
                        .GroupBy(g => g.FormId)
                        .AsEnumerable()
                        .Where(i => General.CleanInput(i.FirstOrDefault().Form.Name.ToLower()).Contains(keyword))
                        .Select(i => new FormModel
                        {
                            Id = i.FirstOrDefault().FormId,
                            Name = i.FirstOrDefault().Form.Name
                        }).ToList();
                    return formModel;
                }
                else if (clientId != 0 && projectId == 0 && string.IsNullOrEmpty(searchKeyword) && subscriberId != 0)
                {
                    var projectList = db.Projects.Where(i => i.ClientId == clientId && i.Client.SubscriberId == subscriberId).Select(i => i.Id).ToList();
                    formModel = db.ProjectForms.Where(i => projectList.Contains(i.ProjectId) && !i.IsDeleted && !i.Form.IsDeleted)
                        .GroupBy(g => g.FormId)
                        .AsEnumerable()
                        .Select(i => new FormModel
                        {
                            Id = i.FirstOrDefault().FormId,
                            Name = i.FirstOrDefault().Form.Name
                        }).ToList();

                    return formModel;

                }
                else if (clientId == 0 && !string.IsNullOrEmpty(searchKeyword) && subscriberId != 0)
                {
                    var keyword = General.CleanInput(searchKeyword.ToLower());
                    formModel = db.Forms.AsEnumerable().Where(i => General.CleanInput(i.Name.ToLower()).Contains(keyword) && !i.IsDeleted && i.SubscriberId == subscriberId)
                        .Select(i => new FormModel
                        {
                            Id = i.Id,
                            Name = i.Name
                        }).ToList();
                    return formModel;
                }
                else if (clientId == 0 && projectId == 0 && string.IsNullOrEmpty(searchKeyword) && subscriberId != 0)
                {
                    formModel = db.Forms.Where(i => !i.IsDeleted && i.SubscriberId == subscriberId).Select(j => new FormModel
                    {
                        Id = j.Id,
                        Name = j.Name
                    }).ToList();
                    return formModel;
                }
                else if (clientId != 0 && projectId != 0 && string.IsNullOrEmpty(searchKeyword) && subscriberId != 0)
                {
                    var test = db.ProjectForms.Where(i => i.ProjectId == projectId).ToList();
                    formModel = db.ProjectForms.Where(i => i.ProjectId == projectId && !i.IsDeleted && i.Form.SubscriberId == subscriberId && !i.Form.IsDeleted)
                        .GroupBy(g => g.FormId)
                        .AsEnumerable()
                        .Select(i => new FormModel
                        {
                            Id = i.FirstOrDefault().FormId,
                            Name = i.FirstOrDefault().Form.Name
                        }).ToList();

                    return formModel;
                }
                else
                {
                    return formModel;
                }
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                throw;
            }
        }

        #endregion

        #region Formzi Mobile App Synchronization

        /// <summary>
        /// Get client, project and form list
        /// Mobile Specific
        /// </summary>
        /// <param name="employeeId"></param>
        /// <returns>Returns Client, project and form list of employee</returns>
        //[Route("api/CPFList")]
        [HttpPost]
        public object GetCPFList([FromBody]EmployeeProjectFormDateModel dateModel)
        {
            try
            {
                DateTime d = Convert.ToDateTime(dateModel.DateTime);
                ApiReturnData model = new ApiReturnData();
                ClientServices clientService = new ClientServices();
                List<ClientDataModel> ClientList = new List<ClientDataModel>();

                IEnumerable<EmployeeForm> EmployeeFormList = db.EmployeeForms.Where(i => i.EmployeeId == dateModel.EmployeeId && !i.IsDeleted).AsEnumerable();

                if (EmployeeFormList.Any())
                {
                    //
                    //Modified by jay 17-6-2016
                    //
                    Employee empModel = db.Employees.Where(i => i.Id == dateModel.EmployeeId).FirstOrDefault();
                    var fileUrl = empModel.Subscriber.AppSettings.Where(i => i.Key == Constants.FileUrl).FirstOrDefault().Value;


                    model.Success = true;
                    model.Message = "";
                    model.SuccessData = new
                    {
                        Id = empModel.Id,
                        PayrollId = empModel.PayrollId,
                        FirstName = empModel.FirstName,
                        LastName = empModel.LastName,
                        ProfilePicture = !string.IsNullOrEmpty(empModel.ProfilePicture) ?
                        fileUrl + empModel.SubscriberId + Constants.ImageFolder + Constants.EmployeeFolder + empModel.Id + "/" + empModel.ProfilePicture : "",
                        BirthDate = empModel.BirthDate,
                        PhoneNumber = empModel.PhoneNumber,
                        AppLoginId = empModel.AppLoginId,
                        SubscriberId = empModel.SubscriberId,
                        SubscriberLogo = !string.IsNullOrEmpty(empModel.Subscriber.CompanyLogo) ? fileUrl + empModel.SubscriberId + Constants.ImageFolder + Constants.SubscriberFolder + empModel.Subscriber.CompanyLogo : "",
                        IsFound = true,
                        ClientList = clientService.GetCPF(dateModel),
                        TimeStamp = Common.GetDateTime(db)
                    };
                    return model;
                }
                else
                {
                    //This will be executed when no form assign to employee
                    //Here API is working successfully but ti inform developer; "IsFound" value set to false.
                    model.Success = false;
                    model.Message = "No data found.";
                    model.SuccessData = new { IsFound = false, ClientList = ClientList, TimeStamp = Common.GetDateTime(db) };
                    return model;
                }
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);

                ApiReturnData model = new ApiReturnData();
                model.Success = false;
                model.Message = "An error occurred. Please try agian later";
                model.SuccessData = null;
                return model;
            }
        }

        /// <summary>
        /// Get client, project and form list
        /// Mobile Specific
        /// </summary>
        /// <param name="employeeId"></param>
        /// <returns>Returns Client, project and form list of employee</returns>
        //[Route("api/CPFListOld")]
        [HttpPost]
        public object GetCPFList2([FromBody]EmployeeProjectFormDateModel dateModel)
        {
            try
            {
                DateTime d = Convert.ToDateTime(dateModel.DateTime);

                var EmployeeForms = db.EmployeeForms.Where(i => i.EmployeeId == dateModel.EmployeeId && (i.CreatedOn > d || i.UpdatedOn > d)).AsEnumerable();

                List<ClientDataModel> ClientList = new List<ClientDataModel>();
                List<ProjectDataModel> ProjectList = new List<ProjectDataModel>();
                List<FormModel> FormList = new List<FormModel>();
                List<string> ClientListIds = new List<string>();
                List<string> ProjectListIds = new List<string>();

                int SubscriberId = db.Employees.Where(i => i.Id == dateModel.EmployeeId).FirstOrDefault().SubscriberId;

                if (db.Clients.Where(i => i.SubscriberId == SubscriberId && (i.CreatedOn > d || i.UpdatedOn > d)).Any())
                {
                    return GetCPFList(dateModel);
                }
                else
                {
                    ClientList = db.Clients.Where(i => i.SubscriberId == SubscriberId)
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

                    ClientListIds = ClientList.Select(i => i.Id.ToString()).ToList();
                }

                if (db.Projects.Where(i => ClientListIds.Contains(i.Id.ToString()) && (i.CreatedOn > d || i.UpdatedOn > d)).Any())
                {
                    return GetCPFList(dateModel);
                }
                else
                {
                    ProjectList = db.Projects.Where(i => ClientListIds.Contains(i.Id.ToString()))
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

                    ProjectListIds = ProjectList.Select(i => i.Id.ToString()).ToList();
                }
                if (EmployeeForms.Any())
                {
                    return GetCPFList(dateModel);
                }
                else
                {
                    ClientList = new List<ClientDataModel>();
                    ProjectList = new List<ProjectDataModel>();
                    FormList = new List<FormModel>();
                }

                var model = new { ClientList = ClientList, ProjectList = ProjectList, FormList = FormList, TimeStamp = Common.GetDateTime(db) };
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
        /// Get client, project and form list
        /// Mobile Specific
        /// </summary>
        /// <param name="employeeId"></param>
        /// <returns>Returns Client, project and form list of employee</returns>
        [Route("api/CPFUpdated")]
        [HttpPost]
        public object CheckCPFUpdated([FromBody]EmployeeProjectFormDateModel dateModel)
        {
            try
            {
                DateTime d = Convert.ToDateTime(dateModel.DateTime);
                ApiReturnData model = new ApiReturnData();

                IEnumerable<EmployeeForm> EmployeeForms = db.EmployeeForms.Where(i => i.EmployeeId == dateModel.EmployeeId && (i.CreatedOn >= d || i.UpdatedOn >= d)).AsEnumerable();

                if (EmployeeForms.Any())
                {
                    return GetCPFList(dateModel);
                }
                else
                {
                    EmployeeForms = db.EmployeeForms.Where(i => i.EmployeeId == dateModel.EmployeeId && !i.IsDeleted).AsEnumerable();

                    bool IsClientListUpdated = EmployeeForms
                        .Where(i => (i.Client.CreatedOn >= d || i.Client.UpdatedOn >= d)).Any();

                    bool IsProjectListUpdated = EmployeeForms
                         .Where(i => (i.Project.CreatedOn >= d || i.Project.UpdatedOn >= d)).Any();

                    bool IsFormListUpdated = EmployeeForms
                        .Where(i => (i.Form.CreatedOn >= d || i.Form.UpdatedOn >= d)).Any();

                    if (IsClientListUpdated || IsProjectListUpdated || IsFormListUpdated)
                    {
                        return GetCPFList(dateModel);
                    }
                    else
                    {
                        model.Success = true;
                        model.Message = "";
                        model.SuccessData = new { IsFound = false, TimeStamp = Common.GetDateTime(db) };
                        return model;
                    }
                }
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                ApiReturnData model = new ApiReturnData();
                model.Success = false;
                model.Message = Constants.ERROR_EXCEPTION_MESSAGE;
                model.SuccessData = null;
                return model;
            }
        }

        #endregion


        [Route("api/CPFListForEmp")]
        [HttpGet]
        public object CPFListForEmp(int subscriberId,int employeeId = 0)
        {
            try
            {
                return _clientService.GetCPFForEmp(subscriberId, employeeId);
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }

        [Route("api/SaveCPFEList")]
        [HttpPost]
        public object SaveCPFListUpdate(List<EmployeeForm> model)
        {
            try
            {
                return _clientService.UpdateCPFForEmp(model);
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return false;
            }
        }

        [Route("api/RemoveCPFEList/{id}")]
        [HttpDelete]
        public object SaveCPFListUpdate(int id)
        {
            try
            {
                return _clientService.RemoveCPFForEmp(id);
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