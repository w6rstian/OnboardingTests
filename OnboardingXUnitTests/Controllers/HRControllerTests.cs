using Xunit;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Onboarding.Controllers;
using Onboarding.Models;
using Onboarding.Interfaces;
using System.Security.Claims;
using Task = System.Threading.Tasks.Task;

namespace OnboardingXUnitTests
{
    public class HRControllerTests
    {
        private readonly HRController _controller;
        private readonly UserManager<User> _userManager;
        private readonly IUserStore<User> _userStore;
        private readonly IEmailSender _emailSender;

        public HRControllerTests()
        {
            _userManager = A.Fake<UserManager<User>>(x => x.WithArgumentsForConstructor(() => new UserManager<User>(A.Fake<IUserStore<User>>(), null, null, null, null, null, null, null, null)));
            _userStore = A.Fake<IUserStore<User>>();
            _emailSender = A.Fake<IEmailSender>();

            _controller = new HRController(_userManager, _emailSender, _userStore);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "testuser"),
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Role, "HR")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
        }

        [Fact]
        public void HRPanel_ReturnsViewResult()
        {
            // Act
            var result = _controller.HRPanel();

            // Assert
            result.Should().BeOfType<ViewResult>();
        }
    }
}
