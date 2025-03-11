namespace myAISapi.Models
{
	public interface IDM_HanhTrinh_Store
	{
		void AddRoute(DM_HanhTrinh message);
		IEnumerable<DM_HanhTrinh> GetAllRoute();
		void ClearMessages();
		void DeleteFirstMessage();
	}
}
