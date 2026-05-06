using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Onboarding.Controllers;
using Onboarding.Data;
using Onboarding.Models;
using Onboarding.Interfaces;
using Xunit;
using Task = System.Threading.Tasks.Task;

namespace OnboardingXUnitTests.Unit.Controllers
{
    public class TasksControllerTests : IDisposable
    {
        private readonly TasksController _controller;
        private readonly ApplicationDbContext _context;

        public TasksControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);

            _controller = new TasksController(_context);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Name, "test@test.com")
            }, "TestAuthentication"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }
        /* ---- Autor Michał Kobyliński ---*/
        [Fact]
        public async Task Index_ReturnsViewResult()
        {
            // Act
            var result = await _controller.Index();

            // Assert
            result.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public async Task Details_IdIsNull_ReturnsNotFound()
        {
            // Act
            var result = await _controller.Details(null);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }
        [Fact]
        public async Task Details_TaskNotFound_ReturnsNotFound()
        {
            var result = await _controller.Details(null);

            result.Should().BeOfType<NotFoundResult>();
        }


        [Fact]
        public async Task Details_TaskExists_ReturnsViewResultWithTask()
        {
            // Arrange
            var expectedTask = new Onboarding.Models.Task
            {
                Id = 1,
                Title = "Test",
                Description = "Test",

                CourseId = 1,
                Course = new Course { Id = 1, Name = "Test" }, 

                MentorId = 2,
                Mentor = new User { Id = 2, Name = "Test" }  
            };

            _context.Tasks.Add(expectedTask);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.Details(expectedTask.Id);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>("ponieważ zadanie zostało zapisane wraz z powiązanymi danymi").Subject;

            var model = viewResult.Model.Should().BeAssignableTo<Onboarding.Models.Task>().Subject;
            model.Id.Should().Be(expectedTask.Id);
        }

        [Fact]
        public async Task Create_MissingRequiredFields_ReturnsViewResult()
        {
            // Arrange
            _context.Courses.Add(new Course { Id = 1, Name = "Kurs" });
            _context.Users.Add(new User { Id = 1, Name = "Mentor" }); 
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.Create(1, "", "Opis", 1, null, null);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;

            _controller.ModelState.IsValid.Should().BeFalse();

            viewResult.ViewData.ContainsKey("CourseId").Should().BeTrue();
            viewResult.ViewData.ContainsKey("MentorId").Should().BeTrue();
        }

        [Fact]
        public async Task Create_CourseNotFound_ReturnsNotFound()
        {
            // Act
            var result = await _controller.Create(1, "Tytuł", "Opis", 999, null, null);

            // Assert
            result.Should().BeOfType<NotFoundResult>("ponieważ w bazie nie ma kursu o ID 999");
        }
        [Fact]
        public async Task Create_MentorNotFound_ReturnsNotFound()
        {
            // Arrange
            var course = new Course { Id = 1, Name = "Kurs testowy" };
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.Create(999, "Tytuł", "Opis", 1, null, null);

            // Assert
            result.Should().BeOfType<NotFoundResult>("ponieważ w bazie nie ma mentora o ID 999");
        }

        
        [Fact]
        public async Task Create_ValidData_SavesTaskAndReturnsRedirectToIndex()
        {
            
            var course = new Course { Id = 1, Name = "Kurs" };
            var mentor = new User { Id = 1, Name = "Mentor" }; 

            _context.Courses.Add(course);
            _context.Users.Add(mentor);
            await _context.SaveChangesAsync();

            string title = "Nowe zadanie";
            string links = "https://google.com https://github.com"; 
            string articleContent = "Treść artykułu";

            // Act
            var result = await _controller.Create(mentor.Id, title, "Opis", course.Id, articleContent, links);

            // Assert
            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("Index");

            var savedTask = await _context.Tasks
                .Include(t => t.Links)
                .Include(t => t.Articles)
                .FirstOrDefaultAsync(t => t.Title == title);

            savedTask.Should().NotBeNull();
            savedTask.Description.Should().Be("Opis");
            savedTask.CourseId.Should().Be(1);
            savedTask.MentorId.Should().Be(1);

            savedTask.Links.Should().HaveCount(2);
            savedTask.Links.Should().Contain(l => l.LinkUrl == "https://google.com");

            savedTask.Articles.Should().HaveCount(1);
            savedTask.Articles.First().Content.Should().Be("Treść artykułu");
        }
        [Fact]
        public async Task DeleteGet_IdIsNull_ReturnsNotFound()
        {
            var result = await _controller.Delete(null);

            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task DeleteGet_TaskNotFound_ReturnsNotFound()
        {
            var result = await _controller.Delete(999);

            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task DeleteGet_TaskExists_ReturnsViewResultWithTask()
        {
            var expectedTask = new Onboarding.Models.Task
            {
                Id = 1,
                Title = "Test",
                Description = "Test",
                CourseId = 1,
                Course = new Course { Id = 1, Name = "Test" },
                MentorId = 1,
                Mentor = new User { Id = 1, Name = "Test" }
            };

            _context.Tasks.Add(expectedTask);
            await _context.SaveChangesAsync();

            var result = await _controller.Delete(expectedTask.Id);

            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeAssignableTo<Onboarding.Models.Task>().Subject;
            model.Id.Should().Be(expectedTask.Id);
        }
        /* --------------------------*/
        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
