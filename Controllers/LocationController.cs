using AssetRegistry.Attributes;
using AssetRegistry.DTOs.Location;
using AssetRegistry.DTOs.Response;
using AssetRegistry.Interfaces;
using AssetRegistry.Models.Location;
using AssetRegistry.Models.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Net;

namespace AssetRegistry.Controllers
{
    [Route("api/location")]
    [ApiController]
    public class LocationController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IDateTimeService _dateTimeService;

        public LocationController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            IDateTimeService dateTimeService)
        {
            _userManager = userManager;
            _context = context;
            _dateTimeService = dateTimeService;
        }

        [HasPermission("Location.Read")]
        [HttpGet]
        [Route("get-all")]
        public async Task<IActionResult> GetAllAsync()
        {
            try
            {
                var _locations = await (from lo in _context.Locations
                                        join co in _context.Companies on lo.CompanyId equals co.Id into co_join
                                        from co in co_join.DefaultIfEmpty()
                                        join di in _context.Divisions on co.Id equals di.CompanyId into di_join
                                        from di in di_join.DefaultIfEmpty()
                                        select new LocationListDTO()
                                        {
                                            Id = lo.Id,
                                            LocationId = lo.Code,
                                            LocationAddress = lo.Address,
                                            CompanyName = co.Name,
                                            DivisionId = di.Code,
                                            DivisionName = di.Name,
                                            IsActive = lo.IsActive
                                        }).FirstOrDefaultAsync();
                return Ok(new ResponseDTO { code = (int)HttpStatusCode.OK, msg = "success", data = _locations });
            }
            catch (Exception ex)
            {
                return UnprocessableEntity(new ResponseDTO { code = (int)HttpStatusCode.InternalServerError, msg = $"{ex.Message}", data = "" });
            }
        }

        [HasPermission("Location.Read")]
        [HttpGet]
        [Route("get-by-id{id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            try
            {
                var _location = await (from lo in _context.Locations
                                       join co in _context.Companies on lo.CompanyId equals co.Id into co_join
                                       from co in co_join.DefaultIfEmpty()
                                       join di in _context.Divisions on co.Id equals di.CompanyId into di_join
                                       from di in di_join.DefaultIfEmpty()
                                       where lo.Id == id
                                       select new LocationListDTO()
                                       {
                                           Id = lo.Id,
                                           LocationId = lo.Code,
                                           LocationAddress = lo.Address,
                                           CompanyName = co.Name,
                                           DivisionId = di.Code,
                                           DivisionName = di.Name,
                                           IsActive = lo.IsActive
                                       }).FirstOrDefaultAsync();
                return Ok(new ResponseDTO { code = (int)HttpStatusCode.OK, msg = "success", data = _location });
            }
            catch (Exception ex)
            {
                return UnprocessableEntity(new ResponseDTO { code = (int)HttpStatusCode.InternalServerError, msg = $"{ex.Message}", data = "" });
            }
        }

        [HasPermission("Location.Create")]
        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> CreateAsync([FromBody] LocationCreateDTO model)
        {
            try
            {
                var _jwt = Request.Headers["Authorization"].ToString().Replace("Bearer ", "").Replace("bearer ", "");
                var _tokenstring = new JwtSecurityTokenHandler().ReadJwtToken(_jwt).Payload;
                var _postedUser = _tokenstring["oid"].ToString();
                var _loggedInUser = await _userManager.FindByIdAsync(_postedUser);

                Location location = new Location();
                location.Code = model.LocatonId;
                location.Address = model.LocationAddress;
                location.CompanyId = model.CompanyId;
                location.DivisionId = model.DivisionId;
                location.IsActive = true;
                _context.Locations.Add(location);
                await _context.SaveChangesAsync();

                return Ok(new ResponseDTO { code = (int)HttpStatusCode.OK, msg = "Location created successfully", data = "" });
            }
            catch (Exception ex)
            {
                return UnprocessableEntity(new ResponseDTO { code = (int)HttpStatusCode.InternalServerError, msg = $"{ex.Message}", data = "" });
            }
        }

        [HasPermission("Location.Update")]
        [HttpPut]
        [Route("update")]
        public async Task<IActionResult> UpdatedAsync([FromBody] LocationUpdateDTO model)
        {
            try
            {
                var _jwt = Request.Headers["Authorization"].ToString().Replace("Bearer ", "").Replace("bearer ", "");
                var _tokenstring = new JwtSecurityTokenHandler().ReadJwtToken(_jwt).Payload;
                var _postedUser = _tokenstring["oid"].ToString();
                var _loggedInUser = await _userManager.FindByIdAsync(_postedUser);

                var _location = await _context.Locations.Where(x => x.Id == model.Id).FirstOrDefaultAsync();
                if (_location != null)
                {
                    _location.Code = model.LocatonId;
                    _location.Address = model.LocationAddress;
                    _location.CompanyId = model.CompanyId;
                    _location.DivisionId = model.DivisionId;
                    _location.IsActive = model.IsActive;
                    _context.Locations.Update(_location);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    return NotFound(new ResponseDTO { code = 404, msg = "Location not found", data = "" });
                }
                return Ok(new ResponseDTO { code = (int)HttpStatusCode.OK, msg = "Location updated successfully", data = "" });
            }
            catch (Exception ex)
            {
                return UnprocessableEntity(new ResponseDTO { code = (int)HttpStatusCode.InternalServerError, msg = $"{ex.Message}", data = "" });
            }
        }

        [HasPermission("Location.Delete")]
        [HttpPut]
        [Route("deactivate{id}")]
        public async Task<IActionResult> DeactivateAsync(int id)
        {
            try
            {
                var _jwt = Request.Headers["Authorization"].ToString().Replace("Bearer ", "").Replace("bearer ", "");
                var _tokenstring = new JwtSecurityTokenHandler().ReadJwtToken(_jwt).Payload;
                var _postedUser = _tokenstring["oid"].ToString();
                var _loggedInUser = await _userManager.FindByIdAsync(_postedUser);

                var _location = await _context.Locations.Where(x => x.Id == id).FirstOrDefaultAsync();
                if (_location != null)
                {
                    _location.IsActive = false;
                    _context.Locations.Update(_location);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    return NotFound(new ResponseDTO { code = 404, msg = "Location not found", data = "" });
                }
                return Ok(new ResponseDTO { code = (int)HttpStatusCode.OK, msg = "Location deactivate successfully", data = "" });
            }
            catch (Exception ex)
            {
                return UnprocessableEntity(new ResponseDTO { code = (int)HttpStatusCode.InternalServerError, msg = $"{ex.Message}", data = "" });
            }
        }

        [HasPermission("Location.Delete")]
        [HttpDelete]
        [Route("delete{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            try
            {
                var _location = await _context.Locations.Where(x => x.Id == id).FirstOrDefaultAsync();
                if (_location != null)
                {
                    _location.IsActive = false;
                    _context.Locations.Remove(_location);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    return NotFound(new ResponseDTO { code = 404, msg = "Location not found", data = "" });
                }
                return Ok(new ResponseDTO { code = (int)HttpStatusCode.OK, msg = "Location deleted successfully", data = "" });
            }
            catch (Exception ex)
            {
                return UnprocessableEntity(new ResponseDTO { code = (int)HttpStatusCode.InternalServerError, msg = $"{ex.Message}", data = "" });
            }
        }

    }
}
