using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Moq;
using Onboarding.Controllers;
using Onboarding.Data;
using Onboarding.Data.Enums;
using Onboarding.Hubs;
using Onboarding.Models;
using System.Security.Claims;
using Xunit;

namespace OnboardingXUnitTests
{
    public class BuddyControllerTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Mock<IHubContext<ChatHub>> _mockHubContext;
        private readonly BuddyController _controller;
        private readonly Mock<IHubClients> _mockClients;
        private readonly Mock<IClientProxy> _mockClientProxy;

        public BuddyControllerTests()
        {
            // Konfiguracja In-Memory Database
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);

            // Mock UserManager
            var userStore = new Mock<IUserStore<User>>();
            _mockUserManager = new Mock<UserManager<User>>(
                userStore.Object, null, null, null, null, null, null, null, null);

            // Mock SignalR Hub
            _mockHubContext = new Mock<IHubContext<ChatHub>>();
            _mockClients = new Mock<IHubClients>();
            _mockClientProxy = new Mock<IClientProxy>();

            _mockHubContext.Setup(h => h.Clients).Returns(_mockClients.Object);
            _mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(_mockClientProxy.Object);

            _controller = new BuddyController(_context, _mockUserManager.Object, _mockHubContext.Object);
        }

        [Fact]
        public void Index_ReturnsViewResult()
        {
            // Act
            var result = _controller.Index();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void BuddyPanel_ReturnsViewResult()
        {
            // Act
            var result = _controller.BuddyPanel();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async System.Threading.Tasks.Task Newbies_WithoutSearchTerm_ReturnsAllNewbiesForCurrentBuddy()
        {
            // Arrange
            var buddy = new User { Id = 1, Email = "buddy@test.com", Name = "Buddy", Surname = "Test" };
            var newbie1 = new User { Id = 2, Email = "newbie1@test.com", Name = "Newbie1", Surname = "Test", Buddy = buddy };
            var newbie2 = new User { Id = 3, Email = "newbie2@test.com", Name = "Newbie2", Surname = "Test", Buddy = buddy };
            var otherNewbie = new User { Id = 4, Email = "other@test.com", Name = "Other", Surname = "Test" };

            _context.Users.AddRange(buddy, newbie1, newbie2, otherNewbie);
            await _context.SaveChangesAsync();

            _mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                           .ReturnsAsync(buddy);

            // Act
            var result = await _controller.Newbies(null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<User>>(viewResult.Model);
            Assert.Equal(2, model.Count);
            Assert.Contains(model, u => u.Id == 2);
            Assert.Contains(model, u => u.Id == 3);
        }

        [Fact]
        public async System.Threading.Tasks.Task Newbies_WithSearchTerm_ReturnsFilteredNewbies()
        {
            // Arrange
            var buddy = new User { Id = 1, Email = "buddy@test.com", Name = "Buddy", Surname = "Test" };
            var newbie1 = new User { Id = 2, Email = "john@test.com", Name = "John", Surname = "Doe", Buddy = buddy };
            var newbie2 = new User { Id = 3, Email = "jane@test.com", Name = "Jane", Surname = "Smith", Buddy = buddy };

            _context.Users.AddRange(buddy, newbie1, newbie2);
            await _context.SaveChangesAsync();

            _mockUserManager.Setup(m => m.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                           .ReturnsAsync(buddy);

            // Act
            var result = await _controller.Newbies("john");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<User>>(viewResult.Model);
            Assert.Single(model);
            Assert.Equal("John", model.First().Name);
        }

        [Fact]
        public async System.Threading.Tasks.Task SendFeedbackToMentor_WithEmptyContent_ReturnsErrorMessage()
        {
            // Arrange
            SetupControllerContext();

            // Act
            var result = await _controller.SendFeedbackToMentor(1, 1, "");

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Newbies", redirectResult.ActionName);
            Assert.Equal("Feedback nie może być pusty.", _controller.TempData["Error"]);
        }

        [Fact]
        public async System.Threading.Tasks.Task SendFeedbackToMentor_WithNonExistentCourse_ReturnsErrorMessage()
        {
            // Arrange
            SetupControllerContext();

            // Act
            var result = await _controller.SendFeedbackToMentor(1, 999, "Test feedback");

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Newbies", redirectResult.ActionName);
            Assert.Equal("Kurs lub mentor nie istnieje.", _controller.TempData["Error"]);
        }

        private void SetupControllerContext()
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Name, "test@test.com")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = principal
                }
            };

            // Setup TempData
            _controller.TempData = new Microsoft.AspNetCore.Mvc.ViewFeatures.TempDataDictionary(
                new DefaultHttpContext(),
                Mock.Of<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataProvider>());
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
