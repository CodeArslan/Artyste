using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Artyste.Models
{
	public class ApplicationUser:IdentityUser
	{
		[Required]
		public required string FirstName { get;set; }
		[Required]
		public required string LastName { get;set; }
		[Required]
		public required string AccountType { get; set; }
		public string? Gender { get; set; }
		public string? Description { get; set; }


	}
}
