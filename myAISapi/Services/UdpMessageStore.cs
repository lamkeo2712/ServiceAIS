using myAISapi.Models;
using System.Collections.Concurrent;

namespace myAISapi.Services
{
	public class UdpMessageStore : IUdpMessageStore
	{
		private readonly ConcurrentQueue<string> _messages = new ConcurrentQueue<string>();

		public void AddMessage(string message)
		{
			_messages.Enqueue(message);
		}

		public string[] GetAllMessages()
		{
			return _messages.ToArray();
		}

		public void ClearMessages()
		{
			_messages.Clear();
		}
		public void Delete()
		{
			_messages.TryDequeue(out _);
		}
	}
}
