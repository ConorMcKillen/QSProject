using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

using QSProject.Data.Services;
using QSProject.Models;
using QSProject.Helpers;
using QSProject.Data.Security; 
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using QSProject.Data.Models;

namespace QSProject.Controllers
{
    public class UserController : BaseController
    {
        private readonly IMedicineService _svc;
        private readonly IConfiguration _config;

        // Configured via DI
        public UserController(IMedicineService svc, IConfiguration config)
        {
            _svc = svc;
            _config = config;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login([Bind("Email, Password")] UserLoginViewModel m)
        {
            // call service to Authenticate user
            var user = _svc.Authenticate(m.Email, m.Password);

            if (user == null)
            {
                ModelState.AddModelError("Email", "Invalid Login Credentials");
                ModelState.AddModelError("Password", "Invalid Login Credentials");
                return View(m);
            }

            // sign user in using cookie authenication to store principal
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                AuthBuilder.BuildClaimsPrincipal(user)
                );
            return RedirectToAction("Index", "Home");
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register([Bind("Name, Email, Password, PasswordConfirm, Role")]
        UserRegisterViewModel m)
        {
            if (!ModelState.IsValid)
            {
                return View(m);
            }

            var user = _svc.Register(m.Name, m.Email, m.Password, m.Role);

            // check if email is unique
            if (user == null)
            {
                ModelState.AddModelError("Email", "Email Address has already been used. Choose another");
                return View(m);
            }
            // registration successful now redirect to login page
            Alert("Registration Successful - Now Login", AlertType.Info);

            return RedirectToAction(nameof(Login));
        }

        [Authorize]
        public IActionResult UpdateProfile()
        {
            // use BaseClass helper method to retrieve ID of signed in User
            var user = _svc.GetUser(GetSignedInUserId());
            var userViewModel = new UserProfileViewModel
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role
            };

            return View(userViewModel);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile([Bind("Id,Name,Email,Role")] UserProfileViewModel m)
        {
            var user = _svc.GetUser(m.Id);

            // check if form is invalid and redisplay
            if (!ModelState.IsValid || user == null)
            {
                return View(m);
            }

            // update user details and call service
            user.Name = m.Name;
            user.Email = m.Email;
            user.Role = m.Role;
            var updated = _svc.UpdateUser(user);

            // check if error updating service
            if (updated == null)
            {
                Alert("There was a problem updating. Please try again", AlertType.Warning);
                return View(m);
            }

            Alert("Successfully Updated Account Details", AlertType.Info);

            // sign the user in with updated details
            await SignInCookie(user);

            return RedirectToAction("Index", "Home");
        }

        // Change password
        [Authorize]
        public IActionResult UpdatePassword()
        {
            // use BaseClass helper method to retrieve Id of signed in user
            var user = _svc.GetUser(GetSignedInUserId());
            var passwordViewModel = new UserPasswordViewModel
            { 
                Id = user.Id,
                Password = user.Password,
                PasswordConfirm = user.Password
            };

            return View(passwordViewModel);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePassword([Bind("Id,OldPassword,Password,PasswordConfirm")] UserPasswordViewModel m)
        {
            var user = _svc.GetUser(m.Id);
            if(!ModelState.IsValid || user == null)
            {
                return View(m);
            }

            // update the password
            user.Password = m.Password;

            // save changes
            var updated = _svc.UpdateUser(user);
            if(updated == null)
            {
                Alert("There was a problem updating the password. Please try again.", AlertType.Warning);
                return View(m);
            }

            Alert("Successfully updated password", AlertType.Info);

            // sign the user in with updated details
            await SignInCookie(user);

            return RedirectToAction("Index", "Home");
        }


        // Sign user in using Cookie authentication scheme
        private async Task SignInCookie(User user)
        {
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                AuthBuilder.BuildClaimsPrincipal(user)
                );
        }

        // Called by Remote Validation attribute on RegisterViewModel to verify email address is unique
        [AcceptVerbs("GET", "POST")]
        public IActionResult GetUserByEmailAddress(string email)
        {
            // use BaseClass helper method to retrieve Id of signed in user
            var id = GetSignedInUserId();

            // check if email is available, unless already owned by user with id
            var user = _svc.GetUserByEmail(email);

            if (user != null)
            {
                return Json($"A user with this email address {email} already exists");
            }

            return Json(true);
        }

        // Called by Remote Validation attribute on ChangePassword to verify old password
        [AcceptVerbs("GET", "POST")]
        public IActionResult VerifyPassword(string oldPassword)
        {
            // use BaseClass helper method to retrieve Id of signed in user
            var id = GetSignedInUserId();

            // check if email is available, unless already owned by user with id
            var user = _svc.GetUser(id);
            if(user == null || !Hasher.ValidateHash(user.Password, oldPassword))
            {
                return Json($"Please enter current password.");
            }

            return Json(true);
        }



        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction(nameof(Login));
        }

        public IActionResult ErrorNotAuthorised()
        {
            Alert("Not Authorised", AlertType.Warning);
            return RedirectToAction("Index", "Home");
        }

        public IActionResult ErrorNotAuthenticated()
        {
            Alert("Not Authenticated", AlertType.Warning);
            return RedirectToAction("Login", "User");
        }

        
        [AcceptVerbs("GET", "POST")]
        public IActionResult VerifyEmailAddress(string email)
        {
            if (_svc.GetUserByEmail(email) != null)
            {
                return Json($"Email address {email} is already in use. Please choose another.");
            }

            return Json(true);
        }

    }
}
