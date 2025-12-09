using AssetRegistry.Enums;
using System.Security.Claims;

namespace AssetRegistry.Interfaces
{
    public interface IClaimStore
    {
        Task<IEnumerable<Claim>> GetClaims(ClaimCategories Category);
        Task<IEnumerable<Claim>> GetClaims();
        //Task<IEnumerable<Claim>> CompanyClaims();
        //Task<IEnumerable<Claim>> DivisionClaims();
        //IEnumerable<Claim> RoleClaims();
        //Task<IEnumerable<Claim>> UserClaims();
        //Task<IEnumerable<Claim>> ItemCollection();
        //Task<IEnumerable<Claim>> Warehouse();
        //Task<IEnumerable<Claim>> Finance();
    }
}
