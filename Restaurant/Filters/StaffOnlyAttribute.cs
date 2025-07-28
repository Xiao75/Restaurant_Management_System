using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace REstaurant.Filters
{
    public class StaffOnlyAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var httpContext = context.HttpContext;
            var session = httpContext.Session;

            var isStaff = httpContext.Session.GetInt32("IsStaff") == 1;
            var isAdmin = httpContext.Session.GetInt32("IsAdmin") == 1;
            var isSuperAdmin = httpContext.Session.GetInt32("IsSuperAdmin") == 1;

            if(!isStaff && !isAdmin && ! isSuperAdmin)
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Home", null);
            }
        }
    }
}