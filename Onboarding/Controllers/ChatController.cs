using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Onboarding.Models;
using Onboarding.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Onboarding.Hubs;

namespace Onboarding.Controllers
{
	public class ChatController : Controller
	{
		private readonly ApplicationDbContext _db;
		private readonly IHubContext<ChatHub> _chatHub;

		public ChatController(ApplicationDbContext db, IHubContext<ChatHub> chatHub)
		{
			_db = db;
			_chatHub = chatHub;
		}

		[Authorize]
		public IActionResult UserList()
		{
			var users = _db.Users.ToList();
			return View(users);
		}

		[Authorize]
		public IActionResult Index(int receiverId)
		{
			var senderId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

			var messages = _db.Messages
				.Where(m => (m.SenderId == senderId && m.ReceiverId == receiverId) ||
							(m.SenderId == receiverId && m.ReceiverId == senderId))
				.OrderBy(m => m.SentAt)
				.ToList();

			ViewBag.ReceiverId = receiverId;
			ViewBag.ReceiverName = _db.Users.FirstOrDefault(u => u.Id == receiverId)?.UserName;

			return View(messages);
		}

		[Authorize]
		[HttpPost]
		public async Task<IActionResult> SendMessage(int receiverId, string content)
		{
			if (!string.IsNullOrEmpty(content))
			{
				var senderId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
				var senderName = User.Identity.Name;

				var message = new Message
				{
					Content = content,
					SentAt = DateTime.Now,
					SenderId = senderId,
					ReceiverId = receiverId
				};

				_db.Messages.Add(message);
				await _db.SaveChangesAsync();

				// SignalR dynamiczne przesyłanie wiadomości
				var groupName = $"{Math.Min(senderId, receiverId)}_{Math.Max(senderId, receiverId)}";
				await _chatHub.Clients.Group($"{Math.Min(senderId, receiverId)}_{Math.Max(senderId, receiverId)}")
	.SendAsync("ReceiveMessage", message.Content, message.SentAt.ToString("g"), message.SenderId);

			}

			return RedirectToAction("Index", new { receiverId });
		}
	}
}
