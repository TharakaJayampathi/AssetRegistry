namespace AssetRegistry.DTOs.Users
{
    public class UserCreateDTO
    {
        public string? Code { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? RoleId { get; set; }
        public int CompanyId { get; set; }
        public int DivisionId { get; set; }
        public int LocationId { get; set; }
        public string? Password { get; set; }
    }
}
