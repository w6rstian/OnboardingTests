using Xunit;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Onboarding.Controllers;
using Onboarding.Data;
using Onboarding.Models;
using Onboarding.Interfaces;
using System;
using System.Security.Claims;
using Task = System.Threading.Tasks.Task;

namespace OnboardingXUnitTests.Unit.Controllers
{
    public class QuestionsControllerTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly QuestionsController _controller;

        public QuestionsControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);
            _context.Database.EnsureCreated();

            _controller = new QuestionsController(_context);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "testuser"),
                new Claim(ClaimTypes.NameIdentifier, "1")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
        }

        /*---Autor Michał Kobyliński----*/
        [Fact]
        public async Task Index_ReturnsViewResult()
        {
            // Act
            var result = await _controller.Index();

            // Assert
            result.Should().BeOfType<ViewResult>();
        }

        
        [Fact]
        public async Task Details_QuestionExists_ReturnsViewResultWithQuestion()
        {
            // Arrange
            var question = new Question
            {
                Id = 1,
                Description = "Pytanie testowe",
                AnswerA = "A",
                AnswerB = "B",
                AnswerC = "C",
                AnswerD = "D",
                CorrectAnswer = "A",
                TestId = 1,
                Test = new Test { Id = 1, Name = "Test 1", CourseId = 1 }
            };
            _context.Questions.Add(question);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.Details(1);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeAssignableTo<Question>().Subject;
            model.Id.Should().Be(1);
        }

       
        [Fact]
        public async Task CreatePost_ValidData_SavesQuestionAndRedirects()
        {
            // Arrange
            var test = new Test { Id = 1, Name = "Test z C#", CourseId = 1 };
            _context.Tests.Add(test);
            await _context.SaveChangesAsync();

            
            var result = await _controller.Create("Nowe pytanie?", "Odp A", "Odp B", "Odp C", "Odp D", "A", test.Id);

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");

            var savedQuestion = await _context.Questions.FirstOrDefaultAsync();
            savedQuestion.Should().NotBeNull();
            savedQuestion.Description.Should().Be("Nowe pytanie?");
        }

       
        [Fact]
        public async Task EditPost_ValidData_UpdatesQuestionAndRedirects()
        {
            // Arrange
            var question = new Question { Id = 1, Description = "Stara treść", AnswerA = "A", AnswerB = "B", AnswerC = "C", AnswerD = "D", CorrectAnswer = "A", TestId = 1 };
            _context.Questions.Add(question);
            await _context.SaveChangesAsync();

            _context.ChangeTracker.Clear();

            var updatedQuestion = new Question { Id = 1, Description = "Zaktualizowana treść", AnswerA = "A", AnswerB = "B", AnswerC = "C", AnswerD = "D", CorrectAnswer = "B", TestId = 1 };

            // Act
            var result = await _controller.Edit(1, updatedQuestion);

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");

            var dbQuestion = await _context.Questions.FindAsync(1);
            dbQuestion.Description.Should().Be("Zaktualizowana treść");
            dbQuestion.CorrectAnswer.Should().Be("B");
        }
        [Fact]
        public async Task DeleteConfirmed_DeletesQuestionAndRedirects()
        {
            // Arrange
            var question = new Question { Id = 1, Description = "Do usunięcia", AnswerA = "A", AnswerB = "B", AnswerC = "C", AnswerD = "D", CorrectAnswer = "A", TestId = 1 };
            _context.Questions.Add(question);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.DeleteConfirmed(1);

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");

            var dbQuestion = await _context.Questions.FindAsync(1);
            dbQuestion.Should().BeNull();
        }
        /*-----------------*/
        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}