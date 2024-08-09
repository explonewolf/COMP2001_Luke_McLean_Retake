using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging; // Added for logging
using Microsoft.OpenApi.Models;
using System;
using WebApplication1.Data;
using WebApplication1.Services;

namespace WebApplication1
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add distributed memory cache for session management
            services.AddDistributedMemoryCache();

            // Configure session state
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(20);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // Add database context with SQL Server configuration
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            // Add HttpClient service for API calls
            services.AddHttpClient();

            // Register application services (like authentication service)
            services.AddScoped<IAuthService, AuthService>();

            // Configure MVC with controllers and views
            services.AddControllersWithViews();
            services.AddRazorPages();

            // Add and configure Swagger for API documentation
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Your API",
                    Version = "v1",
                    Description = "Description of your API"
                });
                c.DescribeAllParametersInCamelCase();
            });

            // Add CORS policy to allow requests from any origin
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins",
                    builder =>
                    {
                        builder.AllowAnyOrigin()
                               .AllowAnyMethod()
                               .AllowAnyHeader();
                    });
            });

            // Configure authentication with cookies
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.Cookie.Name = "YourAppAuthCookie";
                    options.LoginPath = "/Login";
                    options.AccessDeniedPath = "/AccessDenied";
                });

            // Add logging service
            services.AddLogging();

            // Add authorization service
            services.AddAuthorization();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger) // Add logger here
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            // Enable HTTPS redirection
            app.UseHttpsRedirection();

            // Enable serving static files
            app.UseStaticFiles();

            // Configure routing
            app.UseRouting();

            // Enable CORS
            app.UseCors("AllowAllOrigins");

            // Enable authentication and authorization
            app.UseAuthentication();
            app.UseAuthorization();

            // Enable session management
            app.UseSession();

            // Log a message when the application starts
            logger.LogInformation("Application started at {Time}", DateTime.UtcNow);

            // Middleware to protect pages, redirecting to login if not authenticated
            app.Use(async (context, next) =>
            {
                var path = context.Request.Path.Value;

                // Log request path and authentication status
                logger.LogInformation("Request for path: {Path}, IsAuthenticated: {IsAuthenticated}", path, context.User.Identity.IsAuthenticated);

                // Check if the user is authenticated
                if (path != "/Login" && !context.User.Identity.IsAuthenticated)
                {
                    // Redirect to login page if not authenticated
                    logger.LogWarning("User is not authenticated, redirecting to Login page.");
                    context.Response.Redirect("/Login");
                    return;
                }

                await next();
            });

            // Configure endpoints
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                endpoints.MapRazorPages();
            });

            // Enable Swagger middleware for API documentation
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Your API v1");
                c.RoutePrefix = string.Empty; // Serve Swagger UI at the root
            });
        }
    }
}
