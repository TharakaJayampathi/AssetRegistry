namespace AssetRegistry.DTOs.Roles
{
    public class RoleUpdateDTO
    {
        public string RoleId { get; set; }
        public string Code { get; set; }
        public string RoleName { get; set; }
        public bool IsActive { get; set; }
        public List<int> Permissions { get; set; }
    }
}
