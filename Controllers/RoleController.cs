using AssetRegistry.DTOs.Response;
using AssetRegistry.DTOs.Roles;
using AssetRegistry.Models.Role;
using AssetRegistry.Models.RolePermission;
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
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public RoleController(
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context)
        {
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
        public async Task<IActionResult> CreateAsync(RoleCreateDTO model)
        {
            try
            {
                var _role = await _roleManager.FindByNameAsync(model.RoleName.Trim());
                if (_role != null)
                {
                    return UnprocessableEntity(new ResponseDTO { code = (int)HttpStatusCode.InternalServerError, msg = "Role already exists", data = "" });
                }
                var _res = await _roleManager.CreateAsync(new IdentityRole(model.RoleName));
                if (_res.Succeeded)
                {
                    var _roleDetail = _context.Roles.Where(x => x.Name == model.RoleName).FirstOrDefault();
                    if (_roleDetail != null)
                    {
                        RoleData roleData = new RoleData();
                        roleData.RoleId = _roleDetail.Id;
                        roleData.Code = model.Code;
                        roleData.IsActive = true;
                        _context.RoleDatas.Add(roleData);
                        await _context.SaveChangesAsync();

                        List<RolePermission> rolePermissionList = new List<RolePermission>();
                        foreach (var _permission in model.Permissions)
                        {
                            RolePermission rolePermission = new RolePermission();
                            rolePermission.RoleId = _roleDetail.Id;
                            rolePermission.PermissionType = _permission;
                            rolePermissionList.Add(rolePermission);
                        }
                        _context.RolePermissions.AddRange(rolePermissionList);
                        await _context.SaveChangesAsync();
                    }
                }
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
                var _res = await _roleManager.UpdateAsync(_role);
                if (_res.Succeeded)
                {
                    var roleData = _context.RoleDatas.Where(x => x.RoleId == model.RoleId).FirstOrDefault();
                    if (roleData != null)
                    {
                        roleData.Code = model.Code;
                        roleData.IsActive = model.IsActive;
                        _context.RoleDatas.Update(roleData);
                        await _context.SaveChangesAsync();

                        var _existingRolePermissions = await _context.RolePermissions.Where(x => x.RoleId == model.RoleId).ToListAsync();
                        _context.RolePermissions.RemoveRange(_existingRolePermissions);
                        await _context.SaveChangesAsync();

                        List<RolePermission> rolePermissionList = new List<RolePermission>();
                        foreach (var _permission in model.Permissions)
                        {
                            RolePermission rolePermission = new RolePermission();
                            rolePermission.RoleId = model.RoleId;
                            rolePermission.PermissionType = _permission;
                            rolePermissionList.Add(rolePermission);
                        }
                        _context.RolePermissions.AddRange(rolePermissionList);
                        await _context.SaveChangesAsync();
                    }
                }
                return Ok(new ResponseDTO { code = (int)HttpStatusCode.OK, msg = "Role updated successfully", data = "" });
            }
            catch (Exception ex)
            {
                return UnprocessableEntity(new ResponseDTO { code = (int)HttpStatusCode.InternalServerError, msg = $"{ex.Message}", data = "" });
            }
        }
    }
}
