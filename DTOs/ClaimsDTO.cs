namespace AssetRegistry.DTOs
{
    internal class ClaimsDTO
    {
    }

    public class RoleClaim
    {
        public string ClaimType { get; set; }
        public bool IsSelected { get; set; }
    }

    public class RoleClaimView
    {
        public string RoleId { get; set; }
        public IEnumerable<RoleClaim> RoleClaims { get; set; }
    }

    public class UserClaim
    {
        public string ClaimType { get; set; }
        public string ClaimValue { get; set; }
        public bool IsSelected { get; set; }
    }

    public class UserCliamsView
    {
        public string UserId { get; set; }
        public IEnumerable<UserClaim> UserClaims { get; set; }
    }
}
