using AssetRegistry.Handlers;

namespace AssetRegistry.Extensions
{
    public static class AuthorizationPermissionExtensions
    {
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
