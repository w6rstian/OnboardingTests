using Xunit;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.Routing;
using Onboarding.Controllers;
using Onboarding.Models;
using Onboarding.Interfaces;
using System.Security.Claims;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace OnboardingXUnitTests.Controllers
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

            var httpContext = new DefaultHttpContext() { User = user };
            httpContext.Request.Scheme = "https";

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = httpContext
            };

            _controller.TempData = new TempDataDictionary(httpContext, A.Fake<ITempDataProvider>());

            var urlHelperFake = A.Fake<IUrlHelper>();
            A.CallTo(() => urlHelperFake.Action(A<UrlActionContext>.Ignored)).Returns("https://fakeurl.com");
            _controller.Url = urlHelperFake;
        }

        [Fact]
        public void HRPanel_ReturnsViewResult()
        {
            var result = _controller.HRPanel();

            result.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public void CreateEmployeeGet_ReturnsViewResult()
        {
            var result = _controller.CreateEmployee();

            result.Should().BeOfType<ViewResult>();
        }

        [Theory]
        [InlineData("", "Kowalski", "test@test.com")]
        [InlineData("Jan", "", "test@test.com")]
        [InlineData("Jan", "Kowalski", "")]
        [InlineData(null, null, null)]
        public async Task CreateEmployeePost_EmptyFields_ReturnsViewWithError(string name, string lastname, string email)
        {
            var result = await _controller.CreateEmployee(name, lastname, email);

            result.Should().BeOfType<ViewResult>();
            _controller.ModelState.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task CreateEmployeePost_ValidData_CreatesUserSendsEmailAndRedirects()
        {
            A.CallTo(() => _userManager.CreateAsync(A<User>.Ignored, A<string>.Ignored))
                .Returns(IdentityResult.Success);

            A.CallTo(() => _userManager.GetUserIdAsync(A<User>.Ignored))
                .Returns("fake-user-id");

            A.CallTo(() => _userManager.GenerateEmailConfirmationTokenAsync(A<User>.Ignored))
                .Returns("fake-token");

            var result = await _controller.CreateEmployee("Jan", "Kowalski", "jan@test.com");

            var redirectResult = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirectResult.ActionName.Should().Be("CreateEmployee");
            _controller.TempData["SuccessMessage"].Should().Be("Employee created successfully. A confirmation email has been sent.");

            A.CallTo(() => _userStore.SetUserNameAsync(A<User>.Ignored, "jan@test.com", CancellationToken.None))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _userManager.SetEmailAsync(A<User>.Ignored, "jan@test.com"))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _emailSender.SendEmailAsync("jan@test.com", "Confirm your email", A<string>.Ignored))
                .MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task CreateEmployeePost_CreationFails_ReturnsViewWithErrors()
        {
            var error = new IdentityError { Description = "Password too weak" };
            A.CallTo(() => _userManager.CreateAsync(A<User>.Ignored, A<string>.Ignored))
                .Returns(IdentityResult.Failed(error));

            var result = await _controller.CreateEmployee("Jan", "Kowalski", "jan@test.com");

            result.Should().BeOfType<ViewResult>();
            _controller.ModelState.IsValid.Should().BeFalse();
            _controller.ModelState[string.Empty].Errors[0].ErrorMessage.Should().Be("Password too weak");

            A.CallTo(() => _emailSender.SendEmailAsync(A<string>.Ignored, A<string>.Ignored, A<string>.Ignored))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task CreateEmployeePost_CreationFails_AddsAllIdentityErrorsToModelState()
        {
            var errors = new List<IdentityError>
            {
                new IdentityError { Description = "Has³o jest za krótkie." },
                new IdentityError { Description = "Has³o musi mieæ znak specjalny." }
            };

            A.CallTo(() => _userManager.CreateAsync(A<User>.Ignored, A<string>.Ignored)).Returns(IdentityResult.Failed(errors.ToArray()));

            var result = await _controller.CreateEmployee("Jan", "Kowalski", "jan@test.com");

            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            _controller.ModelState.IsValid.Should().BeFalse();

            _controller.ModelState[string.Empty].Errors.Should().HaveCount(2);
            _controller.ModelState[string.Empty].Errors[0].ErrorMessage.Should().Be("Has³o jest za krótkie.");
            _controller.ModelState[string.Empty].Errors[1].ErrorMessage.Should().Be("Has³o musi mieæ znak specjalny.");
        }

        [Fact]
        public async Task CreateEmployeePost_ValidData_SetsEmailConfirmedToTrue()
        {

            User capturedUser = null;

            A.CallTo(() => _userManager.CreateAsync(A<User>.Ignored, A<string>.Ignored)).Invokes((User u, string p) => capturedUser = u).Returns(IdentityResult.Success);

            A.CallTo(() => _userManager.GetUserIdAsync(A<User>.Ignored)).Returns("1");
            A.CallTo(() => _userManager.GenerateEmailConfirmationTokenAsync(A<User>.Ignored)).Returns("token");


            await _controller.CreateEmployee("Jan", "Kowalski", "jan@test.com");

            capturedUser.Should().NotBeNull();
            capturedUser.Name.Should().Be("Jan");
            capturedUser.Surname.Should().Be("Kowalski");

            capturedUser.EmailConfirmed.Should().BeTrue();
        }

        [Fact]
        public async Task CreateEmployeePost_NullFields_ReturnsViewWithError()
        {
            var result = await _controller.CreateEmployee(null, null, null);

            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            _controller.ModelState.IsValid.Should().BeFalse();
            _controller.ModelState[string.Empty].Errors[0].ErrorMessage.Should().Be("All fields are required.");
        }
    }
}