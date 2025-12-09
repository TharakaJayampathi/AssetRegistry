using AssetRegistry.Attributes;
using AssetRegistry.DTOs.Location;
using AssetRegistry.DTOs.Response;
using AssetRegistry.Models.Location;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace AssetRegistry.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LocationController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HasPermission("Location.Read")]
        [HttpGet]
        [Route("GetAll")]
        public async Task<IActionResult> GetAllAsync()
        {
            try
            {
                var _locations = await _context.Companies.ToListAsync();
                return Ok(new ResponseDTO { code = (int)HttpStatusCode.OK, msg = "success", data = _locations });
            }
            catch (Exception ex)
            {
                return UnprocessableEntity(new ResponseDTO { code = (int)HttpStatusCode.InternalServerError, msg = $"{ex.Message}", data = "" });
            }
        }

        [HasPermission("Location.Read")]
        [HttpGet]
        [Route("GetById{id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            try
            {
                var _location = await _context.Companies.Where(x => x.Id == id).FirstOrDefaultAsync();
                return Ok(new ResponseDTO { code = (int)HttpStatusCode.OK, msg = "success", data = _location });
            }
            catch (Exception ex)
            {
                return UnprocessableEntity(new ResponseDTO { code = (int)HttpStatusCode.InternalServerError, msg = $"{ex.Message}", data = "" });
            }
        }

        [HasPermission("Location.Create")]
        [HttpPost]
        [Route("Create")]
        public async Task<IActionResult> CreateAsync([FromBody] LocationCreateDTO model)
        {
            try
            {
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
        [Route("Update")]
        public async Task<IActionResult> UpdatedAsync([FromBody] LocationUpdateDTO model)
        {
            try
            {
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
        [Route("Deactivate{id}")]
        public async Task<IActionResult> DeactivateAsync(int id)
        {
            try
            {
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
        [Route("{id}")]
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
