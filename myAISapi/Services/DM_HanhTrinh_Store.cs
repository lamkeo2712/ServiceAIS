using myAISapi.Models;
using System.Collections.Concurrent;

namespace myAISapi.Services
{
	public class DM_HanhTrinh_Store:IDM_HanhTrinh_Store
	{
		private  ConcurrentQueue<DM_HanhTrinh> _route = new();

		public void AddRoute(DM_HanhTrinh message)
		{
			_route.Enqueue(message);
		}

		public IEnumerable<DM_HanhTrinh> GetAllRoute()
		{
			return _route.ToArray();
		}

		public void ClearMessages()
		{
			while (_route.TryDequeue(out _)) { }
		}

		public void DeleteFirstMessage()
		{
			_route.TryDequeue(out _);
		}

		public void DeleteMessages(IEnumerable<DM_HanhTrinh> batch)
		{
			var newQueue = new ConcurrentQueue<DM_HanhTrinh>();
			var mmsisToDelete = batch.Select(m => m.MMSI).ToHashSet();

			while (_route.TryDequeue(out var message))
			{
				if (!mmsisToDelete.Contains(message.MMSI))
				{
					newQueue.Enqueue(message);
				}
			}

			_route = newQueue;
		}
	}
}
