using AssetRegistry.Attributes;
using AssetRegistry.DTOs.Division;
using AssetRegistry.DTOs.Response;
using AssetRegistry.Interfaces;
using AssetRegistry.Models.Division;
using AssetRegistry.Models.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Net;

namespace AssetRegistry.Controllers
{
    [Route("api/division")]
    [ApiController]
    public class DivisionController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IDateTimeService _dateTimeService;

        public DivisionController(
            UserManager<ApplicationUser> userManager, 
            ApplicationDbContext context,
            IDateTimeService dateTimeService)
        {
            _userManager = userManager;
            _context = context;
            _dateTimeService = dateTimeService;
        }

        [HasPermission("Division.Read")]
        [HttpGet]
        [Route("get-all")]
        public async Task<IActionResult> GetAllAsync()
        {
            try
            {
                var _divisions = await (from di in _context.Divisions
                                        join co in _context.Companies on di.CompanyId equals co.Id into co_join
                                        from co in co_join.DefaultIfEmpty()
                                        select new DivisionListDTO()
                                        {
                                            Id = di.Id,
                                            DivisionId = di.Code,
                                            DivisionName = di.Name,
                                            CompanyName = co.Name
                                        }).FirstOrDefaultAsync();
                return Ok(new ResponseDTO { code = (int)HttpStatusCode.OK, msg = "success", data = _divisions });
            }
            catch (Exception ex)
            {
                return UnprocessableEntity(new ResponseDTO { code = (int)HttpStatusCode.InternalServerError, msg = $"{ex.Message}", data = "" });
            }
        }

        [HasPermission("Division.Read")]
        [HttpGet]
        [Route("get-by-id{id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            try
            {
                var _division = await (from di in _context.Divisions
                                        join co in _context.Companies on di.CompanyId equals co.Id into co_join
                                        from co in co_join.DefaultIfEmpty()
                                        where di.Id == id
                                        select new DivisionDTO()
                                        {
                                            Id = di.Id,
                                            DivisionId = di.Code,
                                            DivisionName = di.Name,
                                            CompanyName = co.Name
                                        }).FirstOrDefaultAsync();
                return Ok(new ResponseDTO { code = (int)HttpStatusCode.OK, msg = "success", data = _division });
            }
            catch (Exception ex)
            {
                return UnprocessableEntity(new ResponseDTO { code = (int)HttpStatusCode.InternalServerError, msg = $"{ex.Message}", data = "" });
            }
        }

        [HasPermission("Division.Create")]
        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> CreateAsync([FromBody] DivisionCreateDTO model)
        {
            try
            {
                var _jwt = Request.Headers["Authorization"].ToString().Replace("Bearer ", "").Replace("bearer ", "");
                var _tokenstring = new JwtSecurityTokenHandler().ReadJwtToken(_jwt).Payload;
                var _postedUser = _tokenstring["oid"].ToString();
                var _loggedInUser = await _userManager.FindByIdAsync(_postedUser);

                Division division = new Division();
                division.Code = model.DivisionId;
                division.Name = model.DivisionName;
                division.CompanyId = model.CompanyId;
                division.IsActive = true;
                _context.Divisions.Add(division);
                await _context.SaveChangesAsync();

                return Ok(new ResponseDTO { code = (int)HttpStatusCode.OK, msg = "Division created successfully", data = "" });
            }
            catch (Exception ex)
            {
                return UnprocessableEntity(new ResponseDTO { code = (int)HttpStatusCode.InternalServerError, msg = $"{ex.Message}", data = "" });
            }
        }

        [HasPermission("Division.Update")]
        [HttpPut]
        [Route("update")]
        public async Task<IActionResult> UpdatedAsync([FromBody] DivisionUpdateDTO model)
        {
            try
            {
                var _jwt = Request.Headers["Authorization"].ToString().Replace("Bearer ", "").Replace("bearer ", "");
                var _tokenstring = new JwtSecurityTokenHandler().ReadJwtToken(_jwt).Payload;
                var _postedUser = _tokenstring["oid"].ToString();
                var _loggedInUser = await _userManager.FindByIdAsync(_postedUser);

                var _division = await _context.Divisions.Where(x => x.Id == model.Id).FirstOrDefaultAsync();
                if (_division != null)
                {
                    _division.Code = model.DivisionId;
                    _division.Name = model.DivisionName;
                    _division.CompanyId = model.CompanyId;
                    _division.IsActive = model.IsActive;
                    _context.Divisions.Update(_division);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    return NotFound(new ResponseDTO { code = 404, msg = "Division not found", data = "" });
                }
                return Ok(new ResponseDTO { code = (int)HttpStatusCode.OK, msg = "Division updated successfully", data = "" });
            }
            catch (Exception ex)
            {
                return UnprocessableEntity(new ResponseDTO { code = (int)HttpStatusCode.InternalServerError, msg = $"{ex.Message}", data = "" });
            }
        }

        [HasPermission("Division.Delete")]
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

                var _division = await _context.Divisions.Where(x => x.Id == id).FirstOrDefaultAsync();
                if (_division != null)
                {
                    _division.IsActive = false;
                    _context.Divisions.Update(_division);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    return NotFound(new ResponseDTO { code = 404, msg = "Division not found", data = "" });
                }
                return Ok(new ResponseDTO { code = (int)HttpStatusCode.OK, msg = "Division deactivate successfully", data = "" });
            }
            catch (Exception ex)
            {
                return UnprocessableEntity(new ResponseDTO { code = (int)HttpStatusCode.InternalServerError, msg = $"{ex.Message}", data = "" });
            }
        }

        [HasPermission("Division.Delete")]
        [HttpDelete]
        [Route("delete{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            try
            {
                var _division = await _context.Divisions.Where(x => x.Id == id).FirstOrDefaultAsync();
                if (_division != null)
                {
                    _division.IsActive = false;
                    _context.Divisions.Remove(_division);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    return NotFound(new ResponseDTO { code = 404, msg = "Division not found", data = "" });
                }
                return Ok(new ResponseDTO { code = (int)HttpStatusCode.OK, msg = "Division deleted successfully", data = "" });
            }
            catch (Exception ex)
            {
                return UnprocessableEntity(new ResponseDTO { code = (int)HttpStatusCode.InternalServerError, msg = $"{ex.Message}", data = "" });
            }
        }

    }
}
