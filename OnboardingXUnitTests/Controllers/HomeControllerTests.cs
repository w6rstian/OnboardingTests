using Xunit;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Onboarding.Controllers;
using Onboarding.Models;
using Onboarding.Interfaces;
using System.Security.Claims;

namespace OnboardingXUnitTests
{
    public class HomeControllerTests
    {
        private readonly HomeController _controller;
        private readonly ILogger<HomeController> _logger;

        public HomeControllerTests()
        {
            _logger = A.Fake<ILogger<HomeController>>();
            _controller = new HomeController(_logger);

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
    }
}
