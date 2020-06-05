﻿using Atlass.Framework.Core.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atlass.Framework.Core.Base
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class IPFilterAttribute: ActionFilterAttribute
    {
        private readonly IAtlassReuqestHelper RequestHelper;
        public IPFilterAttribute(IAtlassReuqestHelper atlassReuqest)
        {
            RequestHelper = atlassReuqest;
        }
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (context == null)
            {
                throw new ArgumentNullException("filterContext");
            }

            //访问记录 
            var visit = RequestHelper.Visit();
            VisitQueueInstance.Add(visit);

            //允许AllowAnonymous匿名访问
            var controllerActionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;
            if (controllerActionDescriptor != null)
            {

                var isDefined = controllerActionDescriptor.ControllerTypeInfo.GetCustomAttributes(inherit: true)
                    .Any(a => a.GetType().Equals(typeof(AllowAnonymousAttribute)));

                if (isDefined)
                {

                }
            }


            await base.OnActionExecutionAsync(context, next);
        }
    }
}