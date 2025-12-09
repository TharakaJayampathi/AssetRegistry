using AssetRegistry.Exceptions;
using AssetRegistry.Interfaces;
using AssetRegistry.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Text;

namespace AssetRegistry.Handlers
{
    public class PermissionAuthorizationHandler
         : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PermissionAuthorizationHandler(IServiceScopeFactory serviceScopeFactory, IHttpContextAccessor httpContextAccessor)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            var _httpresource = (DefaultHttpContext)context.Resource;

            var asdas = context.User.Identities;
            if (context.User.IsInRole("SuperAdmin"))
            {
                //throw new PermissionDeniedException(HttpStatusCode.Unauthorized);
                context.Succeed(requirement);
                return;
            }
            else
            {
                var jwtToken = _httpresource.HttpContext.Request.Headers["Authorization"].ToString()
                    .Replace("Bearer ", "")
                    .Replace("bearer ", "");

                //if (string.IsNullOrEmpty(jwtToken))
                //{
                //    jwtToken = _httpresource.HttpContext.Request.Headers["Authorization"].ToString().Replace("bearer ", "");
                //}

                using IServiceScope _scope = _serviceScopeFactory.CreateScope();

                var _configuration = _scope.ServiceProvider.GetService<IConfiguration>();
                var _identity = _scope.ServiceProvider.GetService<IIdentityService>();

                int _validity = Convert.ToInt32(_configuration.GetSection("Jwt")["Validity"]);

                if (!string.IsNullOrEmpty(jwtToken))
                {
                    var handler = new JwtSecurityTokenHandler();

                    try
                    {
                        var key = Encoding.ASCII.GetBytes(_configuration.GetSection("Jwt")["Secret"]);

                        handler.ValidateToken(jwtToken, new TokenValidationParameters
                        {
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = new SymmetricSecurityKey(key),
                            ValidateIssuer = false,
                            ValidateAudience = false,
                            ClockSkew = TimeSpan.Zero
                        }, out SecurityToken validatedToken);


                        var token = handler.ReadJwtToken(jwtToken);

                        var _oid = token.Claims.Where(x => x.Type == "oid").FirstOrDefault().Value;
                        var _signature = token.Claims.Where(x => x.Type == "signature").FirstOrDefault().Value;

                        var _permissions = token.Claims.Where(x => x.Type == "permissions")
                        .Select(x => x.Value).FirstOrDefault();
                        //var permissionList = JsonConvert.DeserializeObject<HashSet<string>>(_permissions);

                        if (await _identity.IsSessionValid(jwtToken))
                        {
                            if (_permissions.Contains(requirement.Permission))
                            {
                                context.Succeed(requirement);
                            }
                            else
                            {
                                throw new PermissionDeniedException(HttpStatusCode.Forbidden);
                            }
                        }
                        else
                        {
                            throw new JWTInvalidException(HttpStatusCode.Unauthorized, "Login Session is Expired");
                        }

                    }
                    catch (PermissionDeniedException er)
                    {
                        throw new PermissionDeniedException(HttpStatusCode.Forbidden);
                    }
                    catch (Exception er)
                    {
                        throw new JWTInvalidException(HttpStatusCode.Unauthorized, er.Message);
                    }


                }
                else
                {
                    var _signInManager = _scope.ServiceProvider.GetService<SignInManager<ApplicationUser>>();

                    if (context.User.Claims.Count() > 0)
                    {
                        var _userId = context.User.Claims.ElementAt(0)?.Value;

                        if (string.IsNullOrEmpty(_userId))
                        {
                            return;
                        }

                        if (_signInManager.IsSignedIn(_httpresource.HttpContext.User))
                        {
                            var _httpContext = _httpContextAccessor.HttpContext;

                            IPermissionService _permissionService = _scope.ServiceProvider.GetRequiredService<IPermissionService>();
                            HashSet<string> _permissions = await _permissionService.GetPermissions(_userId);

                            if (_permissions.Contains(requirement.Permission))
                            {
                                context.Succeed(requirement);
                            }
                            else
                            {
                                throw new PermissionDeniedException(HttpStatusCode.Forbidden);
                            }
                        }
                    }
                    else
                    {
                        context.Fail();
                        throw new JWTInvalidException(HttpStatusCode.Unauthorized, "JWT Authentication Faliure");
                    }
                }
            }

        }
    }
}
