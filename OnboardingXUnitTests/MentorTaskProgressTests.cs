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
using Onboarding.Data.Enums;
namespace OnboardingXUnitTests
{
    public class MentorTaskProgressTests
    {
        private readonly ApplicationDbContext _context;
        private readonly MentorTaskProgress _controller;

        public MentorTaskProgressTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);

            _controller = new MentorTaskProgress(_context);
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                new Claim(ClaimTypes.NameIdentifier, "1")
            }));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
        }

        [Fact]
        public async System.Threading.Tasks.Task Index_ReturnsTasksForCurrentMentor()
        {
            _context.Users.Add(new User { Id = 1, Name = "Mentor", Surname = "Test" });

            _context.Courses.Add(new Course
            {
                Id = 1,
                Name = "Course 1",
                Image = new byte[] { 0 },
                ImageMimeType = "image/png"
            });

            _context.Tasks.Add(new Onboarding.Models.Task
            {
                Id = 1,
                MentorId = 1,  // Ważne: musi być zgodny z użytkownikiem zalogowanym w teście
                Title = "Task 1",
                Description = "Description",
                CourseId = 1
            });

            await _context.SaveChangesAsync();

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<List<Onboarding.Models.Task>>(viewResult.Model);

            // Debugowanie pomocnicze (opcjonalnie)
            Assert.NotNull(model);
            Assert.True(model.Any(), "Lista zadań jest pusta.");

            Assert.Single(model);
            Assert.Equal("Task 1", model[0].Title);
        }

        [Fact]
        public async System.Threading.Tasks.Task Grade_GET_WithValidIds_ReturnsView()
        {
            SetupEntities();

            var result = await _controller.Grade(1, 1);
            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async System.Threading.Tasks.Task Grade_POST_WithValidData_UpdatesTaskAndRedirects()
        {
            SetupEntities();

            var result = await _controller.Grade(1, "A");

            var userTask = await _context.UserTasks.FindAsync(1);
            Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal(StatusTask.Graded, userTask.Status);
        }

        private void SetupEntities()
        {
            _context.Users.Add(new User { Id = 1, Name = "Test", Surname = "User" });

            _context.Courses.Add(new Course
            {
                Id = 1,
                Name = "Course",
                Image = new byte[] { 0 },
                ImageMimeType = "image/png"
            });

            _context.Tasks.Add(new Onboarding.Models.Task
            {
                Id = 1,
                MentorId = 1,
                Title = "Task 1",
                Description = "Description",
                CourseId = 1
            });

            _context.UserTasks.Add(new UserTask
            {
                UserTaskId = 1,
                UserId = 1,
                TaskId = 1,
                Container = "Container",
                Grade = "B",
                Status = StatusTask.New
            });

            _context.SaveChanges();
        }
    }
}
