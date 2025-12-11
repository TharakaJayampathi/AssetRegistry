using AssetRegistry.DTOs;
using AssetRegistry.DTOs.Login;
using AssetRegistry.DTOs.Users;
using AssetRegistry.Extensions;
using AssetRegistry.Interfaces;
using AssetRegistry.Models.Permission;
using AssetRegistry.Models.Role;
using AssetRegistry.Models.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AssetRegistry.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly string? _key;
        private readonly int _validity;
        private readonly int _userSessionValidity;
        private readonly int _refreshTokenValidity;

        public IdentityService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context,
            IConfiguration configuration)
        {
            _key = configuration.GetSection("Jwt")["Secret"];
            _validity = Convert.ToInt32(configuration.GetSection("Jwt")["Validity"]);
            _userSessionValidity = Convert.ToInt32(configuration.GetSection("Jwt")["UserSessionValidity"]);
            _refreshTokenValidity = Convert.ToInt32(configuration.GetSection("Jwt")["RefreshTokenValidity"]);

            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _configuration = configuration;
        }

        public async Task<bool> IsSessionValid(string Session)
        {
            var _tokenstring = new JwtSecurityTokenHandler().ReadJwtToken(Session).Payload;
            var _postedUser = _tokenstring["oid"].ToString();
            var _requestSignature = _tokenstring["signature"].ToString();
            var _tokenExpireTime = long.Parse(_tokenstring["exp"].ToString());
            var _tokenExpireTimeToUTC = DateTimeOffset.FromUnixTimeSeconds(_tokenExpireTime).UtcDateTime;

            if (_tokenExpireTimeToUTC > DateTime.UtcNow)
            {
                return true;
            }
            return false;
        }

        public async Task<LoginResponseDTO> GetToken(string userName, string password, string AppId = "", string DeviceId = "")
        {
            ApplicationUser _user = await _userManager.FindByNameAsync(userName);

            if (_user is not null)
            {
                if (_user.IsActive)
                {
                    var _signIn = await _signInManager.PasswordSignInAsync(_user, password, true, false);

                    if (_signIn.Succeeded)
                    {
                        string _jwtToken = await GenerateToken(_user);
                        string _refreshToken = await GenerateRefreshToken(_user.Id);
                        var _issuedat = DateTime.UtcNow;
                        var _expireson = _issuedat.AddDays(_validity);
                        //var _expireson = DateTime.Now.AddMinutes(10);
                        var _json = new LoginResponseDTO
                        {
                            token_type = "Bearer",
                            code = 200,
                            msg = "success",
                            access_token = _jwtToken,
                            refresh_token = _refreshToken,
                            issued_at = _issuedat,
                            expires_on = _expireson
                        };
                        await SetLoginSession(_jwtToken, _userSessionValidity);
                        return _json;
                    }
                    else
                    {
                        var _json = new LoginResponseDTO
                        {
                            token_type = "",
                            code = 401,
                            msg = "Sign In Failed.Username or Password is Incorrect",
                            access_token = "",
                            refresh_token = ""
                        };
                        return _json;
                    }
                }
                else
                {
                    var _json = new LoginResponseDTO
                    {
                        token_type = "",
                        code = 401,
                        msg = "User is Not Acitve",
                        access_token = "",
                        refresh_token = ""
                    };
                    return _json;
                }
            }
            else
            {
                var _json = new LoginResponseDTO
                {
                    token_type = "",
                    code = 404,
                    msg = "User Not Found",
                    access_token = "",
                    refresh_token = ""
                };
                return _json;
            }
        }

        public async Task<string> GenerateToken(ApplicationUser user, string SessionKey = "")
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
            var _userRole = await (from userRole in _context.UserRoles
                                   join ro in _context.Roles on userRole.RoleId equals ro.Id
                                   where userRole.UserId == user.Id
                                   select new Role()
                                   {
                                       Id = ro.Id,
                                       Name = ro.Name
                                   }).FirstOrDefaultAsync();

            List<string> _permissions = new List<string>();

            if (_userRole.Name == "SuperAdmin")
            {
                _permissions = await _context.Permissions
                                .Where(x => x.IsActive == true)
                                .Select(x => x.Name)
                                .ToListAsync();
            }
            else
            {
                _permissions = await (from pe in _context.Permissions
                                      join rp in _context.RolePermissions on pe.Id equals rp.PermissionType
                                      join ro in _context.Roles on rp.RoleId equals ro.Id
                                      where ro.Id == _userRole.Id
                                      select new Permission()
                                      {
                                          Id = pe.Id,
                                          Name = pe.Name,
                                          Type = pe.Type
                                      })
                                      .Select(x => x.Name)
                                      .ToListAsync();
            }

            var _userdetails = await (from us in _context.ApplicationUsers
                                      join ur in _context.UserRoles on us.Id equals ur.UserId
                                      join ro in _context.Roles on ur.RoleId equals ro.Id
                                      where us.Id == user.Id
                                      select new UserListDTO()
                                      {
                                          Id = us.Id,
                                          FirstName = us.FirstName,
                                          LastName = us.LastName,
                                          Email = us.Email,
                                          IsActive = us.IsActive,
                                          RoleName = ro.Name
                                      }).FirstOrDefaultAsync();

            var _issuedAt = DateTime.UtcNow;
            var _notBefore = _issuedAt;
            var _expiresAt = _issuedAt.AddDays(_validity);

            var _sessionKey = GenerateSignature();
            if (!string.IsNullOrEmpty(SessionKey))
            {
                _sessionKey = SessionKey;
            }

            List<string> _userPermissions = new List<string>();

            List<Claim> _claims = new List<Claim> {
                        new Claim("oid", user.Id),
                        new Claim("unique_name", user.UserName),
                        new Claim("email", user.Email),
                        new Claim("phone", $"{ user.PhoneNumber}"),
                        new Claim("given_name", $"{user.FirstName}"),
                        new Claim("family_name", $"{user.LastName}"),
                        new Claim("name", $"{user.FirstName} {user.LastName}"),
                        new Claim("role", $"{_userdetails.RoleName}"),
                        new Claim("timeZone", ""),
                        new Claim("signature", _sessionKey)
            };

            foreach (var permission in _permissions)
            {
                _userPermissions.Add(permission);
            }

            var _permissionArray = string.Join(",", _userPermissions.ToArray());
            _claims.Add(new Claim("permissions", $"{_permissionArray}"));

            var _signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                _configuration["Jwt:ValidAudience"],
                _configuration["Jwt:ValidIssuer"],
                claims: _claims,
                notBefore: _notBefore,
                expires: _expiresAt,
                signingCredentials: _signIn);

            var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);
            await AddAuthToken(user.Id, jwtToken, DateTime.UtcNow.AddDays(_validity));
            //await _identityService.AddAuthToken(user.Id, jwtToken, DateTime.UtcNow.AddMinutes(10));
            return jwtToken;
        }

        public async Task<string> GenerateRefreshToken(string UserId)
        {
            var _refreshToken = GenerateSignature();

            await AddRefreshToken(UserId, _refreshToken, DateTime.UtcNow.AddDays(_refreshTokenValidity));
            //await _identityService.AddRefreshToken(UserId, _refreshToken, DateTime.UtcNow.AddMinutes(15));
            return _refreshToken;
        }

        public async Task<bool> SetLoginSession(string Session, int Validity, /*string DeviceId, */bool IsNewUser = false)
        {
            var _tokenstring = new JwtSecurityTokenHandler().ReadJwtToken(Session).Payload;
            var _postedUser = _tokenstring["oid"].ToString();
            var _signature = _tokenstring["signature"].ToString();
            long unixTime = DateTimeOffset.MaxValue.ToUnixTimeSeconds();
            int _maxValidity = 2000;

            if (Validity > 0)
            {
                unixTime = DateTimeOffset.UtcNow.AddHours(Validity).ToUnixTimeSeconds();
                _maxValidity = Validity;
            }
            return true;
        }

        public static string GenerateSignature()
        {
            byte[] randomBytes = new byte[128];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes).Replace("/", "--");
        }

        public async Task AddAuthToken(string UserId, string AuthToken, DateTime ExpireOn)
        {
            try
            {
                var _authToken = await _context.UserAuthTokens.FirstOrDefaultAsync(x => x.UserId == UserId);
                if (_authToken is null)
                {
                    await _context.UserAuthTokens.AddAsync(new UserAuthToken
                    {
                        UserId = UserId,
                        AuthToken = AuthToken,
                        ExpireOn = GetUnixTime(ExpireOn)
                    });
                }
                else
                {
                    _authToken.AuthToken = AuthToken;
                    _authToken.ExpireOn = GetUnixTime(ExpireOn);
                }
                var _res = await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task AddRefreshToken(string UserId, string RefreshToken, DateTime ExpireOn)
        {
            try
            {
                var _refreshToken = await _context.UserRefreshTokens.FirstOrDefaultAsync(x => x.UserId == UserId);

                if (_refreshToken is null)
                {
                    await _context.UserRefreshTokens.AddAsync(new UserRefreshToken
                    {
                        UserId = UserId,
                        RefreshToken = RefreshToken,
                        ExpireOn = GetUnixTime(ExpireOn)
                    });
                }
                else
                {
                    _refreshToken.RefreshToken = RefreshToken;
                    _refreshToken.ExpireOn = GetUnixTime(ExpireOn);
                }
                var _res = await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
        }

        private long GetUnixTime(DateTime Date)
        {
            return new DateTimeOffset(Date).ToUnixTimeSeconds();
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"])),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }
            return principal;
        }

        public async Task<Result> ValidateRefreshToken(string UserId, string RefreshToken)
        {
            var _refreshToken = await _context.UserRefreshTokens.FirstOrDefaultAsync(x => x.UserId == UserId);

            if (_refreshToken is null)
            {
                return Result.Failure("Refresh Token Not Found.Please Sign in Using your Username and Password");
            }

            var _nowTime = GetUnixTime(DateTime.UtcNow);

            if ((_refreshToken.RefreshToken == RefreshToken) && (_refreshToken.ExpireOn > _nowTime))
            {
                return Result.Success();
            }

            return Result.Failure("Refresh Token has been Changed.If This was Done without your Concent Please Sign in Using your Username and Password to Revoke the Current Token");
        }

        public async Task<Result> ChangePassword(string UserId, string OldPassword, string NewPassword)
        {
            var _user = await _userManager.FindByIdAsync(UserId);
            var result = await _userManager.ChangePasswordAsync(_user, OldPassword, NewPassword);
            return result.ToApplicationResult();
        }

        public async Task<IEnumerable<UsersView>> GetLoginSessions()
        {
            List<UsersView> _lst = new();
            return _lst;
        }

        public async Task RemoveLoginSession(string UserId, string DeviceId)
        {
            try
            {
                await RemoveSessionFromDb(UserId);
            }
            catch (Exception ex)
            {
            }
        }

        public async Task RemoveSessionFromDb(string UserId)
        {
        }
    }
}
