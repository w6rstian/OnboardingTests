using System;
using System.Security.Claims;
using System.Threading.Tasks;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Onboarding.Controllers;
using Onboarding.Models;
using Onboarding.Interfaces;
using Xunit;
using Task = System.Threading.Tasks.Task;

namespace OnboardingXUnitTests.Controllers
{
    public class UserControllerTests
    {
        private readonly UserController _controller;
        private readonly UserManager<User> _fakeUserManager;
        private readonly IEmailSender _fakeEmailSender;
        private readonly IUserStore<User> _fakeUserStore;

        public UserControllerTests()
        {
            _fakeUserStore = A.Fake<IUserStore<User>>();
            _fakeUserManager = A.Fake<UserManager<User>>(x => x.WithArgumentsForConstructor(
                new object[] { _fakeUserStore, null, null, null, null, null, null, null, null }));
            _fakeEmailSender = A.Fake<IEmailSender>();

            _controller = new UserController(_fakeUserManager, _fakeEmailSender, _fakeUserStore);

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
        public void MainPage_ReturnsViewResult()
        {
            // Act
            var result = _controller.MainPage();

            // Assert
            result.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public void UserPanel_ReturnsViewResult()
        {
            // Act
            var result = _controller.UserPanel();

            // Assert
            result.Should().BeOfType<ViewResult>();
        }
    }
}
