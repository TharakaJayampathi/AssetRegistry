using AssetRegistry.DTOs;
using AssetRegistry.DTOs.LoginDTO;
using AssetRegistry.DTOs.Users;
using AssetRegistry.Interfaces;
using AssetRegistry.Models.Permissions;
using AssetRegistry.Models.Roles;
using AssetRegistry.Models.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
            //var _deviceId = _tokenstring["deviceId"].ToString();

            //string _sessionValue = "";

            //if (_memoryCache.TryGetValue($"signin-{_deviceId}", out _sessionValue))
            //{
            //    var _expireTime = Convert.ToInt64(_sessionValue.Split('_')[0].ToString());
            //    var _signature = _sessionValue.Split('_')[1].ToString();

            //    var _currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            //    if (_expireTime > _currentTime)
            //    {
            //        return true;
            //        //if (_signature == _requestSignature)
            //        //{
            //        //    return true;
            //        //}

            //    }

            //    return false;
            //}

            //return false;
            ////return true;

            //var _existingUserDeviceSession = await _context.UserDeviceSessions.Where(x => x.UserId == _postedUser).FirstOrDefaultAsync();

            var _tokenExpireTime = long.Parse(_tokenstring["exp"].ToString());
            var _tokenExpireTimeToUTC = DateTimeOffset.FromUnixTimeSeconds(_tokenExpireTime).UtcDateTime;

            //if (_existingUserDeviceSession != null)
            //{
            //if (_existingUserDeviceSession.DeviceId == _deviceId)
            //{
            if (_tokenExpireTimeToUTC > DateTime.UtcNow)
            {
                //#region Session Configuration
                //int Validity = -1;
                //long unixTime = DateTimeOffset.MaxValue.ToUnixTimeSeconds();
                //int _maxValidity = 2000;

                //if (Validity > 0)
                //{
                //    unixTime = DateTimeOffset.UtcNow.AddHours(Validity).ToUnixTimeSeconds();
                //    _maxValidity = Validity;
                //}

                //_memoryCache.Set($"signin-{_deviceId}", $"{unixTime}_{_requestSignature}", TimeSpan.FromDays(_maxValidity));
                //#endregion

                return true;
            }
            //}
            //else
            //{
            //    await RemoveSessionFromDb(_postedUser);
            //    return false;
            //}
            //}
            //else
            //{
            //    return false;
            //}

            return false;
        }

        public async Task<bool> IsSessionValid(string Session, string DeviceId)
        {
            var _tokenstring = new JwtSecurityTokenHandler().ReadJwtToken(Session).Payload;

            var _postedUser = _tokenstring["oid"].ToString();
            var _requestSignature = _tokenstring["signature"].ToString();

            string _sessionValue = "";

            //if (_memoryCache.TryGetValue($"signin-{DeviceId}", out _sessionValue))
            //{
            var _expireTime = Convert.ToInt64(_sessionValue.Split('_')[0].ToString());
            var _signature = _sessionValue.Split('_')[1].ToString();

            var _currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            if (_expireTime > _currentTime)
            {
                if (_signature == _requestSignature)
                {
                    return true;
                }

            }

            //return false;
            //}

            return false;
            //return true;

        }

        public async Task<LoginResponseDTO> GetToken(string userName, string password, string AppId = "", string DeviceId = "")
        {
            //ApplicationUser _user = await _identityService.GetUserByName(userName);
            ApplicationUser _user = await _userManager.FindByNameAsync(userName);

            if (_user is not null)
            {
                //if (!string.IsNullOrEmpty(DeviceId))
                //{
                if (_user.IsActive)
                {
                    var _signIn = await _signInManager.PasswordSignInAsync(_user, password, true, false);

                    if (_signIn.Succeeded)
                    {
                        //string _jwtToken = await GenerateToken(_user, DeviceId: DeviceId);
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

                        //if (_user.IsNewUser)
                        //{
                        //    await _identityService.SetLoginSession(_jwtToken, _userSessionValidity, DeviceId, true);
                        //    return _json;
                        //}
                        //else
                        //{
                        //    await _identityService.RemoveSessionFromDb(_user.Id);
                        //    await _identityService.SetLoginSession(_jwtToken, _userSessionValidity, DeviceId);
                        //    return _json;
                        //}

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
                            refresh_token = "",
                            //issued_at = "",
                            //expires_on = ""
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
                        refresh_token = "",
                        //issued_at = "",
                        //expires_on = ""
                    };

                    return _json;
                }
                //}
                //else
                //{
                //    var _json = new LoginResponseDTO
                //    {
                //        token_type = "",
                //        code = 401,
                //        msg = "Device Id is Required",
                //        access_token = "",
                //        refresh_token = "",
                //        //issued_at = "",
                //        //expires_on = ""
                //    };

                //    return _json;
                //}
            }
            else
            {
                var _json = new LoginResponseDTO
                {
                    token_type = "",
                    code = 404,
                    msg = "User Not Found",
                    access_token = "",
                    refresh_token = "",
                    //issued_at = "",
                    //expires_on = ""
                };

                return _json;
            }
        }

        public async Task<string> GenerateToken(ApplicationUser user, string SessionKey = "")
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
            //var _permissions = await _permissionService.GetPermissions(user.Id);
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
                                /*.Where(x => x.IsActive == true)*/
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

            //var _userdetails = await _identityService.GetUserProfile(user.Id);
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
                                          Nic = us.Nic,
                                          Address = us.Address,
                                          IsActive = us.IsActive,
                                          RoleId = ur.RoleId,
                                          RoleName = ro.Name
                                      }).FirstOrDefaultAsync();

            var _issuedAt = DateTime.UtcNow;
            var _notBefore = _issuedAt;

            var _expiresAt = _issuedAt.AddDays(_validity);
            //var _expiresAt = DateTime.Now.AddMinutes(10);

            var _sessionKey = GenerateSignature();

            if (!string.IsNullOrEmpty(SessionKey))
            {
                _sessionKey = SessionKey;
            }

            //string[] _allowedApps = new string[] { "appid1", "appid2" };//Enabled if Multiple Modules available in the App
            List<string> _userPermissions = new List<string>();

            List<Claim> _claims = new List<Claim> {
                        new Claim("oid", user.Id),
                        new Claim("unique_name", user.UserName),
                        new Claim("email", user.Email),
                        new Claim("phone", $"{ user.PhoneNumber}"),
                        new Claim("given_name", $"{user.FirstName}"),
                        new Claim("family_name", $"{user.LastName}"),
                        new Claim("name", $"{user.FirstName} {user.LastName}"),
                        //new Claim("allowed_apps", $"[{_allowedApps[0]}, {_allowedApps[1]}]"), //passing Module/App Ids
                        new Claim("role", $"{_userdetails.RoleName}"),
                        new Claim("timeZone", ""),
                        new Claim("signature", _sessionKey)
                        //new Claim("deviceId", DeviceId)
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
                //claims: claims,
                claims: _claims,
                notBefore: _notBefore,
                expires: _expiresAt,
                signingCredentials: _signIn);

            //try
            //{
            //    var _userrole = await _identityService.GetUserRoles(user.Id);

            //    LoginHistoryDTO loginHistroy = new();
            //    loginHistroy.UserId = $"{user.UserName} - {string.Join(',', _userrole)}";
            //    loginHistroy.DeviceId = "Mobile";
            //    loginHistroy.LoginDate = DateTimeHelper.GetCurrentTime();
            //    await _loginHistoryReopsitory.AddLoginHistory(loginHistroy);
            //}
            //catch
            //{

            //}

            var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);
            await AddAuthToken(user.Id, jwtToken, DateTime.UtcNow.AddDays(_validity));
            //await _identityService.AddAuthToken(user.Id, jwtToken, DateTime.UtcNow.AddMinutes(10));
            return jwtToken;
        }

        public async Task<string> GenerateRefreshToken(string UserId)
        {
            //var randomNumber = new byte[64];
            //using var rng = RandomNumberGenerator.Create();
            //rng.GetBytes(randomNumber);

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

            //var _user = await _userManager.FindByIdAsync(_postedUser);
            //if (_user != null)
            //{
            //    if (IsNewUser && _user.FrAvailable == true)
            //    {
            //        _user.IsNewUser = false;
            //        await _userManager.UpdateAsync(_user);
            //    }
            //}

            //var _hasDeviceSession = await AddSessionToDb(DeviceId, _postedUser);

            //if (!_hasDeviceSession)
            //{
            //    return false;
            //}
            //else
            //{
            //    _memoryCache.Set($"signin-{DeviceId}", $"{unixTime}_{_signature}", TimeSpan.FromDays(_maxValidity));
            //    return true;
            //}
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

        //public async Task<UsersView> GetUserProfile(string UserKey)
        //{
        //    Guid _userKey;

        //    var _allClaims = await _claimStore.GetClaims(ClaimCategories.MANAGE_USERS);

        //    if (Guid.TryParse(UserKey, out _userKey))
        //    {
        //        var _user = await (from us in _context.Users
        //                               //let div = (_context.DivisionUsers.Where(x => x.UserId == us.Id).ToList())
        //                           join usr in _context.UserRoles on us.Id equals usr.UserId
        //                           join role in _context.Roles on usr.RoleId equals role.Id
        //                           where us.Id == UserKey
        //                           select new UsersView
        //                           {
        //                               Id = us.Id,
        //                               FirstName = us.FirstName,
        //                               LastName = us.LastName,
        //                               //Designation = us.Designation,
        //                               IsActive = us.IsActive,
        //                               PhoneNumber = us.PhoneNumber,
        //                               Email = us.Email,
        //                               RoleName = role.Name,
        //                               RoleId = role.Id,
        //                               UserName = us.UserName,
        //                               //DivisionUsers = div,

        //                           }).FirstOrDefaultAsync();

        //        var _userClaims = await _context.UserClaims.Where(x => x.UserId == _user.Id).ToListAsync();

        //        List<UserClaim> _lst = new List<UserClaim>();

        //        foreach (var claim in _allClaims)
        //        {
        //            if (_userClaims.Where(x => x.ClaimType == claim.Type).FirstOrDefault() != null)
        //            {
        //                _lst.Add(new UserClaim { ClaimType = claim.Type, IsSelected = true });
        //            }
        //            else
        //            {
        //                _lst.Add(new UserClaim { ClaimType = claim.Type, IsSelected = false });
        //            }
        //        }

        //        _user.UserClaims = _lst;

        //        return _user;
        //    }
        //    else
        //    {
        //        var _user = await (from us in _context.Users
        //                               //let div = (_context.DivisionUsers.Where(x => x.UserId == us.Id).ToList())
        //                           join usr in _context.UserRoles on us.Id equals usr.UserId
        //                           join role in _context.Roles on usr.RoleId equals role.Id
        //                           where us.Email == UserKey
        //                           select new UsersView
        //                           {
        //                               Id = us.Id,
        //                               FirstName = us.FirstName,
        //                               LastName = us.LastName,
        //                               //Designation = us.Designation,
        //                               IsActive = us.IsActive,
        //                               PhoneNumber = us.PhoneNumber,
        //                               Email = us.Email,
        //                               RoleName = role.Name,
        //                               RoleId = role.Id,
        //                               UserName = us.UserName,
        //                               //DivisionUsers = div,

        //                           }).FirstOrDefaultAsync();

        //        var _userClaims = await _context.UserClaims.Where(x => x.UserId == _user.Id).ToListAsync();

        //        List<UserClaim> _lst = new List<UserClaim>();

        //        foreach (var claim in _allClaims)
        //        {
        //            if (_userClaims.Where(x => x.ClaimType == claim.Type).FirstOrDefault() != null)
        //            {
        //                _lst.Add(new UserClaim { ClaimType = claim.Type, IsSelected = true });
        //            }
        //            else
        //            {
        //                _lst.Add(new UserClaim { ClaimType = claim.Type, IsSelected = false });
        //            }
        //        }

        //        _user.UserClaims = _lst;

        //        return _user;
        //    }
        //}

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

            //_dateTimeService.GetUnixTime()
            var _nowTime = GetUnixTime(DateTime.UtcNow);

            if ((_refreshToken.RefreshToken == RefreshToken) && (_refreshToken.ExpireOn > _nowTime))
            {
                return Result.Success();
            }

            //return Result.Failure($"Expire: {_refreshToken.ExpireOn} Now: {_nowTime}");
            return Result.Failure("Refresh Token has been Changed.If This was Done without your Concent Please Sign in Using your Username and Password to Revoke the Current Token");
        }

        //public async Task RemoveSessionFromDb(string UserId)
        //{
        //    var _session = await _context.UserDeviceSessions.Where(x => x.UserId == UserId).ToListAsync();

        //    if (_session.Count() > 0)
        //    {
        //        _memoryCache.Remove($"signin-{_session.FirstOrDefault().DeviceId}");
        //        _context.UserDeviceSessions.RemoveRange(_session);
        //        await _context.SaveChangesAsync();
        //    }
        //}
    }
}
