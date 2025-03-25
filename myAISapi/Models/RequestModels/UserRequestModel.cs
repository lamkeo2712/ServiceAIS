namespace myAISapi.Models.RequestModels
{
	public sealed record UserRequestModel
	{
		public string Username { get; set; }
		public string Password { get; set; }
	}
}
