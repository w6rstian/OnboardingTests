using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Onboarding.Controllers;
using Onboarding.Data;
using Onboarding.Interfaces;
using Onboarding.Models;
using Onboarding.ViewModels;
using System.Security.Claims;
using Xunit;
using Task = System.Threading.Tasks.Task;

namespace OnboardingXUnitTests.Controllers
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
            // Act
            var result = _controller.Create();

            // Assert
            result.Should().BeOfType<ViewResult>();
        }
        [Fact]
        public async Task CreatePost_ModelIsNull_ReturnsViewWithModelError()
        {
            // Act
            var result = await _controller.Create(null);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            _controller.ModelState.ErrorCount.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task CreatePost_CourseNameEmpty_ReturnsViewWithMentorsData()
        {
            // Arrange
            _context.Users.Add(new User { Id = 1, Name = "Jan", Surname = "Testowy" });
            await _context.SaveChangesAsync();

            var viewModel = new CreateOnboardingViewModel { CourseName = "" };

            // Act
            var result = await _controller.Create(viewModel);

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            _controller.ModelState.IsValid.Should().BeFalse();
            viewResult.ViewData.ContainsKey("Mentors").Should().BeTrue();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }



        // S

        [Fact]
        public void Create_WhenNoMentors_AddsModelError()
        {
            // Act
            var result = _controller.Create();

            // Assert
            result.Should().BeOfType<ViewResult>();
            _controller.ModelState.IsValid.Should().BeFalse();
        }

        [Fact]
        public void Create_WhenMentorsExist_ReturnsViewWithMentors()
        {
            // Arrange
            _context.Users.Add(new User { Id = 1, Name = "Anna", Surname = "Nowak" });
            _context.SaveChanges();

            // Act
            var result = _controller.Create();

            // Assert
            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.ViewData.ContainsKey("Mentors").Should().BeTrue();
        }


        [Fact]
        public async Task CreatePost_InvalidMentorId_AddsModelError()
        {
            // Arrange
            var vm = new CreateOnboardingViewModel
            {
                CourseName = "Test Course",
                MentorId = 99
            };

            // Act
            var result = await _controller.Create(vm);

            // Assert
            result.Should().BeOfType<ViewResult>();
            _controller.ModelState.IsValid.Should().BeFalse();
        }


        [Fact]
        public async Task CreatePost_ValidCourseWithoutTasks_CreatesCourse()
        {
            // Arrange
            _context.Users.Add(new User { Id = 1, Name = "Jan", Surname = "Mentor" });
            await _context.SaveChangesAsync();

            var vm = new CreateOnboardingViewModel
            {
                CourseName = "Course A",
                MentorId = 1
            };

            // Act
            var result = await _controller.Create(vm);

            // Assert
            result.Should().BeOfType<RedirectToActionResult>();
            _context.Courses.Should().HaveCount(1);
        }


        [Fact]
        public async Task CreatePost_TaskWithoutTitle_AddsModelError()
        {
            // Arrange
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

            // Act
            var result = await _controller.Create(vm);

            // Assert
            result.Should().BeOfType<ViewResult>();
            _controller.ModelState.IsValid.Should().BeFalse();
        }


        [Fact]
        public async Task CreatePost_TaskMentorDoesNotExist_ReturnsError()
        {
            // Arrange
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

            // Act
            var result = await _controller.Create(vm);

            // Assert
            result.Should().BeOfType<ViewResult>();
            _controller.ModelState.IsValid.Should().BeFalse();
        }


        [Fact]
        public async Task CreatePost_ValidTask_AddsTaskToDatabase()
        {
            // Arrange
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
                MentorId = 1,
                ArticleContent = "article",
                Links = "link"
            }
        }
            };

            // Act
            await _controller.Create(vm);

            // Assert
            _context.Tasks.Should().HaveCount(1);
        }


        [Fact]
        public async Task CreatePost_TaskCreatesLinks()
        {
            // Arrange
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
                MentorId = 1,
                ArticleContent = "article",
                Links = "http://a.com http://b.com"
            }
        }
            };

            // Act
            await _controller.Create(vm);

            // Assert
            var task = _context.Tasks.Include(t => t.Links).First();
            task.Links.Should().HaveCount(2);
        }


        [Fact]
        public async Task CreatePost_TaskCreatesArticle()
        {
            // Arrange
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
                MentorId = 1,
                ArticleContent = "article text",
                Links = "link"
            }
        }
            };

            // Act
            await _controller.Create(vm);

            // Assert
            var task = _context.Tasks.Include(t => t.Articles).First();
            task.Articles.Should().HaveCount(1);
        }


        [Fact]
        public async Task CreatePost_TestWithQuestions_IsSaved()
        {
            // Arrange
            _context.Users.Add(new User { Id = 1, Name = "Jan", Surname = "Mentor" });
            await _context.SaveChangesAsync();

            var vm = new CreateOnboardingViewModel
            {
                CourseName = "Course",
                MentorId = 1,
                Tests = new List<TestViewModel>
        {
            new TestViewModel
            {
                Name = "Test 1",
                Questions = new List<QuestionViewModel>
                {
                    new QuestionViewModel
                    {
                        Description = "Question",
                        AnswerA = "A",
                        AnswerB = "B",
                        AnswerC = "C",
                        AnswerD = "D",
                        CorrectAnswer = "A"
                    }
                }
            }
        }
            };

            // Act
            await _controller.Create(vm);

            // Assert
            _context.Tests.Should().HaveCount(1);
        }
    }
}
