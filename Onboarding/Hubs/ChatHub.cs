using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Onboarding.Hubs
{
	public class ChatHub : Hub
	{
		public async Task SendMessage(string messageContent, string sentAt, int senderId, int receiverId)
		{
			var groupName = GetGroupName(senderId, receiverId);
			await Clients.Group(groupName).SendAsync("ReceiveMessage", messageContent, sentAt, senderId);
		}


		public override async Task OnConnectedAsync()
		{
			var httpContext = Context.GetHttpContext();
			var senderId = int.Parse(httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
			var receiverId = int.Parse(httpContext.Request.Query["receiverId"]);

			var groupName = GetGroupName(senderId, receiverId);
			await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
		}

		public override async Task OnDisconnectedAsync(System.Exception exception)
		{
			var httpContext = Context.GetHttpContext();
			var senderId = int.Parse(httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
			var receiverId = int.Parse(httpContext.Request.Query["receiverId"]);

			var groupName = GetGroupName(senderId, receiverId);
			await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
		}

		private string GetGroupName(int senderId, int receiverId)
		{
			return senderId < receiverId ? $"{senderId}_{receiverId}" : $"{receiverId}_{senderId}";
		}
	}
}
