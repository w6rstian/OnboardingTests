using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Onboarding.Controllers;
using Onboarding.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Routing;

namespace OnboardingXUnitTests
{
    public class HRControllerTests
    {
        private Mock<UserManager<User>> _mockUserManager;
        private Mock<IEmailSender> _mockEmailSender;
        private Mock<IUserStore<User>> _mockUserStore;

        public HRControllerTests()
        {
            _mockUserStore = new Mock<IUserStore<User>>();
            _mockEmailSender = new Mock<IEmailSender>();
            _mockUserManager = MockUserManager();
        }

        private HRController CreateController()
        {
            return new HRController(_mockUserManager.Object, _mockEmailSender.Object, _mockUserStore.Object);
        }

        private Mock<UserManager<User>> MockUserManager()
        {
            var store = new Mock<IUserStore<User>>();
            return new Mock<UserManager<User>>(
                store.Object, null, null, null, null, null, null, null, null);
        }

        [Fact]
        public void HRPanel_ReturnsView()
        {
            // Arrange
            var controller = CreateController();

            // Act
            var result = controller.HRPanel();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void CreateEmployee_Get_ReturnsView()
        {
            // Arrange
            var controller = CreateController();

            // Act
            var result = controller.CreateEmployee();

            // Assert
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async System.Threading.Tasks.Task CreateEmployee_Post_InvalidModel_ReturnsViewWithErrors()
        {
            // Arrange
            var controller = CreateController();

            // Act
            var result = await controller.CreateEmployee("", "", "");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(controller.ModelState.IsValid);
        }

        [Fact]
        public async System.Threading.Tasks.Task CreateEmployee_Post_CreationFails_ShowsErrors()
        {
            // Arrange
            var controller = CreateController();

            _mockUserManager.Setup(um => um.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Creation failed." }));

            _mockUserStore.Setup(us => us.SetUserNameAsync(It.IsAny<User>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(System.Threading.Tasks.Task.CompletedTask);

            var result = await controller.CreateEmployee("Jane", "Doe", "jane@example.com");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(controller.ModelState.IsValid);
            Assert.Contains(controller.ModelState, kvp => kvp.Value.Errors.Count > 0);
        }
    }
}