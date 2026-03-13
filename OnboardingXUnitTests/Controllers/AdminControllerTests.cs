using Xunit;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Onboarding.Controllers;
using Onboarding.Data;
using Onboarding.Models;
using Onboarding.Interfaces;
using System.Security.Claims;
using Task = System.Threading.Tasks.Task;
using Onboarding.ViewModels;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace OnboardingXUnitTests.Controllers
{
    public class AdminControllerTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly AdminController _controller;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;

        public AdminControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);
            _context.Database.EnsureCreated();

            _userManager = A.Fake<UserManager<User>>(x => x.WithArgumentsForConstructor(() => new UserManager<User>(A.Fake<IUserStore<User>>(), null, null, null, null, null, null, null, null)));
            _roleManager = A.Fake<RoleManager<IdentityRole<int>>>(x => x.WithArgumentsForConstructor(() => new RoleManager<IdentityRole<int>>(A.Fake<IRoleStore<IdentityRole<int>>>(), null, null, null, null)));

            _controller = new AdminController(_context, _roleManager, _userManager);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "testuser"),
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Role, "Admin")
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

        [Fact]
        public async Task ManagerRoles_ReturnsViewWithAllUsers_WhenSearchTermIsEmpty()
        {
            var testUsers = new List<User>
            {
                new User { Id = 1, Email = "test1@test.com", Name = "Jan", Surname = "Kowalski", UserName = "test1@test.com" },
                new User { Id = 2, Email = "test2@test.com", Name = "Anna", Surname = "Nowak", UserName = "test2@test.com" }
            };

            _context.Users.AddRange(testUsers);

            var testRoles = new List<IdentityRole<int>>
            {
                new IdentityRole<int> { Id = 1, Name = "Admin", NormalizedName = "ADMIN" },
                new IdentityRole<int> { Id = 2, Name = "User", NormalizedName = "USER" }
            };
            _context.Roles.AddRange(testRoles);
            _context.SaveChanges();

            A.CallTo(() => _userManager.Users).Returns(_context.Users);
            A.CallTo(() => _roleManager.Roles).Returns(_context.Roles);

            A.CallTo(() => _userManager.GetRolesAsync(A<User>._)).Returns(Task.FromResult<IList<string>>(new List<string> { "User" }));

            var result = await _controller.ManageRoles(null);

            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeAssignableTo<List<ManageRolesViewModel>>().Subject;

            model.Should().HaveCount(2);
            model.Any(m => m.User.Email == "test1@test.com").Should().BeTrue();
            model.Any(m => m.User.Email == "test2@test.com").Should().BeTrue();
        }

        [Fact]
        public async Task UpdateRole_ReturnsError_WhenAdminTriesToChangeOwnRole()
        {
            int adminId = 1;
            var adminUser = new User { Id = adminId, UserName = "admin@test.com" };

            _controller.TempData = A.Fake<ITempDataDictionary>(); 

            _context.Users.Add(adminUser);
            await _context.SaveChangesAsync();

            A.CallTo(() => _userManager.FindByIdAsync(adminId.ToString())).Returns(adminUser);
            A.CallTo(() => _userManager.GetRolesAsync(adminUser)).Returns(new List<string> { "Admin" });

            A.CallTo(() => _userManager.GetUserId(_controller.User)).Returns(adminId.ToString());

            var result = await _controller.UpdateRole(adminId, "User");

            var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("ManageRoles");
            _controller.TempData["Error"].Should().Be("Nie mo¿na usun¹æ roli 'Admin' ostatniemu administratorowi.");
        }

        [Fact]
        public async Task UpdateRole_SuccessfullyUpdatesRole_WhenConditionsAreMet()
        {
            int targetUserId = 10;
            int currentAdminId = 1;
            var targetUser = new User { Id = targetUserId, UserName = "user@test.com" };
            var otherAdmin = new User { Id = 2, UserName = "other@admin.com" };

            _context.Users.Add(targetUser);
            await _context.SaveChangesAsync();

            A.CallTo(() => _userManager.FindByIdAsync(targetUserId.ToString())).Returns(targetUser);
            A.CallTo(() => _userManager.GetRolesAsync(targetUser)).Returns(new List<string> { "User" });

            A.CallTo(() => _userManager.GetUserId(_controller.User)).Returns(currentAdminId.ToString());

            var allAdmins = new List<User> { new User { Id = 1 }, otherAdmin };
            A.CallTo(() => _userManager.GetUsersInRoleAsync("Admin")).Returns(allAdmins);

            var result = await _controller.UpdateRole(targetUserId, "Admin");

            A.CallTo(() => _userManager.RemoveFromRolesAsync(targetUser, A<IEnumerable<string>>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _userManager.AddToRoleAsync(targetUser, "Admin")).MustHaveHappenedOnceExactly();

            var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("ManageRoles");
        }

        [Fact]
        public void AdminPanel_ReturnsViewResult()
        {
            var result = _controller.AdminPanel();

            result.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public async Task ManageUsers_ReturnsFilteredUsers_WhenSearchTermIsProvided()
        {
            var user1 = new User { Id = 5, Name = "Marek", Surname = "B¹czek", Email = "marek@test.com", UserName = "marek@test.com" };
            var user2 = new User { Id = 6, Name = "Zofia", Surname = "Nowak", Email = "zofia@test.com", UserName = "zofia@test.com" };

            _context.Users.AddRange(user1, user2);
            await _context.SaveChangesAsync();

            A.CallTo(() => _userManager.Users).Returns(_context.Users);

            var result = await _controller.ManageUsers("Marek");

            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeAssignableTo<IEnumerable<User>>().Subject;

            model.Should().ContainSingle();
            model.First().Name.Should().Be("Marek");
            _controller.ViewData["SearchTerm"].Should().Be("marek");
        }

        [Fact]
        public async Task DeleteUser_RemovesUserAndRedirects_WhenUserExists()
        {
            var userToDelete = new User { Id = 99, Email = "delete@me.com", UserName = "delete@me.com" };
            _context.Users.Add(userToDelete);
            await _context.SaveChangesAsync();

            A.CallTo(() => _userManager.FindByIdAsync("99")).Returns(userToDelete);

            A.CallTo(() => _userManager.DeleteAsync(userToDelete)).Returns(IdentityResult.Success);

            var result = await _controller.DeleteUser("99");

            var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("ManageUsers");

            A.CallTo(() => _userManager.DeleteAsync(userToDelete)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task DeleteUser_ReturnsNotFound_WhenUserDoesNotExist()
        {
            A.CallTo(() => _userManager.FindByIdAsync("100")).Returns((User)null);

            var result = await _controller.DeleteUser("100");

            result.Should().BeOfType<NotFoundResult>();
        }

        [Fact]
        public async Task EditUser_GET_ReturnsViewWithModel_WhenUserExists()
        {
            var user = new User { Id = 1, Name = "Jan", Surname = "Kowalski", Email = "jan@test.com" };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            A.CallTo(() => _userManager.Users).Returns(_context.Users);
            A.CallTo(() => _userManager.GetRolesAsync(user)).Returns(new List<string> { "Nowy" });
            A.CallTo(() => _userManager.GetUsersInRoleAsync("Buddy")).Returns(new List<User> { new User { Id = 2, Name = "Marek", Surname = "Opiekun" } });

            var result = await _controller.EditUser(1);

            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeOfType<EditUserViewModel>().Subject;
            model.Name.Should().Be("Jan");
            model.AvailableBuddies.Should().NotBeEmpty(); 
        }

        [Fact]
        public async Task EditUser_POST_UpdatesUserAndAddsNotification_WhenBuddyChanged()
        {
            var user = new User { Id = 1, Name = "Jan", Surname = "Kowalski", BuddyId = null };
            var buddy = new User { Id = 2, Name = "Marek", Surname = "Opiekun" };

            _context.Users.AddRange(user, buddy);
            await _context.SaveChangesAsync();

            A.CallTo(() => _userManager.Users).Returns(_context.Users);
            A.CallTo(() => _userManager.UpdateAsync(A<User>._)).Returns(IdentityResult.Success);

            var model = new EditUserViewModel
            {
                Id = 1,
                Name = "Jan Zmieniony",
                Surname = "Kowalski",
                Email = "jan@test.com",
                BuddyId = 2 
            };

            var result = await _controller.EditUser(model);

            var redirect = result.Should().BeOfType<RedirectToActionResult>().Subject;
            redirect.ActionName.Should().Be("ManageUsers");

            user.Name.Should().Be("Jan Zmieniony");

            _context.Notifications.Should().Contain(n => n.UserId == 1 && n.Message.Contains("Marek"));
        }

        [Fact]
        public void Create_GET_ReturnsViewResult()
        {
            var result = _controller.Create();

            result.Should().BeOfType<ViewResult>();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
