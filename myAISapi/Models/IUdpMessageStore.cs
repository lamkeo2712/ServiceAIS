namespace myAISapi.Models
{
	public interface IUdpMessageStore
	{
		void AddMessage(string message);
		string[] GetAllMessages();
		void ClearMessages();
		void Delete();
	}
}
