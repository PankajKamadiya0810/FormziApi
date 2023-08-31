using FormziApi.Database;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web;

namespace FormziApi
{
    public static class Common
    {
        public static DateTime GetDateTime(FormziEntities db)
        {
            //return DateTime.SpecifyKind(((IObjectContextAdapter)db).ObjectContext.CreateQuery<DateTime>("CurrentDateTime()").AsEnumerable().First(), DateTimeKind.Utc);
            return ((IObjectContextAdapter)db).ObjectContext.CreateQuery<DateTime>("CurrentDateTime()").AsEnumerable().First().ToUniversalTime();
        }
    }
}