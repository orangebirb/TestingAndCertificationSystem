using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TestingAndCertificationSystem.Models;
using TestingAndCertificationSystem.ViewModels;

namespace TestingAndCertificationSystem.Controllers
{
    public class TestsController : Controller
    {
        private readonly UserManager<UserIdentity> _userManager;
        private TestingSystemDBContext _context = new TestingSystemDBContext();

        public TestsController(UserManager<UserIdentity> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Tests()
        {

            UserIdentity currentUser = await _userManager.GetUserAsync(User);
            int adminCompanyId = currentUser.CompanyId; // current user's (admin's) company id
            
            if (User.IsInRole("CompanyAdmin"))
            {
                List<string> companyWorkersId = _userManager.Users.Where(x => x.CompanyId == adminCompanyId).Select(x => x.Id).ToList();

                //all tests where author is in company (admin and moderators)
                var testsAdmin = _context.Test.Where(x => companyWorkersId.Any(z => z == x.TestAuthorId)).ToList();

                return View(testsAdmin);
            }

            var tests = _context.Test.Where(x => x.TestAuthorId == User.FindFirstValue(ClaimTypes.NameIdentifier).ToString()).ToList();
            
            return View(tests);
        }

        [HttpGet]
        public IActionResult CreateTest()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateTest(CreateTestViewModel model)
        {
            if (ModelState.IsValid)
            {
                Test newTest = new Test()
                {
                    TestAuthorId = User.FindFirstValue(ClaimTypes.NameIdentifier).ToString(),
                    Name = model.Name,
                    Description = model.Description,
                    DurationInMinutes = model.DurationInMinutes,
                    Certificate = model.Certificate,
                    Instruction = model.Instruction,
                    IsPrivate = model.IsPrivate,
                    IsActive = false,
                    PassingMarkInPercents = model.PassingMarkInPercents
                };

                _context.Test.Add(newTest);

                await _context.SaveChangesAsync();

                ViewBag.successMessage = "Test created";
            }

            return View();
        }
    }
}
