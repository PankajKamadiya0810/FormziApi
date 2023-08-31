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
using Newtonsoft.Json;

namespace FormziApi.Controllers
{
    [Authorize]
    [EnableCors("*", "*", "*")]
    public class RoleController : ApiController
    {
        #region Fields

        LogProvider lp = new LogProvider("Formzi");
        private FormziEntities db = new FormziEntities();

        #endregion

        #region Methods

        // GET: api/roles
        [Route("api/roles")]
        [HttpGet]
        public object GetRoles()
        {
            try
            {
                return db.Roles.ToList().ToListModel<Role, RoleModel>();
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }

        //Created by jay mistry on 14-07-2016
        #region Roles CRUD

        [Route("api/roles/{subscriberId}")]
        [HttpGet]
        public object Roles(int subscriberId = 0)
        {
            try
            {
                List<RoleModel> roles = db.Roles.Where(r => r.SubscriberId == subscriberId).AsEnumerable().Select(i => new RoleModel
                {
                    Id = i.Id,
                    Name = i.Name,
                    Value = i.Value,
                    SubscriberId = i.SubscriberId,
                    IsActive = i.IsActive,
                }).ToList();
                foreach (var item in roles)
                {
                    int reporttoId = db.RoleReportsToes.Where(rt => rt.RoleId == item.Id).AsEnumerable().Select(i => i.ReportToId).FirstOrDefault();
                    if (reporttoId != 0)
                    {
                        item.ReportToName = db.Roles.Where(r => r.Id == reporttoId).AsEnumerable().Select(i => i.Name).FirstOrDefault();
                    }
                    else
                    {
                        item.ReportToName = "";
                    }
                }
                return roles;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }

        //Changed By Hiren 7-11-2017
        [Route("api/role")]
        [HttpPost]
        public object AddRole(string data)
        {
            try
            {
                if (string.IsNullOrEmpty(data))
                    return null;
                RoleModel model = JsonConvert.DeserializeObject<RoleModel>(data);
                if (model == null)
                    return null;
                DateTime currentDate = Common.GetDateTime(db);
                if (db.Roles.Where(i => i.SubscriberId == model.SubscriberId && i.Name == model.Name).Any())
                {
                    return false;
                }

                Database.Role dbModel = new Database.Role();
                dbModel.Name = model.Name;
                dbModel.IsActive = model.IsActive;
                dbModel.SubscriberId = model.SubscriberId;
                dbModel.Value = model.Value;
                dbModel.CreatedBy = model.CreatedBy;
                dbModel.CreatedOn = currentDate;
                dbModel.UpdatedBy = model.CreatedBy;
                dbModel.UpdatedOn = currentDate;

                db.Roles.Add(dbModel);
                db.SaveChanges();

                //Added By Hiren 21-11-2017
                if (model.ReportToId != 0)
                {
                    RoleReportsTo rr = new RoleReportsTo();

                    rr.RoleId = dbModel.Id;
                    rr.ReportToId = model.ReportToId;
                    rr.IsDeleted = false;
                    rr.IsActive = true;
                    rr.CreatedBy = model.CreatedBy;
                    rr.CreatedOn = currentDate;
                    rr.UpdatedBy = model.CreatedBy;
                    rr.UpdatedOn = currentDate;
                    db.RoleReportsToes.Add(rr);
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

        //Changed By Hiren 23-11-2017
        [Route("api/role")]
        [HttpPut]
        public object UpdateRole(RoleModel model)
        {
            try
            {
                if (model == null)
                {
                    return false;
                }
                Role dbModel = new Database.Role();
                dbModel = db.Roles.Where(i => i.Id == model.Id).FirstOrDefault();

                if (dbModel == null)
                    return false;
                dbModel.Name = model.Name;
                dbModel.IsActive = model.IsActive;
                dbModel.SubscriberId = model.SubscriberId;
                dbModel.Value = model.Value;
                dbModel.UpdatedBy = model.CreatedBy;
                dbModel.UpdatedOn = model.UpdatedOn;
                db.Entry(dbModel).State = EntityState.Modified;
                db.SaveChanges();

                //Changed By Hiren 22-11-2017
                if (model.ReportToId != 0)
                {
                    RoleReportsTo dbRoleReportToModel = db.RoleReportsToes.Where(i => i.RoleId == dbModel.Id).FirstOrDefault();
                    if (dbRoleReportToModel != null)
                    {
                        dbRoleReportToModel.ReportToId = model.ReportToId;
                        dbRoleReportToModel.UpdatedOn = model.UpdatedOn;
                        dbRoleReportToModel.UpdatedBy = model.CreatedBy;
                        db.Entry(dbRoleReportToModel).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    else
                    {
                        RoleReportsTo rr = new RoleReportsTo();
                        rr.RoleId = dbModel.Id;
                        rr.ReportToId = model.ReportToId;
                        rr.IsDeleted = false;
                        rr.IsActive = true;
                        rr.CreatedBy = model.CreatedBy;
                        rr.CreatedOn = model.UpdatedOn;
                        rr.UpdatedBy = model.CreatedBy;
                        rr.UpdatedOn = model.UpdatedOn;
                        db.RoleReportsToes.Add(rr);
                        db.SaveChanges();
                    }
                }
                else
                {
                    //Remove the Role Report To
                    List<RoleReportsTo> roleReportToList = null;
                    roleReportToList = db.RoleReportsToes.Where(e => e.RoleId == dbModel.Id).ToList();
                    db.RoleReportsToes.RemoveRange(roleReportToList);
                    db.SaveChanges();
                }
                //End
                return true;
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return false;
            }
        }

        //Method is not in use
        [Route("api/role/{id}")]
        [HttpDelete]
        public object DeleteRole(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return false;
                }

                if (db.EmployeeRoles.Where(i => i.RoleId == id).Any())
                {
                    return false;
                }

                Role dbModel = db.Roles.Where(i => i.Id == id).FirstOrDefault();

                if (dbModel == null) return false;


                List<EmployeeRole> empRoleList = db.EmployeeRoles.Where(i => i.RoleId == id).ToList();

                if (empRoleList.Count > 0)
                {
                    db.EmployeeRoles.RemoveRange(empRoleList);
                    db.SaveChanges();
                }

                db.Roles.Remove(dbModel);
                db.SaveChanges();

                //dbModel.IsActive = false;
                //db.Entry(dbModel).State = EntityState.Modified;
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

        //Changed By Hiren 7-11-2017
        [Route("api/role")]
        [HttpGet]
        public object GetRole(int id = 0, int subscriberId = 0)
        {
            try
            {
                if (id == 0)
                {
                    return null;
                }
                //Changed By Hiren 6-11-2017
                Role roleModel = db.Roles.Where(i => i.Id == id && i.SubscriberId == subscriberId).FirstOrDefault();
                RoleModel model = new RoleModel();
                model.Id = roleModel.Id;
                model.Name = roleModel.Name;
                model.Value = roleModel.Value;
                model.SubscriberId = roleModel.SubscriberId;
                model.IsActive = roleModel.IsActive;

                RoleReportsTo roleReportToModel = db.RoleReportsToes.Where(i => i.RoleId == model.Id).FirstOrDefault();
                if (roleReportToModel != null)
                {
                    model.ReportToId = roleReportToModel.ReportToId;
                }
                else
                {
                    model.ReportToId = 0;
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

        //Changed By Hiren 18-11-2017
        [Route("api/roleByName")]
        [HttpGet]
        public bool RoleByName(int subscriberId, int id, string name)
        {
            try
            {
                var role = db.Roles.Where(r => r.Id != id && r.Name == name && r.SubscriberId == subscriberId).FirstOrDefault();
                if (role == null)
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

        /// <summary>
        /// Check the Role Value Exit Or Not (Added By Hiren 1-11-2017) //Method is not in use
        /// </summary>
        /// <param name="subscriberId">subscriberId</param>
        /// <param name="id">id</param>
        /// <param name="value">value</param>
        /// <returns>True/False</returns>
        [Route("api/roleByValue")]
        [HttpGet]
        public bool RoleByValue(int subscriberId, int id, int value)
        {
            try
            {
                var role = db.Roles.Where(r => r.Id != id && r.Value == value && r.SubscriberId == subscriberId).FirstOrDefault();
                if (role == null)
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

        /// <summary>
        /// Added By Hiren 21-11-2017
        /// </summary>
        /// <param name="roleId">roleId</param>
        /// <param name="subscriberId">subscriberId</param>
        /// <returns>Higher Role List</returns>
        [Route("api/rolesById")]
        [HttpGet]
        public object RolesById(int id = 0, int subscriberId = 0)
        {
            try
            {
                return db.Roles.Where(i => i.SubscriberId == subscriberId && i.Id != id).AsEnumerable().Select(i => new RoleModel
                {
                    Id = i.Id,
                    Name = i.Name,
                    Value = i.Value,
                    SubscriberId = i.SubscriberId,
                    IsActive = i.IsActive
                }).ToList();
            }
            catch (Exception e)
            {
                lp.Info(e.Message);
                lp.HandleError(e, e.Message);
                return null;
            }
        }
        #endregion

        #endregion
    }
}