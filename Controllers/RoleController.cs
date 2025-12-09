using AssetRegistry.DTOs.Response;
using AssetRegistry.DTOs.Roles;
using AssetRegistry.Models.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace AssetRegistry.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public RoleController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        [HttpGet]
        [Route("GetAll")]
        public async Task<IActionResult> GetAllAsync()
        {
            try
            {
                var roles = await _context.Roles.ToListAsync();
                return Ok(new ResponseDTO { code = (int)HttpStatusCode.OK, msg = "success", data = roles });
            }
            catch (Exception ex)
            {
                return UnprocessableEntity(new ResponseDTO { code = (int)HttpStatusCode.InternalServerError, msg = $"{ex.Message}", data = "" });
            }
        }

        [HttpGet]
        [Route("GetById{id}")]
        public async Task<IActionResult> GetByIdAsync(string id)
        {
            try
            {
                var _role = await _roleManager.FindByIdAsync(id);
                return Ok(new ResponseDTO { code = (int)HttpStatusCode.OK, msg = "success", data = _role });
            }
            catch (Exception ex)
            {
                return UnprocessableEntity(new ResponseDTO { code = (int)HttpStatusCode.InternalServerError, msg = $"{ex.Message}", data = "" });
            }
        }

        [HttpPost]
        [Route("Create")]
        public async Task<IActionResult> CreateAsync(string roleName)
        {
            try
            {
                var _role = await _roleManager.FindByNameAsync(roleName.Trim());
                if (_role != null)
                {
                    return UnprocessableEntity(new ResponseDTO { code = (int)HttpStatusCode.InternalServerError, msg = "Role already exists", data = "" });
                }
                await _roleManager.CreateAsync(new IdentityRole(roleName));
                return Ok(new ResponseDTO { code = (int)HttpStatusCode.OK, msg = "Role created successfully", data = "" });
            }
            catch (Exception ex)
            {
                return UnprocessableEntity(new ResponseDTO { code = (int)HttpStatusCode.InternalServerError, msg = $"{ex.Message}", data = "" });
            }
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IActionResult> UpdateAsync(RoleUpdateDTO model)
        {
            try
            {
                var _role = await _roleManager.FindByIdAsync(model.RoleId);
                var _roleByName = await _roleManager.FindByNameAsync(model.RoleName.Trim());
                if (_roleByName != null)
                {
                    if (_roleByName.Id != model.RoleId)
                    {
                        return UnprocessableEntity(new ResponseDTO { code = (int)HttpStatusCode.InternalServerError, msg = "Role already exists", data = "" });
                    }
                }
                _role.Name = model.RoleName;
                await _roleManager.UpdateAsync(_role);
                return Ok(new ResponseDTO { code = (int)HttpStatusCode.OK, msg = "Role updated successfully", data = "" });
            }
            catch (Exception ex)
            {
                return UnprocessableEntity(new ResponseDTO { code = (int)HttpStatusCode.InternalServerError, msg = $"{ex.Message}", data = "" });
            }
        }
    }
}
