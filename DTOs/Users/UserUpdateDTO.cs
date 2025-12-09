namespace AssetRegistry.DTOs.Users
{
    public class UserUpdateDTO
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string RoleId { get; set; }
        public int CompanyId { get; set; }
        public int DivisionId { get; set; }
        public int LocationId { get; set; }
        public string? Password { get; set; }
        public bool IsActive { get; set; }
    }
}
