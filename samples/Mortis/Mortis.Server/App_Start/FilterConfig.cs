﻿using System.Web;
using System.Web.Mvc;

namespace Mortis.Server
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
