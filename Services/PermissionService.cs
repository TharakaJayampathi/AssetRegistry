using AssetRegistry.Interfaces;

namespace AssetRegistry.Services
{
    public class PermissionService : IPermissionService
    {
        private readonly ApplicationDbContext _context;

        public PermissionService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<HashSet<string>> GetPermissions(string userId)
        {
            return new HashSet<string>();
        }

    }
}
