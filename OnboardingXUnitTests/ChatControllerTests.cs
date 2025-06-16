using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Moq;
using Onboarding.Controllers;
using Onboarding.Data;
using Onboarding.Hubs;
using Onboarding.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;
using Task = System.Threading.Tasks.Task;

namespace OnboardingXUnitTests
{
    public class ChatControllerTests
    {
        private ApplicationDbContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        private ClaimsPrincipal GetFakeUser(int id, string username)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, id.ToString()),
                new Claim(ClaimTypes.Name, username)
            };

            var identity = new ClaimsIdentity(claims, "TestAuthType");
            return new ClaimsPrincipal(identity);
        }

        [Fact]
        public void UserList_ReturnsUsersInView()
        {
            var db = GetInMemoryDbContext();
            db.Users.Add(new User { Id = 1, UserName = "Alice" });
            db.Users.Add(new User { Id = 2, UserName = "Bob" });
            db.SaveChanges();

            var mockHub = new Mock<IHubContext<ChatHub>>();
            var controller = new ChatController(db, mockHub.Object);

            var result = controller.UserList() as ViewResult;

            var model = Assert.IsAssignableFrom<List<User>>(result.Model);
            Assert.Equal(2, model.Count);
        }

        [Fact]
        public void Index_ReturnsMessagesBetweenUsers()
        {
            var db = GetInMemoryDbContext();
            db.Users.AddRange(
                new User { Id = 1, UserName = "Alice" },
                new User { Id = 2, UserName = "Bob" }
            );
            db.Messages.AddRange(
                new Message { SenderId = 1, ReceiverId = 2, Content = "Hi", SentAt = DateTime.Now.AddMinutes(-1) },
                new Message { SenderId = 2, ReceiverId = 1, Content = "Hello", SentAt = DateTime.Now }
            );
            db.SaveChanges();

            var mockHub = new Mock<IHubContext<ChatHub>>();
            var controller = new ChatController(db, mockHub.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = GetFakeUser(1, "Alice")
                    }
                }
            };

            var result = controller.Index(2) as ViewResult;

            var model = Assert.IsAssignableFrom<List<Message>>(result.Model);
            Assert.Equal(2, model.Count);
        }

        [Fact]
        public async Task SendMessage_AddsMessageAndCallsSignalR()
        {
            var db = GetInMemoryDbContext();
            var mockClients = new Mock<IHubClients>();
            var mockClientProxy = new Mock<IClientProxy>();

            mockClients.Setup(c => c.Group(It.IsAny<string>())).Returns(mockClientProxy.Object);

            var mockHub = new Mock<IHubContext<ChatHub>>();
            mockHub.Setup(h => h.Clients).Returns(mockClients.Object);

            var controller = new ChatController(db, mockHub.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = GetFakeUser(1, "Alice")
                    }
                }
            };

            var result = await controller.SendMessage(2, "Test message");

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);

            var message = db.Messages.FirstOrDefault();
            Assert.NotNull(message);
            Assert.Equal("Test message", message.Content);

            mockClientProxy.Verify(p =>
                p.SendCoreAsync("ReceiveMessage",
                    It.Is<object[]>(o => o[0].ToString() == "Test message"),
                    default),
                Times.Once);
        }
    }
}
