using Microsoft.AspNetCore.SignalR;
using myAISapi.Hubs;

namespace myAISapi.Services
{
	public interface IAlertService
	{
		Task PushBroadcastAsync(object alert);
		Task PushToUserAsync(string userId, object alert);
		Task PushToGroupAsync(string group, object alert);
	}

	public class AlertService : IAlertService
	{
		private readonly IHubContext<NotifyHub> _hub;

		public AlertService(IHubContext<NotifyHub> hub)
		{
			_hub = hub;
		}

		// Gửi cho tất cả client
		public async Task PushBroadcastAsync(object alert)
		{
			await _hub.Clients.All.SendAsync("ReceiveAlert", alert);
		}

		// Gửi cho 1 user (dựa trên UserIdentifier của SignalR)
		public async Task PushToUserAsync(string userId, object alert)
		{
			await _hub.Clients.User(userId).SendAsync("ReceiveAlert", alert);
		}

		// Gửi cho 1 group
		public async Task PushToGroupAsync(string group, object alert)
		{
			await _hub.Clients.Group(group).SendAsync("ReceiveAlert", alert);
		}
	}
}
