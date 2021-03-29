using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using TestingAndCertificationSystem.Models;
using TestingAndCertificationSystem.ViewModels;
using MimeKit;
using TestingAndCertificationSystem.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Syncfusion.HtmlConverter;
using Syncfusion.Pdf;

namespace TestingAndCertificationSystem.Controllers
{
    [Authorize]
    public class TestsController : Controller
    {
        private readonly UserManager<UserIdentity> _userManager;
        private readonly TestingSystemDBContext _context = new TestingSystemDBContext();

        public TestsController(UserManager<UserIdentity> userManager)
        {
            _userManager = userManager;
        }

        #region test management

        [Authorize(Roles = Roles.CompanyAdmin + ", " + Roles.CompanyModerator)]
        public async Task<IActionResult> Tests(SortingOrders sortOrder, int page = 1)
        {
            int pageSize = 10;

            ViewData["CurrentSort"] = sortOrder;
            ViewData["DurationSortParm"] = sortOrder == SortingOrders.DurationAsc ? SortingOrders.DurationDesc : SortingOrders.DurationAsc;
            ViewData["NameSortParm"] = sortOrder == SortingOrders.NameAsc ? SortingOrders.NameDesc : SortingOrders.NameAsc;
            ViewData["IsActiveSortParm"] = sortOrder == SortingOrders.IsActiveOnly ? SortingOrders.IsNotActiveOnly : SortingOrders.IsActiveOnly;

            //list of all additional tasks
            List<AdditionalTask> listOfTasks = new List<AdditionalTask>();
            listOfTasks = _context.AdditionalTask.ToList();

            IQueryable<Test> tests;

            UserIdentity currentUser = await _userManager.GetUserAsync(User);
            int adminCompanyId = currentUser.CompanyId; // current user's (admin's) company id

            if (User.IsInRole(Roles.CompanyAdmin))
            {
                //only workers in company
                List<string> companyWorkersId = _userManager.Users.Where(x => x.CompanyId == adminCompanyId).Select(x => x.Id).ToList();

                //tests where author is in company (admin and moderators)
                tests = _context.Test.Where(x => companyWorkersId.Any(z => z == x.TestAuthorId));
            }
            else
            {
                //only this.user tests (for moderators)
                tests = _context.Test.Where(x => x.TestAuthorId == User.FindFirstValue(ClaimTypes.NameIdentifier));
            }

            tests = sortOrder switch
            {
                SortingOrders.DurationAsc => tests.OrderBy(x => x.DurationInMinutes),
                SortingOrders.DurationDesc => tests.OrderByDescending(x => x.DurationInMinutes),
                SortingOrders.NameAsc => tests.OrderBy(x => x.Name),
                SortingOrders.NameDesc => tests.OrderByDescending(x => x.Name),
                SortingOrders.IsActiveOnly => tests.Where(x => x.IsActive == true),
                SortingOrders.IsNotActiveOnly => tests.Where(x => x.IsActive == false),
                _ => tests
            };

            var count = await tests.CountAsync();
            var items = await tests.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            Pagination pagination = new Pagination(count, page, pageSize);
            PaginationGeneric<Test> paginationTests = new PaginationGeneric<Test>
            {
                pagination = pagination,
                source = items
            };

            ViewBag.TasksList = listOfTasks;
            ViewBag.Page = page;
            ViewBag.PageCount = (count + pageSize - 1) / pageSize;

            return View(paginationTests);
        }


        [Authorize(Roles = Roles.CompanyAdmin + ", " + Roles.CompanyModerator)]
        public IActionResult CreateTest()
        {
            return View();
        }


        [Authorize(Roles = Roles.CompanyAdmin + ", " + Roles.CompanyModerator)]
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

                return RedirectToAction("Tests");
            }

            return View();
        }


        [Authorize(Roles = Roles.CompanyAdmin + ", " + Roles.CompanyModerator)]
        public IActionResult EditTest(int testId)
        {
            Test testToEdit = _context.Test.Find(testId);

            return View(testToEdit);
        }


        [Authorize(Roles = Roles.CompanyAdmin + ", " + Roles.CompanyModerator)]
        [HttpPost]
        public async Task<IActionResult> EditTest(Test test)
        {
            try
            {
                Test testToEdit = _context.Test.Find(test.Id);

                _context.Entry(testToEdit).CurrentValues.SetValues(test);

                _context.Test.Update(testToEdit);
                await _context.SaveChangesAsync();

                ViewBag.successMessage = "Changes saved";
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }

            return View();
        }


        [Authorize(Roles = Roles.CompanyAdmin + ", " + Roles.CompanyModerator)]
        [HttpPost]
        public async Task<IActionResult> DeleteTest(int testId)
        {
            Test testToDelete = _context.Test.Find(testId);

            //deletes additional task if exists
            if (testToDelete.AdditionalTaskId != null)
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


        [Authorize(Roles = Roles.CompanyAdmin + ", " + Roles.CompanyModerator)]
        [HttpPost]
        public async Task<IActionResult> ChangeActivityStatusTest(int testId, DateTime endingDateTime)
        {
            Test testToEdit = _context.Test.Find(testId);

            if (testToEdit != null)
            {
                if (_context.Question.Where(x => x.TestId == testToEdit.Id).Count() == 0)
                {
                    TempData["ErrorMessage"] = "You can't activate test without any questions";
                }
                else
                {
                    if (testToEdit.IsActive == true)
                    {
                        testToEdit.IsActive = false;
                    }
                    else
                    {
                        if (endingDateTime > DateTime.Now)
                        {
                            testToEdit.TokenEndTime = endingDateTime;
                            testToEdit.TokenStartTime = DateTime.Now;

                            testToEdit.IsActive = true;
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Choose correct date";
                        }
                    }

                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction("TestInfopage", new { testId });
        }


        [Authorize(Roles = Roles.CompanyAdmin + ", " + Roles.CompanyModerator)]
        public IActionResult TestInfopage(int testId)
        {
            Test test = _context.Test.Find(testId);

            if (test != null)
            {
                if (test.AdditionalTaskId != 0)
                {
                    test.AdditionalTask = _context.AdditionalTask.Find(test.AdditionalTaskId);
                }

                test.Question = _context.Question.Where(x => x.TestId == test.Id).ToList();

                foreach (var question in test.Question)
                {
                    question.Choice = _context.Choice.Where(x => x.QuestionId == question.Id).ToList();
                }

                ViewBag.VerifiedUsersCount = _context.VerifiedUsers.Where(x => x.TestId == test.Id).Count();

                return View(test);
            }

            return RedirectToAction("Error");
        }


        [AllowAnonymous]
        public async Task<IActionResult> Instruction(int testId)
        {
            Test test = _context.Test.Find(testId);

            if (test != null)
            {
                UserIdentity testAuthor = await _userManager.FindByIdAsync(test.TestAuthorId);
                var company = _context.Company.Where(x => x.Id == testAuthor.CompanyId).FirstOrDefault();

                ViewBag.TestAuthor = testAuthor;
                ViewBag.Company = company;

                ViewBag.QuestionCount = _context.Question.Where(x => x.TestId == test.Id).Count();

                if(test.IsPrivate == true)
                {
                    ViewBag.UserHaveAccess = (_context.VerifiedUsers.Where(x => x.UserEmail == User.Identity.Name).Count() != 0) ? true : false;
                }

                // --- temp script for deactivating test ---
                if (DateTime.Now >= test.TokenEndTime)
                {
                    test.IsActive = false;
                    _context.SaveChanges();
                }

                return View(test);
            }
            return RedirectToAction("Error");
        }

        #endregion

        #region questions management

        [Authorize(Roles = Roles.CompanyAdmin + ", " + Roles.CompanyModerator)]
        public IActionResult CreateQuestion()
        {
            return View();
        }

        [Authorize(Roles = Roles.CompanyAdmin + ", " + Roles.CompanyModerator)]
        [HttpPost]
        public async Task<IActionResult> CreateQuestion(QuestionDataModel model, int testId)
        {
            List<Choice> choices = new List<Choice>();

            if (ModelState.IsValid && testId != 0)
            {
                //if question text = null || not created any options || text in all this options = null
                if(model.Question.Text == null || model.Choices == null || model.Choices.Any(x => x.Choice.Text == null))
                {
                    ViewBag.ErrorMessage = "You can't add a question without text or any options";
                    return View();
                }

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

        [Authorize(Roles = Roles.CompanyAdmin + ", " + Roles.CompanyModerator)]
        [HttpPost]
        public async Task<IActionResult> DeleteQuestion(int questionId, int testId)
        {
            Question questionToDelete = _context.Question.Find(questionId);

            if (questionToDelete != null)
            {
                var choices = _context.Choice.Where(x => x.QuestionId == questionToDelete.Id); //deleting choices of this answer

                // _context.Choice.RemoveRange(choices);
                _context.Question.Remove(questionToDelete);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("TestInfopage", new { testId });
        }

        #endregion

        #region additional task management

        [Authorize(Roles = Roles.CompanyAdmin + ", " + Roles.CompanyModerator)]
        public IActionResult CreateAdditionalTask(int testId)
        {
            return View();
        }

        [Authorize(Roles = Roles.CompanyAdmin + ", " + Roles.CompanyModerator)]
        [HttpPost]
        public async Task<IActionResult> CreateAdditionalTask(AdditionalTaskViewModel model, int testId)
        {
            if (ModelState.IsValid)
            {
                UserIdentity currentUser = await _userManager.GetUserAsync(User);
                string userEmail = currentUser.Email;

                AdditionalTask newTask = new AdditionalTask()
                {
                    Name = model.Name,
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

                return RedirectToAction("TestInfopage", new { testId });
            }

            return View();
        }

        [Authorize(Roles = Roles.CompanyAdmin + ", " + Roles.CompanyModerator)]
        public async Task<IActionResult> EditAdditionalTask(int taskId)
        {
            AdditionalTask taskToEdit = await _context.AdditionalTask.FindAsync(taskId);

            TempData["testId"] = _context.Test.Where(x => x.AdditionalTaskId == taskToEdit.Id).FirstOrDefault().Id;

            return View(taskToEdit);
        }

        [Authorize(Roles = Roles.CompanyAdmin + ", " + Roles.CompanyModerator)]
        [HttpPost]
        public async Task<IActionResult> EditAdditionalTask(AdditionalTask additionalTask)
        {
            AdditionalTask taskToEdit = _context.AdditionalTask.Find(additionalTask.Id);

            _context.Entry(taskToEdit).CurrentValues.SetValues(additionalTask);

            _context.AdditionalTask.Update(taskToEdit);

            await _context.SaveChangesAsync();

            ViewBag.successMessage = "Changes saved";

            TempData["testId"] = _context.Test.Where(x => x.AdditionalTaskId == taskToEdit.Id).FirstOrDefault().Id;
            return View();
        }

        [Authorize(Roles = Roles.CompanyAdmin + ", " + Roles.CompanyModerator)]
        [HttpPost]
        public async Task<IActionResult> DeleteAdditionalTask(int taskId, int testId)
        {
            AdditionalTask taskToDelete = _context.AdditionalTask.Find(taskId);

            //gets test with current task

            if (taskToDelete != null)
            {
                _context.AdditionalTask.Remove(taskToDelete);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("TestInfopage", new { testId });
        }
        #endregion

        #region user test management

        [Authorize(Roles = Roles.User + ", " + Roles.CompanyModerator)]
        [HttpPost]
        public async Task<IActionResult> Registration(int testId)
        {
            if (testId != 0)
            {
                UserIdentity currentUser = await _userManager.GetUserAsync(User);

                var currentTest = _context.Test.Find(testId);

                if (currentTest != null)
                {
                    Registration registration = _context.Registration.Where(x => x.UserId == currentUser.Id
                        && x.TestId == testId
                        && x.EndingTime > DateTime.Now).FirstOrDefault();

                    if (registration != null)
                    {
                        TestResults results = _context.TestResults.Where(x => x.RegistrationId == registration.Id).FirstOrDefault();

                        //registrated & not submited = current registration
                        if (results == null)
                        {
                            return RedirectToAction("Test", new { token = registration.Token, qNum = 1 });
                        }

                        //registrated & submited = new registration

                        Registration newRegistration = new Registration()
                        {
                            UserId = currentUser.Id,
                            TestId = testId,
                            Token = Guid.NewGuid(),
                            EntryTime = DateTime.Now,
                            EndingTime = DateTime.Now.AddMinutes(currentTest.DurationInMinutes)
                        };

                        _context.Registration.Add(newRegistration);

                        await _context.SaveChangesAsync();

                        return RedirectToAction("Test", new { token = newRegistration.Token, qNum = 1 });
                    }
                    else //not registrated
                    {
                        Registration newRegistration = new Registration()
                        {
                            UserId = currentUser.Id,
                            TestId = testId,
                            Token = Guid.NewGuid(),
                            EntryTime = DateTime.Now,
                            EndingTime = DateTime.Now.AddMinutes(currentTest.DurationInMinutes)
                        };

                        _context.Registration.Add(newRegistration);

                        await _context.SaveChangesAsync();

                        return RedirectToAction("Test", new { token = newRegistration.Token, qNum = 1 });
                    }

                }
            }

            return View("Error");
        }



        [Authorize(Roles = Roles.User + ", " + Roles.CompanyModerator)]
        [HttpPost]
        public async Task<IActionResult> SubmitAnswer(QuestionDataModel model, Guid token, int qNum)
        {
            if (model != null)
            {
                if (model.Choices.All(x => x.IsChecked == false))
                {
                    //if submit without any answer
                    TempData["ErrorMessage"] = "You did not choose an answer";
                    return RedirectToAction("Test", new { token = token, qNum = qNum });
                }

                TestResults testResults;
                var registration = _context.Registration.Where(x => x.Token == token).FirstOrDefault();

                //if test result context is not created
                if (_context.TestResults.Where(x => x.RegistrationId == registration.Id).FirstOrDefault() == null)
                {
                    //creating new
                    testResults = new TestResults()
                    {
                        RegistrationId = _context.Registration.Where(x => x.Token == token).FirstOrDefault().Id
                    };

                    _context.TestResults.Add(testResults);

                    await _context.SaveChangesAsync();
                }
                else
                {
                    //searching test result
                    testResults = _context.TestResults.Where(x => x.RegistrationId == _context.Registration.Where(x => x.Token == token).FirstOrDefault().Id).FirstOrDefault();
                }

                var questionAnswer = _context.QuestionAnswer.Where(x => x.RegistrationId == registration.Id && x.QuestionId == model.Question.Id).FirstOrDefault();

                //if not answered already
                if (questionAnswer == null)
                {
                    var userAnswers = model.Choices.Where(x => x.IsChecked == true).ToList();

                    if (userAnswers.Count != 0)
                    {
                        List<QuestionAnswer> listQA = new List<QuestionAnswer>();

                        userAnswers.ForEach(x => x.Choice.Points = _context.Choice.Where(y => y.Id == x.Choice.Id).FirstOrDefault().Points); //sets mark

                        float mark = (float)userAnswers.Sum(x => x.Choice.Points); //calculates total mark for all correct answers

                        foreach (var choice in userAnswers)
                        {
                            QuestionAnswer qa = new QuestionAnswer();

                            qa.RegistrationId = _context.Registration.Where(x => x.Token == token).FirstOrDefault().Id;
                            qa.QuestionId = model.Question.Id;
                            qa.ChoiceId = choice.Choice.Id;
                            qa.TotalMark = mark;
                            qa.TestResultId = testResults.Id;

                            listQA.Add(qa);
                        }

                        _context.QuestionAnswer.AddRange(listQA);

                        await _context.SaveChangesAsync();
                    }
                }

                if (qNum == 0)
                    qNum = 1;

                //if question is last
                if (_context.Question.Where(x => x.TestId == registration.TestId).Count() <= qNum)
                {
                    return RedirectToAction("TestResults", new { token = token }); //redirect to results test
                }

                qNum++;

                return RedirectToAction("Test", new { token = token, qNum = qNum });
            }
            return View("Error");
        }



        [Authorize(Roles = Roles.User + ", " + Roles.CompanyModerator)]
        public IActionResult Test(Guid token, int qNum)
        {
            if (token != null)
            {
                var registration = _context.Registration.Where(x => x.Token == token).FirstOrDefault();

                ViewBag.QuestionCount = _context.Question.Where(x => x.TestId == registration.TestId).Count();

                var testAuthor = _context.Test.Where(x => x.Id == registration.TestId).FirstOrDefault().TestAuthorId;
                ViewBag.TestAuthorEmail = _userManager.Users.Where(x => x.Id == testAuthor).FirstOrDefault().Email;

                if (registration != null)
                {

                    ViewBag.Token = registration.Token;
                    ViewBag.EntryTime = registration.EntryTime;
                    ViewBag.EndTime = registration.EndingTime;

                    var test = _context.Test.Where(x => x.Id == registration.TestId).FirstOrDefault();

                    if (qNum < 1)
                        qNum = 1;

                    if (test != null)
                    {
                        QuestionDataModel question = new QuestionDataModel();

                        question.Question = _context.Question.Where(x => x.TestId == test.Id).ToList()[qNum - 1];
                        question.Choices = new List<ChoiceModel>();

                        foreach (var choice in _context.Choice.Where(x => x.QuestionId == question.Question.Id).ToList())
                        {
                            question.Choices.Add(new ChoiceModel { Choice = choice });
                        }

                        return View(question);
                    }
                }
            }
            return View("Error");
        }



        [Authorize(Roles = Roles.User + ", " + Roles.CompanyModerator)]
        public async Task<IActionResult> TestResults(Guid token)
        {
            var registration = _context.Registration.Where(x => x.Token == token).FirstOrDefault();

            if (registration != null)
            {
                var test = _context.Test.Where(x => x.Id == registration.TestId).FirstOrDefault();
                var totalQuestionCount = _context.Question.Where(x => x.TestId == test.Id).Count();

                var testResult = _context.TestResults.Where(x => x.RegistrationId == registration.Id).FirstOrDefault();

                IEnumerable<QuestionAnswer> userResults = _context.QuestionAnswer.Where(x => x.TestResultId == testResult.Id); //user's answers

                //user`s scored mark in percents

                var userMark = (int)(userResults.GroupBy(x => x.QuestionId).Select(y => y.First()).Sum(x => x.TotalMark) / (totalQuestionCount) * 100);

                testResult.FinalMarkInPercents = userMark;

                if (userMark >= test.PassingMarkInPercents)
                {
                    testResult.IsPassed = true;

                    if (test.AdditionalTaskId != null)
                    {
                        var additionalTask = _context.AdditionalTask.Where(x => x.Id == test.AdditionalTaskId).FirstOrDefault();

                        UserIdentity currentUser = await _userManager.GetUserAsync(User);

                        SendAdditionalTask(additionalTask, test.Name, currentUser);
                    }
                }
                else
                    testResult.IsPassed = false;

                await _context.SaveChangesAsync();

                ViewBag.RequiredMinMark = test.PassingMarkInPercents;
                ViewBag.q = userMark;

                return View(testResult);
            }

            return View("Error");
        }


        private void SendAdditionalTask(AdditionalTask additionalTask, string testName, UserIdentity user)
        {
            string path = @"D:\Diploma\smtp data.txt";

            try
            {
                StreamReader reader = new StreamReader(path);

                string smtpLogin = reader.ReadLine();
                string smtpPassword = reader.ReadLine();

                reader.Close();

                MimeMessage msg = new MimeMessage();
                msg.From.Add(new MailboxAddress("Testing and Certification system", smtpLogin));
                msg.To.Add(new MailboxAddress(user.FirstName + ' ' + user.LastName, user.Email));
                msg.Subject = "Additional task to test \"" + testName + "\"";
                msg.Body = new BodyBuilder() { HtmlBody = 

                    "<h3><b>" + additionalTask.Name + "</b></h3>" +
                    "<br/>" + additionalTask.Text +
                    "<br/><br/><b>Expiration date: </b>" + additionalTask.ExpirationDate +
                    "<br/><b>After completing the task</b>, send a reply to the " + additionalTask.RecipientEmail + " mail"

                }.ToMessageBody();

                using (MailKit.Net.Smtp.SmtpClient client = new MailKit.Net.Smtp.SmtpClient())
                {
                    client.Connect("smtp.gmail.com", 465, true);
                    client.Authenticate(smtpLogin, smtpPassword);
                    client.Send(msg);

                    client.Disconnect(true);
                }
            }
            catch(Exception)
            {
                TempData["ErrorMessage"] = "Smtp error: Failed to send additional task";
            }
        }

        #endregion

        #region assignment results

        [Authorize(Roles = Roles.CompanyAdmin + ", " + Roles.CompanyModerator)]
        //attempts of a chosen test (only for admin or moderator)
        public async Task<IActionResult> TestAttempts(int testId, SortingOrders sortOrder, int page = 1)
        {
            int pageSize = 8;

            ViewData["CurrentSort"] = sortOrder;

            ViewData["DateSortParm"] = sortOrder == SortingOrders.DateAsc ? SortingOrders.DateDesc : SortingOrders.DateAsc;
            ViewData["MarkSortParm"] = sortOrder == SortingOrders.MarkAsc ? SortingOrders.MarkDesc : SortingOrders.MarkAsc;
            ViewData["PassSortParm"] = sortOrder == SortingOrders.PassedOnly ? SortingOrders.FailedOnly : SortingOrders.PassedOnly;

            //all attempts of selected test
            IQueryable<Registration> testAttempts = _context.Registration.Where(x => x.TestId == testId).OrderBy(x => x.Id);

            //additional data for displaying in view
            var users = _userManager.Users.Where(x => testAttempts.ToList().Select(x => x.UserId).Any(z => z == x.Id)).ToList();
            var results = _context.TestResults.Where(x => testAttempts.ToList().Select(x => x.Id).Any(z => z == x.RegistrationId)).ToList();

            testAttempts = sortOrder switch
            {
                SortingOrders.DateAsc => testAttempts.OrderBy(x => x.EntryTime),
                SortingOrders.DateDesc => testAttempts.OrderByDescending(x => x.EntryTime),
                SortingOrders.MarkDesc => testAttempts.OrderByDescending(x => _context.TestResults.Where(y => y.RegistrationId == x.Id).FirstOrDefault().FinalMarkInPercents),
                SortingOrders.MarkAsc => testAttempts.OrderBy(x => _context.TestResults.Where(y => y.RegistrationId == x.Id).FirstOrDefault().FinalMarkInPercents),
                SortingOrders.PassedOnly => testAttempts.Where(x => _context.TestResults.Where(y => y.RegistrationId == x.Id).FirstOrDefault().IsPassed == true),
                SortingOrders.FailedOnly => testAttempts.Where(x => _context.TestResults.Where(y => y.RegistrationId == x.Id).FirstOrDefault().IsPassed == false),
                _ => testAttempts
            };

            ViewBag.Users = users;
            ViewBag.Results = results;

            var count = await testAttempts.CountAsync();
            var items = await testAttempts.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            Pagination pagination = new Pagination(count, page, pageSize);
            PaginationGeneric<Registration> paginationTestAttempts = new PaginationGeneric<Registration>
            {
                pagination = pagination,
                source = items
            };

            ViewBag.TestName = _context.Test.Where(x => x.Id == testId).FirstOrDefault().Name;
            ViewBag.Page = page;
            ViewBag.PageCount = (count + pageSize - 1) / pageSize;

            return View(paginationTestAttempts);
        }


        [Authorize(Roles = Roles.CompanyAdmin + ", " + Roles.CompanyModerator)]
        //attempt details (only for admin or moderator)
        public IActionResult AttemptInfopage(int registrationId)
        {
            Registration registration = _context.Registration.Find(registrationId);

            TestResults testResults = _context.TestResults.Where(x => x.RegistrationId == registration.Id).FirstOrDefault();

            UserIdentity user = _userManager.Users.Where(x => x.Id == registration.UserId).FirstOrDefault();

            var questionAnswers = _context.QuestionAnswer.Where(x => x.RegistrationId == registration.Id).ToList();

            if (questionAnswers.Where(x => x.QuestionId == null).Count() != 0)
            {
                ViewBag.DeletedQuestionsMessage = "Some questions in this test have been removed, but they still have an effect on the test results";
            }

            //questions in original test
            var testQuestions = _context.Question.Where(x => x.TestId == registration.TestId).ToList();

            if (testQuestions != null)
            {
                foreach (var question in testQuestions)
                {
                    question.Choice = _context.Choice.Where(x => x.QuestionId == question.Id).ToList();
                }
            }

            ViewBag.questions = testQuestions;
            ViewBag.regStart = registration.EntryTime;
            ViewBag.regEnd = registration.EndingTime;
            ViewBag.results = testResults;
            ViewBag.user = user;
            ViewBag.requiredMark = _context.Test.Where(x => x.Id == registration.TestId).FirstOrDefault().PassingMarkInPercents;
            ViewBag.pointsMark = questionAnswers.GroupBy(x => x.QuestionId).Select(y => y.First()).Sum(x => x.TotalMark);
            ViewBag.pointsTotal = questionAnswers.GroupBy(x => x.QuestionId).Select(y => y.First()).Count();
            ViewBag.testId = registration.TestId;

            return View(questionAnswers);
        }


        [Authorize(Roles = Roles.User + ", " + Roles.CompanyModerator)]
        //user can only see their test attempts
        public async Task<IActionResult> UserAttempts(SortingOrders sortOrder, int page = 1)
        {
            int pageSize = 8;

            ViewData["CurrentSort"] = sortOrder;
            ViewData["MarkSortParm"] = sortOrder == SortingOrders.MarkAsc ? SortingOrders.MarkDesc : SortingOrders.MarkAsc;
            ViewData["DateSortParm"] = sortOrder == SortingOrders.DateAsc ? SortingOrders.DateDesc : SortingOrders.DateAsc;
            ViewData["PassSortParm"] = sortOrder == SortingOrders.PassedOnly ? SortingOrders.FailedOnly : SortingOrders.PassedOnly;

            UserIdentity currentUser = await _userManager.GetUserAsync(User);

            var userRegistrations = _context.Registration.Where(x => x.UserId == currentUser.Id).ToList();

            IQueryable<TestResults> userTestResults = _context.TestResults.Where(x => userRegistrations.Select(x => x.Id).Any(y => y == x.RegistrationId));

            var testsInfo = _context.Test.Where(x => userRegistrations.Select(x => x.TestId).Any(y => y == x.Id)).ToList();

            userTestResults = sortOrder switch
            {
                SortingOrders.MarkAsc => userTestResults.OrderBy(x => x.FinalMarkInPercents),
                SortingOrders.MarkDesc => userTestResults.OrderByDescending(x => x.FinalMarkInPercents),
                SortingOrders.DateAsc => userTestResults.OrderBy(x => x.RegistrationId),
                SortingOrders.DateDesc => userTestResults.OrderByDescending(x => x.RegistrationId),
                SortingOrders.PassedOnly => userTestResults.Where(x => x.IsPassed == true),
                SortingOrders.FailedOnly => userTestResults.Where(x => x.IsPassed == false),
                _ => userTestResults
            };

            var count = await userTestResults.CountAsync();
            var items = await userTestResults.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            Pagination pagination = new Pagination(count, page, pageSize);
            PaginationGeneric<TestResults> paginationTestAttempts = new PaginationGeneric<TestResults>
            {
                pagination = pagination,
                source = items
            };

            ViewBag.registrations = userRegistrations;
            ViewBag.tests = testsInfo;
            ViewBag.Page = page;
            ViewBag.PageCount = (count + pageSize - 1) / pageSize;

            return View(paginationTestAttempts);

        }

        #endregion

        #region test privacy settings

        public async Task<IActionResult> VerifiedUsers(int testId, SortingOrders sortOrder, int page = 1)
        {
            int pageSize = 10;

            ViewData["CurrentSort"] = sortOrder;

            ViewData["EmailSortParm"] = sortOrder == SortingOrders.EmailAsc ? SortingOrders.EmailDesc : SortingOrders.EmailAsc;

            var verifiedUsers = _context.VerifiedUsers.Where(x => x.TestId == testId);

            verifiedUsers = sortOrder switch
            {
                SortingOrders.EmailAsc => verifiedUsers.OrderBy(x => x.UserEmail),
                SortingOrders.EmailDesc => verifiedUsers.OrderByDescending(x => x.UserEmail),
                _ => verifiedUsers
            };

            var count = await verifiedUsers.CountAsync();
            var items = await verifiedUsers.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            Pagination pagination = new Pagination(count, page, pageSize);
            PaginationGeneric<VerifiedUsers> paginationVerifiedUsers = new PaginationGeneric<VerifiedUsers>
            {
                pagination = pagination,
                source = items
            };

            ViewBag.Page = page;
            ViewBag.PageCount = (count + pageSize - 1) / pageSize;

            return View(paginationVerifiedUsers);
        }

        public async Task<IActionResult> AddUserToVL(int testId, string userEmail)
        {
            var test = _context.Test.Find(testId);

            if(test != null)
            {
                if(string.IsNullOrEmpty(userEmail) || string.IsNullOrWhiteSpace(userEmail))
                {
                    TempData["ErrorMessage"] = "Email can't be null or whitespace";
                }
                else
                {
                    VerifiedUsers newVerifiedUser = new VerifiedUsers()
                    {
                        TestId = testId,
                        UserEmail = userEmail
                    };

                    _context.VerifiedUsers.Add(newVerifiedUser);

                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction("VerifiedUsers", new { testId });
        }

        public async Task<IActionResult> RemoveUserFromVL(int testId, string userEmail)
        {
            var test = _context.Test.Find(testId);

            if (test != null)
            {
                var user = _context.VerifiedUsers.Where(x => x.UserEmail == userEmail && x.TestId == test.Id).FirstOrDefault();

                _context.VerifiedUsers.Remove(user);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("VerifiedUsers", new { testId });
        }

        #endregion

        #region certificate

        public async Task<IActionResult> GenerateCertificate(int resultsId)
        {
            var results = _context.TestResults.Find(resultsId);

            if(results == null)
            {
                //err message here

                return RedirectToAction("UserAttempts");
            }
            else
            {
                HtmlToPdfConverter converter = new HtmlToPdfConverter();

                WebKitConverterSettings settings = new WebKitConverterSettings();
                settings.WebKitPath = @"QtBinariesWindows";

                converter.ConverterSettings = settings;

                //filling template with test results data
                #region certificate text

                string str = System.IO.File.ReadAllText(@"Resources\CertificateTemplate.txt");

                UserIdentity currentUser = await _userManager.GetUserAsync(User);
                var registration = _context.Registration.Find(results.RegistrationId);
                var test = _context.Test.Find(registration.TestId);
                var testAuthor = _userManager.Users.Where(x => x.Id == test.TestAuthorId).FirstOrDefault();
                var company = _context.Company.Find(testAuthor.CompanyId);

                str = str.Replace("_companyName", company.FullName);
                str = str.Replace("_testAuthorName", testAuthor.FirstName + " " + testAuthor.LastName);
                str = str.Replace("_userName", currentUser.FirstName + " " + currentUser.LastName);
                str = str.Replace("_testName", test.Name);
                str = str.Replace("_testScore", results.FinalMarkInPercents.ToString() + "%");
                str = str.Replace("_date", registration.EntryTime.ToString("dd/MM/yyyy"));

                #endregion

                PdfDocument document = converter.Convert(str, string.Empty);

                //saving to downloads folder
                MemoryStream memStream = new MemoryStream();
                document.Save(memStream);
                document.Close(true);

                memStream.Position = 0;

                FileStreamResult fileStreamResult = new FileStreamResult(memStream, "application/pdf");
                fileStreamResult.FileDownloadName = "Certificate_" + currentUser.FirstName + currentUser.LastName + "_" + test.Name + ".pdf";

                return fileStreamResult;
            }
        }

        #endregion
    }
}
