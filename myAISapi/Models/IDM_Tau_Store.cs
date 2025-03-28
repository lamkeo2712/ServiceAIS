namespace myAISapi.Models
{
	public interface IDM_Tau_Store
	{
		void AddShip(DM_Tau message);
		IEnumerable<DM_Tau> GetAllShip();
		void ClearMessages();
		void DeleteFirstMessage();
		void DeleteMessages(IEnumerable<DM_Tau> batch);
	}
}
