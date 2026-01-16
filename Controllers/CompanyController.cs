using AssetRegistry.Attributes;
using AssetRegistry.DTOs.Company;
using AssetRegistry.DTOs.Response;
using AssetRegistry.Interfaces;
using AssetRegistry.Models.Company;
using AssetRegistry.Models.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Net;

namespace AssetRegistry.Controllers
{
    [Route("api/company")]
    [ApiController]
    public class CompanyController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IDateTimeService _dateTimeService;

        public CompanyController(
            UserManager<ApplicationUser> userManager, 
            ApplicationDbContext context,
            IDateTimeService dateTimeService)
        {
            _userManager = userManager;
            _context = context;
            _dateTimeService = dateTimeService;
        }

        [HasPermission("Company.Read")]
        [HttpGet]
        [Route("get-all")]
        public async Task<IActionResult> GetAllAsync()
        {
            try
            {
                var _companies = await (from co in _context.Companies
                                        select new CompanyListDTO()
                                        {
                                            Id = co.Id,
                                            CompanyId = co.Code,
                                            CompanyName = co.Name,
                                            IsActive = co.IsActive
                                        }).ToListAsync();
                return Ok(new ResponseDTO { code = (int)HttpStatusCode.OK, msg = "success", data = _companies });
            }
            catch (Exception ex)
            {
                return UnprocessableEntity(new ResponseDTO { code = (int)HttpStatusCode.InternalServerError, msg = $"{ex.Message}", data = "" });
            }
        }

        [HasPermission("Company.Read")]
        [HttpGet]
        [Route("get-by-id{id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            try
            {
                var _company = await (from co in _context.Companies
                                      where co.Id == id
                                      select new CompanyDTO()
                                      {
                                          Id = co.Id,
                                          CompanyId = co.Code,
                                          CompanyName = co.Name,
                                          IsActive = co.IsActive
                                      }).FirstOrDefaultAsync();
                return Ok(new ResponseDTO { code = (int)HttpStatusCode.OK, msg = "success", data = _company });
            }
            catch (Exception ex)
            {
                return UnprocessableEntity(new ResponseDTO { code = (int)HttpStatusCode.InternalServerError, msg = $"{ex.Message}", data = "" });
            }
        }

        [HasPermission("Company.Create")]
        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> CreateAsync([FromBody] CompanyCreateDTO model)
        {
            try
            {
                var _jwt = Request.Headers["Authorization"].ToString().Replace("Bearer ", "").Replace("bearer ", "");
                var _tokenstring = new JwtSecurityTokenHandler().ReadJwtToken(_jwt).Payload;
                var _postedUser = _tokenstring["oid"].ToString();
                var _loggedInUser = await _userManager.FindByIdAsync(_postedUser);

                Company company = new Company();
                company.Code = model.CompanyId;
                company.Name = model.Name;
                company.IsActive = true;
                _context.Companies.Add(company);
                await _context.SaveChangesAsync();

                return Ok(new ResponseDTO { code = (int)HttpStatusCode.OK, msg = "Company created successfully", data = "" });
            }
            catch (Exception ex)
            {
                return UnprocessableEntity(new ResponseDTO { code = (int)HttpStatusCode.InternalServerError, msg = $"{ex.Message}", data = "" });
            }
        }

        [HasPermission("Company.Update")]
        [HttpPut]
        [Route("update")]
        public async Task<IActionResult> UpdatedAsync([FromBody] CompanyUpdateDTO model)
        {
            try
            {
                var _jwt = Request.Headers["Authorization"].ToString().Replace("Bearer ", "").Replace("bearer ", "");
                var _tokenstring = new JwtSecurityTokenHandler().ReadJwtToken(_jwt).Payload;
                var _postedUser = _tokenstring["oid"].ToString();
                var _loggedInUser = await _userManager.FindByIdAsync(_postedUser);

                var _company = await _context.Companies.Where(x => x.Id == model.Id).FirstOrDefaultAsync();
                if (_company != null)
                {
                    _company.Code = model.CompanyId;
                    _company.Name = model.Name;
                    _company.IsActive = model.IsActive;
                    _context.Companies.Update(_company);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    return NotFound(new ResponseDTO { code = 404, msg = "Company not found", data = "" });
                }
                return Ok(new ResponseDTO { code = (int)HttpStatusCode.OK, msg = "Company updated successfully", data = "" });
            }
            catch (Exception ex)
            {
                return UnprocessableEntity(new ResponseDTO { code = (int)HttpStatusCode.InternalServerError, msg = $"{ex.Message}", data = "" });
            }
        }

        [HasPermission("Company.Delete")]
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

                var _company = await _context.Companies.Where(x => x.Id == id).FirstOrDefaultAsync();
                if (_company != null)
                {
                    _company.IsActive = false;
                    _context.Companies.Update(_company);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    return NotFound(new ResponseDTO { code = 404, msg = "Company not found", data = "" });
                }
                return Ok(new ResponseDTO { code = (int)HttpStatusCode.OK, msg = "Company deactivate successfully", data = "" });
            }
            catch (Exception ex)
            {
                return UnprocessableEntity(new ResponseDTO { code = (int)HttpStatusCode.InternalServerError, msg = $"{ex.Message}", data = "" });
            }
        }

        [HasPermission("Company.Delete")]
        [HttpDelete]
        [Route("delete{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            try
            {
                var _company = await _context.Companies.Where(x => x.Id == id).FirstOrDefaultAsync();
                if (_company != null)
                {
                    _company.IsActive = false;
                    _context.Companies.Remove(_company);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    return NotFound(new ResponseDTO { code = 404, msg = "Company not found", data = "" });
                }
                return Ok(new ResponseDTO { code = (int)HttpStatusCode.OK, msg = "Company deleted successfully", data = "" });
            }
            catch (Exception ex)
            {
                return UnprocessableEntity(new ResponseDTO { code = (int)HttpStatusCode.InternalServerError, msg = $"{ex.Message}", data = "" });
            }
        }

    }
}
