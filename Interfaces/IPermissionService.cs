namespace AssetRegistry.Interfaces
{
    public interface IPermissionService
    {
        Task<HashSet<string>> GetPermissions(string UserId);
    }
}
