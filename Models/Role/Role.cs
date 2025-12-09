using Microsoft.AspNetCore.Identity;

namespace AssetRegistry.Models.Role
{
    public class Role : IdentityRole
    {
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}
