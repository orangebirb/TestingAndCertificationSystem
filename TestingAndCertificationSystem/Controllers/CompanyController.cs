using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestingAndCertificationSystem.Models;
using TestingAndCertificationSystem.Resources;

namespace TestingAndCertificationSystem.Controllers
{
    [Authorize]
    [Authorize(Roles = Roles.CompanyAdmin)]
    public class CompanyController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<UserIdentity> _userManager;
        private TestingSystemDBContext _context = new TestingSystemDBContext();

        public CompanyController(UserManager<UserIdentity> userManager, RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task<IActionResult> Moderators(SortingOrders sortOrder, int page = 1)
        {
            int pageSize = 10;

            ViewData["CurrentSort"] = sortOrder;

            ViewData["NameSortParm"] = sortOrder == SortingOrders.NameAsc ? SortingOrders.NameDesc : SortingOrders.NameAsc;
            ViewData["CitySortParm"] = sortOrder == SortingOrders.CityAsc ? SortingOrders.CityDesc : SortingOrders.CityAsc;

            UserIdentity currentUser = await _userManager.GetUserAsync(User);
            int adminCompanyId = currentUser.CompanyId; // current user's (admin's) company id

            var moderators = _userManager.GetUsersInRoleAsync(Roles.CompanyModerator).Result.Where(x => x.CompanyId == adminCompanyId);

            moderators = sortOrder switch
            {
                SortingOrders.CityAsc => moderators.OrderBy(x => x.City),
                SortingOrders.CityDesc => moderators.OrderByDescending(x => x.City),
                SortingOrders.NameAsc => moderators.OrderBy(x => x.FirstName),
                SortingOrders.NameDesc => moderators.OrderByDescending(x => x.FirstName),
                _ => moderators
            };

            var count = moderators.Count();
            var items = moderators.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            Pagination pagination = new Pagination(count, page, pageSize);
            PaginationGeneric<UserIdentity> paginationModerators = new PaginationGeneric<UserIdentity>
            {
                pagination = pagination,
                source = items
            };

            ViewBag.Page = page;
            ViewBag.PageCount = (count + pageSize - 1) / pageSize;

            return View(paginationModerators);
        }

        [HttpGet]
        public IActionResult SearchModerators(string userSearch, SortingOrders sortOrder, int page = 1)
        {
            int pageSize = 10;

            ViewData["CurrentSort"] = sortOrder;

            ViewData["NameSortParm"] = sortOrder == SortingOrders.NameAsc ? SortingOrders.NameDesc : SortingOrders.NameAsc;
            ViewData["CitySortParm"] = sortOrder == SortingOrders.CityAsc ? SortingOrders.CityDesc : SortingOrders.CityAsc;

            IEnumerable<UserIdentity> searchResult;

            if (string.IsNullOrEmpty(userSearch) || string.IsNullOrWhiteSpace(userSearch))
            {
                searchResult = _userManager.GetUsersInRoleAsync(Roles.User).Result.ToList();
            }
            else
            {
                searchResult = _userManager.GetUsersInRoleAsync(Roles.User).Result.Where(
                    x => x.Email.ToLower().Contains(userSearch.ToLower()) || 
                    x.FirstName.ToLower().Contains(userSearch.ToLower()) || 
                    x.LastName.ToLower().Contains(userSearch.ToLower())).ToList();
            }

            ViewData["userDetails"] = userSearch;

            searchResult = sortOrder switch
            {
                SortingOrders.CityAsc => searchResult.OrderBy(x => x.City),
                SortingOrders.CityDesc => searchResult.OrderByDescending(x => x.City),
                SortingOrders.NameAsc => searchResult.OrderBy(x => x.FirstName),
                SortingOrders.NameDesc => searchResult.OrderByDescending(x => x.FirstName),
                _ => searchResult
            };

            var count = searchResult.Count();
            var items = searchResult.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            Pagination pagination = new Pagination(count, page, pageSize);
            PaginationGeneric<UserIdentity> paginationSearchResult = new PaginationGeneric<UserIdentity>
            {
                pagination = pagination,
                source = items
            };

            ViewBag.Page = page;
            ViewBag.PageCount = (count + pageSize - 1) / pageSize;

            return View(paginationSearchResult);
        }

        public async Task<IActionResult> AddModerator(string id)
        {
            UserIdentity user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            UserIdentity currentUser = await _userManager.GetUserAsync(User);
            int userCompanyId = currentUser.CompanyId; // current user's (admin's) company id

            user.CompanyId = userCompanyId;

            var UserResult = await _userManager.UpdateAsync(user);

            IdentityResult roleRemoveResult = await _userManager.RemoveFromRoleAsync(user, "User");

            //adding role
            bool userRoleExists = await _roleManager.RoleExistsAsync("CompanyModerator");

            if (!userRoleExists) //creating role if not exists
            {
                IdentityResult roleAddResult = await _roleManager.CreateAsync(new IdentityRole("CompanyModerator"));
            }

            //adding new user to role
            var addingUserToRole = await _userManager.AddToRoleAsync(user, "CompanyModerator");

            if (UserResult.Succeeded)
            {
                return RedirectToAction("Moderators");
            }
            else
            {
                foreach (var error in UserResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View();
        }

        public async Task<IActionResult> RemoveModerator(string id)
        {
            UserIdentity user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                ModelState.AddModelError("", "User can't be found");
            }
            else
            {
                UserIdentity currentUser = await _userManager.GetUserAsync(User);
                //int userCompanyId = currentUser.CompanyId; // current user's (admin's) company id

                user.CompanyId = 0;

                var UserResult = await _userManager.UpdateAsync(user);

                await _userManager.RemoveFromRoleAsync(user, "CompanyModerator");

                //adding role
                bool userRoleExists = await _roleManager.RoleExistsAsync("User");

                if (!userRoleExists) //creating role if not exists
                {
                    await _roleManager.CreateAsync(new IdentityRole("User"));
                }

                //adding new user to role
                await _userManager.AddToRoleAsync(user, "User");

                if (UserResult.Succeeded)
                {
                    return RedirectToAction("Moderators");
                }
                else
                {
                    foreach (var error in UserResult.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            UserIdentity currentUser = await _userManager.GetUserAsync(User);
            int adminCompanyId = currentUser.CompanyId; // current user's (admin's) company id

            Company company = _context.Company.Find(adminCompanyId);

            return View(company);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Company company)
        {
            try
            {
                Company companyToEdit = _context.Company.Find(company.Id);

                _context.Entry(companyToEdit).CurrentValues.SetValues(company);

                _context.Company.Update(companyToEdit);
                await _context.SaveChangesAsync();

                ViewBag.successMessage = "Changes saved";
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            return View();
        }
    }
}
