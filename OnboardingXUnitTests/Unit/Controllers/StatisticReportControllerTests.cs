using Xunit;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Onboarding.Controllers;
using Onboarding.Data;
using Onboarding.Models;
using Onboarding.Interfaces;
using System.Security.Claims;
using Task = System.Threading.Tasks.Task;
using Onboarding.Data.Enums;

namespace OnboardingXUnitTests.Unit.Controllers
{
    public class StatisticReportControllerTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly StatisticReportController _controller;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;

        public StatisticReportControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);
            _context.Database.EnsureCreated();

            _userManager = A.Fake<UserManager<User>>(x => x.WithArgumentsForConstructor(() => new UserManager<User>(A.Fake<IUserStore<User>>(), null, null, null, null, null, null, null, null)));
            _roleManager = A.Fake<RoleManager<IdentityRole<int>>>(x => x.WithArgumentsForConstructor(() => new RoleManager<IdentityRole<int>>(A.Fake<IRoleStore<IdentityRole<int>>>(), null, null, null, null)));

            _controller = new StatisticReportController(_context, _userManager, _roleManager);

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

        [Fact]
        public async Task GetCourseTasks_ReturnsJsonResult()
        {
            var result = await _controller.GetCourseTasks(1);

            result.Should().BeOfType<JsonResult>();
        }

        [Fact]
        public async Task Index_ReturnsViewResult_WithValidModel()
        {

            _context.Roles.Add(new IdentityRole<int> { Id = 1, Name = "Nowy" });
            _context.Roles.Add(new IdentityRole<int> { Id = 2, Name = "Manager" });

            var user = new User { Id = 1, Name = "Jan", Surname = "Kowalski", Email = "jan@test.com" };
            _context.Users.Add(user);

            var course = new Course { Id = 1, Name = "Testowy Kurs" };
            _context.Courses.Add(course);

            var userCourse = new UserCourse { UserId = 1, CourseId = 1 };
            _context.UserCourses.Add(userCourse);

            await _context.SaveChangesAsync();

            A.CallTo(() => _roleManager.Roles).Returns(_context.Roles);

            A.CallTo(() => _userManager.GetRolesAsync(A<User>._)).Returns(new List<string> { "Nowy" });

            var result = await _controller.Index();
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeOfType<StatisticReportViewModel>().Subject;

            model.Roles.Should().Contain("Nowy");
            model.UserCountsByRole["Nowy"].Should().Be(1);
            model.Courses.Should().HaveCount(1);
            model.NewUsersInCourses.Should().HaveCount(1);
            model.NewUsers.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetNewUsersByCourse_ReturnsJsonResult_WithFilteredUsers()
        {

            var user = new User { Id = 1, Name = "Nowy", Surname = "U¿ytkownik" };
            _context.Users.Add(user);
            _context.UserCourses.Add(new UserCourse { UserId = 1, CourseId = 10 });
            await _context.SaveChangesAsync();

            A.CallTo(() => _userManager.GetRolesAsync(A<User>._)).Returns(new List<string> { "Nowy" });
            var result = await _controller.GetNewUsersByCourse(10);

            var jsonResult = result.Should().BeOfType<JsonResult>().Subject;
            var data = jsonResult.Value.Should().BeAssignableTo<System.Collections.IEnumerable>().Subject;
            data.Cast<object>().Should().HaveCount(1);
        }

        [Fact]
        public async Task GetUsersByRole_ReturnsJsonResult_WithMatchingRole()
        {
            var user = new User { Id = 1, Name = "Test", Surname = "Manager" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            A.CallTo(() => _userManager.GetRolesAsync(A<User>._)).Returns(new List<string> { "Manager" });

            var result = await _controller.GetUsersByRole("Manager");

            var jsonResult = result.Should().BeOfType<JsonResult>().Subject;
            var data = jsonResult.Value.Should().BeAssignableTo<System.Collections.IEnumerable>().Subject;
            data.Cast<object>().Should().HaveCount(1);
        }

        [Theory]
        [InlineData("Manager")]
        [InlineData("Mentor")]
        [InlineData("Buddy")]
        [InlineData("Nowy")]
        public async Task GetDetailsByRoleUser_ReturnsJsonResult_DependingOnRole(string role)
        {
            var mentor = new User { Id = 1, Name = "Mentor" };
            var buddy = new User { Id = 2, Name = "Buddy", BuddyId = 1 };

            _context.Users.AddRange(mentor, buddy);
            _context.Courses.Add(new Course { Id = 10, MentorId = 1, Name = "Kurs",});
            _context.Tasks.Add(new Onboarding.Models.Task { Id = 20, MentorId = 1, Title = "Zadanie", Description = "Desc", CourseId = 10 });
            _context.UserCourses.Add(new UserCourse { UserId = 1, CourseId = 10 });

            await _context.SaveChangesAsync();


            var result = await _controller.GetDetailsByRoleUser(role, 1);

            var jsonResult = result.Should().BeOfType<JsonResult>().Subject;
            jsonResult.Value.Should().NotBeNull();
        }

        [Fact]
        public async Task GetCourseTasks_ReturnsJsonResult_WithTasks()
        {
            var mentor = new User { Id = 1, Name = "Mentor", Surname = "Test" };
            var course = new Course { Id = 1, Name = "Kurs" };
            var task = new Onboarding.Models.Task { Id = 1, Title = "Zadanie", Description = "Test", CourseId = 1, Mentor = mentor };

            _context.Users.Add(mentor);
            _context.Courses.Add(course);
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            var result = await _controller.GetCourseTasks(1);

            var jsonResult = result.Should().BeOfType<JsonResult>().Subject;
            var data = jsonResult.Value.Should().BeAssignableTo<System.Collections.IEnumerable>().Subject;
            data.Cast<object>().Should().HaveCount(1);
        }

        [Fact]
        public async Task GetCourseTasks_ReturnsJsonNull_WhenCourseNotFound()
        {

            var result = await _controller.GetCourseTasks(999);

            var jsonResult = result.Should().BeOfType<JsonResult>().Subject;
            jsonResult.Value.Should().BeNull();
        }

        [Fact]
        public async Task GetNewUserDetails_ReturnsJsonResult_WithAllDetails()
        {
            var buddy = new User { Id = 2, Name = "Pan", Surname = "Buddy" };
            var user = new User { Id = 1, Name = "Nowy", Surname = "User", Buddy = buddy, BuddyId = 2 };
            var course = new Course { Id = 10, Name = "Kurs testowy" };
            var task = new Onboarding.Models.Task { Id = 20, Title = "Test Task", Description = "Test", CourseId = 10 };

            _context.Users.AddRange(user, buddy);
            _context.Courses.Add(course);
            _context.Tasks.Add(task);
            _context.UserCourses.Add(new UserCourse { UserId = 1, CourseId = 10 });
            _context.UserTasks.Add(new UserTask { UserId = 1, TaskId = 20, Status = StatusTask.Completed, Grade = "5", Container = "TestContainer" });

            await _context.SaveChangesAsync();

            var result = await _controller.GetNewUserDetails(1);

            var jsonResult = result.Should().BeOfType<JsonResult>().Subject;
            jsonResult.Value.Should().NotBeNull();

            var json = System.Text.Json.JsonSerializer.Serialize(jsonResult.Value);

            using var doc = System.Text.Json.JsonDocument.Parse(json);
            var root = doc.RootElement;

            root.GetProperty("UserId").GetInt32().Should().Be(1);
            root.GetProperty("UserName").GetString().Should().Be("Nowy User");
            root.GetProperty("BuddyName").GetString().Should().Be("Pan Buddy");
        }

        [Fact]
        public async Task GetNewUserDetails_ReturnsNotFound_WhenUserDoesNotExist()
        {
            // Act
            var result = await _controller.GetNewUserDetails(99);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
