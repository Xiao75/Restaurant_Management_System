using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Restaurant.Filters
{
    public class SuperAdminOnlyAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var httpContext = context.HttpContext;
            var session = httpContext.Session;

            var isSuperAdmin = httpContext.Session.GetInt32("IsSuperAdmin") == 1;

            if (!isSuperAdmin)
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Home", null);

            }
        }
    }
}