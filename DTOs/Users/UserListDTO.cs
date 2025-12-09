namespace AssetRegistry.DTOs.Users
{
    public class UserListDTO
    {
        public string? Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Nic { get; set; }
        public string? Address { get; set; }
        public bool IsActive { get; set; }
        public string? RoleId { get; set; }
        public string? RoleName { get; set; }
    }
}
