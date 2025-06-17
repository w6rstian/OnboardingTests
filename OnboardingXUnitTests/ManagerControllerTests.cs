using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Onboarding.Controllers;
using Onboarding.Data;
using Onboarding.Models;
using System.Security.Claims;
using Task = System.Threading.Tasks.Task;

namespace OnboardingXUnitTests
{
    public class ManagerControllerTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly ManagerController _controller;

        public ManagerControllerTests()
        {
            var store = new Mock<IUserStore<User>>();
            _mockUserManager = new Mock<UserManager<User>>(
                store.Object, null, null, null, null, null, null, null, null);

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _controller = new ManagerController(_context, _mockUserManager.Object);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Fact]
        public void Index_ReturnsViewResult()
        {
            var result = _controller.Index();
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public void ManagerPanel_ReturnsViewResult()
        {
            var result = _controller.ManagerPanel();
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task MyCourses_ReturnsViewResult_WithCoursesList()
        {
            var currentUser = new User { Id = 1, UserName = "test@example.com" };
            _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(currentUser);

            _context.Courses.AddRange(
                new Course { Id = 1, Name = "Course 1", Mentor = currentUser },
                new Course { Id = 2, Name = "Course 2", Mentor = currentUser }
            );
            _context.SaveChanges();

            var result = await _controller.MyCourses();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<Course>>(viewResult.Model);
            Assert.Equal(2, model.Count);
        }

        [Fact]
        public async Task MyCourses_ReturnsEmptyList_WhenNoCourses()
        {
            var currentUser = new User { Id = 1, UserName = "test@example.com" };
            _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(currentUser);

            var result = await _controller.MyCourses();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<Course>>(viewResult.Model);
            Assert.Empty(model);
        }

        [Fact]
        public async Task MyCourses_ReturnsOnlyCurrentUserCourses()
        {
            var currentUser = new User { Id = 1, UserName = "test@example.com" };
            var otherUser = new User { Id = 2, UserName = "other@example.com" };
            _mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(currentUser);

            _context.Courses.AddRange(
                new Course { Id = 1, Name = "Course 1", Mentor = currentUser },
                new Course { Id = 2, Name = "Course 2", Mentor = otherUser },
                new Course { Id = 3, Name = "Course 3", Mentor = currentUser }
            );
            _context.SaveChanges();

            var result = await _controller.MyCourses();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<Course>>(viewResult.Model);
            Assert.Equal(2, model.Count);
            Assert.All(model, c => Assert.Equal(currentUser.Id, c.Mentor.Id));
        }
    }
}
