
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
using FormziApi.Models;
using System.Web;
using FormziApi.Extention;
using Newtonsoft.Json.Linq;

namespace FormziApi.Controllers
{
    [Authorize]
    [EnableCors("*", "*", "*")]
    public class OperationController : ApiController
    {
        #region Fields

        private LogProvider lp = new LogProvider("Formzi");
        private FormziEntities db = new FormziEntities(); 
        
        #endregion

        #region Methods

        [Route("api/operationList")]
        [HttpGet]
        public object GetOperationList(int subscriberId, int appLoginId, int roleValue, int roleId)
        {
            try
            {
                var operation = db.Operations.Where(m => !m.IsDeleted && m.SubscriberId == subscriberId).OrderByDescending(t => t.CreatedOn);

                return operation.ToList().Select(x =>
                {
                    var model = x.ToModel<Operation, OperationModel>();
                    if (roleValue == Convert.ToInt16(Constants.Roles.SubscriberAdmin))
                    {
                        int operationAdminRoleValue = Convert.ToInt16(Constants.Roles.OperationAdmin);
                        int operationAdminRoleId = db.Roles.Where(r => r.Value == operationAdminRoleValue && r.SubscriberId == subscriberId).FirstOrDefault().Id;//Changed By Hiren 29-11-2017
                        var employeeRole = db.EmployeeRoles.Where(t => t.OperationId == x.Id && t.RoleId == operationAdminRoleId).FirstOrDefault();
                        if (employeeRole != null)
                        {
                            var employee = db.Employees.Where(t => t.AppLoginId == employeeRole.AppLoginId).FirstOrDefault();
                            model.ManagerName = employee.FirstName + " " + employee.LastName;
                        }
                        else
                        {
                            model.ManagerName = "-";
                        }
                        model.NoOfLocations = db.Locations.Where(e => e.OperationId == x.Id && !e.Address.IsDeleted).Count();
                    }
                    else if (roleValue == Convert.ToInt16(Constants.Roles.OperationAdmin))
                    {
                        var employeeRole = db.EmployeeRoles.Where(t => t.OperationId == x.Id && t.RoleId == roleId && t.AppLoginId == appLoginId).FirstOrDefault();
                        if (employeeRole != null)
                        {
                            var employee = db.Employees.Where(t => t.AppLoginId == employeeRole.AppLoginId).FirstOrDefault();
                            model.ManagerName = employee.FirstName + " " + employee.LastName;
                            model.NoOfLocations = db.Locations.Where(e => e.OperationId == x.Id && !e.Address.IsDeleted).Count();
                        }
                        else
                        {
                            model = null;
                        }
                    }

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

        [Route("api/checkOperationName")]
        [HttpGet]
        public bool CheckOperationName(string name, int subscriberId, int operationId)
        {
            try
            {
                int count = db.Operations.Where(e => e.Name.ToLower() == name.ToLower() && e.SubscriberId == subscriberId && !e.IsDeleted && e.Id != operationId).Count();
                if (count == 0)
                    return false;
                else
                    return true;

            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return false;
            }
        }

        [Route("api/checkLocationName")]
        [HttpGet]
        public bool CheckLocationName(string name, int operationId, int locationId)
        {
            try
            {
                int count = db.Locations.Where(e => e.Name.ToLower() == name.ToLower() && e.OperationId == operationId && e.Id != locationId && !e.Operation.IsDeleted).Count();
                if (count == 0)
                    return false;
                else
                    return true;

            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return false;
            }
        }

        [Route("api/getOperation/{id}")]
        [HttpGet]
        public object GetOPerationById(int id)
        {
            try
            {
                OperationModel operation = db.Operations.Where(e => e.Id == id && e.IsDeleted == false).FirstOrDefault().ToModel<Operation, OperationModel>();
                operation.Locations = GetLocationByOperationId(id);
                operation.OperationSettings = db.OperationSettings.Where(e => e.OperationId == id).ToList().ToListModel<OperationSetting, OperationSettingModel>();
                return operation;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }

        [Route("api/editOperationInfo")]
        [HttpPost]
        public object EditOperationInfo(JObject jObject)
        {
            try
            {
                OperationModel operationModel = jObject["operationModel"].ToObject<OperationModel>();
                Operation operation = db.Operations.Find(operationModel.Id);
                operation.UpdatedOn = Common.GetDateTime(db);
                operation.Name = operationModel.Name;
                operation.IsActive = operationModel.IsActive;
                db.Entry(operation).State = EntityState.Modified;

                if (jObject["employeeRoleModel"] != null && jObject["employeeRoleModel"].Count() > 0)
                {
                    EmployeeRoleModel employeeRoleModel = jObject["employeeRoleModel"].ToObject<EmployeeRoleModel>();
                    EmployeeRole newEmployeeRole = new EmployeeRole();
                    newEmployeeRole.AppLoginId = employeeRoleModel.AppLoginId;
                    newEmployeeRole.OperationId = operation.Id;
                    newEmployeeRole.RoleId = employeeRoleModel.RoleId;
                    db.EmployeeRoles.Add(newEmployeeRole);

                    EmployeeRole operationAdmin = db.EmployeeRoles.Where(e => e.RoleId == employeeRoleModel.RoleId && e.OperationId == operation.Id).FirstOrDefault();
                    if (operationAdmin != null)
                    {
                        int mobileUser = Convert.ToInt16(Constants.Roles.MobileUser);
                        mobileUser = db.Roles.Where(r => r.Value == mobileUser && r.SubscriberId == operation.SubscriberId).FirstOrDefault().Id;//Changed By Hiren 29-11-2017
                        EmployeeRole defaultUser = db.EmployeeRoles.Where(e => e.RoleId == mobileUser && e.AppLoginId == operationAdmin.AppLoginId).FirstOrDefault();
                        if (defaultUser != null)
                        {
                            db.EmployeeRoles.Remove(operationAdmin);
                        }
                        else
                        {
                            operationAdmin.RoleId = mobileUser;
                            operationAdmin.OperationId = 0;
                            db.Entry(operationAdmin).State = EntityState.Modified;
                        }
                    }
                }
                db.SaveChanges();
                var locations = GetLocationByOperationId(operation.Id);
                return locations;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return false;
            }
        }

        [Route("api/addoperation")]
        [HttpPost]
        public int PostOperationInfo(JObject jObject)
        {
            try
            {
                if (jObject == null)
                    return 0;

                OperationModel operationModel = jObject["operationModel"].ToObject<OperationModel>();
                AddressModel addressModel = jObject["addressModel"].ToObject<AddressModel>();
                LocationModel locationModel = jObject["locationModel"].ToObject<LocationModel>();
                List<OperationSettingModel> operationSettingsModel = jObject["operationSettingsModel"].ToObject<List<OperationSettingModel>>();

                operationModel.CreatedOn = operationModel.UpdatedOn = Common.GetDateTime(db);
                Operation operation = operationModel.ToEntity<OperationModel, Operation>();
                db.Operations.Add(operation);
                db.SaveChanges();

                addressModel.CreatedOn = addressModel.UpdatedOn = Common.GetDateTime(db);
                Address address = addressModel.ToEntity<AddressModel, Address>();
                db.Addresses.Add(address);
                db.SaveChanges();

                locationModel.OperationId = operation.Id;
                locationModel.AddressId = address.Id;
                Location location = locationModel.ToEntity<LocationModel, Location>();
                db.Locations.Add(location);
                db.SaveChanges();

                foreach (OperationSettingModel operationSettingModel in operationSettingsModel)
                {
                    operationSettingModel.OperationId = operation.Id;
                    OperationSetting operationSetting = operationSettingModel.ToEntity<OperationSettingModel, OperationSetting>();
                    db.OperationSettings.Add(operationSetting);
                    db.SaveChanges();//change by jay
                }
                if (jObject["employeeRoleModel"] != null && jObject["employeeRoleModel"].HasValues)//change by jay
                {
                    EmployeeRoleModel employeeRoleModel = jObject["employeeRoleModel"].ToObject<EmployeeRoleModel>();
                    EmployeeRole newEmployeeRole = new EmployeeRole();
                    newEmployeeRole.AppLoginId = employeeRoleModel.AppLoginId;
                    newEmployeeRole.OperationId = operation.Id;
                    newEmployeeRole.RoleId = employeeRoleModel.RoleId;
                    db.EmployeeRoles.Add(newEmployeeRole);
                    db.SaveChanges();//change by jay
                }

                return operation.Id;

            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return 0;
            }
        }

        [Route("api/operationsettings")]
        [HttpPost]
        public object PutOperationSettings(JObject jObject)
        {
            try
            {
                if (jObject == null)
                    return null;
                //db.OperationSettings
                OperationModel operationModel = jObject["operationModel"].ToObject<OperationModel>();
                List<OperationSettingModel> operationSettingsModel = jObject["operationSettingsModel"].ToObject<List<OperationSettingModel>>();
                List<OperationSetting> addSettings = operationSettingsModel.ToListModel<OperationSettingModel, OperationSetting>();
                var removeSettings = db.OperationSettings.Where(o => o.OperationId == operationModel.Id).ToList();
                // Bulk remove 
                db.OperationSettings.RemoveRange(removeSettings);
                // Bulk Insert
                db.OperationSettings.AddRange(addSettings);
                db.SaveChanges();
                var locations = GetLocationByOperationId(operationModel.Id);
                return locations;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return false;
            }
        }

        [Route("api/operationlocations")]
        [HttpDelete]
        public object DeleteClientLocation(int id)
        {
            try
            {
                Address location = db.Addresses.Find(id);
                if (location == null)
                {
                    return NotFound();
                }
                location.CreatedOn = Common.GetDateTime(db);
                location.IsDeleted = true;
                db.Entry(location).State = EntityState.Modified;
                db.SaveChanges();
                return GetOperationLocation(location.Locations.Where(t => t.AddressId == location.Id).FirstOrDefault().OperationId);
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }

        [Route("api/operations")]
        [HttpDelete]
        public object DeleteOperation(int id, int appLoginId, int roleValue, int roleId)
        {
            try
            {
                Operation operations = db.Operations.Find(id);
                if (operations == null)
                {
                    return NotFound();
                }
                operations.UpdatedOn = Common.GetDateTime(db);
                operations.IsDeleted = true;
                db.Entry(operations).State = EntityState.Modified;
                db.SaveChanges();
                return GetOperationList(operations.SubscriberId, appLoginId, roleValue, roleId);
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }

        //Added by jay mistry 29-10-2015
        [Route("api/EmployeeOperationLanguages/{employeeId}")]
        [HttpGet]
        public object EmployeeOperationLanguages(long employeeId)
        {
            var employeeLocationDetails = db.EmployeeLocations.Where(i => i.EmployeeId == employeeId).FirstOrDefault();

            if (employeeLocationDetails != null)
            {
                var languageIdList = employeeLocationDetails.Location.Operation.OperationSettings.Where(i => i.Key == Constants.OperatingLanguage).Select(i => i.Value).ToList();
                var languageList = db.Languages.AsEnumerable().Where(i => languageIdList.Contains(i.Id.ToString())).Select(i => new LanguageNameModel
                {
                    Id = i.Id,
                    Name = i.Name
                }).ToList();
                LanguageListModel model = new LanguageListModel
                {
                    languages = languageList,
                };
                return model;
            }
            return null;
        }

        #endregion

        #region Operation Location

        [Route("api/getOperationLocation/{operationId}")]
        [HttpGet]
        public List<LocationModel> GetLocationByOperationId(int operationId)
        {
            try
            {
                var locations = db.Locations.Where(e => e.OperationId == operationId && !e.Address.IsDeleted).ToList().ToListModel<Location, LocationModel>();
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

        [Route("api/RemoveOperationLocation/{addressId}")]
        [HttpGet]
        public object DeleteOperationLocation(int addressId)
        {
            try
            {
                Address address = db.Addresses.Find(addressId);
                int operationId = address.Locations.FirstOrDefault().OperationId;
                if (address == null)
                {
                    return null;
                }
                address.UpdatedOn = Common.GetDateTime(db);
                address.IsDeleted = true;
                db.Entry(address).State = EntityState.Modified;
                db.SaveChanges();
                db = new FormziEntities();
                var locations = GetLocationByOperationId(operationId);
                return locations;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }

        [Route("api/locationList/{operationId}")]
        [HttpGet]
        public object GetOperationLocation(int operationId)
        {
            try
            {
                var location = db.Locations.Where(m => m.OperationId == operationId).Select(m => new
                {
                    m.AddressId,
                    m.Name,
                    Address = db.Addresses.Where(a => a.Id == m.AddressId && a.IsDeleted == false).Select(t => new
                    {
                        t.Id,
                        t.Address1,
                        t.Address2,
                        t.PhoneNumber,
                        t.FaxNumber,
                        t.City,
                        t.CountryId,
                        t.StateProvinceId,
                        t.ZipPostalCode,
                        t.CreatedOn
                    }).FirstOrDefault(),


                }).OrderByDescending(a => a.AddressId).ToList();
                return location;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }

        [Route("api/operationlocation")]
        [HttpPost]
        public object PostOperationLocation(JObject jObject)
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
                var locations = GetLocationByOperationId(location.OperationId);
                return locations;

            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }

        [Route("api/editOperationLocation")]
        [HttpPost]
        public object PutOperationLocation(Location location)
        {
            try
            {
                if (location == null)
                    return null;

                location.Address.UpdatedOn = Common.GetDateTime(db);
                db.Entry(location).State = EntityState.Modified;
                db.Entry(location.Address).State = EntityState.Modified;
                db.SaveChanges();
                db = new FormziEntities();
                var locations = GetLocationByOperationId(location.OperationId);
                return locations;

            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return 0;
            }

        }

        #endregion

    }
}