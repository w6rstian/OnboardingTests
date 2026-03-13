using System;
using System.Security.Claims;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Onboarding.Controllers;
using Onboarding.Data;
using Onboarding.Models;
using Onboarding.Interfaces;
using Xunit;
using Task = System.Threading.Tasks.Task;

namespace OnboardingXUnitTests.Controllers
{
    public class UserCoursesControllerTests : IDisposable
    {
        private readonly UserCoursesController _controller;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _fakeUserManager;

        public UserCoursesControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);

            var store = A.Fake<IUserStore<User>>();
            _fakeUserManager = A.Fake<UserManager<User>>(x => x.WithArgumentsForConstructor(
                new object[] { store, null, null, null, null, null, null, null, null }));

            _controller = new UserCoursesController(_context, _fakeUserManager);

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

        [Fact]
        public async Task Index_ReturnsViewResult()
        {
            // Act
            var result = await _controller.Index();

            // Assert
            result.Should().BeOfType<ViewResult>();
        }

        public void Dispose()
        {
            _context.Dispose();
        }


        // S

        [Fact]
        public async Task Index_ReturnsUserCoursesList()
        {
            // Arrange
            _context.UserCourses.Add(new UserCourse { Id = 1, UserId = 1, CourseId = 1 });
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.Index();

            // Assert
            var view = result.Should().BeOfType<ViewResult>().Subject;
            view.Model.Should().NotBeNull();
        }

        [Fact]
        public async Task Details_NullId_ReturnsNotFound()
        {
            // Act
            var result = await _controller.Details(null);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task Details_NonExistingId_ReturnsNotFound()
        {
            // Act
            var result = await _controller.Details(99);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }


        [Fact]
        public async Task Details_ExistingId_ReturnsView()
        {
            // Arrange
            _context.UserCourses.Add(new UserCourse { Id = 1, UserId = 1, CourseId = 1 });
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.Details(1);

            // Assert
            result.Should().BeOfType<ViewResult>();
        }


        [Fact]
        public async Task Create_Post_ValidData_SavesUserCourse()
        {
            // Act
            var result = await _controller.Create(1, 1);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            _context.UserCourses.Should().HaveCount(1);
        }


        [Fact]
        public async Task Edit_Get_NullId_ReturnsNotFound()
        {
            // Act
            var result = await _controller.Edit(null);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }


        [Fact]
        public async Task Edit_Get_NonExistingId_ReturnsNotFound()
        {
            // Act
            var result = await _controller.Edit(5);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }


        [Fact]
        public async Task Delete_Get_ExistingId_ReturnsView()
        {
            // Arrange
            _context.UserCourses.Add(new UserCourse { Id = 1, UserId = 1, CourseId = 1 });
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.Delete(1);

            // Assert
            result.Should().BeOfType<ViewResult>();
        }


        [Fact]
        public async Task DeleteConfirmed_RemovesUserCourse()
        {
            // Arrange
            _context.UserCourses.Add(new UserCourse { Id = 1, UserId = 1, CourseId = 1 });
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.DeleteConfirmed(1);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            _context.UserCourses.Should().BeEmpty();
        }
    }
}
