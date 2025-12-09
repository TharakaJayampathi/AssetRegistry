namespace AssetRegistry.DTOs.LoginDTO
{
    public class LoginResponseDTO
    {
        public int code { get; set; }
        public string msg { get; set; }
        public string access_token { get; set; }
        public string refresh_token { get; set; }
        public string token_type { get; set; }
        public DateTime issued_at { get; set; } = DateTime.UtcNow;
        public DateTime expires_on { get; set; } = DateTime.UtcNow;
        public string time_zone { get; set; } = "lk";
    }
}
