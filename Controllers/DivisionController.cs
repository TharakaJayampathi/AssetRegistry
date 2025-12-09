using AssetRegistry.Attributes;
using AssetRegistry.DTOs.Division;
using AssetRegistry.DTOs.Response;
using AssetRegistry.Models.Division;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace AssetRegistry.Controllers
{
    [Route("api/[controller]")]
    [HasPermission("Company.Read")]
    [ApiController]
    public class DivisionController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DivisionController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("GetAll")]
        public async Task<IActionResult> GetAllAsync()
        {
            try
            {
                var _divisions = await _context.Divisions.ToListAsync();
                return Ok(new ResponseDTO { code = (int)HttpStatusCode.OK, msg = "success", data = _divisions });
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
                var _division = await _context.Divisions.Where(x => x.Id == id).FirstOrDefaultAsync();
                return Ok(new ResponseDTO { code = (int)HttpStatusCode.OK, msg = "success", data = _division });
            }
            catch (Exception ex)
            {
                return UnprocessableEntity(new ResponseDTO { code = (int)HttpStatusCode.InternalServerError, msg = $"{ex.Message}", data = "" });
            }
        }

        [HttpPost]
        [Route("Create")]
        public async Task<IActionResult> CreateAsync([FromBody] DivisionCreateDTO model)
        {
            try
            {
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

        [HttpPost]
        [Route("Update")]
        public async Task<IActionResult> UpdatedAsync([FromBody] DivisionUpdateDTO model)
        {
            try
            {
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

        [HttpPut]
        [Route("Deactivate{id}")]
        public async Task<IActionResult> DeactivateAsync(int id)
        {
            try
            {
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

        [HttpDelete]
        [Route("{id}")]
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
