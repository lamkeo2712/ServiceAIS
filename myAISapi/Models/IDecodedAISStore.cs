namespace myAISapi.Models
{
	public interface IDecodedAISStore
	{
		void AddDecodedMessage(DecodedAISMessage message);
		IEnumerable<DecodedAISMessage> GetAllDecodedMessages();
		void ClearMessages();
		void DeteleFirstMessage();
	}
}
