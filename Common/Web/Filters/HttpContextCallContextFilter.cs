using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Argus.Common.Data;

namespace Argus.Common.Web.Filters
{
    public class HttpContextCallContextFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            // Store the current HttpContext in CallContext
            CallContext.SetData("HttpContext", context.HttpContext);
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            // Optionally clear it after the request
            // CallContext.SetData("HttpContext", null);
        }
    }
}
