namespace AssetRegistry.Interfaces
{
    public interface IIdentityService
    {
        Task<bool> IsSessionValid(string Session);
        Task<bool> IsSessionValid(string Session, string DeviceId);
    }
}
