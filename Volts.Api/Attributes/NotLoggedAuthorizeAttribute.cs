using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

namespace Volts.Api.Attributes
{
    public class NotLoggedAuthorizeAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            // Não autenticado
            if (user?.Identity is null || !user.Identity.IsAuthenticated)
            {
                context.Result = new JsonResult(new { reason = "not-logged" })
                {
                    StatusCode = (int)HttpStatusCode.Unauthorized
                };
                return;
            }
        }
    }
}
