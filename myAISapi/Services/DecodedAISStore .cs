using myAISapi.Models;
using System.Collections.Concurrent;

namespace myAISapi.Services
{
	public class DecodedAISStore:IDecodedAISStore
	{
		private readonly ConcurrentQueue<DecodedAISMessage> _decodedMessages = new();

		public void AddDecodedMessage(DecodedAISMessage message)
		{
			_decodedMessages.Enqueue(message);
		}

		public IEnumerable<DecodedAISMessage> GetAllDecodedMessages()
		{
			return _decodedMessages.ToArray();
		}

		public void ClearMessages()
		{
			while (_decodedMessages.TryDequeue(out _)) { }
		}
		
		public void DeteleFirstMessage()
		{
			_decodedMessages.TryDequeue(out _);
		}
	}
}
