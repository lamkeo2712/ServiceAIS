using myAISapi.Models;
using System.Collections.Concurrent;

namespace myAISapi.Services
{
	public class DM_Tau_Store : IDM_Tau_Store
	{
		private readonly ConcurrentDictionary<int, DM_Tau> _ship = new();

		public void AddShip(DM_Tau message)
		{
			_ship.AddOrUpdate(message.MMSI, message, (key, existingShip) =>
			{
				// Cập nhật thông tin nếu MMSI đã tồn tại
				existingShip = message;
				return existingShip;
			});
		}

		public IEnumerable<DM_Tau> GetAllShip()
		{
			return _ship.Values;
		}

		public void ClearMessages()
		{
			_ship.Clear();
		}

		public void DeleteFirstMessage()
		{
			if (_ship.Any())
			{
				int firstKey = _ship.Keys.First();
				_ship.TryRemove(firstKey, out _);
			}
		}
	}
}
