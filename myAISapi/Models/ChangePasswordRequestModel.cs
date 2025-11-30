namespace myAISapi.Models
{
	public class ChangePasswordRequestModel
	{
		public string CurrentPassword { get; set; } = null!;
		public string NewPassword { get; set; } = null!;
	}
}
