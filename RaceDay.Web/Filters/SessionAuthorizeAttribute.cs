
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RaceDay.Web.Filters
{
    public class SessionAuthorizeAttribute : IAsyncActionFilter
    {
        private readonly string? _requiredRole;

        public SessionAuthorizeAttribute(string? requiredRole = null)
        {
            _requiredRole = requiredRole;
        }

        public async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            var session = context.HttpContext.Session;

            var token = session.GetString("Token");
            var role = session.GetString("Role");

            if (string.IsNullOrEmpty(token))
            {
                context.Result = new RedirectToActionResult(
                    "Login", "Account", null);
                return;
            }

            if (!string.IsNullOrEmpty(_requiredRole) &&
                !string.Equals(role, _requiredRole,
                    StringComparison.OrdinalIgnoreCase))
            {
                context.Result = new RedirectToActionResult(
                    "Index", "Home", null);
                return;
            }

            await next();
        }
    }
}
