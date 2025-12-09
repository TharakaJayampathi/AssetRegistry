namespace AssetRegistry.DTOs.Role
{
    public class RoleListDTO
    {
        public string? Id { get; set; }
        public string? RoleId { get; set; }
        public string? RoleName { get; set; }
        public List<string> Permissions { get; set; }
        public bool IsActive { get; set; }
    }
}
