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

namespace OnboardingXUnitTests
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
