using myAISapi.Models;
using System.Collections.Concurrent;

namespace myAISapi.Services
{
	public class DM_Tau_HS_Store:IDM_Tau_HS_Store
	{
		private readonly ConcurrentQueue<DM_Tau> _ship = new();

		public void AddShip(DM_Tau message)
		{
			_ship.Enqueue(message);
		}

		public IEnumerable<DM_Tau> GetAllShip()
		{
			return _ship.ToArray();
		}

		public void ClearMessages()
		{
			while (_ship.TryDequeue(out _)) { }
		}

		public void DeleteFirstMessage()
		{
			_ship.TryDequeue(out _);
		}
	}
}
