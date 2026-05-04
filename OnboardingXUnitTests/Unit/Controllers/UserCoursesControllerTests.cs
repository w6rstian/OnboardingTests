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

namespace OnboardingXUnitTests.Unit.Controllers
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
             
            var result = await _controller.Index();

             
            result.Should().BeOfType<ViewResult>();
        }

        public void Dispose()
        {
            _context.Dispose();
        }


        // Sebastian Szklanko

        [Fact]
        public async Task Index_ReturnsUserCoursesList()
        {
             
            _context.UserCourses.Add(new UserCourse { Id = 1, UserId = 1, CourseId = 1 });
            await _context.SaveChangesAsync();

             
            var result = await _controller.Index();

             
            var view = result.Should().BeOfType<ViewResult>().Subject;
            view.Model.Should().NotBeNull();
        }

        // Sebastian Szklanko
        [Fact]
        public async Task Details_NullId_ReturnsNotFound()
        {
             
            var result = await _controller.Details(null);

             
            result.Should().BeOfType<NotFoundResult>();
        }

        // Sebastian Szklanko
        [Fact]
        public async Task Details_NonExistingId_ReturnsNotFound()
        {
             
            var result = await _controller.Details(99);

             
            result.Should().BeOfType<NotFoundResult>();
        }

        // Sebastian Szklanko
        [Fact]
        public async Task Details_ExistingId_ReturnsView()
        {
             
            var user = new User { Id = 1, Name = "Jan", Surname = "User" };
            var course = new Course { Id = 1, Name = "Course" };

            _context.Users.Add(user);
            _context.Courses.Add(course);

            _context.UserCourses.Add(new UserCourse
            {
                Id = 1,
                UserId = 1,
                CourseId = 1
            });

            await _context.SaveChangesAsync();

             
            var result = await _controller.Details(1);

             
            result.Should().BeOfType<ViewResult>();
        }

        // Sebastian Szklanko
        [Fact]
        public async Task Create_Post_ValidData_SavesUserCourse()
        {
             
            var result = await _controller.Create(1, 1);

             
            result.Should().BeOfType<RedirectToActionResult>();
            _context.UserCourses.Should().HaveCount(1);
        }

        // Sebastian Szklanko
        [Fact]
        public async Task Edit_Get_NullId_ReturnsNotFound()
        {
             
            var result = await _controller.Edit(null);

             
            result.Should().BeOfType<NotFoundResult>();
        }

        // Sebastian Szklanko
        [Fact]
        public async Task Edit_Get_NonExistingId_ReturnsNotFound()
        {
             
            var result = await _controller.Edit(5);

             
            result.Should().BeOfType<NotFoundResult>();
        }

        // Sebastian Szklanko
        [Fact]
        public async Task Delete_Get_ExistingId_ReturnsView()
        {
             
            var user = new User { Id = 1, Name = "Jan", Surname = "User" };
            var course = new Course { Id = 1, Name = "Course" };

            _context.Users.Add(user);
            _context.Courses.Add(course);

            _context.UserCourses.Add(new UserCourse
            {
                Id = 1,
                UserId = 1,
                CourseId = 1
            });

            await _context.SaveChangesAsync();

             
            var result = await _controller.Delete(1);

             
            result.Should().BeOfType<ViewResult>();
        }

        // Sebastian Szklanko
        [Fact]
        public async Task DeleteConfirmed_RemovesUserCourse()
        {
             
            _context.UserCourses.Add(new UserCourse { Id = 1, UserId = 1, CourseId = 1 });
            await _context.SaveChangesAsync();

             
            var result = await _controller.DeleteConfirmed(1);

             
            result.Should().BeOfType<RedirectToActionResult>();
            _context.UserCourses.Should().BeEmpty();
        }
    }
}
