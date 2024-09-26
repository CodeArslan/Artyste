using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Artyste.Models
{
	public class AddOns
	{
		[Key]
		public int Id { get; set; }
		[Required(ErrorMessage = "AddOn Name is required.")]
		public required string name { get; set; }

		[Required(ErrorMessage = "User ID is required.")]
		public string? UserId { get; set; }

		[ForeignKey("UserId")]
		public virtual ApplicationUser? User { get; set; }

	}
}
