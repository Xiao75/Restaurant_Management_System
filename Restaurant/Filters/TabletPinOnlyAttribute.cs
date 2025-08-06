using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Restaurant.Filters
{
    public class TabletPinOnlyAttribute : ActionFilterAttribute
    {
        private readonly string _sessionKey = "TabletPinVerified";

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var httpContext = context.HttpContext;

            // Get the current action and controller names
            var actionName = context.ActionDescriptor.RouteValues["action"];
            var controllerName = context.ActionDescriptor.RouteValues["controller"];

            // Allow access to EnterPin action without checking session
            if (controllerName == "TabletOrder" && actionName == "EnterPin")
            {
                return; // Skip filter
            }

            bool isPinVerified = httpContext.Session.GetString(_sessionKey) == "true";

            if (!isPinVerified)
            {
                context.Result = new RedirectToActionResult("EnterPin", "TabletOrder", null);
            }
        }
    }
}
