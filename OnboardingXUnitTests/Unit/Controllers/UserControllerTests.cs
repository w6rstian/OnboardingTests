using System;
using System.Security.Claims;
using System.Collections.Generic;
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
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.Routing;

namespace OnboardingXUnitTests.Unit.Controllers
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

            var fakeUrlHelper = A.Fake<IUrlHelper>();
            _controller.Url = fakeUrlHelper;

            var tempDataProvider = A.Fake<ITempDataProvider>();
            var tempDataDictionaryFactory = new TempDataDictionaryFactory(tempDataProvider);
            _controller.TempData = tempDataDictionaryFactory.GetTempData(new DefaultHttpContext());

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

        // New tests - MyAccount
        [Fact]
        public async Task MyAccount_Get_ReturnsViewWithUser()
        {
            // Arrange - checks if returns user
            var testUser = new User { Name = "Jan" };
            A.CallTo(() => _fakeUserManager.GetUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult(testUser));

            // Act
            var result = await _controller.MyAccount();

            // Assert 
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().Be(testUser);
        }

        [Fact]
        public async Task MyAccount_Post_ReturnsNotFound_WhenUserNull()
        {
            // Arrange - checks whether user is null
            A.CallTo(() => _fakeUserManager.GetUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult<User>(null));

            // Act
            var result = await _controller.MyAccount("Jan", "K", "j@j.pl", "123", "IT", "Dev");

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task MyAccount_Post_ReturnsRedirect_OnSuccess()
        {
            // Arrange
            var testUser = new User { Id = 1, Email = "test@test.com" };
            A.CallTo(() => _fakeUserManager.GetUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult(testUser));
            A.CallTo(() => _fakeUserManager.UpdateAsync(A<User>._)).Returns(Task.FromResult(IdentityResult.Success));
            A.CallTo(() => _fakeUserManager.GetUserIdAsync(A<User>._)).Returns(Task.FromResult(testUser.Id.ToString()));
            A.CallTo(() => _fakeUserManager.GenerateEmailConfirmationTokenAsync(A<User>._)).Returns(Task.FromResult("token123"));

            // Act
            var result = await _controller.MyAccount("Jan", "K", "j@j.pl", "123", "IT", "Dev");

            // Assert
            var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("MyAccount");

            // Checks if mail was sent
            A.CallTo(() => _fakeEmailSender.SendEmailAsync(A<string>._, A<string>._, A<string>._)).MustHaveHappened();
        }

        [Fact]
        public async Task MyAccount_Post_ReturnsView_WhenUpdateFails()
        {
            // Arrange
            var testUser = new User { Id = 1 };
            var identityError = IdentityResult.Failed(new IdentityError { Description = "Błąd zapisu" });
            A.CallTo(() => _fakeUserManager.GetUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult(testUser));
            A.CallTo(() => _fakeUserManager.UpdateAsync(A<User>._)).Returns(identityError);

            // Act
            var result = await _controller.MyAccount("Jan", "K", "j@j.pl", "123", "IT", "Dev");

            // Assert
            result.Should().BeOfType<ViewResult>();
            _controller.ModelState.ErrorCount.Should().BeGreaterThan(0);
        }

        [Theory] // 3 tests
        [InlineData("", "Kowalski")]
        [InlineData("Jan", "")]
        [InlineData(null, null)]
        public async Task MyAccount_Post_HandlesMissingNames(string n, string ln)
        {
            // Arrange
            var testUser = new User { Id = 1 };
            A.CallTo(() => _fakeUserManager.GetUserAsync(A<ClaimsPrincipal>._)).Returns(testUser);
            A.CallTo(() => _fakeUserManager.UpdateAsync(A<User>._)).Returns(IdentityResult.Success);
            A.CallTo(() => _fakeUserManager.GetUserIdAsync(A<User>._)).Returns("1");

            // Act
            var result = await _controller.MyAccount(n, ln, "test@o2.pl", "111", "IT", "Dev");

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
        }

        [Fact]
        public async Task MyAccount_Post_ReturnsRedirect_EvenIfUserIdIsNull()
        {
            // Arrange - checks if GetUserIdAsync throws error/null
            var testUser = new User { Id = 1 };
            A.CallTo(() => _fakeUserManager.GetUserAsync(A<ClaimsPrincipal>._)).Returns(testUser);
            A.CallTo(() => _fakeUserManager.UpdateAsync(A<User>._)).Returns(IdentityResult.Success);
            A.CallTo(() => _fakeUserManager.GetUserIdAsync(A<User>._)).Returns(Task.FromResult<string>(null));

            // Act
            var result = await _controller.MyAccount("J", "K", "j@j.pl", "1", "D", "P");

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
        }

        [Fact]
        public async Task MyAccount_Post_AddsMultipleErrorsToModelState()
        {
            // Arrange - checks if UpdateAsync throws many errors
            var testUser = new User { Id = 1 };
            var identityError = IdentityResult.Failed(
                new IdentityError { Description = "Error 1" },
                new IdentityError { Description = "Error 2" }
            );
            A.CallTo(() => _fakeUserManager.GetUserAsync(A<ClaimsPrincipal>._)).Returns(testUser);
            A.CallTo(() => _fakeUserManager.UpdateAsync(A<User>._)).Returns(identityError);

            // Act
            await _controller.MyAccount("a", "b", "c", "d", "e", "f");

            // Assert
            _controller.ModelState.ErrorCount.Should().Be(2);
        }

        // Tests of UserSettings
        [Fact]
        public async Task UserSettings_Post_UpdatesData_AndRedirects()
        {
            // Arrange
            var testUser = new User { Name = "Stare" };
            var model = new User { Name = "Nowe", Surname = "Nazwisko", PhoneNumber = "999" };

            A.CallTo(() => _fakeUserManager.GetUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult(testUser));
            A.CallTo(() => _fakeUserManager.UpdateAsync(A<User>._)).Returns(Task.FromResult(IdentityResult.Success));

            // Act
            var result = await _controller.UserSettings(model);

            // Assert
            testUser.Name.Should().Be("Nowe");
            result.Should().BeOfType<RedirectToActionResult>();
        }

        [Fact]
        public async Task UserSettings_Post_ReturnsView_WhenUpdateFails()
        {
            // Arrange
            var testUser = new User { Name = "Stare" };
            var identityError = IdentityResult.Failed(new IdentityError { Description = "Błąd bazy" });

            A.CallTo(() => _fakeUserManager.GetUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult(testUser));
            A.CallTo(() => _fakeUserManager.UpdateAsync(A<User>._)).Returns(Task.FromResult(identityError));

            // Act
            var result = await _controller.UserSettings(new User());

            // Assert
            result.Should().BeOfType<ViewResult>();
            _controller.ModelState.IsValid.Should().BeFalse(); // Checks if error is in ModelState
        }

        [Fact]
        public async Task UserSettings_Get_ReturnsViewWithUser()
        {
            // Arrange
            var testUser = new User { Id = 1, Name = "Marek" };
            A.CallTo(() => _fakeUserManager.GetUserAsync(A<ClaimsPrincipal>._)).Returns(testUser);

            // Act
            var result = await _controller.UserSettings();

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().Be(testUser);
        }

        [Fact]
        public async Task UserSettings_Post_ReturnsNotFound_WhenUserIsNull()
        {
            // Arrange
            A.CallTo(() => _fakeUserManager.GetUserAsync(A<ClaimsPrincipal>._)).Returns(Task.FromResult<User>(null));

            // Act
            var result = await _controller.UserSettings(new User());

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task UserSettings_Post_SetsTempData_OnSuccess()
        {
            // Arrange
            var testUser = new User { Id = 1 };
            A.CallTo(() => _fakeUserManager.GetUserAsync(A<ClaimsPrincipal>._)).Returns(testUser);
            A.CallTo(() => _fakeUserManager.UpdateAsync(A<User>._)).Returns(IdentityResult.Success);

            // Act
            await _controller.UserSettings(new User { Name = "Nowe" });

            // Assert
            _controller.TempData["SuccessMessage"].Should().Be("Dane użytkownika zostały zaktualizowane.");
        }

        [Fact]
        public async Task UserSettings_Post_UpdatesAllFieldsCorrectly()
        {
            // Arrange
            var testUser = new User { Id = 1 };
            var model = new User
            {
                Name = "A",
                Surname = "B",
                PhoneNumber = "123",
                Department = "IT",
                Position = "Dev"
            };
            A.CallTo(() => _fakeUserManager.GetUserAsync(A<ClaimsPrincipal>._)).Returns(testUser);
            A.CallTo(() => _fakeUserManager.UpdateAsync(A<User>._)).Returns(IdentityResult.Success);

            // Act
            await _controller.UserSettings(model);

            // Assert
            testUser.Surname.Should().Be("B");
            testUser.PhoneNumber.Should().Be("123");
            testUser.Department.Should().Be("IT");
            testUser.Position.Should().Be("Dev");
        }
    }
}
