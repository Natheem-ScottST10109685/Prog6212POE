using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ST10109685_PROGPOEPART2.Controllers;
using ST10109685_PROGPOEPART2.Models;
using System.Globalization;

namespace ST10109685_PROGPOEPART2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure global culture settings
            var cultureInfo = new CultureInfo("en-US");
            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Retrieve ClaimDirectory from appsettings.json
            var claimDirectory = builder.Configuration["ClaimDirectory"];
            if (string.IsNullOrEmpty(claimDirectory))
            {
                throw new InvalidOperationException("ClaimDirectory is not configured in appsettings.json.");
            }

            // Register the IClaimService with the configured ClaimDirectory
            builder.Services.AddScoped<IClaimService>(provider => new ClaimService(claimDirectory));

            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Home/Login"; // Redirect here if not authenticated
                    options.AccessDeniedPath = "/Account/AccessDenied"; // Redirect here if access is denied
                });

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("LecturerOnly", policy => policy.RequireRole("Lecturer"));
                options.AddPolicy("CoordinatorOnly", policy => policy.RequireRole("ProgrammeCoordinator", "AcademicManager"));
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // Add authentication and authorization middleware
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
