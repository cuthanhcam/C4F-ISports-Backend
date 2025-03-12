using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace api.Middlewares
{
    public class RoleMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string[] _requiredRoles;

        public RoleMiddleware(RequestDelegate next, params string[] requiredRoles)
        {
            _next = next;
            _requiredRoles = requiredRoles;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var userRole = context.User.FindFirst(ClaimTypes.Role)?.Value;
            if (userRole == null || !_requiredRoles.Contains(userRole))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("You do not have permission to access this resource.");
                return;
            }

            await _next(context);
        }
    }

    public static class RoleMiddlewareExtensions
    {
        public static IApplicationBuilder UseRole(this IApplicationBuilder builder, params string[] roles)
        {
            return builder.UseMiddleware<RoleMiddleware>(roles);
        }
    }
}