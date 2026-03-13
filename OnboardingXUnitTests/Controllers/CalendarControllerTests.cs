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
using Onboarding.ViewModels;

namespace OnboardingXUnitTests.Controllers
{
    public class CalendarControllerTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly CalendarController _controller;
        private readonly UserManager<User> _userManager;

        public CalendarControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);
            _context.Database.EnsureCreated();

            _userManager = A.Fake<UserManager<User>>(x => x.WithArgumentsForConstructor(() => new UserManager<User>(A.Fake<IUserStore<User>>(), null, null, null, null, null, null, null, null)));

            _controller = new CalendarController(_context, _userManager);

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
        public void Index_ReturnsViewResult()
        {
            // Act
            var result = _controller.Index();

            // Assert
            result.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public async Task CreateMeeting_GET_ReturnsViewWithModel()
        {
            var currentUser = new User { Id = 1, Email = "test@test.com" };
            var otherUser = new User { Id = 2, Email = "other@test.com", Name = "Jan", Surname = "Kowalski" };

            _context.Users.AddRange(currentUser, otherUser);
            await _context.SaveChangesAsync();

            A.CallTo(() => _userManager.GetUserAsync(A<ClaimsPrincipal>._)).Returns(currentUser);

            var result = await _controller.CreateMeeting(MeetingType.General);

            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeOfType<MeetingViewModel>().Subject;
            model.Type.Should().Be(MeetingType.General);
            model.AllUsers.Should().NotBeEmpty();
        }

        [Fact]
        public async Task CreateMeeting_POST_InvalidModel_ReturnsViewWithAllUsers()
        {
            var currentUser = new User { Id = 1, Email = "test@test.com" };
            var otherUser = new User { Id = 2, Email = "other@test.com", Name = "Jan", Surname = "Kowalski" };
            _context.Users.AddRange(currentUser, otherUser);
            await _context.SaveChangesAsync();

            A.CallTo(() => _userManager.GetUserAsync(A<ClaimsPrincipal>._)).Returns(currentUser);

            _controller.ModelState.AddModelError("Title", "Required");
            var model = new MeetingViewModel { SelectedUsersIds = new List<string>() };

            var result = await _controller.CreateMeeting(model);

            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var returnedModel = viewResult.Model.Should().BeOfType<MeetingViewModel>().Subject;
            returnedModel.AllUsers.Should().NotBeEmpty();
        }

        [Fact]
        public async Task CreateMeeting_POST_ValidModel_RedirectsToIndex()
        {
            var currentUser = new User { Id = 1, Email = "test@test.com" };
            A.CallTo(() => _userManager.GetUserAsync(A<ClaimsPrincipal>._)).Returns(currentUser);

            var model = new MeetingViewModel
            {
                Title = "Spotkanie",
                Start = DateTime.Now.AddDays(1),
                End = DateTime.Now.AddDays(1).AddHours(1), 
                SelectedUsersIds = new List<string> { "2" }, 
                Type = MeetingType.General
            };

            var result = await _controller.CreateMeeting(model);

            result.Should().BeOfType<RedirectToActionResult>()
                  .Which.ActionName.Should().Be("Index");

            _context.Meetings.Should().Contain(m => m.Title == "Spotkanie");
        }

        [Fact]
        public async Task GetEvents_ReturnsJsonResult_WithMeetingsAndExamples()
        {
            var currentUser = new User { Id = 1, Email = "test@test.com" };
            A.CallTo(() => _userManager.GetUserAsync(A<ClaimsPrincipal>._)).Returns(currentUser);

            var result = await _controller.GetEvents("General");

            var jsonResult = result.Should().BeOfType<JsonResult>().Subject;

            var data = jsonResult.Value.Should().BeAssignableTo<System.Collections.IEnumerable>().Subject;
            data.Cast<object>().Should().NotBeEmpty();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
