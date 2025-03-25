using System.ComponentModel.DataAnnotations;

namespace myAISapi.Models
{
	public class User
	{
		[Key]
		public int Id { get; set; }

		[Required]
		public string Username { get; set; }

		[Required]
		public string PasswordHash { get; set; }

		[Required]
		public string Role { get; set; }

		public string? RefreshToken { get; set; }

		public DateTime? RefreshTokenExpiryTime { get; set; }
	}
}
