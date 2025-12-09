using Microsoft.AspNetCore.Identity;

namespace AssetRegistry.Models.Roles
{
    public class Role : IdentityRole
    {
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
