
using System;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Routing;

namespace Ralltiir.Server.Helpers
{
    public sealed class QueryParamRequiredAttribute : ActionMethodSelectorAttribute
    {
        private readonly string _name;

        public QueryParamRequiredAttribute(string name)
        {
            _name = name;
        }

        public override bool IsValidForRequest(RouteContext context, ActionDescriptor action)
        {
            if (!context.HttpContext.Request.Query.ContainsKey(_name)) return false;
            
            return !string.IsNullOrEmpty(context.HttpContext.Request.Query[_name]);
        }
    }
}


