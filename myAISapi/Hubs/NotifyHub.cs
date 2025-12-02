using Microsoft.AspNetCore.SignalR;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;

namespace myAISapi.Hubs
{
	[Authorize]
	public class NotifyHub : Hub
	{
		public async Task JoinGroup(string groupName)
		{
			await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
		}

		public async Task Ping()
		{
			await Clients.Caller.SendAsync("Pong", "pong from server");
		}
	}
}
