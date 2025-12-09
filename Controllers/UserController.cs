using AssetRegistry.Attributes;
using AssetRegistry.DTOs.Response;
using AssetRegistry.DTOs.Users;
using AssetRegistry.Models.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace AssetRegistry.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public UserController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }


        [HasPermission("User.Read")]
        [HttpGet]
        [Route("get-all")]
        public async Task<IActionResult> GetAllAsync()
        {
            try
            {
                var _users = await (from us in _context.ApplicationUsers
                                    join ur in _context.UserRoles on us.Id equals ur.UserId into ur_join
                                    from ur in ur_join.DefaultIfEmpty()
                                    join ro in _context.Roles on ur.RoleId equals ro.Id into ro_join
                                    from ro in ro_join.DefaultIfEmpty()
                                    join co in _context.Companies on us.CompanyId equals co.Id into co_join
                                    from co in co_join.DefaultIfEmpty()
                                    join di in _context.Divisions on us.DivisionId equals di.Id into di_join
                                    from di in di_join.DefaultIfEmpty()
                                    join lo in _context.Locations on us.LocationId equals lo.Id into lo_join
                                    from lo in lo_join.DefaultIfEmpty()
                                    select new UserListDTO()
                                    {
                                        Id = us.Id,
                                        UserId = us.Code,
                                        FirstName = us.FirstName,
                                        LastName = us.LastName,
                                        FullName = $"{us.FirstName} {us.LastName}",
                                        Email = us.Email,
                                        PhoneNumber = us.PhoneNumber,
                                        RoleName = ro.Name,
                                        CompanyName = co.Name,
                                        DivisionId = di.Code,
                                        DivisionName = di.Name,
                                        LocationAddress = lo.Address,
                                        IsActive = us.IsActive
                                    }).ToListAsync();
                return Ok(new ResponseDTO { code = (int)HttpStatusCode.OK, msg = "success", data = _users });
            }
            catch (Exception ex)
            {
                return UnprocessableEntity(new ResponseDTO { code = (int)HttpStatusCode.InternalServerError, msg = $"{ex.Message}", data = "" });
            }
        }


        [HasPermission("User.Read")]
        [HttpGet]
        [Route("get-by-id{id}")]
        public async Task<IActionResult> GetByIdAsync(string id)
        {
            try
            {
                var _user = await (from us in _context.ApplicationUsers
                                   join ur in _context.UserRoles on us.Id equals ur.UserId into ur_join
                                   from ur in ur_join.DefaultIfEmpty()
                                   join ro in _context.Roles on ur.RoleId equals ro.Id into ro_join
                                   from ro in ro_join.DefaultIfEmpty()
                                   join co in _context.Companies on us.CompanyId equals co.Id into co_join
                                   from co in co_join.DefaultIfEmpty()
                                   join di in _context.Divisions on us.DivisionId equals di.Id into di_join
                                   from di in di_join.DefaultIfEmpty()
                                   join lo in _context.Locations on us.LocationId equals lo.Id into lo_join
                                   from lo in lo_join.DefaultIfEmpty()
                                   where us.Id == id
                                   select new UserDTO()
                                   {
                                       Id = us.Id,
                                       UserId = us.Code,
                                       FirstName = us.FirstName,
                                       LastName = us.LastName,
                                       FullName = $"{us.FirstName} {us.LastName}",
                                       Email = us.Email,
                                       PhoneNumber = us.PhoneNumber,
                                       RoleName = ro.Name,
                                       CompanyName = co.Name,
                                       DivisionId = di.Code,
                                       DivisionName = di.Name,
                                       LocationAddress = lo.Address,
                                       IsActive = us.IsActive
                                   }).FirstOrDefaultAsync();
                return Ok(new ResponseDTO { code = (int)HttpStatusCode.OK, msg = "success", data = _user });
            }
            catch (Exception ex)
            {
                return UnprocessableEntity(new ResponseDTO { code = (int)HttpStatusCode.InternalServerError, msg = $"{ex.Message}", data = "" });
            }
        }


        [HasPermission("User.Create")]
        [HttpPost]
        [Route("create")]
        public async Task<IActionResult> CreateAsync([FromBody] UserCreateDTO model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.Email) || !new EmailAddressAttribute().IsValid(model.Email))
                {
                    return UnprocessableEntity(new ResponseDTO { code = (int)HttpStatusCode.InternalServerError, msg = "Invalid email address", data = "" });
                }

                var userExists = await _userManager.FindByNameAsync(model.Email);
                if (userExists != null)
                {
                    return UnprocessableEntity(new ResponseDTO { code = (int)HttpStatusCode.InternalServerError, msg = "User already exists", data = "" });
                }

                var _role = await _roleManager.FindByIdAsync(model.RoleId);
                if (_role == null)
                {
                    return UnprocessableEntity(new ResponseDTO { code = (int)HttpStatusCode.InternalServerError, msg = "Role Not Exist", data = "" });
                }

                ApplicationUser user = new()
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    UserName = model.Email,
                    PhoneNumber = model.PhoneNumber,
                    CompanyId = model.CompanyId,
                    DivisionId = model.DivisionId,
                    LocationId = model.LocationId,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    IsActive = true,
                    CreatedBy = "Admin",
                    CreatedOn = DateTime.Now
                };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, _role.Name);
                }
                else
                {
                    return UnprocessableEntity(new ResponseDTO { code = (int)HttpStatusCode.InternalServerError, msg = "User creation failed", data = "" });
                }
                return Ok(new ResponseDTO { code = (int)HttpStatusCode.OK, msg = "User created successfully", data = "" });
            }
            catch (Exception ex)
            {
                return UnprocessableEntity(new ResponseDTO { code = (int)HttpStatusCode.InternalServerError, msg = $"{ex.Message}", data = "" });
            }
        }

        [HasPermission("User.Update")]
        [HttpPut]
        [Route("update")]
        public async Task<IActionResult> UpdateAsync([FromBody] UserUpdateDTO model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.Email) || !new EmailAddressAttribute().IsValid(model.Email))
                {
                    return UnprocessableEntity(new ResponseDTO { code = (int)HttpStatusCode.InternalServerError, msg = "Invalid email address", data = "" });
                }

                var _user = await _userManager.FindByIdAsync(model.UserId);
                if (_user == null)
                {
                    return NotFound(new ResponseDTO { code = 404, msg = "User not found", data = "" });
                }

                var _role = await _roleManager.FindByIdAsync(model.RoleId);
                if (_role == null)
                {
                    return UnprocessableEntity(new ResponseDTO { code = (int)HttpStatusCode.InternalServerError, msg = "Role Not Exist", data = "" });
                }

                _user.FirstName = model.FirstName;
                _user.LastName = model.LastName;
                _user.Email = model.Email;
                _user.UserName = model.Email;
                _user.PhoneNumber = model.PhoneNumber;
                _user.CompanyId = model.CompanyId;
                _user.DivisionId = model.DivisionId;
                _user.LocationId = model.LocationId;
                _user.IsActive = model.IsActive;

                var result = await _userManager.UpdateAsync(_user);
                if (result.Succeeded)
                {
                    var _inRole = await _userManager.GetRolesAsync(_user);
                    if (_inRole.Count() > 0)
                    {
                        await _userManager.RemoveFromRolesAsync(_user, _inRole);
                    }
                    await _userManager.AddToRoleAsync(_user, _role.Name);

                    var _token = await _userManager.GeneratePasswordResetTokenAsync(_user);
                    await _userManager.ResetPasswordAsync(_user, _token, model.Password);
                }
                else
                {
                    return UnprocessableEntity(new ResponseDTO { code = (int)HttpStatusCode.InternalServerError, msg = "User update failed", data = "" });
                }
                return Ok(new ResponseDTO { code = (int)HttpStatusCode.OK, msg = "User updated successfully", data = "" });
            }
            catch (Exception ex)
            {
                return UnprocessableEntity(new ResponseDTO { code = (int)HttpStatusCode.InternalServerError, msg = $"{ex.Message}", data = "" });
            }
        }

        [HasPermission("User.Delete")]
        [HttpPut]
        [Route("deactivate{id}")]
        public async Task<IActionResult> DeactivateAsync(string id)
        {
            try
            {
                var _user = await _userManager.FindByIdAsync(id);
                if (_user == null)
                {
                    return NotFound(new ResponseDTO { code = 404, msg = "User not found", data = "" });
                }
                _user.IsActive = false;
                await _userManager.UpdateAsync(_user);

                return Ok(new ResponseDTO { code = (int)HttpStatusCode.OK, msg = "User deactivate successfully", data = "" });
            }
            catch (Exception ex)
            {
                return UnprocessableEntity(new ResponseDTO { code = (int)HttpStatusCode.InternalServerError, msg = $"{ex.Message}", data = "" });
            }
        }
    }
}
