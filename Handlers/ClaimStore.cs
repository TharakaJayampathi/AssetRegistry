using AssetRegistry.Enums;
using AssetRegistry.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AssetRegistry.Handlers
{
    public class ClaimStore : IClaimStore
    {
        private readonly ApplicationDbContext _context;
        public ClaimStore(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Claim> AllClaims = new List<Claim>()
        {
            ///Role Claims - Modules in the System will be assigned to Roles
            new Claim("User Management", "User Management"),

            ///Application Module Permissions
            new Claim("View", "View"),
            new Claim("Create", "Create"),
            new Claim("Edit", "Edit"),
            new Claim("Delete", "Delete"),
        };

        public async Task<IEnumerable<Claim>> GetClaims(ClaimCategories Category)
        {
            var _claims = await _context.ClaimTypes.Where(x => x.Resource == (int)Category).ToListAsync();
            List<Claim> _lst = new List<Claim>();

            foreach (var item in _claims)
            {
                _lst.Add(new Claim(item.Claim, "true"));
            }

            return _lst;
        }

        public async Task<IEnumerable<Claim>> GetClaims()
        {
            var _claims = await _context.ClaimTypes.ToListAsync();
            List<Claim> _lst = new List<Claim>();

            foreach (var item in _claims)
            {
                _lst.Add(new Claim(item.Claim, "true"));
            }

            return _lst;
        }
    }
}
