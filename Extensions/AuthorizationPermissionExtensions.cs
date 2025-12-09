using AssetRegistry.Handlers;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

namespace AssetRegistry.Extensions
{
    public static class AuthorizationPermissionExtensions
    {
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, string secret)
        {
            services
                .AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ClockSkew = TimeSpan.Zero
                    };
                });

            return services;
        }

        public static IServiceCollection AddAuthorizationPermissions(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy("User.Read", policy =>
                    policy.Requirements.Add(new PermissionRequirement("User.Read")));
                options.AddPolicy("User.Create", policy =>
                    policy.Requirements.Add(new PermissionRequirement("User.Create")));
                options.AddPolicy("User.Update", policy =>
                    policy.Requirements.Add(new PermissionRequirement("User.Update")));
                options.AddPolicy("User.Delete", policy =>
                    policy.Requirements.Add(new PermissionRequirement("User.Delete")));

                options.AddPolicy("Role.Read", policy =>
                    policy.Requirements.Add(new PermissionRequirement("Role.Read")));
                options.AddPolicy("Role.Create", policy =>
                    policy.Requirements.Add(new PermissionRequirement("Role.Create")));
                options.AddPolicy("Role.Update", policy =>
                    policy.Requirements.Add(new PermissionRequirement("Role.Update")));
                options.AddPolicy("Role.Delete", policy =>
                    policy.Requirements.Add(new PermissionRequirement("Role.Delete")));

                options.AddPolicy("Company.Read", policy =>
                    policy.Requirements.Add(new PermissionRequirement("Company.Read")));
                options.AddPolicy("Company.Create", policy =>
                    policy.Requirements.Add(new PermissionRequirement("Company.Create")));
                options.AddPolicy("Company.Update", policy =>
                    policy.Requirements.Add(new PermissionRequirement("Company.Update")));
                options.AddPolicy("Company.Delete", policy =>
                    policy.Requirements.Add(new PermissionRequirement("Company.Delete")));

                options.AddPolicy("Division.Read", policy =>
                    policy.Requirements.Add(new PermissionRequirement("Division.Read")));
                options.AddPolicy("Division.Create", policy =>
                    policy.Requirements.Add(new PermissionRequirement("Division.Create")));
                options.AddPolicy("Division.Update", policy =>
                    policy.Requirements.Add(new PermissionRequirement("Division.Update")));
                options.AddPolicy("Division.Delete", policy =>
                    policy.Requirements.Add(new PermissionRequirement("Division.Delete")));

                options.AddPolicy("Location.Read", policy =>
                    policy.Requirements.Add(new PermissionRequirement("Location.Read")));
                options.AddPolicy("Location.Create", policy =>
                    policy.Requirements.Add(new PermissionRequirement("Location.Create")));
                options.AddPolicy("Location.Update", policy =>
                    policy.Requirements.Add(new PermissionRequirement("Location.Update")));
                options.AddPolicy("Location.Delete", policy =>
                    policy.Requirements.Add(new PermissionRequirement("Location.Delete")));
            });

            return services;
        }

        public static IServiceCollection AddSwaggerWithJwt(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                // Enable JWT authorization input box in Swagger
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter JWT like: Bearer {your token}"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[]{}
                    }
                });
            });

            return services;
        }
    }
}
