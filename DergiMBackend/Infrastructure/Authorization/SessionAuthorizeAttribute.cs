using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using DergiMBackend.Services.IServices;

namespace DergiMBackend.Authorization
{
    public class SessionAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var sessionService = context.HttpContext.RequestServices.GetService<ISessionService>();

            if (sessionService == null)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var sessionToken = context.HttpContext.Request.Headers["SessionToken"].FirstOrDefault();

            if (string.IsNullOrEmpty(sessionToken))
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            try
            {
                sessionService.ValidateSessionTokenAsync(sessionToken).GetAwaiter().GetResult();
            }
            catch
            {
                context.Result = new UnauthorizedResult();
            }
        }
    }
}
