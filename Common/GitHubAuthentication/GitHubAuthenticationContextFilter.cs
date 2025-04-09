using Argus.Common.Data;
using Argus.Data;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Argus.Common.GitHubAuthentication
{
    public class GitHubAuthenticationContextFilter : ActionFilterAttribute
    {
        public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var token = context.HttpContext.Request.Headers["x-github-token"].ToString();
            CallContext.SetData(ServiceConstants.Authentication.GitHubTokenKey, token);
            CallContext.SetData(ServiceConstants.Authentication.UserNameKey, context.HttpContext.User.Identities.First(i => !string.IsNullOrEmpty(i.Name)).Name);
            await next();
        }
    }
}
