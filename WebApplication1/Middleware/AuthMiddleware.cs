using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System.Threading.Tasks;
using WebApplication1.Models;
using WebApplication1.Services;

namespace WebApplication1.Middleware
{
    public class AuthMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceScopeFactory _scopeFactory;

        public AuthMiddleware(RequestDelegate next, IServiceScopeFactory scopeFactory)
        {
            _next = next;
            _scopeFactory = scopeFactory;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Check if the request is for Swagger and the user is not authenticated
            if (context.Request.Path.StartsWithSegments("/swagger") && !context.User.Identity.IsAuthenticated)
            {
                // Extract user credentials from the query parameters
                var userName = context.Request.Query["userName"].ToString();
                var email = context.Request.Query["email"].ToString();
                var password = context.Request.Query["password"].ToString();

                // Validate the extracted credentials
                var loginModel = new LoginModel
                {
                    UserName = userName,
                    Email = email,
                    Password = password
                };

                // Create a scope to resolve the scoped service
                using (var scope = _scopeFactory.CreateScope())
                {
                    var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
                    var isAuthenticated = await authService.VerifyUserAsync(loginModel);

                    if (!isAuthenticated)
                    {
                        // If not authenticated, return 401 Unauthorized
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsync("Unauthorized");
                        return;
                    }
                }
            }

            // If authenticated or not accessing Swagger, proceed to the next middleware
            await _next(context);
        }
    }
}
