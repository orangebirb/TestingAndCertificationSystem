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
using TestingAndCertificationSystem.Controllers;
using MimeKit;

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

                return RedirectToAction("Tests");
            }

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> EditTest(int testId)
        {
            Test testToEdit = _context.Test.Find(testId);

            return View(testToEdit);
        }

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

        [HttpPost]
        public async Task<IActionResult> ChangeActivityStatusTest(int testId, DateTime endingDateTime)
        {
            Test testToEdit = _context.Test.Find(testId);

            if (testToEdit != null)
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
                    }

                    await _context.SaveChangesAsync();
            }

            return RedirectToAction("TestInfopage", new { testId });

        }

        public IActionResult TestInfopage(int testId)
        {
            Test test = _context.Test.Find(testId);

            if(test != null)
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

                return View(test);
            }

            return RedirectToAction("Error");

        }

        public async Task<IActionResult> Instruction(int testId)
        {
            Test test = _context.Test.Find(testId);

            UserIdentity testAuthor = await _userManager.FindByIdAsync(test.TestAuthorId);
            var company = _context.Company.Where(x => x.Id == testAuthor.CompanyId).FirstOrDefault();

            ViewBag.TestAuthor = testAuthor;
            ViewBag.Company = company;

            if (test != null)
            {
                ViewBag.QuestionCount = _context.Question.Where(x => x.TestId == test.Id).Count();

                // --- temp script for deactivating test ---
                if(DateTime.Now >= test.TokenEndTime)
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

        [HttpGet]
        public IActionResult CreateQuestion()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateQuestion(QuestionDataModel model, int testId)
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

        [HttpGet]
        public IActionResult CreateAdditionalTask(int testId)
        {
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

                return RedirectToAction("TestInfopage", new { testId });
            }

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> EditAdditionalTask(int taskId)
        {
            AdditionalTask taskToEdit = _context.AdditionalTask.Find(taskId);

            return View(taskToEdit);
        }

        [HttpPost]
        public async Task<IActionResult> EditAdditionalTask(AdditionalTask additionalTask)
        {
            //try
            //{
                AdditionalTask taskToEdit = _context.AdditionalTask.Find(additionalTask.Id);

                _context.Entry(taskToEdit).CurrentValues.SetValues(additionalTask);

                _context.AdditionalTask.Update(taskToEdit);

                await _context.SaveChangesAsync();

                ViewBag.successMessage = "Changes saved";
            //}
            //catch (Exception ex)
            //{
                //ModelState.AddModelError("", ex.Message);
            //}

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAdditionalTask(int taskId, int testId)
        {
            AdditionalTask taskToDelete = _context.AdditionalTask.Find(taskId);

            //gets test with current task
            //var testId = _context.Test.Where(x => taskId == x.AdditionalTaskId);
            //без передавания с параметров

            if (taskToDelete != null)
            {
                _context.AdditionalTask.Remove(taskToDelete);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("TestInfopage", new { testId });
        }
        #endregion

        #region user test management

        [HttpPost]
        public async Task<IActionResult> Registration(int testId)
        {
            if(testId != 0)
            {
                UserIdentity currentUser = await _userManager.GetUserAsync(User);

                var currentTest = _context.Test.Find(testId);

                if(currentTest != null)
                {
                    Registration registration = _context.Registration.Where(x => x.UserId == currentUser.Id
                        && x.TestId == testId
                        && x.EndingTime > DateTime.Now).FirstOrDefault();

                    if (registration != null)
                    {
                        TestResults results = _context.TestResults.Where(x => x.RegistrationId == registration.Id).FirstOrDefault();

                        //registrated & not submited = current registration
                        if(results == null)
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

        [HttpPost]
        public async Task<IActionResult> SubmitAnswer(QuestionDataModel model, Guid token, int qNum)
        {
            if(model != null)
            {
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

        [HttpGet]
        public async Task<IActionResult> Test(Guid token, int qNum)
        {
            if (token != null)
            {
                var registration = _context.Registration.Where(x => x.Token == token).FirstOrDefault();

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

        public async Task<IActionResult> TestResults(Guid token)
        {
            var registration = _context.Registration.Where(x => x.Token == token).FirstOrDefault();

            if(registration != null)
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

                    if (test.AdditionalTaskId != 0)
                    {
                        var additionalTask = _context.AdditionalTask.Where(x => x.Id == test.AdditionalTaskId).FirstOrDefault();

                        UserIdentity currentUser = await _userManager.GetUserAsync(User);

                        SendAdditionalTask(additionalTask, currentUser.Email);
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

        private void SendAdditionalTask(AdditionalTask additionalTask, string recepientEmail)
        {
                
        }


        #endregion

        #region assignment results

        public async Task<IActionResult> TestAttempts(int testId)
        {
            var testAttempts = _context.Registration.Where(x => x.TestId == testId).ToList();

            var users = _userManager.Users.Where(x => testAttempts.Select(x => x.UserId).Any(z => z == x.Id)).ToList();
            var results = _context.TestResults.Where(x => testAttempts.Select(x => x.Id).Any(z => z == x.RegistrationId)).ToList();

            ViewBag.Users = users;
            ViewBag.Results = results;

            return View(testAttempts);
        }

        public IActionResult AttemptInfopage(int registrationId)
        {
            Registration registration = _context.Registration.Find(registrationId);

            TestResults testResults = _context.TestResults.Where(x => x.RegistrationId == registration.Id).FirstOrDefault();

            UserIdentity user = _userManager.Users.Where(x => x.Id == registration.UserId).FirstOrDefault();

            var questionAnswers = _context.QuestionAnswer.Where(x => x.RegistrationId == registration.Id).ToList();

            if(questionAnswers.Where(x => x.QuestionId == null).Count() != 0)
            {
                ViewBag.DeletedQuestionsMessage = "Some questions in this test have been removed, but they still have an effect on the test results";
            }

            //questions in original test
            var testQuestions = _context.Question.Where(x => x.TestId == registration.TestId).ToList();

            if(testQuestions != null)
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

            return View(questionAnswers);
        }

        public async Task<IActionResult>  UserAttempts()
        {
            UserIdentity currentUser = await _userManager.GetUserAsync(User);

            var userRegistrations = _context.Registration.Where(x => x.UserId == currentUser.Id).ToList();

            var userTestResultss = _context.TestResults.Where(x => userRegistrations.Select(x => x.Id).Any(y => y == x.RegistrationId)).ToList();

            ViewBag.registrations = userRegistrations;

            return View(userTestResultss);
        }


        #endregion
    }
}
