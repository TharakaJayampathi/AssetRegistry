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

        //public async Task<HashSet<string>> GetPermissions(string UserId)
        //{
        //    var _permissions = await (from usr in _context.Users
        //                              join userrole in _context.UserRoles on usr.Id equals userrole.UserId
        //                              join roleperm in _context.RolePermissions on userrole.RoleId equals roleperm.RoleId
        //                              join perm in _context.Permissions on roleperm.PermissionId equals perm.Id
        //                              where userrole.UserId == $"{UserId}"
        //                              select new { Permission = perm.Category }).ToListAsync();

        //    return _permissions.Select(x => x.Permission).ToHashSet();
        //}

        public async Task<HashSet<string>> GetPermissions(string userId)
        {
            return new HashSet<string>();
        }

    }
}
