using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Artyste.Models
{
	public class Services
	{
		[Key] 
		public int ServiceId { get; set; }

		[Required]
		[StringLength(100)]
		public required string ServiceName { get; set; }

		
		public decimal FromPrice { get; set; }

		public decimal ToPrice { get; set; }

		[Required(ErrorMessage = "User ID is required.")]
		public string? UserId { get; set; }

		[ForeignKey("UserId")]
		public virtual ApplicationUser? User { get; set; }
	}
}
