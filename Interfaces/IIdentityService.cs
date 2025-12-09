using AssetRegistry.DTOs.LoginDTO;
using AssetRegistry.Models.User;

namespace AssetRegistry.Interfaces
{
    public interface IIdentityService
    {
        Task<bool> IsSessionValid(string Session);
        Task<bool> IsSessionValid(string Session, string DeviceId);
        Task<LoginResponseDTO> GetToken(string userName, string password, string AppId = "", string DeviceId = "");
        Task<string> GenerateToken(ApplicationUser user, string SessionKey = "");
        Task<string> GenerateRefreshToken(string UserId);
        Task<bool> SetLoginSession(string Session, int Validity, /*string DeviceId, */bool IsNewUser = false);
        Task AddAuthToken(string UserId, string AuthToken, DateTime ExpireOn);
        Task AddRefreshToken(string UserId, string RefreshToken, DateTime ExpireOn);
    }
}
