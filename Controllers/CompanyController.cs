using AssetRegistry.DTOs.Company;
using AssetRegistry.DTOs.Response;
using AssetRegistry.Models.Company;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace AssetRegistry.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompanyController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CompanyController(
            ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("GetAll")]
        public async Task<IActionResult> GetAllAsync()
        {
            try
            {
                var companies = await _context.Companies.ToListAsync();
                return Ok(new ResponseDTO { code = (int)HttpStatusCode.OK, msg = "success", data = companies });
            }
            catch (Exception ex)
            {
                return UnprocessableEntity(new ResponseDTO { code = (int)HttpStatusCode.InternalServerError, msg = $"{ex.Message}", data = "" });
            }
        }

        [HttpGet]
        [Route("GetById{id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            try
            {
                var _company = await _context.Companies.Where(x => x.Id == id).FirstOrDefaultAsync();
                return Ok(new ResponseDTO { code = (int)HttpStatusCode.OK, msg = "success", data = _company });
            }
            catch (Exception ex)
            {
                return UnprocessableEntity(new ResponseDTO { code = (int)HttpStatusCode.InternalServerError, msg = $"{ex.Message}", data = "" });
            }
        }

        [HttpPost]
        [Route("Create")]
        public async Task<IActionResult> CreateAsync([FromBody] CompanyCreateDTO model)
        {
            try
            {
                Company company = new Company();
                company.CompanyId = model.CompanyId;
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

        [HttpPost]
        [Route("Update")]
        public async Task<IActionResult> UpdatedAsync([FromBody] CompanyUpdateDTO model)
        {
            try
            {
                var _company = await _context.Companies.Where(x => x.Id == model.Id).FirstOrDefaultAsync();
                if (_company != null)
                {
                    _company.CompanyId = model.CompanyId;
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

        [HttpPut]
        [Route("Deactivate{id}")]
        public async Task<IActionResult> DeactivateAsync(int id)
        {
            try
            {
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

        [HttpDelete]
        [Route("{id}")]
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
