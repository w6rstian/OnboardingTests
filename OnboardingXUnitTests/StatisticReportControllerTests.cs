using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Onboarding.Controllers;
using Onboarding.Data;
using Onboarding.Models;
using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
namespace OnboardingXUnitTests
{
    public class StatisticReportControllerTests
    {
        private readonly ApplicationDbContext _context;
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Mock<RoleManager<IdentityRole<int>>> _mockRoleManager;
        private readonly StatisticReportController _controller;

        public StatisticReportControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);

            var userStore = new Mock<IUserStore<User>>();
            _mockUserManager = new Mock<UserManager<User>>(userStore.Object, null, null, null, null, null, null, null, null);

            var roleStore = new Mock<IRoleStore<IdentityRole<int>>>();
            _mockRoleManager = new Mock<RoleManager<IdentityRole<int>>>(roleStore.Object, null, null, null, null);

            _controller = new StatisticReportController(_context, _mockUserManager.Object, _mockRoleManager.Object);
        }

        [Fact]
        public async System.Threading.Tasks.Task GetNewUsersByCourse_ReturnsCorrectUsers()
        {
            var user = new User { Id = 1, Name = "New", Surname = "User" };
            _context.Users.Add(user);
            _context.Courses.Add(new Course { Id = 1, Name = "Course 1", Image = new byte[] { 0 }, ImageMimeType = "image/png" });
            _context.UserCourses.Add(new UserCourse { UserId = 1, CourseId = 1 });
            await _context.SaveChangesAsync();

            _mockUserManager.Setup(um => um.GetRolesAsync(user)).ReturnsAsync(new List<string> { "Nowy" });

            var result = await _controller.GetNewUsersByCourse(1);

            var jsonResult = Assert.IsType<JsonResult>(result);
            Assert.NotNull(jsonResult.Value);
        }
    }
}
