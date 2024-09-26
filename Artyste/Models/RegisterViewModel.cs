using System.ComponentModel.DataAnnotations;

namespace Artyste.Models
{
	public class RegisterViewModel
	{
		public string? PhoneNumber { get; set; }
		public string? Email { get; set; }
		public string? Password { get; set; }
		public string? FirstName { get; set; }
		public string? LastName { get; set; }
		public string? AccountType { get; set; }

		public string? Gender { get; set; }
		public string? Description { get; set; }
	}
}
