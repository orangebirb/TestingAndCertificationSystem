using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
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

        #region test management

        [HttpGet]
        public async Task<IActionResult> Tests()
        {
            //list of all additional tasks
            List<AdditionalTask> listOfTasks = new List<AdditionalTask>();
            listOfTasks = _context.AdditionalTask.ToList();

            ViewBag.TasksList = listOfTasks;

            UserIdentity currentUser = await _userManager.GetUserAsync(User);
            int adminCompanyId = currentUser.CompanyId; // current user's (admin's) company id

            if (User.IsInRole("CompanyAdmin"))
            {
                //only workers in company
                List<string> companyWorkersId = _userManager.Users.Where(x => x.CompanyId == adminCompanyId).Select(x => x.Id).ToList();

                //tests where author is in company (admin and moderators)
                var testsAdmin = _context.Test.Where(x => companyWorkersId.Any(z => z == x.TestAuthorId)).ToList();

                return View(testsAdmin);
            }

            //only this.user tests (for moderators)
            var tests = _context.Test.Where(x => x.TestAuthorId == User.FindFirstValue(ClaimTypes.NameIdentifier));

            return View(tests.ToList());
        }

        [HttpGet]
        public IActionResult CreateTest()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateTest(TestViewModel model)
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

                //TempData["testId"] = newTest.Id;
                //TempData.Keep();

                return RedirectToAction("Tests");
                //return RedirectToAction("CreateAdditionalTask", newTest.Id);
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteTest(int testId)
        {
            Test testToDelete = _context.Test.Find(testId);

            //deletes additional task if exists
            if(testToDelete.AdditionalTaskId != null)
            {
                AdditionalTask additionalTask = _context.AdditionalTask.Find(testToDelete.AdditionalTaskId);
              
                if (additionalTask != null)
                {
                    _context.AdditionalTask.Remove(additionalTask);
                    await _context.SaveChangesAsync();
                }
            }

            if (testToDelete != null)
            {
                _context.Test.Remove(testToDelete);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Tests");
        }

        #endregion

        #region questions management

        [HttpGet]
        public IActionResult CreateQuestion()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateQuestion(QuestionDataViewModel model, int testId)
        {
            List<Choice> choices = new List<Choice>();

            if (ModelState.IsValid && testId != 0)
            {
                int totalCount = model.Choices.Count(x => x.Choice.Text != null); //amount of choices
                int trueAnswCount = model.Choices.Count(x => x.IsChecked == true && x.Choice.Text != null); //marked as true choices

                Question newQuestion = new Question()
                {
                    TestId = testId,
                    Text = model.Question.Text,
                    QuestionType = trueAnswCount == 1 ? "RADIO" : "CHECKBOX"
                };

                _context.Question.Add(newQuestion);
                await _context.SaveChangesAsync();

                foreach (var item in model.Choices)
                {
                    if (item.Choice.Text != null)
                    {
                        if (item.IsChecked == true)
                        {
                            item.Choice.Points = (double)1 / trueAnswCount;
                        }

                        Choice choice = new Choice()
                        {
                            QuestionId = newQuestion.Id,
                            Text = item.Choice.Text,
                            Points = item.Choice.Points
                        };

                        choices.Add(choice);
                    }
                }

                _context.Choice.AddRange(choices);

                await _context.SaveChangesAsync();
            }

            return View();
        }

        #endregion

        #region additional task management

        [HttpGet]
        public IActionResult CreateAdditionalTask(int testId)
        {
            TempData["testId"] = testId;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateAdditionalTask(AdditionalTaskViewModel model, int testId)
        {
            if(ModelState.IsValid)
            {
                UserIdentity currentUser = await _userManager.GetUserAsync(User);
                string userEmail = currentUser.Email;

                AdditionalTask newTask = new AdditionalTask()
                {
                    Name = model.Name,
                    Description = model.Description,
                    ExpirationDate = model.ExpirationDate,
                    RecipientEmail = userEmail,
                    Text = model.Text,
                };

                //adding task
                _context.AdditionalTask.Add(newTask);

                await _context.SaveChangesAsync();

                //finds a test and adds task data to it
                var test = _context.Test.Find(testId);

                test.AdditionalTaskId = newTask.Id;
                test.AdditionalTask = newTask;

                ViewBag.Name = test.AdditionalTask.Name;

                await _context.SaveChangesAsync();

                return RedirectToAction("Tests");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteTask(int taskId)
        {
            AdditionalTask taskToDelete = _context.AdditionalTask.Find(taskId);

            if (taskToDelete != null)
            {
                _context.AdditionalTask.Remove(taskToDelete);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Tests");
        }
        #endregion
    }
}
