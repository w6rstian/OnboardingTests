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
using Onboarding.Data.Enums;
using Onboarding.ViewModels;
using System.Security.Claims;
using System.Linq;
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
        private readonly User _currentUser;

        public CalendarControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);
            _context.Database.EnsureCreated();

            _currentUser = new User { Id = 1, Name = "Test", Surname = "User", Email = "test@user.com" };
            var otherUser = new User { Id = 2, Name = "Other", Surname = "User", Email = "other@user.com" };
            _context.Users.AddRange(_currentUser, otherUser);
            _context.SaveChanges();

            _userManager = A.Fake<UserManager<User>>(x => x.WithArgumentsForConstructor(() => new UserManager<User>(A.Fake<IUserStore<User>>(), null, null, null, null, null, null, null, null)));

            A.CallTo(() => _userManager.GetUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult((User?)_currentUser));

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
        public async Task CreateMeeting_Get_ReturnsViewWithModelAndFilteredUsers()
        {
            // Act
            var result = await _controller.CreateMeeting(MeetingType.BuddyCheckIn);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeOfType<MeetingViewModel>().Subject;
            model.Type.Should().Be(MeetingType.BuddyCheckIn);
            model.AllUsers.Should().HaveCount(1);
            model.AllUsers.First().Value.Should().Be("2");
        }

        [Fact]
        public async Task CreateMeeting_Post_InvalidModel_ReturnsViewWithModelAndAllUsers()
        {
            // Arrange
            var model = new MeetingViewModel { Type = MeetingType.General };
            _controller.ModelState.AddModelError("Title", "Required");

            // Act
            var result = await _controller.CreateMeeting(model);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var returnedModel = viewResult.Model.Should().BeOfType<MeetingViewModel>().Subject;
            returnedModel.AllUsers.Should().HaveCount(1);
            returnedModel.AllUsers.First().Value.Should().Be("2");
        }

        [Fact]
        public async Task CreateMeeting_Post_ValidModel_AddsMeetingToDatabaseAndRedirects()
        {
            // Arrange
            var model = new MeetingViewModel
            {
                Title = "Meeting Title",
                Start = DateTime.Now,
                End = DateTime.Now.AddHours(1),
                Type = MeetingType.General,
                SelectedUsersIds = new List<string> { "2" }
            };

            // Act
            var result = await _controller.CreateMeeting(model);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should().Be("Index");
            _context.Meetings.Should().HaveCount(1);
            var meeting = _context.Meetings.Include(m => m.Participants).First();
            meeting.Title.Should().Be("Meeting Title");
            meeting.Participants.Should().HaveCount(1);
            meeting.Participants.First().UserId.Should().Be(2);
        }

        [Fact]
        public async Task GetEvents_ReturnsEventsForCurrentUser()
        {
            // Arrange
            var otherUser = _context.Users.Find(2);
            var meeting = new Meeting
            {
                OrganizerId = _currentUser.Id,
                Title = "Organized by me",
                Start = DateTime.Now,
                End = DateTime.Now.AddHours(1),
                Type = MeetingType.BuddyCheckIn
            };
            meeting.Participants.Add(new MeetingParticipant { User = otherUser, UserId = otherUser.Id });
            _context.Meetings.Add(meeting);

            var meeting2 = new Meeting
            {
                OrganizerId = otherUser.Id,
                Title = "Participating in",
                Start = DateTime.Now,
                End = DateTime.Now.AddHours(1),
                Type = MeetingType.General
            };
            meeting2.Participants.Add(new MeetingParticipant { User = _currentUser, UserId = _currentUser.Id });
            _context.Meetings.Add(meeting2);

            var meeting3 = new Meeting // unrelated
            {
                OrganizerId = 99,
                Title = "Unrelated",
                Start = DateTime.Now,
                End = DateTime.Now.AddHours(1),
                Type = MeetingType.General
            };
            _context.Meetings.Add(meeting3);

            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetEvents(null);

            // Assert
            var jsonResult = result.Should().BeOfType<JsonResult>().Subject;
            var events = jsonResult.Value as System.Collections.IEnumerable;
            events.Cast<object>().Should().HaveCount(5);
        }

        [Fact]
        public async Task GetEvents_WithSpecificType_FiltersEvents()
        {
            // Arrange
            var meeting = new Meeting
            {
                OrganizerId = _currentUser.Id,
                Title = "General Meeting",
                Start = DateTime.Now,
                End = DateTime.Now.AddHours(1),
                Type = MeetingType.General
            };
            _context.Meetings.Add(meeting);
            await _context.SaveChangesAsync();

            // Act
            var result = await _controller.GetEvents("General");

            // Assert
            var jsonResult = result.Should().BeOfType<JsonResult>().Subject;
            var events = jsonResult.Value as System.Collections.IEnumerable;
            
            // Should contain 1 DB event (General) and 2 example events (Spotkanie zespołu, Lunch z klientem)
            events.Cast<object>().Should().HaveCount(3);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
