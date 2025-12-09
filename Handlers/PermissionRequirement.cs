using Microsoft.AspNetCore.Authorization;

namespace AssetRegistry.Handlers
{
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public PermissionRequirement(string permission)
        {
            Permission = permission;
        }

        public string Permission { get; }
    }
}
