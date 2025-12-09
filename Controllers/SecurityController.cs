using AssetRegistry.DTOs;
using AssetRegistry.DTOs.LoginDTO;
using AssetRegistry.DTOs.Users;
using AssetRegistry.Enums;
using AssetRegistry.Interfaces;
using AssetRegistry.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace AssetRegistry.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SecurityController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly string? _key;
        private readonly int _validity;
        private readonly int _userSessionValidity;
        private readonly int _refreshTokenValidity;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public SecurityController(
            IConfiguration configuration,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _refreshTokenValidity = Convert.ToInt32(configuration.GetSection("Jwt")["RefreshTokenValidity"]);
            _validity = Convert.ToInt32(configuration.GetSection("Jwt")["Validity"]);
            _userSessionValidity = Convert.ToInt32(configuration.GetSection("Jwt")["UserSessionValidity"]);
            _key = configuration.GetSection("Jwt")["Secret"];

            _configuration = configuration;
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("security/token")]
        public async Task<IActionResult> Post(LoginDTO Model)
        {
            try
            {
                string? _userId = null;
                var _user = await _userManager.FindByNameAsync(Model.Username);

                if (_user is not null)
                {
                    _userId = _user.Id;
                }

                var _loginRes = await GetToken(Model.Username, Model.Password);

                if (_loginRes.code == 200)
                {
                    return Ok(_loginRes);
                }
                else if (_loginRes.code == 400)
                {
                    return NotFound(_loginRes);
                }
                else if (_loginRes.code == 401)
                {
                    return Unauthorized(_loginRes);
                }
                else if (_loginRes.code == 404)
                {
                    return Unauthorized(_loginRes);
                }
                else
                {
                    return Unauthorized(_loginRes);
                }
            }
            catch (Exception ex)
            {
                var _response = JsonConvert.SerializeObject(ex);
                return UnprocessableEntity(new { code = 422, msg = "Data cannot be Proccessed", data = "" });
            }
        }

        private async Task<LoginResponseDTO> GetToken(string userName, string password, string AppId = "")
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

        private async Task<string> GenerateToken(ApplicationUser user, string SessionKey = "")
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
            //var _permissions = await _permissionService.GetPermissions(user.Id);
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

            //foreach (var permission in _permissions)
            //{
            //    _userPermissions.Add(permission);
            //}

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
            return jwtToken;
        }

        private async Task<string> GenerateRefreshToken(string UserId)
        {
            var _refreshToken = GenerateSignature();
            await AddRefreshToken(UserId, _refreshToken, DateTime.UtcNow.AddDays(_refreshTokenValidity));
            return _refreshToken;
        }

        private async Task<bool> SetLoginSession(string Session, int Validity, bool IsNewUser = false)
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

        private static string GenerateSignature()
        {
            byte[] randomBytes = new byte[128];

            using var rng = RandomNumberGenerator.Create();

            rng.GetBytes(randomBytes);

            return Convert.ToBase64String(randomBytes).Replace("/", "--");

        }

        private async Task AddAuthToken(string UserId, string AuthToken, DateTime ExpireOn)
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

        private async Task AddRefreshToken(string UserId, string RefreshToken, DateTime ExpireOn)
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

    }
}
