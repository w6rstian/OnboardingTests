using Xunit;
using FakeItEasy;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using Onboarding.Controllers;
using Onboarding.Data;
using Onboarding.Models;
using Onboarding.Hubs;
using System.Security.Claims;
using Task = System.Threading.Tasks.Task;

namespace OnboardingXUnitTests.Controllers
{
    public class ChatControllerTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly ChatController _controller;
        private readonly IHubContext<ChatHub> _chatHub;

        public ChatControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);
            _context.Database.EnsureCreated();

            _chatHub = A.Fake<IHubContext<ChatHub>>();

            _controller = new ChatController(_context, _chatHub);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, "testuser"),
                new Claim(ClaimTypes.NameIdentifier, "1")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
        }

        [Fact]
        public void UserList_ReturnsViewWithAllUsers()
        {
            _context.Users.AddRange(
                new User { Id = 1, UserName = "user1" },
                new User { Id = 2, UserName = "user2" }
            );
            _context.SaveChanges();

            var result = _controller.UserList();

            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeAssignableTo<IEnumerable<User>>().Subject;
            model.Should().HaveCount(2);
        }

        [Fact]
        public void Index_SetsCorrectReceiverNameInViewBag()
        {
            _context.Users.Add(new User { Id = 2, UserName = "ReceiverUser" });
            _context.SaveChanges();

            _controller.Index(2);

            string receiverName = _controller.ViewBag.ReceiverName;
            receiverName.Should().Be("ReceiverUser");
            ((int)_controller.ViewBag.ReceiverId).Should().Be(2);
        }

        [Fact]
        public async Task SendMessage_ValidContent_SavesMessageToDb()
        {
            var clients = A.Fake<IHubClients>();
            var clientProxy = A.Fake<IClientProxy>();
            A.CallTo(() => _chatHub.Clients).Returns(clients);
            A.CallTo(() => clients.Group(A<string>._)).Returns(clientProxy);

            var result = await _controller.SendMessage(2, "Hello World");

            result.Should().BeOfType<RedirectToActionResult>().Which.ActionName.Should().Be("Index");
            _context.Messages.Should().HaveCount(1);
            _context.Messages.First().Content.Should().Be("Hello World");
            _context.Messages.First().ReceiverId.Should().Be(2);
        }

        [Fact]
        public async Task SendMessage_EmptyContent_DoesNotSaveMessage()
        {
            await _controller.SendMessage(2, "");

            _context.Messages.Should().BeEmpty();
        }

        [Fact]
        public async Task SendMessage_CallsSignalRWithCorrectGroupName()
        {
            var clients = A.Fake<IHubClients>();
            var clientProxy = A.Fake<IClientProxy>();
            A.CallTo(() => _chatHub.Clients).Returns(clients);
            A.CallTo(() => clients.Group("1_2")).Returns(clientProxy);

            await _controller.SendMessage(2, "Test SignalR");

            A.CallTo(() => clients.Group("1_2")).MustHaveHappenedOnceExactly();
            A.CallTo(() => clientProxy.SendCoreAsync("ReceiveMessage", A<object[]>._, A<CancellationToken>._)).MustHaveHappened();
        }

        [Fact]
        public void Index_ReturnsOnlyMessagesBetweenSenderAndReceiver_OrderedByDate()
        {
            var senderId = 1;
            var receiverId = 2;
            var otherId = 3;

            _context.Messages.AddRange(
                new Message { SenderId = senderId, ReceiverId = receiverId, Content = "First", SentAt = DateTime.Now.AddMinutes(-10) },
                new Message { SenderId = receiverId, ReceiverId = senderId, Content = "Second", SentAt = DateTime.Now.AddMinutes(-5) },
                new Message { SenderId = senderId, ReceiverId = otherId, Content = "Unrelated", SentAt = DateTime.Now }
            );
            _context.SaveChanges();

            var result = _controller.Index(receiverId);

            var viewResult = result.Should().BeOfType<ViewResult>().Subject;
            var model = viewResult.Model.Should().BeAssignableTo<IEnumerable<Message>>().Subject.ToList();
            
            model.Should().HaveCount(2);
            model[0].Content.Should().Be("First");
            model[1].Content.Should().Be("Second");
            model[0].SentAt.Should().BeBefore(model[1].SentAt);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
