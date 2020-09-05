using System;
using System.Threading.Tasks;
using BugTracker.Data;
using BugTracker.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace BugTracker.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet("/login")]
        public ActionResult Login(string message)
        {   
            if(_signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Message = message;
            return View();
        }

        [HttpPost("/login")]
        public async Task<ActionResult> Login(UserLoginModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Username, model.Password, false, false);
                if(result.Succeeded)
                {
                    var name = _signInManager.IsSignedIn(User);
                    return RedirectToAction("Index", "Home");
                }
            }
            string message = "Username or Password is incorrect.";
            return RedirectToAction(nameof(Login), new {message = message});
        }

        [HttpGet("/signup")]
        public ActionResult SignUp(string message)
        {
            if(_signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Message = message;
            if (TempData["SignUpError"] is string stringModel)
            {
                var model = JsonConvert.DeserializeObject<UserSignUpModel>(stringModel);
                return View(model);
            }
            return View();
        }

        [HttpPost("/signup")]
        public async Task<ActionResult> SignUp(UserSignUpModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Username,
                    Email = model.Email
                };
                var userData = await _userManager.FindByEmailAsync(model.Email);
                if (userData != null)
                {
                    string message = "Email Already Registred.";
                    return RedirectToAction(nameof(SignUp), new {message = message});
                }
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("index", "home");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            TempData["SignUpError"] = JsonConvert.SerializeObject(model);
            return RedirectToAction(nameof(SignUp));
        }

        [HttpGet("/logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet("/accessdenied")]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}