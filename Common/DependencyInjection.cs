using AssetRegistry.Models;
using AssetRegistry.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Runtime.Serialization;

namespace AssetRegistry.Common
{
    public static class DependencyInjection
    {
        public static IServiceCollection RegisterApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            return services;
        }

        public static IServiceCollection RegisterInfrastructerServices(this IServiceCollection services, IConfiguration configuration)
        {
            //services.AddDbContext<ApplicationDbContext>(options =>
            //   options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));


            //services.AddAuthentication(opt =>
            //{
            //    //opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            //    //opt.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            //    //opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            //    opt.DefaultScheme = "BROWNS_AUTH_SCHEME";
            //    opt.DefaultChallengeScheme = "BROWNS_AUTH_SCHEME";

            //})
            //.AddCookie("Cookies", options =>
            //{
            //    //options.LoginPath = "/login";
            //    options.ExpireTimeSpan = TimeSpan.FromDays(1);
            //})
            //.AddJwtBearer(options =>
            //{
            //    options.RequireHttpsMetadata = false;
            //    options.SaveToken = true;
            //    options.TokenValidationParameters = new TokenValidationParameters()
            //    {
            //        ValidateIssuer = false,
            //        ValidateAudience = false,
            //        ValidateIssuerSigningKey = true,
            //        ClockSkew = TimeSpan.Zero,
            //        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetSection("Jwt")["Secret"]))
            //    };

            //    options.Events = new JwtBearerEvents
            //    {
            //        OnAuthenticationFailed = context => {
            //            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            //            {
            //                context.Response.Headers.Add("IS-TOKEN-EXPIRED", "true");
            //            }
            //            return Task.CompletedTask;
            //        }
            //    };
            //})
            //.AddPolicyScheme("BROWNS_AUTH_SCHEME", "BROWNS_AUTH_SCHEME", options =>
            //{
            //    // runs on each request
            //    options.ForwardDefaultSelector = context =>
            //    {
            //        // filter by auth type
            //        string authorization = context.Request.Headers[HeaderNames.Authorization];
            //        string origin = context.Request.Headers[HeaderNames.Origin];
            //        if (origin == "api")
            //        {
            //            return JwtBearerDefaults.AuthenticationScheme;
            //        }
            //        else
            //        {
            //            return IdentityConstants.ApplicationScheme;
            //        }
            //        //if (!string.IsNullOrEmpty(authorization) && authorization.StartsWith("Bearer "))
            //        //    return JwtBearerDefaults.AuthenticationScheme;

            //        //// otherwise always check for cookie auth
            //        ////return "Cookies";

            //    };
            //});

            #region Identity Config
            services.Configure<IdentityOptions>(options =>
            {
                // Password settings.
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 1;

                // Lockout settings.
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(60);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                // User settings.
                options.User.AllowedUserNameCharacters =
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@";
                options.User.RequireUniqueEmail = false;
            });
            #endregion

            services.AddAuthorization(options =>
            {
                options.AddPolicy("ManageUsers", policy => policy.RequireAssertion
                    (
                        context => context.User.HasClaim(c => c.Type == "ManageUsers.View")
                        && context.User.HasClaim(c => c.Type == "ManageUsers.Create" || c.Type == "ManageUsers.Update" || c.Type == "ManageUsers.Delete" || c.Type == "ManageUsers.ResetPassword") || context.User.IsInRole("SuperAdmin")
                   ));

                options.AddPolicy("ManageCompanies", policy => policy.RequireAssertion
                    (
                        context => context.User.HasClaim(c => c.Type == "ManageCompanies.View")
                        && context.User.HasClaim(c => c.Type == "ManageCompanies.Create" || c.Type == "ManageCompanies.Update" || c.Type == "ManageCompanies.Delete") || context.User.IsInRole("SuperAdmin")
                   ));
            });

            #region Repositories
            #endregion

            ////Google Auth Config Come Below
            return services;
        }

        public static IServiceCollection RegisterAzureServices(this IServiceCollection services, IConfiguration configuration)
        {
            return services;
        }

        public static IServiceCollection RegisterMessageBusServices(this IServiceCollection services, IConfiguration configuration)
        {
            return services;
        }
    }
}
