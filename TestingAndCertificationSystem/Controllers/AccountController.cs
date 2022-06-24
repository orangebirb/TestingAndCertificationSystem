using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestingAndCertificationSystem.Models;
using TestingAndCertificationSystem.ViewModels;

namespace TestingAndCertificationSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<UserIdentity> _userManager;
        private readonly SignInManager<UserIdentity> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly DBContext _context;

        public AccountController(UserManager<UserIdentity> userManager, SignInManager<UserIdentity> signInManager, RoleManager<IdentityRole> roleManager, DBContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
        }

        #region Registration as user

        [HttpGet]
        public IActionResult RegistrationAsUser()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RegistrationAsUser(RegistrationViewModel model)
        {
            if (ModelState.IsValid)
            {
                UserIdentity user = new UserIdentity
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    City = model.City,
                    Phone = model.Phone,
                    Description = model.Description,
                    UserName = model.Email
                };

                // creation new user
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    //adding role
                    bool userRoleExists = await _roleManager.RoleExistsAsync("User");

                    if (!userRoleExists) //creating role if not exists
                    {
                        IdentityResult roleResult = await _roleManager.CreateAsync(new IdentityRole("User"));
                    }

                    //adding new user to role
                    var addingUserToRole = await _userManager.AddToRoleAsync(user, "User");

                    // setting cookie
                    await _signInManager.SignInAsync(user, false);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            return View(model);
        }

        #endregion

        #region Registration as company

        [HttpGet]
        public IActionResult RegistrationAsCompany()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RegistrationAsCompany(RegistrationAsCompanyViewModel model)
        {
            if (ModelState.IsValid)
            {
                Company newCompany = new Company
                {
                    FullName = model.company.FullName,
                    Description = model.company.Description,
                    WebsiteUrl = model.company.WebsiteUrl
                };

                _context.Company.Add(newCompany);

                //saving added company to db
                _context.SaveChanges();

                UserIdentity user = new UserIdentity
                {
                    FirstName = model.user.FirstName,
                    LastName = model.user.LastName,
                    Email = model.user.Email,
                    City = model.user.City,
                    Phone = model.user.Phone,
                    Description = model.user.Description,
                    UserName = model.user.Email,
                    CompanyId = newCompany.Id
                };

                // creating new user
                var result = await _userManager.CreateAsync(user, model.user.Password);

                if (result.Succeeded)
                {
                    //adding role
                    bool userRoleExists = await _roleManager.RoleExistsAsync("CompanyAdmin");

                    if (!userRoleExists) //creating role if not exists
                    {
                        IdentityResult roleResult = await _roleManager.CreateAsync(new IdentityRole("CompanyAdmin"));
                    }

                    //adding new user to role
                    var addingUserToRole = await _userManager.AddToRoleAsync(user, "CompanyAdmin");

                    // setting cookie
                    await _signInManager.SignInAsync(user, false);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            return View(model);
        }

        #endregion

        #region Login & Logout

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, true, false);

                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Wrong login or password");
                }
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            //removing cookie
            await _signInManager.SignOutAsync();

            return RedirectToAction("Index", "Home");
        }

        #endregion

        #region Edit user profile

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            UserIdentity currentUser = await _userManager.GetUserAsync(User);

            return View(currentUser);
        }

        [HttpPost]
        public async Task<IActionResult> EditProfile(UserIdentity user)
        {
            try
            {
                UserIdentity currentUser = await _userManager.GetUserAsync(User);

                currentUser.FirstName = user.FirstName;
                currentUser.LastName = user.LastName;
                currentUser.City = user.City;
                currentUser.Phone = user.Phone;
                currentUser.Description = user.Description;

                var result = await _userManager.UpdateAsync(currentUser);

                TempData["SuccessMessage"] = "User profile updated";
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Failed to update user profile";
            }

            return RedirectToAction("Profile");
        }

        #endregion
    }
}
