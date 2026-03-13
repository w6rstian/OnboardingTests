using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Onboarding.Controllers;
using Onboarding.Data;
using Onboarding.Models;
using Onboarding.ViewModels; 
using Xunit;
using Task = System.Threading.Tasks.Task;

namespace OnboardingXUnitTests.Controllers
{
    public class TestsControllerTests : IDisposable
    {
        private readonly TestsController _controller;
        private readonly ApplicationDbContext _context;

        public TestsControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);

            _controller = new TestsController(_context);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Name, "test@test.com")
            }, "TestAuthentication"));

            var httpContext = new DefaultHttpContext { User = user };

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            
            _controller.TempData = new TempDataDictionary(httpContext, A.Fake<ITempDataProvider>());
        }

        [Fact]
        public async Task Index_ReturnsViewResult()
        {
            var result = await _controller.Index();

            result.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public void CreateGet_ReturnsViewResult()
        {
            var result = _controller.Create();

            result.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public async Task CreatePost_CourseNotFound_ReturnsNotFound()
        {
            var test = new Onboarding.Models.Test { CourseId = 999, Name = "Nowy Test" };

            var result = await _controller.Create(test, new List<Question>());

            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task CreatePost_ValidData_SavesTestAndRedirects()
        {
            var course = new Course { Id = 1, Name = "Kurs C#" };
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            var test = new Onboarding.Models.Test { CourseId = 1, Name = "Test z C#" };
            var questions = new List<Question>
            {
                new Question
                {
                    Description = "Pytanie 1",
                    AnswerA = "A",
                    AnswerB = "B",
                    AnswerC = "C",
                    AnswerD = "D",
                    CorrectAnswer = "A"
                }
            };

            var result = await _controller.Create(test, questions);

            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");

            var savedTest = await _context.Tests.Include(t => t.Questions).FirstOrDefaultAsync();
            savedTest.Should().NotBeNull();
            savedTest.Name.Should().Be("Test z C#");
            savedTest.Questions.Should().HaveCount(1);
        }

        [Fact]
        public async Task Details_IdIsNull_ReturnsNotFound()
        {
            var result = await _controller.Details(null);

            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task Details_TestExists_ReturnsViewResult()
        {
            var test = new Onboarding.Models.Test { Id = 1, Name = "Test 1", Course = new Course { Id = 1, Name = "Kurs" } };
            _context.Tests.Add(test);
            await _context.SaveChangesAsync();

            var result = await _controller.Details(1);

            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeAssignableTo<Onboarding.Models.Test>().Subject;
            model.Id.Should().Be(1);
        }

        [Fact]
        public async Task EditGet_TestExists_ReturnsViewResult()
        {
            var test = new Onboarding.Models.Test { Id = 1, Name = "Test", Course = new Course { Id = 1, Name = "Kurs" } };
            _context.Tests.Add(test);
            await _context.SaveChangesAsync();

            var result = await _controller.Edit(1);

            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeAssignableTo<Onboarding.Models.Test>().Subject;
            model.Id.Should().Be(1);
        }

        

        [Fact]
        public async Task DeleteGet_TestExists_ReturnsViewResult()
        {
            var test = new Onboarding.Models.Test { Id = 1, Name = "Test", Course = new Course { Id = 1, Name = "Kurs" } };
            _context.Tests.Add(test);
            await _context.SaveChangesAsync();

            var result = await _controller.Delete(1);

            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().BeAssignableTo<Onboarding.Models.Test>();
        }
        [Fact]
        public async Task EditPost_ValidData_UpdatesTestAndRedirects()
        {
            var test = new Onboarding.Models.Test
            {
                Id = 1,
                Name = "Stara nazwa",
                CourseId = 1,
                Course = new Course { Id = 1, Name = "Kurs" },
                Questions = new List<Question>
                {
                    new Question { Description = "Stare pytanie", AnswerA = "1", AnswerB = "2", AnswerC = "3", AnswerD = "4", CorrectAnswer = "A" }
                }
            };
            _context.Tests.Add(test);
            await _context.SaveChangesAsync();

            var updatedTest = new Onboarding.Models.Test { Id = 1, Name = "Nowa nazwa", CourseId = 1 };
            var newQuestions = new List<Question>
            {
                new Question { Description = "Nowe pytanie", AnswerA = "A", AnswerB = "B", AnswerC = "C", AnswerD = "D", CorrectAnswer = "B" }
            };

            var result = await _controller.Edit(1, updatedTest, newQuestions);

            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");

            var savedTest = await _context.Tests.Include(t => t.Questions).FirstOrDefaultAsync(t => t.Id == 1);
            savedTest.Name.Should().Be("Nowa nazwa");
            savedTest.Questions.Should().HaveCount(1);
            savedTest.Questions.First().Description.Should().Be("Nowe pytanie");
        }

        [Fact]
        public async Task DeleteConfirmed_DeletesTestAndRedirects()
        {
            var test = new Onboarding.Models.Test { Id = 1, Name = "Test do usunięcia", Questions = new List<Question>() };
            _context.Tests.Add(test);
            await _context.SaveChangesAsync();

            var result = await _controller.DeleteConfirmed(1);

            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");

            var deletedTest = await _context.Tests.FirstOrDefaultAsync(t => t.Id == 1);
            deletedTest.Should().BeNull();
        }

        [Fact]
        public async Task ExecuteGet_AlreadyTaken_RedirectsToDetails()
        {
            _context.UserTestResults.Add(new UserTestResult { TestId = 1, UserId = "1" });
            await _context.SaveChangesAsync();

            var result = await _controller.Execute(1);

            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Details");
            _controller.TempData["Error"].Should().Be("Już rozwiązałeś ten test.");
        }

        [Fact]
        public async Task ExecuteGet_NotTaken_ReturnsViewWithViewModel()
        {
            var test = new Onboarding.Models.Test
            {
                Id = 1,
                Name = "C# Basics",
                CourseId = 5,
                Questions = new List<Question>
                {
                    new Question { Id = 10, Description = "Q1", AnswerA = "A", AnswerB = "B", AnswerC = "C", AnswerD = "D", CorrectAnswer = "A" }
                }
            };
            _context.Tests.Add(test);
            await _context.SaveChangesAsync();

            var result = await _controller.Execute(1);

            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeAssignableTo<TestViewModel>().Subject;
            model.TestId.Should().Be(1);
            model.Questions.Should().HaveCount(1);
        }

        [Fact]
        public async Task ExecutePost_ValidAnswers_CalculatesScoreAndRedirects()
        {
            var test = new Onboarding.Models.Test
            {
                Id = 1,
                Name = "Test kompetencji", // Brakowało nazwy testu
                CourseId = 5,
                Questions = new List<Question>
                {
                    new Question { Id = 10, TestId = 1, Description = "Q1", AnswerA = "A", AnswerB = "B", AnswerC = "C", AnswerD = "D", CorrectAnswer = "A" },
                    new Question { Id = 11, TestId = 1, Description = "Q2", AnswerA = "A", AnswerB = "B", AnswerC = "C", AnswerD = "D", CorrectAnswer = "B" }
                }
            };
            _context.Tests.Add(test);
            await _context.SaveChangesAsync();

            var viewModel = new TestViewModel
            {
                TestId = 1,
                CourseId = 5,
                Answers = new List<AnswerSubmissionModel>
                {
                    new AnswerSubmissionModel { QuestionId = 10, SelectedAnswer = "A" }, // Dobra odpowiedź
                    new AnswerSubmissionModel { QuestionId = 11, SelectedAnswer = "C" }  // Zła odpowiedź
                }
            };

            var result = await _controller.Execute(viewModel);

            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Details");
            redirectResult.ControllerName.Should().Be("UserCoursesList");
            _controller.TempData["Message"].Should().Be("Twój wynik: 1 poprawnych odpowiedzi.");

            var testResult = await _context.UserTestResults.FirstOrDefaultAsync(r => r.TestId == 1 && r.UserId == "1");
            testResult.Should().NotBeNull();
            testResult.CorrectAnswers.Should().Be(1);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}