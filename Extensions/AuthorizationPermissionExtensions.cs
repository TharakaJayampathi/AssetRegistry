using AssetRegistry.Handlers;
using Microsoft.IdentityModel.Tokens;
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
                options.AddPolicy("Company.Read", policy =>
                    policy.Requirements.Add(new PermissionRequirement("Company.Read")));

                options.AddPolicy("User.Read", policy =>
                    policy.Requirements.Add(new PermissionRequirement("User.Read")));
            });

            return services;
        }
    }
}
