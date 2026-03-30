using Xunit;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Onboarding.Controllers;
using Onboarding.Data;
using Onboarding.Models;
using Onboarding.Interfaces;
using Onboarding.Hubs;
using System.Security.Claims;
using Task = System.Threading.Tasks.Task;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Onboarding.Data.Enums;

namespace OnboardingXUnitTests.Unit.Controllers
{
    public class BuddyControllerTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly BuddyController _controller;
        private readonly UserManager<User> _userManager;
        private readonly IHubContext<ChatHub> _chatHub;

        public BuddyControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);
            _context.Database.EnsureCreated();

            _userManager = A.Fake<UserManager<User>>(x => x.WithArgumentsForConstructor(() => new UserManager<User>(A.Fake<IUserStore<User>>(), null, null, null, null, null, null, null, null)));
            _chatHub = A.Fake<IHubContext<ChatHub>>();

            _controller = new BuddyController(_context, _userManager, _chatHub);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "testuser"),
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Role, "Buddy")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
        }

        [Fact]
        public void BuddyPanel_ReturnsViewResult()
        {
            var result = _controller.BuddyPanel();

            result.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public async Task Newbies_ReturnsOnlyAssignedNewbies_WhenSearchTermIsProvided()
        {
            var currentUser = new User { Id = 1, UserName = "testuser" };
            var assignedNewbie = new User { Id = 10, Name = "Marek", Surname = "B¹czek", Buddy = currentUser };
            var otherNewbie = new User { Id = 11, Name = "Anna", Surname = "Nowak", Buddy = null };

            _context.Users.AddRange(assignedNewbie, otherNewbie);
            await _context.SaveChangesAsync();

            A.CallTo(() => _userManager.GetUserAsync(_controller.User)).Returns(currentUser);

            var result = await _controller.Newbies("Marek");

            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeAssignableTo<IEnumerable<User>>().Subject;

            model.Should().ContainSingle();
            model.First().Name.Should().Be("Marek");
            _controller.ViewData["SearchTerm"].Should().Be("marek"); 
        }

        [Fact]
        public async Task SendFeedbackToMentor_ReturnsError_WhenContentIsEmpty()
        {
            _controller.TempData = A.Fake<ITempDataDictionary>();

            var result = await _controller.SendFeedbackToMentor(10, 1, "");

            var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Newbies");
            _controller.TempData["Error"].Should().Be("Feedback nie moze byc pusty.");
        }

        [Fact]
        public async Task SendFeedbackToMentor_Success_SavesMessageAndCallsSignalR()
        {
            _controller.TempData = A.Fake<ITempDataDictionary>();
            var mentor = new User { Id = 5, Name = "Mentor" };

            var course = new Course
            {
                Id = 1,
                Name = "Testowy Kurs", 
                Mentor = mentor,
                MentorId = 5
            };

            _context.Users.Add(mentor);
            _context.Courses.Add(course);
            await _context.SaveChangesAsync(); 

            var clientsProxy = A.Fake<IClientProxy>();
            A.CallTo(() => _chatHub.Clients.Group(A<string>._)).Returns(clientsProxy);

            var result = await _controller.SendFeedbackToMentor(10, 1, "Wszystko super!");

            _context.Messages.Should().Contain(m => m.Content == "Wszystko super!" && m.ReceiverId == 5);

            A.CallTo(() => clientsProxy.SendCoreAsync("ReceiveMessage", A<object[]>._, default))
                .MustHaveHappenedOnceExactly();

            var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("Newbies");
            _controller.TempData["Success"].Should().Be("Feedback zostal wysany do mentora.");
        }

        [Fact]
        public async Task TaskStatus_ReturnsViewWithTasks_IncludingDebugTask()
        {
            var currentUser = new User { Id = 1, Email = "buddy@test.com", UserName = "buddy@test.com" };
            var newbieDebug = new User
            {
                Id = 2,
                Email = "nowy1@mail.com",
                UserName = "nowy1@mail.com",
                Buddy = currentUser
            };

            _context.Users.AddRange(currentUser, newbieDebug);

            var course = new Course { Id = 1, Name = "Kurs C#" };

            var taskBase = new Onboarding.Models.Task
            {
                Id = 1,
                Title = "Zadanie 1",
                Description = "Opis zadania", // To naprawia b³¹d
                Course = course
            };

            var userTask = new UserTask
            {
                user = newbieDebug,
                Task = taskBase,
                Status = StatusTask.InProgress,
                Container = "TestContainer",
                Grade = "brak"
            };

            _context.UserTasks.Add(userTask);
            await _context.SaveChangesAsync();

            A.CallTo(() => _userManager.GetUserAsync(_controller.User)).Returns(currentUser);
            A.CallTo(() => _userManager.GetUsersInRoleAsync("Nowy")).Returns(new List<User> { newbieDebug });

            var result = await _controller.TaskStatus();

            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeAssignableTo<List<UserTask>>().Subject;

            model.Should().HaveCount(2);
            model.Should().Contain(t => t.Task.Title == "Zadanie 1");
            model.Should().Contain(t => t.Task.Title == "Testowy task");
        }

        [Fact]
        public void Index_ReturnsViewResult()
        {
            // Act
            var result = _controller.Index();

            // Assert
            result.Should().BeOfType<ViewResult>();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
