using myAISapi.Models;
using System.Collections.Concurrent;

namespace myAISapi.Services
{
	public class DM_HanhTrinh_Store:IDM_HanhTrinh_Store
	{
		private readonly ConcurrentQueue<DM_HanhTrinh> _route = new();

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
	}
}
