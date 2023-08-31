using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FormziApi.Database;

namespace FormziApi.Models
{
    public class EmployeeModel
    {
        public long Id { get; set; }
        public string PayrollId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public bool Gender { get; set; }
        public string ProfilePicture { get; set; }
        public Nullable<System.DateTime> BirthDate { get; set; }
        public string PhoneNumber { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public System.DateTime UpdatedOn { get; set; }
        public bool IsDeleted { get; set; }
        public long AppLoginId { get; set; }
        public int SubscriberId { get; set; }
        public int SystemRoleId { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public string Email { get; set; } //added by jay
        public string LastLogin2 { get; set; }//added by jay
        public DateTime LastLogin { get; set; }
        public AppLoginModel AppLogin { get; set; }
        public List<DocumentModel> Documents { get; set; }
        public List<EmployeeLocationModel> EmployeeLocations { get; set; }
        public List<FormModel> FormList { get; set; }
        public virtual List<EmployeeForm> EmployeeForms { get; set; }
        public List<ClientDataModel> EmployeeProjectForms { get; set; }
        public virtual List<FormModelLess> AddedForms { get; set; }
        public virtual List<FormModelLess> RemovedForms { get; set; }
        public string Title { get; set; }
        public long ReportingEmployeeId { get; set; }
        public int OperationId {get;set;}//Added By Hiren 21-11-2017

        public DateTime LastPassedDate { get; set; }

        public AddressModel Address { get; set; }

        public string OperationLocationName { get; set; }

        //Added By Hiren 21-11-2017
        public long CreatedBy { get; set; }
        public long UpdatedBy { get; set; }
    }
    //Added by jay mistry 29-10-2015
    public class LanguageListModel
    {
        public List<LanguageNameModel> languages { get; set; }
    }
    public class LanguageNameModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class EmployeeFormModel
    {
        public int Id { get; set; }
        public long EmployeeId { get; set; }
        public int ClientId { get; set; }
        public int ProjectId { get; set; }
        public long FormId { get; set; }
        public bool IsDeleted { get; set; }
        public long CreatedBy { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public long UpdatedBy { get; set; }
        public System.DateTime UpdatedOn { get; set; }
    }
    public class EmployeeFormList
    {
        public List<EmployeeFormModel> EmployeeForms { get; set; }
        public List<FormVersionModel> FormVersions { get; set; }
        public List<FormsList> Forms { get; set; }
    }
    public class EmployeeProjectForms
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public int ClientId { get; set; }
        public string ClientName { get; set; }

        public long FormId { get; set; }
        public string FormName { get; set; }

        public int EmployeeId { get; set; }

        //public List<FormByName> Forms { get; set; }
    }
}