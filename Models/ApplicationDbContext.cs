using AssetRegistry.Models.Claims;
using AssetRegistry.Models.Company;
using AssetRegistry.Models.User;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    private readonly IHttpContextAccessor? _httpContextAccessor;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IHttpContextAccessor? httpContextAccessor = null)
        : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
    }

    public DbSet<ApplicationUser> ApplicationUsers { get; set; }
    public DbSet<Company> Companies { get; set; }
    public DbSet<ClaimType> ClaimTypes { get; set; }
    public DbSet<UserAuthToken> UserAuthTokens { get; set; }
    public DbSet<UserRefreshToken> UserRefreshTokens { get; set; }
}
