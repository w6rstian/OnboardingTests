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
            // setup mocks
            _fakeUserStore = A.Fake<IUserStore<User>>();
            _fakeUserManager = A.Fake<UserManager<User>>(x => x.WithArgumentsForConstructor(
                new object[] { _fakeUserStore, null, null, null, null, null, null, null, null }));
            _fakeEmailSender = A.Fake<IEmailSender>();

            _controller = new UserController(_fakeUserManager, _fakeEmailSender, _fakeUserStore);

            // setup UrlHelper
            var fakeUrlHelper = A.Fake<IUrlHelper>();
            A.CallTo(() => fakeUrlHelper.Action(A<UrlActionContext>._)).Returns("http://localhost/confirm-email");
            _controller.Url = fakeUrlHelper;

            // setup tempdata
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

        // MainPage and UserPanel tests
        [Fact]
        public void MainPage_ReturnsViewResult()
        {
            var result = _controller.MainPage();
            result.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public void UserPanel_ReturnsViewResult()
        {
            var result = _controller.UserPanel();
            result.Should().BeOfType<ViewResult>();
        }

        // MyAccount Tests
        [Fact]
        public async Task MyAccountGet_ReturnsView_WithCurrentUser()
        {
            // Arrange
            var testUser = new User { Name = "Jan" };
            A.CallTo(() => _fakeUserManager.GetUserAsync(A<ClaimsPrincipal>._)).Returns(testUser);

            // Act
            var result = await _controller.MyAccount();

            // Assert 
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.Model.Should().Be(testUser);
        }

        [Fact]
        public async Task MyAccountPost_WhenUserIsNull_ReturnsNotFound()
        {
            // Arrange
            A.CallTo(() => _fakeUserManager.GetUserAsync(A<ClaimsPrincipal>._)).Returns((User)null!);

            // Act
            var result = await _controller.MyAccount("Jan", "K", "nowy@mail.pl", "123", "IT", "Dev");

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task MyAccountPost_WhenUpdateSucceeds_UpdatesUserSendsEmailAndRedirects()
        {
            // Arrange
            var testUser = new User { Id = 1, Email = "test@test.com" };
            A.CallTo(() => _fakeUserManager.GetUserAsync(A<ClaimsPrincipal>._)).Returns(testUser);
            A.CallTo(() => _fakeUserManager.UpdateAsync(A<User>._)).Returns(Task.FromResult(IdentityResult.Success));
            A.CallTo(() => _fakeUserManager.GetUserIdAsync(A<User>._)).Returns(Task.FromResult(testUser.Id.ToString()));
            A.CallTo(() => _fakeUserManager.GenerateEmailConfirmationTokenAsync(A<User>._)).Returns(Task.FromResult("token123"));

            // Act
            var result = await _controller.MyAccount("Jan", "Kowalski", "nowy@mail.pl", "123", "IT", "Dev");

            // Assert
            testUser.Name.Should().Be("Jan");
            testUser.Surname.Should().Be("Kowalski");
            testUser.Department.Should().Be("IT");

            A.CallTo(() => _fakeEmailSender.SendEmailAsync("nowy@mail.pl", "Confirm your email", A<string>._))
                .MustHaveHappenedOnceExactly();

            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("MyAccount");
        }

        [Fact]
        public async Task MyAccountPost_WhenUpdateFails_AddsErrorsToModelStateAndReturnsView()
        {
            // Arrange
            var testUser = new User { Id = 1 };
            var identityError = IdentityResult.Failed(new IdentityError { Description = "Błąd zapisu w bazie danych" });

            A.CallTo(() => _fakeUserManager.GetUserAsync(A<ClaimsPrincipal>._)).Returns(testUser);
            A.CallTo(() => _fakeUserManager.UpdateAsync(A<User>._)).Returns(identityError);

            // Act
            var result = await _controller.MyAccount("Jan", "K", "j@j.pl", "123", "IT", "Dev");

            // Assert
            result.Should().BeOfType<ViewResult>();
            _controller.ModelState.ErrorCount.Should().Be(1);
            _controller.ModelState.Values.SelectMany(v => v.Errors).Should().Contain(e => e.ErrorMessage == "Błąd zapisu w bazie danych");

            A.CallTo(() => _fakeEmailSender.SendEmailAsync(A<string>._, A<string>._, A<string>._))
                .MustNotHaveHappened();
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
            var fullModel = new User { Name = "Nowe", Surname = "Kowalski", Department = "IT", Position = "Dev", PhoneNumber = "123" };
            await _controller.UserSettings(fullModel);

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
