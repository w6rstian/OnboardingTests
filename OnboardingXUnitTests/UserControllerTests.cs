
using Xunit;
using Moq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Onboarding.Controllers;
using Onboarding.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Logging;

namespace OnboardingXUnitTests
{
	public class UserControllerTests
	{
		private readonly Mock<UserManager<User>> _mockUserManager;
		private readonly Mock<IEmailSender> _mockEmailSender;
		private readonly Mock<IUserStore<User>> _mockUserStore;

		public UserControllerTests()
		{
			_mockUserStore = new Mock<IUserStore<User>>();
			_mockUserManager = new Mock<UserManager<User>>(
				_mockUserStore.Object,
				null,
				new Mock<IPasswordHasher<User>>().Object,
				new IUserValidator<User>[0],
				new IPasswordValidator<User>[0],
				new Mock<ILookupNormalizer>().Object,
				new Mock<IdentityErrorDescriber>().Object,
				null,
				new Mock<ILogger<UserManager<User>>>().Object
			);
			_mockEmailSender = new Mock<IEmailSender>();
		}

		private UserController CreateControllerWithUserContext(User user)
		{
			var controller = new UserController(_mockUserManager.Object, _mockEmailSender.Object, _mockUserStore.Object);

			var userClaims = new ClaimsPrincipal(new ClaimsIdentity(new[]
			{
				new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
			}, "mock"));

			controller.ControllerContext = new ControllerContext
			{
				HttpContext = new DefaultHttpContext { User = userClaims }
			};

			return controller;
		}

		[Fact]
		public async System.Threading.Tasks.Task MyAccount_Get_ReturnsViewWithUser()
		{
			var user = new User { Id = 123, Name = "John" };
			_mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(user);
			var controller = CreateControllerWithUserContext(user);

			var result = await controller.MyAccount();

			var viewResult = Assert.IsType<ViewResult>(result);
			Assert.Equal(user, viewResult.Model);
		}

		[Fact]
		public async System.Threading.Tasks.Task MyAccount_Post_ValidData_UpdatesUserAndRedirects()
		{
			// Arrange
			var user = new User { Id = 123, Email = "test@example.com" };

			_mockUserManager
				.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
				.ReturnsAsync(user);

			_mockUserManager
				.Setup(x => x.UpdateAsync(It.IsAny<User>()))
				.ReturnsAsync(IdentityResult.Success);

			_mockUserManager
				.Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<User>()))
				.ReturnsAsync("fake-code");

			_mockUserManager
				.Setup(x => x.GetUserIdAsync(It.IsAny<User>()))
				.ReturnsAsync(user.Id.ToString());

			_mockEmailSender
				.Setup(x => x.SendEmailAsync(
					It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()
				))
				.Returns(System.Threading.Tasks.Task.CompletedTask);

			var controller = CreateControllerWithUserContext(user);

			
			controller.ControllerContext.HttpContext.Request.Scheme = "https";
			controller.ControllerContext.HttpContext.Request.Host = new HostString("localhost");

			
			var mockUrlHelper = new Mock<IUrlHelper>();
			mockUrlHelper
				.Setup(x => x.Action(It.IsAny<UrlActionContext>()))
				.Returns("http://dummy-confirmation-url");
			controller.Url = mockUrlHelper.Object;

			// Act
			var result = await controller.MyAccount("John", "Doe", "test@example.com", "123456789", "IT", "Developer");

			// Assert
			var redirect = Assert.IsType<RedirectToActionResult>(result);
			Assert.Equal("MyAccount", redirect.ActionName);
		}



	}
}
