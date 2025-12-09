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

        [HttpGet]
        [Route("GetAll")]
        public async Task<IActionResult> GetAllAsync()
        {
            try
            {
                var _users = await (from us in _context.ApplicationUsers
                                    join ur in _context.UserRoles on us.Id equals ur.UserId
                                    join ro in _context.Roles on ur.RoleId equals ro.Id
                                    select new UserListDTO()
                                    {
                                        Id = us.Id,
                                        FirstName = us.FirstName,
                                        LastName = us.LastName,
                                        Email = us.Email,
                                        Nic = us.Nic,
                                        Address = us.Address,
                                        IsActive = us.IsActive,
                                        RoleId = ur.RoleId,
                                        RoleName = ro.Name
                                    }).ToListAsync();
                return Ok(new ResponseDTO { code = (int)HttpStatusCode.OK, msg = "success", data = _users });
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
                var _user = await (from us in _context.ApplicationUsers
                                   join ur in _context.UserRoles on us.Id equals ur.UserId
                                   join ro in _context.Roles on ur.RoleId equals ro.Id
                                   where us.Id == id
                                   select new UserListDTO()
                                   {
                                       Id = us.Id,
                                       FirstName = us.FirstName,
                                       LastName = us.LastName,
                                       Email = us.Email,
                                       Nic = us.Nic,
                                       Address = us.Address,
                                       IsActive = us.IsActive,
                                       RoleId = ur.RoleId,
                                       RoleName = ro.Name
                                   }).ToListAsync();
                return Ok(new ResponseDTO { code = (int)HttpStatusCode.OK, msg = "success", data = _user });
            }
            catch (Exception ex)
            {
                return UnprocessableEntity(new ResponseDTO { code = (int)HttpStatusCode.InternalServerError, msg = $"{ex.Message}", data = "" });
            }
        }

        [HttpPost]
        [Route("Create")]
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
                    FirstName = model.Email,
                    LastName = model.LastName,
                    Nic = model.Nic,
                    Address = model.Address,
                    Email = model.Email,
                    UserName = model.Email,
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

        [HttpPut]
        [Route("Update")]
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

                _user.FirstName = model.FirstName;
                _user.LastName = model.LastName;
                _user.Email = model.Email;
                _user.UserName = model.Email;

                var result = await _userManager.UpdateAsync(_user);
                var _token = await _userManager.GeneratePasswordResetTokenAsync(_user);
                await _userManager.ResetPasswordAsync(_user, _token, model.Password);

                if (!result.Succeeded)
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

        [HttpPut]
        [Route("Deactivate{id}")]
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
