using AssetRegistry.Handlers;
using AssetRegistry.Interfaces;
using AssetRegistry.Models.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.IdentityModel.Tokens.Jwt;

namespace AssetRegistry.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public IdentityService(
            UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
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
    }
}
