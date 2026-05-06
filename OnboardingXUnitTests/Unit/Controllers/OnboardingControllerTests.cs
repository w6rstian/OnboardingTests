using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Onboarding.Controllers;
using Onboarding.Data;
using Onboarding.Models;
using Onboarding.ViewModels;
using System.Security.Claims;
using Task = System.Threading.Tasks.Task;

namespace OnboardingXUnitTests.Unit.Controllers
{
    public class OnboardingControllerTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly OnboardingController _controller;

        public OnboardingControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);
            _context.Database.EnsureCreated();

            _controller = new OnboardingController(_context);

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
        public void Create_ReturnsViewResult()
        {

            var result = _controller.Create();


            result.Should().BeOfType<ViewResult>();
        }
        /* ---- Autor Michał Kobyliński ---*/
        [Fact]
        public async Task CreatePost_ModelIsNull_ReturnsViewWithModelError()
        {

            var result = await _controller.Create(null);


            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            _controller.ModelState.ErrorCount.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreatePost_CourseNameEmpty_ReturnsViewWithMentorsData()
        {

            _context.Users.Add(new User { Id = 1, Name = "Jan", Surname = "Testowy" });
            await _context.SaveChangesAsync();

            var viewModel = new CreateOnboardingViewModel { CourseName = "" };


            var result = await _controller.Create(viewModel);


            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            _controller.ModelState.IsValid.Should().BeFalse();
            viewResult.ViewData.ContainsKey("Mentors").Should().BeTrue();
        }
        /*-----------------*/
        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
        


        // Sebastian Szklanko

        [Fact]
        public void Create_WhenNoMentors_AddsModelError()
        {

            var result = _controller.Create();


            result.Should().BeOfType<ViewResult>();
            _controller.ModelState.IsValid.Should().BeFalse();
        }

        // Sebastian Szklanko
        [Fact]
        public void Create_WhenMentorsExist_ReturnsViewWithMentors()
        {

            _context.Users.Add(new User { Id = 1, Name = "Anna", Surname = "Nowak" });
            _context.SaveChanges();


            var result = _controller.Create();


            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.ViewData.ContainsKey("Mentors").Should().BeTrue();
        }

        // Sebastian Szklanko
        [Fact]
        public async Task CreatePost_InvalidMentorId_AddsModelError()
        {

            var vm = new CreateOnboardingViewModel
            {
                CourseName = "Test Course",
                MentorId = 99
            };


            var result = await _controller.Create(vm);


            result.Should().BeOfType<ViewResult>();
            _controller.ModelState.IsValid.Should().BeFalse();
        }

        // Sebastian Szklanko
        [Fact]
        public async Task CreatePost_TaskWithoutTitle_AddsModelError()
        {

            _context.Users.Add(new User { Id = 1, Name = "Jan", Surname = "Mentor" });
            await _context.SaveChangesAsync();

            var vm = new CreateOnboardingViewModel
            {
                CourseName = "Course",
                MentorId = 1,
                Tasks = new List<TaskViewModel>
            {
                new TaskViewModel
                {
                    Title = "",
                    Description = "desc",
                    MentorId = 1,
                    ArticleContent = "article",
                    Links = "link"
                }
            }
                };


            var result = await _controller.Create(vm);


            result.Should().BeOfType<ViewResult>();
            _controller.ModelState.IsValid.Should().BeFalse();
        }

        // Sebastian Szklanko
        [Fact]
        public async Task CreatePost_TaskMentorDoesNotExist_ReturnsError()
        {

            _context.Users.Add(new User { Id = 1, Name = "Jan", Surname = "Mentor" });
            await _context.SaveChangesAsync();

            var vm = new CreateOnboardingViewModel
            {
                CourseName = "Course",
                MentorId = 1,
                Tasks = new List<TaskViewModel>
        {
            new TaskViewModel
            {
                Title = "Task",
                Description = "desc",
                MentorId = 99,
                ArticleContent = "article",
                Links = "link"
            }
        }
            };


            var result = await _controller.Create(vm);


            result.Should().BeOfType<ViewResult>();
            _controller.ModelState.IsValid.Should().BeFalse();
        }

        // Sebastian Szklanko
        [Fact]
        public async Task CreatePost_ReturnsActionResult()
        {
            var vm = new CreateOnboardingViewModel();

            var result = await _controller.Create(vm);

            result.Should().BeAssignableTo<IActionResult>();
        }

        // Sebastian Szklanko
        [Fact]
        public async Task CreatePost_InvalidModel_ReturnsView()
        {
            _controller.ModelState.AddModelError("error", "invalid");

            var vm = new CreateOnboardingViewModel();

            var result = await _controller.Create(vm);

            result.Should().BeOfType<ViewResult>();
        }

        // Sebastian Szklanko
        [Fact]
        public void CreateGet_ReturnsView()
        {

            var result = _controller.Create();


            result.Should().BeOfType<ViewResult>();
        }

        // Sebastian Szklanko
        [Fact]
        public async Task CreatePost_ReturnType_IsValid()
        {
            var vm = new CreateOnboardingViewModel();

            var result = await _controller.Create(vm);

            result.Should().NotBeNull();
        }

        // Sebastian Szklanko
        [Fact]
        public async Task CreatePost_DoesNotThrowException()
        {
            var vm = new CreateOnboardingViewModel();

            Func<Task> act = async () => await _controller.Create(vm);

            await act.Should().NotThrowAsync();
        }
    }
}
