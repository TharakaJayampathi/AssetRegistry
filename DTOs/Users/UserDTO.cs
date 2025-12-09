namespace AssetRegistry.DTOs.Users
{
    public class UserDTO
    {
        public string? Id { get; set; }
        public string? UserId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? RoleName { get; set; }
        public string? CompanyName { get; set; }
        public string? DivisionId { get; set; }
        public string? DivisionName { get; set; }
        public string? LocationAddress { get; set; }
        public bool IsActive { get; set; }
    }
}
