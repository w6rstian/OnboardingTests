using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Onboarding.Controllers;
using Onboarding.Data;
using Onboarding.Interfaces;
using Onboarding.Models;
using Onboarding.ViewModels;
using System.Security.Claims;
using Xunit;
using Task = System.Threading.Tasks.Task;

namespace OnboardingXUnitTests.Controllers
{
    public class OnboardingControllerTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly OnboardingController _controller;

        public OnboardingControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);
            _context.Database.EnsureCreated();

            _controller = new OnboardingController(_context);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "testuser"),
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Role, "Admin")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
        }

        [Fact]
        public void Create_ReturnsViewResult()
        {
            // Act
            var result = _controller.Create();

            // Assert
            result.Should().BeOfType<ViewResult>();
        }
        [Fact]
        public async Task CreatePost_ModelIsNull_ReturnsViewWithModelError()
        {
            // Act
            var result = await _controller.Create(null);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            _controller.ModelState.ErrorCount.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreatePost_CourseNameEmpty_ReturnsViewWithMentorsData()
        {
            // Arrange
            _context.Users.Add(new User { Id = 1, Name = "Jan", Surname = "Testowy" });
            await _context.SaveChangesAsync();

            var viewModel = new CreateOnboardingViewModel { CourseName = "" };

            // Act
            var result = await _controller.Create(viewModel);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            _controller.ModelState.IsValid.Should().BeFalse();
            viewResult.ViewData.ContainsKey("Mentors").Should().BeTrue();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
