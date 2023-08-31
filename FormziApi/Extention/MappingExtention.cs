using AutoMapper;
using FormziApi.Database;
using FormziApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FormziApi.Extention
{
    public static class MappingExtention
    {
        public static TDestination ToModel<TSource, TDestination>(this TSource source)
        {
            return Mapper.Map<TDestination>(source);
        }

        public static List<TDestination> ToListModel<TSource, TDestination>(this List<TSource> source)
        {
            return Mapper.Map<List<TDestination>>(source);
        }

        //public static Employee ToEntity(this EmployeeModel source, Employee employee)
        //{
        //    return Mapper.Map<EmployeeModel, Employee>(source, employee);
        //}

        public static TDestination ToEntity<TSource, TDestination>(this TSource source)
        {
            return Mapper.Map<TDestination>(source);
        }

        public static TDestination ToEntity<TSource, TDestination>(this TSource source, TDestination entity)
        {
            return Mapper.Map<TSource, TDestination>(source, entity);
        }
    }
}