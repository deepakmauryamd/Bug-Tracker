using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BugTracker.Data;
using BugTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

namespace BugTracker.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public AdminController(
            RoleManager<IdentityRole> roleManager,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration
        )
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _configuration = configuration;
        }

        [HttpGet("/createroles")]
        public ActionResult CreateRoles()
        {
            return View();
        }

        [HttpPost("/createroles")]
        public async Task<ActionResult> CreateRoles(RoleModel model)
        {
            if (ModelState.IsValid)
            {
                IdentityRole identityRole = new IdentityRole
                {
                    Name = model.RoleName
                };

                var result = await _roleManager.CreateAsync(identityRole);
                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(DisplayRoles));
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(model);
        }

        [HttpGet("/displayusers")]
        public ActionResult DisplayUsers(string message = null)
        {
            var users = _userManager.Users;
            ViewBag.message = message;
            return View(users);
        }

        [HttpPost]
        public async Task<ActionResult> DeleteUser(string Id)
        {
            if (Id != null)
            {
                try
                {
                    var user = await _userManager.FindByIdAsync(Id);

                    var result = await _userManager.DeleteAsync(user);
                    if (result.Succeeded)
                    {
                        return RedirectToAction(nameof(DisplayUsers), new { message = "User Deleted successfully" });
                    }
                }
                catch (DbUpdateException ex)
                {
                    ViewBag.Message = @"Cannot delete user if it has created any bugs in any project. 
                                    To delete a user first remove all bugs which are added by that user";
                    ViewBag.ErrorMessage = ex.Message;
                    return View("~/Views/Shared/Error.cshtml");
                }
            }
            return RedirectToAction(nameof(DisplayUsers));
        }

        [HttpGet("/displayroles")]
        public ActionResult DisplayRoles(string message = null)
        {
            var roles = _roleManager.Roles;
            ViewBag.message = message;
            return View(roles);
        }

        [HttpPost("/deleterole/{Id}")]
        public async Task<ActionResult> DeleteRole(string Id)
        {
            var role = await _roleManager.FindByIdAsync(Id);
            if (role == null)
            {
                string message = $"Role for Id : {Id} not found.";
                return RedirectToAction(nameof(DisplayRoles), new { message = message });
            }
            try
            {
                var result = await _roleManager.DeleteAsync(role);
                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(DisplayRoles));
                }
            }
            catch (DbUpdateException ex)
            {
                ViewBag.Message = @"Cannot delete role if it has any users in it. 
                                    To delete a role first remove all users from that role then delete.";
                ViewBag.ErrorMessage = ex.Message;
                return View("~/Views/Shared/Error.cshtml");
            }
            string ErrorMessage = "Something went wrong, role not deleted.";
            return RedirectToAction(nameof(DisplayRoles), new { message = ErrorMessage });
        }

        [HttpGet("/editrole/{Id}")]
        public async Task<ActionResult> EditRole(string Id)
        {
            var role = await _roleManager.FindByIdAsync(Id);
            if (role == null)
            {
                string message = $"Role for Id : {Id} not found.";
                return RedirectToAction(nameof(DisplayRoles), new { message = message });
            }

            var editRoleModel = new EditRoleModel
            {
                Id = role.Id,
                RoleName = role.Name
            };

            editRoleModel.Users = new List<string>();

            var users = await _userManager.GetUsersInRoleAsync(role.Name);

            foreach (var user in users)
            {
                editRoleModel.Users.Add(user.UserName);
            }
            return View(editRoleModel);
        }

        [HttpPost("/editrole")]
        public async Task<ActionResult> EditRole(EditRoleModel model)
        {
            if (ModelState.IsValid)
            {
                var role = await _roleManager.FindByIdAsync(model.Id);
                if (role == null)
                {
                    string message = $"Role for Id : {model.Id} not found.";
                    return RedirectToAction(nameof(DisplayRoles), new { message = message });
                }

                role.Name = model.RoleName;
                var result = await _roleManager.UpdateAsync(role);
                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(DisplayRoles));
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(model);
        }

        [HttpGet("/edituserinrole/{roleId}")]
        public async Task<ActionResult> EditUserInRole(string roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                string message = $"Role for Id : {roleId} not found.";
                return RedirectToAction(nameof(DisplayRoles), new { message = message });
            }

            var users = _userManager.Users.ToList();

            var model = new List<UserRoleModel>();

            foreach (var user in users)
            {
                var userRoleModel = new UserRoleModel
                {
                    UserId = user.Id,
                    Username = user.UserName
                };
                if (await _userManager.IsInRoleAsync(user, role.Name))
                {
                    userRoleModel.IsSelected = true;
                }
                model.Add(userRoleModel);
            }

            ViewBag.RoleName = role.Name;
            ViewBag.RoleId = role.Id;

            return View(model);
        }

        [HttpPost("/edituserinrole/{roleId}")]
        public async Task<ActionResult> EditUserInRole(string roleId, List<UserRoleModel> model)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                string message = $"Role for Id : {roleId} not found.";
                return RedirectToAction(nameof(DisplayRoles), new { message = message });
            }
            for (int i = 0; i < model.Count; i++)
            {
                var user = await _userManager.FindByIdAsync(model[i].UserId);
                var result = new IdentityResult();

                if (model[i].IsSelected && !(await _userManager.IsInRoleAsync(user, role.Name)))
                {
                    result = await _userManager.AddToRoleAsync(user, role.Name);
                }
                else if (!model[i].IsSelected && (await _userManager.IsInRoleAsync(user, role.Name)))
                {
                    result = await _userManager.RemoveFromRoleAsync(user, role.Name);
                }
                else
                {
                    continue;
                }
                if (result.Succeeded == false && i == (model.Count - 1))
                {
                    return RedirectToAction(nameof(EditUserInRole), new { Id = roleId });
                }
            }
            return RedirectToAction(nameof(EditRole), new { Id = roleId });
        }

        [AllowAnonymous]
        [Route("/role/admin/")]
        public async Task<IActionResult> CreateAdminUser()
        {
            if (await _userManager.Users.AnyAsync())
            {
                if (!User.IsInRole("Admin"))
                {
                    return Forbid();
                }
            }

            if (!await _roleManager.RoleExistsAsync("Admin"))
            {
                await _roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            ApplicationUser admin = await _userManager.FindByNameAsync("admin");
            if (admin != null)
            {
                return BadRequest("Admin user already exists.");
            }
            var adminPassword = _configuration["AdminPassword"];
            if (string.IsNullOrEmpty(adminPassword))
            {
                return BadRequest("Admin password not set in configuration.");
            }

            var adminUser = new ApplicationUser
            {
                UserName = "admin",
                Email = "admin@example.com"
            };
            var result = await _userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(adminUser, "Admin");
                return Ok("Admin user created successfully.");
            }
            return BadRequest("Something went Wrong.");
        }
    }
}