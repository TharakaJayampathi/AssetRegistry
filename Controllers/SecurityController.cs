using AssetRegistry.DTOs;
using AssetRegistry.DTOs.LoginDTO;
using AssetRegistry.DTOs.Tokens;
using AssetRegistry.Extensions;
using AssetRegistry.Interfaces;
using AssetRegistry.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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
        private readonly IIdentityService _identityService;

        public SecurityController(
            IConfiguration configuration,
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            IIdentityService identityService)
        {
            _refreshTokenValidity = Convert.ToInt32(configuration.GetSection("Jwt")["RefreshTokenValidity"]);
            _validity = Convert.ToInt32(configuration.GetSection("Jwt")["Validity"]);
            _userSessionValidity = Convert.ToInt32(configuration.GetSection("Jwt")["UserSessionValidity"]);
            _key = configuration.GetSection("Jwt")["Secret"];

            _configuration = configuration;
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
            _identityService = identityService;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("security/token")]
        public async Task<IActionResult> Post(LoginDTO Model)
        {
            //await _log.AddAPILog(null, "api/security/token", JsonConvert.SerializeObject(Model), "", "", (byte)ApiLogEnum.LOG);

            try
            {
                string? _userId = null;
                //var _user = await _identityService.GetUserByName(Model.Username);
                var _user = await _userManager.FindByNameAsync(Model.Username);

                if (_user is not null)
                {
                    _userId = _user.Id;
                }

                var _loginRes = await _identityService.GetToken(Model.Username, Model.Password);

                if (_loginRes.code == 200)
                {
                    //await _log.AddAPILog(_userId, "api/security/token", JsonConvert.SerializeObject(Model), $"Device Id - {Model.DeviceId}", "Device Login:OK", (byte)ApiLogEnum.LOG);

                    //await _loginHistoryReopsitory
                    //        .AddLoginHistory(new domain.Entities.LoginHistory
                    //        {
                    //            UserId = Model.Username,
                    //            LoginDate = DateTime.Now,
                    //        });

                    //await _log.AddAPILog(_userId, "api/security/token", JsonConvert.SerializeObject(Model), JsonConvert.SerializeObject(_loginRes), "Secuirity Token:OK", (byte)ApiLogEnum.LOG);
                    return Ok(_loginRes);
                }
                else if (_loginRes.code == 400)
                {
                    //await _log.AddAPILog(_userId, "api/security/token", JsonConvert.SerializeObject(Model), JsonConvert.SerializeObject(_loginRes), "Secuirity Token:Error", (byte)ApiLogEnum.ERROR);
                    return NotFound(_loginRes);
                }
                else if (_loginRes.code == 401)
                {
                    //await _log.AddAPILog(_userId, "api/security/token", JsonConvert.SerializeObject(Model), JsonConvert.SerializeObject(_loginRes), "Secuirity Token:Error", (byte)ApiLogEnum.ERROR);
                    return Unauthorized(_loginRes);
                }
                else if (_loginRes.code == 404)
                {
                    //await _log.AddAPILog(_userId, "api/security/token", JsonConvert.SerializeObject(Model), JsonConvert.SerializeObject(_loginRes), "Secuirity Token:Error", (byte)ApiLogEnum.ERROR);
                    return Unauthorized(_loginRes);
                }
                else
                {
                    //await _log.AddAPILog(_userId, "api/security/token", JsonConvert.SerializeObject(Model), JsonConvert.SerializeObject(_loginRes), "Secuirity Token:Error", (byte)ApiLogEnum.ERROR);
                    return Unauthorized(_loginRes);
                }
            }
            catch (Exception ex)
            {
                var _response = JsonConvert.SerializeObject(ex);
                //await _log.AddAPILog(null, "api/security/token", JsonConvert.SerializeObject(Model), $"{_response}", "Secuirity Token:Error", (byte)ApiLogEnum.ERROR);
                return UnprocessableEntity(new { code = 422, msg = "Data cannot be Proccessed", data = "" });
            }
        }

        [HttpPost]
        [Route("security/auth-refresh")]
        public async Task<IActionResult> RefreshAuthToken(TokenDTO Model)
        {
            //await _log.AddAPILog(null, "api/security/auth-refresh", JsonConvert.SerializeObject(Model), "", "", (byte)ApiLogEnum.LOG);

            var _tokenstring = new JwtSecurityTokenHandler().ReadJwtToken(Model.AccessToken).Payload;

            var _signature = _tokenstring["signature"].ToString();
            //var _deviceId = _tokenstring["deviceId"].ToString();
            var _postedUser = _tokenstring["oid"].ToString();

            try
            {
                if (Model is null)
                {
                    //await _log.AddAPILog(_postedUser, "api/security/auth-refresh", JsonConvert.SerializeObject(Model), "Invalid client request", "Auth Refresh:Error", (byte)ApiLogEnum.ERROR);
                    return BadRequest("Invalid client request");
                }

                string accessToken = Model.AccessToken;
                string refreshToken = Model.RefreshToken;

                var principal = _identityService.GetPrincipalFromExpiredToken(accessToken);
                if (principal == null)
                {
                    //await _log.AddAPILog(_postedUser, "api/security/auth-refresh", JsonConvert.SerializeObject(Model), "Invalid access token or refresh token", "Auth Refresh:Error", (byte)ApiLogEnum.ERROR);
                    return BadRequest("Invalid access token or refresh token");
                }

                string username = principal.Identity.Name;

                //var _user = await _identityService.GetUserByName(username);
                var _user = await _userManager.FindByNameAsync(username);

                var _isValidRefreshToken = await _identityService.ValidateRefreshToken(_user.Id, Model.RefreshToken);

                if (!_isValidRefreshToken.Succeeded)
                {
                    //await _identityService.RemoveSessionFromDb(_user.Id);
                    //await _log.AddAPILog(_postedUser, "api/security/auth-refresh", JsonConvert.SerializeObject(Model), $"{_isValidRefreshToken.Message}", "Auth Refresh:Error", (byte)ApiLogEnum.ERROR);
                    return Unauthorized(_isValidRefreshToken.Message);
                }

                var newAccessToken = await _identityService.GenerateToken(_user/*, DeviceId: _deviceId*/);
                var newRefreshToken = await _identityService.GenerateRefreshToken(_user.Id);

                //await _log.AddAPILog(_postedUser, "api/security/auth-refresh", JsonConvert.SerializeObject(Model), "", "Auth Refresh:OK", (byte)ApiLogEnum.LOG);

                return Ok(new
                {
                    access_token = newAccessToken,
                    refresh_token = newRefreshToken
                });
            }
            catch (Exception ex)
            {
                var _response = JsonConvert.SerializeObject(ex);
                //await _log.AddAPILog(_postedUser, "api/security/auth-refresh", JsonConvert.SerializeObject(Model), $"{_response}", "Auth Refresh:Error", (byte)ApiLogEnum.ERROR);
                return UnprocessableEntity(new { code = 422, msg = "Data cannot be Proccessed", data = "" });
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("security/change-password")]
        public async Task<IActionResult> ChangePassword(MobileChangePassword Model)
        {
            //await _log.AddAPILog(null, "api/security/change-password", JsonConvert.SerializeObject(Model), "", "", (byte)ApiLogEnum.LOG);

            try
            {
                //var _user = await _identityService.GetUserByName(Model.Username);
                var _user = await _userManager.FindByNameAsync(Model.Username);

                if (_user != null)
                {
                    var res = await _identityService.ChangePassword(_user.Id, Model.OldPassword, Model.NewPassword);

                    if (res.Succeeded)
                    {
                        //await _activityLogRepository.AddActivityLog("User", Model.Username, "Change Password", $"{_user.FirstName} {_user.LastName} Password Changed");
                        //await _log.AddAPILog(_user.Id, "api/security/change-password", JsonConvert.SerializeObject(Model), JsonConvert.SerializeObject(res), "Change Password:OK", (byte)ApiLogEnum.LOG);

                        return Ok(new
                        {
                            code = 200,
                            msg = "Password Updated Successfuly"
                        });
                    }

                    //await _log.AddAPILog(_user.Id, "api/security/change-password", JsonConvert.SerializeObject(Model), JsonConvert.SerializeObject(res), "Change Password:Error", (byte)ApiLogEnum.ERROR);

                    return UnprocessableEntity(new
                    {
                        code = 422,
                        msg = "Password Cannot be Changed"
                    });
                }
                else
                {
                    //await _log.AddAPILog(null, "api/security/change-password", JsonConvert.SerializeObject(Model), "User Not Found", "Change Password:Error", (byte)ApiLogEnum.ERROR);
                    return UnprocessableEntity(new { code = 422, msg = "User Not Found" });
                }
            }
            catch (Exception ex)
            {
                var _response = JsonConvert.SerializeObject(ex);
                //await _log.AddAPILog(null, "api/security/change-password", JsonConvert.SerializeObject(Model), $"{_response}", "Change Password:Error", (byte)ApiLogEnum.ERROR);
                return Problem(detail: $"{ex.Message}", statusCode: 500, title: "Server Error");
            }
        }

        //[AllowAnonymous]
        [HttpGet]
        [Route("security/sessions/get")]
        public async Task<IActionResult> GetUserSessions()
        {
            var _jwt = Request.Headers["Authorization"].ToString().Replace("Bearer ", "").Replace("bearer ", "");
            var _tokenstring = new JwtSecurityTokenHandler().ReadJwtToken(_jwt).Payload;
            var _postedUser = _tokenstring["oid"].ToString();

            //await _log.AddAPILog(_postedUser, "security/sessions/get", "", "", "", (byte)ApiLogEnum.LOG);

            try
            {
                await _identityService.GetLoginSessions();
                //await _log.AddAPILog(_postedUser, "security/sessions/get", "", "", "Get Sessions:OK", (byte)ApiLogEnum.LOG);
                return Ok();
            }
            catch (Exception ex)
            {
                var _response = JsonConvert.SerializeObject(ex);
                //await _log.AddAPILog(_postedUser, "api/security/frs-login", "", $"{_response}", "Get Sessions:Error", (byte)ApiLogEnum.ERROR);
                return UnprocessableEntity(new { code = 422, msg = "Data cannot be Proccessed", data = "" });
            }
        }

        

        [AllowAnonymous]
        //[HasPermission("Users.RemoveSession")]
        [HttpPost]
        [Route("security/session/remove/{id}")]
        public async Task<IActionResult> RemoveUserSession(string id)
        {
            //await _log.AddAPILog(id, "api/security/session/remove", $"User Id - {id}", "", "", (byte)ApiLogEnum.LOG);

            try
            {
                await _identityService.RemoveLoginSession(id, "");
                //await _log.AddAPILog(id, "api/security/session/remove", $"User Id - {id}", "", "Session Remove:OK", (byte)ApiLogEnum.LOG);
                return Ok();
            }
            catch (Exception ex)
            {
                var _response = JsonConvert.SerializeObject(ex);
                //await _log.AddAPILog(id, "api/security/session/remove", $"User Id - {id}", $"{_response}", "Session Remove:Error", (byte)ApiLogEnum.ERROR);
                return UnprocessableEntity(new { code = 422, msg = "Data cannot be Proccessed", data = "" });
            }
        }

        

        [AllowAnonymous]
        //[HasPermission("Users.UserProfile")]
        [HttpPost]
        [Route("security/sign-out")]
        public async Task<IActionResult> Logoff()
        {
            var _jwt = Request.Headers["Authorization"].ToString().Replace("Bearer ", "").Replace("bearer ", "");
            var _tokenstring = new JwtSecurityTokenHandler().ReadJwtToken(_jwt).Payload;
            var _postedUser = _tokenstring["oid"].ToString();

            //await _log.AddAPILog(_postedUser, "api/security/sign-out", $"User Id - {_postedUser}", "", "Session Sign Out", (byte)ApiLogEnum.LOG);

            try
            {
                await _identityService.RemoveLoginSession(_postedUser, "");
                //await _log.AddAPILog(_postedUser, "api/security/sign-out", $"User Id - {_postedUser}", "", "Session Sign Out:OK", (byte)ApiLogEnum.LOG);
                return Ok(new { code = 200, msg = "", data = "" });
            }
            catch (Exception ex)
            {
                var _response = JsonConvert.SerializeObject(ex);
                //await _log.AddAPILog(null, "api/security/sign-out", $"User Id - {_postedUser}", $"{_response}", "Session Sign Out:Error", (byte)ApiLogEnum.ERROR);
                return UnprocessableEntity(new { code = 422, msg = "Data cannot be Proccessed", data = "" });
            }
        }

    }
}
