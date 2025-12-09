using Microsoft.AspNetCore.Authorization;

namespace AssetRegistry.Attributes
{
    public sealed class HasPermissionAttribute : AuthorizeAttribute
    {
        public HasPermissionAttribute(string permission)
            : base(policy: permission)
        {
        }
    }
}
