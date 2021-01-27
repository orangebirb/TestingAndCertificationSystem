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

        public AccountController(UserManager<UserIdentity> userManager, SignInManager<UserIdentity> signInManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
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
                TestingSystemDBContext ts = new TestingSystemDBContext();

                Company newCompany = new Company
                {
                    FullName = model.company.FullName,
                    ShortName = model.company.ShortName,
                    EstablishmentDate = model.company.EstablishmentDate,
                    Description = model.company.Description,
                    WebsiteUrl = model.company.WebsiteUrl
                };

                ts.Company.Add(newCompany);

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

                //saving added company to db
                ts.SaveChanges();

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
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);

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
    }
}
