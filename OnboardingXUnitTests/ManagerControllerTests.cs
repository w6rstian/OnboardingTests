using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Onboarding.Controllers;
using Onboarding.Models;

namespace OnboardingXUnitTests
{
    public class ManagerControllerTests
    {
        [Fact]
        public void Index_ReturnsViewResult()
        {
            var controller = new ManagerController(null, null);
            var result = controller.Index();
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void ManagerPanel_ReturnsViewResult()
        {
            var controller = new ManagerController(null, null);
            var result = controller.ManagerPanel();
            Assert.IsType<ViewResult>(result);
        }
    }
}
