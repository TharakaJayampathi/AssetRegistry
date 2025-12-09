namespace AssetRegistry.DTOs.Roles
{
    public class RoleCreateDTO
    {
        public string Code { get; set; }
        public string RoleName { get; set; }
        public List<RolePermissionCreateDTO> Permissions { get; set; }
    }
}
