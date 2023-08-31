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
using FormziApi.Extention;
using FormziApi.Models;
using System.Web;
using System.IO;
using Newtonsoft.Json;
using System.Configuration;
using FormziApi.Services;
using JWT;
using System.Web.WebPages.Html;
using System.Text.RegularExpressions;

namespace FormziApi.Controllers
{
    [Authorize]
    [EnableCors("*", "*", "*")]
    public class EmployeesController : ApiController
    {
        #region Fields
        private LogProvider lp = new LogProvider("EmployeesController");
        private FormziEntities db = new FormziEntities();
        #endregion

        #region Employee
        [Route("api/employeesForOperation/{subscriberId}")]
        [HttpGet]
        public object GetEmployees(int subscriberId)
        {
            try
            {
                int role = Convert.ToInt16(Constants.Roles.OperationAdmin);
                role = db.Roles.Where(r => r.Value == role && r.SubscriberId == subscriberId).FirstOrDefault().Id;//Changed By Hiren 29-11-2017
                var employee = db.Employees.Where(e => e.AppLogin.IsDeleted == false && e.SubscriberId == subscriberId).ToList().Select(l => { return new { l.Id, l.FirstName, l.LastName, l.AppLoginId }; }).OrderByDescending(t => t.Id);
                return new { Employee = employee, Role = role };
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }

        /// <summary>
        /// Get List of employees
        /// </summary>
        /// <param name="subscriberId"></param>
        /// <returns></returns>
        [Route("api/employeeList")]
        [HttpGet]
        public object GetEmployeeList(int subscriberId)
        {
            try
            {
                var employee = db.Employees.Where(e => e.SubscriberId == subscriberId).OrderByDescending(t => t.Id);
                int roleValue = Convert.ToInt16(Constants.Roles.OperationAdmin);
                int roleId = db.Roles.Where(r => r.Value == roleValue && r.SubscriberId == subscriberId).FirstOrDefault().Id;//Changed By Hiren 29-11-2017

                return employee.ToList().Select(e =>
                {
                    EmployeeModel model = new EmployeeModel();
                    model.FullName = e.FirstName + " " + e.LastName;
                    model.Id = e.Id;
                    model.UpdatedOn = e.UpdatedOn;
                    model.AppLogin = e.AppLogin.ToModel<AppLogin, AppLoginModel>();

                    //Changed By Hiren 29-11-2017
                    var employeeRole = e.AppLogin.EmployeeRoles.Where(r => r.AppLoginId == e.AppLoginId).FirstOrDefault();
                    model.RoleName = "";
                    if (employeeRole != null)
                    {
                        model.RoleName = db.Roles.Where(r => r.Id == employeeRole.RoleId).AsEnumerable().Select(i => i.Name).FirstOrDefault(); ;
                    }
                    else
                    {
                        model.RoleName = "";
                    }
                    //End
                    //old code two role
                    //var employeeRole = e.AppLogin.EmployeeRoles.ToList().ToListModel<EmployeeRole, EmployeeRoleModel>();
                    //model.RoleName = "";
                    //for (int i = 0; i < employeeRole.Count; i++)
                    //{
                    //    if (model.RoleName.IndexOf(employeeRole[i].RoleName) == -1)
                    //    {
                    //        if (i > 0)
                    //            model.RoleName += " ";
                    //            model.RoleName += employeeRole[i].RoleName + ",";
                    //    }
                    //}
                    //model.RoleName = !string.IsNullOrEmpty(model.RoleName) ? model.RoleName.Substring(0, model.RoleName.Length - 1) : string.Empty;
                    //end
                    model.OperationLocationName = e.EmployeeLocations.FirstOrDefault() != null ? e.EmployeeLocations.FirstOrDefault().Location.Name : string.Empty;
                    if (e.AppLogin.LoginLogs.Count > 0)
                        model.LastLogin = e.AppLogin.LoginLogs.Where(h => h.AppLoginId == e.AppLoginId && h.Action.ToLower() == Constants.LoggedIn.ToLower()).OrderByDescending(t => t.Id).FirstOrDefault().LoggedOn;
                    else
                        model.LastLogin = e.CreatedOn;
                    return model;
                });
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }

        [Route("api/employees")]
        [HttpGet]
        public object GetEmployees()
        {
            try
            {
                var emplyees = db.Employees.Where(m => m.AppLogin.IsDeleted == false);
                return emplyees.ToList().Select(e =>
                {
                    var model = e.ToModel<Employee, EmployeeModel>();
                    model.AppLogin = e.AppLogin.ToModel<AppLogin, AppLoginModel>();
                    model.RoleId = e.AppLogin.EmployeeRoles.Where(a => a.AppLoginId == e.AppLoginId).FirstOrDefault().Role.Id;
                    model.RoleName = e.AppLogin.EmployeeRoles.Where(a => a.AppLoginId == e.AppLoginId).FirstOrDefault().Role.Name;
                    model.LastLogin = e.AppLogin.LoginLogs.Count > 0 ? e.AppLogin.LoginLogs.Where(h => h.AppLoginId == e.AppLoginId && h.Action.ToLower() == Constants.LoggedIn.ToLower()).OrderByDescending(t => t.Id).FirstOrDefault().LoggedOn : DateTime.Now;
                    return model;
                });
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }

        // GET: api/Employees/5
        [HttpGet]
        public object GetEmployee(int id)
        {
            try
            {
                Employee employee = db.Employees.Where(e => e.Id == id).FirstOrDefault(); //Remove for VPHS //&& e.AppLogin.IsDeleted == false

                if (employee == null)
                {
                    return null;
                }
                else
                {
                    string password = General.Decrypt(employee.AppLogin.Password);
                    //EmployeeModel model = employee.ToModel<Employee, EmployeeModel>();

                    EmployeeModel model = new EmployeeModel();

                    model.Id = employee.Id;
                    model.PayrollId = employee.PayrollId;
                    model.Title = employee.Title;
                    model.FirstName = employee.FirstName;
                    model.Gender = employee.Gender;
                    model.LastName = employee.LastName;
                    model.ProfilePicture = employee.ProfilePicture;
                    model.BirthDate = employee.BirthDate;
                    model.PhoneNumber = employee.PhoneNumber;
                    model.CreatedOn = employee.CreatedOn;
                    model.UpdatedOn = employee.UpdatedOn;
                    model.AppLoginId = employee.AppLoginId;
                    model.SubscriberId = employee.SubscriberId;
                    model.IsDeleted = employee.AppLogin.IsDeleted;
                    model.SystemRoleId = employee.SystemRoleId;
                    model.AppLogin = employee.AppLogin.ToModel<AppLogin, AppLoginModel>();
                    model.AppLogin.Password = password;
                    model.AppLogin.EmployeeRoles = employee.AppLogin.EmployeeRoles.ToList().ToListModel<EmployeeRole, EmployeeRoleModel>();
                    model.EmployeeLocations = employee.EmployeeLocations.ToList().ToListModel<EmployeeLocation, EmployeeLocationModel>();
                    int operationId = 0;
                    if (model.EmployeeLocations.Count > 0)
                    {
                        int locationId = model.EmployeeLocations[0].LocationId;
                        operationId = db.Locations.Where(l => l.Id == locationId).FirstOrDefault().OperationId;
                        model.EmployeeLocations[0].OperationId = operationId;
                    }
                    model.Documents = employee.Documents.ToList().ToListModel<Document, DocumentModel>();
                    model.EmployeeProjectForms = GetEmployeeProjectForms(employee.Id);

                    //Added By Hiren 21-11-2017
                    int emproleId = 0;
                    if (model.AppLogin.EmployeeRoles.Count > 0)
                    {
                        emproleId = db.EmployeeRoles.Where(r => r.AppLoginId == model.AppLoginId).FirstOrDefault().RoleId;
                        model.RoleId = emproleId;
                        if (emproleId != 0)
                        {
                            UserReportTo userReportToObj = db.UserReportToes.Where(e => e.EmpId == model.Id && e.EmpRoleId == emproleId).FirstOrDefault();
                            if (userReportToObj != null)
                            {
                                model.ReportingEmployeeId = userReportToObj.ReportToEmpId;
                            }
                            else
                            {
                                model.ReportingEmployeeId = 0;
                            }
                        }
                    }
                    //End
                    return model;
                }
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }

        // Used to update specific fields of table but foreign key fields cant get
        [HttpPatch]
        public int PatchEmployee(int id, Delta<Employee> employee)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return 0;
                }
                Employee objemployee = db.Employees.Where(e => e.Id == id).SingleOrDefault();
                objemployee.AppLogin.Password = General.Encrypt(objemployee.AppLogin.Password);
                objemployee.UpdatedOn = Common.GetDateTime(db);
                employee.Patch(objemployee);
                db.SaveChanges();
                return id;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return 0;
            }

        }

        [Route("api/addemployee/{subscriberId}")]
        [HttpPost]
        public async Task<HttpResponseMessage> AddEmployee(int subscriberId)
        {
            try
            {
                if (!Request.Content.IsMimeMultipartContent())
                {
                    throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                }

                var root = db.AppSettings.Where(s => s.SubscriberId == subscriberId && s.Key.ToLower() == Constants.FileRoot.ToLower()).FirstOrDefault().Value;
                root = root + subscriberId + Constants.ImageFolder + Constants.EmployeeFolder;

                MultipartFormDataStreamProvider provider = new MultipartFormDataStreamProvider(root);

                var result = await Request.Content.ReadAsMultipartAsync(provider);
                if (result.FormData["model"] == null)
                {
                    throw new HttpResponseException(HttpStatusCode.BadRequest);
                }

                var model = result.FormData["model"];

                // Add employee  data
                Employee employee = JsonConvert.DeserializeObject<Employee>(model);
                EmployeeModel employeemodel = JsonConvert.DeserializeObject<EmployeeModel>(model);//Added By Hiren 21-11-2017

                employee.AppLogin.Password = General.Encrypt(employee.AppLogin.Password);
                employee.CreatedOn = employee.UpdatedOn = Common.GetDateTime(db);
                int role = Convert.ToInt16(Constants.Roles.OperationAdmin);
                role = db.Roles.Where(r => r.Value == role && r.SubscriberId == subscriberId).FirstOrDefault().Id;//Changed By Hiren 29-11-2017
                List<int> operationIds = employee.AppLogin.EmployeeRoles.Where(e => e.RoleId == role).Select(e => e.OperationId).ToList();
                List<EmployeeRole> employeeRoles = null;
                if (operationIds.Count > 0)
                {
                    employeeRoles = db.EmployeeRoles.Where(e => e.RoleId == role && operationIds.Contains(e.OperationId)).ToList();
                    foreach (var item in employeeRoles)
                    {
                        role = Convert.ToInt16(Constants.Roles.MobileUser);
                        role = db.Roles.Where(r => r.Value == role && r.SubscriberId == subscriberId).FirstOrDefault().Id;// Changed By Hiren 29-11-2017
                        EmployeeRole defaultRole = db.EmployeeRoles.Where(e => e.RoleId == role && e.AppLoginId == item.AppLoginId).FirstOrDefault();
                        if (defaultRole != null)
                        {
                            db.EmployeeRoles.Remove(item);
                        }
                        else
                        {
                            item.RoleId = role;
                            item.OperationId = 0;
                            db.Entry(item).State = EntityState.Modified;
                        }
                    }
                }

                db.Employees.Add(employee);
                db.SaveChanges();

                //Added By Hiren 21-11-2017
                if (employeemodel.RoleId != 0)
                {
                    //Added Data into Employee Role Table
                    EmployeeRole emprole = new EmployeeRole();
                    emprole.AppLoginId = employee.AppLoginId;
                    emprole.RoleId = employeemodel.RoleId;
                    emprole.OperationId = employeemodel.OperationId;
                    db.EmployeeRoles.Add(emprole);
                    db.SaveChanges();
                    //End

                    if (employeemodel.ReportingEmployeeId != 0)
                    {
                        long ReportEmpApploginId = db.Employees.Where(e => e.Id == employeemodel.ReportingEmployeeId).FirstOrDefault().AppLoginId;
                        if (ReportEmpApploginId != 0)
                        {
                            int ReportToEmpRoleId = db.EmployeeRoles.Where(e => e.AppLoginId == ReportEmpApploginId).FirstOrDefault().RoleId;
                            if (ReportToEmpRoleId != 0)
                            {
                                UserReportTo urt = new UserReportTo();
                                urt.EmpId = employee.Id;
                                urt.EmpRoleId = employeemodel.RoleId;
                                urt.ReportToEmpId = employeemodel.ReportingEmployeeId;
                                urt.ReportToEmpRoleId = ReportToEmpRoleId;
                                urt.IsDeleted = false;
                                urt.IsActive = true;
                                urt.CreatedBy = employeemodel.CreatedBy;
                                urt.CreatedOn = employee.CreatedOn;
                                urt.UpdatedBy = employeemodel.CreatedBy;
                                urt.UpdatedOn = employee.CreatedOn;
                                db.UserReportToes.Add(urt);
                                db.SaveChanges();
                            }
                        }
                    }
                }
                //End

                root = root + employee.Id + "/";
                Directory.CreateDirectory(root);

                foreach (MultipartFileData fileData in result.FileData)
                {
                    if (string.IsNullOrEmpty(fileData.Headers.ContentDisposition.FileName))
                    {
                        return Request.CreateResponse(HttpStatusCode.NotAcceptable, "This request is not properly formatted");
                    }
                    string fileName = fileData.Headers.ContentDisposition.FileName;
                    string extension = "", newFileName = "";

                    if (fileName.StartsWith("\"") && fileName.EndsWith("\""))
                    {
                        fileName = fileName.Trim('"');
                    }
                    if (fileName.Contains(@"/") || fileName.Contains(@"\"))
                    {
                        fileName = Path.GetFileName(fileName);
                    }
                    extension = Path.GetExtension(fileName);

                    if (fileData.Headers.ContentDisposition.Name.Trim('"') == Constants.ProfilePic)
                    {
                        newFileName = Constants.ProfilePicPath + employee.Id;
                        employee.ProfilePicture = Constants.ProfilePicPath + employee.Id + extension;
                    }
                    else if (fileData.Headers.ContentDisposition.Name.Trim('"') == Constants.IdProof)
                    {
                        newFileName = Constants.IdProofPath + employee.Id;
                        foreach (var item in employee.Documents)
                        {
                            item.Title = item.Title;
                            item.EmployeeId = employee.Id;
                            item.Path = newFileName + extension;
                        }
                    }

                    File.Move(fileData.LocalFileName, Path.Combine(root, newFileName + extension));
                }
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, employee.Id);
                //return Request.CreateResponse(HttpStatusCode.OK, "true");
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }

        [Route("api/editemployee")]
        [HttpPost]
        public async Task<HttpResponseMessage> EditEmployee(int subscriberId, int employeeId)
        {
            try
            {
                if (!Request.Content.IsMimeMultipartContent())
                {
                    throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
                }
                var root = db.AppSettings.Where(s => s.Key.ToLower() == Constants.FileRoot.ToLower()).FirstOrDefault().Value;
                root = root + subscriberId + Constants.ImageFolder + Constants.EmployeeFolder + employeeId + "/";

                Directory.CreateDirectory(root);
                MultipartFormDataStreamProvider provider = new MultipartFormDataStreamProvider(root);

                var result = await Request.Content.ReadAsMultipartAsync(provider);
                if (result.FormData["model"] == null)
                {
                    throw new HttpResponseException(HttpStatusCode.BadRequest);
                }

                var model = result.FormData["model"];
                DateTime currentDate = Common.GetDateTime(db);
                Employee employee = JsonConvert.DeserializeObject<Employee>(model);
                EmployeeModel employeemodel = JsonConvert.DeserializeObject<EmployeeModel>(model);//Added By Hiren 21-11-2017

                employee.AppLogin.Password = General.Encrypt(employee.AppLogin.Password);
                employee.UpdatedOn = currentDate;

                int role = Convert.ToInt16(Constants.Roles.OperationAdmin);
                role = db.Roles.Where(r => r.Value == role && r.SubscriberId == subscriberId).FirstOrDefault().Id;//Changed By Hiren 29-11-2017

                List<int> operationIds = employee.AppLogin.EmployeeRoles.Where(e => e.RoleId == role).Select(e => e.OperationId).ToList();
                List<EmployeeRole> employeeRoles = null;

                employeeRoles = db.EmployeeRoles.Where(e => e.AppLoginId == employee.AppLoginId).ToList();
                db.EmployeeRoles.RemoveRange(employeeRoles);

                if (operationIds.Count > 0)
                {
                    employeeRoles = db.EmployeeRoles.Where(e => e.RoleId == role && operationIds.Contains(e.OperationId) && e.AppLoginId != employee.AppLoginId).ToList();

                    foreach (var item in employeeRoles)
                    {
                        role = Convert.ToInt16(Constants.Roles.MobileUser);
                        role = db.Roles.Where(r => r.Value == role && r.SubscriberId == subscriberId).FirstOrDefault().Id;//Changed By Hiren 29-11-2017
                        EmployeeRole defaultRole = db.EmployeeRoles.Where(e => e.RoleId == role && e.AppLoginId == item.AppLoginId).FirstOrDefault();
                        if (defaultRole != null)
                        {
                            db.EmployeeRoles.Remove(item);
                        }
                        else
                        {
                            item.RoleId = role;
                            item.OperationId = 0;
                            db.Entry(item).State = EntityState.Modified;
                        }
                    }
                }

                //Added By Hiren 22-11-2017
                if (employeemodel.RoleId != 0)
                {
                    //Added Data into Employee Role Table
                    EmployeeRole emprole = new EmployeeRole();
                    emprole.AppLoginId = employee.AppLoginId;
                    emprole.RoleId = employeemodel.RoleId;
                    emprole.OperationId = employeemodel.OperationId;
                    db.EmployeeRoles.Add(emprole);
                    db.SaveChanges();

                    employeemodel.UpdatedOn = Common.GetDateTime(db);

                    if (employeemodel.ReportingEmployeeId != 0)
                    {
                        long ReportEmpApploginId = db.Employees.Where(e => e.Id == employeemodel.ReportingEmployeeId).FirstOrDefault().AppLoginId;
                        if (ReportEmpApploginId != 0)
                        {
                            int ReportToEmpRoleId = db.EmployeeRoles.Where(e => e.AppLoginId == ReportEmpApploginId).FirstOrDefault().RoleId;
                            if (ReportToEmpRoleId != 0)
                            {
                                UserReportTo dbUserReportToToModel = db.UserReportToes.Where(i => i.EmpId == employee.Id).FirstOrDefault();
                                if (dbUserReportToToModel != null)
                                {
                                    dbUserReportToToModel.EmpRoleId = employeemodel.RoleId;
                                    dbUserReportToToModel.ReportToEmpId = employeemodel.ReportingEmployeeId;
                                    dbUserReportToToModel.ReportToEmpRoleId = ReportToEmpRoleId;
                                    dbUserReportToToModel.UpdatedOn = employee.UpdatedOn;
                                    dbUserReportToToModel.UpdatedBy = employeemodel.CreatedBy;
                                    db.Entry(dbUserReportToToModel).State = EntityState.Modified;
                                    db.SaveChanges();
                                }
                                else
                                {
                                    UserReportTo urt = new UserReportTo();

                                    urt.EmpId = employee.Id;
                                    urt.EmpRoleId = employeemodel.RoleId;
                                    urt.ReportToEmpId = employeemodel.ReportingEmployeeId;
                                    urt.ReportToEmpRoleId = ReportToEmpRoleId;
                                    urt.IsDeleted = false;
                                    urt.IsActive = true;
                                    urt.CreatedBy = employeemodel.CreatedBy;
                                    urt.CreatedOn = employee.CreatedOn;
                                    urt.UpdatedBy = employeemodel.CreatedBy;
                                    urt.UpdatedOn = employee.UpdatedOn;

                                    db.UserReportToes.Add(urt);
                                    db.SaveChanges();
                                }
                            }
                        }
                    }
                    else
                    {
                        //Remove the User Report To by Employee Id
                        List<UserReportTo> userReportToList = null;
                        userReportToList = db.UserReportToes.Where(e => e.EmpId == employee.Id).ToList();
                        db.UserReportToes.RemoveRange(userReportToList);
                        db.SaveChanges();
                    }
                }
                //End

                //added by jay
                foreach (var item in employee.EmployeeLocations)
                {
                    //added by jay
                    EmployeeLocation empLoc = db.EmployeeLocations.Where(i => i.EmployeeId == employee.Id).ToList().FirstOrDefault();
                    //if subscriber admin then location is null so emploc != null need to check.
                    if (empLoc != null && empLoc.LocationId != item.LocationId)
                    {
                        empLoc.LocationId = item.LocationId;
                        db.SaveChanges();
                    }
                    else if (empLoc == null)//added by jay
                    {
                        EmployeeLocation empLocModel = new EmployeeLocation();
                        empLocModel.EmployeeId = employee.Id;
                        empLocModel.LocationId = item.LocationId;
                        db.EmployeeLocations.Add(empLocModel);
                        db.SaveChanges();
                    }
                }

                // get the files
                foreach (MultipartFileData fileData in result.FileData)
                {
                    ImageUpload imgUploadObj = new ImageUpload();
                    string guid = Guid.NewGuid().ToString().ToLower();

                    if (string.IsNullOrEmpty(fileData.Headers.ContentDisposition.FileName))
                    {
                        return Request.CreateResponse(HttpStatusCode.NotAcceptable, "This request is not properly formatted");
                    }
                    string fileName = fileData.Headers.ContentDisposition.FileName;
                    string extension = "", newFileName = "";

                    if (fileName.StartsWith("\"") && fileName.EndsWith("\""))
                    {
                        fileName = fileName.Trim('"');
                        extension = Path.GetExtension(fileName);
                    }
                    if (fileName.Contains(@"/") || fileName.Contains(@"\"))
                    {
                        fileName = Path.GetFileName(fileName);
                        extension = Path.GetExtension(fileName);
                    }

                    if (fileData.Headers.ContentDisposition.Name.Trim('"') == Constants.ProfilePic)
                    {
                        imgUploadObj.Width = Helper.Constants.EmpProfilePicMaxSize;
                        newFileName = Constants.ProfilePicPath + employee.Id;
                        employee.ProfilePicture = Constants.ProfilePicPath + employee.Id + extension;
                    }
                    else if (fileData.Headers.ContentDisposition.Name.Trim('"') == Constants.IdProof)
                    {
                        imgUploadObj.Width = Helper.Constants.EmpDocMaxSize;
                        newFileName = Constants.IdProofPath + employee.Id;
                        foreach (var item in employee.Documents)
                        {
                            item.Title = item.Title;
                            item.Path = newFileName + "_" + new Regex("-").Replace(guid, "") + extension; //Replace all
                            db.Documents.Add(item);
                        }
                    }

                    string filesToDelete = "*" + newFileName + "*";
                    string[] fileList = System.IO.Directory.GetFiles(root, filesToDelete);
                    foreach (string file in fileList)
                    {
                        File.Delete(file);
                    }

                    if (fileData.Headers.ContentDisposition.Name.Trim('"') == Constants.ProfilePic)
                    {
                        File.Move(fileData.LocalFileName, Path.Combine(root, employee.ProfilePicture));
                        imgUploadObj.OpenFileAndResize(Path.Combine(root, employee.ProfilePicture));
                    }
                    else if (fileData.Headers.ContentDisposition.Name.Trim('"') == Constants.IdProof)
                    {
                        File.Move(fileData.LocalFileName, Path.Combine(root, newFileName + "_" + new Regex("-").Replace(guid, "") + extension));
                        imgUploadObj.OpenFileAndResize(Path.Combine(root, newFileName + "_" + new Regex("-").Replace(guid, "") + extension));
                    }
                }
                db.Entry(employee.AppLogin).State = EntityState.Modified;
                employee.EmployeeLocations = null;
                employee.UpdatedOn = currentDate;
                db.Entry(employee).State = EntityState.Modified;
                db.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "true");
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }

        [HttpDelete]
        public object DeleteEmployee(int id)
        {
            try
            {
                Employee employee = db.Employees.Find(id);
                if (employee == null)
                {
                    return NotFound();
                }
                employee.UpdatedOn = Common.GetDateTime(db);
                employee.AppLogin.IsDeleted = true;//
                db.Entry(employee).State = EntityState.Modified;
                db.SaveChanges();
                return GetEmployeeList(employee.SubscriberId);
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }

        [Route("api/RemoveEmployeeDocument/{id}")]
        [HttpDelete]
        public object RemoveEmployeeDocument(int id)
        {
            try
            {
                return true;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }

        /// <summary>
        /// Remove employee's profile pic and id proof 
        /// DELETE Image
        /// </summary>
        /// <param name="imageType">profilePic,IdProof</param>
        /// <param name="empId">number</param>
        /// <param name="fileName"></param>
        /// <param name="subscriberId">number</param>
        /// <returns></returns>
        [Route("api/removeEmployeeImage")]
        [HttpDelete]
        public object DeleteEmployee(string imageType, long empId, string fileName, long subscriberId)
        {
            try
            {
                if (empId <= 0 || string.IsNullOrEmpty(fileName) || subscriberId <= 0)
                    return false;

                var root = db.AppSettings.Where(s => s.SubscriberId == subscriberId && s.Key.ToLower() == Constants.FileRoot.ToLower()).FirstOrDefault().Value;
                root = root + subscriberId + Constants.ImageFolder + Constants.EmployeeFolder + empId + "/";

                if (imageType == "profilepic")
                {
                    Employee model = db.Employees.Where(i => i.Id == empId).FirstOrDefault();
                    if (model != null && !string.IsNullOrEmpty(model.ProfilePicture))
                    {
                        if (!(System.IO.Directory.Exists(root) && System.IO.File.Exists(root + model.ProfilePicture)))
                            return false;

                        File.Delete(root + model.ProfilePicture);

                        model.ProfilePicture = "";
                        db.Entry(model).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    else
                    {
                        return false;
                    }
                }
                else if (imageType == "IdProof")
                {
                    List<Document> documents = db.Documents.Where(i => i.EmployeeId == empId).ToList();
                    if (documents != null)
                    {
                        foreach (var item in documents)
                        {
                            if (!string.IsNullOrEmpty(item.Path))
                            {
                                if (System.IO.Directory.Exists(root) && System.IO.File.Exists(root + item.Path))
                                {
                                    File.Delete(root + item.Path);
                                }
                                db.Documents.Remove(item);
                                db.SaveChanges();
                            }
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
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

        #region Employee Forms
        [Route("api/removeEmployeeForm")]
        [HttpPost]
        public bool RemoveEmployeeForm(int ProjectId, int ClientId, int FormId, long EmployeeId)
        {
            try
            {
                if (ProjectId != 0 && ClientId != 0 && FormId != 0 && EmployeeId != 0)
                {
                    var employeeFormModel = db.EmployeeForms.Where(i => i.EmployeeId == EmployeeId && i.ProjectId == ProjectId && i.ClientId == ClientId && i.FormId == FormId).FirstOrDefault();
                    if (employeeFormModel == null)
                        return false;
                    else
                    {
                        employeeFormModel.UpdatedOn = Common.GetDateTime(db);
                        employeeFormModel.IsDeleted = true;
                        db.Entry(employeeFormModel).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        return true;
                    }
                }
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

        [Route("api/updateEmployeeForms")]
        [HttpPost]
        public List<ClientDataModel> UpdateEmployeeForms(UpdateEmpFormsModel model)
        {
            try
            {
                var empId = model.EmployeeId;
                foreach (var item in model.EmployeeForms)
                {
                    var form = db.EmployeeForms.Where(i => i.EmployeeId == empId && i.FormId == item.FormId && i.ClientId == item.ClientId && i.ProjectId == item.ProjectId).FirstOrDefault();
                    if (form != null)
                    {
                        form.UpdatedOn = Common.GetDateTime(db);
                        form.IsDeleted = item.IsDeleted;
                        db.Entry(form).State = System.Data.Entity.EntityState.Modified;
                    }
                    else
                    {
                        EmployeeForm empForm = new EmployeeForm();
                        empForm.EmployeeId = empId;
                        empForm.FormId = item.FormId;
                        empForm.ClientId = item.ClientId;
                        empForm.ProjectId = item.ProjectId;
                        empForm.IsDeleted = item.IsDeleted;
                        empForm.CreatedBy = empId;
                        empForm.CreatedOn = Common.GetDateTime(db);
                        empForm.UpdatedBy = empId;
                        empForm.UpdatedOn = Common.GetDateTime(db);
                        db.EmployeeForms.Add(empForm);
                    }
                }
                db.SaveChanges();
                return GetEmployeeProjectForms(empId);
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }

        [Route("api/GetEmployeeProjectForms")]
        [HttpPost]
        public List<ClientDataModel> GetEmployeeProjectForms(long employeeId)
        {
            try
            {
                EmployeeService _employeeService = new EmployeeService();
                EmployeeProjectFormDateModel empProDataModel = new EmployeeProjectFormDateModel();
                empProDataModel.EmployeeId = (int)employeeId;
                empProDataModel.DateTime = Convert.ToDateTime("2010-12-04");
                List<ClientDataModel> employeeProjectForms = _employeeService.EmployeeForms(empProDataModel);

                return employeeProjectForms;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }

        /// <summary>
        /// List of forms employee wise
        /// </summary>
        /// <param name="id">subscriber Id. Ex. 1</param>
        /// <returns>List containing Id, Name, Description, CreatedOn, UpdatedOn,TotalSubmission,WebFormUID,IsActive,IsDashboard,WorkFlowEnabled,IsPrivateForm</returns>
        // GET: api/Forms
        [Route("api/EmployeeForms/{id}")]
        [HttpGet]
        public object EmployeeForms(long id)
        {
            try
            {
                List<FormModel> forms = db.EmployeeForms.Where(i => i.EmployeeId == id && !i.IsDeleted).AsEnumerable()
                    .Select(m => new FormModel
                    {
                        Id = m.Form.Id,
                        Name = m.Form.Name,
                        Description = m.Form.Description,
                        CreatedOn = m.Form.CreatedOn,
                        UpdatedOn = m.Form.UpdatedOn,
                        TotalSubmission = m.Form.FormSubmissions.Where(a => !a.IsDeleted).Count(),
                        WebFormUID = m.Form.WebFormUID,
                        IsActive = m.Form.IsActive,
                        IsDashboard = m.Form.Dashboards.Any(),
                        WorkFlowEnabled = m.Form.WorkFlowEnabled,
                        IsPrivateForm = m.Form.IsPrivateForm == null ? false : m.Form.IsPrivateForm,
                    }).OrderByDescending(a => a.CreatedOn).ToList();
                //Added by Hiren 19-11-2017
                foreach (var item in forms)
                {
                    var empIds = db.UserReportToes.Where(q => q.ReportToEmpId == id).AsEnumerable().Select(i => i.EmpId).Distinct().ToList();
                    empIds.Add(0);
                    empIds.Add(id);
                    var submissionList = (from f in db.Forms
                                          join fs in db.FormSubmissions on f.Id equals fs.FormId
                                          join sl in db.SubmissionLogs on fs.Id equals sl.SubmissionId
                                          where empIds.Contains(sl.EmployeeId) && fs.FormId == item.Id && fs.IsDeleted == false
                                          select new
                                          {
                                              fs.Id,
                                              sl.EmployeeId
                                          }).ToList();
                    item.TotalSubmission = submissionList.Select(i => i.Id).Distinct().Count();
                }
                //End
                return forms;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }
        #endregion

        #region Login ######
        [AllowAnonymous]
        [Route("api/logout/{appLoginId}")]
        [HttpGet]
        public bool Logout(Int64 appLoginId)
        {
            try
            {
                //LoginLog loginLog = new LoginLog();
                //loginLog.DeviceId = "";
                //loginLog.LoggedOn = Common.GetDateTime(db);
                //loginLog.IpAddress = GetLanIPAddress();
                //loginLog.Action = Constants.LoggedOut;
                //loginLog.AppLoginId = appLoginId;
                //db.LoginLogs.Add(loginLog);
                //db.SaveChanges();
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
        /// Check for login
        /// </summary>
        /// <param name="email">Email address of user</param>
        /// <param name="password">Password of user</param>
        /// <param name="deviceId">DeviceId of mobile/tablet</param>
        /// <returns>
        /// SubscriberLogo, FileUrl, OpetationCount, AppLoginId, RoleId, RoleValue, SubscriberId, Employee, IsReset, ResetLink
        /// FileUrl -> Use to get file from this url.
        /// OpetationCount -> No of operations.
        /// IsReset -> will be true in case of forgot password.
        /// ResetLink -> In case of forgot password Reset link will be sent.
        /// </returns>
        /// 
        [Route("api/checklogin")]
        [HttpGet]
        [AllowAnonymous]
        public IHttpActionResult CheckLogin(string email, string password, string deviceId)
        {
            try
            {
                ApiReturnData model = new ApiReturnData();
                var appLogin = new AppLogin();
                string pwd = General.Encrypt(password);

                bool isWebReq = string.IsNullOrWhiteSpace(deviceId) ? true : false;
                if (isWebReq)
                    appLogin = db.AppLogins.Where(e => e.Email == email && e.Password == pwd && !e.IsDeleted && e.IsWebEnabled == true).FirstOrDefault();
                else
                    appLogin = db.AppLogins.Where(e => e.Email == email && e.Password == pwd && !e.IsDeleted && e.IsMobileEnabled == true).FirstOrDefault();
                if (appLogin != null)
                {
                    LoginLog loginLog = new LoginLog();
                    loginLog.DeviceId = deviceId;
                    loginLog.LoggedOn = Common.GetDateTime(db);
                    loginLog.IpAddress = GetLanIPAddress();
                    loginLog.Action = Constants.LoggedIn;
                    loginLog.AppLoginId = appLogin.Id;
                    db.LoginLogs.Add(loginLog);
                    db.SaveChanges();
                    EmployeeRole empRolesModel = appLogin.EmployeeRoles.OrderBy(o => o.RoleId).FirstOrDefault();
                    var operationCount = db.Operations.Where(e => e.SubscriberId == appLogin.SubscriberId).Count();
                    int roleValue = empRolesModel != null && empRolesModel.Role != null ? empRolesModel.Role.Value : 0;
                    int roleId = empRolesModel != null && empRolesModel.Role != null ? empRolesModel.Role.Id : 0;
                    Employee employee = appLogin.Employees.Select(i => new Employee
                    {
                        Id = i.Id,
                        PayrollId = i.PayrollId,
                        FirstName = i.FirstName,
                        LastName = i.LastName,
                        ProfilePicture = i.ProfilePicture,
                        BirthDate = i.BirthDate,
                        PhoneNumber = i.PhoneNumber,
                        AppLoginId = i.AppLoginId,
                        SubscriberId = i.SubscriberId,
                        SystemRoleId = i.SystemRoleId
                    }).FirstOrDefault();

                    if (operationCount == 0)
                    {
                        var root = db.AppSettings.Where(s => s.Key.ToLower() == Constants.FileRoot.ToLower()).FirstOrDefault().Value;
                        root = root + appLogin.SubscriberId + Constants.ImageFolder + Constants.EmployeeFolder;
                        Directory.CreateDirectory(root);
                    }

                    //Updated by jay mistry on 8-12-2015
                    List<AppSetting> appSettingList = db.AppSettings.Where(a => a.SubscriberId == employee.SubscriberId).ToList();
                    string fileUrl = appSettingList.Where(a => a.Key.ToLower() == Constants.FileUrl.ToLower()).FirstOrDefault().Value;
                    //end
                    string ResetLink = "";
                    if (appLogin.IsReset != null && appLogin.IsReset == true)
                    {
                        ResetLink = appLogin.ResetLink.ToString();
                    }
                    //added by jay //add new parameter IsReset
                    object dbUser;
                    AppSetting appSettingTokenDetails = appSettingList.Where(a => a.Key.ToLower() == Constants.APP_AUTH_KEY.ToLower()).FirstOrDefault();
                    string SecureWebForm = appSettingList.Where(a => a.Key.ToLower() == Constants.SECURE_WEB_FORM.ToLower()).FirstOrDefault().Value;
                    string subscriberToken = "";
                    var token = "";
                    if (appSettingTokenDetails != null)
                    {
                        subscriberToken = appSettingTokenDetails.Value;
                        token = CreateToken(subscriberToken, out dbUser);
                    }
                    else
                    {
                        model.Success = false;
                        model.Message = "Error on generating Token.";
                        model.SuccessData = new { SubscriberLogo = appLogin.Subscriber.CompanyLogo, FileUrl = fileUrl, OpetationCount = operationCount, AppLoginId = appLogin.Id, RoleId = roleId, RoleValue = roleValue, SubscriberId = appLogin.SubscriberId, Employee = employee, IsReset = appLogin.IsReset, ResetLink = ResetLink, Token = token, SecureWebForm = SecureWebForm };
                        return Ok(model);
                    }
                    //return new { SubscriberLogo = appLogin.Subscriber.CompanyLogo, FileUrl = fileUrl, OpetationCount = operationCount, AppLoginId = appLogin.Id, RoleId = roleId, RoleValue = roleValue, SubscriberId = appLogin.SubscriberId, Employee = employee, IsReset = appLogin.IsReset, ResetLink = ResetLink, Token = token };
                    model.Success = true;
                    model.Message = "";
                    model.SuccessData = new { SubscriberLogo = appLogin.Subscriber.CompanyLogo, FileUrl = fileUrl, OpetationCount = operationCount, AppLoginId = appLogin.Id, RoleId = roleId, RoleValue = roleValue, SubscriberId = appLogin.SubscriberId, Employee = employee, IsReset = appLogin.IsReset, ResetLink = ResetLink, Token = token, SecureWebForm = SecureWebForm };
                    return Ok(model);
                }
                else
                {
                    model.Success = false;
                    model.Message = "The email or password provided is incorrect.";
                    model.SuccessData = null;
                    return Ok(model);
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
                return Ok(model);
            }
        }

        public string GetLanIPAddress()
        {
            string ipaddress;
            ipaddress = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (ipaddress == "" || ipaddress == null)
                ipaddress = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];

            //Get the Host Name
            string stringHostName = Dns.GetHostName();
            //Get The Ip Host Entry
            IPHostEntry ipHostEntries = Dns.GetHostEntry(stringHostName);
            //Get The Ip Address From The Ip Host Entry Address List
            IPAddress[] arrIpAddress = ipHostEntries.AddressList;
            //return arrIpAddress[1].ToString();
            return ipaddress;
        }

        [Route("api/emailexist")]
        [HttpGet]
        public bool CheckEmailExist(int id, string email)
        {
            try
            {
                var appLogin = db.AppLogins.Where(e => e.Id != id && e.Email == email).FirstOrDefault();
                if (appLogin == null)
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

        [Route("api/changepassword")]
        [HttpPost]
        public bool ChangePassword(int id, string oldPassword, string newPassword)
        {
            try
            {
                string password = General.Encrypt(oldPassword);
                Employee employee = db.Employees.Where(e => e.Id == id && e.AppLogin.Password == password && e.AppLogin.IsDeleted == false).FirstOrDefault();
                if (employee == null)
                {
                    return false;
                }
                employee.AppLogin.Password = General.Encrypt(newPassword);
                employee.UpdatedOn = Common.GetDateTime(db);
                db.Entry(employee).State = EntityState.Modified;
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

        public bool ForgotPassWordMailTemplate(string name, string email, int languageId, AppLogin appLogin)
        {
            try
            {
                string strTemplate = "";
                string subscriberLogo = "";
                string fileUrl = "";
                string emailSignature = "";
                int subscriberId = db.AppLogins.Where(i => i.Id == appLogin.Id).FirstOrDefault().SubscriberId;
                Subscriber subscriberdbModel = db.Subscribers.Where(i => i.Id == subscriberId).FirstOrDefault();
                fileUrl = subscriberdbModel.AppSettings.Where(i => i.Key == Constants.FileUrl).FirstOrDefault().Value;
                subscriberLogo = !string.IsNullOrEmpty(subscriberdbModel.CompanyLogo) ? fileUrl + subscriberdbModel.Id + Constants.ImageFolder + Constants.SubscriberFolder + subscriberdbModel.CompanyLogo : "";
                List<AppSetting> appSettingList = subscriberdbModel.AppSettings.ToList();
                AppSetting appSettingModel = appSettingList.Where(i => i.Key == Constants.EmailSignature).FirstOrDefault();
                if (appSettingModel == null)
                {
                    return false;
                }
                else
                {
                    emailSignature = appSettingModel.Value;
                }
                StreamReader fp = new StreamReader(System.Web.HttpContext.Current.Server.MapPath("~/MailTemplates/Languages/" + languageId + "/ForgotPassword.html"));
                strTemplate = fp.ReadToEnd();
                fp.Close();
                string password = General.CreatePassword();
                strTemplate = strTemplate.Replace("@Name", name).Replace("@Password", password).Replace("@EmailSignature", emailSignature).Replace("@CompanyLogo", subscriberLogo).Replace("@UrlLink", !string.IsNullOrEmpty(subscriberdbModel.PreferredDomain) ? subscriberdbModel.PreferredDomain : subscriberdbModel.SubDomain + "." + Constants.FORMZI_DOMAIN);
                Helper.General.SendEmail(email, strTemplate, "Forgot Password");
                appLogin.Password = General.Encrypt(password);
                //Added by jay //Add IsReset field in database set to true.
                appLogin.IsReset = true;
                appLogin.ResetLink = Guid.NewGuid();
                //End
                db.Entry(appLogin).State = EntityState.Modified;
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

        //Added by Jay 
        //Add parameter bool IsMobile
        [AllowAnonymous]
        [Route("api/forgotPassword")]
        [HttpGet]
        public bool ForgotPassword(string email, int languageId, bool isMobile)
        {
            try
            {
                var appLogin = new AppLogin();
                if (isMobile)
                    appLogin = db.AppLogins.Where(a => a.Email == email && a.IsMobileEnabled && !a.IsDeleted).FirstOrDefault();
                else
                    appLogin = db.AppLogins.Where(a => a.Email == email && a.IsWebEnabled && !a.IsDeleted).FirstOrDefault();

                if (appLogin == null)
                    return false;
                else
                {
                    var employee = appLogin.Employees.FirstOrDefault();
                    int baseLanguage = languageId;
                    if (isMobile)
                    {
                        baseLanguage = db.SubscriberLanguages
                            .Where(i => i.SubcriberId == appLogin.SubscriberId)
                            .OrderBy(o => o.DisplayOrder).FirstOrDefault().LanguageId;
                    }
                    if (employee != null)
                        return ForgotPassWordMailTemplate(employee.FirstName + " " + employee.LastName, appLogin.Email, baseLanguage, appLogin);
                    else
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

        [Route("api/resetpassword")]
        [HttpPost]
        public bool ResetPassword(int id, string newPassword, string ResetLink)
        {
            try
            {
                Guid _resetLink = new Guid(ResetLink);
                AppLogin appLogin = db.AppLogins.Where(e => e.Id == id && e.ResetLink == _resetLink).FirstOrDefault();
                if (appLogin == null)
                    return false;
                appLogin.Password = General.Encrypt(newPassword);
                //Added by jay
                appLogin.IsReset = false;
                appLogin.ResetLink = null;
                //End
                db.Entry(appLogin).State = EntityState.Modified;
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
        /// Create a Jwt with user information
        /// </summary>
        /// <param name="subscriberToken"></param>
        /// <param name="dbUser"></param>
        /// <returns></returns>
        private static string CreateToken(string subscriberToken, out object dbUser)
        {
            var unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var expiry = Math.Round((DateTime.UtcNow.AddHours(2) - unixEpoch).TotalSeconds);
            var issuedAt = Math.Round((DateTime.UtcNow - unixEpoch).TotalSeconds);
            var notBefore = Math.Round((DateTime.UtcNow.AddMonths(6) - unixEpoch).TotalSeconds);

            var payload = new Dictionary<string, object>
            {
                ////{"email", user.Email},
                ////{"userId", user.Id},
                ////{"role", "Admin"  },
                ////{"sub", user.Id},
                ////{"nbf", notBefore},
                ////{"iat", issuedAt},
                ////{"exp", expiry}

                {"token", subscriberToken }
            };

            //var secret = ConfigurationManager.AppSettings.Get("jwtKey");
            const string apikey = "secretKey";

            var token = JsonWebToken.Encode(payload, apikey, JwtHashAlgorithm.HS256);

            dbUser = new { subscriberToken };
            return token;
        }

        /// <summary>
        /// User List For Particular Role (Added By Hiren 10-11-2017)
        /// </summary>
        /// <param name="subscriberId">subscriberId</param>
        /// <param name="roleId">roleId</param>
        /// <returns>User List</returns>
        [Route("api/UserListForRole")]
        [HttpGet]
        public object UserListForRole(int subscriberId, int roleId)
        {
            try
            {
                RoleReportsTo roleReportTo = db.RoleReportsToes.Where(r => r.RoleId == roleId).FirstOrDefault();
                List<UserReportToModel> userList = new List<UserReportToModel>();
                if (roleReportTo != null)
                {
                    userList = (from emp in db.Employees
                                join app in db.AppLogins on emp.AppLoginId equals app.Id
                                join emprole in db.EmployeeRoles on emp.AppLoginId equals emprole.AppLoginId
                                join rrt in db.RoleReportsToes on emprole.RoleId equals rrt.ReportToId
                                where emp.SubscriberId == subscriberId && rrt.IsDeleted == false && rrt.ReportToId == roleReportTo.ReportToId && rrt.RoleId == roleId
                                select new UserReportToModel
                                {
                                    Id = emp.Id,
                                    Name = emp.FirstName + " " + emp.LastName,
                                    ReportToId = rrt.ReportToId,
                                    RoleId = rrt.RoleId,
                                }).ToList();
                }
                return userList;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }
        #endregion ######

        #region Login Mobile Specific

        /// <summary>
        /// Mobile Application specific
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <param name="deviceId"></param>
        /// <returns>User details</returns>
        [AllowAnonymous]
        [Route("api/AppCheckLogin")]
        [HttpGet, HttpPost]
        public object CheckLoginFormzi(string email = null, string password = null, string deviceId = null, int apploginId = 0, int subscriberId = 0)
        {
            try
            {
                List<AppLogin> appLogin = new List<AppLogin>();
                ApiReturnData model = new ApiReturnData();
                string pwd = General.Encrypt(password);

                if (apploginId > 0 && subscriberId > 0)
                    appLogin = db.AppLogins.Where(e => e.Id == subscriberId && e.SubscriberId == subscriberId).ToList();
                else
                    appLogin = db.AppLogins.Where(e => e.Email == email && e.Password == pwd && !e.IsDeleted && e.IsMobileEnabled == true).ToList();

                if (appLogin.Any())
                {
                    if (appLogin.Count == 1)
                    {
                        if (appLogin.FirstOrDefault().IsReset != null && appLogin.FirstOrDefault().IsReset == true)
                        {
                            model.Success = false;
                            model.Message = "You have request for forgot password, please check your email for more process.";
                            model.SuccessData = null;
                            return model;
                        }
                        LoginLog loginLog = new LoginLog();
                        loginLog.DeviceId = deviceId;
                        loginLog.LoggedOn = Common.GetDateTime(db);
                        loginLog.IpAddress = GetLanIPAddress();
                        loginLog.Action = Constants.LoggedIn;
                        loginLog.AppLoginId = appLogin.FirstOrDefault().Id;
                        db.LoginLogs.Add(loginLog);
                        db.SaveChanges();
                        model.Success = true;
                        model.SuccessData = SubscribersDetails(appLogin, false);
                        return model;
                    }
                    else
                    {
                        if (appLogin.Where(x => x.IsReset != null && x.IsReset == true).ToList().Count() > 0)
                        {
                            model.Success = false;
                            model.Message = "You have request for forgot password, please check your email for more process.";
                            model.SuccessData = null;
                            return model;
                        }
                        SubscriberService subscriberService = new SubscriberService();
                        model.Success = true;
                        model.SuccessData = new
                        {
                            IsMultipleSubscriberUser = true,
                            Subscribers = subscriberService.GetSubscribers(appLogin)
                        };
                        return model;
                    }
                }
                else
                {
                    model.Success = false;
                    model.Message = "The email or password provided is incorrect.";
                    model.SuccessData = null;
                    return model;
                }
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                ApiReturnData model = new ApiReturnData();
                model.Success = false;
                model.Message = "The email or password provided is incorrect.";
                model.SuccessData = null;
                return model;
            }
        }

        public object SubscribersDetails(List<AppLogin> appLogin, bool IsMultipleSubscriber = false)
        {
            var fileUrl = appLogin.FirstOrDefault().Subscriber.AppSettings.Where(i => i.Key == Constants.FileUrl).FirstOrDefault().Value;

            //Formzi.Controllers.ClientController ClientObj = new ClientController();
            EmployeeProjectFormDateModel epfObj = new EmployeeProjectFormDateModel();
            ClientServices clientService = new ClientServices();
            SubscriberService subscriberService = new SubscriberService();
            epfObj.EmployeeId = (int)appLogin.FirstOrDefault().Employees.FirstOrDefault().Id;
            epfObj.DateTime = new DateTime(1990, 1, 1);

            long SubscriberId = appLogin.FirstOrDefault().Employees.FirstOrDefault().SubscriberId;

            List<AppSetting> appSettingList = db.AppSettings.Where(a => a.SubscriberId == SubscriberId).ToList();
            object dbUser;
            string subscriberToken = appSettingList.Where(a => a.Key.ToLower() == Constants.APP_AUTH_KEY.ToLower()).FirstOrDefault().Value;
            var token = CreateToken(subscriberToken, out dbUser);

            string profilePic;
            if (string.IsNullOrEmpty(appLogin.FirstOrDefault().Employees.FirstOrDefault().ProfilePicture))
                profilePic = "";
            else
                profilePic = fileUrl + appLogin.FirstOrDefault().Employees.FirstOrDefault().SubscriberId + Constants.ImageFolder + Constants.EmployeeFolder + appLogin.FirstOrDefault().Employees.FirstOrDefault().Id + "/" + appLogin.FirstOrDefault().Employees.FirstOrDefault().ProfilePicture;

            string CompanyLogo;
            if (string.IsNullOrEmpty(appLogin.FirstOrDefault().Subscriber.CompanyLogo))
                CompanyLogo = "";
            else
                CompanyLogo = fileUrl + appLogin.FirstOrDefault().Employees.FirstOrDefault().SubscriberId + Constants.ImageFolder + Constants.SubscriberFolder + appLogin.FirstOrDefault().Subscriber.CompanyLogo;

            return (object)new
            {
                IsMultipleSubscriberUser = IsMultipleSubscriber,
                Id = appLogin.FirstOrDefault().Employees.FirstOrDefault().Id,
                PayrollId = appLogin.FirstOrDefault().Employees.FirstOrDefault().PayrollId,
                FirstName = appLogin.FirstOrDefault().Employees.FirstOrDefault().FirstName,
                LastName = appLogin.FirstOrDefault().Employees.FirstOrDefault().LastName,
                ProfilePicture = profilePic,
                BirthDate = appLogin.FirstOrDefault().Employees.FirstOrDefault().BirthDate,
                PhoneNumber = appLogin.FirstOrDefault().Employees.FirstOrDefault().PhoneNumber,
                AppLoginId = appLogin.FirstOrDefault().Employees.FirstOrDefault().AppLoginId,
                SubscriberId = appLogin.FirstOrDefault().Employees.FirstOrDefault().SubscriberId,
                SubscriberLogo = CompanyLogo,
                ClientList = clientService.GetCPF(epfObj),
                TimeStamp = Common.GetDateTime(db),
                Token = token,
                SystemRoleId = appLogin.FirstOrDefault().Employees.FirstOrDefault().SystemRoleId,
                SubscriberLanguages = subscriberService.GetSubscriberLanguages(SubscriberId)
            };
        }

        /// <summary>
        /// Mobile specific. Forgot password.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="languageId"></param>
        /// <param name="isMobile">1 for mobile, 0 for web</param>
        /// <returns></returns>
        [AllowAnonymous]
        [Route("api/AppForgotPassword")]
        [HttpGet]
        public ApiReturnData ForgotPasswordFormzi(string email, int languageId, bool isMobile)
        {
            try
            {
                var appLogin = new AppLogin();
                ApiReturnData model = new ApiReturnData();
                if (isMobile)
                    appLogin = db.AppLogins.Where(a => a.Email == email && a.IsMobileEnabled && !a.IsDeleted).FirstOrDefault();
                else
                    appLogin = db.AppLogins.Where(a => a.Email == email && a.IsWebEnabled && !a.IsDeleted).FirstOrDefault();

                if (appLogin == null)
                {
                    model.Success = false;
                    model.Message = "The email addresss " + email + " is not registered with us.";
                    model.SuccessData = null;
                    return model;
                }
                else
                {
                    var employee = appLogin.Employees.FirstOrDefault();
                    int baseLanguage = languageId;
                    if (isMobile)
                    {
                        baseLanguage = db.SubscriberLanguages
                            .Where(i => i.SubcriberId == appLogin.SubscriberId)
                            .OrderBy(o => o.DisplayOrder).FirstOrDefault().LanguageId;
                    }
                    if (employee != null)
                    {
                        bool IsMailSent = ForgotPassWordMailTemplate(employee.FirstName + " " + employee.LastName, appLogin.Email, baseLanguage, appLogin);

                        if (IsMailSent)
                        {
                            model.Success = true;
                            model.Message = "We have sent you mail on " + email + " with a link to reset you password.";
                            model.SuccessData = null;
                            return model;
                        }
                        else
                        {
                            model.Success = false;
                            model.Message = "Can not able to send email on given email address.";
                            model.SuccessData = null;
                            return model;
                        }
                    }
                    else
                    {
                        model.Success = false;
                        model.Message = "The email addresss " + email + " is not registered with us.";
                        model.SuccessData = null;
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
                model.Message = "An error occurred. Please try agian later";
                model.SuccessData = null;
                return model;
            }
        }

        /// <summary>
        /// Check Employee Forms Exist Or Not (Added By Hiren 31-10-2017)
        /// </summary>
        /// <param name="id">WebFormUID</param>
        /// <param name="employeeId">employeeId</param>
        /// <returns>True/False</returns>
        [Route("api/employeeFormExist")]
        [HttpGet]
        public bool CheckEmployeeFormsExist(string id, int employeeId)
        {
            try
            {
                var form = db.Forms.Where(f => f.WebFormUID == id).FirstOrDefault();
                var employeeForm = db.EmployeeForms.Where(e => e.FormId == form.Id && e.EmployeeId == employeeId).FirstOrDefault();
                if (employeeForm == null)
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
        #endregion

        #region I-Witness Specific ######

        /// <summary>
        /// I-Witness specific - No more in use.
        /// Resolve issue from i-witness.org
        /// Check login credential
        /// added by jay
        /// </summary>
        /// <param name="subscriberId"></param>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <param name="formId">it's form submission id</param>
        /// <returns></returns>
        [AllowAnonymous]
        //[Route("api/IWitnessLoginAuthenticate")]
        [HttpGet]
        public object IWitnessLoginAuthenticate(int subscriberId, string email, string password, long formId)
        {
            try
            {
                string pwd = General.Encrypt(password);
                var appLogin = db.AppLogins.Where(e => e.Email == email && e.Password == pwd && !e.IsDeleted && e.IsWebEnabled == true).FirstOrDefault();
                if (appLogin != null)
                {
                    LoginLog loginLog = new LoginLog();
                    loginLog.DeviceId = "";
                    loginLog.LoggedOn = Common.GetDateTime(db);
                    loginLog.IpAddress = GetLanIPAddress();
                    loginLog.Action = Constants.LoggedIn;
                    loginLog.AppLoginId = appLogin.Id;
                    db.LoginLogs.Add(loginLog);
                    db.SaveChanges();

                    long AppLoginId = appLogin.Id;
                    var Employee = db.Employees.Where(i => i.AppLoginId == AppLoginId).FirstOrDefault();

                    var FormSubmissionDetails = db.FormSubmissions.Where(i => i.Id == formId && i.IsApproved != 2).FirstOrDefault();

                    long FormId = 0;
                    if (FormSubmissionDetails == null)
                    {
                        return new { IsSuccess = false, Message = "Issue is not available for resolve." };
                    }
                    else
                    {
                        FormId = FormSubmissionDetails.FormId;
                    }
                    long EmployeeId = Employee.Id;
                    if (db.EmployeeForms.Where(i => i.EmployeeId == EmployeeId && i.FormId == FormId & !i.IsDeleted).Any())
                    {
                        return new { IsSuccess = true, AppLoginId = AppLoginId };
                    }
                    else
                    {
                        return new { IsSuccess = false, Message = "You don't have permission to resolve this issue." };
                    }
                }
                else
                {
                    return new { IsSuccess = false, Message = "Wrong email address or password." };
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