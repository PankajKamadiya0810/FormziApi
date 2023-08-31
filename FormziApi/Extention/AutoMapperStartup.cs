using AutoMapper;
using FormziApi.Database;
using FormziApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FormziApi.Extenstion
{
    public class AutoMapperStartup
    {
        public virtual void RegisterMapping()
        {
            // Entity to Model

            Mapper.CreateMap<Address, AddressModel>();

            Mapper.CreateMap<Role, RoleModel>();

            Mapper.CreateMap<Employee, EmployeeModel>()
                .ForMember(dest => dest.AppLogin, opt => opt.Ignore())
                .ForMember(dest => dest.EmployeeLocations, opt => opt.Ignore())
                .ForMember(dest => dest.Documents, opt => opt.Ignore())
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(x => (x.FirstName + " " + x.LastName)));

            Mapper.CreateMap<Operation, OperationModel>()
                .ForMember(dest => dest.Locations, opt => opt.Ignore())
                .ForMember(dest => dest.OperationSettings, opt => opt.Ignore())
                .ForMember(dest => dest.NoOfLocations, opt => opt.Ignore())
                .ForMember(dest => dest.ManagerName, opt => opt.Ignore());
            

            Mapper.CreateMap<Location, LocationModel>()
                .ForMember(dest => dest.Address, opt => opt.Ignore());

            Mapper.CreateMap<OperationSetting, OperationSettingModel>();

            Mapper.CreateMap<Subscriber, SubscriberModel>()
                .ForMember(dest => dest.Address, opt => opt.Ignore())
                .ForMember(dest => dest.ProfilePic, opt => opt.Ignore())
                 .ForMember(dest => dest.EmployeeId, opt => opt.Ignore())
                .ForMember(dest => dest.Email, opt => opt.Ignore());

            Mapper.CreateMap<SubscriptionPlan, SubscriptionPlanModel>();

            Mapper.CreateMap<AppLogin, AppLoginModel>()
                .ForMember(dest => dest.EmployeeRoles, opt => opt.Ignore());

            Mapper.CreateMap<EmployeeRole, EmployeeRoleModel>();

            Mapper.CreateMap<EmployeeLocation, EmployeeLocationModel>();

            Mapper.CreateMap<Document, DocumentModel>();

            Mapper.CreateMap<FormSubmission, FormSubmissionModel>()
                .ForMember(dest => dest.FormName, opt => opt.Ignore());

            Mapper.CreateMap<FormAnswer, FormAnswerModel>();

            Mapper.CreateMap<Form, FormModel>();

            Mapper.CreateMap<FormQuestion, FormQuestionsModel>()
                .ForMember(dest => dest.FormAnswers, opt => opt.Ignore());

            Mapper.CreateMap<Client, ClientModel>();

            // Model to Entity

            Mapper.CreateMap<ClientModel, Client>();

            Mapper.CreateMap<FormAnswerModel, FormAnswer>();

            Mapper.CreateMap<FormSubmissionModel, FormSubmission>();

            Mapper.CreateMap<EmployeeModel, Employee>();

            Mapper.CreateMap<OperationModel, Operation>();

            Mapper.CreateMap<AddressModel, Address>();

            Mapper.CreateMap<LocationModel, Location>();
                

            Mapper.CreateMap<OperationSettingModel, OperationSetting>();

            Mapper.CreateMap<EmployeeRoleModel, EmployeeRole>();

            Mapper.CreateMap<AppLoginModel, AppLogin>();

            Mapper.CreateMap<EmployeeLocationModel, EmployeeLocation>();

            Mapper.CreateMap<SubscriberModel, Subscriber>();

            Mapper.CreateMap<DocumentModel, Document>();
        }
    }
}