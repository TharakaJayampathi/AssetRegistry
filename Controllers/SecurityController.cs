using AssetRegistry.DTOs;
using AssetRegistry.DTOs.LoginDTO;
using AssetRegistry.DTOs.Tokens;
using AssetRegistry.Interfaces;
using AssetRegistry.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;

namespace AssetRegistry.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SecurityController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IIdentityService _identityService;

        public SecurityController(
            UserManager<ApplicationUser> userManager,
            IIdentityService identityService)
        {
            _userManager = userManager;
            _identityService = identityService;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("token")]
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
                var _loginRes = await _identityService.GetToken(Model.Username, Model.Password);

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

        [HttpPost]
        [Route("auth-refresh")]
        public async Task<IActionResult> RefreshAuthToken(TokenDTO Model)
        {
            var _tokenstring = new JwtSecurityTokenHandler().ReadJwtToken(Model.AccessToken).Payload;
            var _signature = _tokenstring["signature"].ToString();
            var _postedUser = _tokenstring["oid"].ToString();

            try
            {
                if (Model is null)
                {
                    return BadRequest("Invalid client request");
                }

                string accessToken = Model.AccessToken;
                string refreshToken = Model.RefreshToken;

                var principal = _identityService.GetPrincipalFromExpiredToken(accessToken);
                if (principal == null)
                {
                    return BadRequest("Invalid access token or refresh token");
                }

                string username = principal.Identity.Name;
                var _user = await _userManager.FindByNameAsync(username);

                var _isValidRefreshToken = await _identityService.ValidateRefreshToken(_user.Id, Model.RefreshToken);
                if (!_isValidRefreshToken.Succeeded)
                {
                    return Unauthorized(_isValidRefreshToken.Message);
                }

                var newAccessToken = await _identityService.GenerateToken(_user);
                var newRefreshToken = await _identityService.GenerateRefreshToken(_user.Id);
                return Ok(new
                {
                    access_token = newAccessToken,
                    refresh_token = newRefreshToken
                });
            }
            catch (Exception ex)
            {
                var _response = JsonConvert.SerializeObject(ex);
                return UnprocessableEntity(new { code = 422, msg = "Data cannot be Proccessed", data = "" });
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("change-password")]
        public async Task<IActionResult> ChangePassword(MobileChangePassword Model)
        {
            try
            {
                var _user = await _userManager.FindByNameAsync(Model.Username);
                if (_user != null)
                {
                    var res = await _identityService.ChangePassword(_user.Id, Model.OldPassword, Model.NewPassword);
                    if (res.Succeeded)
                    {
                        return Ok(new
                        {
                            code = 200,
                            msg = "Password Updated Successfuly"
                        });
                    }

                    return UnprocessableEntity(new
                    {
                        code = 422,
                        msg = "Password Cannot be Changed"
                    });
                }
                else
                {
                    return UnprocessableEntity(new { code = 422, msg = "User Not Found" });
                }
            }
            catch (Exception ex)
            {
                var _response = JsonConvert.SerializeObject(ex);
                return Problem(detail: $"{ex.Message}", statusCode: 500, title: "Server Error");
            }
        }

        [HttpGet]
        [Route("sessions-get")]
        public async Task<IActionResult> GetUserSessions()
        {
            var _jwt = Request.Headers["Authorization"].ToString().Replace("Bearer ", "").Replace("bearer ", "");
            var _tokenstring = new JwtSecurityTokenHandler().ReadJwtToken(_jwt).Payload;
            var _postedUser = _tokenstring["oid"].ToString();

            try
            {
                await _identityService.GetLoginSessions();
                return Ok();
            }
            catch (Exception ex)
            {
                var _response = JsonConvert.SerializeObject(ex);
                return UnprocessableEntity(new { code = 422, msg = "Data cannot be Proccessed", data = "" });
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("session-remove/{id}")]
        public async Task<IActionResult> RemoveUserSession(string id)
        {
            try
            {
                await _identityService.RemoveLoginSession(id, "");
                return Ok();
            }
            catch (Exception ex)
            {
                var _response = JsonConvert.SerializeObject(ex);
                return UnprocessableEntity(new { code = 422, msg = "Data cannot be Proccessed", data = "" });
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("sign-out")]
        public async Task<IActionResult> Logoff()
        {
            var _jwt = Request.Headers["Authorization"].ToString().Replace("Bearer ", "").Replace("bearer ", "");
            var _tokenstring = new JwtSecurityTokenHandler().ReadJwtToken(_jwt).Payload;
            var _postedUser = _tokenstring["oid"].ToString();

            try
            {
                await _identityService.RemoveLoginSession(_postedUser, "");
                return Ok(new { code = 200, msg = "", data = "" });
            }
            catch (Exception ex)
            {
                var _response = JsonConvert.SerializeObject(ex);
                return UnprocessableEntity(new { code = 422, msg = "Data cannot be Proccessed", data = "" });
            }
        }

    }
}
