﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Restaurant.Filters
{
    public class AdminOnlyAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var httpContext = context.HttpContext;
            var isAdmin = httpContext.Session.GetInt32("IsAdmin") == 1;
            var isSuperAdmin = httpContext.Session.GetInt32("IsSuperAdmin") == 1;

            if (!isAdmin && !isSuperAdmin)
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Home", null);

            }
        }
    }

}