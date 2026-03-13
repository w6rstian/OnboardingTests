using Xunit;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Onboarding.Controllers;
using Onboarding.Data;
using Onboarding.Models;
using Onboarding.Interfaces;
using System.Security.Claims;
using Task = System.Threading.Tasks.Task;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace OnboardingXUnitTests.Controllers
{
    public class RewardsControllerTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly RewardsController _controller;

        public RewardsControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);
            _context.Database.EnsureCreated();

            _controller = new RewardsController(_context);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "testuser"),
                new Claim(ClaimTypes.NameIdentifier, "1")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };

            _controller.TempData = A.Fake<ITempDataDictionary>();
        }

        [Fact]
        public void RateBuddy_ReturnsViewResult()
        {
            var currentUser = new User { Id = 1, Name = "Test", Surname = "User", BuddyId = 2 };
            var buddy = new User { Id = 2, Name = "Buddy", Surname = "User" };
            _context.Users.AddRange(currentUser, buddy);
            _context.SaveChanges();

            var result = _controller.RateBuddy();

            result.Should().BeOfType<ViewResult>();
        }

        [Fact]
        public void RateBuddy_ReturnsNotFound_WhenUserHasNoBuddy()
        {
            var currentUser = new User { Id = 1, Name = "Test", Surname = "User", BuddyId = null };
            _context.Users.Add(currentUser);
            _context.SaveChanges();

            var result = _controller.RateBuddy();

            result.Should().BeOfType<NotFoundObjectResult>()
                .Which.Value.Should().Be("Brak przypisanego Buddy'ego.");
        }

        [Fact]
        public void RateMentor_ReturnsNotFound_WhenMentorDoesNotExist()
        {

            var result = _controller.RateMentor(99, 1);

            result.Should().BeOfType<NotFoundObjectResult>()
                .Which.Value.Should().Be("Mentor nie znaleziony.");
        }

        [Fact]
        public void RateMentor_ReturnsViewResult_WithCorrectViewBagData()
        {

            var mentorId = 10;
            var taskId = 5;
            var mentor = new User { Id = mentorId, Name = "Marek", Surname = "Mentor" };


            var rewards = new List<Reward>
            {
                new Reward { Receiver = mentorId, Rating = 5, Feedback = "Super" },
                new Reward { Receiver = mentorId, Rating = 3, Feedback = "Ok" }
            };

            _context.Users.Add(mentor);
            _context.Rewards.AddRange(rewards);
            _context.SaveChanges();

            var result = _controller.RateMentor(mentorId, taskId);

            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            viewResult.ViewName.Should().Be("RatePerson");

            var model = viewResult.Model.Should().BeOfType<Reward>().Subject;
            model.Receiver.Should().Be(mentorId);

            ((string)_controller.ViewBag.PersonType).Should().Be("Mentora");
            ((string)_controller.ViewBag.PersonName).Should().Be("Marek Mentor");
            ((double)_controller.ViewBag.AverageRating).Should().Be(4.0);
            ((int)_controller.ViewBag.TaskId).Should().Be(taskId);
        }

        [Fact]
        public async Task RateMentor_Post_ReturnsResultFromSaveRating()
        {

            var receiverId = 10;
            var rating = 5;
            var feedback = "Œwietna wspó³praca";
            var taskId = 5;
            _context.Users.Add(new User { Id = receiverId, Name = "Mentor", Surname = "Test" });
            await _context.SaveChangesAsync();

            var result = await _controller.RateMentor(receiverId, rating, feedback, taskId);

            result.Should().NotBeNull();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
